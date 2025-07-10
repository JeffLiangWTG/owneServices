using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415GoodsShipmentItemGoodsInformationProvider : IGoodsShipmentItemTypeGoodsInformation
	{
		public IM413AndIM415GoodsShipmentItemGoodsInformationProvider(CusEntryLine entryLine)
		{
			this.entryLine = entryLine;
			randomMainPackInvoiceLine = entryLine.RandomMainPackLineOrRandomLine;
		}
		readonly CusEntryLine entryLine;
		readonly JobComInvoiceLine randomMainPackInvoiceLine;

		public decimal NetMass => entryLine.EffectiveCustomsWeight.InKilogramsSafe;

		public decimal SupplementaryUnits => entryLine.SupplementaryQuantity;

		public decimal GrossMass => entryLine.EffectiveGrossWeight.InKilogramsSafe;

		public string GoodsDescription => randomMainPackInvoiceLine.JI_Description;

		public IReadOnlyCollection<CargoWise.Customs.IE.MessageContracts.Interfaces.IPackaging> Packaging => packaging ??= AISPackagingProvider.GetCollection(entryLine);
		IReadOnlyCollection<CargoWise.Customs.IE.MessageContracts.Interfaces.IPackaging> packaging;

		public string CusCode => randomMainPackInvoiceLine.ZG_CusNumber;

		public IGoodsShipmentItemTypeGoodsInformationCommodityCode CommodityCode => commodityCodeCached ??= new IM413AndIM415GoodsShipmentItemGoodsInformationCommodityCodeProvider(randomMainPackInvoiceLine);

		IGoodsShipmentItemTypeGoodsInformationCommodityCode commodityCodeCached;
	}
}
