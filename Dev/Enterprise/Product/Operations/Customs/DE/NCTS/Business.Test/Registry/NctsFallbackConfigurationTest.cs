using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing;

[TestedType(typeof(NctsFallbackConfiguration))]
sealed class NctsFallbackConfigurationTest : RegistryBusinessObjectTemplateTestCase<NctsFallbackConfiguration>
{
	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;

	protected override NctsFallbackConfiguration GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

	protected override NctsFallbackConfiguration GetBusinessObjectToSerialise()
	{
		BizObj.Start = ZDateTime.Today.AddHours(6).AddMinutes(30).AddSeconds(10);
		BizObj.CustomsIncidentNumber = "1234567890";
		return BizObj;
	}
}
