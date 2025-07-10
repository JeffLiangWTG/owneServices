using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

public abstract class CusExitConsignmentConfigurationTest<T> : TestCaseWithFactory
	where T : CusExitConsignmentConfiguration, new()
{
	public void TestCusExitConsignmentItemConfiguration() => AssertType(ExpectedCusExitConsignmentItemConfigurationType, configuration.CusExitConsignmentItemConfiguration);

	protected abstract Type ExpectedCusExitConsignmentItemConfigurationType { get; }

	protected override void SetUp()
	{
		base.SetUp();
		configuration = new T();
	}
	protected T configuration;
}

sealed class CusExitConsignmentConfigurationBaseOnlyTest : CusExitConsignmentConfigurationTest<CusExitConsignmentConfiguration>
{
	protected override Type ExpectedCusExitConsignmentItemConfigurationType => typeof(CusExitConsignmentItemConfiguration);
}
