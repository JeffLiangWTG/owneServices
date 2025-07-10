using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	// Used for JE_MessageStatus in JobDeclaration, CH_Status on EntryHeader

	public class CMRImportMessageStatusList : CodeDescriptionPairList
	{
		public CustomsEntryStatus GetCustomsEntryStatusFromCode(string code)
		{
			foreach (CustomsEntryStatus entryStatus in this)
			{
				if (entryStatus.Code == code)
				{
					return entryStatus;
				}
			}
			return null;
		}

		public CMRImportMessageStatusList()
		{
			Add(CustomsEntryStatus.NotSent);
			Add(CustomsEntryStatus.HoldAwaiting);
			Add(CustomsEntryStatus.DeclarationWorkComplete);

			Add(CustomsEntryStatus.ScheduledLodgeWithPayment);
			Add(CustomsEntryStatus.ScheduledLodgeWithoutPayment);
			Add(CustomsEntryStatus.ScheduledPayment);

			Add(CustomsEntryStatus.AwaitingSAC);
			Add(CustomsEntryStatus.AwaitingPreLodge);
			Add(CustomsEntryStatus.AwaitingFormalLodge);
			Add(CustomsEntryStatus.AwaitingPayment);
			Add(CustomsEntryStatus.AwaitingAmendment);
			Add(CustomsEntryStatus.AwaitingWithdrawal);

			Add(CustomsEntryStatus.FailSAC);
			Add(CustomsEntryStatus.FailPreLodge);
			Add(CustomsEntryStatus.FailFormalLodge);
			Add(CustomsEntryStatus.FailPayment);
			Add(CustomsEntryStatus.FailAmendment);
			Add(CustomsEntryStatus.FailWithdrawal);

			Add(CustomsEntryStatus.ClearSAC);
			Add(CustomsEntryStatus.ClearPreLodge);
			Add(CustomsEntryStatus.ClearFormalLodge);
			Add(CustomsEntryStatus.ClearPayment);
			Add(CustomsEntryStatus.ClearAmendment);
			Add(CustomsEntryStatus.ClearWithdrawal);
		}

		public static bool IsAwaitingResponse(string statusCode)
		{
			return
				statusCode == CustomsEntryStatus.AwaitingSAC.Code ||
				statusCode == CustomsEntryStatus.AwaitingPreLodge.Code ||
				statusCode == CustomsEntryStatus.AwaitingFormalLodge.Code ||
				statusCode == CustomsEntryStatus.AwaitingPayment.Code ||
				statusCode == CustomsEntryStatus.AwaitingAmendment.Code ||
				statusCode == CustomsEntryStatus.AwaitingWithdrawal.Code;
		}

		public static bool IsFailedResponse(string statusCode)
		{
			return
				statusCode == CustomsEntryStatus.FailSAC.Code ||
				statusCode == CustomsEntryStatus.FailPreLodge.Code ||
				statusCode == CustomsEntryStatus.FailFormalLodge.Code ||
				statusCode == CustomsEntryStatus.FailPayment.Code ||
				statusCode == CustomsEntryStatus.FailAmendment.Code ||
				statusCode == CustomsEntryStatus.FailWithdrawal.Code;
		}
	}
}

