using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public class AgreedRateMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<AgreedRateMessageSendingObject>,
		IJobDeclarationMessageSendingObjectParent
	{
		public AgreedRateMessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration)
		{
		}
		public string MessageType => ElectronicDocumentTypeList.Codes._5BA;

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override NonPersistentBusinessObjectCollection<AgreedRateMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			return new AgreedRateMessageSendingObjectCollection(ParentDeclaration);
		}

		public IEnumerable<AgreedRateMessageSendingObject> ObjectsToSend => SendingObjectsCollection.Cast<AgreedRateMessageSendingObject>().Where(x => x.ShouldSend);

		#region IJobDeclarationMessageSendingObjectParent Members

		bool IsFaultPartyRelevant => throw new System.NotImplementedException();

		bool IsReasonCodeRelevant => throw new System.NotImplementedException();

		bool IsDateOfFinalPriceRelevant => throw new System.NotImplementedException();

		string IJobDeclarationMessageSendingObjectParent.MessageType => MessageType;

		bool IJobDeclarationMessageSendingObjectParent.IsFaultPartyRelevant => IsFaultPartyRelevant;

		bool IJobDeclarationMessageSendingObjectParent.IsReasonCodeRelevant => IsReasonCodeRelevant;

		bool IJobDeclarationMessageSendingObjectParent.IsDateOfFinalPriceRelevant => IsDateOfFinalPriceRelevant;

		MessageSender IJobDeclarationMessageSendingObjectParent.GetMessageSender(MessageFunctionCode messageFunctionCode)
		{
			return MessageSender.New(ObjectsToSend, Factory, MessageType, messageFunctionCode);
		}

		#endregion
	}
}
