using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

public abstract class CusExitConsignmentItemConfigurationTest<T> : TestCaseWithFactory
	where T : CusExitConsignmentItemConfiguration, new()
{
	public void TestGetValidationDecider() => CombineAssertions(() =>
	{
		var item = Factory.New<CusExitConsignmentItem>();
		AssertType("base type", ExpectedBaseValidationDeciderType, configuration.GetValidationDecider(item));

		item = Factory.GetUcc6ExitHeader().CusExitConsignments.AddNew().CusExitConsignmentItems.AddNew();
		AssertType("Ucc6", ExpectedUcc6ValidationDeciderType, configuration.GetValidationDecider(item));
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

sealed class CusExitConsignmentItemConfigurationBaseOnlyTest : CusExitConsignmentItemConfigurationTest<CusExitConsignmentItemConfiguration>
{
	protected override Type ExpectedUcc6ValidationDeciderType => typeof(CusExitConsignmentItemUcc6ValidationDecider);
}

