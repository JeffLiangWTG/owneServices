using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.TemporaryStorage;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageMessageSendingObject : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject, IEntryMessageSendingObjectInfo, IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider
{
	public TemporaryStorageMessageSendingObject(TemporaryStorageHeader header) : base(header)
	{
	}

	public new class Schema : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject.Schema
	{
		public const string JobReferenceNumber = "JobReferenceNumber";
	}

	public new TemporaryStorageMessageSendingObjectValidation Validation => (TemporaryStorageMessageSendingObjectValidation)base.Validation;

	protected override EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectValidation GetNewValidation() => new TemporaryStorageMessageSendingObjectValidation(this);

	public new TemporaryStorageHeader Header => (TemporaryStorageHeader)base.Header;

	#region JobReferenceNumber

	[ResourceStringData("NPBO:Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageMessageSendingObject|JobReferenceNumber", ShortCaption = "Ref. No.", MediumCaption = "Job Ref. No.", Caption = "Job Reference Number")]
	public ZString JobReferenceNumber => Header.JobNumber;

	public ZPropertyInfo JobReferenceNumberInfo => GetZPropertyInfo(Schema.JobReferenceNumber);

	#endregion

	protected override bool ShouldSend_ReadOnly => true;

	protected override bool DeclarationType_ReadOnly => true;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		DeclarationType = MessageSubTypeList.Codes.DeclarationOfTemporaryStorage;
	}

	public override void ValidateShouldSend()
	{
		if (!IsValidationSuspended)
		{
			Validation.ValidateShouldSend();
		}
	}

	#region IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider Members

	ICryptokiGlbExternalPassword IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.CryptokiCertificate
		=> cryptokiCertificate ??= new GlbCertificateProvider().GetCryptokiCertificate();

	ICryptokiGlbExternalPassword cryptokiCertificate;

	IGlbMauExternalPassword IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.MauCertificate
		=> mauCertificate ??= new GlbCertificateProvider().GetMauCertificatePassword(Header.AMA_CustomsProfile);

	IGlbMauExternalPassword mauCertificate;

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.ServiceTypeNamespace => AidaSoapMessageNamespaceConstants.PntsServiceTypeNamespace;

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.ServiceTypePrefix => Ucc6XmlConstants.ServiceTypePrefix.Pnts;

	IAidaXmlSigner IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.XmlSigner
		=> xmlSigner ??= new AidaXmlSigner();

	IAidaXmlSigner xmlSigner;

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.CustomsMessageText
		=> customsMessageText ??= GetMessageBuilder()?.GenerateXmlMessage()?.GetSerializedString() ?? ZString.Empty;

	string customsMessageText;

	bool IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.HasValidAutomaticSignature => GlbStaff.CurrentUser.HasValidAutomaticSignaturePassword();

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.GetMessageType() => MessageType;

	ZString IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.GetServiceId() => NewDeclarationServiceId;

	ILocalReferenceNumberGenerator IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider.LocalReferenceNumberGenerator => localReferenceNumberGenerator ??= new TemporaryStorageLocalReferenceNumberGenerator(Header, new LocalReferenceNumberGenerator(Factory));
	ILocalReferenceNumberGenerator localReferenceNumberGenerator;

	#endregion

	#region IEntryMessageSendingObjectInfo Members

	ZBool IEntryMessageSendingObjectInfo.EntryStatusAllowsSending => TemporaryStorageMessageSendingObjectStrategyFactory.CreateStrategy(this).DoesStatusAllowSending();

	ZString IEntryMessageSendingObjectInfo.EntryReference => Header.AMA_JobReference;

	ZString IEntryMessageSendingObjectInfo.EntryMessageStatus => Header.AMA_MessageStatus;

	ZString IEntryMessageSendingObjectInfo.EntryCustomsStatus => Header.CustomsStatus;

	#endregion

	#region IOutgoingCustomsMessageGeneratorValuesProvider Members

	BusinessObject IOutgoingCustomsMessageGeneratorValuesProvider.Parent => Header;

	ZString IOutgoingCustomsMessageGeneratorValuesProvider.GetApplicationReference() => PntsApplicationReference;

	ZString IOutgoingCustomsMessageGeneratorValuesProvider.GetSubType() => DeclarationType;

	#endregion

	protected IXmlMessageBuilder GetMessageBuilder()
	{
		switch (DeclarationType)
		{
			case MessageSubTypeList.Codes.DeclarationOfTemporaryStorage:
				if (MessageType == EDIMessageTypeList.Codes.NewDeclaration)
				{
					var dataProvider = TemporaneaCustodiaG4DataProvider.CreateProvider(Header, new G4AdditionalDataProvider());
					return new TemporaneaCustodiaG4MessageBuilder(dataProvider);
				}

				if (MessageType == EDIMessageTypeList.Codes.Amendment)
				{
					var dataProvider = TemporaneaCustodiaG4AmendmentDataProvider.CreateProvider(Header, new G4AdditionalDataProvider());
					return new TemporaneaCustodiaG4AmendmentMessageBuilder(dataProvider);
				}

				break;
		}
		return null;
	}

	const string PntsApplicationReference = "TST";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Does not have to be translated")]
	const string NewDeclarationServiceId = "submission";
}
