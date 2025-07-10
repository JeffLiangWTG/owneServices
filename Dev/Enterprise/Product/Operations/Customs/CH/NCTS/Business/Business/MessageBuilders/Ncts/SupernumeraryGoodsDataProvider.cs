using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class SupernumeraryGoodsDataProvider : ISupernumeraryGoods
{
	public static IEnumerable<SupernumeraryGoodsDataProvider> NewCollection(SupernumeraryGoodsCollection supernumeraryGoods)
		=> supernumeraryGoods?.Select(x => new SupernumeraryGoodsDataProvider(x));

	SupernumeraryGoodsDataProvider(SupernumeraryGoods supernumeraryGoods)
	{
		this.supernumeraryGoods = supernumeraryGoods;
	}
	readonly SupernumeraryGoods supernumeraryGoods;

	public int SequenceNumber => supernumeraryGoods.CSI_LineNo;

	public string DescriptionOfGoods => supernumeraryGoods.CSI_Description;

	public decimal GrossMass => supernumeraryGoods.CSI_Quantity;

	public int NumberOfPackages => supernumeraryGoods.CSI_PackQty;

	public string TypeOfPackages => supernumeraryGoods.CSI_PackType;

	public string HarmonizedSystemSubHeadingCode => supernumeraryGoods.CSI_Tariff.ReturnNullIfEmpty();
}
