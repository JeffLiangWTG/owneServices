using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSSynchronousResponseEDIMessage : CDSEDIMessage
	{
		public CDSSynchronousResponseEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.SynchronousResponse;
			EM_MessageSubType = CDSEDIMessageTypeList.Codes.ConversationID;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}

		public CDSEDIMessage OriginalOutgoingMessage
		{
			get
			{
				var correlationId = Interchange?.CCSUKProcessingInstructionHelper?.CorrelationId ?? ZGuid.Empty;

				if (correlationId == ZGuid.Empty)
				{
					correlationId = EHubTrackingId;
				}

				var query = new ZQuery(EDIMessageSchema.EM_EI, correlationId);
				return Factory.LoadTop1<CDSEDIMessage>(query);
			}
		}

		public SynchronousResponse MessageDataObject => messageDataObject ?? (messageDataObject = new SynchronousResponse(EM_MessageText));
		SynchronousResponse messageDataObject;
	}
}
