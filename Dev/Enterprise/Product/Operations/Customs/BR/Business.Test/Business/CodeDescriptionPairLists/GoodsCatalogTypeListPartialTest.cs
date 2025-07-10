using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class GoodsCatalogTypeListPartialTest : TestCase
	{
		public void TestMapToCustomsCode()
		{
			AssertEquals("Should be Empty", ZString.Empty, GoodsCatalogTypeList.MapToCustomsCode(""));
			AssertEquals("Should be Empty", ZString.Empty, GoodsCatalogTypeList.MapToCustomsCode("XX"));
			AssertEquals("Should be Exportacao", "EXPORTACAO", GoodsCatalogTypeList.MapToCustomsCode(GoodsCatalogTypeList.Codes.Export));
			AssertEquals("Should be Importacao", "IMPORTACAO", GoodsCatalogTypeList.MapToCustomsCode(GoodsCatalogTypeList.Codes.Import));
		}

		public void TestMapToCWCode()
		{
			AssertEquals("Should be Empty", ZString.Empty, GoodsCatalogTypeList.MapToCWCode(""));
			AssertEquals("Should be Empty", ZString.Empty, GoodsCatalogTypeList.MapToCWCode("XX"));
			AssertEquals("Should be EXP", GoodsCatalogTypeList.Codes.Export, GoodsCatalogTypeList.MapToCWCode("EXPORTACAO"));
			AssertEquals("Should be IMP", GoodsCatalogTypeList.Codes.Import, GoodsCatalogTypeList.MapToCWCode("IMPORTACAO"));
		}
	}
}
