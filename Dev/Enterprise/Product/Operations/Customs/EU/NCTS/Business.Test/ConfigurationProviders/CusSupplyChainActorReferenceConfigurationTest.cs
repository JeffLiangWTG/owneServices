using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(CusSupplyChainActorReferenceConfiguration))]
public abstract class CusSupplyChainActorReferenceConfigurationAbstractTest<T> : TestCaseWithFactory
	where T : CusSupplyChainActorReferenceConfiguration, new()
{
	public void TestValidationDecider()
	{
		AssertType(ExpectedCusSupplyChainActorReferenceValidationDeciderType, configuration.GetValidationDecider());
	}

	protected abstract Type ExpectedCusSupplyChainActorReferenceValidationDeciderType { get; }

	protected override void SetUp()
	{
		base.SetUp();
		configuration = new T();
	}

	T configuration;
}

[TestedType(typeof(CusSupplyChainActorReferenceConfiguration))]
sealed class CusSupplyChainActorReferenceConfigurationBaseOnlyTest : CusSupplyChainActorReferenceConfigurationAbstractTest<CusSupplyChainActorReferenceConfiguration>
{
	protected override Type ExpectedCusSupplyChainActorReferenceValidationDeciderType => typeof(CusSupplyChainActorReferenceValidationDecider);
}
