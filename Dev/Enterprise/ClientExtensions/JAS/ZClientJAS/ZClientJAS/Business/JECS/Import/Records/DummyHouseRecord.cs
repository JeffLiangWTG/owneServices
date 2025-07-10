
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Utilities;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class DummyHouseRecord : JXCRecord
	{
		public DummyHouseRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		public JASForwardingShipment LoadOrCreateShipment(JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			JASForwardingShipment result = null;

			if (consol != null)
			{
				ZString originUNLOCO = GetUNLOCOFromOfficeCode(consol.Factory, OriginOfficeCode);
				JASForwardingShipmentLocator locator = new JASForwardingShipmentLocator();
				result = locator.Find(consol, HouseBillNumber, TransportMode, originUNLOCO, "");
				if (result == null)
				{
					result = (JASForwardingShipment)consol.Shipments.AddNew();
					NotifyAttachingNewShipmentToConsol(consol, notificationSubscriber);
				}
				else
				{
					consol.Shipments.Add(result);
					NotifyAttachingExistingShipmentToConsol(consol, result, notificationSubscriber);
				}
			}

			return result;
		}

		public void UpdateShipment(JASForwardingShipment shipment, INotifications notificationSubscriber)
		{
			using (new DataImportFlagChanger(shipment))
			{
				shipment.JS_HouseBill = HouseBillNumber.Left(AutoJobShipment.Schema.JS_HouseBillMaxLength);
				if (shipment.JS_RL_NKOrigin.IsEmpty)
				{
					shipment.JS_RL_NKOrigin = GetUNLOCOFromOfficeCode(shipment.Factory, OriginOfficeCode);
				}
				shipment.JS_BookingReference = OriginTrafficFileNo.Left(AutoJobShipment.Schema.JS_BookingReferenceMaxLength);
			}
		}

		#region Implementation

		ZString OriginTrafficFileNo
		{
			get { return Fields.GetFieldValue(JXCConstants.DHABFieldPosition.OriginTrafficFileNo); }
		}

		ZString HouseBillNumber
		{
			get { return Fields.GetFieldValue(JXCConstants.DHABFieldPosition.HouseBillNumber); }
		}

		ZString OriginOfficeCode
		{
			get { return Fields.GetFieldValue(JXCConstants.DHABFieldPosition.OriginOfficeCode); }
		}

		ZString TransportMode
		{
			get { return LineType == JXCConstants.LineTypes.DHAB ? Core.Constants.TransportModes.Air : Core.Constants.TransportModes.Sea; }
		}

		#endregion
	}
}
