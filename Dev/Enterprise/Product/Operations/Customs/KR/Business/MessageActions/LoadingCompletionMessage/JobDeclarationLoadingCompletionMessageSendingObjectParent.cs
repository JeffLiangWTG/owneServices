using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationLoadingCompletionMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<JobDeclarationLoadingCompletionMessageSendingObject>,
		IJobDeclarationMessageSendingObjectParent
	{
		public JobDeclarationLoadingCompletionMessageSendingObjectParent(JobDeclaration declaration, string messageType)
			: base(declaration)
		{
			MessageType = messageType;
		}
		string MessageType { get; }

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override NonPersistentBusinessObjectCollection<JobDeclarationLoadingCompletionMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			return new JobDeclarationLoadingCompletionMessageSendingObjectCollection(ParentDeclaration, MessageType);
		}

		public IEnumerable<JobDeclarationLoadingCompletionMessageSendingObject> ObjectsToSend => SendingObjectsCollection.Cast<JobDeclarationLoadingCompletionMessageSendingObject>().Where(x => x.ShouldSend);

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
