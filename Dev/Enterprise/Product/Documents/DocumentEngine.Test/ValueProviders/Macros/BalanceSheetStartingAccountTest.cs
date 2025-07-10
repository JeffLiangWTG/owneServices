using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(BalanceSheetStartingAccount))]
	sealed class BalanceSheetStartingAccountTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match <Balance Sheet Starting Account>", ValueProviderToTest.IsResponsibleForReplacing("<Balance Sheet Starting Account>", Passes.FirstPass));
			Assert("should not match <BalnceSheetStartingAccount>", !ValueProviderToTest.IsResponsibleForReplacing("<BalnceSheetStartingAccount>", Passes.FirstPass));
			Assert("should not match <ChinaBalanceSheetStartingAccount>", !ValueProviderToTest.IsResponsibleForReplacing("<ChinaBalanceSheetStartingAccount>", Passes.FirstPass));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new BalanceSheetStartingAccount();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('65BBEAF1-B62C-4837-B45B-B069FB0A5ECB', '9999.99.99', 'control account', 'BSH','DR',1, NULL, 0, 'LI')");
			var locator = new RegistryItemSetLocator();
			var accRegistryItemSet = (RegistryItemSet)locator.GetRegistryItemSet("AccountingConfigurationRegistry");
			var registryItem = (RegistryItemImpl)accRegistryItemSet.FindByName("GL_BS_ACCOUNT_START");
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid("65BBEAF1-B62C-4837-B45B-B069FB0A5ECB"));
		}
	}
}
