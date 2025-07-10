using System.Collections;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class OceanMessageExporter : AirOceanMessageExporter
	{
		public OceanMessageExporter(JASForwardingConsol consol, INotifications notificationSubscriber)
			: base(consol, notificationSubscriber)
		{
		}

		public OceanMessageExporter(PreShipmentWrapper preShipmentWrapper, INotifications notificationSubscriber)
			: base(preShipmentWrapper, notificationSubscriber)
		{
		}

		protected override void AddMasterDetails(JASForwardingConsol consol, ArrayList lines)
		{
			lines.Add(new OMANLine(consol));
		}

		protected override void AddHouseDetailsFromShipment(IJXCExportHeader headerData, JASForwardingShipment shipment, ArrayList lines, HouseLevelRecordType houseBillOfLadingType)
		{
			ExportHouseBillOfLading houseBillOfLading = new ExportHouseBillOfLading(headerData, shipment);
			lines.Add(new OHBLLine(houseBillOfLadingType, headerData, houseBillOfLading));
			AddReferenceLines(lines, shipment);
			AddContainerLines(lines, headerData, shipment);
			AddChargesLines(lines, headerData, shipment);
			AddShipmentMarksAndNumbersLine(lines, shipment);
		}

		protected override ZString GetMessageFileNameFromConsolMasterBill(JASForwardingConsol consol, ZString suffix)
		{
			ZString result = consol.JK_MasterBillNum + suffix;
			if (result.IsEmpty)
			{
				result = consol.JK_UniqueConsignRef;
			}
			return result;
		}

		public override JXCExportValidationType ExportValidationTypeToUse
		{
			get { return JXCExportValidationType.Ocean; }
		}

		void AddContainerLines(ArrayList lines, IJXCExportHeader headerData, JASForwardingShipment shipment)
		{
			JASForwardingConsol consol = headerData as JASForwardingConsol;
			foreach (JASForwardingPackLine packLine in shipment.OuterPackLines)
			{
				ForwardingContainer container = (ForwardingContainer)((consol != null) ? packLine.GetContainer(consol) : packLine.GetContainer(shipment.Sailing));
				lines.Add(new CONTLine(container, packLine));
			}
		}

		void AddChargesLines(ArrayList lines, IJXCExportHeader headerData, JASForwardingShipment shipment)
		{
			JobCharge[] charges = shipment.GetCollectCharges(headerData.ReceivingForwarder);
			if (charges != null)
			{
				foreach (JobCharge jobCharge in charges)
				{
					lines.Add(new CHGSLine(jobCharge));
				}
			}
		}
	}
}
