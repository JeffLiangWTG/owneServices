using CargoWise.Types;
using Enterprise.Client.JAS.Business.Utilities;
using Enterprise.Freight.Common.Business;
using Enterprise.Registry.Business;
using ExportAWBHeader = Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader;
using ExportAWBRateLine = Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class FBDNRecord : JXCRecord
	{
		public FBDNRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		public void UpdateShipment(JASForwardingShipment shipment)
		{
			if (shipment != null)
			{
				if (shipment.JS_GoodsDescription.IsEmpty)
				{
					shipment.JS_GoodsDescription = GoodsDescription.Left(AutoJobShipment.Schema.JS_GoodsDescriptionMaxLength);
				}

				if (shipment.JS_F3_NKPackType.IsEmpty || shipment.JS_F3_NKPackType == FreightPacksDataRegistry.Instance.OuterPackUnit.Value)
				{
					shipment.JS_F3_NKPackType = new JASUnitConverter().GetShipmentOuterPacksTypeFromJASPackageUnit(TypeOfPieces);
				}
			}
		}

		#region UpdateAWBHeader

		public void UpdateAWBHeader(ExportAWBHeader aWBHeader)
		{
			if (aWBHeader != null)
			{
				ExportAWBRateLine rateLine = GetFirstEmptyRateLine(aWBHeader);
				rateLine.ER_NoOfPiecesOrRCP = NoOfPiecesOrRCP.ToString();
				rateLine.ER_GrossWeight = GrossWeight;
				rateLine.ER_WeightInLBsOrKGs = WeightInKgsOrLbs.Left(rateLine.ER_WeightInLBsOrKGsInfo.MaxLength);
				rateLine.ER_RateClass = RateClassCode.Left(rateLine.ER_RateClassInfo.MaxLength);
				rateLine.ER_CommodityItemNumber = IATACommodityItemNumber.Left(ExportAWBRateLine.Schema.ER_CommodityItemNumberMaxLength);
				rateLine.ER_ChargeableWeight = ChargeableWeight;
				rateLine.ER_RateChargeOrDiscount = RateCharge;
				rateLine.ER_Total = Total;
				SetNatureAndQtyOfGoods(aWBHeader);
			}
		}

		void SetNatureAndQtyOfGoods(ExportAWBHeader aWBHeader)
		{
			if (!NatureAndQtyOfGoods.IsEmpty)
			{
				var natureAndQtyOfGoodsInfo = aWBHeader.GetRateLineAt(aWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);
				if (natureAndQtyOfGoodsInfo != null)
				{
					natureAndQtyOfGoodsInfo.NatureAndQtyOfGoodsDescription = NatureAndQtyOfGoods.Left(ExportAWBRateLine.Schema.ER_NatureAndQtyOfGoodsMaxLength);
				}
			}
		}

		public
 ExportAWBRateLine GetFirstEmptyRateLine(ExportAWBHeader aWBHeader)
		{
			ExportAWBRateLine result = null;

			foreach (ExportAWBRateLine rateLine in aWBHeader.AWBRateLines)
			{
				if (rateLine.IsRateDescriptionEmpty)
				{
					result = rateLine;
					break;
				}
			}

			return result;
		}

		#endregion

		#region Implementation

		ZInt NoOfPiecesOrRCP
		{
			get { return Fields.GetIntFieldValue(JXCConstants.FBDNFieldPositions.NoOfPiecesOrRCP); }
		}

		ZString TypeOfPieces
		{
			get { return Fields.GetFieldValue(JXCConstants.FBDNFieldPositions.TypeOfPieces); }
		}

		ZDecimal GrossWeight
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.FBDNFieldPositions.GrossWeight); }
		}

		ZString WeightInKgsOrLbs
		{
			get { return Fields.GetFieldValue(JXCConstants.FBDNFieldPositions.WeightInKgsOrLbs); }
		}

		ZString RateClassCode
		{
			get { return Fields.GetFieldValue(JXCConstants.FBDNFieldPositions.RateClassCode); }
		}

		ZString IATACommodityItemNumber
		{
			get { return Fields.GetFieldValue(JXCConstants.FBDNFieldPositions.IATACommodityItemNumber); }
		}

		ZDecimal ChargeableWeight
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.FBDNFieldPositions.ChargeableWeight); }
		}

		ZDecimal RateCharge
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.FBDNFieldPositions.RateCharge); }
		}

		ZDecimal Total
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.FBDNFieldPositions.Total); }
		}

		ZString NatureAndQtyOfGoods
		{
			get { return Fields.GetFieldValue(JXCConstants.FBDNFieldPositions.NatureAndQtyOfGoods); }
		}

		ZString GoodsDescription
		{
			get { return Fields.GetFieldValue(JXCConstants.FBDNFieldPositions.DescriptionOfGoods); }
		}

		#endregion
	}
}
