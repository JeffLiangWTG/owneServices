using CargoWise.Types;
using Enterprise.Client.JAS.Business.AWB;
using Enterprise.Client.JAS.Business.Utilities;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class FBDNLine : MessageLine
	{
		public FBDNLine(ExportAWBHeader aWBHeader, ExportAWBRateLine aWBRateLine)
		{
			this.AWBHeader = aWBHeader;
			this.AWBRateLine = aWBRateLine;
		}

		protected override int FieldCount
		{
			get { return JXCConstants.FBDNFieldCount; }
		}

		protected override ZString LineType
		{
			get { return JXCConstants.LineTypes.FBDN; }
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.FBDNFieldPositions.NoOfPiecesOrRCP, AWBRateLine.ER_NoOfPiecesOrRCPAsInt);
			dataRow.SetField(JXCConstants.FBDNFieldPositions.TypeOfPieces, TypeOfPieces);
			dataRow.SetField(JXCConstants.FBDNFieldPositions.GrossWeight, AWBRateLine.ER_GrossWeight);
			dataRow.SetField(JXCConstants.FBDNFieldPositions.WeightInKgsOrLbs, AWBRateLine.ER_WeightInLBsOrKGs);
			dataRow.SetField(JXCConstants.FBDNFieldPositions.RateClassCode, AWBRateLine.ER_RateClass);
			dataRow.SetField(JXCConstants.FBDNFieldPositions.IATACommodityItemNumber, AWBRateLine.ER_CommodityItemNumber);
			dataRow.SetField(JXCConstants.FBDNFieldPositions.ChargeableWeight, AWBRateLine.ER_ChargeableWeight);
			dataRow.SetField(JXCConstants.FBDNFieldPositions.RateCharge, AWBRateLine.ER_RateChargeOrDiscount);
			dataRow.SetField(JXCConstants.FBDNFieldPositions.Total, AWBRateLine.ER_Total);
			dataRow.SetField(JXCConstants.FBDNFieldPositions.NatureAndQtyOfGoods, NatureAndQtyOfGoods, JXCConstants.FBDNFieldBoundaries.NatureAndQtyOfGoodsMaxLength);
			dataRow.SetField(JXCConstants.FBDNFieldPositions.DescriptionOfGoods, AWBHeader.GoodsDescription, JXCConstants.FBDNFieldBoundaries.GoodsDescriptionMaxLength);
		}

		#region Implementation

		public
 ZString NatureAndQtyOfGoods
		{
			get { return AWBRateLine.NatureAndQtyOfGoods.Text; }
		}

		public
 ZString TypeOfPieces
		{
			get
			{
				return (AWBHeader.EH_Table == JobShipmentSchema.Constants.TableName)
					? GetJASPackageUnitTypeFromShipmentOuterPacksType()
					: (ZString)FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
			}
		}

		ZString GetJASPackageUnitTypeFromShipmentOuterPacksType()
		{
			ZString packType = FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
			JASShipmentExportAWBHeader shipmentAWBHeader = AWBHeader as JASShipmentExportAWBHeader;
			if (shipmentAWBHeader != null && shipmentAWBHeader.Shipment != null)
			{
				packType = shipmentAWBHeader.Shipment.JS_F3_NKPackType;
			}
			return new JASUnitConverter().GetJASPackageUnitFromShipmentOuterPacksType(packType);
		}

		#endregion

		protected readonly ExportAWBHeader AWBHeader;
		readonly ExportAWBRateLine AWBRateLine;
	}
}

#region Implementation
#endregion
