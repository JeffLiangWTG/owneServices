using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(EmailSenderOverride))]
	sealed class EmailSenderOverrideTest : ValueProviderTest
	{
		const string MacroKey = "<EmailSenderOverride>";

		public void TestMacroWorksWithOverride()
		{
			Report.MenuItem.SU_EmailSenderOverride = "mail@email.com";

			AssertEquals("mail@email.com", ValueProviderToTest.GetReplacement(MacroKey, Report));
		}

		public void TestNoOverride()
		{
			Report.MenuItem.SU_EmailSenderOverride = null;
			AssertEquals(ZString.Empty, ValueProviderToTest.GetReplacement(MacroKey, Report));

			Report.MenuItem.SU_EmailSenderOverride = "";
			AssertEquals(ZString.Empty, ValueProviderToTest.GetReplacement(MacroKey, Report));
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<EmailSenderOverridee>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<EEmailSenderOverride>", Passes.FirstPass));

			Assert(ValueProviderToTest.IsResponsibleForReplacing(MacroKey, Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing(MacroKey.ToLowerInvariant(), Passes.FirstPass));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new EmailSenderOverride();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MenuItem.SU_EmailSenderOverride = "mail@email.com";
		}
	}
}
