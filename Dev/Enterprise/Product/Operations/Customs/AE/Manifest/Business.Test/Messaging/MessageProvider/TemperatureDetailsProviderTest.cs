using Enterprise.Edifact.D23A.Elements;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class TemperatureDetailsProviderTest : Customs.Business.Testing.DataProviderTestCase<TemperatureDetailsProvider>
{
	[ExpectNoExceptions]
	public void TestTemperatureTypeCode() => NUnit.Framework.Assert.That(GetProvider().TemperatureTypeCode, Is.EqualTo(TemperatureTypeCodeQualifierList.TransportTemperature.ToString()));

	[ExpectNoExceptions]
	public void TestTemperatureDegree()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().TemperatureDegree, Is.EqualTo(0m), "TemperatureDegree not set");

			container.ACN_SetPointTemperature = 30m;
			NUnit.Framework.Assert.That(GetProvider().TemperatureDegree, Is.EqualTo(30m), "TemperatureDegree set");
		});
	}

	[ExpectNoExceptions]
	public void TestTemperatureUnit()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().TemperatureUnit, Is.Null.Or.Empty, "TemperatureUnit not set - should be [null] or [empty]");

			container.ACN_SetPointTemperatureUnit = "C";
			NUnit.Framework.Assert.That(GetProvider().TemperatureUnit, Is.EqualTo("CEL"), "TemperatureUnit set");

			container.ACN_SetPointTemperatureUnit = "F";
			NUnit.Framework.Assert.That(GetProvider().TemperatureUnit, Is.EqualTo("FAH"), "TemperatureUnit set");

			container.ACN_SetPointTemperatureUnit = "A";
			NUnit.Framework.Assert.That(GetProvider().TemperatureUnit, Is.EqualTo("A"), "TemperatureUnit set");
		});
	}

	protected override TemperatureDetailsProvider GetProvider() => new TemperatureDetailsProvider(container);

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<AsycudaManifestHeader>();
		container = header.Containers.AddNew();
	}
	AsycudaContainer container;
}
