using System.Data;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSPentantAcaMessage : CDSEDIMessage<MetaData>
	{
		public CDSPentantAcaMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.CDSPentantAcaMessage;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}
	}
}
