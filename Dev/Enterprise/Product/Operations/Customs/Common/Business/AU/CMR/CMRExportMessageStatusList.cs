using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRExportOtherMessageStatusList : CodeDescriptionPairList
	{
		public CustomsEntryStatus GetExportOtherMessageStatusFromCode(string code)
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

		public CMRExportOtherMessageStatusList()
		{
			Add(CustomsEntryStatus.AwaitingWARRELOriginal);
			Add(CustomsEntryStatus.AwaitingWARRELWithdrawal);
			Add(CustomsEntryStatus.AwaitingWARRELReplacement);
			Add(CustomsEntryStatus.FailWARRELOriginal);
			Add(CustomsEntryStatus.FailWARRELWithdrawal);
			Add(CustomsEntryStatus.FailWARRELReplacement);
			Add(CustomsEntryStatus.ClearWARRELOriginal);
			Add(CustomsEntryStatus.ClearWARRELWithdrawal);
			Add(CustomsEntryStatus.ClearWARRELReplacement);
			Add(CustomsEntryStatus.AwaitingWARRETOriginal);
			Add(CustomsEntryStatus.AwaitingWARRETReplacement);
			Add(CustomsEntryStatus.FailWARRETOriginal);
			Add(CustomsEntryStatus.FailWARRETReplacement);
			Add(CustomsEntryStatus.ClearWARRETOriginal);
			Add(CustomsEntryStatus.ClearWARRETReplacement);
			Add(CustomsEntryStatus.AwaitingDEPRECOriginal);
			Add(CustomsEntryStatus.AwaitingDEPRECWithdrawal);
			Add(CustomsEntryStatus.AwaitingDEPRECReplacement);
			Add(CustomsEntryStatus.FailDEPRECOriginal);
			Add(CustomsEntryStatus.FailDEPRECWithdrawal);
			Add(CustomsEntryStatus.FailDEPRECReplacement);
			Add(CustomsEntryStatus.ClearDEPRECOriginal);
			Add(CustomsEntryStatus.ClearDEPRECWithdrawal);
			Add(CustomsEntryStatus.ClearDEPRECReplacement);
			Add(CustomsEntryStatus.ErrorDEPRECOriginal);
			Add(CustomsEntryStatus.ErrorDEPRECWithdrawal);
			Add(CustomsEntryStatus.ErrorDEPRECReplacement);
			Add(CustomsEntryStatus.AwaitingDEPRELOriginal);
			Add(CustomsEntryStatus.AwaitingDEPRELWithdrawal);
			Add(CustomsEntryStatus.AwaitingDEPRELReplacement);
			Add(CustomsEntryStatus.FailDEPRELOriginal);
			Add(CustomsEntryStatus.FailDEPRELWithdrawal);
			Add(CustomsEntryStatus.FailDEPRELReplacement);
			Add(CustomsEntryStatus.ClearDEPRELOriginal);
			Add(CustomsEntryStatus.ClearDEPRELWithdrawal);
			Add(CustomsEntryStatus.ClearDEPRELReplacement);
			Add(CustomsEntryStatus.ErrorDEPRELOriginal);
			Add(CustomsEntryStatus.ErrorDEPRELWithdrawal);
			Add(CustomsEntryStatus.ErrorDEPRELReplacement);
		}

		public static bool IsAwaitingResponse(string statusCode)
		{
			return
				statusCode == CustomsEntryStatus.AwaitingWARRELOriginal.Code ||
				statusCode == CustomsEntryStatus.AwaitingWARRELWithdrawal.Code ||
				statusCode == CustomsEntryStatus.AwaitingWARRELReplacement.Code ||
				statusCode == CustomsEntryStatus.AwaitingWARRETOriginal.Code ||
				statusCode == CustomsEntryStatus.AwaitingWARRETReplacement.Code ||
				statusCode == CustomsEntryStatus.AwaitingDEPRECOriginal.Code ||
				statusCode == CustomsEntryStatus.AwaitingDEPRECWithdrawal.Code ||
				statusCode == CustomsEntryStatus.AwaitingDEPRECReplacement.Code ||
				statusCode == CustomsEntryStatus.AwaitingDEPRELOriginal.Code ||
				statusCode == CustomsEntryStatus.AwaitingDEPRELWithdrawal.Code ||
				statusCode == CustomsEntryStatus.AwaitingDEPRELReplacement.Code;
		}
	}
}
