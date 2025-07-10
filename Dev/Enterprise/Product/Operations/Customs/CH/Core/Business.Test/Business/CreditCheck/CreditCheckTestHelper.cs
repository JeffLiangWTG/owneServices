using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public static class CreditCheckTestHelper
{
	public static IDisposable DisposableCreditControllerOverride() => AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CreateAuthorizationRequirements());

	public static IDisposable DisposableCreditCheckOnSendOverride(bool value) => CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);

	static AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection CreateAuthorizationRequirements() => new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection
	{
		new AmountOrPercentageBasedThreeLevelAuthorisationRequirement
		{
			Amount = 1,
			Percentage = 0,
			Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo,
			AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly
		},
		new AmountOrPercentageBasedThreeLevelAuthorisationRequirement
		{
			Amount = 1,
			Percentage = 0,
			Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above,
			AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly
		}
	};
}
