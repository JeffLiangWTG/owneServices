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

sealed class H4HeaderWrapper : IH4Header
{
	public H4HeaderWrapper(CusEntryHeader entryHeader, IMessageSendingWrapperFactory messageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		var declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
		this.messageSendingWrapperFactory = Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
		this.amendment = amendment;

		InitializeLazy(entryHeader, declaration, entryInstruction);
	}

	string IH4Header.DeclarationCustomsOffice => JobDeclarationWrapper.DeclarationCustomsOffice;

	string IH4Header.AdditionalDeclarationType => EntryInstructionWrapper.AdditionalDeclarationType;

	string IH4Header.SignerFiscalCode => lazySignerFiscalCode.Value;
	Lazy<string> lazySignerFiscalCode;

	IDeclarationAmendment IH4Header.Amendment => amendment;

	IReadOnlyCollection<IAdditionalInformation> IH4Header.AdditionalInformation => EntryHeaderWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<IPreviousDocument> IH4Header.PreviousDocuments => EntryInstructionWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<MessageBuilder.ISupportingDocument> IH4Header.SupportingDocuments
		=> EntryInstructionWrapper.SupportingDocuments ?? Array.Empty<MessageBuilder.ISupportingDocument>();

	string IH4Header.Ucr => JobDeclarationWrapper.Ucr;

	string IH4Header.Lrn => ITEDIMessage.ITMessageNumberPlaceholder;

	string IH4Header.DeferredPayment => EntryHeaderWrapper.DeferredPayment;

	IWarehouse IH4Header.Warehouse => EntryInstructionWrapper.Warehouse;

	IEoriTrader IH4Header.Supplier => JobDeclarationWrapper.Supplier;

	IEoriTrader IH4Header.Importer => JobDeclarationWrapper.Importer;

	IEoriTrader IH4Header.Declarant => JobDeclarationWrapper.ImportDeclarant;

	IRepresentative IH4Header.Representative => JobDeclarationWrapper.Representative;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IH4Header.AdditionalSupplyChainActors
		=> EntryInstructionWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	IReadOnlyCollection<IAuthorization> IH4Header.Authorizations => EntryInstructionWrapper.Authorizations ?? Array.Empty<IAuthorization>();

	string IH4Header.GuaranteeHolderIdentificationNumber
		=> EntryInstructionWrapper.GetGuaranteeHolderIdentificationNumber(JobDeclarationWrapper.ImportDeclarant);

	ITermsOfDelivery IH4Header.TermsOfDelivery => EntryHeaderWrapper.TermOfDelivery;

	string IH4Header.InvoiceCurrencyCode => EntryHeaderWrapper.InvoiceCurrencyCode;

	decimal? IH4Header.InvoiceTotalAmount => EntryHeaderWrapper.InvoiceTotalAmount.GetValueOrDefault();

	string IH4Header.InternalCurrencyUnit => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

	decimal IH4Header.ExchangeRate => EntryHeaderWrapper.ExchangeRate;

	string IH4Header.CountryOfDestination => JobDeclarationWrapper.GoodsCountryOfDestination;

	string IH4Header.RegionOfDestination => JobDeclarationWrapper.RegionOfDestination;

	string IH4Header.CountryOfDispatch => JobDeclarationWrapper.GoodsCountryOfOrigin;

	ILocationOfGoods IH4Header.LocationOfGoods => JobDeclarationWrapper.ImportLocationOfGoods;

	string IH4Header.PresentationCustomsOffice => JobDeclarationWrapper.PresentationCustomsOffice;

	string IH4Header.SupervisingCustomsOffice => JobDeclarationWrapper.SupervisingCustomsOffice;

	DateTime? IH4Header.AcceptanceDate => EntryInstructionWrapper.AcceptanceDate;

	decimal IH4Header.GrossMass => EntryHeaderWrapper.GrossMass;

	int IH4Header.NumberOfPackages => EntryHeaderWrapper.NumberOfPackages;

	bool IH4Header.IsContainerizedTransport => JobDeclarationWrapper.IsContainerizedTransport;

	int IH4Header.BorderMeansOfTransportMode => JobDeclarationWrapper.BorderTransportMode;

	int? IH4Header.InlandTransportMode => JobDeclarationWrapper.InlandTransportMode;

	IArrivalMeansOfTransport IH4Header.ArrivalMeansOfTransport => JobDeclarationWrapper.ArrivalMeansOfTransport;

	string IH4Header.BorderMeansOfTransportNationality => JobDeclarationWrapper.BorderMeansOfTransportNationality;

	IReadOnlyCollection<string> IH4Header.GuaranteeTypes => EntryInstructionWrapper.GuaranteeTypes ?? Array.Empty<string>();

	IReadOnlyCollection<IGuarantee> IH4Header.Guarantees => EntryInstructionWrapper.Guarantees ?? Array.Empty<IGuarantee>();

	int IH4Header.NatureOfTransaction => EntryHeaderWrapper.NatureOfTransaction;

	#region Implementation

	void InitializeLazy(CusEntryHeader entryHeader, JobDeclaration declaration, CusEntryInstruction entryInstruction)
	{
		declarationWrapper = new Lazy<IJobDeclarationCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper(declaration));
		entryHeaderWrapper = new Lazy<ICusEntryHeaderCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(entryHeader));
		lazySignerFiscalCode = new Lazy<string>(() => GlbStaff.CurrentUser.GetSignerFiscalCode());
		entryInstructionWrapper = new Lazy<ICusEntryInstructionCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper(entryInstruction));
	}

	#endregion

	IJobDeclarationCustomsMessageWrapper JobDeclarationWrapper => declarationWrapper.Value;
	ICusEntryInstructionCustomsMessageWrapper EntryInstructionWrapper => entryInstructionWrapper.Value;
	ICusEntryHeaderCustomsMessageWrapper EntryHeaderWrapper => entryHeaderWrapper.Value;

	Lazy<IJobDeclarationCustomsMessageWrapper> declarationWrapper;
	Lazy<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapper;
	Lazy<ICusEntryInstructionCustomsMessageWrapper> entryInstructionWrapper;

	readonly CusEntryInstruction entryInstruction;
	readonly IMessageSendingWrapperFactory messageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;
}
