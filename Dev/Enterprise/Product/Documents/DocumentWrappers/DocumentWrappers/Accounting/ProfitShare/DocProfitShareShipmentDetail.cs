using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocProfitShareShipmentDetail : DocBaseWrapper
	{
		protected DocProfitShareShipmentDetail(ProfitShareShipmentDetail profitShareShipmentDetail, BusinessObjectFactory factoryToWrap)
			: base(profitShareShipmentDetail, factoryToWrap)
		{
		}

		public static DocProfitShareShipmentDetail New(ProfitShareShipmentDetail profitShareShipmentDetail, BusinessObjectFactory factoryToWrap)
		{
			return profitShareShipmentDetail != null ? new DocProfitShareShipmentDetail(profitShareShipmentDetail, factoryToWrap) : null;
		}

		ProfitShareShipmentDetail ProfitShareShipmentDetail
		{
			get { return (ProfitShareShipmentDetail)WrappedObject; }
		}

		#region Charges

		public DocJobChargeCollection Charges => DocJobChargeCollection.GetCollection(this, nameof(Charges), ProfitShareShipmentDetail.ChargesIncludedInProfitShare);

		public ZDecimal CalculateProfitShare(JobCharge charge, RefCurrency currency = default)
		{
			var chargeable = ProfitShareShipmentDetail.Parent.InvoicingSupporter.ActualChargeable;
			var countainerCount = ProfitShareShipmentDetail.Parent.InvoicingSupporter.ContainerCount;

			var totalProfit = ProfitShareShipmentDetail.CalculateCharge((ChargeWithCost)charge, currency: currency, isForProfitCalculation: true);
			var grossRevenue = ProfitShareShipmentDetail.CalculateCharge((ChargeWithCost)charge, currency: currency, isForProfitCalculation: false);

			var (result, _) = ProfitShareShipmentDetail.ProfitSharePartyDetails?.CalculateProfitShareForSingleCharge
			(
				totalProfit,
				grossRevenue,
				chargeable,
				countainerCount
			) ?? (ZDecimal.Zero , ZString.Empty);
			return result;
		}

		#endregion

		#region Profit Share Agreement

		public DocProfitShareAgreement ProfitShareAgreement
		{
			get
			{
				if (fProfitShareAgreement == null)
				{
					fProfitShareAgreement = DocProfitShareAgreement.New(ProfitShareShipmentDetail.ProfitShareAgreement, Factory);
				}

				return fProfitShareAgreement;
			}
		}

		DocProfitShareAgreement fProfitShareAgreement;

		#endregion

		#region Percentage

		public ZDecimal RelevantAgentPercent
		{
			get { return ProfitShareShipmentDetail.ProfitSharePercent; }
		}

		#endregion

		#region Job Number

		public ZString JobNumber
		{
			get { return ProfitShareShipmentDetail.Parent != null ? ProfitShareShipmentDetail.Parent.JobNumber : ""; }
		}

		#endregion

		public ZDecimal ProfitShareInLocalCurrency => ProfitShareShipmentDetail.ProfitShareInLocalCurrencyForPrinting;

		public ZString PartyRateBasis => ProfitShareShipmentDetail.PartyRateBasis;

		public ZDecimal PartyRate => ProfitShareShipmentDetail.PartyRate;

		public ZDecimal ActualChargeable => ProfitShareShipmentDetail.Parent.InvoicingSupporter.ActualChargeable;

		public ZInt ContainerCount => ProfitShareShipmentDetail.Parent.InvoicingSupporter.ContainerCount;

		public ZDecimal PartyMinimum => ProfitShareShipmentDetail.PartyMinimum;

		public ZDecimal CalculatedBasisRateTotal
		{
			get
			{
				if (PartyRateBasis == "FLT")
				{
					return PartyRate;
				}
				else if (PartyRateBasis == "UNT")
				{
					return PartyRate * ActualChargeable;
				}
				else
				{
					return PartyRate * ContainerCount;
				}
			}
		}
	}
}
