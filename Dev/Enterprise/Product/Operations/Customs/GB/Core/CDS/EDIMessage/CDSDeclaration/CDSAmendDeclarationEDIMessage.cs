using System.Data;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSAmendDeclarationEDIMessage : CDSEDIMessage<MetaData>
	{
		public CDSAmendDeclarationEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.AmendDeclaration;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}

		protected JobDeclarationMessageSendingObject SendingObject { get; set; }
	}
}
