
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class REFRRecord : JXCRecord
	{
		public REFRRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		public void UpdateShipment(JASForwardingShipment shipment)
		{
			if (ReferenceFromShipper)
			{
				shipment.JS_BookingReference = Reference.Left(shipment.JS_BookingReferenceInfo.MaxLength);
			}
		}

		ZString Reference
		{
			get { return Fields.GetFieldValue(JXCConstants.REFRFieldPositions.Reference); }
		}

		bool ReferenceFromShipper
		{
			get { return Fields.GetFieldValue(JXCConstants.REFRFieldPositions.FromShipperOrConsignee) == "S"; }
		}
	}
}
