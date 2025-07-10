using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class B4HeaderWrapper : IB4Header
{
	public B4HeaderWrapper(CusEntryHeader entryHeader, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.exportMessageSendingWrapperFactory = Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));

		Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));

		InitializeLazy();
	}

	public B4HeaderWrapper(CusEntryHeader entryHeader, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory, IDeclarationAmendment amendment)
		: this(entryHeader, exportMessageSendingWrapperFactory)
	{
		this.amendment = amendment;
	}

	readonly CusEntryHeader entryHeader;
	readonly CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;

	#region IB4Header

	string IB4Header.DeclarationCustomsOffice => JobDeclarationWrapper.DeclarationCustomsOffice;

	IDeclarationAmendment IB4Header.Amendment => amendment;

	string IB4Header.DeclarationType => JobDeclarationWrapper.EntryStyle;

	string IB4Header.AdditionalDeclarationType => EntryInstructionWrapper.AdditionalDeclarationType;

	string IB4Header.Lrn => ITEDIMessage.ITMessageNumberPlaceholder;

	DateTime? IB4Header.AcceptanceDate => EntryInstructionWrapper.AcceptanceDate;

	IReadOnlyCollection<IAuthorization> IB4Header.Authorizations => EntryInstructionWrapper.Authorizations ?? Array.Empty<IAuthorization>();

	IEoriTrader IB4Header.Exporter => JobDeclarationWrapper.Exporter;

	IEoriTrader IB4Header.Declarant => JobDeclarationWrapper.ExportDeclarant;

	IRepresentative IB4Header.Representative => JobDeclarationWrapper.Representative;

	string IB4Header.InvoiceCurrencyCode => EntryHeaderWrapper.InvoiceCurrencyCode;

	decimal? IB4Header.InvoiceTotalAmount => EntryHeaderWrapper.InvoiceTotalAmount;

	DateTime? IB4Header.GoodsPresentationDateTime => EntryInstructionWrapper.GoodsPresentationDateTime;

	string IB4Header.ExitCustomsOffice => JobDeclarationWrapper.ExitCustomsOffice;

	string IB4Header.ExportCustomsOffice => JobDeclarationWrapper.ExportCustomsOffice;

	string IB4Header.PresentationCustomsOffice => JobDeclarationWrapper.PresentationCustomsOffice;

	IReadOnlyCollection<IPreviousDocument> IB4Header.PreviousDocuments => EntryInstructionWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> IB4Header.AdditionalInformation
		=> EntryInstructionWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument> IB4Header.SupportingDocuments
		=> EntryInstructionWrapper.SupportingDocuments ?? Array.Empty<CargoWise.Customs.IT.MessageContracts.Declaration.ISupportingDocument>();

	IReadOnlyCollection<ITransportDocument> IB4Header.TransportDocuments
		=> EntryInstructionWrapper.TransportDocuments ?? Array.Empty<ITransportDocument>();

	string IB4Header.Ucr => JobDeclarationWrapper.Ucr;

	IWarehouse IB4Header.Warehouse => EntryInstructionWrapper.Warehouse;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IB4Header.AdditionalSupplyChainActors => EntryInstructionWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	int IB4Header.BorderMeansOfTransportMode => JobDeclarationWrapper.BorderTransportMode;

	string IB4Header.CarrierIdentificationNumber => JobDeclarationWrapper.CarrierIdentificationNumber;

	IEoriTrader IB4Header.Consignee => EntryHeaderWrapper.Consignee;

	ITermsOfDelivery IB4Header.TermsOfDelivery => EntryHeaderWrapper.TermOfDelivery;

	string IB4Header.TransportChargesMethodOfPayment => EntryHeaderWrapper.TransportChargesMethodOfPayment;

	string IB4Header.CountryOfDestination => JobDeclarationWrapper.GoodsCountryOfDestination;

	string IB4Header.CountryOfExport => JobDeclarationWrapper.CountryOfExport;

	ILocationOfGoods IB4Header.LocationOfGoods => JobDeclarationWrapper.ExportLocationOfGoods;

	IReadOnlyCollection<IConsignmentCountryRouting> IB4Header.ConsignmentRoutings => JobDeclarationWrapper.ConsignmentRoutings ?? Array.Empty<IConsignmentCountryRouting>();

	decimal? IB4Header.GrossMass => EntryHeaderWrapper.GrossMass;

	bool IB4Header.IsContainerizedTransport => JobDeclarationWrapper.IsContainerizedTransport;

	IReadOnlyCollection<ITransportEquipment> IB4Header.TransportEquipment => EntryHeaderWrapper.TransportEquipment ?? Array.Empty<ITransportEquipment>();

	int? IB4Header.NatureOfTransaction => EntryHeaderWrapper.ExportNatureOfTransaction;

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
