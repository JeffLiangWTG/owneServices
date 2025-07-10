using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationMiscMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<JobDeclarationMiscMessageSendingObject>,
		IJobDeclarationMessageSendingObjectParent,
		ISupportMultipleResourceStringData
	{
		public JobDeclarationMiscMessageSendingObjectParent(JobDeclaration declaration, string messageType, MessageFunctionCode functionCode)
			: base(declaration)
		{
			MessageType = messageType;
			this.functionCode = functionCode;
		}
		string MessageType { get; }
		readonly MessageFunctionCode functionCode;

		public bool IsCurrentDateAndNewDateRelevant => MessageType == ElectronicDocumentTypeList.Codes._5AS && functionCode == MessageFunctionCode.Extend;

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		public override bool HasAnyObjectToSend => ObjectsToSend.Any();

		protected override NonPersistentBusinessObjectCollection<JobDeclarationMiscMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			return new JobDeclarationMiscMessageSendingObjectCollection(ParentDeclaration, MessageType, functionCode);
		}

		public IEnumerable<JobDeclarationMiscMessageSendingObject> ObjectsToSend
		{
			get
			{
				if (MessageType == ElectronicDocumentTypeList.Codes._5FN)
				{
					return SendingObjectsCollection.Cast<JobDeclarationMiscMessageSendingObject>()
							.Where(x => x.MessageSendingEntryLines.Cast<MessageSendingEntryLineObject>().Any(x => x.ShouldSend));
				}

				return SendingObjectsCollection.Cast<JobDeclarationMiscMessageSendingObject>().Where(x => x.ShouldSend);
			}
		}

		#region IJobDeclarationMessageSendingObjectParent Members

		bool IJobDeclarationMessageSendingObjectParent.IsFaultPartyRelevant => MessageType == ElectronicDocumentTypeList.Codes._5AS || MessageType == ElectronicDocumentTypeList.Codes._DKJ;
		bool IJobDeclarationMessageSendingObjectParent.IsReasonCodeRelevant => MessageType == ElectronicDocumentTypeList.Codes._5AS || MessageType == ElectronicDocumentTypeList.Codes._DKJ;
		bool IJobDeclarationMessageSendingObjectParent.IsDateOfFinalPriceRelevant => false;

		string IJobDeclarationMessageSendingObjectParent.MessageType => MessageType;

		public static string GetMultipleCaptionKeys(string messageType, MessageFunctionCode functionCode) => messageType + "|" + functionCode.ToString();

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new string[] { ParentDeclaration.JE_MessageType, GetMultipleCaptionKeys(MessageType, functionCode) };

		MessageSender IJobDeclarationMessageSendingObjectParent.GetMessageSender(MessageFunctionCode messageFunctionCode)
		{
			return MessageSender.New(ObjectsToSend, Factory, MessageType, messageFunctionCode);
		}

		#endregion
	}
}
