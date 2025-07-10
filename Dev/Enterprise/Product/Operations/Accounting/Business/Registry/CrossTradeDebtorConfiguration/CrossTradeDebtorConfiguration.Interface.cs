using System;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public partial class CrossTradeDebtorConfiguration : ICrossTradeDebtorDefaultingConfigurationItem
	{
		string ICrossTradeDebtorDefaultingConfigurationItem.JobTypeCode => JobType;

		string ICrossTradeDebtorDefaultingConfigurationItem.TransportModeCode => Mode;

		bool ICrossTradeDebtorDefaultingConfigurationItem.IsCollect => ChargePaymentType == PrepaidCollectFreightForwardingList.Codes.CCX || ChargePaymentType == "ALL";

		bool ICrossTradeDebtorDefaultingConfigurationItem.IsPrepaid => ChargePaymentType == PrepaidCollectFreightForwardingList.Codes.PPD || ChargePaymentType == "ALL";

		ChargedPartyForCrossTradeJob ICrossTradeDebtorDefaultingConfigurationItem.BillToParty
		{
			get
			{
				switch (Debtor)
				{
					case DefaultDebtorList.Codes.PrepaidBillToParty:
						return ChargedPartyForCrossTradeJob.LocalClient;
					case DefaultDebtorList.Codes.CollectBillToParty:
						return ChargedPartyForCrossTradeJob.Agent;
					case DefaultDebtorList.Codes.ControlCustomerFallToPrepaid:
						return ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToLocalClient;
					case DefaultDebtorList.Codes.ControlCustomerFallToCollect:
						return ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToAgent;
					default:
						throw new NotImplementedException(FormattableString.Invariant($"No Mapping to ChargeParty for {Debtor}"));
				}
			}
		}
	}
}
