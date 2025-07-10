using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

public abstract class CusExitSealConfigurationTest<T> : TestCaseWithFactory
	where T : CusExitSealConfiguration, new()
{
	public void TestGetValidationDecider() => CombineAssertions(() =>
	{
		var seal = Factory.New<CusExitSeal>();
		AssertType("base type", ExpectedBaseValidationDeciderType, configuration.GetValidationDecider(seal));

		seal = Factory.GetUcc6ExitHeader().CusExitContainers.AddNew().AllSealNumbers.AddNew();
		AssertType("Ucc6", ExpectedUcc6ValidationDeciderType, configuration.GetValidationDecider(seal));
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

sealed class CusExitSealConfigurationBaseOnlyTest : CusExitSealConfigurationTest<CusExitSealConfiguration>
{
	protected override Type ExpectedUcc6ValidationDeciderType => typeof(CusExitSealUcc6ValidationDecider);
}
