using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.IT.Business.MessageProcessorConstants;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class C2HeaderWrapper : IC2Header
{
	public C2HeaderWrapper(CusEntryHeader entryHeader, CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory, IDeclarationAmendment amendment = null)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		this.exportMessageSendingWrapperFactory = Argument.NotNull(exportMessageSendingWrapperFactory, nameof(exportMessageSendingWrapperFactory));

		var declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		var entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
		this.amendment = amendment;

		InitializeLazy(entryHeader, declaration, entryInstruction);
	}

	#region IC2Header

	string IC2Header.DeclarationCustomsOffice => JobDeclarationWrapper.DeclarationCustomsOffice;

	IDeclarationAmendment IC2Header.Amendment => amendment;

	string IC2Header.SpecificCircumstanceIndicator => JobDeclarationWrapper.SpecificCircumstanceIndicator;

	string IC2Header.Lrn => ITEDIMessage.ITMessageNumberPlaceholder;

	IReadOnlyCollection<IAuthorization> IC2Header.Authorizations => EntryInstructionWrapper.Authorizations ?? Array.Empty<IAuthorization>();

	IEoriTrader IC2Header.Declarant => JobDeclarationWrapper.ExportDeclarant;

	IRepresentative IC2Header.Representative => JobDeclarationWrapper.Representative;

	string IC2Header.PresentationCustomsOffice => JobDeclarationWrapper.PresentationCustomsOffice;

	string IC2Header.PreviousLrn => previousLrn.Value;

	IReadOnlyCollection<IPreviousDocument> IC2Header.PreviousDocuments => EntryInstructionWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	ITermsOfDelivery IC2Header.TermsOfDelivery => EntryHeaderWrapper.TermOfDelivery;

	string IC2Header.TransportChargesMethodOfPayment => EntryHeaderWrapper.TransportChargesMethodOfPayment;

	ILocationOfGoods IC2Header.LocationOfGoods => JobDeclarationWrapper.ExportLocationOfGoods;

	IReadOnlyCollection<IConsignmentCountryRouting> IC2Header.ConsignmentRoutings => JobDeclarationWrapper.ConsignmentRoutings ?? Array.Empty<IConsignmentCountryRouting>();

	DateTime? IC2Header.AcceptanceDate => EntryInstructionWrapper.AcceptanceDate;

	#endregion

	void InitializeLazy(CusEntryHeader entryHeader, JobDeclaration declaration, CusEntryInstruction entryInstruction)
	{
		declarationWrapper = new Lazy<IJobDeclarationCustomsMessageWrapper>(() => exportMessageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper(declaration));
		entryInstructionWrapper = new Lazy<IExportCusEntryInstructionCustomsMessageWrapper>(() => exportMessageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper(entryInstruction));
		entryHeaderWrapper = new Lazy<ICusEntryHeaderCustomsMessageWrapper>(() => exportMessageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(entryHeader));
		previousLrn = new Lazy<ZString>(() => GetPreviousLrn(entryHeader));
	}

	ZString GetPreviousLrn(CusEntryHeader entryHeader) => entryHeader.Messages
		.Cast<EDIMessage>()
		.Where(x => x.EM_MessageType == InterchangeTypes.Ucc6ResponseMessageType)
		.Select(x => (new Ucc6ExportResponseMessage(x.EM_MessageText) as IResponseMessageWithWrapper).GetResponseMessageContents())
		.Where(x => x.State == 2 && !string.IsNullOrWhiteSpace(x.Lrn))
		.OrderByDescending(x => x.ProcessingEndAtUtc)
		.Select(x => x.Lrn)
		.FirstOrDefault() ?? ZString.Empty;

	IJobDeclarationCustomsMessageWrapper JobDeclarationWrapper => declarationWrapper.Value;
	IExportCusEntryInstructionCustomsMessageWrapper EntryInstructionWrapper => entryInstructionWrapper.Value;
	ICusEntryHeaderCustomsMessageWrapper EntryHeaderWrapper => entryHeaderWrapper.Value;

	Lazy<IJobDeclarationCustomsMessageWrapper> declarationWrapper;
	Lazy<IExportCusEntryInstructionCustomsMessageWrapper> entryInstructionWrapper;
	Lazy<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapper;
	Lazy<ZString> previousLrn;

	readonly CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	readonly IDeclarationAmendment amendment;
}
