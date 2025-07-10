using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.MFI
{
	public class MFIForwardingShipmentDocumentSupporter : ForwardingShipmentDocumentSupporter
	{
		public MFIForwardingShipmentDocumentSupporter(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		const string CarrierDocumentPack = "Carrier Document Pack";

		protected override OrgHeader GetShippingLine(ZString menuName)
		{
			OrgHeader result = null;
			if (menuName == CarrierDocumentPack)
			{
				BaseJobDeclaration jobDec = (BaseJobDeclaration)Shipment.DeclarationForDocuments;
				if (jobDec != null)
				{
					result = jobDec.ShippingLine;
				}
			}
			else
			{
				result = base.GetShippingLine(menuName);
			}
			return result;
		}

		protected override OrgAddress GetShippingLineAddress(ZString menuName)
		{
			OrgAddress result = null;
			if (menuName == CarrierDocumentPack)
			{
				result = ((BaseJobDeclaration)Shipment.DeclarationForDocuments)?.ShippingLine?.MainAddress;
			}
			else
			{
				result = base.GetShippingLineAddress(menuName);
			}

			return result;
		}

		#region CartageXmlExporter

		protected new MFIForwardingShipment Shipment
		{
			get { return (MFIForwardingShipment)base.Shipment; }
		}

		#endregion
	}
}
