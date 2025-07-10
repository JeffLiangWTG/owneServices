using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.SCP
{
	public class DocSkyLiftAWB : DocAWB
	{
		#region Constructors and Type Overriding

		protected DocSkyLiftAWB(ExportAWBHeader exportAWBHeader, BusinessObjectFactory factoryToWrap) : base(exportAWBHeader, factoryToWrap)
		{
		}

		public new static DocAWB New(ExportAWBHeader exportAWBHeader, BusinessObjectFactory factoryToWrap)
		{
			if (exportAWBHeader == null)
			{
				return null;
			}
			else
			{
				return new DocSkyLiftAWB(exportAWBHeader, factoryToWrap);
			}
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		#region AccountingInformation Overrides
		public override ZString AccountingInformation1
		{
			get { return GetAccountingInformationForCount(1); }
		}

		public override ZString AccountingInformation2
		{
			get { return GetAccountingInformationForCount(2); }
		}

		public override CargoWise.Types.ZString AccountingInformation3
		{
			get { return GetAccountingInformationForCount(3); }
		}

		public override CargoWise.Types.ZString AccountingInformation4
		{
			get { return GetAccountingInformationForCount(4); }
		}

		public override CargoWise.Types.ZString AccountingInformation5
		{
			get { return GetAccountingInformationForCount(5); }
		}

		protected ZString GetAccountingInformationForCount(ZInt lineNo)
		{
			if (ExportAWBHeader.AWBAccountingInformations.Count > 0)
			{
				if (ExportAWBHeader.AWBAccountingInformations.Count >= lineNo)
				{
					return (ExportAWBHeader.AWBAccountingInformations[lineNo - 1].EA_InformationID.Length > 0 ?
						ExportAWBHeader.AWBAccountingInformations[lineNo - 1].EA_InformationID + " " : "") +
						ExportAWBHeader.AWBAccountingInformations[lineNo - 1].EA_Information;
				}
			}
			return ZString.Empty;
		}

		#endregion

		public ZString AlsoNotify
		{
			get
			{
				ZString result = AlsoNotifyNameAndAddress;
				if (ExportAWBHeader.EH_IsNotifyOverriden)
				{
					result	= (ExportAWBHeader.EH_NotifyOverride1.Length > 0) ? ExportAWBHeader.EH_NotifyOverride1 + "\n" : "";
					result += (ExportAWBHeader.EH_NotifyOverride2.Length > 0) ? ExportAWBHeader.EH_NotifyOverride2 + "\n" : "";
					result += (ExportAWBHeader.EH_NotifyOverride3.Length > 0) ? ExportAWBHeader.EH_NotifyOverride3 + "\n" : "";
					result += (ExportAWBHeader.EH_NotifyOverride4.Length > 0) ? ExportAWBHeader.EH_NotifyOverride4 + "\n" : "";
					result += (ExportAWBHeader.EH_NotifyOverride5.Length > 0) ? ExportAWBHeader.EH_NotifyOverride5 : ZString.Empty;
				}
				return result.TrimEndIncludingWhiteSpace('\r').ToUpper();
			}
		}

		public override ZString ConsolNumber
		{
			get
			{
				if (ExportAWBHeader is ShipmentExportAWBHeader)
				{
					ShipmentExportAWBHeader shipmentAWBHeader = (ShipmentExportAWBHeader)ExportAWBHeader;
					ForwardingShipment shipment = shipmentAWBHeader.Shipment;
					if (shipment != null)
					{
						DocShipment shipmentWrapper = DocForwardingShipment.New(shipment, Factory);
						if (shipmentWrapper != null && shipmentWrapper.Consol != null)
						{
							return shipmentWrapper.Consol.ConsolNumber;
						}
					}
				}
				return ZString.Empty;
			}
		}

		public ZString ShipmentNumber
		{
			get
			{
				if (ExportAWBHeader is ShipmentExportAWBHeader)
				{
					ShipmentExportAWBHeader shipmentAWBHeader = (ShipmentExportAWBHeader)ExportAWBHeader;
					ForwardingShipment shipment = shipmentAWBHeader.Shipment;
					if (shipment != null)
					{
						return shipment.JS_UniqueConsignRef;
					}
				}
				return ZString.Empty;
			}
		}
	}
}
