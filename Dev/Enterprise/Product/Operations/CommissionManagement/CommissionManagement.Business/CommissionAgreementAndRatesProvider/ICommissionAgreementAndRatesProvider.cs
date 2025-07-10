using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.CommissionManagement.Business
{
	public interface ICommissionAgreementAndRates
	{
		OrgCommissionAgreement CommissionAgreement { get; }
		IReadOnlyCollection<RecipientRatePair> RecipientRatePairs { get; }
		IReadOnlyCollection<RecipientRatePair> GetRecipientRatePairs(ZGuid chargeCodePK);
	}

	public class RecipientRatePair
	{
		public RecipientRatePair(OrgCommissionAgreementRecipient recipient, OrgCommissionAgreementRecipientRate rate)
		{
			Recipient = recipient;
			Rate = rate;
		}

		public readonly OrgCommissionAgreementRecipient Recipient;
		public readonly OrgCommissionAgreementRecipientRate Rate;
	}
}
