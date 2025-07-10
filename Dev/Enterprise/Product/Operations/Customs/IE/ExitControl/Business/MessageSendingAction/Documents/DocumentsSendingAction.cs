using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class DocumentsSendingAction : Customs.Business.BaseMessageSendingObject, IExitControlMessageSendingAction
	{
		public DocumentsSendingAction(CusExitReport cusExitReport) : base(cusExitReport.Factory)
		{
			MessagingObject = Argument.NotNull(cusExitReport, nameof(cusExitReport));
		}

		public CusExitReport MessagingObject { get; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldSend = true;
		}

		AdditionalInfoSendingObjectCollection fAddInfoCollection;
		public AdditionalInfoSendingObjectCollection AddInfoCollection
		{
			get
			{
				if (fAddInfoCollection == null)
				{
					fAddInfoCollection = new AdditionalInfoSendingObjectCollection();
					RegisterEditableChildObject(fAddInfoCollection);
				}
				return fAddInfoCollection;
			}
		}

		DocumentSendingObjectCollection fSupportingDocuments;
		public DocumentSendingObjectCollection SupportingDocuments
		{
			get
			{
				if (fSupportingDocuments == null)
				{
					fSupportingDocuments = new DocumentSendingObjectCollection(MessagingObject);
					RegisterEditableChildObject(fSupportingDocuments);
				}
				return fSupportingDocuments;
			}
		}

		[ResourceStringData("4E716D4D-600B-4844-B975-D5D5E723B61F", Caption = "Movement Reference Number", ShortCaption = "MRN")]
		public ZString MovementReference => MessagingObject.Consignment?.CXC_MovementReference ?? ZString.Empty;
		public ZPropertyInfo MovementReferenceInfo => GetZPropertyInfo(nameof(MovementReference));

		[ResourceStringData("C63A590A-DE21-42B4-89AD-EBA0B3B3F4B1", Caption = "Local Reference Number", ShortCaption = "LRN")]
		public ZString LocalReference => MessagingObject.Consignment?.CXC_UniqueConsignmentReference ?? ZString.Empty;
		public ZPropertyInfo LocalReferenceInfo => GetZPropertyInfo(nameof(LocalReference));

		#region Fields

		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				base.ShouldSend = value;
				UpdateValidationModes(value);
			}
		}

		protected void UpdateValidationModes(bool isSpecificModeEnabled)
		{
			if (MessagingObject != null)
			{
				MessagingObject.ValidationModesCalculator.UpdateValidationModes(GetApplicableValidationModes(), isSpecificModeEnabled);
			}
		}
		protected virtual EU.Business.Declaration.ValidationModes GetApplicableValidationModes() => EU.Business.Declaration.ValidationModes.None;

		[ResourceStringData("749E6745-D4DF-4E21-8275-F8C2DE375A4A", Caption = "Message Type")]
		[List(nameof(Lookups) + "." + nameof(DocumentsSendingActionLookups.SendingActionTypeList))]
		[MaxLength(EDIMessage.Schema.EM_MessageTypeMaxLength)]
		public ZString MessageType
		{
			get => fMessageType;
			set
			{
				if (SetNonPersistentPropertyValue(MessageTypeInfo, ref fMessageType, value))
				{
					UpdateValidationModes(ShouldSend);
				}
			}
		}
		ZString fMessageType;

		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(nameof(MessageType));

		#endregion

		#region Lookups & Validations

		protected DocumentsSendingActionLookups GetNewLookups() => new DocumentsSendingActionLookups(this);
		public DocumentsSendingActionLookups Lookups => GetNewLookups();

		protected DocumentsSendingActionValidation GetNewValidation() => new DocumentsSendingActionValidation(this);

		public DocumentsSendingActionValidation Validation => GetNewValidation();

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region Message Sender

		public DocumentsSender CreateSender() => (DocumentsSender)Activator.CreateInstance(SenderType, this);

		protected Type SenderType => typeof(DocumentsSender);

		#endregion

		#region IMessageSendingAction Members
		IMessageAttachee IMessageSendingAction.MessageAttachee => MessagingObject;

		void IMessageSendingAction.AddMessage(OutboundEDIMessage message)
		{
			MessagingObject.Messages.Add(message);
		}
		#endregion
	}
}
