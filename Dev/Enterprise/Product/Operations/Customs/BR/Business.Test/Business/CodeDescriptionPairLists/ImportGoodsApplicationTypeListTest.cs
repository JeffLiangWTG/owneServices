using CargoWise.Customs.Shared.MessageContracts;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportGoodsApplicationTypeListTest : TestCase
	{
		public void TestMapToCustomsCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ImportGoodsApplicationTypeList is Consumption", "CONSUMO", ImportGoodsApplicationTypeList.MapToCustomsCode(ImportGoodsApplicationTypeList.Codes.Consumption));
				AssertEquals("ImportGoodsApplicationTypeList is IncorporationAssets", "INCORPORACAO_ATIVO_FIXO", ImportGoodsApplicationTypeList.MapToCustomsCode(ImportGoodsApplicationTypeList.Codes.IncorporationAssets));
				AssertEquals("ImportGoodsApplicationTypeList is Industrialization", "INDUSTRIALIZACAO", ImportGoodsApplicationTypeList.MapToCustomsCode(ImportGoodsApplicationTypeList.Codes.Industrialization));
				AssertEquals("ImportGoodsApplicationTypeList is Resale", "REVENDA", ImportGoodsApplicationTypeList.MapToCustomsCode(ImportGoodsApplicationTypeList.Codes.Resale));
				AssertEquals("ImportGoodsApplicationTypeList is Other", "OUTRA", ImportGoodsApplicationTypeList.MapToCustomsCode(ImportGoodsApplicationTypeList.Codes.Other));
				Assert("When code not found, should be Empty", ImportGoodsApplicationTypeList.MapToCustomsCode("X").IsEmpty());
			});
		}
	}
}
