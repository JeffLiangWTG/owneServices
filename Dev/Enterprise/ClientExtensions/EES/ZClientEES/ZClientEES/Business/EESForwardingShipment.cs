using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.EES.Business
{
	public class EESForwardingShipment : ForwardingShipment
	{
		#region Constructors and Type Overriding

		public EESForwardingShipment(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static new EESForwardingShipment New(BusinessObjectFactory factory)
		{
			return (EESForwardingShipment)factory.New(typeof(EESForwardingShipment));
		}

		#endregion

		#region DocumentSupporter
		public override DocumentSupporter DocumentSupporter
		{
			get
			{
				if (fEESShipmentDocumentSupporter == null)
				{
					fEESShipmentDocumentSupporter = new EESShipmentDocumentSupporter(this);
				}

				return fEESShipmentDocumentSupporter;
			}
		}
		EESShipmentDocumentSupporter fEESShipmentDocumentSupporter;
		#endregion
	}
}
