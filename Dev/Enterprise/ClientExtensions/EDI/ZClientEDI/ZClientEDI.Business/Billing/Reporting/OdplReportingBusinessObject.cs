using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class OdplReportingBusinessObject : NonPersistentBusinessObject, IObsoleteValidation, ICsvReportingBusinessObject
	{
		public OdplReportingBusinessObject(BusinessObjectFactory factory, ZDateTime periodStart, ZString systemCode, ZGuid organisationPk, ZGuid clientCompanyPk, ZGuid licenceCompanyPk, ZGuid databasePk)
			: base(factory)
		{
			Context = new BillingLoadRawUsageContext(factory, periodStart, organisationPk, clientCompanyPk, licenceCompanyPk, databasePk);
			this.systemCode = systemCode;
		}
		protected readonly BillingLoadRawUsageContext Context;
		readonly ZString systemCode;

		protected virtual BillingSystem BillingSystem
		{
			get
			{
				BillingSystemList billingSystems = new BillingSystemList();
				return billingSystems.FirstOrDefault(x => x.SystemCode == systemCode);
			}
		}

		public ZBlob GetPdfUsageReport()
		{
			ZBlob result = ZBlob.Empty;
			if (BillingSystem != null)
			{
				var rawUsage = BillingSystem.LoadOdplRawUsage(Context);
				if (rawUsage != null)
				{
					DocSystemRawUsage docWrapper = DocSystemRawUsage.New(rawUsage, Factory);

					result = new ZBlob(BillingInvoicingHelper.GetRawDocumentInPdf(UsageDocTemplate, docWrapper));
				}
			}

			return result;
		}

		public void GetCsvUsageReport(Action<string> action)
		{
			if (BillingSystem != null)
			{
				BillingSystem.LoadRawUsageInCsv(Context, false, action);
			}
		}

		void ICsvReportingBusinessObject.GetCsvUsageReport(ICsvUsageReportWriter writer)
		{
			throw new NotImplementedException();
		}

		protected StmTemplate UsageDocTemplate
		{
			get
			{
				if (usageDocTemplate == null)
				{
					const string templateName = "Billing Usage";
					var query = new ZQuery(StmTemplateSchema.SO_Name, templateName);
					query.AddToFilter(StmTemplateSchema.SO_DataContext, Enterprise.Core.Constants.DataContext.CargoWiseBilling);
					usageDocTemplate = Factory.LoadTop1<StmTemplate>(query);
				}
				return usageDocTemplate;
			}
		}
		StmTemplate usageDocTemplate;
	}
}

