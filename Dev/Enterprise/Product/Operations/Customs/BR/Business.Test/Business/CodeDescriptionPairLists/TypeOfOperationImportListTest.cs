using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class TypeOfOperationImportListTest : TestCase
	{
		public void TestMapToCustomsCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("OnItsOwn", "IMPORTACAO_DIRETA", TypeOfOperationImportList.MapToCustomsCode(TypeOfOperationImportList.Codes.OnItsOwn));
				AssertEquals("AccountAndOrder", "IMPORTACAO_POR_CONTA_E_ORDEM", TypeOfOperationImportList.MapToCustomsCode(TypeOfOperationImportList.Codes.AccountAndOrder));
				AssertNull("Invalid code should return NULL", TypeOfOperationImportList.MapToCustomsCode("X"));
			});
		}
	}
}
