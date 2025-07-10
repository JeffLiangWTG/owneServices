
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class SHMKRecord : JXCRecord
	{
		public SHMKRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		public void UpdateShipment(JASForwardingShipment shipment)
		{
			if (shipment != null)
			{
				shipment.JS_MarksAndNumbers = new ZString(ShippingMarksAndNumbers + " " + FreeTextDescription).Left(shipment.JS_MarksAndNumbersInfo.MaxLength);
			}
		}

		ZString ShippingMarksAndNumbers
		{
			get { return Fields.GetFieldValue(JXCConstants.SHMKFieldPositions.ShippingMarksAndNumbers); }
		}

		ZString FreeTextDescription
		{
			get { return Fields.GetFieldValue(JXCConstants.SHMKFieldPositions.FreeTextDescription); }
		}
	}
}
