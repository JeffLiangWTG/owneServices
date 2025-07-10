using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class ForeignExportDeclarationProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(ForeignExportDeclarationProvider.New(null));
			AssertType<ForeignExportDeclarationProvider>(ForeignExportDeclarationProvider.New(Factory.New<MercosulForeignDeclaration>()));
		}

		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var mercosulForeign = entryInstruction.MercosulForeignDeclarations.AddNew();
			mercosulForeign.CSI_Description = "2106199";
			mercosulForeign.CSI_ReferenceNumber = "100";
			mercosulForeign.CSI_ReferenceNumber2 = "999";

			var dataProvider = ForeignExportDeclarationProvider.New(mercosulForeign);
			CombineAssertions(() =>
			{
				AssertEquals("DeclarationNumber should be", "2106199", dataProvider.DeclarationNumber);
				AssertEquals("InitialRangeNumber should be", "100", dataProvider.InitialRangeNumber);
				AssertEquals("FinalRangeNumber should be", "999", dataProvider.FinalRangeNumber);
			});
		}
	}
}
