using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class DeclarationMessageSendingObject : JobDeclarationMessageSendingObject, IMessageSendingObject
{
	protected DeclarationMessageSendingObject(CusEntryHeader header)
		: base(header)
	{
	}

	new CusEntryHeader Header => (CusEntryHeader)base.Header;

	public new class Schema : JobDeclarationMessageSendingObject.Schema
	{
		public const string SubStyle = nameof(DeclarationMessageSendingObject.SubStyle);
		public const string Description = nameof(DeclarationMessageSendingObject.Description);
		public new const int MessageTypeMaxLength = 5;
	}

	public abstract ZString FriendlyNameForMessageManager { get; }

	protected override void SetMessageSendingObjectDefaultValues()
	{
		base.SetMessageSendingObjectDefaultValues();
		ShouldSend = !Header.IsMessageStatusSent;
	}

	public new DeclarationMessageSendingObjectValidation Validation => (DeclarationMessageSendingObjectValidation)base.Validation;

	protected override JobDeclarationMessageSendingObjectValidation GetNewValidation() => new DeclarationMessageSendingObjectValidation(this);

	public DeclarationMessageSendingObjectLookups Lookups
	{
		get
		{
			if (fLookups == null || !IsLookupsCachedInBase)
			{
				fLookups = GetNewLookups();
			}

			return fLookups;
		}
	}
	DeclarationMessageSendingObjectLookups fLookups;

	protected virtual DeclarationMessageSendingObjectLookups GetNewLookups() => new DeclarationMessageSendingObjectLookups(this);

	[ResourceStringData("Enterprise.Customs.CH.Business.DeclarationMessageSendingObject|SubStyle", Caption = "Sub Style")]
	public ZString SubStyle => Header?.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
	public ZPropertyInfo SubStyleInfo => GetZPropertyInfo(Schema.SubStyle);

	[ResourceStringData("Enterprise.Customs.CH.Business.DeclarationMessageSendingObject|DeclarationType", Caption = "Declaration Type")]
	public override ZString DeclarationType => base.DeclarationType;

	[ResourceStringData("Enterprise.Customs.CH.Business.DeclarationMessageSendingObject|Description", Caption = "Description")]
	public ZString Description => Header?.EntryInstruction?.CEI_Description ?? ZString.Empty;
	public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(Schema.Description);

	[ResourceStringData("Enterprise.Customs.CH.Business.DeclarationMessageSendingObject|LocalReferenceNumber", Caption = "Registration Number")]
	[BusinessObjectTestExclude]
	public override ZString LocalReferenceNumber => Header?.CH_BGMReference ?? ZString.Empty;

	[ResourceStringData("Enterprise.Customs.CH.Business.DeclarationMessageSendingObject|EntryStatus", Caption = "Entry Status")]
	public override ZString EntryStatus => base.EntryStatus;

	[MaxLength(Schema.MessageTypeMaxLength)]
	[List(nameof(Lookups) + "." + nameof(DeclarationMessageSendingObjectLookups.MessageTypeList))]
	[ResourceStringData("Enterprise.Customs.CH.Business.DeclarationMessageSendingObject|MessageType", Caption = "Message Type", FullDescription = "Indicates the type of message to be sent to customs.", ShortCaption = "Mess. Type")]
	public override ZString MessageType
	{
		get => base.MessageType;
		set
		{
			base.MessageType = value;
			ClearVOCReasonIfReadOnly();
		}
	}

	protected override ZString GetDefaultMessageType() => Lookups.MessageTypeList.GetAllCodesZString().FirstOrDefault();

	protected override bool ShouldSend_ReadOnly => Header.IsMessageStatusSent && !Env.Security.CHCustomsDeclarationAllowResendToCustoms.IsAllowed;

	protected override bool MessageType_ReadOnly => false;

	[List(nameof(Lookups) + "." + nameof(DeclarationMessageSendingObjectLookups.CorrectionReasonList))]
	[ResourceStringData("Enterprise.Customs.CH.Business.DeclarationMessageSendingObject|VOCReason", Caption = "Reason", FullDescription = "The correction/cancellation reason code to be sent to customs.", ShortCaption = "Reason")]
	public override ZString VOCReason { get => base.VOCReason; set => base.VOCReason = value; }

	public bool IsVOCReason_ReadOnly => VOCReason_ReadOnly;

	[ResourceStringData("Enterprise.Customs.CH.Business.DeclarationMessageSendingObject|ReasonText", Caption = "Reason Text")]
	[ReadOnlyMember(nameof(VOCReason_ReadOnly))]
	public ZString ReasonText
	{
		get => reasonText;
		set
		{
			reasonText = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateReasonText();
			}
			ReasonTextInfo.RefreshBinding();
		}
	}
	ZString reasonText;

	void ClearVOCReasonIfReadOnly()
	{
		if (VOCReason_ReadOnly)
		{
			VOCReason = ZString.Empty;
			ReasonText = ZString.Empty;
		}
	}

	public ZPropertyInfo ReasonTextInfo => GetZPropertyInfo(nameof(ReasonText));

	public bool IsCancellationOrDataRequest => PassarMessageTypeList.IsCancellationMessage(MessageType) || PassarMessageTypeList.IsDataRequestMessage(MessageType);

	#region IMessageSendingObject

	public virtual ZString ApplicationCode => EDIMessage.ApplicationCodes.CHCustomsEdec;

	public ZString GetApplicationReference() => GetApplicationReferenceCore();

	protected virtual ZString GetApplicationReferenceCore() => ZString.Empty;

	public virtual ZString MessageTypeForEDIMessage => Header.Declaration.JE_MessageType;

	public virtual ZString MessageSubTypeForEDIMessage => PassarMessageTypeList.GetMessageSubType(MessageTypeCodeList.Codes.Import, MessageType);

	public ZString ToMessageString() => MessageBuilderFactory.NewMessageBuilder(this).GenerateXmlMessage().GetSerializedString();

	public abstract ZGuid GetCredentialPK();

	#endregion
}
