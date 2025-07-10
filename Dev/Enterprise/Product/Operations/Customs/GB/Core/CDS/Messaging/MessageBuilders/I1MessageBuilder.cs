using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class I1MessageBuilder : I1MessageBuilderControlledGoods
	{
		public I1MessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCode) : base(cusEntryHeader, errorCollector, functionCode)
		{
		}

		protected override void PopulateSpecificTaxBaseQuantity(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
		}

		protected override bool ShouldSendInvoiceLineItemChargeAmount(ICommodity giCommodity) =>
			(!giCommodity.InvoiceLineItemCharge.Currency.IsEmpty && !giCommodity.InvoiceLineItemCharge.Amount.IsEmpty)
			|| giCommodity.DutyTaxFees.Any(x => !x.QuotaOrderID.IsEmpty);
	}
}
