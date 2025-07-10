using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public class ProfitShareConsolChargeCreator : ProfitShareChargeCreator
	{
		public ProfitShareConsolChargeCreator(AgentChargePostingDetails agentPostingDetails, BusinessObjectFactory factory)
		{
			this.PostingDetails = agentPostingDetails;
			this.Factory = factory;
		}

		readonly BusinessObjectFactory Factory;
		readonly AgentChargePostingDetails PostingDetails;

		protected override bool CreateChargesCore()
		{
			if (AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.Value)
			{
				return CreateProfitShareAsAR();
			}
			else
			{
				return CreateProfitShareAsAP();
			}
		}

		protected override BusinessObject ParentObject
		{
			get { return PostingDetails != null ? PostingDetails.Consol as BusinessObject : null; }
		}

		void UpdateJobStatus(ProfitShareCharge profitShareCharge, ProfitShareShipmentDetail shipmentDetail)
		{
			if (profitShareCharge.ProfitShare != 0m)
			{
				shipmentDetail.Job.JH_IsProfitSharePosted = true;

				if (AccountingConfigurationRegistry.Instance.AutoCompleteJobOnProfitShareCalculation.Value)
				{
					shipmentDetail.Job.JH_Status = JobHeaderStatus.Complete.Code;
				}
			}
		}

		#region AR

		protected bool CreateProfitShareAsAR()
		{
			bool result = false;

			foreach (ProfitShareDetail profitShare in PostingDetails.CalculatedProfitShares)
			{
				foreach (ProfitShareShipmentDetail shipmentDetail in profitShare.ProfitShareShipmentDetails)
				{
					foreach (ProfitShareCharge profitShareCharge in shipmentDetail.ProfitShareCharges)
					{
						Charge shipmentCharge = shipmentDetail.Job.Charges.AddNew();
						shipmentCharge.JR_OH_SellAccount = profitShare.ProfitShareParty.PK;
						// Nothing to do with Gateway, we should use shipmentDetail.PartyType to get charge code
						// Test: TestCreateCharges_AR_GatewayConsol_ShouldNotPickGatewayAgentChargeCodeFromRegistry
						shipmentCharge.JR_AC = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value.GetCode(shipmentDetail.PartyType);
						SetCurrencyAndExchangeRatesOnCharge(profitShare, shipmentCharge, profitShareCharge);
						UpdateJobStatus(profitShareCharge, shipmentDetail);
						result = true;
					}
				}
			}

			return result;
		}

		void SetCurrencyAndExchangeRatesOnCharge(ProfitShareDetail profitShare, Charge charge, ProfitShareCharge profitShareCharge)
		{
			if (AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.Value)
			{
				charge.JR_RX_NKSellCurrency = profitShareCharge.Currency.RX_Code;
				charge.JR_OSSellAmt = AccountingUtils.Round(profitShareCharge.ProfitShare ?? 0m, profitShareCharge.Currency) * -1;
			}
			else // local currency only
			{
				ZDecimal exchangeRate = 0;
				RefCurrency currency = null;
				if (PostingDetails.Consol.ReceivingAgentAPInvoicingParty != null && charge.JR_OH_SellAccount == PostingDetails.Consol.ReceivingAgentAPInvoicingParty.PK)
				{
					exchangeRate = PostingDetails.AgentInvoicePostingExchangeRate;
					currency = PostingDetails.AgentInvoicePostingCurrency;
				}
				else
				{
					InvoicingBase agentInvoiceBeingPosted = GetDeliveryAgentInvoiceBeingPosted(profitShare.ProfitShareParty);
					if (agentInvoiceBeingPosted != null)
					{
						exchangeRate = agentInvoiceBeingPosted.AH_ExchangeRate;
						currency = agentInvoiceBeingPosted.TransactionCurrency;
					}
				}

				if (currency != null)
				{
					var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(charge.Company, ExchangeRateValidLedgerEnum.AR, charge);
					charge.InvoicingJob.AddCurrency(currency, exchangeRate, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType);
					charge.JR_RX_NKSellCurrency = currency.RX_Code;
				}

				charge.JR_OSSellAmt = Env.CurrentCompany.ExchangeRate.LocalToForeign(profitShareCharge.ProfitShare ?? 0m, exchangeRate, charge.SellCurrency.RX_Code) * -1;
				charge.JR_LocalSellAmt = AccountingUtils.Round(profitShareCharge.ProfitShare ?? 0m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency) * -1;
			}

			if (charge.BillInInvoiceCurrency)   // We should clear Sell Invoice Currency if it was defaulted
			{
				charge.JR_RX_NKSellInvoiceCurrency = ZString.Empty;
			}
		}

		#endregion

		#region AP

		bool CreateProfitShareAsAP()
		{
			bool result = false;

			List<JobConsolCost> createdConsolCosts = new List<JobConsolCost>();
			foreach (ProfitShareDetail profitShare in PostingDetails.CalculatedProfitShares)
			{
				Dictionary<ProfitShareConsolCostKey, ZDecimal> uniqueCurrenciesAndTotals = GetUniqueProfitShareCosts(profitShare);
				foreach (KeyValuePair<ProfitShareConsolCostKey, ZDecimal> currencyTotal in uniqueCurrenciesAndTotals)
				{
					JobConsolCost pSCost = PostingDetails.AllConsolCosts.TryAddNew();
					if (pSCost != null)
					{
						createdConsolCosts.Add(pSCost);
						SetDetailsOnCost(profitShare, pSCost, currencyTotal);
						UpdateConsolCostCharges(profitShare, currencyTotal, pSCost);
						result = true;
					}
				}
			}

			return result;
		}

		void UpdateConsolCostCharges(ProfitShareDetail profitShare, KeyValuePair<ProfitShareConsolCostKey, ZDecimal> currencyTotal, JobConsolCost pSCost)
		{
			foreach (ProfitShareShipmentDetail shipmentDetail in profitShare.ProfitShareShipmentDetails)
			{
				foreach (ProfitShareCharge profitShareCharge in shipmentDetail.ProfitShareCharges)
				{
					if (profitShareCharge.CurrencyCode == currencyTotal.Key.Currency.RX_Code)
					{
						var charge1 = pSCost.ApportionmentCharges.FindChargeForJob(shipmentDetail.Parent);

						if (AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.Value)
						{
							charge1.JR_OSCostAmt = AccountingUtils.Round(profitShareCharge.ProfitShare ?? 0, profitShareCharge.Currency);
						}
						else
						{
							charge1.JR_OSCostAmt = Env.CurrentCompany.ExchangeRate.LocalToForeign(profitShareCharge.ProfitShare ?? 0, pSCost.E6_ExchangeRate, pSCost.E6_RX_NKCurrency);
							charge1.JR_LocalCostAmt = AccountingUtils.Round(profitShareCharge.ProfitShare ?? 0m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
						}

						charge1.JR_IsIncludedInProfitShare = false;
						UpdateJobStatus(profitShareCharge, shipmentDetail);
					}
				}
			}

			foreach (ApportionSplitCharge charge in pSCost.ApportionmentCharges)
			{
				if (charge.Job == null || !profitShare.ProfitShareShipmentDetails.ContainsShipmentAndProfitShareCharges(charge.Job.JH_ParentID))
				{
					charge.JR_OSCostAmt = ZDecimal.Zero;
				}
			}

			pSCost.RemoveNonApplicableCharges();
			pSCost.PushUnApportionedAmountBasedOnRepresentation(pSCost.E6_OSCostAmount, JobChargeSchema.JR_OSCostAmt);
			pSCost.PushLocalAmountExchangeRateDifferencesToGreatestCharge();
			pSCost.PushUnApportionedGSTAmountBasedOnRepresentation();
			pSCost.PrepareForPosting();
		}

		Dictionary<ProfitShareConsolCostKey, ZDecimal> GetUniqueProfitShareCosts(ProfitShareDetail detail)
		{
			Dictionary<ProfitShareConsolCostKey, ZDecimal> result = new Dictionary<ProfitShareConsolCostKey, ZDecimal>();
			foreach (ProfitShareShipmentDetail shipDetail in detail.ProfitShareShipmentDetails)
			{
				foreach (ProfitShareCharge charge in shipDetail.ProfitShareCharges)
				{
					ProfitShareConsolCostKey key = new ProfitShareConsolCostKey(charge.Currency, shipDetail.PartyType);
					ZDecimal currentAmount = 0m;
					if (result.TryGetValue(key, out currentAmount))
					{
						result[key] += charge.ProfitShare ?? ZDecimal.Zero;
					}
					else
					{
						result.Add(key, charge.ProfitShare ?? ZDecimal.Zero);
					}
				}
			}

			return result;
		}

		struct ProfitShareConsolCostKey
		{
			public ProfitShareConsolCostKey(RefCurrency currency, string partyType)
			{
				this.Currency = currency;
				this.PartyType = partyType;
			}

			public readonly RefCurrency Currency;
			public readonly string PartyType;

			public override bool Equals(object obj)
			{
				ProfitShareConsolCostKey key = (ProfitShareConsolCostKey)obj;
				return key.Currency == this.Currency && key.PartyType == this.PartyType;
			}

			public override int GetHashCode()
			{
				return Currency.GetHashCode() ^ PartyType.GetHashCode();
			}
		}

		void SetDetailsOnCost(ProfitShareDetail profitShare, JobConsolCost pSCost, KeyValuePair<ProfitShareConsolCostKey, ZDecimal> profitShareCostKey)
		{
			CreateApportionmentChargesOnCost(pSCost);
			pSCost.OverrideIsCreditorOverseasAgentCheck(true);
			pSCost.E6_AC_ChargeCode = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value.GetCode(profitShareCostKey.Key.PartyType);
			pSCost.E6_OH_Creditor = profitShare.ProfitShareParty.PK;
			pSCost.E6_IsForCollectInvoice = true;
			pSCost.E6_InvoiceDate = ZDateTime.Now;
			ZString invoiceNum = InvoiceLiteralNumberGenerator.GetNextProfitShareAPInvoiceNumberForConsol(Factory, PostingDetails.Consol.JK_UniqueConsignRef);
			if (AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.Value)
			{
				invoiceNum += "-" + profitShareCostKey.Key.Currency.RX_Code;
			}
			pSCost.E6_InvoiceNum = invoiceNum.SubstringSafe(0, JobConsolCostSchema.E6_InvoiceNum.MaxLength);
			SetCurrencyAndExchangeRatesOnCost(profitShare, pSCost, profitShareCostKey);

			foreach (ProfitShareShipmentDetail shipmentDetail in profitShare.ProfitShareShipmentDetails)
			{
				if (shipmentDetail.ProfitSharePartyDetails != null && shipmentDetail.ProfitSharePartyDetails.PS_PartyRateBasis == OrgProfitSharePartyLookups.FeeBasisCodes.PerContainer)
				{
					pSCost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.ContainerCount;
					break;
				}
			}
		}

		void SetCurrencyAndExchangeRatesOnCost(ProfitShareDetail profitShare, JobConsolCost pSCost, KeyValuePair<ProfitShareConsolCostKey, ZDecimal> profitShareCostKey)
		{
			if (AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.Value)
			{
				pSCost.E6_RX_NKCurrency = profitShareCostKey.Key.Currency.RX_Code;
				if (pSCost.E6_RX_NKCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					pSCost.E6_ExchangeRate = 1;
				}
				else
				{
					pSCost.E6_ExchangeRate = GetImplicitExchangeRateFromChargesContributingToPS(profitShare, profitShareCostKey);
				}
				pSCost.E6_OSCostAmount = AccountingUtils.Round(profitShareCostKey.Value, profitShareCostKey.Key.Currency);
				pSCost.E6_LocalCostAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(pSCost.E6_OSCostAmount, pSCost.E6_ExchangeRate);
			}
			else
			{
				if (PostingDetails.Consol.ReceivingAgentAPInvoicingParty != null && pSCost.E6_OH_Creditor == PostingDetails.Consol.ReceivingAgentAPInvoicingParty.PK)
				{
					pSCost.E6_RX_NKCurrency = PostingDetails.AgentInvoicePostingCurrency.RX_Code;
					pSCost.E6_ExchangeRate = PostingDetails.AgentInvoicePostingExchangeRate;
				}
				else
				{
					InvoicingBase agentInvoiceBeingPosted = GetDeliveryAgentInvoiceBeingPosted(profitShare.ProfitShareParty);
					if (agentInvoiceBeingPosted != null)
					{
						pSCost.E6_RX_NKCurrency = agentInvoiceBeingPosted.AH_RX_NKTransactionCurrency;
						pSCost.E6_ExchangeRate = agentInvoiceBeingPosted.AH_ExchangeRate;
					}
				}

				pSCost.E6_OSCostAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(profitShareCostKey.Value, pSCost.E6_ExchangeRate, pSCost.E6_RX_NKCurrency);
				pSCost.E6_LocalCostAmount = AccountingUtils.Round(profitShareCostKey.Value, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			}
		}

		ZDecimal GetImplicitExchangeRateFromChargesContributingToPS(ProfitShareDetail profitShare, KeyValuePair<ProfitShareConsolCostKey, ZDecimal> profitShareCostKey)
		{
			ZDecimal result = 0;
			decimal oSPSContributions = 0;
			decimal localPSContributions = 0;
			foreach (ProfitShareShipmentDetail shipmentDetail in profitShare.ProfitShareShipmentDetails)
			{
				foreach (Charge pSContributor in shipmentDetail.ChargesIncludedInProfitShare)
				{
					if (pSContributor.JR_RX_NKCostCurrency == profitShareCostKey.Key.Currency.RX_Code)
					{
						oSPSContributions -= pSContributor.JR_AgentDeclaredCostAmt;
						localPSContributions -= pSContributor.JR_AgentDeclaredCostAmtLocal;
					}
					if (pSContributor.JR_RX_NKSellCurrency == profitShareCostKey.Key.Currency.RX_Code)
					{
						oSPSContributions += pSContributor.JR_AgentDeclaredSellAmt;
						localPSContributions += pSContributor.JR_AgentDeclaredSellAmtLocal;
					}
				}
			}
			result = Env.CurrentCompany.ExchangeRate.GetRate(localPSContributions, oSPSContributions);
			return result;
		}

		void CreateApportionmentChargesOnCost(JobConsolCost cost)
		{
			if (cost.Consol != null)
			{
				foreach (IJobInvoicingPlugIn shipment in cost.Consol.CostSupporter.ShipmentsList)
				{
					Job shipmentJob = cost.GetJob(shipment);
					if (shipmentJob != null)
					{
						shipmentJob.Charges.Load();
						if (!cost.ApportionmentCharges.ContainsChargeForJob(shipment))
						{
							ApportionSplitCharge newCharge = cost.ApportionmentCharges.AddNew();
							using (newCharge.SuspendSettingHasChanges())
							{
								newCharge.JR_JH = shipmentJob.PK;
								newCharge.JR_GB = shipmentJob.JH_GB;
								newCharge.JR_GE = shipmentJob.JH_GE;
							}
						}
					}
				}
				cost.UpdateShipmentInfosOnCharges();
			}
		}

		InvoicingBase GetDeliveryAgentInvoiceBeingPosted(OrgHeader org)
		{
			InvoicingBase result = null;
			InvoicingBase[] candidateInvoices = PostingDetails.ChargePoster.GetInvoices(org);

			foreach (InvoicingBase invoice in candidateInvoices)
			{
				if (invoice.AH_RX_NKTransactionCurrency == Core.Constants.CurrencyCodes.UnitedStates)
				{
					result = invoice;
					break;
				}
			}

			if (result == null)
			{
				foreach (InvoicingBase invoice in candidateInvoices)
				{
					if (invoice.AH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						result = invoice;
						break;
					}
				}
			}

			if (result == null)
			{
				foreach (InvoicingBase invoice in candidateInvoices)
				{
					if (invoice.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						result = invoice;
						break;
					}
				}
			}

			return result;
		}
		
		#endregion
	}
}
