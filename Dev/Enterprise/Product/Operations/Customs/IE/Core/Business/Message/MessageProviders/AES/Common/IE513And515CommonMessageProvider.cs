using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public abstract class IE513And515CommonMessageProvider : EntryHeaderMessageProvider
	{
		protected IE513And515CommonMessageProvider(CusEntryHeader entryHeader, bool allowPreviousDocumentsOnHeaderDuringTransitionPeriod) : base(entryHeader)
		{
			isSubStyle_B_C_E_F = false;
			isSubStyle_XOrY = false;
			switch (instruction.CEI_SubStyle.ToUpperInvariant())
			{
				case EntrySubStyleList.Codes.IncompleteDeclaration:
				case EntrySubStyleList.Codes.SimplifiedDeclaration:
				case EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB:
				case EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC:
					isSubStyle_B_C_E_F = true;
					break;
				case EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE:
				case EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF:
					isSubStyle_XOrY = true;
					break;
			}
			this.allowPreviousDocumentsOnHeaderDuringTransitionPeriod = allowPreviousDocumentsOnHeaderDuringTransitionPeriod && isSubStyle_XOrY;
		}
		protected readonly bool isSubStyle_B_C_E_F;
		protected readonly bool isSubStyle_XOrY;
		protected readonly bool allowPreviousDocumentsOnHeaderDuringTransitionPeriod;

		public string PresentationOffice => declaration.PresentationCustomsOffice;
		public string ExportOffice => declaration.JE_CustomsOffice;
		public string ExitOffice => declaration.OfficeOfExitCustomsOffice;
		public string SupervisingOffice => declaration.SupervisingCustomsOffice;

		public IParty Exporter => CachedValueHelper.GetValue(ref exporterCached, () => PartyProvider.New(declaration.SupplierDocumentaryAddress));
		CachedValue<IParty> exporterCached;

		public IParty Declarant => CachedValueHelper.GetValue(ref declarantCached, () => PartyProvider.New(declaration.Declarant));
		CachedValue<IParty> declarantCached;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => RepresentativeProvider.New(declaration.Representative, declaration.JE_DeclarantType));
		CachedValue<IRepresentative> representativeCached;

		public IReadOnlyCollection<IAuthorisation> Authorisations => authorisations ?? (authorisations = instruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Select(x => new AuthorisationProvider(x)).ToArray());
		IReadOnlyCollection<IAuthorisation> authorisations;

		public string DeferredPayment => null;

		public string DeclarationType => declaration.JE_EntryStyle;
		public string AdditionalDeclarationType => instruction.CEI_SubStyle;
		public DateTime PresentationDateTime => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(declaration.ZG_PresentationStartDate);
		public string SpecificCircumstanceIndicator => declaration.ZG_SpecificCircumstanceIndicator;
		public decimal InvoiceAmount => entryHeader.TotalPriceAmount;
		public string Security => DeclarationType == MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec
			? string.Empty
			: (
				instruction.CEI_Style == ExportDeclarationTypeList.Codes.B3 || instruction.CEI_Style == ExportDeclarationTypeList.Codes.B4
					? Constants.ExportOperationSecurity.NonSecurity
					: Constants.ExportOperationSecurity.Security
			);
		public string InvoiceCurrency => entryHeader.TotalPrice.Currency?.Code ?? string.Empty;
		public string TransactionNature => isSubStyle_B_C_E_F ? ZString.Empty : invoice.JZ_ValuationCode;
		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActor => additionalSupplyChainActor ?? (additionalSupplyChainActor = instruction.CusSupplyChainActorReferences.Cast<EU.Business.Declaration.CusSupplyChainActorReference>().Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;

		public IDeliveryTerms DeliveryTerms => isSubStyle_B_C_E_F ? null : DeliveryTermsProvider.New(invoice);
		public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ?? (previousDocuments = !allowPreviousDocumentsOnHeaderDuringTransitionPeriod && declaration.IsTransitionPeriodAES30 ? Array.Empty<IDocument>() : entryHeader.PreviousDocuments.Select(x => new PreviousDocumentProvider(x)).ToArray());
		IReadOnlyCollection<IDocument> previousDocuments;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments = declaration.IsTransitionPeriodAES30 ? Array.Empty<ISupportingDocument>() : entryHeader.SupportingDocuments.Select(x => new SupportingDocumentProvider(x)).ToArray());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ?? (additionalInformations = declaration.IsTransitionPeriodAES30 ? Array.Empty<IAdditionalInformation>() : MessageProviderHelper.FindAdditionalInfos(entryHeader, AdditionalInfoSubTypeList.Codes.AdditionalInformation).Select(x => AdditionalInformationProvider.New(x)).ToArray());
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences = declaration.IsTransitionPeriodAES30 ? Array.Empty<IDocument>() : (instruction.CEI_SubStyle == EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic || instruction.CEI_SubStyle == EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF ? MessageProviderHelper.FindAdditionalInfosExcludeCode(entryHeader, AdditionalInfoSubTypeList.Codes.AdditionalReference, Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture) : MessageProviderHelper.FindAdditionalInfos(entryHeader, AdditionalInfoSubTypeList.Codes.AdditionalReference)).Select(x => new AdditionalReferenceProvider(x)).ToArray());
		IDocument[] additionalReferences;

		public IWarehouse Warehouse => CachedValueHelper.GetValue(ref warehouseCached, () =>
		{
			var shouldUseToWarehouse = instruction.CEI_Style == ExportDeclarationTypeList.Codes.B3;

			if (shouldUseToWarehouse)
			{
				return WarehouseProvider.New(instruction.ToWarehouseType, instruction.ToWarehouseCode);
			}
			else
			{
				return WarehouseProvider.New(instruction.FromWarehouseType, instruction.FromWarehouseCode);
			}
		});
		CachedValue<IWarehouse> warehouseCached;
	}
}
