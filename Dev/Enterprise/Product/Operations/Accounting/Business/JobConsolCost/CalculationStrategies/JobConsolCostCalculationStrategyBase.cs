using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public partial class JobConsolCost
	{
		#region Calculation Strategies

		#region Base

		public abstract class JobConsolCostCalculationStrategyBase
		{
			protected JobConsolCostCalculationStrategyBase(JobConsolCost cost)
			{
				this.Cost = cost;
				this.Factory = cost.Factory;
			}

			protected readonly BusinessObjectFactory Factory;
			protected readonly JobConsolCost Cost;

			public void ReleaseMutexes()
			{
				foreach (Job job in Cost.JobsWithMutexes)
				{
					job.Dispose();
				}
			}

			public virtual void OnE6_OH_CreditorSet()
			{
			}

			public virtual void OnE6_AK_ChequeBookSet()
			{
			}

			public virtual void OnE6_ChequeOrReferenceSet()
			{
			}

			public virtual ZString GetPaddedChequeNo(AccValidationHelper validationHelper, ZString rawChequeNumber)
			{
				return rawChequeNumber;
			}

			public virtual void PrepareForPosting()
			{
			}

			public virtual void HandleDelete()
			{
				if (!Cost.IsGatewayConsolCost)
				{
					var splitCharges = Cost.ApportionmentCharges;
					foreach (ApportionSplitCharge splitCharge in splitCharges)
					{
						if (splitCharge.IsInDatabase)
						{
							using (splitCharge.SetTempContext(BusinessContext.DeletingConsolCost))
							{
								splitCharge.CancelChanges();
							}
						}
					}
				}

				Cost.PaymentBases.DeleteAll();
			}

			public virtual void UpdateApportionmentChargesListing()
			{
				UpdateShipmentInfosOnCharges();
			}

			public void UpdateShipmentInfosOnCharges()
			{
				Dictionary<ZGuid, IJobInvoicingPlugIn> pluginsByPK = null;
				foreach (ApportionSplitCharge charge in Cost.ApportionmentCharges)
				{
					if (charge.ShipmentInfo == null)
					{
						if (charge.Job != null)
						{
							using (Cost.SuspendSplittingApportionAmount())
							using (charge.SuspendSettingHasChanges())
							{
								charge.SetShipmentInfo(FindShipmentInfo(charge.InvoicingJob));
								SetChargeIsUsedForApportionment(charge, charge.JR_OSCostAmt != 0);
							}
						}
					}
				}

				IJobInvoicingPlugIn FindShipmentInfo(Job job)
				{
					IJobInvoicingPlugIn result = null;
					var consol = Cost.Consol;
					if (consol != null)
					{
						if (pluginsByPK == null)
						{
							pluginsByPK = consol.CostSupporter.ShipmentsList.ToDictionary(x => x.PK);
						}

						pluginsByPK.TryGetValue(job.JH_ParentID, out result);
					}

					return result;
				}
			}

			protected virtual void SetChargeIsUsedForApportionment(ApportionSplitCharge charge, bool value)
			{
				charge.SetChargeIsUsedForApportionmentWithoutCalulations(value);
			}

			public virtual void OnE6_ParentIDConsolSet()
			{
				DefaultExchangeRate();

				// if JobConsolCost is created from the Apportionment multiple consol form, then add it to the pending list, it will be calculated later.
				if (Factory.HasContext(BusinessContext.APInvoiceApportionToConsol) && Cost.E6_OH_Creditor.IsValid && Cost.E6_ParentID.IsValid)
				{
					var accrualCalculator = Factory.GetCachedValue(ConsolAndCostAccrualCalculator.ConsolAndCostAccrualCalculatorKey,
																() => { return new ConsolAndCostAccrualCalculator(Factory, Cost.E6_OH_Creditor); });
					accrualCalculator.PendingConsolCosts.Add(Cost);
				}
			}

			public virtual void HandleForeignCostAmountChanged()
			{
			}

			public virtual void HandleForeignGSTAmountChanged()
			{
			}

			public virtual void HandleLocalCostAmountChanged()
			{
			}

			public virtual void HandleLocalGSTAmountChanged()
			{
			}

			public virtual void HandleExchangeRateChanged()
			{
			}

			public virtual void HandleInvoiceNumberChanged()
			{
			}

			public virtual void HandleCreditorChanged()
			{
				DefaultExchangeRate();
			}

			public virtual void HandleCurrencyChanged()
			{
				DefaultExchangeRate();
			}

			protected virtual void DefaultExchangeRate()
			{
				Cost.E6_ExchangeRate = CalculateExchangeRateBasedOnToJobBillingExchangeRateConfig(Cost.Currency);
				Cost.CostExchangeRate.DontSetTodaysRateOnCurrencyChange = true; //to prevent overwriting E6_ExchangeRate in ZExchangeRate.Currency
			}

			public ZDecimal CalculateExchangeRateBasedOnToJobBillingExchangeRateConfig(RefCurrency currency, OrgHeader creditor = null)
			{
				var exchangeRate = 0m;
				if (currency != null)
				{
					if (currency.RX_Code == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						exchangeRate = 1m;
					}
					else
					{
						exchangeRate = GetExchangeRateFromRateFinder(currency, creditor);
					}
				}
				return exchangeRate;
			}

			protected virtual ZDecimal GetExchangeRateFromRateFinder(RefCurrency currency, OrgHeader creditor = null)
			{
				return AccExchangeRateConfigurationRateFinder.GetExchangeRate(Cost.ExchangeRateConfigurationRateConsumer, currency, creditor ?? Cost.Creditor, ExchangeRateValidLedgerEnum.AP);
			}
		}

		#endregion

		#endregion
	}
}
