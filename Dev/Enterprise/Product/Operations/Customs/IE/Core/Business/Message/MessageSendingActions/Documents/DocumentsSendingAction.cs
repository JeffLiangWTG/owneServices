using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class DocumentsSendingAction : CusEntryHeaderMessageSendingAction, IObsoleteValidation
	{
		public DocumentsSendingAction(CusEntryHeader cusEntryHeader) : base(cusEntryHeader)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldSend = true;
		}

		protected override Type SenderType => typeof(DocumentsSender);

		protected override CusEntryHeaderMessageSendingActionLookups GetNewLookups() => new DocumentsSendingActionLookups(this);

		protected override CusEntryHeaderMessageSendingActionValidation GetNewValidation() => new DocumentsSendingActionValidation(this);

		[ReadOnly(true)]
		public override ZBool ShouldSend { get => base.ShouldSend; set => base.ShouldSend = value; }

		AdditionalInfoSendingObjectCollection addInfoCollection;
		public AdditionalInfoSendingObjectCollection AddInfoCollection
		{
			get
			{
				if (addInfoCollection == null)
				{
					addInfoCollection = new AdditionalInfoSendingObjectCollection(EntryHeader, this);
					RegisterEditableChildObject(addInfoCollection);
					addInfoCollection.LoadElements();
				}
				return addInfoCollection;
			}
		}

		DocumentSendingObjectCollection supportingDocuments;
		public DocumentSendingObjectCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new DocumentSendingObjectCollection(EntryHeader, this);
					RegisterEditableChildObject(supportingDocuments);
				}
				return supportingDocuments;
			}
		}

		[ResourceStringData("DBCCD5E0-AC63-414A-8EC2-49FE5AABC604", Caption = "Movement Reference Number", ShortCaption = "MRN")]
		public ZString MovementReference => EntryHeader.MovementReferenceNumber;
		public ZPropertyInfo MovementReferenceInfo => GetZPropertyInfo(nameof(MovementReference));

		[ResourceStringData("6BD7A9CF-B2D6-4EF1-9A53-292C4FAB6B13", Caption = "Local Reference Number", ShortCaption = "LRN")]
		public ZString LocalReference => EntryHeader.CH_BGMReference;
		public ZPropertyInfo LocalReferenceInfo => GetZPropertyInfo(nameof(LocalReference));
	}
}
