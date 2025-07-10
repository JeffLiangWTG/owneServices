using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class C1HeaderWrapper : IC1Header
{
	public C1HeaderWrapper(CusEntryHeader entryHeader, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		this.exportMessageSendingWrapperFactory = Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));

		var declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		var entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
		this.amendment = amendment;

		InitializeLazy(entryHeader, declaration, entryInstruction);
	}

	#region IC1Header

	string IC1Header.DeclarationCustomsOffice => JobDeclarationWrapper.DeclarationCustomsOffice;

	IDeclarationAmendment IC1Header.Amendment => amendment;

	string IC1Header.DeclarationType => JobDeclarationWrapper.EntryStyle;

	string IC1Header.AdditionalDeclarationType => EntryInstructionWrapper.AdditionalDeclarationType;

	bool IC1Header.IsSecurityDeclaration => JobDeclarationWrapper.IsSecurityDeclaration;

	string IC1Header.SpecificCircumstanceIndicator => JobDeclarationWrapper.SpecificCircumstanceIndicator;

	string IC1Header.Lrn => ITEDIMessage.ITMessageNumberPlaceholder;

	string IC1Header.DeferredPayment => EntryHeaderWrapper.DeferredPayment;

	IReadOnlyCollection<IAuthorization> IC1Header.Authorizations => EntryInstructionWrapper.Authorizations ?? Array.Empty<IAuthorization>();

	IEoriTrader IC1Header.Exporter => JobDeclarationWrapper.Exporter;

	IEoriTrader IC1Header.Declarant => JobDeclarationWrapper.ExportDeclarant;

	IRepresentative IC1Header.Representative => JobDeclarationWrapper.Representative;

	DateTime? IC1Header.GoodsPresentationDateTime => EntryInstructionWrapper.GoodsPresentationDateTime;

	string IC1Header.ExitCustomsOffice => JobDeclarationWrapper.ExitCustomsOffice;

	string IC1Header.ExportCustomsOffice => JobDeclarationWrapper.ExportCustomsOffice;

	string IC1Header.PresentationCustomsOffice => JobDeclarationWrapper.PresentationCustomsOffice;

	string IC1Header.SupervisingCustomsOffice => JobDeclarationWrapper.SupervisingCustomsOffice;

	IReadOnlyCollection<IPreviousDocument> IC1Header.PreviousDocuments => EntryInstructionWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> IC1Header.AdditionalInformation => EntryInstructionWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument> IC1Header.SupportingDocuments => EntryInstructionWrapper.SupportingDocuments ?? Array.Empty<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument>();

	IReadOnlyCollection<IAdditionalReference> IC1Header.AdditionalReferences => EntryInstructionWrapper.AdditionalReferences ?? Array.Empty<IAdditionalReference>();

	IReadOnlyCollection<ITransportDocument> IC1Header.TransportDocuments => EntryInstructionWrapper.TransportDocuments ?? Array.Empty<ITransportDocument>();

	string IC1Header.Ucr => JobDeclarationWrapper.Ucr;

	string IC1Header.CarrierIdentificationNumber => JobDeclarationWrapper.CarrierIdentificationNumber;

	IEoriTrader IC1Header.Consignor => EntryHeaderWrapper.Consignor;

	IEoriTrader IC1Header.Consignee => EntryHeaderWrapper.Consignee;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IC1Header.AdditionalSupplyChainActors => EntryInstructionWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	ITermsOfDelivery IC1Header.TermsOfDelivery => EntryHeaderWrapper.TermOfDelivery;

	string IC1Header.TransportChargesMethodOfPayment => EntryHeaderWrapper.TransportChargesMethodOfPayment;

	string IC1Header.CountryOfDestination => EntryHeaderWrapper.CountryOfDestination;

	string IC1Header.CountryOfExport => EntryHeaderWrapper.CountryOfExport;

	ILocationOfGoods IC1Header.LocationOfGoods => JobDeclarationWrapper.ExportLocationOfGoods;

	IReadOnlyCollection<IConsignmentCountryRouting> IC1Header.ConsignmentRoutings => JobDeclarationWrapper.ConsignmentRoutings ?? Array.Empty<IConsignmentCountryRouting>();

	decimal? IC1Header.GrossMass => EntryHeaderWrapper.GrossMass;

	#endregion

	void InitializeLazy(CusEntryHeader entryHeader, JobDeclaration declaration, CusEntryInstruction entryInstruction)
	{
		declarationWrapper = new Lazy<IJobDeclarationCustomsMessageWrapper>(() => exportMessageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper(declaration));
		entryInstructionWrapper = new Lazy<IExportCusEntryInstructionCustomsMessageWrapper>(() => exportMessageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper(entryInstruction));
		entryHeaderWrapper = new Lazy<ICusEntryHeaderCustomsMessageWrapper>(() => exportMessageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(entryHeader));
	}

	IJobDeclarationCustomsMessageWrapper JobDeclarationWrapper => declarationWrapper.Value;
	IExportCusEntryInstructionCustomsMessageWrapper EntryInstructionWrapper => entryInstructionWrapper.Value;
	ICusEntryHeaderCustomsMessageWrapper EntryHeaderWrapper => entryHeaderWrapper.Value;

	Lazy<IJobDeclarationCustomsMessageWrapper> declarationWrapper;
	Lazy<IExportCusEntryInstructionCustomsMessageWrapper> entryInstructionWrapper;
	Lazy<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapper;

	readonly CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;
}
