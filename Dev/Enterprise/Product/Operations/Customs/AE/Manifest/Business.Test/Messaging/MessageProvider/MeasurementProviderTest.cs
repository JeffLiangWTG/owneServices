using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class MeasurementProviderTest : Customs.Business.Testing.DataProviderTestCase<MeasurementProvider>
{
	[ExpectNoExceptions]
	public void TestMeasurementPurpose() => NUnit.Framework.Assert.That(GetProvider().MeasurementPurpose, NUnit.Framework.Is.EqualTo("ABC"));

	[ExpectNoExceptions]
	public void TestMeasurementUnit() => NUnit.Framework.Assert.That(GetProvider().MeasurementUnit, NUnit.Framework.Is.EqualTo("KGM"));

	[ExpectNoExceptions]
	public void TestMeasurementValue() => NUnit.Framework.Assert.That(GetProvider().MeasurementValue, NUnit.Framework.Is.EqualTo(123.45M));

	protected override MeasurementProvider GetProvider() => new MeasurementProvider("ABC", 123.45m, "KGM");
}
