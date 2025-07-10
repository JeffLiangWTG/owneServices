using CargoWise.Customs.Shared.MessageContracts;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportGoodsConditionTypeListTest : TestCase
	{
		public void TestMapToCustomsCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ImportGoodsConditionTypeList is New", "NOVA", ImportGoodsConditionTypeList.MapToCustomsCode(ImportGoodsConditionTypeList.Codes.New));
				AssertEquals("ImportGoodsConditionTypeList is Used", "USADA", ImportGoodsConditionTypeList.MapToCustomsCode(ImportGoodsConditionTypeList.Codes.Used));
				Assert("When code not found, should be Empty", ImportGoodsConditionTypeList.MapToCustomsCode("X").IsEmpty());
			});
		}
	}
}
