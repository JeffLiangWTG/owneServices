using CargoWise.Application;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public class ImportMessageSendingObject : Ucc6JobDeclarationMessageSendingObject
{
	public ImportMessageSendingObject(CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
		: base(header, jobDeclarationMessageSendingObjectParent)
	{
	}

	public override ZString DeclarationType
	{
		get
		{
			if (Header.IsInDepositStatus)
			{
				return ImportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneInDoganaDelleMerciI2;
			}
			return base.DeclarationType;
		}
	}

	protected override IXmlMessageBuilder GetMessageBuilder()
	{
		var messageSendingWrapperFactory = ObjectFactory.Get<CustomsMessageSending.IT.IMessageSendingWrapperFactory>();

		switch (DeclarationType)
		{
			case ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1:
				var h1MessageWrapper = new H1MessageWrapper(Header, messageSendingWrapperFactory, GetAmendmentWrapper());
				return new H1MessageBuilder(h1MessageWrapper);

			case ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeDepositoDoganaleH2:
				var h2MessageWrapper = new H2MessageWrapper(Header, messageSendingWrapperFactory, GetAmendmentWrapper());
				return new H2MessageBuilder(h2MessageWrapper);

			case ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3:
				var h3MessageWrapper = new H3MessageWrapper(Header, messageSendingWrapperFactory, GetAmendmentWrapper());
				return new H3MessageBuilder(h3MessageWrapper);

			case ImportUCC6DeclarationTypeList.Codes.RegimeSpecialePerfezionamentoAttivoH4:
				var h4MessageWrapper = new H4MessageWrapper(Header, messageSendingWrapperFactory, GetAmendmentWrapper());
				return new H4MessageBuilder(h4MessageWrapper);

			case ImportUCC6DeclarationTypeList.Codes.DichiarazioneScambiTerritoriFiscaliSpecialiH5:
				var h5MessageWrapper = new H5MessageWrapper(Header, messageSendingWrapperFactory, GetAmendmentWrapper());
				return new H5MessageBuilder(h5MessageWrapper);

			case ImportUCC6DeclarationTypeList.Codes.DichiarazioneImportazioneSemplificataI1:
				var i1MessageWrapper = new I1MessageWrapper(Header, messageSendingWrapperFactory, GetAmendmentWrapper());
				return new I1MessageBuilder(i1MessageWrapper);

			case ImportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneInDoganaDelleMerciI2:
				var i2MessageWrapper = new I2MessageWrapper(Header, messageSendingWrapperFactory);
				return new I2MessageBuilder(i2MessageWrapper);

			default:
				return null;
		}
	}

	protected override IXmlMessageBuilder GetCancellationXmlMessageBuilder(ICancellation cancellationInfo) => new CancellationMessageBuilder(cancellationInfo);

	protected override Ucc6JobDeclarationMessageSendingObjectLookups GetNewLookups() => new ImportMessageSendingObjectLookups(this);

	protected override ZString ServiceTypeNamespace => AidaSoapMessageNamespaceConstants.ImportServiceTypeNamespace;

	protected override ZString ServiceTypePrefix => Ucc6XmlConstants.ServiceTypePrefix.Import;
}
