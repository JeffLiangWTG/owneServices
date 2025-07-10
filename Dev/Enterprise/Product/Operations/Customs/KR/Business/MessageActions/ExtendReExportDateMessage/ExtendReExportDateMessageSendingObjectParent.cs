using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public class ExtendReExportDateMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<ExtendReExportDateMessageSendingObject>,
		IJobDeclarationMessageSendingObjectParent
	{
		public ExtendReExportDateMessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override NonPersistentBusinessObjectCollection<ExtendReExportDateMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			return new ExtendReExportDateMessageSendingObjectCollection(ParentDeclaration);
		}

		public IEnumerable<ExtendReExportDateMessageSendingObject> ObjectsToSend => SendingObjectsCollection.Cast<ExtendReExportDateMessageSendingObject>();

		#region IJobDeclarationMessageSendingObjectParent Members

		bool IJobDeclarationMessageSendingObjectParent.IsFaultPartyRelevant => throw new System.NotImplementedException();
		bool IJobDeclarationMessageSendingObjectParent.IsReasonCodeRelevant => throw new System.NotImplementedException();
		bool IJobDeclarationMessageSendingObjectParent.IsDateOfFinalPriceRelevant => throw new System.NotImplementedException();

		public string MessageType => ElectronicDocumentTypeList.Codes._D72;

		MessageSender IJobDeclarationMessageSendingObjectParent.GetMessageSender(MessageFunctionCode messageFunctionCode)
		{
			return MessageSender.New(ObjectsToSend, Factory, MessageType, messageFunctionCode);
		}

		[ResourceStringData("E1362630-FEFB-482C-A030-2A9F02F21844", Caption = "Sending Objects Collection")]
		public new NonPersistentBusinessObjectCollection<ExtendReExportDateMessageSendingObject> SendingObjectsCollection => base.SendingObjectsCollection;

		#endregion
	}
}
