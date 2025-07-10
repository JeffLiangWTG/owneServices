using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class JobDocAddressRequirementProviderTest : TestCaseWithFactory
	{
		public void TestJobDocAddressGovRegNumTypes()
		{
			var jobDocAddress = declaration.DocAddresses.AddNew();
			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.GoodsOwner;

			CombineAssertions(() =>
			{
				AssertSame("Cached", Factory.GetCachedValue<GovRegNumTypeList>(), jobDocAddress.Lookups.GovRegNumTypes);
				AssertEquals("Values", "EOR, TEN, TID, UST", jobDocAddress.Lookups.GovRegNumTypes.CodesAsString);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
}
