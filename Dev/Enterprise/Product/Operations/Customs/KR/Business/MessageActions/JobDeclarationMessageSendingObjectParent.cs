using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationMessageSendingObjectParent : Customs.Business.JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>,
		IJobDeclarationMessageSendingObjectParent, ISupportMultipleResourceStringData
	{
		public JobDeclarationMessageSendingObjectParent(JobDeclaration declaration, string messageType)
			: base(declaration)
		{
			MessageType = messageType;
		}

		public static IJobDeclarationMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParent(JobDeclaration declaration, ZString messageType, MessageFunctionCode messageFunctionCode)
		{
			declaration.SetValidationModeOnElectronicMessaging(messageType);
			switch (messageType)
			{
				case ElectronicDocumentTypeList.Codes._5BD:
					return new EarlyReleaseMiscMessageSendingObjectParent(declaration);
				case ElectronicDocumentTypeList.Codes._5BA:
					return new AgreedRateMessageSendingObjectParent(declaration);
				case ElectronicDocumentTypeList.Codes._5SI:
					return new MailItemIDsMessageSendingObjectParent(declaration);
				case ElectronicDocumentTypeList.Codes._5UA:
					return new PenaltyExemptionRequestMessageSendingObjectParent(declaration);
				case ElectronicDocumentTypeList.Codes._5UL:
					return new PenaltyRefundRequestMessageSendingObjectParent(declaration);
				case ElectronicDocumentTypeList.Codes._934:
					return new ValuationDeclarationMessageSendingObjectParent(declaration);
				case ElectronicDocumentTypeList.Codes._D72:
					return new ExtendReExportDateMessageSendingObjectParent(declaration);
				case ElectronicDocumentTypeList.Codes._5BF:
					return new CancellationMessageSendingObjectParent(declaration);
				case ElectronicDocumentTypeList.Codes._DF3:
					return new JobDeclarationLoadingCompletionMessageSendingObjectParent(declaration, messageType);
				case ElectronicDocumentTypeList.Codes._DHR:
				case ElectronicDocumentTypeList.Codes._5SC:
					return new FTAMessageSendingObjectParent(declaration, messageType);
				default:
					if (ElectronicDocumentTypeList.IsAmendment(messageType) && messageFunctionCode == MessageFunctionCode.Amendment)
					{
						return new JobDeclarationAmendmentMessageSendingObjectParent(declaration, messageType);
					}
					else if (IsSendingMiscMessage(messageType, messageFunctionCode))
					{
						return new JobDeclarationMiscMessageSendingObjectParent(declaration, messageType, messageFunctionCode);
					}
					else
					{
						return new JobDeclarationMessageSendingObjectParent(declaration, messageType);
					}
			}
		}

		public static bool IsSendingMiscMessage(string messageType, MessageFunctionCode functionCode)
		{
			return (!ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(messageType) && functionCode != MessageFunctionCode.Amendment) || messageType == ElectronicDocumentTypeList.Codes._5TM || messageType == ElectronicDocumentTypeList.Codes._5FN;
		}

		string MessageType { get; }

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			return new JobDeclarationMessageSendingObjectCollection(ParentDeclaration, MessageType);
		}

		public IEnumerable<JobDeclarationMessageSendingObject> ObjectsToSend => SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().Where(x => x.ShouldSend);
		public IReadOnlyList<string> MultipleKeysToUse => new string[] { ParentDeclaration?.JE_MessageType };

		#region IJobDeclarationMessageSendingObjectParent Members

		bool IJobDeclarationMessageSendingObjectParent.IsFaultPartyRelevant => false;
		bool IJobDeclarationMessageSendingObjectParent.IsReasonCodeRelevant => false;
		bool IJobDeclarationMessageSendingObjectParent.IsDateOfFinalPriceRelevant => false;

		string IJobDeclarationMessageSendingObjectParent.MessageType => MessageType;

		MessageSender IJobDeclarationMessageSendingObjectParent.GetMessageSender(MessageFunctionCode messageFunctionCode)
		{
			return MessageSender.New(ObjectsToSend, Factory, MessageType, messageFunctionCode);
		}

		#endregion
	}
}
