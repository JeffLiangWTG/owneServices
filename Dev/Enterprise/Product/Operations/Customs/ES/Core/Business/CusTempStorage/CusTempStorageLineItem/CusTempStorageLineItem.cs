using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class CusTempStorageLineItem : EU.Business.CusTempStorage.CusTempStorageLineItem
		, Integration.Customs.ES.ICusTempStorageLineItem
	{
		public CusTempStorageLineItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageLineItem|TSI_CommodityCode", Caption = "Commodity Code")]
		public override ZString TSI_CommodityCode { get => base.TSI_CommodityCode; set => base.TSI_CommodityCode = value; }

		[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageLineItem|TSI_NetWeight", Caption = "Net Mass")]
		public override ZDecimal TSI_NetWeight { get => base.TSI_NetWeight; set => base.TSI_NetWeight = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineItemLookups.WeightUQList))]
		[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageLineItem|TSI_NetWeightUQ", Caption = "Net Mass UQ")]
		public override ZString TSI_NetWeightUQ { get => base.TSI_NetWeightUQ; set => base.TSI_NetWeightUQ = value; }

		[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageLineItem|TSI_GoodsValue", Caption = "Goods Value")]
		public override ZDecimal TSI_GoodsValue { get => base.TSI_GoodsValue; set => base.TSI_GoodsValue = value; }

		[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageLineItem|TSI_GuaranteedValue", Caption = "Guaranteed Value")]
		public override ZDecimal TSI_GuaranteedValue { get => base.TSI_GuaranteedValue; set => base.TSI_GuaranteedValue = value; }

		[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageLineItem|TSI_RX_NKCurrency", Caption = "Currency")]
		public override ZString TSI_RX_NKCurrency { get => base.TSI_RX_NKCurrency; set => base.TSI_RX_NKCurrency = value; }

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TSI_RX_NKCurrency = Core.Constants.CurrencyCodes.Spain;
		}

		#region Lookups
		public new CusTempStorageLineItemLookups Lookups => (CusTempStorageLineItemLookups)base.Lookups;

		protected override EU.Business.CusTempStorage.CusTempStorageLineItemLookups GetNewLookups() => new CusTempStorageLineItemLookups(this);
		#endregion
	}
}
