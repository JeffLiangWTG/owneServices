using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public partial class JobConsolCost
	{
		#region ConsolCostCalculationStrategyWithCalculations

		public abstract class ConsolCostCalculationStrategyWithCalculations : JobConsolCostCalculationStrategyBase
		{
			public ConsolCostCalculationStrategyWithCalculations(JobConsolCost cost)
				: base(cost)
			{
			}

			public override void HandleDelete()
			{
				base.HandleDelete();
				for (int i = Cost.ApportionmentCharges.Count - 1; i >= 0; i--)
				{
					ApportionSplitCharge aCharge = Cost.ApportionmentCharges[i];

					if (aCharge.IsCostPosted)
					{
						aCharge.JR_E6 = ZGuid.Empty;
						Cost.ApportionmentCharges.Remove(aCharge);
					}
					else if (aCharge.IsRevenuePosted)
					{
						aCharge.ClearCostData();
						Cost.ApportionmentCharges.Remove(aCharge);
						aCharge.RestoreSellData();
					}
					else if (aCharge.IsInDatabase && aCharge.IsChargeRevenueEdited)
					{
						aCharge.UpdateCostData();
						Cost.ApportionmentCharges.Remove(aCharge);
					}
					else
					{
						aCharge.Delete();
					}
				}
				Cost.DeleteCostOnly();
			}

			public override void HandleExchangeRateChanged()
			{
				base.HandleExchangeRateChanged();
				if (!Cost.IsCalculatingForeignAndLocalAmountsSuspended)
				{
					using (ForeignLocalAmountSuspender suspender = new ForeignLocalAmountSuspender(Cost))
					{
						if (Cost.E6_RX_NKCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							SetForeignAmount();
						}
						else
						{
							SetLocalAmountFromForeignAmount();
						}
					}
				}
			}

			public override void HandleCurrencyChanged()
			{
				base.HandleCurrencyChanged();

				if (Cost.E6_ExchangeRate != 0 && Cost.E6_RX_NKCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					SetForeignAmount();
				}

				if (Cost.E6_ExchangeRate == 0 && Cost.E6_RX_NKCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					ReRoundForeignAmount();
				}

				if (!Cost.E6_OSCostAmount.IsEmpty)
				{
					Cost.SplitApportionAmount();
				}
			}

			protected void SetForeignAmount()
			{
				if (Cost.E6_RX_NKCurrency.IsValid)
				{
					Cost.E6_OSCostAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(Cost.E6_LocalCostAmount, Cost.E6_ExchangeRate, Cost.E6_RX_NKCurrency);
				}
				else
				{
					if (Cost.E6_ExchangeRate == 0)
					{
						Cost.E6_OSCostAmount = Cost.E6_LocalCostAmount;
					}
				}
			}

			protected void ReRoundForeignAmount()
			{
				if (Cost.E6_RX_NKCurrency.IsValid)
				{
					Cost.E6_OSCostAmount = AccountingUtils.Round(Cost.E6_OSCostAmount, Cost.E6_RX_NKCurrency);
				}
			}

			void SetLocalAmountFromForeignAmount()
			{
				Cost.E6_LocalCostAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(Cost.E6_OSCostAmount, Cost.E6_ExchangeRate);
			}

			void SetExchangeRateFromLocalAndForeignAmounts()
			{
				if (Cost.E6_OSCostAmount != 0 && Cost.E6_LocalCostAmount != 0)
				{
					Cost.E6_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(Cost.E6_LocalCostAmount, Cost.E6_OSCostAmount);
				}
			}

			public override void HandleForeignCostAmountChanged()
			{
				base.HandleForeignCostAmountChanged();
				if (!Cost.IsCalculatingForeignAndLocalAmountsSuspended)
				{
					using (ForeignLocalAmountSuspender suspender = new ForeignLocalAmountSuspender(Cost))
					{
						SetLocalAmountFromForeignAmount();
					}
				}
			}

			public override void HandleLocalCostAmountChanged()
			{
				base.HandleLocalCostAmountChanged();
				if (!Cost.IsCalculatingForeignAndLocalAmountsSuspended)
				{
					using (ForeignLocalAmountSuspender suspender = new ForeignLocalAmountSuspender(Cost))
					{
						if (Cost.E6_RX_NKCurrency.IsValid && Cost.E6_RX_NKCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							if (Cost.E6_OSCostAmount != 0)
							{
								SetExchangeRateFromLocalAndForeignAmounts();
							}
							else
							{
								SetForeignAmount();
							}
						}
						else
						{
							SetForeignAmount();
						}
					}
				}
			}

			protected override void SetChargeIsUsedForApportionment(ApportionSplitCharge charge, bool value)
			{
				charge.JR_IsUsedForApportionment = value;
			}

			public override void UpdateApportionmentChargesListing()
			{
				if (Cost.HasContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting) || Cost.IsPosted)
				{
					base.UpdateApportionmentChargesListing();
				}
				else
				{
					AddDefaultCharges();
					base.UpdateApportionmentChargesListing();
					AddShipmentPKsToList();
				}
			}

			void AddShipmentPKsToList()
			{
				int count = Cost.ApportionmentCharges.Count;

				if (count > 0 && Cost.Consol != null)
				{
					var shipmentList = Cost.Consol.CostSupporter.ShipmentsList;
					var shipmentsToApportion = Cost.ShipmentsToApportion;
					List<ZGuid> shipmentPKs = new List<ZGuid>();
					List<ZGuid> shipmentsToApportionPKs = new List<ZGuid>();
					foreach (IJobInvoicingPlugIn shipment in shipmentList)
					{
						shipmentPKs.Add(shipment.PK);
					}
					foreach (IJobInvoicingPlugIn shipment in shipmentsToApportion)
					{
						shipmentsToApportionPKs.Add(shipment.PK);
					}

					using (Cost.ApportionmentCharges.SuspendListChanged())
					{
						var apportionCharges = Cost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToArray();

						foreach (var charge in apportionCharges)
						{
							var chargeRelatedShipment = charge.ShipmentInfo ?? charge.InvoicingJob?.PlugInData;
							if (chargeRelatedShipment != null
								&& (!shipmentPKs.Contains(chargeRelatedShipment.PK)	|| (!shipmentsToApportionPKs.Contains(chargeRelatedShipment.PK) && !charge.JR_IsUsedForApportionment)))
							{
								Cost.RemoveNonApplicableChargeSafe(charge);
							}
						}
					}
				}
			}

			void AddDefaultCharges()
			{
				using (Cost.ApportionmentCharges.SuspendListChanged())
				{
					foreach (IJobInvoicingPlugIn shipment in Cost.ShipmentsToApportion)
					{
						Job shipmentJob = Cost.GetJob(shipment);
						if (shipmentJob != null && shipmentJob.JH_IsActive && !Cost.ApportionmentCharges.ContainsChargeForJob(shipment))
						{
							Cost.ApportionmentCharges.AddDefaultChargeForJob(shipmentJob);
						}
					}
				}
			}
		}

		#endregion
	}
}

