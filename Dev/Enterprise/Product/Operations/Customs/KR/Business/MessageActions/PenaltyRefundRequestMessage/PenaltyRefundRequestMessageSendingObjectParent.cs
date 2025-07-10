using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class PenaltyRefundRequestMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<PenaltyRefundRequestMessageSendingObject>,
		IJobDeclarationMessageSendingObjectParent
	{
		public PenaltyRefundRequestMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		public string MessageType => ElectronicDocumentTypeList.Codes._5UL;

		bool IJobDeclarationMessageSendingObjectParent.IsFaultPartyRelevant => throw new System.NotImplementedException();
		bool IJobDeclarationMessageSendingObjectParent.IsReasonCodeRelevant => throw new System.NotImplementedException();
		bool IJobDeclarationMessageSendingObjectParent.IsDateOfFinalPriceRelevant => throw new System.NotImplementedException();

		public IEnumerable<PenaltyRefundRequestMessageSendingObject> ObjectsToSend => SendingObjectsCollection.Cast<PenaltyRefundRequestMessageSendingObject>().Where(x => x.ShouldSend);

		public MessageSender GetMessageSender(MessageFunctions.MessageFunctionCode messageFunctionCode)
		{
			return MessageSender.New(ObjectsToSend, Factory, MessageType, messageFunctionCode);
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;
		protected override NonPersistentBusinessObjectCollection<PenaltyRefundRequestMessageSendingObject> GetSendingObjectsCollectionCore() => new PenaltyRefundRequestMessageSendingObjectCollection(ParentDeclaration);
	}
}
