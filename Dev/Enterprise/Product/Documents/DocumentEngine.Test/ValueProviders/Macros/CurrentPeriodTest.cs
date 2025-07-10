using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CurrentPeriod))]
	sealed class CurrentPeriodTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CurrentPeriod >", ValueProviderToTest.IsResponsibleForReplacing("< CurrentPeriod >", Passes.FirstPass));
			Assert("should match < Current Period>", ValueProviderToTest.IsResponsibleForReplacing("< Current Period>", Passes.FirstPass));
			Assert("should match < Current Period >", ValueProviderToTest.IsResponsibleForReplacing("< Current Period >", Passes.FirstPass));
			Assert("should match < CurrentPeriod>", ValueProviderToTest.IsResponsibleForReplacing("< CurrentPeriod>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(((CurrentPeriod)ValueProviderToTest).GetPeriod(), ValueProviderToTest.GetReplacement("<CurrentPeriod>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CurrentPeriod();
		}

		[TestDate(2003, 3, 5)]
		public override void TestDocumentation()
		{
			var testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(200301, new ZDateTime(2003, 3, 1), new ZDateTime(2003, 3, 30));
			base.TestDocumentation();
		}
	}
}
