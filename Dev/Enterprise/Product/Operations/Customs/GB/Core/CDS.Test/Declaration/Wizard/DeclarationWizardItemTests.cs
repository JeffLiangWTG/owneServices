using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class DeclarationWizardItemTests : TestCaseWithFactory
	{
		public void TestAdditonalDeclarationTypes()
		{
			var declarationWizardItem = new DeclarationWizardItem();
			declarationWizardItem.AdditionalDeclarationType = "A,D or Y";

			AssertEquals("AdditionalDeclarationTypes should have 3 choices", 3, declarationWizardItem.AdditionalDeclarationTypes().Length);
			AssertEquals("Incorrect Additonal Declaration Type", "A", declarationWizardItem.AdditionalDeclarationTypes()[0]);
			AssertEquals("Incorrect Additonal Declaration Type", "D", declarationWizardItem.AdditionalDeclarationTypes()[1]);
			AssertEquals("Incorrect Additonal Declaration Type", "Y", declarationWizardItem.AdditionalDeclarationTypes()[2]);

			declarationWizardItem.AdditionalDeclarationType = "A or Y";

			AssertEquals("AdditionalDeclarationTypes should have 2 choices", 2, declarationWizardItem.AdditionalDeclarationTypes().Length);
			AssertEquals("Incorrect Additonal Declaration Type", "A", declarationWizardItem.AdditionalDeclarationTypes()[0]);
			AssertEquals("Incorrect Additonal Declaration Type", "Y", declarationWizardItem.AdditionalDeclarationTypes()[1]);
		}
	}
}
