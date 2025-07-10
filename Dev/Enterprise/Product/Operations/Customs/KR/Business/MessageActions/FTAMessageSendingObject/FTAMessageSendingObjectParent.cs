using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public class FTAMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<FTAMessageSendingObject>,
		IJobDeclarationMessageSendingObjectParent
	{
		public FTAMessageSendingObjectParent(JobDeclaration declaration, ZString messageType)
			: base(declaration)
		{
			this.messageType = messageType;
		}
		readonly ZString messageType;

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override NonPersistentBusinessObjectCollection<FTAMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			return new FTAMessageSendingObjectCollection(ParentDeclaration, messageType);
		}

		public IEnumerable<FTAMessageSendingObject> ObjectsToSend => SendingObjectsCollection.Cast<FTAMessageSendingObject>().Where(x => x.ShouldSend);

		[ResourceStringData("166C0B02-7799-49FF-90BB-68BDCB8E0F7F", Caption = "Sending Objects Collection")]
		public new NonPersistentBusinessObjectCollection<FTAMessageSendingObject> SendingObjectsCollection => base.SendingObjectsCollection;

		#region IJobDeclarationMessageSendingObjectParent Members

		bool IsFaultPartyRelevant => throw new System.NotImplementedException();

		bool IsReasonCodeRelevant => throw new System.NotImplementedException();

		bool IsDateOfFinalPriceRelevant => throw new System.NotImplementedException();

		string IJobDeclarationMessageSendingObjectParent.MessageType => messageType;

		bool IJobDeclarationMessageSendingObjectParent.IsFaultPartyRelevant => IsFaultPartyRelevant;

		bool IJobDeclarationMessageSendingObjectParent.IsReasonCodeRelevant => IsReasonCodeRelevant;

		bool IJobDeclarationMessageSendingObjectParent.IsDateOfFinalPriceRelevant => IsDateOfFinalPriceRelevant;

		MessageSender IJobDeclarationMessageSendingObjectParent.GetMessageSender(MessageFunctionCode messageFunctionCode)
		{
			return MessageSender.New(ObjectsToSend, Factory, messageType, messageFunctionCode);
		}

		#endregion
	}
}
