using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

public abstract class CusExitReportConfigurationTest<T> : TestCaseWithFactory
	where T : CusExitReportConfiguration, new()
{
	public void TestGetValidationDecider() => CombineAssertions(() =>
	{
		AssertType("base type", ExpectedBaseValidationDeciderType, configuration.GetValidationDecider(GetExitReport()));

		AssertType("Ucc6", ExpectedUcc6ValidationDeciderType, configuration.GetValidationDecider(GetUcc6ExitReport()));
	});

	protected virtual Type ExpectedBaseValidationDeciderType => null;

	protected abstract Type ExpectedUcc6ValidationDeciderType { get; }

	protected virtual CusExitReport GetExitReport() => Factory.New<CusExitReport>();

	protected virtual CusExitReport GetUcc6ExitReport() => Factory.GetUcc6ExitHeader().CusExitReports.AddNew();

	protected override void SetUp()
	{
		base.SetUp();
		configuration = new T();
	}
	protected T configuration;
}

sealed class CusExitReportConfigurationBaseOnlyTest : CusExitReportConfigurationTest<CusExitReportConfiguration>
{
	protected override Type ExpectedUcc6ValidationDeciderType => typeof(CusExitReportUcc6ValidationDecider);
}

