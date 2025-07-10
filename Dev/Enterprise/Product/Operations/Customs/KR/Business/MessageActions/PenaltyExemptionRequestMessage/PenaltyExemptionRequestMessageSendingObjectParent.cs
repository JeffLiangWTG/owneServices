using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public class PenaltyExemptionRequestMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<PenaltyExemptionRequestMessageSendingObject>,
		IJobDeclarationMessageSendingObjectParent
	{
		public PenaltyExemptionRequestMessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override NonPersistentBusinessObjectCollection<PenaltyExemptionRequestMessageSendingObject> GetSendingObjectsCollectionCore() => new PenaltyExemptionRequestMessageSendingObjectCollection(ParentDeclaration);

		public IEnumerable<PenaltyExemptionRequestMessageSendingObject> ObjectsToSend => SendingObjectsCollection.Cast<PenaltyExemptionRequestMessageSendingObject>().Where(x => x.ShouldSend);

		#region IJobDeclarationMessageSendingObjectParent Members
		bool IJobDeclarationMessageSendingObjectParent.IsFaultPartyRelevant => throw new System.NotImplementedException();
		bool IJobDeclarationMessageSendingObjectParent.IsReasonCodeRelevant => throw new System.NotImplementedException();
		bool IJobDeclarationMessageSendingObjectParent.IsDateOfFinalPriceRelevant => throw new System.NotImplementedException();
		public string MessageType => ElectronicDocumentTypeList.Codes._5UA;

		MessageSender IJobDeclarationMessageSendingObjectParent.GetMessageSender(MessageFunctionCode messageFunctionCode)
		{
			return new GOVCBR5UASender(ObjectsToSend, Factory);
		}
		#endregion
	}
}
