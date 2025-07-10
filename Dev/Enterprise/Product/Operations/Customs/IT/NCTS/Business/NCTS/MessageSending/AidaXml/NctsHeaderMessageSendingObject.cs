using CargoWise.ComponentModel;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public class NctsHeaderMessageSendingObject : EU.NCTS.Business.NctsHeaderMessageSendingObject, IEntryMessageSendingObjectInfo, IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider
{
	public NctsHeaderMessageSendingObject(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	public new class Schema : EU.NCTS.Business.NctsHeaderMessageSendingObject.Schema
	{
		public const string JobReferenceNumber = "JobReferenceNumber";
		public const string MessageSubType = "MessageSubType";
		public const int MessageSubTypeMaxLength = 2;
		public const string Reason = "Reason";
		public const int ReasonMaxLength = 1;
		public const string LegislativeReference = "LegislativeReference";
		public const int LegislativeReferenceMaxLength = 1;
	}

	public new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

	public new NctsHeaderMessageSendingObjectValidation Validation => (NctsHeaderMessageSendingObjectValidation)base.Validation;

	protected override EU.NCTS.Business.NctsHeaderMessageSendingObjectValidation GetNewValidation() => new NctsHeaderMessageSendingObjectValidation(this);

	public new NctsHeaderMessageSendingObjectLookups Lookups => (NctsHeaderMessageSendingObjectLookups)base.Lookups;

	protected override EU.NCTS.Business.NctsHeaderMessageSendingObjectLookups GetNewLookups() => new NctsHeaderMessageSendingObjectLookups(this);

	protected override void SetDefaultDataCore()
	{
		base.SetDefaultDataCore();

		MessageSubType = Phase5DepartureDeclarationTypeList.Codes.SpecialRegimeTransitDeclaration;
	}

	#region JobReferenceNumber

	[ResourceStringData("NPBO:Enterprise.Customs.IT.NCTS.Business.NctsHeaderMessageSendingObject|JobReferenceNumber", ShortCaption = "Ref. No.", MediumCaption = "Job Ref. No.", Caption = "Job Reference Number")]
	public ZString JobReferenceNumber => NctsHeader.JobNumber;

	public ZPropertyInfo JobReferenceNumberInfo => GetZPropertyInfo(Schema.JobReferenceNumber);

	#endregion

	#region MessageType

	public override ZString MessageType
	{
		get => base.MessageType;
		set
		{
			var oldValue = MessageType;
			base.MessageType = value;
			if (!IsCopying && oldValue != MessageType)
			{
				ResetReasonIfNecessary();
				ResetLegislativeReferenceIfNecessary();
				ResetAmendment();
			}
		}
	}

	#endregion

	#region Reason

	[List(nameof(Lookups) + "." + nameof(NctsHeaderMessageSendingObjectLookups.ReasonList))]
	[ResourceStringData("NPBO:Enterprise.Customs.IT.NCTS.Business.NctsHeaderMessageSendingObject|Reason", ShortCaption = "Reason", Caption = "Reason")]
	[MaxLength(Schema.ReasonMaxLength)]
	[ReadOnlyMember(nameof(Reason_ReadOnly))]
	public ZString Reason
	{
		get => reason;
		set
		{
			CheckMaximumLength(ReasonInfo, value);
			SetNonPersistentPropertyValue(ReasonInfo, ref reason, value);

			if (!IsValidationSuspended)
			{
				Validation.ValidateReason();
			}
		}
	}

	ZString reason;

	public ZPropertyInfo ReasonInfo => GetZPropertyInfo(nameof(Reason));

	bool Reason_ReadOnly => !IsCancel && !IsAmend;

	#endregion

	#region LegislativeReference

	[List(nameof(Lookups) + "." + nameof(NctsHeaderMessageSendingObjectLookups.LegislativeReferenceList))]
	[ResourceStringData("NPBO:Enterprise.Customs.IT.NCTS.Business.NctsHeaderMessageSendingObject|LegislativeReference", ShortCaption = "Reference", Caption = "Reference")]
	[MaxLength(Schema.LegislativeReferenceMaxLength)]
	[ReadOnlyMember(nameof(LegislativeReference_ReadOnly))]
	public ZString LegislativeReference
	{
		get => legislativeReference;
		set
		{
			CheckMaximumLength(LegislativeReferenceInfo, value);
			SetNonPersistentPropertyValue(LegislativeReferenceInfo, ref legislativeReference, value);

			if (!IsValidationSuspended)
			{
				Validation.ValidateLegislativeReference();
			}
		}
	}

	ZString legislativeReference;

	public ZPropertyInfo LegislativeReferenceInfo => GetZPropertyInfo(nameof(LegislativeReference));

	bool LegislativeReference_ReadOnly => !IsCancel && !IsAmend;

	#endregion

	#region MessageSubType

	[List(nameof(Lookups) + "." + nameof(NctsHeaderMessageSendingObjectLookups.MessageSubTypeList))]
	[ResourceStringData("NPBO:Enterprise.Customs.IT.NCTS.Business.NctsHeaderMessageSendingObject|MessageSubType", ShortCaption = "Msg. Sub Type", Caption = "Message Sub Type")]
	[MaxLength(Schema.MessageSubTypeMaxLength)]
	public ZString MessageSubType
	{
		get => messageSubType;
		set
		{
			CheckMaximumLength(MessageSubTypeInfo, value);
			SetNonPersistentPropertyValue(MessageSubTypeInfo, ref messageSubType, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateMessageSubType();
			}
		}
	}

	ZString messageSubType;

	public ZPropertyInfo MessageSubTypeInfo => GetZPropertyInfo(Schema.MessageSubType);

	#endregion

	#region IEntryMessageSendingObjectInfo Members

	ZBool IEntryMessageSendingObjectInfo.EntryStatusAllowsSending => NctsHeaderMessageSendingObjectStrategyFactory.CreateStrategy(this).DoesStatusAllowSending();

	ZString IEntryMessageSendingObjectInfo.EntryReference => NctsHeader.BH_JobReference;

	ZString IEntryMessageSendingObjectInfo.EntryMessageStatus => NctsHeader.EffectiveMessageStatus;

	ZString IEntryMessageSendingObjectInfo.EntryCustomsStatus => NctsHeader.DepartureMovementStatus;

	#endregion

	public INctsAmendment Amendment => amendment ??= GetAmendment();
	INctsAmendment amendment;

	protected IXmlMessageBuilder GetCancellationXmlMessageBuilder(ICancellation cancellationInfo)
		=> new CancellationMessageBuilder(cancellationInfo);

	protected override bool IsCancelCore => MessageType == EDIMessageTypeList.Codes.Cancellation;

	protected override bool IsAmendCore => MessageType == EDIMessageTypeList.Codes.Amendment;

	protected IXmlMessageBuilder GetMessageBuilder()
	{
		IMessageSendingWrapperFactory messageSendingWrapperFactory = new MessageSendingWrapperFactory();
		var entityWrapperProvider = new NctsEntityWrapperProvider();

		switch (MessageSubType)
		{
			case Phase5DepartureDeclarationTypeList.Codes.SpecialRegimeTransitDeclaration:
				var d1MessageWrapper = new D1MessageWrapper(NctsHeader, messageSendingWrapperFactory, Amendment);
				return
					!NctsHeader.IsInPhase5TransitionPeriod
					? new D1MessageBuilder(d1MessageWrapper)
					: new D1TransitionPeriodMessageBuilder(d1MessageWrapper);
			case Phase5DepartureDeclarationTypeList.Codes.SpecialRegimeTransitDeclarationWithReducedDataRequirementsRailAirAndSeaTransport:
				var dataProvider = DeclarationD2DataProvider.CreateProvider(this, new D2AdditionalDataProvider(entityWrapperProvider, NctsHeader.IsInPhase5TransitionPeriod));
				return new D2MessageBuilder(dataProvider);
			default:
				return null;
		}
	}

	#region IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.GetMessageType() => MessageType;

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.GetServiceId()
	{
		return IsCancel
			? MessageServiceIdList.Codes.DeclarationCancellation
			: IsAmend
				? MessageServiceIdList.Codes.DeclarationAmendment
				: MessageServiceIdList.Codes.NewDeclarationSending;
	}

	ZString IOutgoingCustomsMessageGeneratorValuesProvider.GetSubType()
	{
		return IsCancel
			? (ZString)EDIMessageTypeList.Codes.Cancellation
			: MessageSubType;
	}

	ZString IOutgoingCustomsMessageGeneratorValuesProvider.GetApplicationReference() => NctsApplicationReference;

	ICryptokiGlbExternalPassword IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.CryptokiCertificate
		=> cryptokiCertificate ?? (cryptokiCertificate = new GlbCertificateProvider().GetCryptokiCertificate());

	IGlbMauExternalPassword IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.MauCertificate
		=> mauPassword ?? (mauPassword = new GlbCertificateProvider().GetMauCertificatePassword(NctsHeader.BH_CustomsProfile));

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.ServiceTypeNamespace => AidaSoapMessageNamespaceConstants.NctsServiceTypeNamespace;

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.ServiceTypePrefix => Ucc6XmlConstants.ServiceTypePrefix.Ncts;

	IAidaXmlSigner IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.XmlSigner => xmlSigner ?? (xmlSigner = new AidaXmlSigner());

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.CustomsMessageText => customsMessageText ?? (customsMessageText = BuildMessage()?.GetSerializedString() ?? ZString.Empty);

	BusinessObject IOutgoingCustomsMessageGeneratorValuesProvider.Parent => NctsHeader.MovementHeader;

	public bool HasValidAutomaticSignature => GlbStaff.CurrentUser.HasValidAutomaticSignaturePassword();

	ILocalReferenceNumberGenerator IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.LocalReferenceNumberGenerator => localReferenceNumberGenerator ??= new LocalReferenceNumberGenerator(Factory);
	ILocalReferenceNumberGenerator localReferenceNumberGenerator;

	#endregion

	#region Implementation

	void ResetReasonIfNecessary()
	{
		if (!IsCancel && !IsAmend)
		{
			Reason = ZString.Empty;
		}
	}

	void ResetLegislativeReferenceIfNecessary()
	{
		if (!IsCancel && !IsAmend)
		{
			LegislativeReference = ZString.Empty;
		}
	}

	IXmlMessage BuildMessage()
	{
		var messageBuilder = IsCancel
			? GetCancellationXmlMessageBuilder(new NctsCancellationMessageWrapper(NctsHeader, this))
			: GetMessageBuilder();

		return messageBuilder?.GenerateXmlMessage();
	}

	INctsAmendment GetAmendment()
	{
		return IsAmend
			? new NctsAmendmentWrapper(this)
			: null;
	}

	void ResetAmendment() => amendment = null;

	#endregion

	ICryptokiGlbExternalPassword cryptokiCertificate;
	IGlbMauExternalPassword mauPassword;
	IAidaXmlSigner xmlSigner;
	string customsMessageText;

	const string NctsApplicationReference = "TRA";
}
