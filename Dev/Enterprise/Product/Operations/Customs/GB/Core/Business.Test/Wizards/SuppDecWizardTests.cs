using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Wizards.CFSP.Testing
{
	class SuppDecWizardTests : TestCaseWithFactory
	{
		public void TestCreateSuppDec()
		{
			var wizardManager = GetNewWizardManager();
			var presenterForTest = new Customs.Business.Testing.RelatedDeclarationControllerTest.ZFormPresenterForTest();
			var relatedDeclarationHelper = new Customs.Business.RelatedDeclarationHelper(presenterForTest);

			wizardManager.SuppDecWizard.DeclarationType = "ISW";
			wizardManager.SuppDecWizard.NumberPackagesToDeclare = 4;
			wizardManager.SuppDecWizard.SupplementaryProcedure = "Y";
			var childCreated = wizardManager.CreateAndShowSupplemenaryDeclaration(relatedDeclarationHelper);  // Don't show the GUI because presenterForTest does nothing

			AssertEquals("ISW", childCreated.JE_DeclarationType);
			AssertEquals("Y", childCreated.JE_EntrySubStyle);
			AssertEquals(4, childCreated.JE_TotalNoOfPacks);
			AssertEquals("", childCreated.JE_MasterBill);
			AssertEquals("", childCreated.JE_HouseBill);
			AssertEquals("", childCreated.ZG_HouseSplitReference);
			AssertEquals("", childCreated.JE_MasterUCR);
			AssertEquals("ZZZZDUCRHERE", childCreated.PreviousDocuments[0].KeyToDeterimeUniqueness);
		}

		public void TestManagerValidateEverything()
		{
			var wizardManager = GetNewWizardManager();
			wizardManager.SuppDecWizard.DeclarationType = "";
			var errors = wizardManager.ValidateEverything();
			AssertContains("Number of packages to declare now cannot be zero", errors);
			AssertContains("Please enter a Supplementary Sub-Type", errors);
			AssertContains("Please enter a Supplementary Type", errors);
			wizardManager.SuppDecWizard.NumberPackagesToDeclare = 1;
			wizardManager.SuppDecWizard.DeclarationType = "ISD";
			wizardManager.SuppDecWizard.SupplementaryProcedure = "Y";
			Assert(wizardManager.ValidateEverything().IsEmpty);
		}

		SuppDecWizardManager GetNewWizardManager()
		{
			var decSfd = Factory.New<JobDeclaration>();
			decSfd.JE_UCR = "DUCRHERE";
			decSfd.JE_TotalNoOfPacks = 6;
			decSfd.JE_MasterBill = "1";
			decSfd.JE_HouseBill = "2";
			decSfd.ZG_HouseSplitReference = "3";
			decSfd.JE_MasterUCR = "4";
			decSfd.Factory.Save();
			var wizardManager = new SuppDecWizardManager(decSfd);
			return wizardManager;
		}
	}

	[TestedType(typeof(SuppDecWizard))]
	public class SuppDecWizardNPBOTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var parentDec = Factory.New<JobDeclaration>();
			return new SuppDecWizard(parentDec);
		}

		public void TestConstructor()
		{
			var parentDec = Factory.New<JobDeclaration>();
			parentDec.JE_TotalNoOfPacks = 6;
			Factory.Save();

			var presenterForTest = new Customs.Business.Testing.RelatedDeclarationControllerTest.ZFormPresenterForTest();
			var relatedDeclarationHelper = new Customs.Business.RelatedDeclarationHelper(presenterForTest);
			var relatedDec = (JobDeclaration)relatedDeclarationHelper.CreateNewRelated(parentDec, "SUP");
			relatedDec.JE_TotalNoOfPacks = 2;
			relatedDec.Factory.Save();

			var wizard = new SuppDecWizard(parentDec);
			wizard.OnBalanceChanged += wizard_OnBalanceChanged;
			AssertEquals("ESD", wizard.DeclarationType);
			AssertEquals(0, wizard.NumberPackagesToDeclare);
			AssertEquals(6, wizard.NumberOfPackagesOfParentBox6);
			AssertEquals(1, wizard.NumberOfSiblingDeclarations);
			AssertEquals(2, wizard.NumberOfPackagesDeclaredOnSiblingDeclarations);
			AssertEquals(4, wizard.NumberOfPackagesRemainingOnSiblingDeclarations);
			AssertEquals(4, wizard.NumberOfPackagesRemainingBalance);
			AssertEquals(0, balancedChangedCounter);
			wizard.NumberPackagesToDeclare = 3;
			AssertEquals(3, wizard.NumberPackagesToDeclare);
			AssertEquals(1, wizard.NumberOfPackagesRemainingBalance);
			AssertEquals(1, balancedChangedCounter);
			wizard.NumberPackagesToDeclare = 4;
			AssertEquals(4, wizard.NumberPackagesToDeclare);
			AssertEquals(0, wizard.NumberOfPackagesRemainingBalance);
			AssertEquals(2, balancedChangedCounter);
		}

		void wizard_OnBalanceChanged(object sender, EventArgs e)
		{
			balancedChangedCounter++;
		}
		int balancedChangedCounter;
	}
}
