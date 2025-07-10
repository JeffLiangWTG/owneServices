using System.Collections.Immutable;
using System.ComponentModel;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU
{
	[ImmutableObject(true)]
	public class CustomsEntryStatus : CodeDescriptionPair
	{
		CustomsEntryStatus(object code, MultilingualString description) : base(code, description) { }

		// General
		public static readonly CustomsEntryStatus NotSent = new CustomsEntryStatus("", ResString.GetMultilingualString("Customs.EntryStatus.NotSent", "Not Sent"));
		public static readonly CustomsEntryStatus HoldAwaiting = new CustomsEntryStatus("HWT", ResString.GetMultilingualString("Customs.EntryStatus.HoldAwaiting", "Hold Awaiting"));
		public static readonly CustomsEntryStatus DeclarationWorkComplete = new CustomsEntryStatus("DWC", ResString.GetMultilingualString("Customs.EntryStatus.DeclarationWorkComplete", "Declaration Work Complete"));

		// Drawbacks
		public static readonly CustomsEntryStatus Lodged = new CustomsEntryStatus("DBL", ResString.GetMultilingualString("Customs.EntryStatus.Lodged", "Lodged"));

		// Export
		public static readonly CustomsEntryStatus AwaitingOriginal = new CustomsEntryStatus("WTO", ResString.GetMultilingualString("Customs.EntryStatus.AwaitingOriginal", "Awaiting Original Response"));
		public static readonly CustomsEntryStatus AwaitingWithdrawal = new CustomsEntryStatus("WTW", ResString.GetMultilingualString("Customs.EntryStatus.AwaitingWithdrawal", "Awaiting Withdrawal Response"));
		public static readonly CustomsEntryStatus AwaitingReplacement = new CustomsEntryStatus("WTR", ResString.GetMultilingualString("Customs.EntryStatus.AwaitingReplacement", "Awaiting Replacement Response"));
		public static readonly CustomsEntryStatus ErrorOriginal = new CustomsEntryStatus("ERO", ResString.GetMultilingualString("Customs.EntryStatus.ErrorOriginal", "Error in Original"));
		public static readonly CustomsEntryStatus ErrorWithdrawal = new CustomsEntryStatus("ERW", ResString.GetMultilingualString("Customs.EntryStatus.ErrorWithdrawal", "Error in Withdrawal"));
		public static readonly CustomsEntryStatus ErrorReplacement = new CustomsEntryStatus("ERR", ResString.GetMultilingualString("Customs.EntryStatus.ErrorReplacement", "Error in Replacement"));
		public static readonly CustomsEntryStatus FailOriginal = new CustomsEntryStatus("FLO", ResString.GetMultilingualString("Customs.EntryStatus.FailOriginal", "Failed Original"));
		public static readonly CustomsEntryStatus FailWithdrawal = new CustomsEntryStatus("FLW", ResString.GetMultilingualString("Customs.EntryStatus.FailWithdrawal", "Failed Withdrawal"));
		public static readonly CustomsEntryStatus FailReplacement = new CustomsEntryStatus("FLR", ResString.GetMultilingualString("Customs.EntryStatus.FailReplacement", "Failed Replacement"));
		public static readonly CustomsEntryStatus Cancelled = new CustomsEntryStatus("CAN", ResString.GetMultilingualString("Customs.EntryStatus.Cancelled", "Declaration Canceled"));
		public static readonly CustomsEntryStatus ClearOriginal = new CustomsEntryStatus("CLO", ResString.GetMultilingualString("Customs.EntryStatus.ClearOriginal", "Declaration Clear (Original)"));
		public static readonly CustomsEntryStatus ClearWithdrawal = new CustomsEntryStatus("CLW", ResString.GetMultilingualString("Customs.EntryStatus.ClearWithdrawal", "Declaration Withdrawn"));
		public static readonly CustomsEntryStatus ClearReplacement = new CustomsEntryStatus("CLR", ResString.GetMultilingualString("Customs.EntryStatus.ClearReplacement", "Declaration Clear (Replacement)"));
		public static readonly CustomsEntryStatus Embargoed = new CustomsEntryStatus("EMB", ResString.GetMultilingualString("Customs.EntryStatus.Embargoed", "Embargoed"));
		public static readonly CustomsEntryStatus ReleasedFromEmbargo = new CustomsEntryStatus("REL", ResString.GetMultilingualString("Customs.EntryStatus.ReleasedFromEmbargo", "Released From Embargo"));
		public static readonly CustomsEntryStatus Transferred = new CustomsEntryStatus("TFD", ResString.GetMultilingualString("Customs.EntryStatus.Transferred", "Transferred"));

		public static readonly CustomsEntryStatus AwaitingWARRELOriginal = new CustomsEntryStatus("WWO", ResString.GetMultilingualString("9805A0FC-E57C-4D43-900C-F99ABF67CFF0", "Awaiting Original WARREL Response"));
		public static readonly CustomsEntryStatus AwaitingWARRELWithdrawal = new CustomsEntryStatus("WWW", ResString.GetMultilingualString("AEF519FF-54AD-42E9-B875-3F3C9467B81A", "Awaiting Withdraw WARREL Response"));
		public static readonly CustomsEntryStatus AwaitingWARRELReplacement = new CustomsEntryStatus("WWR", ResString.GetMultilingualString("6173044D-B561-4053-B3C1-117F4916ED51", "Awaiting Replace WARREL Response"));
		public static readonly CustomsEntryStatus FailWARRELOriginal = new CustomsEntryStatus("FWO", ResString.GetMultilingualString("AA069FAA-31E1-4C88-940A-122FB2CFEC10", "Original WARREL Rejected, press Detail for more information"));
		public static readonly CustomsEntryStatus FailWARRELWithdrawal = new CustomsEntryStatus("FWW", ResString.GetMultilingualString("31016EE2-2B65-428C-B21D-C5E0D1834A29", "Withdraw WARREL Rejected, press Detail for more information"));
		public static readonly CustomsEntryStatus FailWARRELReplacement = new CustomsEntryStatus("FWR", ResString.GetMultilingualString("79E3A103-963C-4ED6-BEA2-7BCC945126A8", "Replace WARREL Rejected, press Detail for more information"));
		public static readonly CustomsEntryStatus ClearWARRELOriginal = new CustomsEntryStatus("CWO", ResString.GetMultilingualString("A8900A85-E0A5-4A85-B180-6A03A0CDF93C", "WARREL Clear, press Detail for more information"));
		public static readonly CustomsEntryStatus ClearWARRELWithdrawal = new CustomsEntryStatus("CWW", ResString.GetMultilingualString("A4F1DCCA-2C84-438D-9A09-6810CAAD3788", "WARREL Withdrawn, press Detail for more information"));
		public static readonly CustomsEntryStatus ClearWARRELReplacement = new CustomsEntryStatus("CWR", ResString.GetMultilingualString("4F76C72E-701E-49DD-A30F-D4A73A145EBA", "WARREL Replaced, press Detail for more information"));

		public static readonly CustomsEntryStatus AwaitingWARRETOriginal = new CustomsEntryStatus("WNO", ResString.GetMultilingualString("52B048C6-0387-4B55-8707-23861A2E0C95", "Awaiting Original WARRET Response"));
		public static readonly CustomsEntryStatus AwaitingWARRETReplacement = new CustomsEntryStatus("WNR", ResString.GetMultilingualString("2348214C-AC13-4680-888F-043615FAD5DA", "Awaiting Replace WARRET Response"));
		public static readonly CustomsEntryStatus FailWARRETOriginal = new CustomsEntryStatus("FNO", ResString.GetMultilingualString("0F9F09F6-022C-4E87-A75F-AE4CDCF594FC", "Original WARRET Rejected, press Detail for more information"));
		public static readonly CustomsEntryStatus FailWARRETReplacement = new CustomsEntryStatus("FNR", ResString.GetMultilingualString("13D0E969-9F02-41A5-B050-14815802A28D", "Replace WARRET Rejected, press Detail for more information"));
		public static readonly CustomsEntryStatus ClearWARRETOriginal = new CustomsEntryStatus("CNO", ResString.GetMultilingualString("9EDB5F98-DB0E-4B79-AE56-2F82A77BF327", "WARRET Clear, press Detail for more information"));
		public static readonly CustomsEntryStatus ClearWARRETReplacement = new CustomsEntryStatus("CNR", ResString.GetMultilingualString("28686128-1AC2-403C-A252-35C350A233FA", "WARRET Replaced, press Detail for more information"));

		public static readonly CustomsEntryStatus AwaitingDEPRECOriginal = new CustomsEntryStatus("WDO", ResString.GetMultilingualString("439C9DF3-6F66-4C8D-BD65-0175125029C0", "Awaiting Original DEPREC Response"));
		public static readonly CustomsEntryStatus AwaitingDEPRECWithdrawal = new CustomsEntryStatus("WDW", ResString.GetMultilingualString("8C28CAE1-85FE-4CE9-89F3-B0259FFB763A", "Awaiting Withdraw DEPREC Response"));
		public static readonly CustomsEntryStatus AwaitingDEPRECReplacement = new CustomsEntryStatus("WDR", ResString.GetMultilingualString("435EB9F8-8C7E-4C8E-854B-6DBD3C8236C1", "Awaiting Replace DEPREC Response"));
		public static readonly CustomsEntryStatus FailDEPRECOriginal = new CustomsEntryStatus("FDO", ResString.GetMultilingualString("87F782A0-9C27-44DA-BB23-400594F01B4C", "Original DEPREC Rejected, press Detail for more information"));
		public static readonly CustomsEntryStatus FailDEPRECWithdrawal = new CustomsEntryStatus("FDW", ResString.GetMultilingualString("56823B50-66AC-4FA5-A5D9-905E594AC221", "Withdraw DEPREC Rejected, press Detail for more information"));
		public static readonly CustomsEntryStatus FailDEPRECReplacement = new CustomsEntryStatus("FDR", ResString.GetMultilingualString("E19D954B-381D-4A83-857E-86FD35DE5F2A", "Replace DEPREC Rejected, press Detail for more information"));
		public static readonly CustomsEntryStatus ClearDEPRECOriginal = new CustomsEntryStatus("CDO", ResString.GetMultilingualString("17F0AA81-8068-4A79-90F0-2A8B5D4DE10B", "DEPREC Clear"));
		public static readonly CustomsEntryStatus ClearDEPRECWithdrawal = new CustomsEntryStatus("CDW", ResString.GetMultilingualString("AE846B1D-4036-4ED4-AB7C-3701BDDE2982", "DEPREC Withdrawn"));
		public static readonly CustomsEntryStatus ClearDEPRECReplacement = new CustomsEntryStatus("CDR", ResString.GetMultilingualString("ECA60553-A196-462D-97BD-9AB3BED2A3C3", "DEPREC Replaced"));
		public static readonly CustomsEntryStatus ErrorDEPRECOriginal = new CustomsEntryStatus("EDO", ResString.GetMultilingualString("88C42E7D-E9C2-4FBC-8194-0E4EA16DCB1A", "DEPREC Accepted with errors, press Detail for more information"));
		public static readonly CustomsEntryStatus ErrorDEPRECWithdrawal = new CustomsEntryStatus("EDW", ResString.GetMultilingualString("CACEAB74-D0D0-4230-8F9D-148C8AE1A32A", "DEPREC Withdrawn with errors, press Detail for more information"));
		public static readonly CustomsEntryStatus ErrorDEPRECReplacement = new CustomsEntryStatus("EDR", ResString.GetMultilingualString("32A9BC2F-4FCA-499E-AB23-40D5376ABAD4", "DEPREC Replaced with errors, press Detail for more information"));

		public static readonly CustomsEntryStatus AwaitingDEPRELOriginal = new CustomsEntryStatus("WGO", ResString.GetMultilingualString("8AE03E6E-1924-4B31-AF17-4D586FDD91A6", "Awaiting Original DEPREL Response"));
		public static readonly CustomsEntryStatus AwaitingDEPRELWithdrawal = new CustomsEntryStatus("WGW", ResString.GetMultilingualString("9372B483-15E0-4265-AEF0-F7129C903F2B", "Awaiting Withdraw DEPREL Response"));
		public static readonly CustomsEntryStatus AwaitingDEPRELReplacement = new CustomsEntryStatus("WGR", ResString.GetMultilingualString("C9318A18-7ED4-447F-A02C-D64DD3788F8B", "Awaiting Replace DEPREL Response"));
		public static readonly CustomsEntryStatus FailDEPRELOriginal = new CustomsEntryStatus("FGO", ResString.GetMultilingualString("A6568083-0FD9-45BB-9886-18023A6F4419", "Original DEPREL Rejected, press Detail for more information"));
		public static readonly CustomsEntryStatus FailDEPRELWithdrawal = new CustomsEntryStatus("FGW", ResString.GetMultilingualString("B6175F3A-086A-4EEE-8D37-6E2E37AED8E4", "Withdraw DEPREL Rejected, press Detail for more information"));
		public static readonly CustomsEntryStatus FailDEPRELReplacement = new CustomsEntryStatus("FGR", ResString.GetMultilingualString("BC9BB9E5-3A30-4569-90D7-F3E5132CAFD2", "Replace DEPREL Rejected, press Detail for more information"));
		public static readonly CustomsEntryStatus ClearDEPRELOriginal = new CustomsEntryStatus("CGO", ResString.GetMultilingualString("8E8CF9DC-A55C-4E71-87BC-5EC9A0B25A90", "DEPREL Clear"));
		public static readonly CustomsEntryStatus ClearDEPRELWithdrawal = new CustomsEntryStatus("CGW", ResString.GetMultilingualString("C38659EC-D6EB-428A-B9C0-219D963753B2", "DEPREL Withdrawn"));
		public static readonly CustomsEntryStatus ClearDEPRELReplacement = new CustomsEntryStatus("CGR", ResString.GetMultilingualString("3D2BF714-F596-4B30-8907-D4B298510E50", "DEPREL Replaced"));
		public static readonly CustomsEntryStatus ErrorDEPRELOriginal = new CustomsEntryStatus("EGO", ResString.GetMultilingualString("C8ABBE6B-11D5-46E6-A246-B6CB64C92A7C", "DEPREL Accepted with errors, press Detail for more information"));
		public static readonly CustomsEntryStatus ErrorDEPRELWithdrawal = new CustomsEntryStatus("EGW", ResString.GetMultilingualString("EEB97F42-2A49-457C-B485-8412EBA7A555", "DEPREL Withdrawn with errors, press Detail for more information"));
		public static readonly CustomsEntryStatus ErrorDEPRELReplacement = new CustomsEntryStatus("EGR", ResString.GetMultilingualString("95C959B0-8C9D-4DF3-A27F-7B4795E67DDA", "DEPREL Replaced with errors, press Detail for more information"));

		// Import
		public static readonly CustomsEntryStatus AwaitingCreate = new CustomsEntryStatus("WTC", ResString.GetMultilingualString("Customs.EntryStatus.AwaitingCreate", "Awaiting Response for Create"));
		public static readonly CustomsEntryStatus AwaitingCPDec = new CustomsEntryStatus("WTD", ResString.GetMultilingualString("Customs.EntryStatus.AwaitingCPDec", "Awaiting Response for CP Dec"));
		public static readonly CustomsEntryStatus AwaitingLodge = new CustomsEntryStatus("WTL", ResString.GetMultilingualString("Customs.EntryStatus.AwaitingLodge", "Awaiting Response for Lodge"));
		public static readonly CustomsEntryStatus AwaitingPay = new CustomsEntryStatus("WTP", ResString.GetMultilingualString("Customs.EntryStatus.AwaitingPay", "Awaiting Response for Pay"));
		public static readonly CustomsEntryStatus FailCreate = new CustomsEntryStatus("FLC", ResString.GetMultilingualString("Customs.EntryStatus.FailCreate", "Create Message Failed"));
		public static readonly CustomsEntryStatus FailCPDec = new CustomsEntryStatus("FLD", ResString.GetMultilingualString("Customs.EntryStatus.FailCPDec", "CP Dec Message Failed"));
		public static readonly CustomsEntryStatus FailLodge = new CustomsEntryStatus("FLL", ResString.GetMultilingualString("Customs.EntryStatus.FailLodge", "Lodge Message Failed"));
		public static readonly CustomsEntryStatus FailPay = new CustomsEntryStatus("FLP", ResString.GetMultilingualString("Customs.EntryStatus.FailPay", "Pay Message Failed"));
		public static readonly CustomsEntryStatus ClearCreate = new CustomsEntryStatus("CLC", ResString.GetMultilingualString("Customs.EntryStatus.ClearCreate", "Create Message Cleared"));
		public static readonly CustomsEntryStatus ClearCPDec = new CustomsEntryStatus("CLD", ResString.GetMultilingualString("Customs.EntryStatus.ClearCPDec", "CP Dec Message Cleared"));
		public static readonly CustomsEntryStatus ClearLodge = new CustomsEntryStatus("CLL", ResString.GetMultilingualString("Customs.EntryStatus.ClearLodge", "Lodge Message Cleared"));
		public static readonly CustomsEntryStatus ClearPay = new CustomsEntryStatus("CLP", ResString.GetMultilingualString("Customs.EntryStatus.ClearPay", "Pay Message Cleared"));
		public static readonly CustomsEntryStatus LodgeImpediment = new CustomsEntryStatus("IMP", ResString.GetMultilingualString("Customs.EntryStatus.LodgeImpediment", "Lodge Impediment"));
		public static readonly CustomsEntryStatus PaymentStopped = new CustomsEntryStatus("PST", ResString.GetMultilingualString("Customs.EntryStatus.PaymentStopped", "Stop Payment"));
		public static readonly CustomsEntryStatus CargoCleared = new CustomsEntryStatus("CCL", ResString.GetMultilingualString("Customs.EntryStatus.CargoCleared", "Cargo Cleared"));
		public static readonly CustomsEntryStatus CargoNotCleared = new CustomsEntryStatus("CNC", ResString.GetMultilingualString("Customs.EntryStatus.CargoNotCleared", "Cargo NOT Cleared"));

		public static readonly CustomsEntryStatus ClearLodgeWithImpediments = new CustomsEntryStatus("CLI", ResString.GetMultilingualString("Customs.EntryStatus.ClearLodgeWithImpediments", "Lodge Message Cleared (with Impediments)"));
		public static readonly CustomsEntryStatus ClearPayWithImpediments = new CustomsEntryStatus("CPI", ResString.GetMultilingualString("Customs.EntryStatus.ClearPayWithImpediments", "Pay Message Cleared (with Impediments)"));

		public static readonly CustomsEntryStatus AmberLine = new CustomsEntryStatus("AMB", ResString.GetMultilingualString("Customs.EntryStatus.AmberLine", "Entry Nominated for Amber Line Processing"));
		public static readonly CustomsEntryStatus CommunityProtectionCheck = new CustomsEntryStatus("COM", ResString.GetMultilingualString("Customs.EntryStatus.CommunityProtectionCheck", "Entry Selected for Community Protection Check"));
		public static readonly CustomsEntryStatus SelectedForCargoExamination = new CustomsEntryStatus("CAR", ResString.GetMultilingualString("Customs.EntryStatus.SelectedForCargoExamination", "Entry Selected for Cargo Examination"));
		public static readonly CustomsEntryStatus RedLine = new CustomsEntryStatus("RED", ResString.GetMultilingualString("Customs.EntryStatus.RedLine", "Entry Selected for Red Line Processing"));

		//Import CMR
		public static readonly CustomsEntryStatus AwaitingPreLodge = new CustomsEntryStatus("WPL", ResString.GetMultilingualString("0EAB635F-35E0-4E74-B178-447343F4565F", "Awaiting Pre-Lodge Response"));
		public static readonly CustomsEntryStatus AwaitingFormalLodge = new CustomsEntryStatus("WFL", ResString.GetMultilingualString("DA4BA158-CEF5-4F31-AA9D-717E0543A79F", "Awaiting Formal Lodge Response"));
		public static readonly CustomsEntryStatus AwaitingPayment = new CustomsEntryStatus("WPY", ResString.GetMultilingualString("8AEBE634-382B-4484-947B-1643FD8F36D4", "Awaiting Payment Response"));
		public static readonly CustomsEntryStatus AwaitingSAC = new CustomsEntryStatus("WSC", ResString.GetMultilingualString("35AA5670-6AA3-4DBE-AA35-FB43D79585D6", "Awaiting SAC Response"));
		public static readonly CustomsEntryStatus AwaitingAmendment = new CustomsEntryStatus("WAM", ResString.GetMultilingualString("6280B22E-E9F3-4018-9220-2B65FAE6A967", "Awaiting Amendment Response"));

		public static readonly CustomsEntryStatus FailPreLodge = new CustomsEntryStatus("FPL", ResString.GetMultilingualString("9EF302AB-65C2-45D8-ACCF-21FF4BAEC5AB", "Failed Pre-Lodge"));
		public static readonly CustomsEntryStatus FailFormalLodge = new CustomsEntryStatus("FFL", ResString.GetMultilingualString("D3E8B27F-34A3-4AB8-874D-E8AB9CA587DF", "Failed Formal Lodge"));
		public static readonly CustomsEntryStatus FailPayment = new CustomsEntryStatus("FPY", ResString.GetMultilingualString("5442AA7D-ED2E-413C-AC46-2A2A5B75A7F0", "Failed Payment"));
		public static readonly CustomsEntryStatus FailSAC = new CustomsEntryStatus("FSC", ResString.GetMultilingualString("B06CC5AD-F3EC-4DC8-9B37-D5EB4C816A5E", "Failed SAC"));
		public static readonly CustomsEntryStatus FailAmendment = new CustomsEntryStatus("FAM", ResString.GetMultilingualString("E1B3F60D-61C5-4D19-A5AC-12C42B60165A", "Failed Amendment of Formal Lodge"));

		public static readonly CustomsEntryStatus ClearPreLodge = new CustomsEntryStatus("CPL", ResString.GetMultilingualString("4ED53FE8-9DDB-4EF8-B463-4AFBCABE4B54", "Cleared Pre - Lodge (Entry Discarded, Formal Lodge Required)"));
		public static readonly CustomsEntryStatus ClearFormalLodge = new CustomsEntryStatus("CFL", ResString.GetMultilingualString("4D70BCA8-E9CD-4453-8C89-5F3B26434BA8", "Cleared Formal Lodge"));
		public static readonly CustomsEntryStatus ClearPayment = new CustomsEntryStatus("CPY", ResString.GetMultilingualString("4401CFED-E6B5-477F-BDD5-13A35CFBCD92", "Payment Response Received (Check the Payment Status for Details)"));
		public static readonly CustomsEntryStatus ClearSAC = new CustomsEntryStatus("CSC", ResString.GetMultilingualString("887366A7-7D6D-475D-B4E5-889216042B2E", "Cleared SAC"));
		public static readonly CustomsEntryStatus ClearAmendment = new CustomsEntryStatus("CAM", ResString.GetMultilingualString("68ED5BA4-6EFA-4AF6-A436-1ED7C6C3A9B6", "Cleared Amendment"));

		// Scheduled Message
		public static readonly CustomsEntryStatus ScheduledLodgeWithPayment = new CustomsEntryStatus("SLP", ResString.GetMultilingualString("10200A31-B28A-4016-AC04-16EFB3B836BC", "Scheduled Lodge with payment"));
		public static readonly CustomsEntryStatus ScheduledLodgeWithoutPayment = new CustomsEntryStatus("SLU", ResString.GetMultilingualString("0DD5F91C-44BF-40FD-9B60-5CAC2AE85F2D", "Scheduled Lodge without payment"));
		public static readonly CustomsEntryStatus ScheduledPayment = new CustomsEntryStatus("SPY", ResString.GetMultilingualString("FCB4E075-7D31-4D29-8FD8-E26DA8C7523F", "Scheduled Payment"));

		public static CustomsEntryStatus MostImportantStatusForEdifice(params CustomsEntryStatus[] statusList)
		{
			return MostImportStatus(ImportStatusInOrderOfImportance, statusList);
		}

		public static CustomsEntryStatus MostImportantStatusForCMR(params CustomsEntryStatus[] statusList)
		{
			return MostImportStatus(CMRImportStatusInOrderOfImportance, statusList);
		}

		#region Implementation

		protected static CustomsEntryStatus MostImportStatus(ImmutableArray<CustomsEntryStatus> importanceList, params CustomsEntryStatus[] statusList)
		{
			int maximumIndex = -1;
			foreach (CustomsEntryStatus status in statusList)
			{
				int indexOfStatus = GetIndexOfImportStatus(status, importanceList);
				if (indexOfStatus != -1 && (maximumIndex == -1 || indexOfStatus > maximumIndex))
				{
					maximumIndex = indexOfStatus;
				}
			}
			return maximumIndex == -1 ? CustomsEntryStatus.NotSent : importanceList[maximumIndex];
		}

		protected static int GetIndexOfImportStatus(CustomsEntryStatus status, ImmutableArray<CustomsEntryStatus> importanceList)
		{
			for (int i = 0; i < importanceList.Length; i++)
			{
				if (status == importanceList[i])
				{
					return i;
				}
			}
			return -1;
		}

		protected static readonly ImmutableArray<CustomsEntryStatus> ImportStatusInOrderOfImportance = new CustomsEntryStatus[]
			{
				CustomsEntryStatus.LodgeImpediment,//this is no used any more and will be removed
				CustomsEntryStatus.PaymentStopped,//this is no used any more and will be removed
				CustomsEntryStatus.NotSent,
				CustomsEntryStatus.ClearCreate,
				CustomsEntryStatus.ClearCPDec,
				CustomsEntryStatus.ClearLodge,
				CustomsEntryStatus.ClearLodgeWithImpediments,

				CustomsEntryStatus.AmberLine,
				CustomsEntryStatus.CommunityProtectionCheck,
				CustomsEntryStatus.SelectedForCargoExamination,
				CustomsEntryStatus.RedLine,

				CustomsEntryStatus.ClearPay,
				CustomsEntryStatus.ClearPayWithImpediments,
				CustomsEntryStatus.CargoCleared,
				CustomsEntryStatus.CargoNotCleared,

				CustomsEntryStatus.FailPay,
				CustomsEntryStatus.FailLodge,
				CustomsEntryStatus.FailCPDec,
				CustomsEntryStatus.FailCreate,

				CustomsEntryStatus.AwaitingCreate,
				CustomsEntryStatus.AwaitingCPDec,
				CustomsEntryStatus.AwaitingLodge,
				CustomsEntryStatus.AwaitingPay,
				CustomsEntryStatus.AwaitingWithdrawal,

				CustomsEntryStatus.DeclarationWorkComplete,
				CustomsEntryStatus.HoldAwaiting }.ToImmutableArray();

		protected static readonly ImmutableArray<CustomsEntryStatus> CMRImportStatusInOrderOfImportance = new CustomsEntryStatus[]
		{
				CustomsEntryStatus.NotSent,
				CustomsEntryStatus.ClearSAC,
				CustomsEntryStatus.ClearPreLodge,
				CustomsEntryStatus.ClearFormalLodge,
				CustomsEntryStatus.ClearPayment,
				CustomsEntryStatus.ClearAmendment,
				CustomsEntryStatus.ClearWithdrawal,

				CustomsEntryStatus.FailSAC,
				CustomsEntryStatus.FailPreLodge,
				CustomsEntryStatus.FailFormalLodge,
				CustomsEntryStatus.FailPayment,
				CustomsEntryStatus.FailAmendment,
				CustomsEntryStatus.FailWithdrawal,

				CustomsEntryStatus.AwaitingSAC,
				CustomsEntryStatus.AwaitingPreLodge,
				CustomsEntryStatus.AwaitingFormalLodge,
				CustomsEntryStatus.AwaitingPayment,
				CustomsEntryStatus.AwaitingAmendment,
				CustomsEntryStatus.AwaitingWithdrawal,
				CustomsEntryStatus.DeclarationWorkComplete,
				CustomsEntryStatus.HoldAwaiting
			}.ToImmutableArray();
		#endregion
	}
}
