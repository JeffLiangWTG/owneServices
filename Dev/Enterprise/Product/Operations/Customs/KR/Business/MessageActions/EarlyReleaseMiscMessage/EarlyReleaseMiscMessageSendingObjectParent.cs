using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public class EarlyReleaseMiscMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<EarlyReleaseMiscMessageSendingObject>,
		IJobDeclarationMessageSendingObjectParent
	{
		public EarlyReleaseMiscMessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override NonPersistentBusinessObjectCollection<EarlyReleaseMiscMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			return new EarlyReleaseMiscMessageSendingObjectCollection(ParentDeclaration);
		}

		public IEnumerable<EarlyReleaseMiscMessageSendingObject> ObjectsToSend => SendingObjectsCollection.Cast<EarlyReleaseMiscMessageSendingObject>().Where(x => x.ShouldSend);

		#region IJobDeclarationMessageSendingObjectParent Members

		public bool IsFaultPartyRelevant => throw new System.NotImplementedException();

		public bool IsReasonCodeRelevant => throw new System.NotImplementedException();

		public bool IsDateOfFinalPriceRelevant => throw new System.NotImplementedException();

		public string MessageType => ElectronicDocumentTypeList.Codes._5BD;

		MessageSender IJobDeclarationMessageSendingObjectParent.GetMessageSender(MessageFunctionCode messageFunctionCode)
		{
			return MessageSender.New(ObjectsToSend, Factory, MessageType, messageFunctionCode);
		}

		#endregion
	}
}
