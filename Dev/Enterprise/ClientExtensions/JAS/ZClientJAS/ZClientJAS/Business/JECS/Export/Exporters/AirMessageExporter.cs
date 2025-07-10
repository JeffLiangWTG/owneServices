using System.Collections;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class AirMessageExporter : AirOceanMessageExporter
	{
		public AirMessageExporter(JASForwardingConsol consol, INotifications notificationSubscriber)
			: base(consol, notificationSubscriber)
		{
		}

		public AirMessageExporter(PreShipmentWrapper preShipmentWrapper, INotifications notificationSubscriber)
			: base(preShipmentWrapper, notificationSubscriber)
		{
		}

		protected override void AddMasterDetails(JASForwardingConsol consol, ArrayList lines)
		{
			ConsolExportAWBHeader consolAWBHeader = consol.AWBHeader as ConsolExportAWBHeader;
			lines.Add(new MAWBLine(consolAWBHeader));
			AddOtherChargesLines(lines, consolAWBHeader);
			AddFreightBreakDownLines(lines, consolAWBHeader);
		}

		protected override void AddHouseDetailsFromShipment(IJXCExportHeader headerData, JASForwardingShipment shipment, ArrayList lines, HouseLevelRecordType hAWBType)
		{
			ShipmentExportAWBHeader shipmentAWBHeader = shipment.AWBHeader as ShipmentExportAWBHeader;
			if (shipmentAWBHeader != null)
			{
				lines.Add(new HAWBLine(headerData, shipmentAWBHeader, hAWBType));
				AddReferenceLines(lines, shipment);
				AddFreightBreakDownLines(lines, shipmentAWBHeader);
				AddOtherChargesLines(lines, shipmentAWBHeader);
			}
			AddShipmentMarksAndNumbersLine(lines, shipment);
		}

		protected override ZString GetMessageFileNameFromConsolMasterBill(JASForwardingConsol consol, ZString suffix)
		{
			return consol.MasterBillMAWB + suffix + '.' + consol.MasterBillAirlinePrefix;
		}

		public override JXCExportValidationType ExportValidationTypeToUse
		{
			get { return JXCExportValidationType.Air; }
		}

		void AddOtherChargesLines(ArrayList lines, ExportAWBHeader aWBHeader)
		{
			foreach (ExportAWBOtherCharges aWBOtherCharges in aWBHeader.AWBOtherCharges)
			{
				lines.Add(new OTHRLine(aWBHeader, aWBOtherCharges));
			}
		}

		void AddFreightBreakDownLines(ArrayList list, ExportAWBHeader aWBHeader)
		{
			foreach (ExportAWBRateLine rateLine in aWBHeader.AWBRateLines)
			{
				if (!rateLine.IsRateDescriptionEmpty)
				{
					list.Add(new FBDNLine(aWBHeader, rateLine));
				}
			}
		}
	}
}

#region PreShipment
#endregion
#region Standard Consol
#endregion
#region Co-Load Consol
#endregion
