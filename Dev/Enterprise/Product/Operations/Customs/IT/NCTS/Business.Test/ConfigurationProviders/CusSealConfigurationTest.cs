using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;
[TestedType(typeof(CusSealConfiguration))]
sealed class CusSealConfigurationTest : CusSealConfigurationAbstractTest<CusSealConfiguration>
{
	protected override Type ExpectedCusSealPhase5ValidationDeciderType => typeof(CusSealPhase5ValidationDecider);

	protected override Type ExpectedCusSealValidationDeciderType => typeof(CusSealValidationDecider);
}
