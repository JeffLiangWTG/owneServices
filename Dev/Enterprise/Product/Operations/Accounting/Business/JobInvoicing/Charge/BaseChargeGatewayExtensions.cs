using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Billing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	internal static class BaseChargeGatewayExtensions
	{
		public static IJobInvoicingPlugIn[] ApportionTargets(this ApportionSplitCharge charge)
		{
			Argument.NotNull(charge, nameof(charge));

			var result = new List<IJobInvoicingPlugIn>();
			result.Add(charge.ShipmentInfo);

			var invoiceTargets = InvoiceTargets(charge);
			var otherGatewayConsols = invoiceTargets
				.Where(x => x.IsGatewayBillingEnabled());

			result.AddRange(otherGatewayConsols);

			return result
				.WhereNotNull()
				.Where(x => !string.IsNullOrWhiteSpace(x.JobNumber))
				.ToArray();
		}

		public static IJobInvoicingPlugIn[] RelatedShipments(this JobCharge charge)
		{
			Argument.NotNull(charge, nameof(charge));

			var stalenessPolicy = CacheStalenessPolicy.StaleWhenDataTableChanges(JobConShipLinkSchema.Constants.TableName, charge.Factory);
			var shipments = charge.Factory.GetCachedValue("BaseChargeExtensions.RelatedShipments" + charge.PK, () => RelatedShipmentsCore(charge), stalenessPolicy);

			return shipments;
		}

		static IJobInvoicingPlugIn[] RelatedShipmentsCore(JobCharge charge)
		{
			Argument.NotNull(charge, nameof(charge));

			if (charge is ApportionSplitCharge appCharge)
			{
				var shipmentInfo = appCharge.ShipmentInfo;
				return shipmentInfo != null ? new[] { shipmentInfo } : Array.Empty<IJobInvoicingPlugIn>();
			}

			var consol = (charge.Job?.Parent as IGenericJobCostPlugIn)?.CostSupporter;
			var result = consol?.ShipmentsList?.Where(x => x.IsInDatabase) ?? Array.Empty<IJobInvoicingPlugIn>();

			return result.OrderBy(x => x.JobNumber).ToArray();
		}

		public static bool InvoiceTargetsEnabled(this BaseCharge charge)
		{
			Argument.NotNull(charge, nameof(charge));

			var enabled = !charge.RelatedJobID.IsEmpty;
			enabled = enabled && charge.RelatedShipments().Any(x => x.JobNumber == charge.JR_Calc_RelatedJobNumber);
			enabled = enabled && !charge.IsRevenuePosted;
			enabled = enabled && (charge.SellAccount?.InvoiceTargetsEnabled() ?? false);

			return enabled;
		}

		public static IJobInvoicingPlugIn[] GetInvoiceTargetsIfEnabled(this BaseCharge charge)
		{
			Argument.NotNull(charge, nameof(charge));

			return charge.InvoiceTargetsEnabled()
				? InvoiceTargets(charge)
				: Array.Empty<IJobInvoicingPlugIn>();
		}

		static IJobInvoicingPlugIn[] InvoiceTargets(this BaseCharge charge)
		{
			var relatedShipment = charge is ApportionSplitCharge appCharge
				? appCharge.ShipmentInfo
				: charge.RelatedJob;

			return GatewayInvoiceTargetJobFinder.GetInvoiceTargets(relatedShipment);
		}

		public static ZString GetInvoiceTarget(this BaseCharge charge)
		{
			Argument.NotNull(charge, nameof(charge));
			return charge.InvoiceTargetsEnabled()
				? GatewayInvoiceTargetJobFinder.GetInvoiceTarget(charge.JR_Calc_RelatedJobNumber, charge.InvoicingJob)
				: ZString.Empty;
		}

		public static ZBool IsDebtorGatewayAgent(this BaseCharge charge)
		{
			Argument.NotNull(charge, nameof(charge));
			var result = false;

			var gatewayAgent = charge.Job?.Parent?.GatewayAgent();
			if (charge.SellAccount != null && gatewayAgent.HasValue)
			{
				result = charge.JR_OH_SellAccount == gatewayAgent.Value.sendingAgent?.PK
					|| charge.JR_OH_SellAccount == gatewayAgent.Value.receivingAgent?.PK;
			}

			return result;
		}

		public static bool IsGatewayApportionedCostCharge(this BaseCharge charge) => TryFindGatewayParentConsolCostPK(charge, out _);
		public static bool IsGatewaySynchronizedSellCharge(this BaseCharge charge) => TryFindSellApportionmentConsolCostPK(charge, out _);

		public static bool TryFindGatewaySellAndCostsFromGatewaySell(this BaseCharge gatewaySellCharge, out (ChargeWithCost gatewayBillingSellCharge, ChargeWithCost[] costCharges) gatewayBatch)
			=> gatewaySellCharge.TryFindGatewaySellAndCosts(true, out gatewayBatch);

		public static bool TryFindGatewaySellAndCostsFromApportionedCost(this BaseCharge apportionedCostCharge, out (ChargeWithCost gatewayBillingSellCharge, ChargeWithCost[] costCharges) gatewayBatch)
			=> apportionedCostCharge.TryFindGatewaySellAndCosts(false, out gatewayBatch);

		static bool TryFindGatewaySellAndCosts(this BaseCharge charge, bool fromSell, out (ChargeWithCost gatewayBillingSellCharge, ChargeWithCost[] costCharges) gatewaySellAndCosts)
		{
			ZGuid gatewaysSellHeaderPK;

			if (fromSell && charge.TryFindSellApportionmentConsolCostPK(out gatewaysSellHeaderPK) || !fromSell && charge.TryFindGatewayParentConsolCostPK(out gatewaysSellHeaderPK))
			{
				gatewaySellAndCosts = LoadGatewayBatch(charge.Factory, gatewaysSellHeaderPK);
				return true;
			}

			gatewaySellAndCosts = default;
			return false;
		}

		static (ChargeWithCost gatewayBillingSellCharge, ChargeWithCost[] costCharges) LoadGatewayBatch(BusinessObjectFactory factory, ZGuid gatewaysSellHeaderPK)
		{
			(ChargeWithCost gatewayBillingSellCharge, ChargeWithCost[] costCharges) gatewaySellAndCosts;
			ChargeWithCost gatewayBillingSellCharge = null;
			var costCharges = new List<ChargeWithCost>();

			var batchQuery = new ZQuery(JobChargeSchema.JR_E6_GatewaySellHeader, gatewaysSellHeaderPK);
			batchQuery.AddToFilter(new ZQuery(JobChargeSchema.JR_E6, gatewaysSellHeaderPK), JoinCondition.Or);
			var allCharges = factory.Load<ChargeWithCost>(batchQuery);

			foreach (var batchCharge in allCharges)
			{
				if (batchCharge.IsGatewayApportionedCostCharge())
				{
					costCharges.Add(batchCharge);
				}
				else
				{
					gatewayBillingSellCharge = batchCharge;
				}
			}

			gatewaySellAndCosts = (gatewayBillingSellCharge, costCharges.ToArray());
			return gatewaySellAndCosts;
		}

		static bool TryFindGatewayParentConsolCostPK(this BaseCharge charge, out ZGuid pk)
		{
			pk = charge.JR_E6 == charge.JR_E6_GatewaySellHeader
				? charge.JR_E6_GatewaySellHeader
				: ZGuid.Empty;

			return pk != ZGuid.Empty;
		}

		static bool TryFindSellApportionmentConsolCostPK(this BaseCharge charge, out ZGuid pk)
		{
			pk = charge.JR_E6 != charge.JR_E6_GatewaySellHeader
				? charge.JR_E6_GatewaySellHeader
				: ZGuid.Empty;

			return pk != ZGuid.Empty;
		}

		internal static void AddGSHBillingEvent(this ChargeWithCost chargeWithCost, string billingEventCode)
		{
			if (chargeWithCost.InvoicingJob.IsEligibleForGSHBilling())
			{
				AccBillingEventCollector.GetInstance(chargeWithCost.Factory).AddEvent(AccBillingCodes.GatewayBilling, (chargeWithCost.InvoicingJob.Parent as IJobCostingPlugIn).PK, JobConsolSchema.Constants.Prefix, billingEventCode);
			}
		}

		internal static bool IsEligibleForGSHBilling(this Job job)
		{
			return job != null &&
				(job.JobType?.Code ?? string.Empty) == JobInvoicingConsumerTypes.GatewayConsol.Code &&
				job.Parent is IBillingPlugin billingPlugin && billingPlugin.IsGatewayBillingEnabled();
		}
	}
}
