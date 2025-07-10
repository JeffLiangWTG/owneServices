using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.ICS
{
	public class IcsNorthernIrelandEDIMessage : GbEDIMessage
	{
		public IcsNorthernIrelandEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland;
		}

		public AsycudaManifestHeader GetManifestUsingICSCorrelationId()
		{
			AsycudaManifestHeader manifestHeader = null;
			if (!EM_ApplicationReference.IsEmpty)
			{
				var correlationID = EM_ApplicationReference;
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationReference, correlationID);
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbMessageICSNorthernIreland);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				var icsMessage = Factory.LoadTop1<IcsNorthernIrelandEDIMessage>(query);
				manifestHeader = icsMessage?.EM_LinkedObject as AsycudaManifestHeader;
			}
			return manifestHeader;
		}
	}
}
