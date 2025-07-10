using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.MFI
{
	public class MFIForwardingShipment : ForwardingShipment
	{
		public MFIForwardingShipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new MFIForwardingShipment New(BusinessObjectFactory factory)
		{
			return (MFIForwardingShipment)factory.New(typeof(MFIForwardingShipment));
		}

		public override DocumentSupporter DocumentSupporter
		{
			get { return new MFIForwardingShipmentDocumentSupporter(this); }
		}

		#region New Properties

		public CommonConsol Consol
		{
			get { return Consols.Count == 1 ? Consols[0] : FindCorrectConsol(); }
		}

		#endregion
	}
}
