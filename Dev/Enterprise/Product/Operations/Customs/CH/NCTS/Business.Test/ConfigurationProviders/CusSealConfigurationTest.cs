using System;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CusSealConfiguration))]
public class CusSealConfigurationTest : EU.NCTS.Business.Testing.CusSealConfigurationAbstractTest<CusSealConfiguration>
{
	protected override Type ExpectedCusSealPhase5ValidationDeciderType => typeof(CusSealPhase5ValidationDecider);

	protected override Type ExpectedCusSealValidationDeciderType => typeof(CusSealValidationDecider);
}
