using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlReportingBusinessObject : NonPersistentBusinessObject, IObsoleteValidation, ICsvReportingBusinessObject
	{
		public StlReportingBusinessObject(BusinessObjectFactory factory, ZDateTime periodStart, ZGuid databasePk, ZGuid priceItemPk, ZGuid clientCompanyPk, ZString systemCodes)
			: base(factory)
		{
			this.context = new BillingLoadRawUsageContext(Factory, periodStart, databasePk, priceItemPk, clientCompanyPk);
			this.systemCodes = systemCodes.Split(',');
			isConsolidatedDatabase = context.IsConsolidatedDatabase;
		}
		readonly BillingLoadRawUsageContext context;
		readonly ZString[] systemCodes;
		readonly bool isConsolidatedDatabase;

		public ZBlob GetPdfUsageReport()
		{
			ZBlob result = ZBlob.Empty;

			StlRawUsage rawUsage = GetRawUsage();
			if (rawUsage != null)
			{
				var docWrapper = DocStlRawUsage.New(rawUsage, Factory);

				result = new ZBlob(BillingInvoicingHelper.GetRawDocumentInPdf(UsageDocTemplate, docWrapper));
			}

			return result;
		}

		public void GetCsvUsageReport(Action<string> action)
		{
			var billingSystemList = new BillingSystemList();

			foreach (var systemCode in systemCodes)
			{
				var reportingBusinessObject = GetReportingBusinessObject(systemCode);

				if (reportingBusinessObject != null)
				{
					reportingBusinessObject.GetCsvUsageReport(action);
				}
				else if (CanLoadStlRawUsage(systemCode))
				{
					LoadStlRawUsageInCsv(action, systemCode);
				}
				else
				{
					var system = billingSystemList.FirstOrDefault(x => systemCode.EqualsIgnoringCase(x.SystemCode) && x.SystemCode != BillingConstants.BillingSystem.ODM);
					if (system != null)
					{
						system.LoadRawUsageInCsv(context, true, action);
					}
					else if (systemCode == BillingConstants.BillingSystem.ODM)
					{
						LoadOnDemandUsageInCsv(action);
					}
					else
					{
						var reportMessage = $"can not load usage report for systemCode: {systemCode}";
						ErrorReporter.ReportDeveloperExceptionOnce(reportMessage, new DeveloperNotificationException(reportMessage));
					}
				}
			}
		}

		public void GetCsvUsageReport(ICsvUsageReportWriter writer)
		{
			var billingSystemList = new BillingSystemList();

			foreach (var systemCode in systemCodes)
			{
				var reportingBusinessObject = GetReportingBusinessObject(systemCode);

				if (reportingBusinessObject != null)
				{
					reportingBusinessObject.GetCsvUsageReport(writer);
				}
				else if (CanLoadStlRawUsage(systemCode))
				{
					LoadStlRawUsageInCsv(writer, systemCode);
				}
				else
				{
					var system = billingSystemList.FirstOrDefault(x => systemCode.EqualsIgnoringCase(x.SystemCode) && x.SystemCode != BillingConstants.BillingSystem.ODM);
					if (system != null)
					{
						system.LoadRawUsageInCsv(context, true, writer);
					}
					else if (systemCode == BillingConstants.BillingSystem.ODM)
					{
						LoadOnDemandUsageInCsv(writer);
					}
				}
			}
		}

		#region Get Raw Usage

		bool CanLoadStlRawUsage(string category)
		{
			return !context.PriceItemPK.IsEmpty &&
				(category == BillingConstants.BillingSystem.STL
				|| EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.ContainsRawUsageCategoryCode(category)
				|| StlRawUsageCaptions.Any(x => x.Category.EqualsIgnoringCase(category)));
		}

		IEnumerable<StlRawUsageReportRefCaption> StlRawUsageCaptions
		{
			get => stlRawUsageCaptions ?? (stlRawUsageCaptions = EDIDataRegistry.Instance.StlRawUsageReportRefCaption.Value.Cast<StlRawUsageReportRefCaption>());
		}
		IEnumerable<StlRawUsageReportRefCaption> stlRawUsageCaptions;

		/// <summary>
		/// For PDF only
		/// </summary>
		/// <returns></returns>
		protected StlRawUsage GetRawUsage()
		{
			StlRawUsage rawUsage = new StlRawUsage(context);

			var billingSystemList = new BillingSystemList();

			foreach (var systemCode in systemCodes)
			{
				StlRawUsage subRawUsage = null;

				var reportingBusinessObject = GetReportingBusinessObject(systemCode);

				if (reportingBusinessObject != null)
				{
					subRawUsage = reportingBusinessObject.LoadStlRawUsage();
				}
				else if (CanLoadStlRawUsage(systemCode))
				{
					subRawUsage = LoadStlRawUsage(systemCode);
				}
				else
				{
					var system = billingSystemList.FirstOrDefault(x => systemCode.EqualsIgnoringCase(x.SystemCode) && x.SystemCode != BillingConstants.BillingSystem.ODM);
					if (system != null)
					{
						subRawUsage = system.LoadStlRawUsage(context);
					}
					else if (systemCode == BillingConstants.BillingSystem.ODM)
					{
						subRawUsage = LoadOnDemandUsage();
					}
				}

				if (subRawUsage != null && subRawUsage.SummarySections.Count > 0)
				{
					rawUsage.MergeSummarySections(subRawUsage);
				}
			}

			return rawUsage;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DbCommand GetStlRawUsageCommand(string category)
		{
			var systemList = new BillingSystemList();
			var excludedSystemCodes = systemCodes
										.Where(x => !x.EqualsIgnoringCase(category) && systemList.Any(s => s.SystemCode != BillingConstants.BillingSystem.ODM
																&& x.EqualsIgnoringCase(s.SystemCode)));

			// used by sequential mode, do not change column order
			const string query =
@"SELECT TX_PriceItemCode, CompanyCode, BranchCode, TX_BillableCount,
	TX_Reference1 = CASE
						WHEN TX_PriceItemCode IN ('WOL', 'ECL', 'ECB') AND ISNULL(TX_Reference3, '') <> '' THEN TX_Reference1 + ' - ' + TX_Reference3
						ELSE TX_Reference1
					END,
	TX_Reference2,
	TX_Reference3 = CASE
						WHEN TX_PriceItemCode IN ('WOL', 'ECL', 'ECB') AND ISNULL(TX_Reference3, '') <> '' THEN ''
						ELSE TX_Reference3
					END,
	TX_Reference4,
	TX_ServiceOccuredUTC, StaffCode, AdjustedUnitCount, TenantID
FROM EdiLoadBillingDbDetailedUsage(@Period, @DatabaseId, @DatabaseNumber, @ClientCompanyPk, @PriceItemPk, @ExcludedSystemCodes)
ORDER BY TX_PriceItemCode, TX_ServiceOccuredUTC, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4";

			var cmd = GetConnectionForReader().Command(query);
			cmd.AddParameter("@DatabaseId", System.Data.SqlDbType.VarChar, (string)context.DatabaseId);
			cmd.AddParameter("@DatabaseNumber", System.Data.SqlDbType.Int, (int)context.DatabaseNumber);
			cmd.AddParameter("@ClientCompanyPk", System.Data.SqlDbType.UniqueIdentifier, !context.ClientCompanyPK.IsEmpty ? context.ClientCompanyPK.ToGuid() : DBNull.Value);
			cmd.AddParameter("@Period", System.Data.SqlDbType.Int, context.PeriodAsInt);
			cmd.AddParameter("@PriceItemPK", System.Data.SqlDbType.UniqueIdentifier, !context.PriceItemPK.IsEmpty ? context.PriceItemPK.ToGuid() : DBNull.Value);
			cmd.AddParameter("@ExcludedSystemCodes", System.Data.SqlDbType.VarChar, string.Join(",", excludedSystemCodes));
			return cmd;
		}

		/// <summary>
		/// For PDF only
		/// </summary>
		/// <param name="category"></param>
		/// <returns></returns>
		StlRawUsage LoadStlRawUsage(string category)
		{
			StlRawUsage rawUsage = new StlRawUsage(context);
			var usageReportRefCaptions = StlRawUsageCaptions;

			using (var command = GetStlRawUsageCommand(category))
			{
				using (var reader = command.ExecuteReader())
				{
					rawUsage.Summary.Header.TopLevelDescription = "Usage Summary";
					rawUsage.Summary.Header.Column1 = "Company Code";
					rawUsage.Summary.Header.Column2 = "Usage Type";
					rawUsage.Summary.Header.Column3 = "Usage Count";
					rawUsage.Summary.Header.Column9 = "Usage Time (UTC)";

					bool isFirstUsage = true;

					while (reader.Read())
					{
						string usageCode = (string)reader["TX_PriceItemCode"];
						string companyCode = (string)reader["CompanyCode"];
						int billableCount = (int)reader["TX_BillableCount"];
						string ref1 = (string)reader["TX_Reference1"];
						string ref2 = (string)reader["TX_Reference2"];
						string ref3 = (string)reader["TX_Reference3"];
						string ref4 = (string)reader["TX_Reference4"];
						ZDateTime usageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
						var adjustedUnitCount = reader["AdjustedUnitCount"] as decimal?;
						var tenantID = AsString(reader["TenantID"]);

						if (isFirstUsage)
						{
							var headerCaptions = usageReportRefCaptions.FirstOrDefault(x => x.UsageCode == usageCode && x.Category == category);

							if (headerCaptions != null)
							{
								rawUsage.Summary.Header.Column4 = headerCaptions.Ref1Caption;
								rawUsage.Summary.Header.Column5 = headerCaptions.Ref2Caption;
								rawUsage.Summary.Header.Column6 = headerCaptions.Ref3Caption;
								rawUsage.Summary.Header.Column7 = headerCaptions.Ref4Caption;
							}
							else
							{
								rawUsage.Summary.Header.Column4 = "Reference 1";
								rawUsage.Summary.Header.Column5 = "Reference 2";
								rawUsage.Summary.Header.Column6 = "Reference 3";
								rawUsage.Summary.Header.Column7 = "Reference 4";
							}

							if (adjustedUnitCount.HasValue)
							{
								rawUsage.Summary.Header.Column8 = "Adjusted Usage Count";
							}
							else if (isConsolidatedDatabase)
							{
								rawUsage.Summary.Header.Column8 = "Tenant ID";
							}

							isFirstUsage = false;
						}

						var line = rawUsage.Summary.Lines.AddNew();
						line.Column1 = companyCode;
						line.Column2 = usageCode;
						line.Column3 = billableCount.ToString(CultureInfo.InvariantCulture);
						line.Column4 = ref1;
						line.Column5 = ref2;
						line.Column6 = ref3;
						line.Column7 = ref4;
						if (adjustedUnitCount.HasValue)
						{
							line.Column8 = adjustedUnitCount.Value.ToString(CultureInfo.InvariantCulture);
						}
						else if (isConsolidatedDatabase)
						{
							line.Column8 = tenantID;
						}
						line.Column9 = usageTime.ToLongTimeString();
					}
				}
			}

			return rawUsage;
		}

		StlRawUsage LoadOnDemandUsage()
		{
			StlRawUsage rawUsage = new StlRawUsage(context);

			rawUsage.Summary.Header.TopLevelDescription = "Usage Summary";
			rawUsage.Summary.Header.Column1 = "Usage Type";
			rawUsage.Summary.Header.Column2 = "Staff Name (Code) / Country";

			GetOnDemandUsage((module, staff, usageTime) =>
			{
				var line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = module;
				line.Column2 = staff;
			});

			return rawUsage;
		}

		#endregion

		#region Get Raw Usage In Csv

		void LoadStlRawUsageInCsv(Action<string> action, string category)
		{
			var usageReportRefCaptions = StlRawUsageCaptions;

			using (var command = GetStlRawUsageCommand(category))
			using (var reader = command.ExecuteReader())
			{
				StlRawUsageReportRefCaption headerCaptions = null;
				string lastUsageCode = string.Empty;

				while (reader.Read())
				{
					string usageCode = (string)reader["TX_PriceItemCode"];
					string companyCode = (string)reader["CompanyCode"];
					int billableCount = (int)reader["TX_BillableCount"];
					string ref1 = (string)reader["TX_Reference1"];
					string ref2 = (string)reader["TX_Reference2"];
					string ref3 = (string)reader["TX_Reference3"];
					string ref4 = (string)reader["TX_Reference4"];
					ZDateTime usageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					string branchCode = (string)reader["BranchCode"];
					string staffCode = (string)reader["StaffCode"];
					var adjustedUnitCount = reader["AdjustedUnitCount"] as decimal?;
					var tenantID = AsString(reader["TenantID"]);

					if (usageCode != lastUsageCode)
					{
						string[] headerColumns;
						headerCaptions = usageReportRefCaptions.FirstOrDefault(x => x.UsageCode == usageCode && x.Category == category);
						if (headerCaptions != null)
						{
							var headerColumnsList = new List<string>(10);
							headerColumnsList.Add("Company Code");
							headerColumnsList.Add("Branch Code");
							headerColumnsList.Add("Staff Code");
							headerColumnsList.Add("Usage Type");
							headerColumnsList.Add("Usage Count");
							if (!headerCaptions.Ref1Caption.IsEmpty) { headerColumnsList.Add(headerCaptions.Ref1Caption); }
							if (!headerCaptions.Ref2Caption.IsEmpty) { headerColumnsList.Add(headerCaptions.Ref2Caption); }
							if (!headerCaptions.Ref3Caption.IsEmpty) { headerColumnsList.Add(headerCaptions.Ref3Caption); }
							if (!headerCaptions.Ref4Caption.IsEmpty) { headerColumnsList.Add(headerCaptions.Ref4Caption); }
							headerColumnsList.Add("Usage Time (UTC)");
							headerColumns = headerColumnsList.ToArray();
						}
						else
						{
							headerColumns = new string[] { "Company Code", "Branch Code", "Staff Code", "Usage Type", "Usage Count", "Reference 1", "Reference 2", "Reference 3", "Reference 4", "Usage Time (UTC)" };
						}

						if (adjustedUnitCount.HasValue)
						{
							headerColumns = headerColumns.Append("Adjusted Unit Count").ToArray();
						}
						else if (isConsolidatedDatabase)
						{
							headerColumns = headerColumns.Append("Tenant ID").ToArray();
						}

						var headerCsvLine = new OCsvLine(headerColumns);
						action(headerCsvLine.ToString());
						lastUsageCode = usageCode;
					}

					var dataValuesList = new List<string>(9);
					dataValuesList.Add(companyCode);
					dataValuesList.Add(branchCode);
					dataValuesList.Add(staffCode);
					dataValuesList.Add(usageCode);
					dataValuesList.Add(billableCount.ToString(CultureInfo.InvariantCulture));
					if (headerCaptions == null || !headerCaptions.Ref1Caption.IsEmpty) { dataValuesList.Add(ref1); }
					if (headerCaptions == null || !headerCaptions.Ref2Caption.IsEmpty) { dataValuesList.Add(ref2); }
					if (headerCaptions == null || !headerCaptions.Ref3Caption.IsEmpty) { dataValuesList.Add(ref3); }
					if (headerCaptions == null || !headerCaptions.Ref4Caption.IsEmpty) { dataValuesList.Add(ref4); }
					dataValuesList.Add(usageTime.ToLongTimeString());
					if (adjustedUnitCount.HasValue)
					{
						dataValuesList.Add(adjustedUnitCount.Value.ToString(CultureInfo.InvariantCulture));
					}
					else if (isConsolidatedDatabase)
					{
						dataValuesList.Add(tenantID);
					}

					var dataValues = dataValuesList.ToArray();
					var dataCsvLine = new OCsvLine(dataValues);
					action(dataCsvLine.ToString());
				}
			}
		}

		void LoadStlRawUsageInCsv(ICsvUsageReportWriter writer, string category)
		{
			using (var command = GetStlRawUsageCommand(category))
			using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
			{
				while (reader.Read())
				{
					int col = 0;
					var usageCode = reader.GetString(col++);
					var companyCode = reader.GetString(col++);
					var branchCode = reader.GetString(col++);
					var billableCount = reader.GetInt32(col++);
					var ref1 = reader.GetString(col++);
					var ref2 = reader.GetString(col++);
					var ref3 = reader.GetString(col++);
					var ref4 = reader.GetString(col++);
					ZDateTime usageTime = reader.GetDateTime(col++);
					var staffCode = reader.GetString(col++);
					var adjustedUnitCount = reader.GetValue(col++) as decimal?;
					var tenantID = AsZString(reader[col++]);

					var refText = string.Join(" ", new[] { ref1, ref2, ref3, ref4 }.Where(x => !string.IsNullOrEmpty(x))).Trim();
					writer.WriteCsvUsageReport(usageTime, companyCode, branchCode, staffCode, refText, usageCode, context.PriceItemDescription, billableCount, adjustedUnitCount, tenantID);
				}
			}
		}

		void LoadOnDemandUsageInCsv(Action<string> action)
		{
			var headerColumns = new string[] { "Usage Type", "Staff Name (Code) / Country" };
			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			GetOnDemandUsage((module, staff, usageTime) =>
			{
				var dataValues = new string[] { module, staff };
				var dataCsvLine = new OCsvLine(dataValues);
				action(dataCsvLine.ToString());
			});
		}

		void LoadOnDemandUsageInCsv(ICsvUsageReportWriter writer)
		{
			GetOnDemandUsage((module, staff, usageTime) =>
			{
				writer.WriteCsvUsageReport(usageTime, "", "", staff, module, context.PriceItemCode, context.PriceItemDescription, 1);
			});
		}

		#endregion

		#region Get Query

		void GetOnDemandUsage(Action<string, string, ZDateTime> moduleAndStaffAction)
		{
			var query = GetOnDemandUsageQuery(context.CategoryAndUsageCodes.Select(x => x.Code));

			using (var command = GetConnectionForReader().Command(query))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var moduleCode = reader.GetString(0);
					var name = reader.GetString(1);
					var staffCode = reader.GetString(2).TrimEnd();
					var usageTime = reader.GetDateTime(3);

					moduleAndStaffAction(moduleCode, name + " (" + staffCode + ")", usageTime);
				}
			}
		}

		string GetOnDemandUsageQuery(IEnumerable<string> moduleCodes)
		{
			var sql = @"
SELECT LX2_ModuleCode,
	LS_FullName,
	LS_Code,
	LX2_FirstUsageUtc
FROM 
	dbo.EdiViewBillableUsage
WHERE 
	LX2_Period = @Period
	AND LD_PK = @DatabasePk
	AND LX2_ModuleCode in ('" + string.Join("', '", moduleCodes) + @"')
	AND LX2_LicenceMode = 'ODM'
";

			var countryUsagePriceCodes = moduleCodes.Where(x => BillingConstants.GeographicCompliance.CountryUsagePriceCodes.Contains(x));
			if (countryUsagePriceCodes.Any())
			{
				sql += @"
UNION ALL
SELECT U1_SubCode LX2_ModuleCode, 
	RN_Desc [LS_FullName], 
	LCC_RN_NKCountryCode [LS_Code], 
	U1_PeriodStart
FROM dbo.ClientChargeableUsage
JOIN dbo.ClientCompany on LCC_PK = U1_LCC
JOIN dbo.RefCountry on RN_Code = LCC_RN_NKCountryCode
WHERE U1_LD = @DatabasePk
	AND U1_Code = 'ODM'
	AND U1_SubCode in ('" + string.Join("', '", countryUsagePriceCodes) + @"')
	AND U1_PeriodStart = DATEFROMPARTS(@Period / 100, @Period % 100, 1)
";
			}

			string query =
@"SELECT LX2_ModuleCode,
	LS_FullName,
	LS_Code,
	LX2_FirstUsageUtc = MIN(LX2_FirstUsageUtc)
FROM 
(
" + sql + @"
) Usage
GROUP BY LS_FullName, LS_Code, LX2_ModuleCode
ORDER by LS_FullName, LS_Code, LX2_ModuleCode
";

			// use literals rather than parameters to force SQL server to recompile the query plan
			query = query.Replace("@DatabasePk", "'" + context.DatabasePK + "'");
			query = query.Replace("@Period", context.PeriodAsInt.ToString(CultureInfo.InvariantCulture));

			return query;
		}

		#endregion

		DbConnection GetConnectionForReader()
		{
			if (Globals.IsTest) { return Db.Connection; }
			return Db.NewExtraConnectionToMainDb();
		}

		protected StmTemplate UsageDocTemplate
		{
			get
			{
				if (usageDocTemplate == null)
				{
					const string templateName = "STL Billing Usage";
					var query = new ZQuery(StmTemplateSchema.SO_Name, templateName);
					query.AddToFilter(StmTemplateSchema.SO_DataContext, Enterprise.Core.Constants.DataContext.CargoWiseBilling);
					usageDocTemplate = Factory.LoadTop1<StmTemplate>(query);
				}
				return usageDocTemplate;
			}
		}
		StmTemplate usageDocTemplate;

		StlReportingBusinessObjectBase GetReportingBusinessObject(ZString systemCode) => StlReportingBusinessObjectFactory.CreateReportingBusinessObject(context, systemCode);

		string AsString(object value) => value is DBNull ? null : value.ToString();

		ZString? AsZString(object value)
		{
			ZString? result = null;
			if (value != DBNull.Value)
			{
				result = value.ToString();
			}
			return result;
		}
	}
}

