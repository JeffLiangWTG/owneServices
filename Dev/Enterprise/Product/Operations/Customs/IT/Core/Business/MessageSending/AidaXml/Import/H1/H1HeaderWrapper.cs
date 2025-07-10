using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;

using CustomsMessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class H1HeaderWrapper : IH1Header
{
	public H1HeaderWrapper(CusEntryHeader entryHeader, IMessageSendingWrapperFactory messageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		var declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
		this.messageSendingWrapperFactory = Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
		this.amendment = amendment;

		InitializeLazy(entryHeader, declaration, entryInstruction);
	}

	#region IH1Header

	DateTime? IH1Header.AcceptanceDate => EntryInstructionWrapper.AcceptanceDate;

	string IH1Header.AdditionalDeclarationType => EntryInstructionWrapper.AdditionalDeclarationType;

	IReadOnlyCollection<IAdditionalInformation> IH1Header.AdditionalInformation => EntryHeaderWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<IAdditionOrDeduction> IH1Header.AdditionOrDeductions => lazyAdditionOrDeductions.Value;
	Lazy<IReadOnlyCollection<IAdditionOrDeduction>> lazyAdditionOrDeductions;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IH1Header.AdditionalSupplyChainActors
		=> EntryInstructionWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	IDeclarationAmendment IH1Header.Amendment => amendment;

	IArrivalMeansOfTransport IH1Header.ArrivalMeansOfTransport => JobDeclarationWrapper.ArrivalMeansOfTransport;

	IReadOnlyCollection<IAuthorization> IH1Header.Authorizations => EntryInstructionWrapper.Authorizations ?? Array.Empty<IAuthorization>();

	int IH1Header.BorderMeansOfTransportMode => JobDeclarationWrapper.BorderTransportMode;

	string IH1Header.BorderMeansOfTransportNationality => JobDeclarationWrapper.BorderMeansOfTransportNationality;

	IEoriTrader IH1Header.Buyer => JobDeclarationWrapper.Buyer;

	string IH1Header.CountryOfDestination => JobDeclarationWrapper.GoodsCountryOfDestination;

	string IH1Header.CountryOfDispatch => JobDeclarationWrapper.GoodsCountryOfOrigin;

	string IH1Header.CustomsDutyPayerIdentificationNumber => JobDeclarationWrapper.DutyPayerIdentificationNumber;

	IEoriTrader IH1Header.Declarant => JobDeclarationWrapper.ImportDeclarant;

	string IH1Header.DeclarationCustomsOffice => JobDeclarationWrapper.DeclarationCustomsOffice;

	string IH1Header.DeferredPayment => EntryHeaderWrapper.DeferredPayment;

	decimal IH1Header.ExchangeRate => EntryHeaderWrapper.ExchangeRate;

	IReadOnlyCollection<IFiscalReference> IH1Header.FiscalReferences => EntryInstructionWrapper.FiscalReferences ?? Array.Empty<IFiscalReference>();

	decimal IH1Header.GrossMass => EntryHeaderWrapper.GrossMass;

	string IH1Header.GuaranteeHolderIdentificationNumber
		=> EntryInstructionWrapper.GetGuaranteeHolderIdentificationNumber(JobDeclarationWrapper.ImportDeclarant);

	IReadOnlyCollection<IGuarantee> IH1Header.Guarantees => EntryInstructionWrapper.Guarantees ?? Array.Empty<IGuarantee>();

	IReadOnlyCollection<string> IH1Header.GuaranteeTypes => EntryInstructionWrapper.GuaranteeTypes ?? Array.Empty<string>();

	IEoriTrader IH1Header.Importer => JobDeclarationWrapper.Importer;

	int? IH1Header.InlandTransportMode => JobDeclarationWrapper.InlandTransportMode;

	string IH1Header.InternalCurrencyUnit => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

	string IH1Header.InvoiceCurrencyCode => EntryHeaderWrapper.InvoiceCurrencyCode;

	decimal? IH1Header.InvoiceTotalAmount => EntryHeaderWrapper.InvoiceTotalAmount;

	bool IH1Header.IsContainerizedTransport => JobDeclarationWrapper.ContainerModeForImportMessage == 1;

	ILocationOfGoods IH1Header.LocationOfGoods => JobDeclarationWrapper.ImportLocationOfGoods;

	string IH1Header.Lrn => ITEDIMessage.ITMessageNumberPlaceholder;

	int IH1Header.NatureOfTransaction => EntryHeaderWrapper.NatureOfTransaction;

	int IH1Header.NumberOfPackages => EntryHeaderWrapper.NumberOfPackages;

	string IH1Header.PresentationCustomsOffice => JobDeclarationWrapper.PresentationCustomsOffice;

	IReadOnlyCollection<IPreviousDocument> IH1Header.PreviousDocuments => EntryInstructionWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	string IH1Header.RegionOfDestination => JobDeclarationWrapper.RegionOfDestination;

	IRepresentative IH1Header.Representative => JobDeclarationWrapper.Representative;

	IEoriTrader IH1Header.Seller => JobDeclarationWrapper.Seller;

	string IH1Header.SignerFiscalCode => lazySignerFiscalCode.Value;
	Lazy<string> lazySignerFiscalCode;

	string IH1Header.SupervisingCustomsOffice => JobDeclarationWrapper.SupervisingCustomsOffice;

	IEoriTrader IH1Header.Supplier => JobDeclarationWrapper.Supplier;

	IReadOnlyCollection<CustomsMessageBuilder.ISupportingDocument> IH1Header.SupportingDocuments
		=> EntryInstructionWrapper.SupportingDocuments ?? Array.Empty<CustomsMessageBuilder.ISupportingDocument>();

	ITermsOfDelivery IH1Header.TermsOfDelivery => EntryHeaderWrapper.TermOfDelivery;

	string IH1Header.Ucr => JobDeclarationWrapper.Ucr;

	IWarehouse IH1Header.Warehouse => EntryInstructionWrapper.Warehouse;

	#endregion

	#region Implementation

	void InitializeLazy(CusEntryHeader entryHeader, JobDeclaration declaration, CusEntryInstruction entryInstruction)
	{
		lazyAdditionOrDeductions = new Lazy<IReadOnlyCollection<IAdditionOrDeduction>>(GetAdditionOrDeductions);
		declarationWrapper = new Lazy<IJobDeclarationCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper(declaration));
		entryHeaderWrapper = new Lazy<ICusEntryHeaderCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(entryHeader));
		lazySignerFiscalCode = new Lazy<string>(() => GlbStaff.CurrentUser.GetSignerFiscalCode());
		entryInstructionWrapper = new Lazy<ICusEntryInstructionCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper(entryInstruction));
	}

	IReadOnlyCollection<IAdditionOrDeduction> GetAdditionOrDeductions() => new Collection<IAdditionOrDeduction>();

	#endregion

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
