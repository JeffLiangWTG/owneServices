using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.AU;

namespace Enterprise.Client.Wow
{
	public class WowDocJobComInvoiceLine : DocJobComInvoiceLine
	{
		#region Constructors and Type Overriding

		protected WowDocJobComInvoiceLine(JobComInvoiceLine invoiceLine, BusinessObjectFactory factoryToWrap) : base(invoiceLine, factoryToWrap)
		{
		}

		public new static DocJobComInvoiceLine New(JobComInvoiceLine invoiceLine, BusinessObjectFactory factoryToWrap)
		{
			return (invoiceLine == null) ? null : new WowDocJobComInvoiceLine(invoiceLine, factoryToWrap);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		protected new WoolworthsJobComInvoiceLine JobComInvoiceLine
		{
			get { return (WoolworthsJobComInvoiceLine)base.JobComInvoiceLine; }
		}

		public DocOrderLineDelivery OrderLineDeliveryUseFirstForDuplicatesOnSameOrderLine
		{
			get { return (JobComInvoiceLine.OrderLineDeliveries.Count == 0) ? null : DocOrderLineDelivery.New(JobComInvoiceLine.OrderLineDeliveries[0], Factory); }
		}

		protected override ZDecimal UnitPriceInLocalCurrencyCore
		{
			get
			{
				return (ComInvoiceHeader.LandedCostingExRateFallBackToJobExRate != 0 && InvoiceQuantity != 0m) ?
					LinePrice / ComInvoiceHeader.LandedCostingExRateFallBackToJobExRate / InvoiceQuantity : 0m;
			}
		}

		protected override ZDecimal LinePriceInLocalCurrencyCore
		{
			get
			{
				return (ComInvoiceHeader.LandedCostingExRateFallBackToJobExRate != 0) ? LinePrice / ComInvoiceHeader.LandedCostingExRateFallBackToJobExRate : 0m;
			}
		}

		public ZDecimal CommisionInLocalCurrency
		{
			get
			{
				return (ComInvoiceHeader.Supplier != null) ? ComInvoiceHeader.Supplier.CustomDecimal1 * LinePriceInLocalCurrency / 100 : 0m;
			}
		}

		public ZDecimal RoyaltyAmount
		{
			get
			{
				return (SupplierPart != null) ? SupplierPart.CustomDecimal1AsPercent * LinePriceInLocalCurrency : 0m;
			}
		}

		public ZDecimal ContingencyAmount
		{
			get
			{
				ZDecimal rate = 0m;

				if (ComInvoiceHeader.Buyer != null)
				{
					rate = ComInvoiceHeader.Buyer.CustomDecimal2;
				}
				else if (Order != null && Order.Consignee != null)
				{
					rate = Order.Consignee.CustomDecimal2;
				}
				return rate / 100 * LinePriceInLocalCurrency;
			}
		}

		public ZString ImporterOrBuyerName
		{
			get
			{
				ZString result = ZString.Empty;

				if (Order != null && Order.Consignee != null)
				{
					result = Order.Consignee.Name;
				}
				else if (ComInvoiceHeader != null &&
					ComInvoiceHeader.Declaration != null &&
					ComInvoiceHeader.Declaration.Consignee != null)
				{
					result = ComInvoiceHeader.Declaration.Consignee.Name;
				}

				return result;
			}
		}

		public ZString BuyerName
		{
			get
			{
				ZString result = ZString.Empty;

				if (OrderLineDeliveryUseFirstForDuplicatesOnSameOrderLine != null && OrderLineDeliveryUseFirstForDuplicatesOnSameOrderLine.OrderLine.CustomAttrib6 != ZString.Empty)
				{
					result = OrderLineDeliveryUseFirstForDuplicatesOnSameOrderLine.OrderLine.CustomAttrib6;
				}
				else if (Order != null)
				{
					result = Order.FirstBuyerContact;
				}

				return result;
			}
		}

		public ZString POMNum
		{
			get
			{
				return (OrderLineDeliveryUseFirstForDuplicatesOnSameOrderLine != null) ?
					OrderLineDeliveryUseFirstForDuplicatesOnSameOrderLine.CustomAttribute1 : ZString.Empty;
			}
		}

		public ZDecimal LandedCostingExRateFallBackToJobExRate
		{
			get
			{
				return (ComInvoiceHeader != null) ? ComInvoiceHeader.LandedCostingExRateFallBackToJobExRate : ZDecimal.Zero;
			}
		}

		public ZDecimal OtherImportCharges
		{
			get { return (WowDeclaration != null) ? CalculateChargesValue(WowDeclaration.OtherImportCharges) : ZDecimal.Zero; }
		}

		public ZDecimal DestinationCharges
		{
			get { return (WowDeclaration != null) ? CalculateChargesValue(WowDeclaration.DestinationCharges) : ZDecimal.Zero; }
		}

		public ZDecimal DetentionCharges
		{
			get { return (WowDeclaration != null) ? CalculateChargesValue(WowDeclaration.DetentionCharges) : ZDecimal.Zero; }
		}

		public ZDecimal OriginCharges
		{
			get { return (WowDeclaration != null) ? CalculateChargesValue(WowDeclaration.OriginCharges) : ZDecimal.Zero; }
		}

		ZDecimal CalculateChargesValue(ZDecimal declarationChargesValue)
		{
			ZDecimal result = ZDecimal.Zero;
			if (WowDeclaration != null && WowDeclaration.TotalInvoiceLinesVolume != 0)
			{
				result = declarationChargesValue * Volume / WowDeclaration.TotalInvoiceLinesVolume;
			}
			return result;
		}

		WowDocDeclaration WowDeclaration
		{
			get { return (ComInvoiceHeader != null && ComInvoiceHeader.Declaration != null) ? ComInvoiceHeader.Declaration as WowDocDeclaration : null; }
		}

		internal const string AgalRecoveryStr = "AGAL RECOVERY";
		internal const string IntertekRecoveryStr = "INTERTEK RECOVERY";
		internal const string BreakageRecoveryStr = "BREAKAGE RECOVERY";
		internal const string WineRecoveryStr = "WINE RECOVERY";
		internal const string SpareRecoveryStr = "SPARE RECOVERY";
	}
}
