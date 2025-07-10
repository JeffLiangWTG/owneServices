using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(PLAppropriationAccount))]
	sealed class PLAppropriationAccountTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match <PL Appropriation Account>", ValueProviderToTest.IsResponsibleForReplacing("<PL Appropriation Account>", Passes.FirstPass));
			Assert("should not match <P&LAppropriationAccount>", !ValueProviderToTest.IsResponsibleForReplacing("<P&LAppropriationAccount>", Passes.FirstPass));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new PLAppropriationAccount();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('65BBEAF1-B62C-4837-B45B-B069FB0A5ECB', '9999.99.99', 'control account', 'BSH','DR',1, NULL, 0, 'LI')");
			var registryItem = ((PLAppropriationAccount)ValueProviderToTest).GetPLAppropriationAccountRegistryItem();
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid("65BBEAF1-B62C-4837-B45B-B069FB0A5ECB"));
		}
	}
}
