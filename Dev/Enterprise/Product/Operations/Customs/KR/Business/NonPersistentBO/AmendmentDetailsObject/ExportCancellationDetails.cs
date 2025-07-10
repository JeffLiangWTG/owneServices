using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class ExportCancellationDetails : NonPersistentBusinessObject, IVisualizerNoteSupporter
	{
		public ExportCancellationDetails(EDIMessage message)
			: base(message?.Factory)
		{
			if (message is null)
			{
				throw new ArgumentNullException(nameof(message), "Message");
			}
			this.message = message;
			entryPK = (message.EM_LinkedObject as CusEntryHeader).PK;
		}

		readonly EDIMessage message;
		readonly ZGuid entryPK;

		IGOVCBR5DTMessageData MessageData5DT
		{
			get
			{
				if (messageData5DT == null && entryPK.IsValid)
				{
					var message5DT = entryPK.GetIncomingMessage(Factory, message.EM_MessageNum, ElectronicDocumentTypeList.Codes._5DT);
					if (message5DT != null)
					{
						using var textReader = message5DT.GetEM_MessageTextReader();
						messageData5DT = new GOVCBR5DTDataProvider().GetMessageData(textReader);
					}
				}
				return messageData5DT;
			}
		}
		IGOVCBR5DTMessageData messageData5DT;

		public JobDeclarationMiscMessageSendingObject MessageSendingObjectDKJ
		{
			get
			{
				if (messageSendingObjectDKJ == null)
				{
					messageSendingObjectDKJ = new JobDeclarationMiscMessageSendingObject(message.EM_LinkedObject as CusEntryHeader, ElectronicDocumentTypeList.Codes._DKJ, MessageFunctions.MessageFunctionCode.Cancellation, x => true);
					if (message.EM_MessageType == ElectronicDocumentTypeList.Codes._DKJ)
					{
						messageSendingObjectDKJ.PopulateCancellationObject(message);
					}
				}
				return messageSendingObjectDKJ;
			}
		}
		JobDeclarationMiscMessageSendingObject messageSendingObjectDKJ;

		public ZString MessageOrEntryStatus => message.MessageOrEntryStatus;

		public ZString FaultParty => MessageSendingObjectDKJ.FaultParty;
		public ZString FaultPartyDescription => Factory.GetCachedValue<ExportImputationReasonCodeList>().GetDescriptionFromCode(FaultParty);
		public ZString ReasonCode => MessageSendingObjectDKJ.ReasonCode;
		public ZString ReasonCodeDescription => Factory.GetCachedValue<ExportDeclarationwithdrawReasonCodeList>().GetDescriptionFromCode(ReasonCode);
		public ZString AmendReasonDescription => MessageSendingObjectDKJ.AmendmentReason;
		public ZDateTime SubmissionDate => MessageSendingObjectDKJ.DateOfApplication;
		public ZDateTime DeclarationDate { get; set; }
		public ZDateTime ReleaseDate { get; set; }
		public OrganizationDocWrapper Declarant { get; set; }
		public OrganizationDocWrapper Supplier { get; set; }
		public ZDateTime AuthorisationDate => MessageData5DT?.DecisionDate ?? ZDate.Empty;
		public ZString AuthorisationNumber => MessageData5DT?.ApprovalNo ?? ZString.Empty;
		public ZString FormattedAuthorisationNumber => MessageFunctions.RequestDocumentNumber(AuthorisationNumber);
		public ZString CustomsOfficerID => MessageData5DT?.CustomsPersonID ?? ZString.Empty;
		public ZString CustomsOfficerName => MessageData5DT?.CustomsPersonName ?? ZString.Empty;

		#region IVisualizerNoteSupporter members
		ZGuid IVisualizerNoteSupporter.PK => entryPK;
		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;
		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;
		#endregion
	}
}
