using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class UploadDocumentsSendingAction : CusEntryHeaderMessageSendingAction, IFallbackProcedureSendingObject
	{
		public static class Schema
		{
			public const string AlternativeDateOfAcceptance = "AlternativeDateOfAcceptance";
			public const string CustomsReference = "CustomsReference";
			public const string CustomsJustification = "CustomsJustification";
		}

		public UploadDocumentsSendingAction(CusEntryHeader cusEntryHeader) : base(cusEntryHeader)
		{
			IsUCC5 = EntryHeader.Declaration?.IsUCC5 ?? false;
		}

		public bool IsUCC5 { get; }

		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				base.ShouldSend = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldSend = true;
			MessageType = AISUploadDocumentsMessageTypeList.Codes.IM483;
		}

		protected override Type SenderType => typeof(UploadDocumentsSender);

		protected override CusEntryHeaderMessageSendingActionLookups GetNewLookups() => new UploadDocumentsSendingActionLookups(this);

		protected override CusEntryHeaderMessageSendingActionValidation GetNewValidation() => new UploadDocumentsSendingActionValidation(this);
		public new UploadDocumentsSendingActionValidation Validation => (UploadDocumentsSendingActionValidation)GetNewValidation();

		[ResourceStringData("53ECFAA7-14CE-44E2-BAC0-7495F4A76443", Caption = "MRN", FullDescription = "Movement Reference Number")]
		public ZString MovementReferenceNumber => EntryHeader.MovementReferenceNumber;
		public ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(nameof(MovementReferenceNumber));

		AdditionalInfoSendingObjectCollection addInfoCollection;
		public AdditionalInfoSendingObjectCollection AddInfoCollection
		{
			get
			{
				if (addInfoCollection == null)
				{
					addInfoCollection = new AdditionalInfoSendingObjectCollection(EntryHeader, this);
					if (MessageType == AISUploadDocumentsMessageTypeList.Codes.IM483)
					{
						addInfoCollection.LoadElements((doc) => doc.CSI_Code != Constants.SupportingDocumentCodes._1Q99 || !doc.CSI_ReferenceNumber.IsEmpty);
					}
					else
					{
						addInfoCollection.LoadElements();
					}
					RegisterEditableChildObject(addInfoCollection);
				}
				return addInfoCollection;
			}
		}

		[ResourceStringData("5DB20991-839B-47B1-8FF1-2F02CC0A76AE", Caption = "Alternative Date of Acceptance")]
		public ZDate AlternativeDateOfAcceptance
		{
			get => alternativeDateOfAcceptance;
			set
			{
				SetNonPersistentPropertyValue(AlternativeDateOfAcceptanceInfo, ref alternativeDateOfAcceptance, value);
				Validation.CheckFallbackProcedure();
			}
		}
		ZDate alternativeDateOfAcceptance;
		public ZPropertyInfo AlternativeDateOfAcceptanceInfo => GetZPropertyInfo(nameof(AlternativeDateOfAcceptance));

		[ResourceStringData("4AAED22C-7B58-4425-A6F4-186CC1ABA5FA", Caption = "Customs Reference")]
		public ZString CustomsReferenceNumber
		{
			get => customsReference;
			set
			{
				SetNonPersistentPropertyValue(CustomsReferenceNumberInfo, ref customsReference, value);
				Validation.CheckFallbackProcedure();
			}
		}
		ZString customsReference;
		public ZPropertyInfo CustomsReferenceNumberInfo => GetZPropertyInfo(nameof(CustomsReferenceNumber));

		[ResourceStringData("C952A61A-549A-40C3-8864-F72FC4896517", Caption = "Customs Justification")]
		public ZString CustomsJustification
		{
			get => customsJustification;
			set
			{
				SetNonPersistentPropertyValue(CustomsJustificationInfo, ref customsJustification, value);
				Validation.CheckFallbackProcedure();
			}
		}
		ZString customsJustification;
		public ZPropertyInfo CustomsJustificationInfo => GetZPropertyInfo(nameof(CustomsJustification));
	}
}
