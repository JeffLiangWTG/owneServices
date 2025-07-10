using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE615MessageProviderTest : IE613And615CommonMessageProviderTest<IE615MessageProvider>
	{
		public void TestExportOperation()
		{
			AssertSame("ExportOperation", Provider, Provider.ExportOperation);
		}

		public void TestLRN()
		{
			AssertEquals("LRN", AESOutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}

		public void TestIsInTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				AssertEquals("Precondition", true, declaration.IsTransitionPeriodAES30);
				AssertEquals(true, Provider.IsInTransitionPeriod);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, false))
			{
				AssertEquals("Precondition", false, declaration.IsTransitionPeriodAES30);
				AssertEquals(false, Provider.IsInTransitionPeriod);
			}
		}

		protected override IE615MessageProvider GetProvider() => new IE615MessageProvider(entryHeader);
	}
}
