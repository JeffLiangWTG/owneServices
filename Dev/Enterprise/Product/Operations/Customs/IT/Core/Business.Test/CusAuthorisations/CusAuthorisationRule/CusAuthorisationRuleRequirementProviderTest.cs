using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CusAuthorisationRuleRequirementProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception when cusAuthorisationHeader param is null", () => new CusAuthorisationRuleRequirementProvider(null));
	}

	public void TestLocFormatRuleRequirement()
	{
		var authorisationRuleRequirementProvider = new CusAuthorisationRuleRequirementProvider(authorisationHeader);

		var locFormatRuleRequirement = authorisationRuleRequirementProvider.LocRuleRequirement;
		AssertNotNull("LocFormatRuleRequirement", locFormatRuleRequirement);
		AssertRuleRequirmentProperties("LocFormatRuleRequirement", locFormatRuleRequirement, "LOC", additionalValidatorOnValueCollectionCount: 1);
	}

	public void TestDocRuleRequirement()
	{
		var authorisationRuleRequirementProvider = new CusAuthorisationRuleRequirementProvider(authorisationHeader);

		var docFormatRuleRequirement = authorisationRuleRequirementProvider.DocRuleRequirement;
		AssertNotNull("DocFormatRuleRequirement", docFormatRuleRequirement);
		AssertRuleRequirmentProperties("DocFormatRuleRequirement", docFormatRuleRequirement, "DOC", expectedMaxAllowed: 1);
	}

	public void TestUseRuleRequirement()
	{
		var authorisationRuleRequirementProvider = new CusAuthorisationRuleRequirementProvider(authorisationHeader);

		var useFormatRuleRequirement = authorisationRuleRequirementProvider.UseRuleRequiremnt;
		AssertNotNull("UseFormatRuleRequirement", useFormatRuleRequirement);
		AssertRuleRequirmentProperties("UseFormatRuleRequirement", useFormatRuleRequirement, "USE", 1, 1, 0);
	}

	protected override void SetUp()
	{
		base.SetUp();

		authorisationHeader = Factory.New<CusAuthorisationHeader>();
	}
	CusAuthorisationHeader authorisationHeader;

	void AssertRuleRequirmentProperties(string assertionMessage, CusAuthorisationRuleRequirement ruleRequirement, string expectedRuleType, int expectedMinRequired = 0, int expectedMaxAllowed = 0, int additionalValidatorOnValueCollectionCount = 0)
	{
		CombineAssertions(assertionMessage, () =>
		{
			AssertEquals("RuleType", expectedRuleType, ruleRequirement.RuleType);
			AssertEquals("MinRequired", expectedMinRequired, ruleRequirement.MinRequired);
			AssertEquals("MaxAllowed", expectedMaxAllowed, ruleRequirement.MaxAllowed);
			AssertEquals("AdditionalValidatorOnValueCollection", additionalValidatorOnValueCollectionCount, ruleRequirement.AdditionalValidatorOnValueCollection.Count());
		});
	}
}
