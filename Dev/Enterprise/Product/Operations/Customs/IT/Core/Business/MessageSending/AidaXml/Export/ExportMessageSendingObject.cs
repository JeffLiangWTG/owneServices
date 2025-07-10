using System;
using CargoWise.Application;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using CancellationMessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration.Export.CancellationMessageBuilder;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public class ExportMessageSendingObject : Ucc6JobDeclarationMessageSendingObject
{
	public ExportMessageSendingObject(CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
		: base(header, jobDeclarationMessageSendingObjectParent)
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
				DefaultCancellationAndAmendmentLegislativeReference();
			}
		}
	}

	public override ZString DeclarationType
	{
		get
		{
			if (Header.IsInDepositStatus)
			{
				return ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2;
			}

			return base.DeclarationType;
		}
	}

	protected override IXmlMessageBuilder GetMessageBuilder()
	{
		var exportMessageSendingWrapperFactory = ObjectFactory.Get<CustomsMessageSending.IT.IExportMessageSendingWrapperFactory>();
		var amendmentWrapper = GetAmendmentWrapper();
		var isInTransitionPeriod = Header.Declaration?.IsTransitionPeriodAES30 ?? false;

		switch (DeclarationType)
		{
			case ExportUCC6DeclarationTypeList.Codes.DichiarazioneRiesportazioneB1:
				var b1MessageWrapper = new B1MessageWrapper(Header, exportMessageSendingWrapperFactory, amendmentWrapper);
				return CreateBuilderBaseOnTransitionPeriodFlag<B1MessageBuilder, B1TransitionPeriodMessageBuilder>(isInTransitionPeriod, b1MessageWrapper);

			case ExportUCC6DeclarationTypeList.Codes.RegimeSpecialeDichiarazioneB2:
				var b2MessageWrapper = new B2MessageWrapper(Header, exportMessageSendingWrapperFactory, amendmentWrapper);
				return CreateBuilderBaseOnTransitionPeriodFlag<B2MessageBuilder, B2TransitionPeriodMessageBuilder>(isInTransitionPeriod, b2MessageWrapper);

			case ExportUCC6DeclarationTypeList.Codes.DichiarazionePerTerritoriFiscaliSpecialiB4:
				var b4MessageWrapper = new B4MessageWrapper(Header, exportMessageSendingWrapperFactory, amendmentWrapper);
				return CreateBuilderBaseOnTransitionPeriodFlag<B4MessageBuilder, B4TransitionPeriodMessageBuilder>(isInTransitionPeriod, b4MessageWrapper);

			case ExportUCC6DeclarationTypeList.Codes.DichiarazioneSemplificataC1:
				var c1MessageWrapper = new C1MessageWrapper(Header, exportMessageSendingWrapperFactory, amendmentWrapper);
				return CreateBuilderBaseOnTransitionPeriodFlag<C1MessageBuilder, C1TransitionPeriodMessageBuilder>(isInTransitionPeriod, c1MessageWrapper);

			case ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2:
				var c2MessageWrapper = new C2MessageWrapper(Header, exportMessageSendingWrapperFactory, amendmentWrapper);
				return CreateBuilderBaseOnTransitionPeriodFlag<C2MessageBuilder, C2TransitionPeriodMessageBuilder>(isInTransitionPeriod, c2MessageWrapper);

			default:
				return null;
		}
	}

	protected override IXmlMessageBuilder GetCancellationXmlMessageBuilder(ICancellation cancellationInfo) => new CancellationMessageBuilder(cancellationInfo);

	protected override ZString ServiceTypeNamespace => AidaSoapMessageNamespaceConstants.ExportServiceTypeNamespace;

	protected override ZString ServiceTypePrefix => Ucc6XmlConstants.ServiceTypePrefix.Export;

	protected override Ucc6JobDeclarationMessageSendingObjectLookups GetNewLookups() => new ExportMessageSendingObjectLookups(this);

	IXmlMessageBuilder CreateBuilderBaseOnTransitionPeriodFlag<TBuilder, TTransitionPeriodBuilder>(bool isInTransitionPeriod, params object[] constructorParams)
		where TBuilder : IXmlMessageBuilder
		where TTransitionPeriodBuilder : IXmlMessageBuilder
	{
		var type = isInTransitionPeriod ? typeof(TTransitionPeriodBuilder) : typeof(TBuilder);
		return (IXmlMessageBuilder)Activator.CreateInstance(type, constructorParams);
	}

	void DefaultCancellationAndAmendmentLegislativeReference()
	{
		CancellationAndAmendmentLegislativeReference = IsAmend
								? (ZString)Ucc6ExportAmendmentLegislativeReferenceList.Codes.CduArt173
								: ZString.Empty;
	}
}
