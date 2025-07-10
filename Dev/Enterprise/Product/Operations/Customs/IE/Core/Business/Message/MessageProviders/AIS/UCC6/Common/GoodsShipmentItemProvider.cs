using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using Interfaces = CargoWise.Customs.IE.MessageContracts.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	class GoodsShipmentItemProvider : IGoodsShipmentItem
	{
		readonly EntryLineWrapper entryLineWrapper;
		readonly JobComInvoiceLine randomInvoiceLine;
		readonly JobComInvoiceHeader randomInvoiceHeader;
		readonly CusEntryLine entryLine;
		readonly JobDeclaration declaration;
		readonly CusEntryInstruction instruction;

		public GoodsShipmentItemProvider(CusEntryLine entryLine, EntryHeaderWrapper entryHeaderWrapper)
		{
			this.entryLine = entryLine;
			entryLineWrapper = new EntryLineWrapper(entryLine, entryHeaderWrapper);
			randomInvoiceLine = entryLineWrapper.RandomInvoiceLine;
			randomInvoiceHeader = entryLineWrapper.RandomInvoiceHeader;
			declaration = entryHeaderWrapper.Declaration;
			instruction = entryHeaderWrapper.Instruction;
		}

		public string DeclarationGoodsItemNumber => entryLine.CL_LineNumber.ToString();

		public decimal StatisticalValue => entryLine.CL_StatisticalValue;

		public string NatureOfTransaction => randomInvoiceHeader.JZ_ValuationCode;

		public string ReferenceNumberUCR => declaration.JE_UCR;

		public DateTime DateOfAcceptance
		{
			get
			{
				return DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(randomInvoiceLine.JI_DateForDutyOverride.IsEmpty && instruction.CEI_DateForDuty.IsValid ? instruction.CEI_DateForDuty : randomInvoiceLine.JI_DateForDutyOverride.IsValid ? randomInvoiceLine.JI_DateForDutyOverride : ZDateTime.Empty, true);
			}
		}

		public IReadOnlyCollection<IAuthorisation> Authorisations => authorisations ?? (authorisations = randomInvoiceLine.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Select(x => new InvoiceLineAuthorizationProvider(x)).ToArray());
		IReadOnlyCollection<IAuthorisation> authorisations;

		public IProcedure Procedure => CachedValueHelper.GetValue(ref procedure, () => new ProcedureProvider(randomInvoiceLine));
		CachedValue<IProcedure> procedure;

		public IReadOnlyCollection<Interfaces.IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors =
		randomInvoiceLine.CusSupplyChainActorReferences.Cast<EU.Business.Declaration.CusSupplyChainActorReference>()
		.Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray());
		IReadOnlyCollection<Interfaces.IAdditionalSupplyChainActor> additionalSupplyChainActors;

		public IParty Buyer => CachedValueHelper.GetValue(ref buyerCached, () => randomInvoiceHeader.BuyerAddress is OrgAddress buyerAddress ? new PartyProvider(buyerAddress) : null);
		CachedValue<IParty> buyerCached;

		public IParty Seller => null;

		public IParty Exporter => CachedValueHelper.GetValue(ref exporterCached, () =>
		{
			if (!entryLineWrapper.EntryHeader.HasInvoiceExporterAddress)
			{
				return null;
			}

			if (entryLineWrapper.EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.InvoiceHeader?.ExporterAddress != null) is JobComInvoiceLine invoiceLine)
			{
				return new PartyProvider(invoiceLine.InvoiceHeader.ExporterAddress);
			}

			if (entryLineWrapper.Declaration.SupplierDocumentaryAddress?.Address is OrgAddress address)
			{
				return new PartyProvider(address);
			}

			return null;
		});
		CachedValue<IParty> exporterCached;

		public IOrigin Origin => CachedValueHelper.GetValue(ref originCached, () => OriginProvider.New(randomInvoiceLine.JI_CountryOfOrigin, randomInvoiceLine.ZG_CountryOfSupply));
		CachedValue<IOrigin> originCached;

		public string CountryOfDispatch => randomInvoiceLine.ZG_CountryOfDispatch;

		public string MemberStateTerritory => null;

		public IDestination Destination => CachedValueHelper.GetValue(ref destinationCached, () => DestinationProvider.New(randomInvoiceLine.ZG_CountryOfDestination, randomInvoiceLine.ZG_RegionOfDestination));
		CachedValue<IDestination> destinationCached;

		public IMCommodity Commodity => CachedValueHelper.GetValue(ref commodity, () => new MCommodityType04Provider(entryLineWrapper));
		CachedValue<IMCommodity> commodity;

		public IReadOnlyCollection<Interfaces.IPackaging> Packages => packages ?? (packages = randomInvoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().Select(x => new PackagingProvider(x.Package.CW_PackType, BulkPackageTypeList.ContainsCode(x.Package.CW_PackType) ? ZInt.Zero : x.Package.CW_PackQty, x.Package.CW_MarksAndNos)).ToArray());
		IReadOnlyCollection<Interfaces.IPackaging> packages;

		CodeDescriptionPairList BulkPackageTypeList => Universal.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(randomInvoiceLine.Factory,
																															Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
																															Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
																															UNPackTypeStartDate,
																															false,
																															Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk);

		public IReadOnlyCollection<IPreviousDocumentGoodsShipmentItem> PreviousDocuments => previousDocumentsCached ?? (previousDocumentsCached =
			entryLineWrapper.EntryLine.PreviousDocuments.Select(x => new PreviousDocumentGoodsShipmentItemProvider(x)).ToArray<IPreviousDocumentGoodsShipmentItem>());
		IReadOnlyCollection<IPreviousDocumentGoodsShipmentItem> previousDocumentsCached;

		public IReadOnlyCollection<ISupportingDocumentGoodsShipmentItem> SupportingDocuments => supportingDocumentsCached ?? (supportingDocumentsCached = EU.Business.Extensions.GetAggregatedData(GetSupportingDocumentKeys(), entryLineWrapper.EntryLine.SupportingDocuments).Select(x => new SupportingDocumentGoodsShipmentItemProvider(x)).ToArray<ISupportingDocumentGoodsShipmentItem>());
		IReadOnlyCollection<ISupportingDocumentGoodsShipmentItem> supportingDocumentsCached;

		string[] GetSupportingDocumentKeys() => new[]
		{
			AdditionalInfo.Schema.CSI_Code,
			AdditionalInfo.Schema.CSI_ReferenceNumber,
			AdditionalInfo.Schema.CSI_ItemNumber,
			AdditionalInfo.Schema.CSI_AdditionalDescription,
			AdditionalInfo.Schema.CSI_DateOfExpiry,
			AdditionalInfo.Schema.CSI_UnitOfQuantity,
			AdditionalInfo.Schema.CSI_Quantity,
			AdditionalInfo.Schema.CSI_RX_NKCurrency,
			AdditionalInfo.Schema.CSI_Value,
		};

		public IReadOnlyCollection<CargoWise.Customs.IE.MessageContracts.Interfaces.IDocument> TransportDocuments => transportDocumentsCached ?? (transportDocumentsCached = entryLineWrapper.EntryLine.GetTransportDocuments<DocumentProvider>());
		IReadOnlyCollection<CargoWise.Customs.IE.MessageContracts.Interfaces.IDocument> transportDocumentsCached;

		public IReadOnlyCollection<ICcQualifierDocument> AdditionalReferences => additionalReferencesCached ?? (additionalReferencesCached = entryLineWrapper.EntryLine.GetAdditionalReferences<CcQualifierDocumentProvider>());
		IReadOnlyCollection<ICcQualifierDocument> additionalReferencesCached;

		public IReadOnlyCollection<ICcQualifierAdditionalInformation> AdditionalInformations => additionalInformationsCached ?? (additionalInformationsCached = entryLineWrapper.EntryLine.GetAdditionalInformations<CcQualifierAdditionalInformationProvider>());
		IReadOnlyCollection<ICcQualifierAdditionalInformation> additionalInformationsCached;

		public ICustomsValuation CustomsValuation => customsValuation ?? (customsValuation = new CustomsValuationProvider(entryLineWrapper));
		ICustomsValuation customsValuation;

		public string ValuationAdjustment => valuationAdjustment ??= AISMessageProviderHelper.GetValuationIndicator(randomInvoiceHeader, randomInvoiceLine);
		string valuationAdjustment;

		public IReadOnlyCollection<IAdditionalFiscalReference> AdditionalFiscalReferences => additionalFiscalReferences ?? (additionalFiscalReferences = randomInvoiceLine.FiscalReferences.Select((x, index) => new AdditionalFiscalReferenceProvider(index + 1, x.CFR_Code, MessageProviderHelper.GetVatIdentificationNumber(randomInvoiceLine.Factory, x.OwnerOrgPK))).ToArray());
		IReadOnlyCollection<IAdditionalFiscalReference> additionalFiscalReferences;

		public ITransportCosts TransportAndInsuranceCostsToTheDestination => null;

		public IReadOnlyCollection<string> ContainerIds => containerIds ?? (containerIds = randomInvoiceLine.ContainersForInvoiceLinesForBindingOnly.Select(x => (string)x.ContainerNumber).ToArray());
		IReadOnlyCollection<string> containerIds;
	}
}
