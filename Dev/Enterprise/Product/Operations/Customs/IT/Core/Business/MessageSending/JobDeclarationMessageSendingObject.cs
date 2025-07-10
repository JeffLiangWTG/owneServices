using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public abstract class JobDeclarationMessageSendingObject : EU.Business.JobDeclarationMessageSendingObject, IOutgoingCustomsMessageGeneratorValuesProvider, IEntryMessageSendingObjectInfo
{
	protected JobDeclarationMessageSendingObject(EU.Business.Declaration.CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent) : base(header)
	{
		JobDeclarationMessageSendingObjectParent = Argument.NotNull(jobDeclarationMessageSendingObjectParent, nameof(jobDeclarationMessageSendingObjectParent));
	}

	public new sealed class Schema : Customs.Business.AutoJobDeclarationMessageSendingObject.Schema
	{
		Schema()
		{
		}

		public const string CancellationAndAmendmentLegislativeReference = "CancellationAndAmendmentLegislativeReference";
		public const int CancellationAndAmendmentLegislativeReferenceMaxLength = 1;
		public new const int VOCReasonMaxLength = 1;
		public const string DeclarationDescription = "DeclarationDescription";
		public const string MessageStatus = "MessageStatus";
		public const string CombinedCustomsMessageSubType = "CombinedCustomsMessageSubType";
		public const string BGMReference = "CH_BGMReference";
		public const string CustomsMessageSendingMode = "CustomsMessageSendingMode";
	}

	public new CusEntryHeader Header => (CusEntryHeader)base.Header;

	public bool IsEntryInAmendingStatus => Header.IsInAmendingStatus;

	public JobDeclarationMessageSendingObjectParent JobDeclarationMessageSendingObjectParent { get; }

	protected abstract ZString GetMessageSubType();

	public JobDeclaration Declaration => Header.Declaration;

	[List(nameof(MessageTypeList))]
	[ResourceStringData("38F73472-C4A8-4655-A285-27F9CFE103C7", Caption = "Msg. Type")]
	public override ZString MessageType
	{
		get => base.MessageType;
		set
		{
			var oldValue = MessageType;
			base.MessageType = value;
			if (!IsCopying && oldValue != MessageType)
			{
				CleanUpVOCReasonAndCancellationAndAmendmentLegislativeReferenceIfNeeded();
				Validation.ValidateShouldSend();
			}
		}
	}

	protected override bool MessageType_ReadOnly => false;

	void CleanUpVOCReasonAndCancellationAndAmendmentLegislativeReferenceIfNeeded()
	{
		if (!IsCancel && !IsAmend)
		{
			VOCReason = ZString.Empty;
			CancellationAndAmendmentLegislativeReference = ZString.Empty;
		}
	}

	[List(nameof(CancellationOrAmendmentReasonList))]
	[MaxLength(Schema.VOCReasonMaxLength)]
	[ResourceStringData("185FBB33-718D-4188-A47B-60ABDC46648C", Caption = "Reason")]
	public override ZString VOCReason { get => base.VOCReason; set => base.VOCReason = value; }

	protected override bool VOCReason_ReadOnly => !IsCancel && !IsAmend;

	[List(nameof(CancellationAndAmendmentLegislativeReferenceList))]
	[MaxLength(Schema.CancellationAndAmendmentLegislativeReferenceMaxLength)]
	[ReadOnlyMember(nameof(CancellationAndAmendmentLegislativeReference_ReadOnly))]
	[ResourceStringData("39897B7C-2606-4DE5-938E-6F6F0515EDEC", Caption = "Reference")]
	public ZString CancellationAndAmendmentLegislativeReference
	{
		get => cancellationAndAmendmentLegislativeReference;

		set
		{
			var oldValue = CancellationAndAmendmentLegislativeReference;
			CheckMaximumLength(CancellationAndAmendmentLegislativeReferenceInfo, value);
			SetNonPersistentPropertyValue(CancellationAndAmendmentLegislativeReferenceInfo, ref cancellationAndAmendmentLegislativeReference, value);
			if (!IsCopying && oldValue != CancellationAndAmendmentLegislativeReference)
			{
				OnCancellationAndAmendmentLegislativeReferenceChange();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateCancellationAndAmendmentLegislativeReference();
			}
		}
	}
	ZString cancellationAndAmendmentLegislativeReference;

	protected virtual void OnCancellationAndAmendmentLegislativeReferenceChange()
	{
	}

	public ZPropertyInfo CancellationAndAmendmentLegislativeReferenceInfo => GetZPropertyInfo(Schema.CancellationAndAmendmentLegislativeReference);

	protected bool CancellationAndAmendmentLegislativeReference_ReadOnly => !IsCancel && !IsAmend;

	[ResourceStringData("Enterprise.Customs.IT.Business.JobDeclarationMessageSendingObject|DeclarationDescription", ShortCaption = "Desc.", Caption = "Description")]
	public ZString DeclarationDescription => Header.EntryInstruction?.CEI_Description ?? ZString.Empty;

	[ResourceStringData("Enterprise.Customs.IT.Business.JobDeclarationMessageSendingObject|MessageStatus", ShortCaption = "Status", Caption = "Message Status")]
	public ZString MessageStatus => Header.CH_Status;

	public ZBool StatusAllowsSending => Header.StatusAllowsSending;

	[ResourceStringData("B6D11859-D862-44C2-BEC9-096EA9B289E1", Caption = "Msg. Sub Type")]
	public ZString CombinedCustomsMessageSubType => GetCombinedCustomsMessageSubType();

	public ZPropertyInfo CombinedCustomsMessageSubTypeInfo => GetZPropertyInfo(Schema.CombinedCustomsMessageSubType);

	protected virtual ZString GetCombinedCustomsMessageSubType() => GetMessageSubType();

	[ResourceStringData("124CC7B6-9FDD-4932-9758-2D176AE76C12", Caption = "Ref No.")]
	public ZString CH_BGMReference => Header.CH_BGMReference;

	public ZString CustomsMessageSendingMode
	{
		get => customsMessageSendingMode;
		set => SetNonPersistentPropertyValue(CustomsMessageSendingModeInfo, ref customsMessageSendingMode, value);
	}
	ZString customsMessageSendingMode;

	public ZPropertyInfo CustomsMessageSendingModeInfo => GetZPropertyInfo(Schema.CustomsMessageSendingMode);

	public new JobDeclarationMessageSendingObjectValidation Validation => (JobDeclarationMessageSendingObjectValidation)base.Validation;

	protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new JobDeclarationMessageSendingObjectValidation(this);

	protected override bool IsCancelCore => MessageType == EDIMessageTypeList.Codes.Cancellation;

	protected override bool IsAmendCore => MessageType == EDIMessageTypeList.Codes.Amendment;

	public virtual CodeDescriptionPairList MessageTypeList => new CodeDescriptionPairList();

	public virtual CodeDescriptionPairList CancellationOrAmendmentReasonList => new CodeDescriptionPairList();

	public virtual CodeDescriptionPairList CancellationAndAmendmentLegislativeReferenceList => new CodeDescriptionPairList();

	public ZBool CanBeSentOnlyInFallbackMode
	{
		get
		{
			var entryInstruction = Header.EntryInstruction;

			var canBeSentOnlyInFallbackMode = ZBool.False;
			if (entryInstruction != null)
			{
				canBeSentOnlyInFallbackMode = entryInstruction.IsPreliminaryDeclarationUnderCodeA || entryInstruction.IsSimplifiedDeclaration;
			}
			return canBeSentOnlyInFallbackMode;
		}
	}

	#region IOutgoingCustomsMessageGeneratorValuesProvider Members

	BusinessObject IOutgoingCustomsMessageGeneratorValuesProvider.Parent => Header;

	ZString IOutgoingCustomsMessageGeneratorValuesProvider.GetSubType() => GetMessageSubType();

	ZString IOutgoingCustomsMessageGeneratorValuesProvider.GetApplicationReference() => Declaration.GetApplicationReference();

	#endregion

	#region IEntryMessageSendingObjectInfo Members

	ZBool IEntryMessageSendingObjectInfo.EntryStatusAllowsSending => StatusAllowsSending;

	ZString IEntryMessageSendingObjectInfo.EntryReference => CH_BGMReference;

	ZString IEntryMessageSendingObjectInfo.EntryMessageStatus => Header.CH_Status;

	ZString IEntryMessageSendingObjectInfo.EntryCustomsStatus => Header.CH_EntryStatus;

	#endregion
}
