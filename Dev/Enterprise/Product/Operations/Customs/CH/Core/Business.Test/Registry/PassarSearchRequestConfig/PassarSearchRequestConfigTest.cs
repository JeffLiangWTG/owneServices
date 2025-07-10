using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PassarSearchRequestConfig))]
sealed class PassarSearchRequestConfigTest : RegistryBusinessObjectTemplateTestCase<PassarSearchRequestConfig>
{
	public void TestIsEnabled()
	{
		AssertEquals("Default", true, PassarSearchRequestConfig.IsEnabled);
	}

	public void TestTimeLimit()
	{
		AssertEquals("Default", 720, PassarSearchRequestConfig.TimeLimit);
	}

	protected override PassarSearchRequestConfig GetBusinessObjectToClone() => (PassarSearchRequestConfig)GetNewBusinessObject();

	protected override PassarSearchRequestConfig GetBusinessObjectToSerialise() => (PassarSearchRequestConfig)GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObject() => new PassarSearchRequestConfig(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));

	protected override bool RequiresFactory => false;
	protected override bool RequiresFallbackLevel => true;

	PassarSearchRequestConfig PassarSearchRequestConfig => passarSearchRequestConfig ??= (PassarSearchRequestConfig)GetNewBusinessObject();
	PassarSearchRequestConfig passarSearchRequestConfig;
}
