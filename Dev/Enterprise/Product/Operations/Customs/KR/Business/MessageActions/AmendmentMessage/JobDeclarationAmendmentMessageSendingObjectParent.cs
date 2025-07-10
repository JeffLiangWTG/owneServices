using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationAmendmentMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<JobDeclarationAmendmentMessageSendingObject>,
		IJobDeclarationMessageSendingObjectParent,
		ISupportMultipleResourceStringData
	{
		public JobDeclarationAmendmentMessageSendingObjectParent(JobDeclaration declaration, string messageType)
			: base(declaration)
		{
			MessageType = messageType;
		}
		string MessageType { get; }

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override NonPersistentBusinessObjectCollection<JobDeclarationAmendmentMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			return new JobDeclarationAmendmentMessageSendingObjectCollection(ParentDeclaration, MessageType);
		}

		public IEnumerable<JobDeclarationAmendmentMessageSendingObject> ObjectsToSend => SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>().Where(x => x.ShouldSend);

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new string[] { ParentDeclaration.JE_MessageType };

		[ResourceStringData("87D50493-7F6C-4972-86E5-1E478A416F7C", Caption = "Sending Objects Collection")]
		public new NonPersistentBusinessObjectCollection<JobDeclarationAmendmentMessageSendingObject> SendingObjectsCollection => base.SendingObjectsCollection;

		#region IJobDeclarationMessageSendingObjectParent Members

		bool IJobDeclarationMessageSendingObjectParent.IsFaultPartyRelevant => MessageType == ElectronicDocumentTypeList.Codes._5AS;
		bool IJobDeclarationMessageSendingObjectParent.IsReasonCodeRelevant => MessageType == ElectronicDocumentTypeList.Codes._5AS;
		bool IJobDeclarationMessageSendingObjectParent.IsDateOfFinalPriceRelevant => MessageType == ElectronicDocumentTypeList.Codes._5AS;

		string IJobDeclarationMessageSendingObjectParent.MessageType => MessageType;

		MessageSender IJobDeclarationMessageSendingObjectParent.GetMessageSender(MessageFunctionCode messageFunctionCode)
		{
			return MessageSender.New(ObjectsToSend, Factory, MessageType, messageFunctionCode);
		}

		#endregion
	}
}
