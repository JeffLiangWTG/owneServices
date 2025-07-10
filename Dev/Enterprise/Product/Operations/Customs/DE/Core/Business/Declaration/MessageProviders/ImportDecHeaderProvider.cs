using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business
{
	public abstract class ImportDecHeaderProvider : ImportHeaderProvider, IImportDecHeader
	{
		public ImportDecHeaderProvider(CusEntryHeader entryHeader) : base(entryHeader)
		{
			EntryInstruction = Argument.NotNull(EntryHeader.EntryInstruction, nameof(EntryHeader.EntryInstruction));
		}
		protected readonly CusEntryInstruction EntryInstruction;

		public string DeclarationKind => EntryInstruction.CEI_SubStyle;

		public string DeclarationType => EntryInstruction.CEI_Style;

		public bool PrematureInputFlag => ImportSubStyleList.IsPrematureDeclaration(EntryInstruction.Factory, EntryInstruction.CEI_SubStyle);

		public int GoodsItemQuantity => CachedValueHelper.GetValue(ref goodsItemQuantity, () => EntryHeader.AllEntryLines.Count);
		CachedValue<int> goodsItemQuantity;

		public string CustomsGoodsStatus => Declaration.JE_EntryStyle;

		public string ProcedureAuthorisation => CachedValueHelper.GetValue(ref procedureAuthorisation, () => EntryInstruction.InvoiceLines
			.Cast<JobComInvoiceLine>()
			.SelectMany(invoiceLine => invoiceLine.SupportingDocuments.Cast<SupportingDocument>(), (i, d) => d)
			.FirstOrDefault(x => CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation.Contains(x.CSI_Code))?.CSI_ReferenceNumber);

		CachedValue<string> procedureAuthorisation;

		public string GoodsLocation => Declaration.JE_LocationOfGoods;

		public string DepartureCountry => Declaration.JE_GoodsOrigin;

		public string CurrencyCode => Constants.CurrencyCodes.Germany;

		public string AdditionalInformation => EntryInstruction.AdditionalInformation;

		public string RepresentativeRelationshipFlag => Declaration.JE_DeclarantType.MapDeclarantTypeToMessaging();

		public string DeclarationPlace => GlbBranch.CurrentBranch.GB_City;

		public IImportParty Declarant => CachedValueHelper.GetValue(ref declarantCached, () => Declaration.Importer != DeclarantAddress?.Header ? ImportPartyProvider.NewOrNull(DeclarantAddress) : null);
		CachedValue<IImportParty> declarantCached;

		public IImportParty Representative => CachedValueHelper.GetValue(ref represetativeCached, () => Declaration.JE_DeclarantType == RepresentationTypeList.Codes._2Direct ? ImportPartyProvider.NewOrNull(Declaration.Representative) : null);
		CachedValue<IImportParty> represetativeCached;

		public IImportParty Principal => CachedValueHelper.GetValue(ref principalCached, () => Declaration.JE_DeclarantType == RepresentationTypeList.Codes._3Indirect ? ImportPartyProvider.NewOrNull(Declaration.BuyingAgentAddress) : null);
		CachedValue<IImportParty> principalCached;

		public IImportPartyContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPersonCached, () => ImportPartyContactPersonProvider.NewOrNull(GlbStaff.CurrentUser));
		CachedValue<IImportPartyContactPerson> contactPersonCached;

		public string BorderTransportMeansMode => Declaration.TransportModeTranslator.TranslateToWCOCode(Declaration.JE_TransportMode);

		public string BorderTransportMeansType => Declaration.ZG_BorderTransportMeans;

		public string BorderTransportMeansInformation => ImportBorderTransportMeansList.RequireInformation(Declaration.ZG_BorderTransportMeans) ? (string)Declaration.JE_VesselName : null;

		public string BorderTransportMeansNationality
		{
			get
			{
				string result = null;
				var transportMode = Declaration.JE_TransportMode;
				if (transportMode == TransportTypeList.Codes.Air
					|| transportMode == TransportTypeList.Codes.Sea
					|| transportMode == TransportTypeList.Codes.Road
					|| transportMode == TransportTypeList.Codes.InlandWaterwayTransport)
				{
					result = Declaration.JE_RN_NKTransportNationality;
				}
				return result;
			}
		}

		public string PreviousAdministrativeReferenceType => CachedValueHelper.GetValue(ref previousAdministrativeReferenceTypeCached, () => EntryInstruction.PreviousDocumentMaster.CSI_Procedure);
		CachedValue<string> previousAdministrativeReferenceTypeCached;

		public string PreviousAdministrativeReferenceNumber => EntryInstruction.PreviousDocumentMaster.HasPreviousDocuments ? (string)EntryInstruction.PreviousDocuments[0].CSI_ReferenceNumber : null;

		public string ForeignTradeStatisticsInlandTransportMode => Declaration.TransportModeTranslator.TranslateToWCOCode(Declaration.JE_TransportModeInland);

		public decimal ForeignTradeStatisticsTotalGrossMassMeasure => CachedValueHelper.GetValue(ref foreignTradeStatisticsTotalGrossMassMeasure, () =>
		{
			ZDecimal result = EntryInstruction.InvoiceLines.Select(l => l.InvoiceHeader).GroupBy(h => h.PK).Select(g => g.First()).Sum(h => new ZWeight(h.JZ_Weight, h.JZ_WeightUQ).InKilogramsSafe);
			if (result > 0)
			{
				result += EntryInstruction.InvoiceLines.Where(l => l.InvoiceHeader.JZ_Weight.IsEmpty).Sum(l => new ZWeight(l.JI_Weight, l.JI_WeightUQ).InKilogramsSafe);
			}

			return result.Round(1).Normalize();
		});
		CachedValue<decimal> foreignTradeStatisticsTotalGrossMassMeasure;

		public ISummaryDeclaration SummaryDeclaration => CachedValueHelper.GetValue(ref summaryDeclarationCached, () => ProvideSummaryDeclaration ? SummaryDeclarationProvider.NewOrNull(EntryInstruction.PreviousDocumentMaster) : null);
		CachedValue<ISummaryDeclaration> summaryDeclarationCached;

		public ICustomsWarehouse CustomsWarehouse => CachedValueHelper.GetValue(ref customsWarehouseCached, () => ProvideCustomsWarehouse ? CustomsWarehouseProvider.NewOrNull(EntryInstruction.PreviousDocumentMaster) : null);
		CachedValue<ICustomsWarehouse> customsWarehouseCached;

		public IInwardProcessing InwardProcessing => CachedValueHelper.GetValue(ref inwardProcessingCached, () => ProvideInwardProcessing ? InwardProcessingProvider.NewOrNull(EntryInstruction.PreviousDocumentMaster) : null);
		CachedValue<IInwardProcessing> inwardProcessingCached;

		public IImportParty Consignee => CachedValueHelper.GetValue(ref consigneeCached, () => ImportPartyProvider.NewOrNull(Declaration.ImporterDocumentaryAddress.Address));
		CachedValue<IImportParty> consigneeCached;

		public string ContainerFlag => ContainerIdentificationNumbers.Any().MapBoolToJN();

		public IReadOnlyCollection<string> ContainerIdentificationNumbers => containerIdentificationNumbers ?? (containerIdentificationNumbers = Declaration.IsContainerised
			? EntryInstruction.InvoiceLines.SelectMany(il => il.ContainersPivot.Select(c => c.ContainerNumber.ToString())).Distinct().ToArray()
			: Array.Empty<string>());
		IReadOnlyCollection<string> containerIdentificationNumbers;

		public string DeliveryTermsCode => CachedValueHelper.GetValue(ref deliveryTermsCode, () => RandomInvoiceHeader?.JZ_IncoTerm);
		CachedValue<string> deliveryTermsCode;

		public string DeliveryTermsDescription => CachedValueHelper.GetValue(ref deliveryTermsDescription, () =>
		{
			string description = null;
			if (DeliveryTermsCode == "XXX")
			{
				description = RandomInvoiceHeader?.JZ_IncoTermDescription;
			}
			return description;
		});
		CachedValue<string> deliveryTermsDescription;

		public IReadOnlyCollection<IImportDocument> Documents => documents ?? (documents =
			IEnumerableExtensions.DistinctBy(EntryInstruction.Invoices
				.SelectMany(i => i.SupportingDocuments.Cast<SupportingDocument>())
				.Where(d => !d.CSI_Code.IsEmpty)
				, d => new { d.CSI_Code, d.CSI_ReferenceNumber, d.CSI_DateOfIssue })
				.Select(d => new ImportDocumentProvider(d))
				.ToArray());
		IReadOnlyCollection<IImportDocument> documents;

		protected JobComInvoiceHeader RandomInvoiceHeader => CachedValueHelper.GetValue(ref randomInvoiceHeader, () => (JobComInvoiceHeader)EntryInstruction.InvoiceLines.FirstOrDefault()?.InvoiceHeader);
		CachedValue<JobComInvoiceHeader> randomInvoiceHeader;

		protected string GetLocalClearanceProcedureAuthorisationNumber(Func<string, bool> isFromSimplifiedDeclaration, Func<string, bool> isFromEntryOfDataInTheDeclarantsRecords)
		{
			string result = null;
			var style = EntryInstruction.CEI_Style;
			var code = ZString.Empty;
			if (isFromSimplifiedDeclaration(style))
			{
				code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			}
			else if (isFromEntryOfDataInTheDeclarantsRecords(style))
			{
				code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			}

			if (!code.IsEmpty)
			{
				result = GetCusAuthorizationUsageNumber(code);
			}
			return result;
		}

		protected string GetCusAuthorizationUsageNumber(string code) => EntryInstruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().FirstOrDefault(x => x.AGC_Code == code)?.AGC_Number.ValueOrNullIfEmpty();

		protected OrgAddress DeclarantAddress => Declaration.Declarant;

		bool ProvideSummaryDeclaration => PreviousAdministrativeReferenceType == PreviousProcedureList.Codes._ATNEU;

		bool ProvideCustomsWarehouse => PreviousAdministrativeReferenceType == PreviousProcedureList.Codes._ATZL;

		bool ProvideInwardProcessing => PreviousAdministrativeReferenceType == PreviousProcedureList.Codes._ATAV;
	}
}
