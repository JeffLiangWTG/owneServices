using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public class CancellationMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<CancellationMessageSendingObject>,
		IJobDeclarationMessageSendingObjectParent
	{
		public CancellationMessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration)
		{
		}
		public string MessageType => ElectronicDocumentTypeList.Codes._5BF;

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override NonPersistentBusinessObjectCollection<CancellationMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			return new CancellationMessageSendingObjectCollection(ParentDeclaration);
		}

		public IEnumerable<CancellationMessageSendingObject> ObjectsToSend => SendingObjectsCollection.Cast<CancellationMessageSendingObject>().Where(x => x.ShouldSend);

		#region IJobDeclarationMessageSendingObjectParent Members

		bool IJobDeclarationMessageSendingObjectParent.IsFaultPartyRelevant => throw new System.NotImplementedException();

		bool IJobDeclarationMessageSendingObjectParent.IsReasonCodeRelevant => throw new System.NotImplementedException();

		bool IJobDeclarationMessageSendingObjectParent.IsDateOfFinalPriceRelevant => throw new System.NotImplementedException();

		MessageSender IJobDeclarationMessageSendingObjectParent.GetMessageSender(MessageFunctionCode messageFunctionCode)
		{
			return MessageSender.New(ObjectsToSend, Factory, MessageType, messageFunctionCode);
		}

		#endregion
	}
}
