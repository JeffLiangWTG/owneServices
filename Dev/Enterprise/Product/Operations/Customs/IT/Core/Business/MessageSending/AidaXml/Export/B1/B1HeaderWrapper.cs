using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class B1HeaderWrapper : IB1Header
{
	public B1HeaderWrapper(CusEntryHeader entryHeader, IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));

		var declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
		this.exportMessageSendingWrapperFactory = Argument.NotNull(exportMessageSendingWrapperFactory, nameof(this.exportMessageSendingWrapperFactory));
		this.amendment = amendment;

		InitializeLazy(entryHeader, declaration, entryInstruction);
	}

	#region IB1Header

	string IB1Header.DeclarationCustomsOffice => JobDeclarationWrapper.DeclarationCustomsOffice;

	IDeclarationAmendment IB1Header.Amendment => amendment;

	string IB1Header.DeclarationType => JobDeclarationWrapper.EntryStyle;

	string IB1Header.AdditionalDeclarationType => EntryInstructionWrapper.AdditionalDeclarationType;

	bool IB1Header.IsSecurityDeclaration => JobDeclarationWrapper.IsSecurityDeclaration;

	string IB1Header.SpecificCircumstanceIndicator => JobDeclarationWrapper.SpecificCircumstanceIndicator;

	string IB1Header.Lrn => ITEDIMessage.ITMessageNumberPlaceholder;

	string IB1Header.DeferredPayment => EntryHeaderWrapper.DeferredPayment;

	IReadOnlyCollection<IAuthorization> IB1Header.Authorizations => EntryInstructionWrapper.Authorizations ?? Array.Empty<IAuthorization>();

	IEoriTrader IB1Header.Exporter => JobDeclarationWrapper.Exporter;

	IEoriTrader IB1Header.Declarant => JobDeclarationWrapper.ExportDeclarant;

	IRepresentative IB1Header.Representative => JobDeclarationWrapper.Representative;

	string IB1Header.InvoiceCurrencyCode => EntryHeaderWrapper.InvoiceCurrencyCode;

	decimal? IB1Header.InvoiceTotalAmount => EntryHeaderWrapper.InvoiceTotalAmount;

	decimal IB1Header.ExchangeRate => EntryHeaderWrapper.ExchangeRate;

	string IB1Header.InternalCurrencyUnit => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

	DateTime? IB1Header.GoodsPresentationDateTime => EntryInstructionWrapper.GoodsPresentationDateTime;

	string IB1Header.ExitCustomsOffice => JobDeclarationWrapper.ExitCustomsOffice;

	string IB1Header.ExportCustomsOffice => JobDeclarationWrapper.ExportCustomsOffice;

	string IB1Header.PresentationCustomsOffice => JobDeclarationWrapper.PresentationCustomsOffice;

	string IB1Header.SupervisingCustomsOffice => JobDeclarationWrapper.SupervisingCustomsOffice;

	IReadOnlyCollection<IPreviousDocument> IB1Header.PreviousDocuments => EntryInstructionWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> IB1Header.AdditionalInformation
		=> EntryInstructionWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument> IB1Header.SupportingDocuments
		=> EntryInstructionWrapper.SupportingDocuments ?? Array.Empty<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument>();

	IReadOnlyCollection<IAdditionalReference> IB1Header.AdditionalReferences
		=> EntryInstructionWrapper.AdditionalReferences ?? Array.Empty<IAdditionalReference>();

	IReadOnlyCollection<ITransportDocument> IB1Header.TransportDocuments
		=> EntryInstructionWrapper.TransportDocuments ?? Array.Empty<ITransportDocument>();

	string IB1Header.Ucr => JobDeclarationWrapper.Ucr;

	IWarehouse IB1Header.Warehouse => EntryInstructionWrapper.Warehouse;

	string IB1Header.CarrierIdentificationNumber => JobDeclarationWrapper.CarrierIdentificationNumber;

	IEoriTrader IB1Header.Consignor => EntryHeaderWrapper.Consignor;

	IEoriTrader IB1Header.Consignee => EntryHeaderWrapper.Consignee;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IB1Header.AdditionalSupplyChainActors => EntryInstructionWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	ITermsOfDelivery IB1Header.TermsOfDelivery => EntryHeaderWrapper.TermOfDelivery;

	string IB1Header.TransportChargesMethodOfPayment => EntryHeaderWrapper.TransportChargesMethodOfPayment;

	DateTime? IB1Header.AcceptanceDate => EntryInstructionWrapper.AcceptanceDate;

	string IB1Header.CountryOfDestination => EntryHeaderWrapper.CountryOfDestination;

	string IB1Header.CountryOfExport => EntryHeaderWrapper.CountryOfExport;

	ILocationOfGoods IB1Header.LocationOfGoods => JobDeclarationWrapper.ExportLocationOfGoods;

	IReadOnlyCollection<IConsignmentCountryRouting> IB1Header.ConsignmentRoutings => JobDeclarationWrapper.ConsignmentRoutings ?? Array.Empty<IConsignmentCountryRouting>();

	decimal? IB1Header.GrossMass => EntryHeaderWrapper.GrossMass;

	bool IB1Header.IsContainerizedTransport => JobDeclarationWrapper.IsContainerizedTransport;

	int IB1Header.BorderMeansOfTransportMode => JobDeclarationWrapper.BorderTransportMode;

	int? IB1Header.InlandTransportMode => EntryInstructionWrapper.InlandTransportMode;

	IReadOnlyCollection<IMeansOfTransport> IB1Header.DepartureMeansOfTransports => EntryInstructionWrapper.DepartureMeansOfTransports ?? Array.Empty<IMeansOfTransport>();

	IReadOnlyCollection<ITransportEquipment> IB1Header.TransportEquipment => EntryHeaderWrapper.TransportEquipment ?? Array.Empty<ITransportEquipment>();

	IMeansOfTransport IB1Header.ActiveBorderMeansOfTransport => JobDeclarationWrapper.GetExportBorderMeansOfTransport(entryInstruction);

	int? IB1Header.NatureOfTransaction => EntryHeaderWrapper.ExportNatureOfTransaction;

	#endregion

	#region Implementation

	void InitializeLazy(CusEntryHeader entryHeader, JobDeclaration declaration, CusEntryInstruction entryInstruction)
	{
		declarationWrapper = new Lazy<IJobDeclarationCustomsMessageWrapper>(() => exportMessageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper(declaration));
		entryHeaderWrapper = new Lazy<ICusEntryHeaderCustomsMessageWrapper>(() => exportMessageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(entryHeader));
		entryInstructionWrapper = new Lazy<IExportCusEntryInstructionCustomsMessageWrapper>(() => exportMessageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper(entryInstruction));
	}

	#endregion

	IExportCusEntryInstructionCustomsMessageWrapper EntryInstructionWrapper => entryInstructionWrapper.Value;
	IJobDeclarationCustomsMessageWrapper JobDeclarationWrapper => declarationWrapper.Value;
	ICusEntryHeaderCustomsMessageWrapper EntryHeaderWrapper => entryHeaderWrapper.Value;

	Lazy<IJobDeclarationCustomsMessageWrapper> declarationWrapper;
	Lazy<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapper;
	Lazy<IExportCusEntryInstructionCustomsMessageWrapper> entryInstructionWrapper;

	readonly CusEntryInstruction entryInstruction;
	readonly IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;
}
