using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class GoodsCatalogStatusTypeListPartialTest : TestCase
	{
		public void TestMapToCWCode()
		{
			AssertEquals("Should be Empty", ZString.Empty, GoodsCatalogStatusTypeList.MapToCWCode(""));
			AssertEquals("Should be Empty", ZString.Empty, GoodsCatalogStatusTypeList.MapToCWCode("XX"));
			AssertEquals("Should be 0", GoodsCatalogStatusTypeList.Codes.Active, GoodsCatalogStatusTypeList.MapToCWCode("Ativado"));
			AssertEquals("Should be 1", GoodsCatalogStatusTypeList.Codes.Inactive, GoodsCatalogStatusTypeList.MapToCWCode("Desativado"));
			AssertEquals("Should be 2", GoodsCatalogStatusTypeList.Codes.Draft, GoodsCatalogStatusTypeList.MapToCWCode("Rascunho"));
		}
	}
}
