using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public abstract class CusAuthorizationUsageConfigurationTest<TConfiguration> : TestCaseWithFactory where TConfiguration : CusAuthorizationUsageConfiguration
{
	public void TestValidationDeciderPhase5()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType(ExpectedPhase5ValidationDeciderType, configuration.GetValidationDecider(header));
	}

	protected abstract Type ExpectedPhase5ValidationDeciderType { get; }

	protected override void SetUp()
	{
		base.SetUp();
		configuration = (TConfiguration)Activator.CreateInstance(typeof(TConfiguration));
	}
	protected TConfiguration configuration;
}

sealed class CusAuthorizationUsageConfigurationBaseOnlyTest : CusAuthorizationUsageConfigurationTest<CusAuthorizationUsageConfiguration>
{
	protected override Type ExpectedPhase5ValidationDeciderType => typeof(CusAuthorizationUsagePhase5ValidationDecider);
}
