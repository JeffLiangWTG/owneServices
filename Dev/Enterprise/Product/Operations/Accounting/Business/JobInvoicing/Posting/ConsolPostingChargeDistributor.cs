using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class ConsolPostingChargeDistributor : PostingChargeDistributor
	{
		public ConsolPostingChargeDistributor(IJobCostingPlugIn consol)
		{
			this.Consol = consol;
		}

		readonly IJobCostingPlugIn Consol;

		protected override void SetKeySellCurrencyForForeignCurrencyInvoices(IReceivablesPostingCharge charge, PostingChargeKey key)
		{
			if (ShouldApplyCurrencyKeyFilter(charge))
			{
				base.SetKeySellCurrencyForForeignCurrencyInvoices(charge, key);
			}
		}

		protected override void SetKeySellCurrencyForBillInInvoiceCurrency(IReceivablesPostingCharge charge, PostingChargeKey key)
		{
			if (ShouldApplyCurrencyKeyFilter(charge))
			{
				base.SetKeySellCurrencyForBillInInvoiceCurrency(charge, key);
			}
		}

		bool ShouldApplyCurrencyKeyFilter(IReceivablesPostingCharge charge) => !IsOverseasAgentCharge(charge.Debtor?.PK);

		public bool IsOverseasAgentCharge(ZGuid? debtorPK)
		{
			return debtorPK != null && Consol.AgentToInvoice() != null && debtorPK == Consol.AgentToInvoice().PK;
		}

		public bool IsFirstChargeOverseasAgentCharge(IReceivablesPostingChargeCollection charges)
		{
			return IsOverseasAgentCharge(charges.DebtorBizO?.PK);
		}

		public bool IsCombinedShipmentCharges(IReceivablesPostingCharge charge)
		{
			return CombinedShipmentChargesForBuyersConsol(charge) ||
				(IsOverseasAgentCharge(charge.Debtor?.PK) && charge.Debtor.MiscServ.OM_FWBillCollectFeesOnSingleInvoice);
		}

		protected override PostingChargeKey CreateBasicKey(IReceivablesPostingCharge charge)
		{
			PostingChargeKey result = null;

			if (IsCombinedShipmentCharges(charge))
			{
				result = new PostingChargeKey(charge.Debtor.PK, "", Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, charge.TaxRatePostingGroupId, placeOfSupply: charge.SellPlaceOfSupply, taxBranch: charge.SellTaxBranch);
			}
			else if (ShouldApplyCurrencyKeyFilter(charge) && Consol.IsAgentCharge(charge as Charge) && charge.Debtor.MiscServ.OM_FWBillCollectFeesOnSingleInvoice)
			{
				result = new PostingChargeKey(charge.Debtor.PK, charge.InvoiceType, Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, charge.TaxRatePostingGroupId, placeOfSupply: charge.SellPlaceOfSupply, taxBranch: charge.SellTaxBranch);
			}
			else
			{
				result = new PostingChargeKey(charge.Debtor.PK, charge.InvoiceType, charge.Job.JobNumber, charge.DebtorAddressPK, charge.DebtorContactPK, charge.TaxRatePostingGroupId, placeOfSupply: charge.SellPlaceOfSupply, taxBranch: charge.SellTaxBranch);
				result.SellReference = charge.SellReference;
			}

#if DEBUG
			ConsolPostingChargeDistributor_TestHelper.CheckCreatedBasicKey(result);
#endif

			return result;
		}

		#region Implementation

		bool CombinedShipmentChargesForBuyersConsol(IReceivablesPostingCharge charge)
		{
			bool result = false;

			bool consolIsBCN = Consol is ForwardingConsol && Consol.CostSupporter != null && Consol.CostSupporter.IsBuyersConsol;

			if (consolIsBCN)
			{
				bool chargeHasSameDebtorAsLeadBCNShipmentLocalClient = false;
				OrgHeader leadBCNShipmentLocalClient = null;

				foreach (IJobInvoicingPlugIn shipment in Consol.CostSupporter.ShipmentsList)
				{
					if (shipment is ForwardingShipment)
					{
						ForwardingShipment forwardShipment = ((ForwardingShipment)shipment);

						if (forwardShipment.IsBuyersConsolLead &&
							forwardShipment.Job != null &&
							forwardShipment.Job.LocalCharges != null &&
							forwardShipment.Job.LocalCharges.CompanyData != null &&
							forwardShipment.Job.LocalCharges.CompanyData.EffectiveBuyersConsolInvoicingStyle == Constants.ConsolInvoicingStyles.ApportionInvoiceMaster)
						{
							leadBCNShipmentLocalClient = forwardShipment.Job.LocalCharges;
							break;
						}
					}
				}

				if (leadBCNShipmentLocalClient != null)
				{
					chargeHasSameDebtorAsLeadBCNShipmentLocalClient = leadBCNShipmentLocalClient.PK == charge.Debtor.PK;
				}

				result = consolIsBCN && chargeHasSameDebtorAsLeadBCNShipmentLocalClient;
			}

			return result;
		}

		#endregion
	}
}
