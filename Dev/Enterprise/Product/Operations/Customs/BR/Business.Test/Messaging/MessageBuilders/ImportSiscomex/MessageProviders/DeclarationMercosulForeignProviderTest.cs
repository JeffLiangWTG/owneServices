using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationMercosulForeignProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationMercosulForeignProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var mercosulForeign = entryInstruction.MercosulForeignDeclarations.AddNew();
			mercosulForeign.CSI_Description = "8922323000";
			mercosulForeign.CSI_ReferenceNumber = "1";
			mercosulForeign.CSI_ReferenceNumber2 = "10";

			var mercosul = new DeclarationMercosulForeignProvider(mercosulForeign);

			AssertEquals("DeclarationMercosulForeignNumber should be", "8922323000", mercosul.DeclarationMercosulForeignNumber);
			AssertEquals("InicialNumber  should be", "1", mercosul.InicialNumber);
			AssertEquals("FinalNumber  should be", "10", mercosul.FinalNumber);
		}
	}
}
