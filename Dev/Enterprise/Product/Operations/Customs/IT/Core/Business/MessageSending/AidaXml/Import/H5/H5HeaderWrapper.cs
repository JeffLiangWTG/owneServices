using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;
using MessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public sealed class H5HeaderWrapper : IH5Header
{
	public H5HeaderWrapper(CusEntryHeader entryHeader, IMessageSendingWrapperFactory messageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		var declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
		this.messageSendingWrapperFactory = Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
		this.amendment = amendment;

		InitializeLazy(entryHeader, declaration, entryInstruction);
	}

	DateTime? IH5Header.AcceptanceDate => EntryInstructionWrapper.AcceptanceDate;

	IReadOnlyCollection<IAdditionalInformation> IH5Header.AdditionalInformation => EntryHeaderWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<IAdditionalSupplyChainActor> IH5Header.AdditionalSupplyChainActors
		=> EntryInstructionWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	IReadOnlyCollection<IAdditionOrDeduction> IH5Header.AdditionOrDeductions => Array.Empty<IAdditionOrDeduction>();

	IDeclarationAmendment IH5Header.Amendment => amendment;

	IArrivalMeansOfTransport IH5Header.ArrivalMeansOfTransport => JobDeclarationWrapper.ArrivalMeansOfTransport;

	IReadOnlyCollection<IAuthorization> IH5Header.Authorizations
		=> EntryInstructionWrapper.Authorizations ?? Array.Empty<IAuthorization>();

	int IH5Header.BorderMeansOfTransportMode => JobDeclarationWrapper.BorderTransportMode;

	string IH5Header.BorderMeansOfTransportNationality => JobDeclarationWrapper.BorderMeansOfTransportNationality;

	string IH5Header.CountryOfDestination => JobDeclarationWrapper.GoodsCountryOfDestination;

	string IH5Header.CountryOfDispatch => JobDeclarationWrapper.GoodsCountryOfOrigin;

	string IH5Header.CustomsDutyPayerIdentificationNumber => JobDeclarationWrapper.DutyPayerIdentificationNumber;

	IEoriTrader IH5Header.Declarant => JobDeclarationWrapper.ImportDeclarant;

	string IH5Header.DeclarationCustomsOffice => JobDeclarationWrapper.DeclarationCustomsOffice;

	decimal IH5Header.GrossMass => EntryHeaderWrapper.GrossMass;

	IEoriTrader IH5Header.Importer => JobDeclarationWrapper.Importer;

	int IH5Header.InlandTransportMode => JobDeclarationWrapper.InlandTransportMode.GetValueOrDefault();

	string IH5Header.InvoiceCurrencyCode => EntryHeaderWrapper.InvoiceCurrencyCode;

	decimal? IH5Header.InvoiceTotalAmount => EntryHeaderWrapper.InvoiceTotalAmount;

	ILocationOfGoods IH5Header.LocationOfGoods => JobDeclarationWrapper.ImportLocationOfGoods;

	string IH5Header.Lrn => ITEDIMessage.ITMessageNumberPlaceholder;

	int? IH5Header.NatureOfTransaction => EntryHeaderWrapper.NatureOfTransaction;

	int IH5Header.NumberOfPackages => EntryHeaderWrapper.NumberOfPackages;

	string IH5Header.PresentationCustomsOffice => JobDeclarationWrapper.PresentationCustomsOffice;

	IReadOnlyCollection<IPreviousDocument> IH5Header.PreviousDocuments
		=> EntryInstructionWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	string IH5Header.RegionOfDestination => JobDeclarationWrapper.RegionOfDestination;

	IRepresentative IH5Header.Representative => JobDeclarationWrapper.Representative;

	string IH5Header.SignerFiscalCode => lazySignerFiscalCode.Value;
	Lazy<string> lazySignerFiscalCode;

	IEoriTrader IH5Header.Supplier => JobDeclarationWrapper.Supplier;

	IReadOnlyCollection<MessageBuilder.ISupportingDocument> IH5Header.SupportingDocuments
		=> EntryInstructionWrapper.SupportingDocuments ?? Array.Empty<MessageBuilder.ISupportingDocument>();

	ITermsOfDelivery IH5Header.TermsOfDelivery => EntryHeaderWrapper.TermOfDelivery;

	string IH5Header.Ucr => JobDeclarationWrapper.Ucr;

	IWarehouse IH5Header.Warehouse => EntryInstructionWrapper.Warehouse;

	void InitializeLazy(CusEntryHeader entryHeader, JobDeclaration declaration, CusEntryInstruction entryInstruction)
	{
		declarationWrapper = new Lazy<IJobDeclarationCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper(declaration));
		entryHeaderWrapper = new Lazy<ICusEntryHeaderCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(entryHeader));
		lazySignerFiscalCode = new Lazy<string>(() => GlbStaff.CurrentUser.GetSignerFiscalCode());
		entryInstructionWrapper = new Lazy<ICusEntryInstructionCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper(entryInstruction));
	}

	Lazy<IJobDeclarationCustomsMessageWrapper> declarationWrapper;
	Lazy<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapper;
	Lazy<ICusEntryInstructionCustomsMessageWrapper> entryInstructionWrapper;

	readonly CusEntryInstruction entryInstruction;
	readonly IMessageSendingWrapperFactory messageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;

	IJobDeclarationCustomsMessageWrapper JobDeclarationWrapper => declarationWrapper.Value;
	ICusEntryInstructionCustomsMessageWrapper EntryInstructionWrapper => entryInstructionWrapper.Value;
	ICusEntryHeaderCustomsMessageWrapper EntryHeaderWrapper => entryHeaderWrapper.Value;
}
