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

sealed class H3HeaderWrapper : IH3Header
{
	public H3HeaderWrapper(CusEntryHeader entryHeader, IMessageSendingWrapperFactory messageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		var declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
		this.messageSendingWrapperFactory = Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
		this.amendment = amendment;

		InitializeLazy(entryHeader, declaration, entryInstruction);
	}

	#region IH3Header

	IDeclarationAmendment IH3Header.Amendment => amendment;

	string IH3Header.DeclarationCustomsOffice => JobDeclarationWrapper.DeclarationCustomsOffice;

	string IH3Header.AdditionalDeclarationType => EntryInstructionWrapper.AdditionalDeclarationType;

	string IH3Header.SignerFiscalCode => lazySignerFiscalCode.Value;
	Lazy<string> lazySignerFiscalCode;

	string IH3Header.Lrn => ITEDIMessage.ITMessageNumberPlaceholder;

	string IH3Header.Ucr => JobDeclarationWrapper.Ucr;

	string IH3Header.DeferredPayment => EntryHeaderWrapper.DeferredPayment;

	IWarehouse IH3Header.Warehouse => EntryInstructionWrapper.Warehouse;

	IReadOnlyCollection<IPreviousDocument> IH3Header.PreviousDocuments => EntryInstructionWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<MessageBuilder.ISupportingDocument> IH3Header.SupportingDocuments
		=> EntryInstructionWrapper.SupportingDocuments ?? Array.Empty<MessageBuilder.ISupportingDocument>();

	IReadOnlyCollection<IAdditionalInformation> IH3Header.AdditionalInformation => EntryHeaderWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IEoriTrader IH3Header.Supplier => JobDeclarationWrapper.Supplier;

	IEoriTrader IH3Header.Importer => JobDeclarationWrapper.Importer;

	IEoriTrader IH3Header.Declarant => JobDeclarationWrapper.ImportDeclarant;

	IRepresentative IH3Header.Representative => JobDeclarationWrapper.Representative;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IH3Header.AdditionalSupplyChainActors
		=> EntryInstructionWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	string IH3Header.GuaranteeHolderIdentificationNumber
		=> EntryInstructionWrapper.GetGuaranteeHolderIdentificationNumber(JobDeclarationWrapper.ImportDeclarant);

	IReadOnlyCollection<IAuthorization> IH3Header.Authorizations => EntryInstructionWrapper.Authorizations ?? Array.Empty<IAuthorization>();

	ITermsOfDelivery IH3Header.TermsOfDelivery => EntryHeaderWrapper.TermOfDelivery;

	string IH3Header.InvoiceCurrencyCode => EntryHeaderWrapper.InvoiceCurrencyCode;

	decimal IH3Header.InvoiceTotalAmount => EntryHeaderWrapper.InvoiceTotalAmount.GetValueOrDefault();

	string IH3Header.InternalCurrencyUnit => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

	decimal? IH3Header.ExchangeRate => EntryHeaderWrapper.ExchangeRate;

	string IH3Header.CountryOfDestination => JobDeclarationWrapper.GoodsCountryOfDestination;

	string IH3Header.RegionOfDestination => JobDeclarationWrapper.RegionOfDestination;

	string IH3Header.CountryOfDispatch => JobDeclarationWrapper.GoodsCountryOfOrigin;

	ILocationOfGoods IH3Header.LocationOfGoods => JobDeclarationWrapper.ImportLocationOfGoods;

	string IH3Header.PresentationCustomsOffice => JobDeclarationWrapper.PresentationCustomsOffice;

	string IH3Header.SupervisingCustomsOffice => JobDeclarationWrapper.SupervisingCustomsOffice;

	DateTime? IH3Header.AcceptanceDate => EntryInstructionWrapper.AcceptanceDate;

	decimal IH3Header.GrossMass => EntryHeaderWrapper.GrossMass;

	int IH3Header.NumberOfPackages => EntryHeaderWrapper.NumberOfPackages;

	int IH3Header.ContainerMode => JobDeclarationWrapper.ContainerModeForImportMessage;

	int IH3Header.BorderMeansOfTransportMode => JobDeclarationWrapper.BorderTransportMode;

	int? IH3Header.InlandTransportMode => JobDeclarationWrapper.InlandTransportMode;

	string IH3Header.BorderMeansOfTransportNationality => JobDeclarationWrapper.BorderMeansOfTransportNationality;

	IArrivalMeansOfTransport IH3Header.ArrivalMeansOfTransport => JobDeclarationWrapper.ArrivalMeansOfTransport;

	IReadOnlyCollection<IGuarantee> IH3Header.Guarantees => EntryInstructionWrapper.Guarantees ?? Array.Empty<IGuarantee>();

	IReadOnlyCollection<string> IH3Header.GuaranteeTypes => EntryInstructionWrapper.GuaranteeTypes ?? Array.Empty<string>();

	int IH3Header.NatureOfTransaction => EntryHeaderWrapper.NatureOfTransaction;

	#endregion

	void InitializeLazy(CusEntryHeader entryHeader, JobDeclaration declaration, CusEntryInstruction entryInstruction)
	{
		declarationWrapper = new Lazy<IJobDeclarationCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper(declaration));
		entryHeaderWrapper = new Lazy<ICusEntryHeaderCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(entryHeader));
		lazySignerFiscalCode = new Lazy<string>(() => GlbStaff.CurrentUser.GetSignerFiscalCode());
		entryInstructionWrapper = new Lazy<ICusEntryInstructionCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper(entryInstruction));
	}

	ICusEntryInstructionCustomsMessageWrapper EntryInstructionWrapper => entryInstructionWrapper.Value;
	IJobDeclarationCustomsMessageWrapper JobDeclarationWrapper => declarationWrapper.Value;
	ICusEntryHeaderCustomsMessageWrapper EntryHeaderWrapper => entryHeaderWrapper.Value;

	Lazy<IJobDeclarationCustomsMessageWrapper> declarationWrapper;
	Lazy<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapper;
	Lazy<ICusEntryInstructionCustomsMessageWrapper> entryInstructionWrapper;

	readonly CusEntryInstruction entryInstruction;
	readonly IMessageSendingWrapperFactory messageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;
}
