using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class IMatchingExtensionsTest : TestCaseWithFactory
	{
		public void TestUnmatchingResultEnumStructureForDataErrors()
		{
			var enumType = typeof(UnmatchingResult);
			var dataErrorInitialValue = UnmatchingResult.DataError;
			var dataErrorInitialValueName = Enum.GetName(enumType, dataErrorInitialValue);
			foreach (var valueName in Enum.GetNames(enumType))
			{
				var value = (UnmatchingResult)Enum.Parse(enumType, valueName);
				if (value > dataErrorInitialValue)
				{
					AssertStartsWith(string.Format("Value '{0}': All data error result names should start with data error initial value name for easy code reading.", valueName), dataErrorInitialValueName, valueName);
				}
				if (valueName != dataErrorInitialValueName && valueName.StartsWith(dataErrorInitialValueName))
				{
					Assert(string.Format("Value '{0}': All values which name starts with data error initial value name should have value more than data error initial value. This will allow to do a check \"result > data error initial value\" to check whether result contains any data errors.",
						valueName), value > dataErrorInitialValue);
				}
			}
		}

		public void TestGenerateMatchLinks()
		{
			AssertGenerateMatchLinks(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestGenerateMatchLinks_EnableNewOSOutstandingAmountFeature()
		{
			AssertGenerateMatchLinks(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertGenerateMatchLinks(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			IMatchingImplementationForTest iMatchingForTest = Factory.New<IMatchingImplementationForTest>();
			iMatchingForTest.CurrentMatchGroup = new TransactionMatchLinkGroup(Factory);

			AssertEquals("CurrentMatchGroup should not be registered as Editable Child", false, iMatchingForTest.IsRegisteredEditableChildObject(iMatchingForTest.CurrentMatchGroup));
			AssertEquals("GenerateMatchLinksCore should not be called", 0, iMatchingForTest.GenerateMatchLinksCoreCallCount);
			iMatchingForTest.GenerateMatchLinks();
			AssertEquals("CurrentMatchGroup should be registered as Editable Child", true, iMatchingForTest.IsRegisteredEditableChildObject(iMatchingForTest.CurrentMatchGroup));
			AssertEquals("GenerateMatchLinksCore should be called", 1, iMatchingForTest.GenerateMatchLinksCoreCallCount);
		}

		public void TestGetIMatchingInfo()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001001", testObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
			var iMatchingForTest = invoice as IMatching;
			iMatchingForTest.OSPartialPaymentAmount = iMatchingForTest.OSOutstandingAmount;
			AssertEquals("Ledger: AP, Transaction type: INV, Payment Amount: -100, Outstanding Amount: -100, OS Payment Amount: -100, OS Outstanding Amount: -100, Currency: AUD, Exchange Rate Amount: 1", iMatchingForTest.GetIMatchingInfo());

			iMatchingForTest.OSPartialPaymentAmount = iMatchingForTest.OSOutstandingAmount + 0.01M;
			var line = invoice.Lines[0];
			var expectInfo = $@"Ledger: AP, Transaction type: INV, Payment Amount: -99.99, Outstanding Amount: -100, OS Payment Amount: -99.99, OS Outstanding Amount: -100, Currency: AUD, Exchange Rate Amount: 1
	Line: PK = {invoice.Lines[0].PK}, Charge Code = , GL Account = {invoice.Lines[0].GLHeader.AccountNum}, Type = CST, OS Amount = -100, Local Amount = -100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {line.AL_AH}, Job PK = {line.AL_JH}, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.";
			AssertEquals(expectInfo, iMatchingForTest.GetIMatchingInfo());
		}
	}
}
