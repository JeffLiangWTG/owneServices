using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;
public abstract class CusExitConsignmentPackageConfigurationTest<T> : TestCaseWithFactory
	where T : CusExitConsignmentPackageConfiguration, new()
{
	public void TestGetValidationDecider() => CombineAssertions(() =>
	{
		var package = Factory.New<CusExitConsignmentPackage>();
		AssertType("base type", ExpectedBaseValidationDeciderType, configuration.GetValidationDecider(package));

		package = Factory.GetUcc6ExitHeader().CusExitConsignmentPackages.AddNew();
		AssertType("Ucc6", ExpectedUcc6ValidationDeciderType, configuration.GetValidationDecider(package));
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

sealed class CusExitConsignmentPackageConfigurationBaseOnlyTest : CusExitConsignmentPackageConfigurationTest<CusExitConsignmentPackageConfiguration>
{
	protected override Type ExpectedUcc6ValidationDeciderType => typeof(CusExitConsignmentPackageUcc6ValidationDecider);
}
