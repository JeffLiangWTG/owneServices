using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public abstract class Ucc6JobDeclarationMessageSendingObject : JobDeclarationMessageSendingObject, IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider
{
	protected Ucc6JobDeclarationMessageSendingObject(CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent) : base(header, jobDeclarationMessageSendingObjectParent)
	{
	}

	public override ZString MessageType
	{
		get => base.MessageType;
		set
		{
			var oldValue = MessageType;
			base.MessageType = value;
			if (!IsCopying && oldValue != MessageType)
			{
				ResetCustomsMessageText();
			}
		}
	}

	public override ZString VOCReason
	{
		get => base.VOCReason;
		set
		{
			var oldValue = VOCReason;
			base.VOCReason = value;
			if (!IsCopying && oldValue != VOCReason)
			{
				ResetCustomsMessageText();
			}
		}
	}

	protected override void OnCancellationAndAmendmentLegislativeReferenceChange()
	{
		base.OnCancellationAndAmendmentLegislativeReferenceChange();

		ResetCustomsMessageText();
	}

	protected virtual void ResetCustomsMessageText()
	{
		customsMessageText = null;
	}

	protected abstract Ucc6JobDeclarationMessageSendingObjectLookups GetNewLookups();

	public Ucc6JobDeclarationMessageSendingObjectLookups Lookups
	{
		get
		{
			if (lookups == null || !IsLookupsCachedInBase)
			{
				lookups = GetNewLookups();
			}

			return lookups;
		}
	}

	Ucc6JobDeclarationMessageSendingObjectLookups lookups;

	protected override ZString GetDefaultMessageType() => Header.IsInAmendingStatus ? EDIMessageTypeList.Codes.Amendment : EDIMessageTypeList.Codes.NewDeclaration;

	public new Ucc6JobDeclarationMessageSendingObjectValidation Validation => (Ucc6JobDeclarationMessageSendingObjectValidation)base.Validation;

	protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new Ucc6JobDeclarationMessageSendingObjectValidation(this);

	protected override ZString GetMessageSubType()
	{
		return IsCancel
			? (ZString)EDIMessageTypeList.Codes.Cancellation
			: DeclarationType;
	}

	public override CodeDescriptionPairList MessageTypeList => Lookups.MessageTypeList;

	public override CodeDescriptionPairList CancellationOrAmendmentReasonList => Lookups.CancellationOrAmendmentReasonList;

	public override CodeDescriptionPairList CancellationAndAmendmentLegislativeReferenceList => Lookups.CancellationAndAmendmentLegislativeReferenceList;

	ICryptokiGlbExternalPassword IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.CryptokiCertificate
		=> cryptokiCertificate ?? (cryptokiCertificate = new GlbCertificateProvider().GetCryptokiCertificate());

	ICryptokiGlbExternalPassword cryptokiCertificate;

	IGlbMauExternalPassword IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.MauCertificate
		=> mauPassword ?? (mauPassword = new GlbCertificateProvider().GetMauCertificatePassword(Declaration.JE_CustomsProfile));

	IGlbMauExternalPassword mauPassword;

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.ServiceTypeNamespace => ServiceTypeNamespace;

	protected abstract ZString ServiceTypeNamespace { get; }

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.ServiceTypePrefix => ServiceTypePrefix;

	protected abstract ZString ServiceTypePrefix { get; }

	IAidaXmlSigner IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.XmlSigner => xmlSigner ?? (xmlSigner = new AidaXmlSigner());
	IAidaXmlSigner xmlSigner;

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.GetMessageType() => MessageType;

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.GetServiceId()
	{
		return IsCancel
			? MessageServiceIdList.Codes.DeclarationCancellation
			: IsAmend
				? MessageServiceIdList.Codes.DeclarationAmendment
				: MessageServiceIdList.Codes.NewDeclarationSending;
	}

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.CustomsMessageText => customsMessageText ?? (customsMessageText = BuildMessage()?.GetSerializedString() ?? ZString.Empty);
	string customsMessageText;

	bool IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.HasValidAutomaticSignature => GlbStaff.CurrentUser.HasValidAutomaticSignaturePassword();

	ILocalReferenceNumberGenerator IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.LocalReferenceNumberGenerator => localReferenceNumberGenerator ??= new LocalReferenceNumberGenerator(Factory);
	ILocalReferenceNumberGenerator localReferenceNumberGenerator;

	ZString IOutgoingCustomsMessageGeneratorValuesProvider.GetApplicationReference() => Declaration.JE_MessageType;

	IXmlMessage BuildMessage()
	{
		var messageBuilder = IsCancel
			? GetCancellationXmlMessageBuilder(new CancellationMessageWrapper(Header, this))
			: GetMessageBuilder();

		return messageBuilder?.GenerateXmlMessage();
	}

	protected abstract IXmlMessageBuilder GetMessageBuilder();

	protected abstract IXmlMessageBuilder GetCancellationXmlMessageBuilder(ICancellation cancellationInfo);

	protected AmendmentWrapper GetAmendmentWrapper()
	{
		return IsAmend
			? new AmendmentWrapper(Header, this)
			: null;
	}
}
