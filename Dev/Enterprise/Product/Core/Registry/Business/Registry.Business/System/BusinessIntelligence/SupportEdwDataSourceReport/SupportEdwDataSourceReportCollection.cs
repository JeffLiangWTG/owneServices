using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class SupportEdwDataSourceReportCollection : RegistryBusinessObjectCollectionTemplate
	{
		public SupportEdwDataSourceReportCollection()
		{
		}

		public SupportEdwDataSourceReportCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new SupportEdwDataSourceReport this[int x]
		{
			get { return (SupportEdwDataSourceReport)base[x]; }
		}

		public new SupportEdwDataSourceReport AddNew()
		{
			return (SupportEdwDataSourceReport)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SupportEdwDataSourceReportCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SupportEdwDataSourceReport(CurrentFallbackLevel, CurrentFactory);
		}

		public static SupportEdwDataSourceReportCollection GetDefault()
		{
			var result = new SupportEdwDataSourceReportCollection();

			if (ObjectFactory.Get<IAccountingRegistryProvider>().EnableGenerateJournalEntriesForPostedAccountingTransactions)
			{
				var defaultReports = new List<string>
				{
					(NoResString)"Balance Sheet",
					(NoResString)"Balance Sheet Periods Analysis",
					(NoResString)"China Balance Sheet",
					(NoResString)"China Cash Flow Statement",
					(NoResString)"China GL Summary",
					(NoResString)"China GL Transaction",
					(NoResString)"China Profit and Loss - Monthly",
					(NoResString)"China Profit and Loss - Yearly",
					(NoResString)"China Reports Breakdown By Categories",
					(NoResString)"China Profit Appropriation Statement",
					(NoResString)"China Statement of Provision for Impairment of Assets",
					(NoResString)"China Statement of Shareholders' Equity",
					(NoResString)"China Trial Balance",
					(NoResString)"China VAT Detailed Report",
					(NoResString)"Eight Column Balance Report",
					(NoResString)"Multi-Language Balance Sheet",
					(NoResString)"Multi-Language Profit and Loss",
					(NoResString)"Multi-Language Transaction",
					(NoResString)"Multi-Language Trial Balance",
					(NoResString)"Profit and Loss - List of Movements by Account, Period, Branch and Department",
					(NoResString)"Profit and Loss by Branch",
					(NoResString)"Profit and Loss by Department",
					(NoResString)"Profit and Loss Periods Analysis",
					(NoResString)"Profit And Loss Report",
					(NoResString)"Transactions",
					(NoResString)"Trial Balance",
					(NoResString)"Trial Balance - List of Movements by Account  Period  Branch and Department",
					(NoResString)"Trial Balance Periods Analysis",
					(NoResString)"China GL Accounts Balances"
				};

				defaultReports.ForEach(reportName =>
				{
					var supportEdwDataSourceReport = result.AddNew();
					supportEdwDataSourceReport.ReportName = reportName;
					supportEdwDataSourceReport.BusinessContext = "RepGLReports";
				});
			}

			return result;
		}
	}
}
