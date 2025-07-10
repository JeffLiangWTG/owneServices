using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaShipmentSelectorLine : ShipmentSelectorLine
	{
		public SeaShipmentSelectorLine(IScanMasterBillProvider oceanBill)
		{
			this.Shipment = oceanBill.IsStandAlone ? oceanBill.MasterHouseBill : (ZString)Res.GetString("FA6B0F9E-81D8-4BA1-9FCA-574CB0BC6573", "All Standards");
			this.oceanBill = oceanBill;
			this.Type = oceanBill.IsStandAlone ? Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy : Core.Constants.ShipmentTypes.StandardHouse;
		}
		readonly IScanMasterBillProvider oceanBill;

		public IScanMasterBillProvider OceanBill
		{
			get { return oceanBill; }
		}

		public void SetShipmentAdditionalInformation(ZString consigneeName, ZString consignorName, ZString goodsDescription)
		{
			this.Consignee = consigneeName;
			this.Consignor = consignorName;
			this.GoodsDescription = goodsDescription;
		}
	}
}
