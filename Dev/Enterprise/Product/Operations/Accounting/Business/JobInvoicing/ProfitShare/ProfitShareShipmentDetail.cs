using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public class ProfitShareShipmentDetail : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ProfitShareShipmentDetail(IJobInvoicingPlugIn plugin, OrgHeader profitShareParty, string partyType, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Parent = plugin;
			this.ProfitShareParty = profitShareParty;
			this.PartyType = partyType;
		}

		public ProfitShareShipmentDetail()
		{
			// This empty constructor is needed for document wrapper functionality
		}

		public readonly string PartyType;
		public readonly IJobInvoicingPlugIn Parent;

		#region Organisation to Credit

		/// <summary>
		/// The organisation that should receive the profit share credit.
		/// </summary>
		public readonly OrgHeader ProfitShareParty;

		#endregion

		#region Job / Charges

		/// <summary>
		/// The Job Invoicing Job that is related to this shipment
		/// </summary>
		public virtual Job Job
		{
			get
			{
				if (fJob == null && Parent != null)
				{
					fJob = new Job.Loader(Factory, Parent).Load();
				}

				return fJob;
			}
		}

		Job fJob;

#if DEBUG
		public void SetJobForTesting(Job jobForTest)
		{
			fJob = jobForTest;
		}
#endif

		/// <summary>
		/// These charges were included in the profit share calculation.
		/// </summary>
		public JobCharge[] ChargesIncludedInProfitShare
		{
			get
			{
				if (Job == null)
				{
					return System.Array.Empty<JobCharge>();
				}

				ZQuery profitShareFilter = new ZQuery();
				profitShareFilter.AddToFilter(JobChargeSchema.JR_JH, SQLComparisonOperator.Equal, Job.PK);
				profitShareFilter.AddToFilter(JobChargeSchema.JR_IsIncludedInProfitShare, ZBool.True);

				foreach (ZGuid profitShareChargeCode in GetAllProfitShareChargeCodePerParty())
				{
					profitShareFilter.AddToFilter(JobChargeSchema.JR_AC, SQLComparisonOperator.NotEqual, profitShareChargeCode);
				}

				profitShareFilter.AddToFilter(JobChargeSchema.JR_AC, SQLComparisonOperator.NotEqual, AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.Value);
				profitShareFilter.AddToFilter(JoinCondition.And, JobChargeSchema.JR_AC, SQLComparisonOperator.NotEqual, ZGuid.Empty);

				return Job.Charges.Find(profitShareFilter).Cast<JobCharge>().ToArray();
			}
		}

		ZGuid[] GetAllProfitShareChargeCodePerParty()
		{
			List<ZGuid> result = new List<ZGuid>();
			foreach (FieldInfo partyTypeField in typeof(OrgProfitSharePartyLookups.PartyTypeCodes).GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				string partyType = (string)partyTypeField.GetValue(null);
				ZGuid chargeCodePK = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value.GetCode(partyType);
				if (!result.Contains(chargeCodePK))
				{
					result.Add(chargeCodePK);
				}
			}

			return result.ToArray();
		}

		public virtual List<ProfitShareCharge> ProfitShareCharges
		{
			get
			{
				if (ProfitShareAgreement == null)
				{
					return new List<ProfitShareCharge>();
				}

				var profitShareCharges = new List<ProfitShareCharge>();

				if (AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.Value && !IsForPrinting)
				{
					foreach (var currency in UniqueCurrenciesOnCharges)
					{
						var calculatedProfit = CalculateProfit(currency);
						var calculatedGrossRevenue = CalculateGrossRevenue(currency);
						var (calculatedProfitShare, description) = CalculateProfitToInvoice(calculatedProfit, calculatedGrossRevenue);
						if (calculatedProfitShare != null)
						{
							calculatedProfitShare = AccountingUtils.Round(calculatedProfitShare.Value, currency);
						}
						profitShareCharges.Add(new ProfitShareCharge(this, currency, calculatedProfit, calculatedGrossRevenue, calculatedProfitShare, description));
					}
				}
				else if (ChargesIncludedInProfitShare.Any())
				{
					var calculatedProfit = CalculateProfit(null);
					var calculatedGrossRevenue = CalculateGrossRevenue(null);
					var (calculatedProfitShare, description) = CalculateProfitToInvoice(calculatedProfit, calculatedGrossRevenue);
					if (calculatedProfitShare != null)
					{
						calculatedProfitShare = AccountingUtils.Round(calculatedProfitShare.Value, GlbCompany.CurrentCompany.LocalCurrency);
					}
					profitShareCharges.Add(new ProfitShareCharge(this, GlbCompany.CurrentCompany.LocalCurrency, calculatedProfit, calculatedGrossRevenue, calculatedProfitShare, description));
				}

				return profitShareCharges;
			}
		}

		RefCurrency[] UniqueCurrenciesOnCharges
		{
			get
			{
				List<RefCurrency> result = new List<RefCurrency>();

				if (ChargesIncludedInProfitShare != null)
				{
					foreach (ChargeWithCost charge in ChargesIncludedInProfitShare)
					{
						if (charge.SellCurrency != null && !result.Contains(charge.SellCurrency))
						{
							result.Add(charge.SellCurrency);
						}

						if (charge.CostCurrency != null && !result.Contains(charge.CostCurrency))
						{
							result.Add(charge.CostCurrency);
						}
					}
				}

				return result.ToArray();
			}
		}

		#endregion

		#region Total Profit + Profit To Invoice

		ZDecimal Calculate(RefCurrency currency, bool isForProfitCalculation)
		{
			ZDecimal result = 0m;

			if (ChargesIncludedInProfitShare != null)
			{
				foreach (ChargeWithCost charge in ChargesIncludedInProfitShare)
				{
					result += CalculateCharge(charge, currency, isForProfitCalculation);
				}
			}

			if (result < 0 && ProfitShareAgreement != null && !ProfitShareAgreement.O4_ShareLosses)
			{
				result = 0;
			}

			return result;
		}

		public ZDecimal CalculateCharge(ChargeWithCost charge, RefCurrency currency, bool isForProfitCalculation)
		{
			ZDecimal result = 0m;

			if (currency != null)
			{
				if (charge.SellCurrency == currency)
				{
					result += charge.JR_AgentDeclaredSellAmt;
				}

				if (isForProfitCalculation && charge.CostCurrency != null && charge.CostCurrency == currency)
				{
					result -= charge.JR_AgentDeclaredCostAmt;
				}
			}
			else
			{
				if (isForProfitCalculation)
				{
					result += charge.GetProfitInLocalCurrency(CostRevenueAmountType.AgentDeclared);
				}
				else
				{
					result += charge.JR_AgentDeclaredSellAmtLocal;
				}
			}

			return result;
		}

		ZDecimal CalculateProfit(RefCurrency currency)
		{
			return Calculate(currency, true);
		}

		ZDecimal CalculateGrossRevenue(RefCurrency currency)
		{
			return Calculate(currency, false);
		}

		(ZDecimal? result, ZString calculationDescription) CalculateProfitToInvoice(ZDecimal calculatedProfit, ZDecimal calculatedGrossRevenue)
		{
			if (ProfitSharePartyDetails == null)
			{
				return (null, ZString.Empty);
			}

			return ProfitSharePartyDetails.CalculateProfitShare(calculatedProfit, calculatedGrossRevenue, Parent.InvoicingSupporter.ActualChargeable, Parent.InvoicingSupporter.ContainerCount);
		}

		#endregion

		#region Profit Share Agreement

		public OrgProfitShareDetails ProfitShareAgreement { get; set; }

		/// <summary>
		/// Being used by DocumentWrappers as a shortcut to show parties' percents
		/// </summary>
		public ZDecimal ProfitSharePercent => ProfitSharePartyDetails?.PS_PartyProfitSharePercent ?? ZDecimal.Zero;

		public ZString PartyRateBasis => ProfitSharePartyDetails?.PS_PartyRateBasis ?? ZString.Empty;

		public ZDecimal PartyRate => ProfitSharePartyDetails?.PS_PartyRate ?? ZDecimal.Zero;

		public ZDecimal PartyMinimum => ProfitSharePartyDetails?.PS_PartyMinimum ?? ZDecimal.Zero;

		public ZDecimal ProfitShareInLocalCurrencyForPrinting
		{
			get
			{
				IsForPrinting = true;

				var result = ProfitShareCharges
					.FirstOrDefault(x => x.Currency.Code == GlbCompany.CurrentCompany.LocalCurrency.Code)
					?.ProfitShare
					?? ZDecimal.Zero;

				IsForPrinting = false;
				return result;
			}
		}

		public OrgProfitShareParty ProfitSharePartyDetails => ProfitShareAgreement?.PartyDetails.GetParty(PartyType);

		bool IsForPrinting { get; set; }

		#endregion
	}

	#region Profit Share Charge Detail

	public class ProfitShareCharge
	{
		public ProfitShareCharge(ProfitShareShipmentDetail shipmentDetail, RefCurrency currency, ZDecimal profit, ZDecimal grossRevenue, ZDecimal? profitShare, ZString description)
		{
			this.ShipmentDetail = shipmentDetail;
			this.CurrencyCode = currency.RX_Code;
			this.Currency = currency;
			this.Profit = profit;
			this.GrossRevenue = grossRevenue;
			this.ProfitShare = profitShare;
			this.Description = description;
		}

		public readonly ProfitShareShipmentDetail ShipmentDetail;
		public readonly ZString CurrencyCode;
		public readonly ZDecimal Profit;
		public readonly ZDecimal GrossRevenue;
		public readonly ZDecimal? ProfitShare;
		public readonly RefCurrency Currency;
		public readonly ZString Description;
	}

	#endregion
}
