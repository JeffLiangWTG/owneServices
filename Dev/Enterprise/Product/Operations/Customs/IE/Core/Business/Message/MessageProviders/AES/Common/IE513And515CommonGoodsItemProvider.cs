using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES
{
	class IE513And515CommonGoodsItemProvider : IIE513And515CommonGoodsItem
	{
		public IE513And515CommonGoodsItemProvider(CusEntryLine entryLine, EntryHeaderWrapper entryHeaderWrapper, bool isSubStyle_B_C_E_F)
			: this(entryLine, entryHeaderWrapper, isSubStyle_B_C_E_F, true)
		{
		}

		public IE513And515CommonGoodsItemProvider(CusEntryLine entryLine, EntryHeaderWrapper entryHeaderWrapper, bool isSubStyle_B_C_E_F, bool shouldIncludeHeaderPreviousDocumentDuringTransitionPeriod)
		{
			entryLineWrapper = new EntryLineWrapper(entryLine, entryHeaderWrapper);
			this.entryLine = entryLineWrapper.EntryLine;
			invoiceLine = entryLineWrapper.RandomInvoiceLine;
			invoiceHeader = entryLineWrapper.RandomInvoiceHeader;
			entryHeader = entryLineWrapper.EntryHeader;
			declaration = entryLineWrapper.Declaration;
			instruction = entryLineWrapper.Instruction;
			this.isSubStyle_B_C_E_F = isSubStyle_B_C_E_F;
			this.shouldIncludeHeaderPreviousDocumentDuringTransitionPeriod = shouldIncludeHeaderPreviousDocumentDuringTransitionPeriod;
		}
		internal readonly EntryLineWrapper entryLineWrapper;
		internal readonly CusEntryLine entryLine;
		internal readonly JobComInvoiceLine invoiceLine;
		internal readonly JobComInvoiceHeader invoiceHeader;
		internal readonly CusEntryHeader entryHeader;
		internal readonly JobDeclaration declaration;
		internal readonly CusEntryInstruction instruction;
		internal readonly bool isSubStyle_B_C_E_F;
		readonly bool shouldIncludeHeaderPreviousDocumentDuringTransitionPeriod;

		static readonly ImmutableArray<string> proceduresNotRequiringStatisticsValue = ImmutableArray.Create(
			UniversalReferenceConstants.ProcedureCodes.ProcedureCode._31,
			UniversalReferenceConstants.ProcedureCodes.ProcedureCode._71,
			UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76,
			UniversalReferenceConstants.ProcedureCodes.ProcedureCode._77,
			UniversalReferenceConstants.ProcedureCodes.ProcedureCode._78);

		public short GoodsItemNumber => entryLine.CL_LineNumber;
		public decimal StatisticalValue => declaration.IsTransitionPeriodAES30 && proceduresNotRequiringStatisticsValue.Contains(Procedure.RequestedProcedure) || !entryLine.Header.EntryInstruction.StatisticalValueRequired ? 0 : entryLine.CL_StatisticalValue;
		public string CountryOfExport => invoiceLine.JI_RN_NKCountryOfExport;
		public string CountryOfDestination => invoiceLine.ZG_CountryOfDestination;
		public string NatureOfTransaction => isSubStyle_B_C_E_F ? ZString.Empty : invoiceHeader.JZ_ValuationCode;
		public string MethodOfPayment => isSubStyle_B_C_E_F ? ZString.Empty : invoiceHeader.ZG_TransportChargesMethodOfPayment;

		public IParty Consignor => CachedValueHelper.GetValue(ref consignorCached, () =>
		{
			if ((invoiceLine.ExporterAddress ?? invoiceHeader.SupplierAddress) is OrgAddress consignorAddress)
			{
				return PartyProvider.New(consignorAddress, PartyProvider.FallBackStyle.Consignor);
			}
			else
			{
				return PartyProvider.New(declaration.SupplierDocumentaryAddress, PartyProvider.FallBackStyle.Consignor);
			}
		});
		CachedValue<IParty> consignorCached;

		public IParty Consignee => CachedValueHelper.GetValue(ref consigneeCached, () =>
		{
			if ((invoiceLine.ConsigneeAddress ?? invoiceHeader.BuyerAddress) is OrgAddress consigneeAddress)
			{
				return PartyProvider.New(consigneeAddress);
			}
			else
			{
				return PartyProvider.New(declaration.ImporterDocumentaryAddress);
			}
		});
		CachedValue<IParty> consigneeCached;

		public string ReferenceNumberUCR => CachedValueHelper.GetValue(ref referenceNumberUCRCached, () => declaration.IsExpressConsignmentsOfExitSummary ? string.Empty : invoiceHeader.JZ_UCR.ToString());
		CachedValue<string> referenceNumberUCRCached;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ??= entryLine.CusSupplyChainActorReferences.Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray<IAdditionalSupplyChainActor>();
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

		public IReadOnlyCollection<IPackaging> Packages => packages ??= MessageProviderHelper.GetPackingDetail(entryLine.RandomMainPackLineOrRandomLine);
		IReadOnlyCollection<IPackaging> packages;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ??= MessageProviderHelper.GetE1301AdditionalReferences(declaration, entryHeader, entryLine);
		IReadOnlyCollection<IDocument> additionalReferences;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??= MessageProviderHelper.GetE1301AdditionalInformations(declaration, entryHeader, entryLine);
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IAuthorisation> Authorisations => authorisations ??= entryLine.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Select(x => new AuthorisationProvider(x)).ToArray();
		IReadOnlyCollection<IAuthorisation> authorisations;

		public IProcedure Procedure => procedure ??= new ProcedureProvider(invoiceLine);
		IProcedure procedure;

		public IOrigin Origin => origin ??= new OriginProvider(invoiceLine);
		IOrigin origin;

		public IReadOnlyCollection<IPreviousDocumentLine> PreviousDocuments => previousDocuments ??= MessageProviderHelper.GetE1301PreviousDocumentLines(declaration, entryHeader, entryLine, shouldIncludeHeaderPreviousDocumentDuringTransitionPeriod);
		IReadOnlyCollection<IPreviousDocumentLine> previousDocuments;

		public IReadOnlyCollection<ISupportingDocumentLine> SupportingDocuments => supportingDocuments ??= MessageProviderHelper.GetE1301SupportingDocumentLines(declaration, entryHeader, entryLine);
		IReadOnlyCollection<ISupportingDocumentLine> supportingDocuments;

		public IReadOnlyCollection<IDocument> TransportDocuments => transportDocument ??= GetE1301TransportDocuments();
		IReadOnlyCollection<IDocument> transportDocument;

		public ICommodityTypeWithSupplementaryUnits Commodity => commodity ??= new CommodityTypeWithSupplementaryUnitsProvider(entryLineWrapper);
		ICommodityTypeWithSupplementaryUnits commodity;

		IDocument[] GetE1301TransportDocuments()
		{
			var transportDocuments = MessageProviderHelper.FindAdditionalInfos(entryLine, AdditionalInfoSubTypeList.Codes.TransportDocument);

			if (declaration.IsTransitionPeriodAES30 && entryHeader.AdditionalInfos.Any())
			{
				transportDocuments = transportDocuments.Union(MessageProviderHelper.FindAdditionalInfos(entryHeader, AdditionalInfoSubTypeList.Codes.TransportDocument));
			}

			transportDocuments = EU.Business.Extensions.GetAggregatedData(MessageProviderHelper.GetTransportDocumentKeys(), transportDocuments);

			return transportDocuments.Select(x => new AdditionalReferenceProvider(x)).ToArray<IDocument>();
		}
	}
}
