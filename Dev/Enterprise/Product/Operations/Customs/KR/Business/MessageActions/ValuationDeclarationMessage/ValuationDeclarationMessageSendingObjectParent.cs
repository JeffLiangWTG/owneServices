using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public class ValuationDeclarationMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<ValuationDeclarationMessageSendingObject>,
		IJobDeclarationMessageSendingObjectParent
	{
		public ValuationDeclarationMessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override NonPersistentBusinessObjectCollection<ValuationDeclarationMessageSendingObject> GetSendingObjectsCollectionCore() => new ValuationDeclarationMessageSendingObjectCollection(ParentDeclaration);

		public IEnumerable<ValuationDeclarationMessageSendingObject> ObjectsToSend => SendingObjectsCollection.Cast<ValuationDeclarationMessageSendingObject>().Where(x => x.ShouldSend);

		#region IJobDeclarationMessageSendingObjectParent Members
		bool IJobDeclarationMessageSendingObjectParent.IsFaultPartyRelevant => throw new System.NotImplementedException();
		bool IJobDeclarationMessageSendingObjectParent.IsReasonCodeRelevant => throw new System.NotImplementedException();
		bool IJobDeclarationMessageSendingObjectParent.IsDateOfFinalPriceRelevant => throw new System.NotImplementedException();
		public string MessageType => ElectronicDocumentTypeList.Codes._934;

		MessageSender IJobDeclarationMessageSendingObjectParent.GetMessageSender(MessageFunctionCode messageFunctionCode)
		{
			return MessageSender.New(ObjectsToSend, Factory, MessageType, messageFunctionCode);
		}
		#endregion
	}
}
