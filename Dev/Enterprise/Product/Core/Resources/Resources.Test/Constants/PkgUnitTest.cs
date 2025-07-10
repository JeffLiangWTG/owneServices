using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class PkgUnitTest : TestCase
	{
		public void TestGetDescriptions()
		{
			AssertEquals("Envelope", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Envelope).ToString());
			AssertEquals("Envelopes", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Envelope, Constants.PluralState.Plural).ToString());
			AssertEquals("Envelope(s)", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Envelope, Constants.PluralState.PluralOrNonPlural).ToString());

			AssertEquals("Unit", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Unit).ToString());
			AssertEquals("Units", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Unit, Constants.PluralState.Plural).ToString());
			AssertEquals("Unit(s)", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Unit, Constants.PluralState.PluralOrNonPlural).ToString());

			AssertEquals("Gross", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Gross).ToString());
			AssertEquals("Gross", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Gross, Constants.PluralState.Plural).ToString());
			AssertEquals("Gross", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Gross, Constants.PluralState.PluralOrNonPlural).ToString());

			AssertEquals("Nothing in, nothing out, no kaboom", "", Constants.PkgUnit.GetDescription("").ToString());
		}
	}
}
