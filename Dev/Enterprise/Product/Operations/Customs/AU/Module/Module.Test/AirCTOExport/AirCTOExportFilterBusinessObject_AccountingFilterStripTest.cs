using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.Testing
{
	sealed class AirCTOExportFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<AirCTOExportCustomsManifestHeader>
	{
		public void TestJobInvoicingStatusFilter()
		{
			var header = Factory.NewWithValidTestData<AirCTOExportCustomsManifestHeader>();
			var line1 = header.Lines.AddNew();
			line1.FillWithValidTestData();
			var job1 = new JobHeader.Loader(line1).TryLoadOrCreate();
			AssertNotNull(job1);
			AssertEquals(JobHeaderStatus.Working.Code, job1.JH_Status);
			Factory.Save();
			var filterBo = new AirCTOExportFilterBusinessObject(true, true);
			var filter = (ModuleTextBaseFilter)filterBo["Invoicing Job Status"];
			filter.Property = JobHeaderStatus.Working.Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			var filterCollection = new ExportCustomsManifestHeaderCollection(Factory);
			filterCollection.Load(filterBo.Filter);
			AssertCollectionContains(header, filterCollection);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filterCollection.Load(filterBo.Filter);
			AssertCollectionNotContains(header, filterCollection);
		}

		protected override AirCTOExportCustomsManifestHeader GetNewBusinessObjectForFilterCollection()
		{
			var header = Factory.NewWithValidTestData<AirCTOExportCustomsManifestHeader>();
			header.Lines.AddNew().FillWithValidTestData();
			return header;
		}

		protected override IJobHeaderParent GetJobParent(AirCTOExportCustomsManifestHeader bizo) => bizo.Lines[0];

		protected override ModuleIdentifier FilterStripModuleID => ModuleIDs.Customs.AU.AirCTOExport;

		protected override ControllerID ControllerIDForBillingIfDifferFromModuleController => ControllerIDs.Customs.AU.AirCTOHawbExport;

		protected override bool ShouldUseBillingFilters => false;
	}
}
