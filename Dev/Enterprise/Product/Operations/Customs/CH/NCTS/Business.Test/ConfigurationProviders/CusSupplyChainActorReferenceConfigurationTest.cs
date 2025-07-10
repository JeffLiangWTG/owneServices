using System;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CusSupplyChainActorReferenceConfiguration))]
public class CusSupplyChainActorReferenceConfigurationTest : EU.NCTS.Business.Testing.CusSupplyChainActorReferenceConfigurationAbstractTest<CusSupplyChainActorReferenceConfiguration>
{
	protected override Type ExpectedCusSupplyChainActorReferenceValidationDeciderType => typeof(CusSupplyChainActorReferenceValidationDecider);
}
