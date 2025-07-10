using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

sealed class ExportCusEntryInstructionCustomsMessageWrapper : CusEntryInstructionCustomsMessageWrapper, IExportCusEntryInstructionCustomsMessageWrapper
{
	public ExportCusEntryInstructionCustomsMessageWrapper(CusEntryInstruction entryInstruction) : base(entryInstruction)
	{
		InitializeLazy();
	}

	#region IExportCusEntryInstructionCustomsMessageWrapper

	IReadOnlyCollection<IAdditionalInformation> IExportCusEntryInstructionCustomsMessageWrapper.AdditionalInformation => lazyAdditionalInformation.Value;
	Lazy<IReadOnlyCollection<IAdditionalInformation>> lazyAdditionalInformation;

	IReadOnlyCollection<ITransportDocument> IExportCusEntryInstructionCustomsMessageWrapper.TransportDocuments => lazyTransportDocuments.Value;
	Lazy<IReadOnlyCollection<ITransportDocument>> lazyTransportDocuments;

	IReadOnlyCollection<IAdditionalReference> IExportCusEntryInstructionCustomsMessageWrapper.AdditionalReferences => lazyAdditionalReferences.Value;
	Lazy<IReadOnlyCollection<IAdditionalReference>> lazyAdditionalReferences;

	IReadOnlyCollection<IAuthorization> ICusEntryInstructionCustomsMessageWrapper.Authorizations => lazyAuthorizations.Value;
	Lazy<IReadOnlyCollection<IAuthorization>> lazyAuthorizations;

	#endregion

	#region Implementation

	void InitializeLazy()
	{
		lazyAdditionalInformation = new Lazy<IReadOnlyCollection<IAdditionalInformation>>(GetAdditionalInformationItems);
		lazyAdditionalReferences = new Lazy<IReadOnlyCollection<IAdditionalReference>>(GetAdditionalReferences);
		lazyTransportDocuments = new Lazy<IReadOnlyCollection<ITransportDocument>>(GetTransportDocuments);
		lazyAuthorizations = new Lazy<IReadOnlyCollection<IAuthorization>>(GetAuthorizations);
	}

	IReadOnlyCollection<ITransportDocument> GetTransportDocuments()
	{
		Func<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo, ITransportDocument> mappingFunc = a => new TransportDocumentWrapper(a);
		if (!Declaration.IsTransitionPeriodAES30)
		{
			var entryInstruction = EntryInstruction;
			var instructionTransportDocuments = entryInstruction.AdditionalInfos.Where(a => a.IsATransportDocument).Select(mappingFunc);
			var invoiceLinesAndHeadersTransportDocuments = SharedWrapperDataProvider.GetAdditionalInfosFromInvoiceLinesAndHeaders(entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>(), AdditionalInfoSubTypeList.Codes.TransportDocument, mappingFunc);

			return instructionTransportDocuments.Union(invoiceLinesAndHeadersTransportDocuments)
			.GroupBy(x => new { x.DocumentType, x.ReferenceNumber })
			.Select(x => x.First())
			.OrderBy(x => x.DocumentType)
			.ThenBy(x => x.ReferenceNumber)
			.ToArray();
		}

		return GetAdditionalInfoItemsBySubType(AdditionalInfoSubTypeList.Codes.TransportDocument, mappingFunc);
	}

	IReadOnlyCollection<IAdditionalReference> GetAdditionalReferences()
		=> GetAdditionalInfoItemsBySubType(AdditionalInfoSubTypeList.Codes.AdditionalReference, a => new AdditionalReferenceWrapper(a));

	IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformationItems()
		=> GetAdditionalInfoItemsBySubType(AdditionalInfoSubTypeList.Codes.AdditionalInformation, a => new AdditionalInformationWrapper(a));

	IReadOnlyCollection<T> GetAdditionalInfoItemsBySubType<T>(string additionalInfoSubType, Func<AdditionalInfo, T> getWrapperFunc)
		=> EntryInstruction.AdditionalInfos
			.Where(a => a.CSI_SubType == additionalInfoSubType)
			.Cast<AdditionalInfo>()
			.Select(getWrapperFunc)
			.ToArray();

	IReadOnlyCollection<IAuthorization> GetAuthorizations()
	{
		return EntryInstruction.CusAuthorizationUsages
			.Select(x => new CustomsCodeAuthorizationWrapper(x))
			.ToArray();
	}

	#endregion
}
