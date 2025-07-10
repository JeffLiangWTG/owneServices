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
using CustomsMessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class H2HeaderWrapper : IH2Header
{
	public H2HeaderWrapper(CusEntryHeader entryHeader, IMessageSendingWrapperFactory messageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		var declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
		this.messageSendingWrapperFactory = Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
		this.amendment = amendment;

		InitializeLazy(entryHeader, declaration, entryInstruction);
	}

	#region IH2Header

	string IH2Header.DeclarationCustomsOffice => JobDeclarationWrapper.DeclarationCustomsOffice;

	string IH2Header.AdditionalDeclarationType => EntryInstructionWrapper.AdditionalDeclarationType;

	IDeclarationAmendment IH2Header.Amendment => amendment;

	string IH2Header.SignerFiscalCode => lazySignerFiscalCode.Value;
	Lazy<string> lazySignerFiscalCode;

	IReadOnlyCollection<IAdditionalInformation> IH2Header.AdditionalInformation => EntryHeaderWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<IPreviousDocument> IH2Header.PreviousDocuments => EntryInstructionWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<CustomsMessageBuilder.ISupportingDocument> IH2Header.SupportingDocuments
		=> EntryInstructionWrapper.SupportingDocuments ?? Array.Empty<CustomsMessageBuilder.ISupportingDocument>();

	string IH2Header.Ucr => JobDeclarationWrapper.Ucr;

	string IH2Header.Lrn => ITEDIMessage.ITMessageNumberPlaceholder;

	IWarehouse IH2Header.Warehouse => EntryInstructionWrapper.Warehouse;

	IEoriTrader IH2Header.Importer => JobDeclarationWrapper.Importer;

	IEoriTrader IH2Header.Declarant => JobDeclarationWrapper.ImportDeclarant;

	IRepresentative IH2Header.Representative => JobDeclarationWrapper.Representative;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IH2Header.AdditionalSupplyChainActors => EntryInstructionWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	IReadOnlyCollection<IAuthorization> IH2Header.Authorizations => EntryInstructionWrapper.Authorizations ?? Array.Empty<IAuthorization>();

	string IH2Header.CountryOfDestination => JobDeclarationWrapper.GoodsCountryOfDestination;

	string IH2Header.RegionOfDestination => JobDeclarationWrapper.RegionOfDestination;

	string IH2Header.CountryOfDispatch => JobDeclarationWrapper.GoodsCountryOfOrigin;

	ILocationOfGoods IH2Header.LocationOfGoods => JobDeclarationWrapper.ImportLocationOfGoods;

	string IH2Header.PresentationCustomsOffice => JobDeclarationWrapper.PresentationCustomsOffice;

	string IH2Header.SupervisingCustomsOffice => JobDeclarationWrapper.SupervisingCustomsOffice;

	decimal IH2Header.GrossMass => EntryHeaderWrapper.GrossMass;

	bool IH2Header.IsContainerizedTransport => JobDeclarationWrapper.IsContainerizedTransport;

	int IH2Header.BorderMeansOfTransportMode => JobDeclarationWrapper.BorderTransportMode;

	int? IH2Header.InlandTransportMode => JobDeclarationWrapper.InlandTransportMode;

	int IH2Header.NatureOfTransaction => EntryHeaderWrapper.NatureOfTransaction;

	#endregion

	#region Implementation

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

	#endregion

	Lazy<IJobDeclarationCustomsMessageWrapper> declarationWrapper;
	Lazy<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapper;
	Lazy<ICusEntryInstructionCustomsMessageWrapper> entryInstructionWrapper;

	readonly CusEntryInstruction entryInstruction;
	readonly IMessageSendingWrapperFactory messageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;
}
