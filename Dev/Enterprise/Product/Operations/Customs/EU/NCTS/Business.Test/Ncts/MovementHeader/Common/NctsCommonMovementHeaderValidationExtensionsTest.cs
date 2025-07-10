using System;
using System.Linq.Expressions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsCommonMovementHeaderValidationExtensionsTest : TestCaseWithFactory
{
	public void TestIsDeparturePhase5RuleActive()
		=> AssertIsRuleActiveGeneric<INctsDepartureMovementHeaderPhase5ValidationDecider>(CreateDeparturePhase5MovementHeader(), x => x.IsRuleB1922Active);

	public void TestIsRuleActiveGeneric()
		=> AssertIsRuleActiveGeneric<INctsMovementHeaderValidationDecider>(CreateDeparturePhase5MovementHeader(), x => x.IsRuleB1858Active);

	NctsCommonMovementHeader CreateDeparturePhase5MovementHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader;
	}

	void AssertIsRuleActiveGeneric<TValidationDecider>(
		NctsCommonMovementHeader movementHeader,
		Expression<Func<TValidationDecider, bool>> rule)
		where TValidationDecider : class, INctsMovementHeaderValidationDecider => CombineAssertions(() =>
	{
		using var testContext = new MovementHeaderValidationDeciderTestContext<TValidationDecider>(Factory);
		var getRuleStatus = rule.Compile();

		testContext.ClearCachedValidationDecider(movementHeader);
		testContext.EnableRule(rule);
		var validationDecider = (TValidationDecider)movementHeader.ValidationDecider;
		Assert("Rule enabled", getRuleStatus(validationDecider));

		testContext.ClearCachedValidationDecider(movementHeader);
		testContext.DisableRule(rule);
		validationDecider = (TValidationDecider)movementHeader.ValidationDecider;
		AssertEquals("Rule disabled", expected: false, getRuleStatus(validationDecider));
	});
}
