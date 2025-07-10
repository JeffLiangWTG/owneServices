using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

public abstract class CusExitContainerConfigurationTest<T> : TestCaseWithFactory
	where T : CusExitContainerConfiguration, new()
{
	public void TestGetValidationDecider() => CombineAssertions(() =>
	{
		var container = Factory.New<CusExitContainer>();
		AssertType("base type", ExpectedBaseValidationDeciderType, configuration.GetValidationDecider(container));

		container = Factory.GetUcc6ExitHeader().CusExitContainers.AddNew();
		AssertType("Ucc6", ExpectedUcc6ValidationDeciderType, configuration.GetValidationDecider(container));
	});

	protected virtual Type ExpectedBaseValidationDeciderType => null;

	protected abstract Type ExpectedUcc6ValidationDeciderType { get; }

	protected override void SetUp()
	{
		base.SetUp();
		configuration = new T();
	}
	protected T configuration;
}

sealed class CusExitContainerConfigurationBaseOnlyTest : CusExitContainerConfigurationTest<CusExitContainerConfiguration>
{
	protected override Type ExpectedUcc6ValidationDeciderType => typeof(CusExitContainerUcc6ValidationDecider);
}
