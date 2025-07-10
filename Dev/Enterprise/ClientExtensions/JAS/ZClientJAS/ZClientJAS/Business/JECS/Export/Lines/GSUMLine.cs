
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class GSUMLine : MessageLine
	{
		public GSUMLine(GsumShipmentWrapper shipmentWrapper, ZString statusCode, ZDateTime statusDateTime)
		{
			this.ShipmentWrapper = shipmentWrapper;
			this.StatusCode = statusCode;
			this.StatusDateTime = statusDateTime;
		}

		protected override ZString LineType
		{
			get { return JXCConstants.LineTypes.GSUM; }
		}

		protected override int FieldCount
		{
			get { return JXCConstants.GSUMFieldCount; }
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.GSUMFieldPositions.AirMaritimeShipment, (ShipmentWrapper.Shipment.IsAir) ? "A" : "M");
			dataRow.SetField(JXCConstants.GSUMFieldPositions.HouseBillSent, "Y");
			dataRow.SetField(JXCConstants.GSUMFieldPositions.StatusCode, StatusCode);
			dataRow.SetField(JXCConstants.GSUMFieldPositions.StatusDate, StatusDateTime.ToString("dd/MM/yyyy"));
			dataRow.SetField(JXCConstants.GSUMFieldPositions.StatusTime, StatusDateTime.ToShortTimeString());
			dataRow.SetField(JXCConstants.GSUMFieldPositions.OriginTrafficSystemFileNumber, ShipmentWrapper.Shipment.JS_UniqueConsignRef);
			dataRow.SetField(JXCConstants.GSUMFieldPositions.HouseBillNumber, ShipmentWrapper.Shipment.JS_HouseBill);
			dataRow.SetField(JXCConstants.GSUMFieldPositions.OriginOfficeCode, OriginOfficeCode);
			dataRow.SetField(JXCConstants.GSUMFieldPositions.PredictedOrActual, "A");
		}

		ZString OriginOfficeCode
		{
			get
			{
				ZString result = JXCConstants.NoNettingCode;

				if (ShipmentWrapper.OriginOfficeOrg != null && !ShipmentWrapper.OriginOfficeOrg.OfficeCode.IsEmpty)
				{
					result = ShipmentWrapper.OriginOfficeOrg.OfficeCode;
				}
				else
				{
					JASOrgHeader currentOrg = GlbBranch.CurrentBranch.OrgProxy as JASOrgHeader;
					if (currentOrg != null && !currentOrg.OfficeCode.IsEmpty)
					{
						result = currentOrg.OfficeCode;
					}
				}

				return result;
			}
		}

		readonly GsumShipmentWrapper ShipmentWrapper;
		readonly ZString StatusCode;
		readonly ZDateTime StatusDateTime;
	}
}
