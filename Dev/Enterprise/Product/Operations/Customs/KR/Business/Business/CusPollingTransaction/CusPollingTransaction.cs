using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class CusPollingTransaction : Customs.Business.CusPollingTransaction,
		Integration.Customs.KR.ICusPollingTransaction,
		IEDIMessageCollectionProviderWithID
	{
		public CusPollingTransaction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPT_ApplicationCode = ApplicationCodes.KRCustoms;
		}

		public EDIMessage ParentDLTMessage => Factory.Load(CPT_ParentTableCode, CPT_ParentID) as EDIMessage;

		ZString IEDIMessageCollectionProviderWithID.IDNumber => CPT_TransactionID;

		[ChildEditable(true)]
		public Enterprise.Messaging.Business.EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this);
					messages.Load();
					RegisterEditableChildObject(messages);
				}
				return messages;
			}
		}
		EDIMessageCollection messages;
		void IEDIMessageCollectionProviderWithID.MarkAsFailed()
		{
			CPT_Status = CusPollingTransactionStatusList.Codes.Error;
		}

		public EDIMessage DOCMessage
		{
			get
			{
				if (docMessage?.IsDeleted ?? true)
				{
					docMessage = Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == EDIInterchangeType.DOC);
				}
				return docMessage;
			}
		}
		EDIMessage docMessage;

		public EDIMessage RCVMessage
		{
			get
			{
				if (rcvMessage?.IsDeleted ?? true)
				{
					if (DOCMessage != null && DOCMessage.Interchange != null)
					{
						var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, DOCMessage.Interchange.EI_SessionGUID);
						interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodes.KRCustoms);
						interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
						var rcvInterchange = Factory.LoadTop1<EDIInterchange>(interchangeQuery);
						if (rcvInterchange != null && rcvInterchange.ContainedMessages.Count > 0)
						{
							rcvMessage = (EDIMessage)rcvInterchange.ContainedMessages[0];
						}
					}
				}
				return rcvMessage;
			}
		}
		EDIMessage rcvMessage;
	}
}
