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

sealed class I1HeaderWrapper : II1Header
{
	public I1HeaderWrapper(CusEntryHeader entryHeader, IMessageSendingWrapperFactory messageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		var declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
		this.messageSendingWrapperFactory = Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
		this.amendment = amendment;

		InitializeLazy(declaration, entryInstruction, entryHeader);
	}

	#region II1Header

	string II1Header.DeclarationCustomsOffice => JobDeclarationWrapper.DeclarationCustomsOffice;

	IDeclarationAmendment II1Header.Amendment => amendment;

	string II1Header.DeclarationEntryStyle => JobDeclarationWrapper.EntryStyle;

	string II1Header.AdditionalDeclarationType => EntryInstructionWrapper.AdditionalDeclarationType;

	string II1Header.SignerFiscalCode => lazySignerFiscalCode.Value;
	Lazy<string> lazySignerFiscalCode;

	IReadOnlyCollection<IPreviousDocument> II1Header.PreviousDocuments => EntryInstructionWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> II1Header.AdditionalInformation => EntryHeaderWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<MessageBuilder.ISupportingDocument> II1Header.SupportingDocuments => EntryInstructionWrapper.SupportingDocuments ?? Array.Empty<MessageBuilder.ISupportingDocument>();

	string II1Header.Ucr => JobDeclarationWrapper.Ucr;

	string II1Header.Lrn => ITEDIMessage.ITMessageNumberPlaceholder;

	IEoriTrader II1Header.Supplier => JobDeclarationWrapper.Supplier;

	IEoriTrader II1Header.Importer => JobDeclarationWrapper.Importer;

	IEoriTrader II1Header.Declarant => JobDeclarationWrapper.ImportDeclarant;

	IRepresentative II1Header.Representative => JobDeclarationWrapper.Representative;

	IReadOnlyCollection<IAdditionalSupplyChainActor> II1Header.AdditionalSupplyChainActors => EntryInstructionWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	IReadOnlyCollection<IAuthorization> II1Header.Authorizations => EntryInstructionWrapper.Authorizations ?? Array.Empty<IAuthorization>();

	string II1Header.InvoiceCurrencyCode => EntryHeaderWrapper.InvoiceCurrencyCode;

	decimal? II1Header.InvoiceTotalAmount => lazyInvoiceTotalAmount.Value;
	Lazy<decimal?> lazyInvoiceTotalAmount;

	string II1Header.CountryOfDispatch => JobDeclarationWrapper.GoodsCountryOfOrigin;

	ILocationOfGoods II1Header.LocationOfGoods => JobDeclarationWrapper.ImportLocationOfGoods;

	string II1Header.PresentationCustomsOffice => JobDeclarationWrapper.PresentationCustomsOffice;

	string II1Header.SupervisingCustomsOffice => JobDeclarationWrapper.SupervisingCustomsOffice;

	decimal II1Header.GrossMass => EntryHeaderWrapper.GrossMass;

	string II1Header.CustomsDutyPayerIdentificationNumber => JobDeclarationWrapper.DutyPayerIdentificationNumber;

	#endregion

	#region Implementation

	void InitializeLazy(JobDeclaration declaration, CusEntryInstruction entryInstruction, CusEntryHeader entryHeader)
	{
		declarationWrapper = new Lazy<IJobDeclarationCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper(declaration));
		entryHeaderWrapper = new Lazy<ICusEntryHeaderCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(entryHeader));
		lazySignerFiscalCode = new Lazy<string>(() => GlbStaff.CurrentUser.GetSignerFiscalCode());
		entryInstructionWrapper = new Lazy<ICusEntryInstructionCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper(entryInstruction));
		lazyInvoiceTotalAmount = new Lazy<decimal?>(GetInvoiceTotalAmount);
	}

	decimal? GetInvoiceTotalAmount()
	{
		return !EntryHeaderWrapper.InvoiceCurrencyCode.IsEmpty ? EntryHeaderWrapper.InvoiceTotalAmount.GetValueOrDefault() : null;
	}

	IJobDeclarationCustomsMessageWrapper JobDeclarationWrapper => declarationWrapper.Value;
	ICusEntryInstructionCustomsMessageWrapper EntryInstructionWrapper => entryInstructionWrapper.Value;
	ICusEntryHeaderCustomsMessageWrapper EntryHeaderWrapper => entryHeaderWrapper.Value;

	Lazy<IJobDeclarationCustomsMessageWrapper> declarationWrapper;
	Lazy<ICusEntryInstructionCustomsMessageWrapper> entryInstructionWrapper;
	Lazy<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapper;

	readonly IMessageSendingWrapperFactory messageSendingWrapperFactory;
	readonly CusEntryInstruction entryInstruction;
	readonly IDeclarationAmendment amendment;

	#endregion
}
