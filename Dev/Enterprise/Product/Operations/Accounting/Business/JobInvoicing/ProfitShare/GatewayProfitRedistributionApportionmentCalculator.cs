using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	class GatewayProfitRedistributionApportionmentCalculator
	{
		public GatewayProfitRedistributionApportionmentCalculator(
			IEnumerable<ProfitShareForwardingConsolWrapper> consols,
			IEnumerable<IJobInvoicingPlugIn> shipments,
			GatewayProfitRedistributionApportionmentCriteria criteria,
			IDisposableProfitShareRedistributionLogger logger)
		{
			this.consols = consols?.DistinctBy(consol => consol.Consol.PK).ToList();
			this.shipments = shipments?.DistinctBy(shipment => shipment.PK).ToList();
			this.criteria = criteria;
			this.shipmentShares = new Dictionary<ZGuid, decimal>();
			this.logger = logger;
			this.currencyCode = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
		}

		readonly IEnumerable<ProfitShareForwardingConsolWrapper> consols;
		readonly IEnumerable<IJobInvoicingPlugIn> shipments;
		readonly GatewayProfitRedistributionApportionmentCriteria criteria;
		readonly Dictionary<ZGuid, decimal> shipmentShares;
		readonly IDisposableProfitShareRedistributionLogger logger;
		readonly ZString currencyCode;

		public void CalculateProfitPerShipment()
		{
			if (shipmentShares.Count == 0)
			{
				logger.Information((NoResString)"Starting Consols' profit Calculation...");

				var totalProfit = CalculateTotalProfitOfConsols();
				logger.Information($"Total Consols Profit ({currencyCode}): {totalProfit.TotalProfitOfConsols.ToString()}");
				logger.Information($"Total Consols Profit for redistribution ({currencyCode}): {totalProfit.TotalProfitOfConsolsAvailableForRedistribution.ToString()}");

				if (criteria != null)
				{
					criteria.ProfitShareRedistribution.PSR_TotalProfitShare = totalProfit.TotalProfitOfConsols;
					criteria.ProfitShareRedistribution.PSR_RedistributedProfitShare = totalProfit.TotalProfitOfConsolsAvailableForRedistribution;
					criteria.ProfitShareRedistribution.PSR_RX_NKCurrency = currencyCode;
				}

				var totalShipmentsValue = 0m;

				if (shipments != null && criteria != null)
				{
					logger.Information($"Calculating Total Shipments' chargeables...");
					logger.Information($"Apportionment Method: {criteria.ProfitApportionmentMethod}");

					foreach (var shipment in shipments)
					{
						var shipmentValue = GetShipmentValue(shipment.InvoicingSupporter);
						logger.Information($"{shipment.JobNumber} chargeable by Profit Apportionment Method: {shipmentValue.ToString()}");

						totalShipmentsValue += shipmentValue;
						shipmentShares.Add(shipment.PK, shipmentValue);
					}
				}

				logger.Information($"Total Shipments' chargeables: {totalShipmentsValue.ToString()}");
				if (totalShipmentsValue > 0)
				{
					var multiplier = totalProfit.TotalProfitOfConsolsAvailableForRedistribution / totalShipmentsValue;
					foreach (var shipment in shipments)
					{
						var shares = shipmentShares[shipment.PK] * multiplier;
						shipmentShares[shipment.PK] = shares;

						logger.Information($"{shipment.JobNumber} Shares: {shares.ToString()}");
					}
				}
			}
		}

		public decimal GetShipmentShare(ZGuid shipmentPK)
		{
			if (shipmentShares.TryGetValue(shipmentPK, out decimal share))
			{
				return share;
			}

			return 0;
		}

		decimal GetShipmentValue(IJobInvoicingSupporter shipment)
		{
			switch (criteria.ProfitApportionmentMethod)
			{
				case "SHP":
					return 1m;
				case "CHG":
					return CalculateShipmentChargeable(shipment);
				case "GWT":
					return Constants.Weight.Convert(shipment.ActualWeight, shipment.ActualWeightUnit, Constants.Weight.Kilograms);
				case "GVT":
					return Constants.Volume.Convert(shipment.ActualVolume, shipment.ActualVolumeUnit, Constants.Volume.CubicMetres);
				default:
					break;
			}
			return 0m;
		}

		decimal CalculateShipmentChargeable(IJobInvoicingSupporter shipment)
			=> ChargeableAmountCalculator.Convert(
				new Quantity(shipment.ActualChargeable, shipment.ActualChargeableUnit),
				Constants.Weight.Kilograms,
				ChargeableAmountCalculator.GetDefaultConversionFactors(shipment.IsDomestic, shipment.TransportMode, Constants.Weight.Kilograms)).Amount;

		(decimal TotalProfitOfConsols, decimal TotalProfitOfConsolsAvailableForRedistribution) CalculateTotalProfitOfConsols()
		{
			var totalProfitOfConsols = 0m;
			var totalProfitOfConsolsAvailableForRedistribution = 0m;

			if (consols != null && criteria != null)
			{
				foreach (var item in consols)
				{
					using (var job = item.Consol.Job as Job)
					{
						if (job != null)
						{
							var totalProfit = new Money(job.JH_ProfitLoss, GlbCompany.CurrentCompany.LocalCurrency).Amount;
							logger.Information($"{item.Consol.JK_UniqueConsignRef} Total Profit ({currencyCode}): {totalProfit.ToString()}.");

							var consolProfit = job.GetTotalProfitAndLossLocal(charge => criteria.IsChargeGroupProfitShared(charge.ChargeCode, job.PaymentTerm)).Amount;
							var calculatedProfit = consolProfit >= 0 || criteria.ShareLosses ? consolProfit : 0;
							logger.Information($"{item.Consol.JK_UniqueConsignRef} Total Profit for redistribution ({currencyCode}): {calculatedProfit.ToString()}.");

							// assign the calculated profit share to main object
							var profitShareRedistributionConsol = criteria.ProfitShareRedistribution.ConsolProfitShares
								.Cast<ConsolidationProfitShare>()
								.FirstOrDefault(x => x.PK == item.Consol.PK);//note - we don't have to find it based on CPS_JH as ConsolProfitShares will only have data for current company.

							if (profitShareRedistributionConsol == null)
							{
								profitShareRedistributionConsol = criteria.ProfitShareRedistribution.ConsolProfitShares.AddNew();
								profitShareRedistributionConsol.CPS_JK = item.Consol.PK;
								profitShareRedistributionConsol.CPS_JH_ConsolJob = job.PK;
							}

							if (profitShareRedistributionConsol != null)
							{
								profitShareRedistributionConsol.CPS_TotalConsolProfitShare = totalProfit;
								profitShareRedistributionConsol.CPS_RedistributedConsolProfitShare = calculatedProfit;
								profitShareRedistributionConsol.CPS_RX_NKCurrency = currencyCode;
							}

							item.JK_Calc_TotalProfitAmount = totalProfit;
							item.JK_Calc_RedistributedProfitAmount = calculatedProfit;

							totalProfitOfConsols += totalProfit;
							totalProfitOfConsolsAvailableForRedistribution += calculatedProfit;
						}
						else
						{
							logger.Warning($"{item.Consol.JK_UniqueConsignRef} doesn't have a Job header.");
						}
					}
				}
			}

			return (totalProfitOfConsols, totalProfitOfConsolsAvailableForRedistribution);
		}
	}
}
