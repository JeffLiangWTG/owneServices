using Enterprise.Accounting.Integration.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.Testing
{
	sealed class AirCTOImportFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<CTOCusHAWB>
	{
		protected override CTOCusHAWB GetNewBusinessObjectForFilterCollection()
		{
			var hawb = Factory.NewWithValidTestData<CTOCusHAWB>();
			var mawb = Factory.NewWithValidTestData<CTOCusMAWB>();
			hawb.CS_CM = mawb.PK;
			mawb.CM_FlightNo = flightNumber.ToString();
			flightNumber++;
			return hawb;
		}

		protected override IJobHeaderParent GetJobParent(CTOCusHAWB bizo) => bizo.MAWB;

		protected override ModuleIdentifier FilterStripModuleID => ModuleIDs.Customs.AU.AirCTOImport;

		protected override bool ShouldUseBillingFilters => false;

		protected override void SetUp()
		{
			flightNumber = 0;
			base.SetUp();
		}

		int flightNumber;
	}
}
