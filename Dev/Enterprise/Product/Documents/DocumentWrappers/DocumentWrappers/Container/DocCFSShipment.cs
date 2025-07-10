using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocCFSShipment : DocumentWrapper
	{
		DocCFSShipment(NonPersistentCFSShipment shipment, BusinessObjectFactory factoryToWrap)
			: base(shipment, factoryToWrap)
		{
		}

		public static DocCFSShipment New(NonPersistentCFSShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			return (shipment != null) ? new DocCFSShipment(shipment, factoryToWrap) : null;
		}

		public override string ToString()
		{
			return "";
		}

		public ZString CFSContainerNumber
		{
			get { return Shipment.CFSContainerNumber; }
		}

		public ZString CFSVessel
		{
			get { return Shipment.CFSVessel; }
		}

		public ZString CFSVoyage
		{
			get { return Shipment.CFSVoyage; }
		}

		protected ZString fCFSPackageType;
		public ZString CFSPackageType
		{
			get { return fCFSPackageType; }
			set { fCFSPackageType = value; }
		}

		public ZString LabelConNoteHeading
		{
			get { return Shipment.LabelConNoteHeading; }
		}

		public ZString LabelConNote
		{
			get { return Shipment.LabelConNote; }
		}

		public ZString LabelConsigneeHeading
		{
			get { return Shipment.LabelConsigneeHeading; }
		}

		public ZString LabelConsignee
		{
			get { return Shipment.LabelConsignee; }
		}

		public ZString ClientName
		{
			get { return Shipment.ClientName; }
		}

		protected ZInt fTotalPackages;
		public ZInt TotalPackages
		{
			get { return fTotalPackages; }
			set { fTotalPackages = value; }
		}

		public ZString ETA
		{
			get { return Shipment.ETA; }
		}

		public ZString Dest
		{
			get { return Shipment.Dest; }
		}

		public ZString HBL
		{
			get { return Shipment.HBL; }
		}

		public ZString Marks
		{
			get { return Shipment.Marks; }
		}

		public ZString ShipmentNumber
		{
			get { return Shipment.ShipmentNumber; }
		}

		protected TextBarcode BarCode
		{
			get { return new TextBarcode(CFSContainerNumber + ShipmentNumber); }
		}

		public ZString BarCodeAs128Font
		{
			get { return BarCode.TextAs128sFontString; }
		}

		protected NonPersistentCFSShipment Shipment
		{
			get { return (NonPersistentCFSShipment)WrappedObject; }
		}

		public bool IsTranshipment(ZString dischargePort)
		{
			return !Dest.IsEmpty && !dischargePort.IsEmpty
				&& Dest.SubstringSafe(0, 2) != dischargePort.SubstringSafe(0, 2);
		}

		public bool IsOnForwarding(ZString dischargePort)
		{
			return !Dest.IsEmpty && !dischargePort.IsEmpty
				&& Dest != dischargePort
				&& Dest.SubstringSafe(0, 2) == dischargePort.SubstringSafe(0, 2);
		}

		public bool IsImport(ZString dischargePort)
		{
			return !Dest.IsEmpty && !dischargePort.IsEmpty
				&& Dest.SubstringSafe(0, 2) == dischargePort.SubstringSafe(0, 2);
		}
	}

	public class NonPersistentCFSShipment : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZString ShipmentNumber;
		public ZString ClientName;
		public ZInt TotalPackages;
		public ZString CFSPackageType;
		public ZString ETA;
		public ZString Dest;
		public ZString HBL;
		public ZString Marks;
		public ZString CFSContainerNumber;
		public ZString CFSVessel;
		public ZString CFSVoyage;
		public ZString LabelConNoteHeading;
		public ZString LabelConNote;
		public ZString LabelConsigneeHeading;
		public ZString LabelConsignee;
		new public static ZString TableName
		{
			get { return "CFSShipment"; }
		}
	}
}
