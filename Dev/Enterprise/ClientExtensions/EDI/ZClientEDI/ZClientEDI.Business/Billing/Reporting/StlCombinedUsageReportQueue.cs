using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlCombinedUsageReportQueue : EdiReportingQueue
	{
		public StlCombinedUsageReportQueue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public const string ReportType = "SCU";

		public static bool RequestReport(ZGuid orgPk, ZGuid contactPk, ZGuid databasePk, ZInt period, ZString supportStaff)
		{
			if (orgPk.IsEmpty || databasePk.IsEmpty || period == 0 || (contactPk.IsEmpty && supportStaff.IsEmpty))
			{
				return false;
			}

			var factory = new BusinessObjectFactory();
			var query = new ZQuery(EdiReportingQueueSchema.ERQ_ReportType, ReportType);
			query.AddToFilter(EdiReportingQueueSchema.ERQ_OH, orgPk);

			if (!supportStaff.IsEmpty)
			{
				query.AddToFilter(EdiReportingQueueSchema.ERQ_GS_NKSupportStaff, supportStaff);
			}
			else
			{
				query.AddToFilter(EdiReportingQueueSchema.ERQ_OC, contactPk);
			}

			query.AddToFilter(EdiReportingQueueSchema.ERQ_LD, databasePk);
			query.AddToFilter(EdiReportingQueueSchema.ERQ_Period, period);
			query.AddToFilter(EdiReportingQueueSchema.ERQ_Status, EdiReportingQueue.QueueStatus.NewReport);
			var queue = factory.LoadTop1<StlCombinedUsageReportQueue>(query);

			if (queue == null)
			{
				queue = factory.New<StlCombinedUsageReportQueue>();
				queue.ERQ_ReportType = ReportType;
				queue.ERQ_Period = period;
				queue.ERQ_LD = databasePk;
				queue.ERQ_OH = orgPk;

				if (!supportStaff.IsEmpty)
				{
					queue.ERQ_GS_NKSupportStaff = supportStaff;
				}
				else
				{
					queue.ERQ_OC = contactPk;
				}

				queue.ERQ_ReportName = queue.ReportName;
				factory.Save();
			}

			return true;
		}

		string ReportName
		{
			get
			{
				var reportName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_StlCombinedUsageReport",
						Period.ToString("yyyyMM", CultureInfo.InvariantCulture),
						Database.LD_ServerCode);
				return reportName;
			}
		}

		protected override void GenerateReportCore(StreamWriter streamWriter)
		{
			var headerColumns = "Usage Time(UTC),Company,Branch,Staff,Reference,Price Code,Price Item Description,Unit Count,Adjusted Unit Count";
			if (Database.IsConsolidatedDatabase)
			{
				headerColumns += ",Tenant ID";
			}

			var headerCsvLine = new OCsvLine(headerColumns);
			streamWriter.WriteLine(headerCsvLine.ToString());

			var summary = LoadReportSummary();
			var writer = new ReportWriter(streamWriter);
			foreach (var lines in summary.GroupBy(x => x.L7_PK))
			{
				try
				{
					var systemCodes = string.Join(",", lines.Select(x => x.U1_Code));
					var reportingObj = new StlReportingBusinessObject(Factory, Period, ERQ_LD, lines.Key, ZGuid.Empty, systemCodes);
					reportingObj.GetCsvUsageReport(writer);
				}
				catch (Exception ex) when (!CanRetryOnError)
				{
					writer.WriteCsvUsageReport(ZDateTime.UtcNow, "", "", "", "The report generation process has encountered errors, potentially resulting in incomplete data within the report. Please reach out to customer service for further information.", "", "", 0);
					ErrorReporter.ReportOnce("StlCombinedUsageReportQueue.GenerateReportCore", ex.Message, ex);
				}
			}
		}

		IEnumerable<UsageSummaryLine> LoadReportSummary()
		{
			var isGenericUsageProduct = EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.ContainsProductCode(Database.LD_Product);
			var summary = new DynamicBusinessObjectCollection(Factory);
			var summaryQuery = isGenericUsageProduct ?
				"SELECT DISTINCT LD_PK, L7_PK, U1_Code, L7_Order FROM EdiGetStlGenericUsageSummary(@OrgPk, @Period, @Product) WHERE LD_PK = @DatabasePk ORDER BY L7_Order;"
			  : "SELECT DISTINCT LD_PK, L7_PK, U1_Code, L7_Order FROM EdiGetStlUsageSummary(@OrgPk, @Period) WHERE LD_PK = @DatabasePk ORDER BY L7_Order;";

			var parameters = new ZSqlParameterCollection();
			parameters.Add(ZSqlParameter.New("@OrgPk", ERQ_OH.ToGuid(), CargoWise.Schema.Schema.GenericGuidSchemaColumn));
			parameters.Add(ZSqlParameter.New("@Period", Period, CargoWise.Schema.Schema.GenericDateTimeColumn));
			parameters.Add(ZSqlParameter.New("@DatabasePk", ERQ_LD.ToGuid(), CargoWise.Schema.Schema.GenericGuidSchemaColumn));
			if (isGenericUsageProduct)
			{
				parameters.Add(ZSqlParameter.New("@Product", Database.LD_Product, CargoWise.Schema.Schema.GenericStringSchemaColumn));
			}

			summary.Load(summaryQuery, parameters);
			return summary.Select(x => new UsageSummaryLine(x)).ToArray();
		}

		class UsageSummaryLine
		{
			public UsageSummaryLine(DynamicBusinessObject bizObj)
			{
				L7_PK = (ZGuid)bizObj[ClientLicencePriceItemSchema.Constants.PK];
				U1_Code =  bizObj[ClientChargeableUsageSchema.Constants.U1_Code].ToString();
			}

			public readonly ZGuid L7_PK;
			public readonly ZString U1_Code;
		}

		protected class ReportWriter : ICsvUsageReportWriter
		{
			public ReportWriter(StreamWriter streamWriter)
			{
				this.StreamWriter = streamWriter;
			}

			public void WriteCsvUsageReport(ZDateTime usageTime, ZString company, ZString branch, ZString staff, ZString reference, ZString priceCode, ZString priceItemDescription, ZInt unitCount, ZDecimal? adjustedUnitCount = null, ZString? tenantID = null)
			{
				var dataValuesList = new List<string>(9);
				dataValuesList.Add(BillingSystem.ToMessageTimeFormat(usageTime));
				dataValuesList.Add(company);
				dataValuesList.Add(branch);
				dataValuesList.Add(staff);
				dataValuesList.Add(reference);
				dataValuesList.Add(priceCode);
				dataValuesList.Add(priceItemDescription.Trim());
				dataValuesList.Add(unitCount.ToString());
				dataValuesList.Add(adjustedUnitCount.HasValue ? adjustedUnitCount.Value.ToString() : "");
				dataValuesList.Add(tenantID.HasValue ? tenantID.Value.ToString() : "");
				var dataCsvLine = new OCsvLine(dataValuesList.ToArray());
				StreamWriter.WriteLine(dataCsvLine.ToString());
			}

			readonly StreamWriter StreamWriter;
		}
	}
}

