using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaManifestHeaderAssemblyDataEDocsViaUniversalXmlSupportTest : TestCaseWithFactory
	{
		public void TestLoadBusinessObjectFromCode()
		{
			var manifest1 = Factory.New<AsycudaManifestHeader>();
			manifest1.AMA_JobReference = "AB1";
			manifest1.AMA_RN_NKCountry = "AU";
			var manifest2 = Factory.New<AsycudaManifestHeader>();
			manifest2.AMA_JobReference = "AB2";
			manifest2.AMA_RN_NKCountry = "AU";
			Factory.Save();

			var loader = new AsycudaManifestHeaderAssemblyDataEDocsViaUniversalXmlSupport();
			CombineAssertions(() =>
				{
					AssertEquals("Load from code 'AB1'", manifest1.PK, loader.LoadBusinessObjectFromCode(Factory, "AB1")?.PK);
					AssertEquals("Load from code 'AB2'", manifest2.PK, loader.LoadBusinessObjectFromCode(Factory, "AB2")?.PK);
				});
		}
	}
}
