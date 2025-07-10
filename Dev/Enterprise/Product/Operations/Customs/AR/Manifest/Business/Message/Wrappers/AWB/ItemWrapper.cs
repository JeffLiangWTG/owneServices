using System;
using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;

namespace Enterprise.Customs.AR.Manifest.Business
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

		decimal IItem.GrossWeight => ARHelperClass.WeightConvertion(pack.APA_WeightUQ, pack.APA_Weight);

		string IItem.GrossWeightUQ => ARHelperClass.WeightUnitCodeCalculator(pack.APA_WeightUQ);

		decimal IItem.GrossVolume => ARHelperClass.VolumeConvertion(pack.APA_VolumeUQ, pack.APA_Volume);

		string IItem.GrossVolumeUQ => ARHelperClass.VolumeUnitCodeCalculator(pack.APA_VolumeUQ);

		decimal IItem.TotalChargeAmount => pack.PackedItem.API_GoodsValue;

		string IItem.TotalChargeAmountCurrencyID => pack.PackedItem.API_RX_NKGoodsValueCurrency;

		int IItem.PieceQty => pack.APA_PackQty;

		string IItem.Identification => pack.PackedItem.API_GoodsDescription;

		string IItem.OriginID => pack.PackedItem.API_RN_NKGoodsOrigin;
	}
}
