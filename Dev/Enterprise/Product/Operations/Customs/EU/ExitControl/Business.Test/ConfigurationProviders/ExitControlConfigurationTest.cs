using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

[TestsSubclassesOf(typeof(ExitControlConfiguration))]
public abstract class ExitControlConfigurationTest<C> : TestCaseWithFactory
	where C : ExitControlConfiguration
{
	public void TestCusExitSealConfiguration() => AssertType(GetCusExitSealConfigurationTypeForTest(), configuration.CusExitSealConfiguration);

	public void TestCusExitReportConfiguration() => AssertType(GetCusExitReportConfigurationTypeForTest(), configuration.CusExitReportConfiguration);

	public void TestCusExitContainerConfiguration() => AssertType(GetCusExitContainerConfigurationTypeForTest(), configuration.CusExitContainerConfiguration);

	public void TestCusExitConsignmentPackageConfiguration() => AssertType(GetCusExitConsignmentPackageConfigurationTypeForTest(), configuration.CusExitConsignmentPackageConfiguration);

	public void TestCusExitConsignmentItemConfiguration() => AssertType(GetCusExitConsignmentItemConfigurationTypeForTest(), configuration.CusExitConsignmentItemConfiguration);

	public void TestCusExitConsignmentConfiguration() => AssertType(GetCusExitConsignmentConfigurationTypeForTest(), configuration.CusExitConsignmentConfiguration);

	protected virtual Type GetCusExitSealConfigurationTypeForTest() => typeof(CusExitSealConfiguration);

	protected virtual Type GetCusExitReportConfigurationTypeForTest() => typeof(CusExitReportConfiguration);

	protected virtual Type GetCusExitContainerConfigurationTypeForTest() => typeof(CusExitContainerConfiguration);

	protected virtual Type GetCusExitConsignmentPackageConfigurationTypeForTest() => typeof(CusExitConsignmentPackageConfiguration);

	protected virtual Type GetCusExitConsignmentItemConfigurationTypeForTest() => typeof(CusExitConsignmentItemConfiguration);

	protected virtual Type GetCusExitConsignmentConfigurationTypeForTest() => typeof(CusExitConsignmentConfiguration);

	protected override void SetUp()
	{
		base.SetUp();
		configuration = (C)Activator.CreateInstance(typeof(C));
	}

	protected C configuration;
}

[TestedType(typeof(ExitControlConfiguration))]
sealed class ExitControlConfigurationBaseOnlyTest : ExitControlConfigurationTest<ExitControlConfiguration>
{
	public void TestGetConfiguration() => CombineAssertions(() =>
	{
		foreach (var item in configurationTestCases)
		{
			var configuration = ExitControlConfiguration.GetConfiguration(Factory, item.Key);
			AssertEquals($"Country Code {item.Key}", item.Value, configuration.GetType().Namespace);
		}
	});

	readonly Dictionary<string, string> configurationTestCases = new()
	{
		{ ZString.Empty, "Enterprise.Customs.EU.ExitControl.Business" },
		{ Core.Constants.CountryCodes.EuropeanUnion, "Enterprise.Customs.EU.ExitControl.Business" },
		{ Core.Constants.CountryCodes.Poland, "Enterprise.Customs.PL.ExitControl.Business" }
	};
}
