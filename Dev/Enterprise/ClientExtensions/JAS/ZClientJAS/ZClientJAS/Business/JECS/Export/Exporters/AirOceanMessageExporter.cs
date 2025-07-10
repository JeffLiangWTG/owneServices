using System;
using System.Collections;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public abstract class AirOceanMessageExporter : JXCMessageExporter
	{
		#region Factory Methods

		public static AirOceanMessageExporter New(JASForwardingConsol consol)
		{
			if (consol == null)
			{
				throw new ArgumentNullException(nameof(consol));
			}

			INotifications notificationSubscriber = new JXCExportLogger(consol);
			return (consol.IsAir)
					? new AirMessageExporter(consol, notificationSubscriber)
					: new OceanMessageExporter(consol, notificationSubscriber);
		}

		public static AirOceanMessageExporter New(PreShipmentWrapper preShipmentWrapper)
		{
			if (preShipmentWrapper == null || preShipmentWrapper.Shipment == null)
			{
				throw new ArgumentNullException(nameof(preShipmentWrapper), "Both preShipmentWrapper and preShipmentWrapper.Shipment cannot be null");
			}

			INotifications notificationSubscriber = new JXCExportLogger(preShipmentWrapper.Shipment);
			return (preShipmentWrapper.Shipment.IsAir)
					? new AirMessageExporter(preShipmentWrapper, notificationSubscriber)
					: new OceanMessageExporter(preShipmentWrapper, notificationSubscriber);
		}

		#endregion

		protected AirOceanMessageExporter(JASForwardingConsol consol, INotifications notificationSubscriber)
			: base(consol, notificationSubscriber)
		{
		}

		protected AirOceanMessageExporter(PreShipmentWrapper preShipmentWrapper, INotifications notificationSubscriber)
			: base(preShipmentWrapper, notificationSubscriber)
		{
		}

		#region GetMessageFileNamesAndContents

		protected override MessageFileNameAndContents[] GetMessageFileNamesAndContents()
		{
			MessageFileNameAndContents[] result;

			JASForwardingConsol consol = HeaderData as JASForwardingConsol;
			if (consol != null)
			{
				if (consol.JK_AgentType == Core.Constants.AgentType.CoLoad)
				{
					result = GetMessageFileNamesAndContentsForCoLoadConsol(consol);
				}
				else
				{
					result = GetMessageFileNamesAndContentsForConsol(consol);
				}
			}
			else
			{
				PreShipmentWrapper preShipment = HeaderData as PreShipmentWrapper;
				if (preShipment != null)
				{
					result = GetMessageFileNamesAndContentsForPreShipment(preShipment);
				}
				else
				{
					result = Array.Empty<MessageFileNameAndContents>();
				}
			}

			return result;
		}

		MessageFileNameAndContents[] GetMessageFileNamesAndContentsForCoLoadConsol(JASForwardingConsol consol)
		{
			MessageFileNameAndContents[] result = new MessageFileNameAndContents[consol.Shipments.Count];

			for (int i = 0; i < consol.Shipments.Count; i++)
			{
				JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments[i];

				ArrayList lines = new ArrayList();
				AddHouseDetailsFromCoLoadShipment(consol, shipment, lines);
				MessageLine[] messageLines = (MessageLine[])lines.ToArray(typeof(MessageLine));

				ZString fileName = GetMessageFileNameFromConsolMasterBill(consol, '_' + i.ToString());
				result[i] = new MessageFileNameAndContents(fileName, messageLines);
			}

			return result;
		}

		MessageFileNameAndContents[] GetMessageFileNamesAndContentsForConsol(JASForwardingConsol consol)
		{
			MessageFileNameAndContents[] result = new MessageFileNameAndContents[1];

			ArrayList lines = new ArrayList();
			AddMasterDetails(consol, lines);
			AddHouseDetails(consol, lines);
			MessageLine[] messageLines = (MessageLine[])lines.ToArray(typeof(MessageLine));

			ZString fileName = GetMessageFileNameFromConsolMasterBill(consol);
			result[0] = new MessageFileNameAndContents(fileName, messageLines);
			return result;
		}

		MessageFileNameAndContents[] GetMessageFileNamesAndContentsForPreShipment(PreShipmentWrapper preShipment)
		{
			MessageFileNameAndContents[] result;

			PreShipmentWrapper preShipmentWrapper = HeaderData as PreShipmentWrapper;
			if (preShipmentWrapper != null)
			{
				result = new MessageFileNameAndContents[1];

				ArrayList lines = new ArrayList();
				AddHouseDetailsFromPreShipment(preShipmentWrapper, lines);
				MessageLine[] messageLines = (MessageLine[])lines.ToArray(typeof(MessageLine));

				ZString fileName = GetMessageFileNameFromHouseBill(preShipmentWrapper.Shipment);
				result[0] = new MessageFileNameAndContents(fileName, messageLines);
			}
			else
			{
				result = Array.Empty<MessageFileNameAndContents>();
			}

			return result;
		}

		ZString GetMessageFileNameFromConsolMasterBill(JASForwardingConsol consol)
		{
			return GetMessageFileNameFromConsolMasterBill(consol, "");
		}

		protected abstract ZString GetMessageFileNameFromConsolMasterBill(JASForwardingConsol consol, ZString suffix);

		ZString GetMessageFileNameFromHouseBill(JASForwardingShipment shipment)
		{
			return shipment.JS_HouseBill;
		}

		#endregion

		#region House Details

		protected void AddReferenceLines(ArrayList lines, JASForwardingShipment shipment)
		{
			if (!shipment.JS_BookingReference.IsEmpty)
			{
				lines.Add(new REFRLine(shipment.JS_BookingReference, REFRLine.ReferenceFrom.Shipper));
			}
			lines.Add(new REFRLine(shipment.JS_HouseBill, REFRLine.ReferenceFrom.Consignee));
		}

		protected void AddShipmentMarksAndNumbersLine(ArrayList lines, JASForwardingShipment shipment)
		{
			if (!shipment.JS_MarksAndNumbers.IsEmpty)
			{
				lines.Add(new SHMKLine(shipment));
			}
		}

		void AddHouseDetailsFromCoLoadShipment(JASForwardingConsol consol, JASForwardingShipment shipment, ArrayList lines)
		{
			AddHouseDetailsFromShipment(consol, shipment, lines, HouseLevelRecordType.CoLoad);
		}

		void AddHouseDetailsFromShipment(JASForwardingConsol consol, JASForwardingShipment shipment, ArrayList lines)
		{
			AddHouseDetailsFromShipment(consol, shipment, lines, HouseLevelRecordType.Standard);
		}

		void AddHouseDetailsFromPreShipment(PreShipmentWrapper preShipment, ArrayList lines)
		{
			AddHouseDetailsFromShipment(preShipment, preShipment.Shipment, lines, HouseLevelRecordType.PreShipment);
		}

		void AddHouseDetails(JASForwardingConsol consol, ArrayList lines)
		{
			foreach (JASForwardingShipment shipment in consol.Shipments)
			{
				AddHouseDetailsFromShipment(consol, shipment, lines);
			}
		}

		protected abstract void AddHouseDetailsFromShipment(IJXCExportHeader headerData, JASForwardingShipment shipment, ArrayList lines, HouseLevelRecordType recordType);

		#endregion

		protected abstract void AddMasterDetails(JASForwardingConsol consol, ArrayList lines);
	}
}
