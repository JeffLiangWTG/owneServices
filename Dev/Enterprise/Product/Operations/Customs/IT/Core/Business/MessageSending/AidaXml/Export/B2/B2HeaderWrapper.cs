using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class B2HeaderWrapper : IB2Header
{
	public B2HeaderWrapper(CusEntryHeader entryHeader, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.exportMessageSendingWrapperFactory = Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));

		Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));

		InitializeLazy();
	}

	public B2HeaderWrapper(CusEntryHeader entryHeader, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory, IDeclarationAmendment amendment)
		: this(entryHeader, exportMessageSendingWrapperFactory)
	{
		this.amendment = amendment;
	}

	readonly CusEntryHeader entryHeader;
	readonly CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;

	#region IB2Header

	string IB2Header.DeclarationCustomsOffice => JobDeclarationWrapper.DeclarationCustomsOffice;

	IDeclarationAmendment IB2Header.Amendment => amendment;

	string IB2Header.DeclarationType => JobDeclarationWrapper.EntryStyle;

	string IB2Header.AdditionalDeclarationType => EntryInstructionWrapper.AdditionalDeclarationType;

	bool IB2Header.IsSecurityDeclaration => JobDeclarationWrapper.IsSecurityDeclaration;

	string IB2Header.SpecificCircumstanceIndicator => JobDeclarationWrapper.SpecificCircumstanceIndicator;

	string IB2Header.Lrn => ITEDIMessage.ITMessageNumberPlaceholder;

	string IB2Header.DeferredPayment => EntryHeaderWrapper.DeferredPayment;

	IReadOnlyCollection<IAuthorization> IB2Header.Authorizations => EntryInstructionWrapper.Authorizations ?? Array.Empty<IAuthorization>();

	IEoriTrader IB2Header.Exporter => JobDeclarationWrapper.Exporter;

	IEoriTrader IB2Header.Declarant => JobDeclarationWrapper.ExportDeclarant;

	IRepresentative IB2Header.Representative => JobDeclarationWrapper.Representative;

	string IB2Header.InvoiceCurrencyCode => EntryHeaderWrapper.InvoiceCurrencyCode;

	decimal? IB2Header.InvoiceTotalAmount => EntryHeaderWrapper.InvoiceTotalAmount;

	decimal IB2Header.ExchangeRate => EntryHeaderWrapper.ExchangeRate;

	string IB2Header.InternalCurrencyUnit => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

	DateTime? IB2Header.GoodsPresentationDateTime => EntryInstructionWrapper.GoodsPresentationDateTime;

	string IB2Header.ExitCustomsOffice => JobDeclarationWrapper.ExitCustomsOffice;

	string IB2Header.ExportCustomsOffice => JobDeclarationWrapper.ExportCustomsOffice;

	string IB2Header.PresentationCustomsOffice => JobDeclarationWrapper.PresentationCustomsOffice;

	string IB2Header.SupervisingCustomsOffice => JobDeclarationWrapper.SupervisingCustomsOffice;

	IReadOnlyCollection<IPreviousDocument> IB2Header.PreviousDocuments => EntryInstructionWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> IB2Header.AdditionalInformation
		=> EntryInstructionWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument> IB2Header.SupportingDocuments
		=> EntryInstructionWrapper.SupportingDocuments ?? Array.Empty<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument>();

	IReadOnlyCollection<IAdditionalReference> IB2Header.AdditionalReferences
		=> EntryInstructionWrapper.AdditionalReferences ?? Array.Empty<IAdditionalReference>();

	IReadOnlyCollection<ITransportDocument> IB2Header.TransportDocuments
		=> EntryInstructionWrapper.TransportDocuments ?? Array.Empty<ITransportDocument>();

	string IB2Header.Ucr => JobDeclarationWrapper.Ucr;

	IWarehouse IB2Header.Warehouse => EntryInstructionWrapper.Warehouse;

	string IB2Header.CarrierIdentificationNumber => JobDeclarationWrapper.CarrierIdentificationNumber;

	IEoriTrader IB2Header.Consignee => EntryHeaderWrapper.Consignee;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IB2Header.AdditionalSupplyChainActors => EntryInstructionWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	ITermsOfDelivery IB2Header.TermsOfDelivery => EntryHeaderWrapper.TermOfDelivery;

	string IB2Header.TransportChargesMethodOfPayment => EntryHeaderWrapper.TransportChargesMethodOfPayment;

	string IB2Header.CountryOfDestination => JobDeclarationWrapper.GoodsCountryOfDestination;

	string IB2Header.CountryOfExport => JobDeclarationWrapper.CountryOfExport;

	ILocationOfGoods IB2Header.LocationOfGoods => JobDeclarationWrapper.ExportLocationOfGoods;

	IReadOnlyCollection<IConsignmentCountryRouting> IB2Header.ConsignmentRoutings => JobDeclarationWrapper.ConsignmentRoutings ?? Array.Empty<IConsignmentCountryRouting>();

	decimal? IB2Header.GrossMass => EntryHeaderWrapper.GrossMass;

	bool IB2Header.IsContainerizedTransport => JobDeclarationWrapper.IsContainerizedTransport;

	int IB2Header.BorderMeansOfTransportMode => JobDeclarationWrapper.BorderTransportMode;

	int? IB2Header.InlandTransportMode => EntryInstructionWrapper.InlandTransportMode;

	IReadOnlyCollection<IMeansOfTransport> IB2Header.DepartureMeansOfTransports => EntryInstructionWrapper.DepartureMeansOfTransports ?? Array.Empty<IMeansOfTransport>();

	IReadOnlyCollection<ITransportEquipment> IB2Header.TransportEquipment => EntryHeaderWrapper.TransportEquipment ?? Array.Empty<ITransportEquipment>();

	int? IB2Header.NatureOfTransaction => EntryHeaderWrapper.ExportNatureOfTransaction;

	DateTime? IB2Header.AcceptanceDate => EntryInstructionWrapper.AcceptanceDate;

	#endregion

	#region Implementation

	void InitializeLazy()
	{
		declarationWrapper = new Lazy<IJobDeclarationCustomsMessageWrapper>(() => exportMessageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper(entryHeader.Declaration));
		entryHeaderWrapper = new Lazy<ICusEntryHeaderCustomsMessageWrapper>(() => exportMessageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(entryHeader));
		entryInstructionWrapper = new Lazy<IExportCusEntryInstructionCustomsMessageWrapper>(() => exportMessageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper(entryHeader.EntryInstruction));
	}

	#endregion

	IExportCusEntryInstructionCustomsMessageWrapper EntryInstructionWrapper => entryInstructionWrapper.Value;
	IJobDeclarationCustomsMessageWrapper JobDeclarationWrapper => declarationWrapper.Value;
	ICusEntryHeaderCustomsMessageWrapper EntryHeaderWrapper => entryHeaderWrapper.Value;

	Lazy<IJobDeclarationCustomsMessageWrapper> declarationWrapper;
	Lazy<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapper;
	Lazy<IExportCusEntryInstructionCustomsMessageWrapper> entryInstructionWrapper;
}
