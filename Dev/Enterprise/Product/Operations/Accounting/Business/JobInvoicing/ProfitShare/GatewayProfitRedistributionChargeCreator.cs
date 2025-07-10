using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	class GatewayProfitRedistributionChargeCreator : ProfitShareChargeCreator, IDisposable
	{
		public GatewayProfitRedistributionChargeCreator(BusinessObjectFactory factory, ForwardingConsol consol, ProfitShareDetailCollection calculatedProfitShares, string shipmentJobNumber)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(calculatedProfitShares, nameof(calculatedProfitShares));
			Argument.NotNullOrEmpty(shipmentJobNumber, nameof(shipmentJobNumber));

			this.calculatedProfitShares = calculatedProfitShares;
			this.shipmentJobNumber = shipmentJobNumber;

			if (consol != null)
			{
				this.consolJob = new Job.Loader(factory, consol).TryLoadOrCreateWithMutex();
			}
		}

		readonly ProfitShareDetailCollection calculatedProfitShares;
		readonly string shipmentJobNumber;
		Job consolJob;

		protected override bool CreateChargesCore()
		{
			return CreateProfitShareAsAR();
		}

		bool CreateProfitShareAsAR()
		{
			bool result = false;

			if (consolJob != null)
			{
				foreach (ProfitShareDetail profitShare in calculatedProfitShares)
				{
					foreach (ProfitShareShipmentDetail shipmentDetail in profitShare.ProfitShareShipmentDetails)
					{
						foreach (ProfitShareCharge profitShareCharge in shipmentDetail.ProfitShareCharges)
						{
							Charge charge = consolJob.Charges.AddNew();
							CreatedChargesCount++;
							charge.JR_AC = LeadGatewayAgentPK;
							charge.JR_Calc_RelatedJobNumber = shipmentDetail.Job.JH_JobNum;

							//We should assign local sell amount instead assigning JR_OSSellAmt and JR_RX_NKSellCurrency
							//Reason:-
							//1. If we assign JR_OSSellAmt and based on accounting logic, if there is different currency involved
							//	then system will try to convert the value in that currency and accordingly modify LocalSell Amt, so there could be rounding error
							//2. Apart from rounding error, we have another challenge when we assign JR_AC, system try and assign value to JR_RX_NKSellCurrency
							//	so if we assign values in wrong order(JR_OSSellAmt and then JR_RX_NKSellCurrency), system will incorrectly convert it
							//  so if in the future, if we ever modify this, make sure we assign JR_RX_NKSellCurrency first and then JR_OSSellAmt
							charge.JR_LocalSellAmt = (profitShareCharge.ProfitShare ?? 0m) * -1;

							// Important: Do not update `SellAccount` before setting the internal job.
							// At this point, `SellAccount` should be empty to prevent unexpected updates by AutoJRJ.
							// See `ChargeInternalFieldsManager.SetCostAndSellAccountFromInternalBranch`.

							// Only run Job Revenue Journal (JRJ) for charges that have a sell account with the same company as the gateway company.
							var profitShareParty = profitShare.ProfitShareParty;
							if (profitShareParty != null)
							{
								if (AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() && profitShareParty.IsProxyOrg(charge.CalculatedCompany))
								{
									charge.JR_JH_InternalJob = shipmentDetail.Job.PK;
								}

								// Since we already set the internal job, we can suspend the default values for auto job revenue journals which are the internal job
								// to avoid extra settings when setting the sell account.
								using (charge.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
								{
									if (profitShareParty.OH_IsDebtor)
									{
										charge.JR_OH_SellAccount = profitShare.ProfitShareParty.PK;
									}
								}
							}

							result = true;
						}
					}
				}
			}

			return result;
		}

		protected override Guid ProfitShareChargeCodePK => LeadGatewayAgentPK;

		Guid LeadGatewayAgentPK
		{
			get
			{
				if (!leadGatewayAgentPK.HasValue)
				{
					var registryPK = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value.GetCode(OrgProfitSharePartyLookups.PartyTypeCodes.LeadGatewayAgent);
					leadGatewayAgentPK = registryPK.IsEmpty ? Guid.Empty : registryPK.ToGuid();
				}
				return leadGatewayAgentPK.Value;
			}
		}
		Guid? leadGatewayAgentPK;

		protected override BusinessObject ParentObject => consolJob;

		protected override void AddLogToParent(bool chargesUpdatedOrCreated, int createdChargesCount)
		{
			if (chargesUpdatedOrCreated && consolJob != null)
			{
				var shortLog = string.Format((NoResString)"Gateway Profit share charges created({0}): {1}", shipmentJobNumber, createdChargesCount);
				consolJob.GetLogs().AddNew(Events.ProfitOfGWConsolsRedistributed, shortLog);  //to be added
			}
		}

		public void Dispose()
		{
			if (consolJob != null)
			{
				consolJob.Dispose();
				consolJob = null;
			}
		}
	}
}
