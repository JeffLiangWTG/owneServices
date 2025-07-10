using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE613MessageProviderTest : IE613And615CommonMessageProviderTest<IE613MessageProvider>
	{
		public void TestExportOperation()
		{
			AssertSame("ExportOperation", Provider, Provider.ExportOperation);
		}

		public void TestMRN()
		{
			entryHeader.MovementReferenceNumberSetter("MRN2343234242");

			AssertEquals("MRN", "MRN2343234242", Provider.MRN);
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

		protected override IE613MessageProvider GetProvider() => new IE613MessageProvider(entryHeader);
	}
}
