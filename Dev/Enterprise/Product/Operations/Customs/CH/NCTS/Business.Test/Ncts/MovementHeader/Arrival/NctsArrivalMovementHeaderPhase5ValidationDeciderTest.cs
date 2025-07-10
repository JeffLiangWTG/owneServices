using System;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class InNCTSTransitionPeriodNctsArrivalMovementHeaderPhase5ValidationDeciderTest : NctsArrivalMovementHeaderPhase5ValidationDeciderTest
{
	protected override bool IsNCTSTransitionPeriod => true;
}

sealed class AfterNCTSTransitionPeriodNctsArrivalMovementHeaderPhase5ValidationDeciderTest : NctsArrivalMovementHeaderPhase5ValidationDeciderTest
{
	protected override bool IsNCTSTransitionPeriod => false;
}

abstract class NctsArrivalMovementHeaderPhase5ValidationDeciderTest : TestCase
{
	public void TestIsInBondEntryTypeListValidationActive()
	{
		AssertEquals(expected: false, validationDecider.IsInBondEntryTypeListValidationActive);
	}

	public void TestIsRuleB1858Active()
	{
		AssertEquals(expected: false, validationDecider.IsRuleB1858Active);
	}

	public void TestIsRuleC0191Active() => AssertEquals(expected: false, validationDecider.IsRuleC0191Active);

	public void TestIsRuleNR0009Active() => AssertEquals(expected: false, validationDecider.IsRuleNR0009Active);

	public void TestIsRuleNR0026Active() => AssertEquals(expected: true, validationDecider.IsRuleNR0026Active);

	public void TestIsRuleNR0028Active() => AssertEquals(expected: true, validationDecider.IsRuleNR0028Active);

	public void TestIsRuleNR0076Active() => AssertEquals(expected: false, validationDecider.IsRuleNR0076Active);

	public void TestIsRuleTR0022Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0022Active);

	public void TestIsRuleTR0034Active() => AssertEquals(expected: false, validationDecider.IsRuleTR0034Active);

	public void TestIsRuleTR0042Active() => AssertEquals(expected: false, validationDecider.IsRuleTR0042Active);

	public void TestIsRuleTR0063Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0063Active);

	public void TestIsRuleTR0071Active() => AssertEquals(expected: false, validationDecider.IsRuleTR0071Active);

	public void TestIsRuleTR0072Active() => AssertEquals(expected: false, validationDecider.IsRuleTR0072Active);

	public void TestIsRuleTR0091Active()
	{
		AssertEquals(expected: true, validationDecider.IsRuleTR0091Active);
	}

	public void TestIsRuleTR0098Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0098Active);

	protected override void SetUp()
	{
		base.SetUp();
		transitionPeriodHelper = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, IsNCTSTransitionPeriod);
		validationDecider = new NctsArrivalMovementHeaderPhase5ValidationDecider();
	}

	NctsArrivalMovementHeaderPhase5ValidationDecider validationDecider;

	protected abstract bool IsNCTSTransitionPeriod { get; }

	protected override void TearDown()
	{
		base.TearDown();
		transitionPeriodHelper?.Dispose();
	}

	IDisposable transitionPeriodHelper;
}
