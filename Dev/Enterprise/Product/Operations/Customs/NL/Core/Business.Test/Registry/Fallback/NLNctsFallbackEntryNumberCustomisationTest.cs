using System.Linq;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(NLNctsFallbackEntryNumberCustomisation))]
sealed class NLNctsFallbackEntryNumberCustomisationTest : RegistryBusinessObjectTemplateTestCase<NLNctsFallbackEntryNumberCustomisation>
{
	public void TestElementAvailibility()
	{
		var nctsFallbackEntryNumberCustomisation = new NLNctsFallbackEntryNumberCustomisation();

		CombineAssertions(() =>
		{
			AssertEquals(5, nctsFallbackEntryNumberCustomisation.Elements.Count);
			AssertEquals(1, nctsFallbackEntryNumberCustomisation.Elements.Where(x => x.Key == BillOfLadingNumberCustomisationElement.Keys.YearAsDigit).Count());
			AssertEquals(1, nctsFallbackEntryNumberCustomisation.Elements.Where(x => x.Key == BillOfLadingNumberCustomisationElement.Keys.SequenceNumber).Count());
			AssertEquals(1, nctsFallbackEntryNumberCustomisation.Elements.Where(x => x.Key == BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits).Count());
			AssertEquals(1, nctsFallbackEntryNumberCustomisation.Elements.Where(x => x.Key == BillOfLadingNumberCustomisationElement.Keys.TransportMode).Count());
			AssertEquals(1, nctsFallbackEntryNumberCustomisation.Elements.Where(x => x.Key == BillOfLadingNumberCustomisationElement.Keys.Direction).Count());
		});
	}

	protected override bool RequiresFactory => false;

	protected override bool RequiresFallbackLevel => false;

	protected override NLNctsFallbackEntryNumberCustomisation GetBusinessObjectToClone()
	{
		return new NLNctsFallbackEntryNumberCustomisation();
	}

	protected override NLNctsFallbackEntryNumberCustomisation GetBusinessObjectToSerialise()
	{
		return new NLNctsFallbackEntryNumberCustomisation();
	}
}
