using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CusTempStorageRegHeaderEDocsViaUniversalXmlSupportTest : TestCaseWithFactory
	{
		public void TestLoadBusinessObjectFromCode()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Reference = "0001";
			header1.SRH_AppCode = TemporaryStorageApplicationCodeList.Codes.SumA;

			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Reference = "0002";
			header2.SRH_AppCode = TemporaryStorageApplicationCodeList.Codes.SumA;
			Factory.Save();

			var loader = new CusTempStorageRegHeaderEDocsViaUniversalXmlSupport();
			AssertEquals(header1.PK, loader.LoadBusinessObjectFromCode(Factory, "0001")?.PK);
			AssertEquals(header2.PK, loader.LoadBusinessObjectFromCode(Factory, "0002")?.PK);
		}

		public void TestTryLoadLanguageNotInDB()
		{
			var loader = new CusTempStorageRegHeaderEDocsViaUniversalXmlSupport();
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, "0003"));
		}
	}
}
