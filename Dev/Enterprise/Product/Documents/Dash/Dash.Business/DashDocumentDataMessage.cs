using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Newtonsoft.Json;

namespace Enterprise.Dash.Business
{
	public class DashDocumentDataMessage : EDIMessage, ICanBeSavedByDocumentFactory
	{
		public DashDocumentDataMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public DashDocument LinkedDashDocument
		{
			get { return Factory.Load<DashDocument>(EM_LinkUniqueID); }
		}

		public DashDocumentDataMessageData MessageData
		{
			get
			{
				if (messageData == null && !EM_MessageData.IsEmpty)
				{
					var jsonText = MessageEncoding.UTF8WithoutBOM.GetString(EM_MessageData);
					messageData = JsonConvert.DeserializeObject<DashDocumentDataMessageData>(jsonText);
				}

				return messageData;
			}
			set
			{
				messageData = value;

				EM_MessageData = messageData == null
					? ZBlob.Empty
					: new ZBlob(MessageEncoding.UTF8WithoutBOM.GetBytes(JsonConvert.SerializeObject(messageData)));
			}
		}

		public override ZBlob EM_MessageData
		{
			get
			{
				return base.EM_MessageData;
			}
			set
			{
				base.EM_MessageData = value;
				messageData = null;
			}
		}

		DashDocumentDataMessageData messageData;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIInterchange.ApplicationCodes.DashDocumentDataProcessing;
			EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;
		}
	}
}
