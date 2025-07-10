using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetAccountingControllerID))]
	sealed class GetAccountingControllerIDTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("GetAccountingControllerID", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<GetAccountingControllerID meh>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetAccountingControllerID(INV,AR)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetAccountingControllerID(INV,AP)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetAccountingControllerID(CRD, ar)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< GETACCOUNTINGCONTROLLERID ( CRD , ap ) >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(ControllerIDs.APInvoice.Name, ValueProviderToTest.GetReplacement("<GetAccountingControllerID(INV,AP)>", Report));
			AssertEquals(ControllerIDs.ARCreditNote.Name, ValueProviderToTest.GetReplacement("<GetAccountingControllerID(CRD,AR)>", Report));
		}

		[ExpectNoExceptions]
		public void TestReplacement_WithInvalidTransactionTypeAndLedger()
		{
			ValueProviderToTest.GetReplacement("<GetAccountingControllerID(XXX,XXX>", Report);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new GetAccountingControllerID();
		}
	}
}
