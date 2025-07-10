using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using MessageSender = Enterprise.Customs.IE.Business.MessageSender;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class DocumentSendingAction : Customs.Business.BaseMessageSendingObject, IMessageSendingAction
	{
		public DocumentSendingAction(NctsHeader header) : base(header.Factory)
		{
			nctsHeader = header;
		}
		internal NctsHeader nctsHeader;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldSend = true;
		}

		public MessageSender CreateSender() => (MessageSender)Activator.CreateInstance(typeof(DocumentsSender), this);

		public ZString MessageType { get; set; }

		public IMessageAttachee MessageAttachee => nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader : nctsHeader;

		public void AddMessage(OutboundEDIMessage message)
		{
			if (nctsHeader.IsDepartureMovement)
			{
				nctsHeader.MovementHeader.Messages.Add(message);
			}
			else
			{
				nctsHeader.Messages.Add(message);
			}
		}

		AdditionalInfoSendingObjectCollection fAddInfoCollection;
		public AdditionalInfoSendingObjectCollection AddInfoCollection
		{
			get
			{
				if (fAddInfoCollection == null)
				{
					fAddInfoCollection = new AdditionalInfoSendingObjectCollection(nctsHeader);
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
					fSupportingDocuments = new DocumentSendingObjectCollection(nctsHeader);
					RegisterEditableChildObject(fSupportingDocuments);
				}
				return fSupportingDocuments;
			}
		}

		public DocumentSendingActionLookups Lookups => GetNewLookups();

		public DocumentSendingActionValidation Validation => GetNewValidation();

		[ResourceStringData("0248D5DB-9108-4DE0-8120-50E6255BA828", Caption = "Movement Reference Number", ShortCaption = "MRN")]
		public ZString MovementReference => nctsHeader.MovementReferenceNumber;

		public ZPropertyInfo MovementReferenceInfo => GetZPropertyInfo(nameof(MovementReference));

		[ResourceStringData("2C5C5CA5-806F-41CA-9764-38E3B7B23EF5", Caption = "Local Reference Number", ShortCaption = "LRN")]
		public ZString LocalReference => nctsHeader.LocalReferenceNumberForDisplay;
		public ZPropertyInfo LocalReferenceInfo => GetZPropertyInfo(nameof(LocalReference));

		protected DocumentSendingActionValidation GetNewValidation() => new DocumentSendingActionValidation(this);

		protected DocumentSendingActionLookups GetNewLookups() => new DocumentSendingActionLookups(this);
	}
}
