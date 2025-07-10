using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU
{
	public class CustomsEntryStatusList : CodeDescriptionPairList
	{
		public CustomsEntryStatusList()
		{
			AddRange(new ExportCustomsEntryStatusList());
			AddRange(new CMR.CMRImportEntryAdviceList());
		}
	}

	public class LegacyCustomsEntryStatusList : CodeDescriptionPairList
	{
		public LegacyCustomsEntryStatusList()
		{
			Add(CustomsEntryStatus.NotSent);
			Add(CustomsEntryStatus.HoldAwaiting);
			Add(CustomsEntryStatus.DeclarationWorkComplete);
			Add(CustomsEntryStatus.AwaitingOriginal);
			Add(CustomsEntryStatus.AwaitingWithdrawal);
			Add(CustomsEntryStatus.AwaitingReplacement);
			Add(CustomsEntryStatus.ErrorOriginal);
			Add(CustomsEntryStatus.ErrorWithdrawal);
			Add(CustomsEntryStatus.ErrorReplacement);
			Add(CustomsEntryStatus.FailOriginal);
			Add(CustomsEntryStatus.FailWithdrawal);
			Add(CustomsEntryStatus.FailReplacement);
			Add(CustomsEntryStatus.ClearOriginal);
			Add(CustomsEntryStatus.ClearWithdrawal);
			Add(CustomsEntryStatus.ClearReplacement);
			Add(CustomsEntryStatus.Embargoed);
			Add(CustomsEntryStatus.ReleasedFromEmbargo);
			Add(CustomsEntryStatus.AwaitingCreate);
			Add(CustomsEntryStatus.AwaitingCPDec);
			Add(CustomsEntryStatus.AwaitingPay);
			Add(CustomsEntryStatus.AwaitingLodge);
			Add(CustomsEntryStatus.FailCreate);
			Add(CustomsEntryStatus.FailCPDec);
			Add(CustomsEntryStatus.FailLodge);
			Add(CustomsEntryStatus.FailPay);
			Add(CustomsEntryStatus.ClearCreate);
			Add(CustomsEntryStatus.ClearCPDec);
			Add(CustomsEntryStatus.ClearLodge);
			Add(CustomsEntryStatus.ClearPay);
			Add(CustomsEntryStatus.LodgeImpediment);
			Add(CustomsEntryStatus.PaymentStopped);
			Add(CustomsEntryStatus.CargoCleared);
			Add(CustomsEntryStatus.CargoNotCleared);
			Add(CustomsEntryStatus.RedLine);
			Add(CustomsEntryStatus.AmberLine);
			Add(CustomsEntryStatus.CommunityProtectionCheck);
			Add(CustomsEntryStatus.SelectedForCargoExamination);
			Add(CustomsEntryStatus.ClearPayWithImpediments);
			Add(CustomsEntryStatus.ClearLodgeWithImpediments);
			Add(CustomsEntryStatus.Lodged);
			Add(CustomsEntryStatus.Cancelled);
			Add(CustomsEntryStatus.Transferred);
		}
	}

	public class EdificeCustomsEntryStatusList : CodeDescriptionPairList
	{
		public EdificeCustomsEntryStatusList()
		{
			Add(CustomsEntryStatus.NotSent);
			Add(CustomsEntryStatus.AwaitingCreate);
			Add(CustomsEntryStatus.AwaitingCPDec);
			Add(CustomsEntryStatus.AwaitingPay);
			Add(CustomsEntryStatus.AwaitingLodge);
			Add(CustomsEntryStatus.ClearCreate);
			Add(CustomsEntryStatus.ClearCPDec);
			Add(CustomsEntryStatus.ClearLodge);
			Add(CustomsEntryStatus.ClearPay);
			Add(CustomsEntryStatus.FailCreate);
			Add(CustomsEntryStatus.FailCPDec);
			Add(CustomsEntryStatus.FailLodge);
			Add(CustomsEntryStatus.FailPay);
			Add(CustomsEntryStatus.HoldAwaiting);
			Add(CustomsEntryStatus.DeclarationWorkComplete);
			Add(CustomsEntryStatus.LodgeImpediment);
			Add(CustomsEntryStatus.PaymentStopped);
			Add(CustomsEntryStatus.CargoCleared);
			Add(CustomsEntryStatus.CargoNotCleared);
			Add(CustomsEntryStatus.RedLine);
			Add(CustomsEntryStatus.AmberLine);
			Add(CustomsEntryStatus.CommunityProtectionCheck);
			Add(CustomsEntryStatus.SelectedForCargoExamination);
			Add(CustomsEntryStatus.ClearPayWithImpediments);
			Add(CustomsEntryStatus.ClearLodgeWithImpediments);
			Add(CustomsEntryStatus.AwaitingWithdrawal);
			Add(CustomsEntryStatus.ScheduledLodgeWithPayment);
			Add(CustomsEntryStatus.ScheduledLodgeWithoutPayment);
			Add(CustomsEntryStatus.ScheduledPayment);
		}
	}

	public class ExportCustomsEntryStatusList : CodeDescriptionPairList
	{
		public ExportCustomsEntryStatusList()
		{
			Add(CustomsEntryStatus.NotSent);
			Add(CustomsEntryStatus.AwaitingOriginal);
			Add(CustomsEntryStatus.AwaitingWithdrawal);
			Add(CustomsEntryStatus.AwaitingReplacement);
			Add(CustomsEntryStatus.ErrorOriginal);
			Add(CustomsEntryStatus.ErrorWithdrawal);
			Add(CustomsEntryStatus.ErrorReplacement);
			Add(CustomsEntryStatus.FailOriginal);
			Add(CustomsEntryStatus.FailWithdrawal);
			Add(CustomsEntryStatus.FailReplacement);
			Add(CustomsEntryStatus.ClearReplacement);
			Add(CustomsEntryStatus.ClearOriginal);
			Add(CustomsEntryStatus.ClearWithdrawal);
		}
	}
}

