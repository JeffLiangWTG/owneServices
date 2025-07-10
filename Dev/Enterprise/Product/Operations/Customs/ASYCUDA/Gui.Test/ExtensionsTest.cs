using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Freight.Forwarding.Business;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class ExtensionsTest : TestCaseWithFactory
	{
		public void TestUnsavedConsolDoesNotHitDatabase()
		{
			var databaseLoadCount = Factory.DatabaseLoadCount;
			Assert(!consol.CheckManifestHeaderHasBeenCreated(Core.Constants.CountryCodes.Fiji, "ASY"));
			Assert(!consol.CheckManifestHeaderHasBeenCreated(Core.Constants.CountryCodes.Turkey, "MAN"));
			AssertEquals(databaseLoadCount, Factory.DatabaseLoadCount);
		}

		public void TestSavedConsolDoesHitDatabase()
		{
			Factory.Save();
			var databaseLoadCount = Factory.DatabaseLoadCount;
			Assert(!consol.CheckManifestHeaderHasBeenCreated(Core.Constants.CountryCodes.Fiji, "ASY"));
			Assert(!consol.CheckManifestHeaderHasBeenCreated(Core.Constants.CountryCodes.Turkey, "MAN"));
			AssertGreaterThan(Factory.DatabaseLoadCount, databaseLoadCount);
		}

		public void TestExistingManifestIsFound()
		{
			var headerWrapper = new ManifestHeadersWrapper(consol);
			var header = headerWrapper.Headers.AddNew(AsycudaManifestHeader.TypeDecider.GetGlobalManifestType(Factory, Core.Constants.CountryCodes.Fiji, "ASY", "NVC"));
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Fiji;
			header.AMA_ManifestType = "ASY";
			Factory.Save();
			Assert(consol.CheckManifestHeaderHasBeenCreated(Core.Constants.CountryCodes.Fiji, "ASY"));

			var header2 = headerWrapper.Headers.AddNew(AsycudaManifestHeader.TypeDecider.GetGlobalManifestType(Factory, Core.Constants.CountryCodes.Turkey, "", ApplicationCodeTypeList.Codes.TRETrade));
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TRETrade;

			Factory.Save();
			Assert(!consol.CheckManifestHeaderHasBeenCreated(Core.Constants.CountryCodes.Turkey));
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.NewWithValidTestData<ForwardingConsol>();
		}
		ForwardingConsol consol;
	}
}
