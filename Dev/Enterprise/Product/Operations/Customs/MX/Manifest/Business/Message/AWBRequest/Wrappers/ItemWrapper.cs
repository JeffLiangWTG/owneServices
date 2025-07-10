using System;
using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class ItemWrapper : IItem
	{
		internal ItemWrapper(AsycudaPack pack)
		{
			this.pack = Argument.NotNull(pack, "asycudaPack cannot be null");
		}
		readonly AsycudaPack pack;

		decimal IItem.SequenceNumber => Convert.ToDecimal(pack.APA_LineNo);

		string IItem.TypeCode => pack.APA_CommodityCode;

		decimal IItem.GrossWeight => AWBRequestHelper.WeightConvertion(pack.APA_WeightUQ, pack.APA_Weight);

		string IItem.GrossWeightUQ => AWBRequestHelper.WeightUnitCodeCalculator(pack.APA_WeightUQ);

		decimal IItem.GrossVolume => AWBRequestHelper.VolumeConvertion(pack.APA_VolumeUQ, pack.APA_Volume);

		string IItem.GrossVolumeUQ => AWBRequestHelper.VolumeUnitCodeCalculator(pack.APA_VolumeUQ);

		decimal IItem.TotalChargeAmount => pack.LinePrice;

		string IItem.TotalChargeAmountCurrencyID => pack.LinePriceCurrency;

		int IItem.PieceQty => pack.APA_PackQty;

		string IItem.Identification => pack.APA_GoodsDescription;

		string IItem.OriginID => pack.Bill.ABL_RL_NKOrigin.SubstringSafe(0, 2);
	}
}
