using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing;

[TestedType(typeof(NctsFallbackConfigurationValidation))]
sealed class NctsFallbackConfigurationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckStart()
	{
		var nctsFallbackConfiguration = new NctsFallbackConfiguration();
		ValidationTestHelper.AssertErrorIfNotEntered(nctsFallbackConfiguration.StartInfo);
	}

	public void TestCheckCustomsIncidentNumber()
	{
		var nctsFallbackConfiguration = new NctsFallbackConfiguration();
		ValidationTestHelper.AssertErrorIfNotEntered(nctsFallbackConfiguration.CustomsIncidentNumberInfo);
	}
}
