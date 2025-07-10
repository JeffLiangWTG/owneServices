using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class GACPGAHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProperties()
		{
			AssertEquals(typeof(YesNoList), header.AddInfoLookups.PGAIndicatorList.GetType());
			AssertEquals(typeof(GACPGADepartmentCodes), header.AddInfoLookups.ProgramCodesList.GetType());
			AssertEquals(typeof(FTAProcessingCodes), header.AddInfoLookups.FTAProcessingCodes.GetType());
			AssertEquals(typeof(RefCountryCollection), header.AddInfoLookups.FibreOrigins.GetType());
			AssertEquals(typeof(RefCountryCollection), header.AddInfoLookups.YarnOrigins.GetType());
			AssertEquals(typeof(RefCountryCollection), header.AddInfoLookups.FabricOrigins.GetType());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<GACPGAHeader>();
		}
		GACPGAHeader header;

		#endregion
	}
}
