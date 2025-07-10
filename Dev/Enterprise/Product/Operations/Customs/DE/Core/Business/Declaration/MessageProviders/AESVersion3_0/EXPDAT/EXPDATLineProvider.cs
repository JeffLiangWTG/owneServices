using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.ZArchitecture;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public class EXPDATLineProvider : AESLineProvider, IEXPDATLine
	{
		public EXPDATLineProvider(CusEntryLine entryLine, IEXPDATHeader headerProvider) : base(entryLine)
		{
			this.headerProvider = Argument.NotNull(headerProvider, nameof(headerProvider));
		}
		readonly IEXPDATHeader headerProvider;

		public string TransactionType => headerProvider.TransactionType.IsNullOrEmpty() ? RandomInvoiceHeader.JZ_ValuationCode.ToString() : string.Empty;

		public string CountryOfExport => headerProvider.ExportCountry.IsEmpty ? RandomInvoiceLine.JI_RN_NKCountryOfExport.ToString() : string.Empty;

		public ZString CommercialReferenceNumber => headerProvider.CommercialReferenceNumber.IsNullOrEmpty() ? RandomInvoiceHeader.JZ_UCR : ZString.Empty;

		public IReadOnlyCollection<IReference> Authorisations => authorisations ?? (authorisations = EntryLine.InvoiceLines.Cast<JobComInvoiceLine>()
				.SelectMany(l => l.AdditionalInfos.Cast<AdditionalInfo>())
				.Where(x => x.CSI_SubType == AdditionalDocTypeList.Codes.Authorization)
				.Select(AdditionalInfoProvider.NewOrNull).ToArray());
		IReadOnlyCollection<IReference> authorisations;

		public ZString RequestedProcedure => RandomInvoiceLine.JI_Procedure.Left(2);

		public ZString PreviousProcedure => RandomInvoiceLine.JI_Procedure.SubstringSafe(2, 2);

		public ZString AdditionalProcedure => RandomInvoiceLine.JI_Procedure.SubstringSafe(4);

		public IAESParty Consignor => CachedValueHelper.GetValue(ref consignorCached, () =>
		{
			PartyProvider provider = null;
			if (headerProvider.Consignor == null)
			{
				var exporterAddress = RandomInvoiceLine.ExporterAddress;
				if (exporterAddress != null)
				{
					provider = PartyProvider.NewOrNull(exporterAddress);
				}
				else
				{
					provider = PartyProvider.NewOrNull(Declaration.SupplierDocumentaryAddress.Address);
				}
			}
			return provider;
		});
		CachedValue<IAESParty> consignorCached;

		public IAESParty Consignee => CachedValueHelper.GetValue(ref consigneeCached, () =>
		{
			PartyProvider provider = null;
			if (headerProvider.Consignee == null)
			{
				var consigneeAddress = RandomInvoiceLine.ConsigneeAddress;
				if (consigneeAddress != null)
				{
					provider = PartyProvider.NewOrNull(consigneeAddress);
				}
				else
				{
					provider = PartyProvider.NewOrNull(RandomInvoiceHeader?.ConsigneeAddress);
				}
			}
			return provider;
		});
		CachedValue<IAESParty> consigneeCached;

		public IReadOnlyCollection<ISupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors =
			IEnumerableExtensions.DistinctBy(EntryLine.InvoiceLines.Cast<JobComInvoiceLine>()
				.SelectMany(x => x.CusSupplyChainActorReferences.Cast<EU.Business.Declaration.CusSupplyChainActorReference>())
				, y => new { y.CFR_Code, y.CFR_Reference })
				.Select(SupplyChainActorProvider.NewOrNull).ToArray());
		IReadOnlyCollection<ISupplyChainActor> additionalSupplyChainActors;

		public string CountryOfOrigin => RandomInvoiceLine.JI_CountryOfOrigin;

		public ZString OriginFederalState => RandomInvoiceLine.JI_StateOrRegionOfOrigin;

		public ZString GoodsDescription => !RandomInvoiceLine.JI_NDescription.IsEmpty ? RandomInvoiceLine.JI_NDescription : RandomInvoiceLine.JI_Description;

		public string CusCode => RandomInvoiceLine.ZG_CusNumber;

		public string HarmonizedSystemSubHeadingCode => RandomInvoiceLine.JI_Tariff.SubstringSafe(0, 6);

		public ZString CombinedNomenclatureCode => RandomInvoiceLine.JI_Tariff.SubstringSafe(6, 2);

		public ZString TaricFirstAdditionalCode => RandomInvoiceLine.JI_SupplementaryCode1;

		public ZString TaricSecondAdditionalCode => RandomInvoiceLine.JI_SupplementaryCode2;

		public IReadOnlyCollection<ZString> TaricOtherAdditionalCodes => taricOtherAdditionalCodes ?? (taricOtherAdditionalCodes = RandomInvoiceLine.AdditionalSupplementaryCodes.GetAllCodes().ToArray());
		IReadOnlyCollection<ZString> taricOtherAdditionalCodes;

		public bool TaricOtherAdditionalCodesSpecified => !Declaration.IsTransitionPeriodAES30;

		public IReadOnlyCollection<string> DangerousGoodsCodes => dangerousGoodsCodes ?? (dangerousGoodsCodes = RandomInvoiceLine.UNDGs.Select(obj => obj.SubstanceCode.ToString()).ToArray());
		IReadOnlyCollection<string> dangerousGoodsCodes;

		public ZDecimal GrossMass => CachedValueHelper.GetValue(ref grossMass, () =>
			{
				var totalInvoiceLinesWeight = ZWeight.Empty;
				foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
				{
					totalInvoiceLinesWeight += new ZWeight(invoiceLine.JI_Weight, invoiceLine.JI_WeightUQ);
				}
				return totalInvoiceLinesWeight.InKilogramsSafe.Round(3).Normalize();
			});
		CachedValue<ZDecimal> grossMass;

		public ZDecimal NetMass => CachedValueHelper.GetValue(ref netMass, () => EntryLine.EffectiveCustomsWeight.Amount.Round(3).Normalize());
		CachedValue<ZDecimal> netMass;

		public IReadOnlyCollection<IPackage> Packages => packages ?? (packages = EntryLine.PackagingDetails.OrderBy(x => x.Package.CW_MarksAndNos).Select(LinePackageProvider.NewOrNull).ToArray<IPackage>());
		IReadOnlyCollection<IPackage> packages;

		public IReadOnlyCollection<IPreviousDocument> PreviousDocuments => previousDocuments ?? (previousDocuments = IEnumerableExtensions.DistinctBy(EntryLine.InvoiceLines
			.Cast<JobComInvoiceLine>()
			.SelectMany(l => l.PreviousDocuments
			.Cast<PreviousDocument>())
			, d => new { d.CSI_Code, d.CSI_ReferenceNumber })
			.Select(PreviousDocumentProvider.NewOrNull).ToArray());
		IReadOnlyCollection<IPreviousDocument> previousDocuments;

		public IReadOnlyCollection<ISupportingDocument> Documents => documents ??= RandomInvoiceLine.SupportingDocuments
			.Cast<SupportingDocument>()
			.OrderBy(x => x.CSI_LineNo)
			.ThenBy(x => x.PK)
			.Select(SupportingDocumentProvider.NewOrNull).ToArray();
		IReadOnlyCollection<ISupportingDocument> documents;

		public IReadOnlyCollection<IReference> AdditionalReferences => additionalReferences ?? (additionalReferences = EntryLine.InvoiceLines
			.Cast<JobComInvoiceLine>()
			.SelectMany(l => l.AdditionalInfos.Cast<AdditionalInfo>())
			.Where(x => x.CSI_SubType == AdditionalDocTypeList.Codes.AdditionalReference)
			.GroupBy(x => new { x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_ReferenceNumber2, x.CSI_RX_NKCurrency })
			.Select(g => AdditionalInfoProvider.NewOrNull(g.Sum(x => x.CSI_Value), g.First())).ToArray());
		IReadOnlyCollection<IReference> additionalReferences;

		public IReadOnlyCollection<IReference> AdditionalInformations => additionalInformations ?? (additionalInformations = RandomInvoiceLine.AdditionalInfos
			.Cast<AdditionalInfo>()
			.Where(x => x.CSI_SubType == AdditionalDocTypeList.Codes.AdditionalInformation)
			.Select(AdditionalInfoProvider.NewOrNull).ToArray());
		IReadOnlyCollection<IReference> additionalInformations;

		public ZString TransportChargesPaymentMethod => headerProvider.TransportChargesPaymentMethod.IsEmpty ? RandomInvoiceHeader.ZG_TransportChargesMethodOfPayment : ZString.Empty;

		public string OutwardProcessingReplacement => EntryHeader.EntryInstruction.Style1stDigitIs1() ? RandomInvoiceLine.ZG_UsualReplacement.MapBoolTo10() : string.Empty;

		public DateTime OutwardProcessingReimportDate => EntryHeader.EntryInstruction.Style1stDigitIs1() ? RandomInvoiceLine.ZG_ReimportDate.Date.SafeDate() : default;

		public ZString WarehouseLocalReferenceNumber => CachedValueHelper.GetValue(ref warehouseLocalReferenceNumber, () => IsWarehouseProcedure && IsLinePreviousProcedureMasterProcedureATZL
				? RandomInvoiceLine.PreviousProcedureMaster.CSI_ReferenceNumber2 : ZString.Empty);
		CachedValue<ZString> warehouseLocalReferenceNumber;

		public IAuthorisation CustomsWarehousingAuthorisation => CachedValueHelper.GetValue(ref customsWarehousingAuthorisationCached, GetCustomsWarehousingAuthorisation);
		CachedValue<IAuthorisation> customsWarehousingAuthorisationCached;

		IAuthorisation GetCustomsWarehousingAuthorisation()
		{
			IAuthorisation result = null;
			if (IsWarehouseProcedure && IsLinePreviousProcedureMasterProcedureATZL)
			{
				var authorizationNumber = RandomInvoiceLine.PreviousProcedureMaster.AuthorizationNumber;
				if (!authorizationNumber.IsEmpty)
				{
					var number3rd4th5thDigits = authorizationNumber.SubstringSafe(2, 3);
					if (number3rd4th5thDigits == CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP)
					{
						result = GetAuthorisation("C517", authorizationNumber);
					}
					else if (number3rd4th5thDigits == CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1)
					{
						result = GetAuthorisation("C518", authorizationNumber);
					}
					else if (number3rd4th5thDigits == CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2)
					{
						result = GetAuthorisation("C519", authorizationNumber);
					}
				}
			}
			return result;

			IAuthorisation GetAuthorisation(ZString type, ZString referenceNumber)
			{
				return AuthorisationProvider.New(type, referenceNumber);
			}
		}

		public IReadOnlyCollection<IWarehouseProcedure> WarehouseProcedures => warehouseProcedures ?? (warehouseProcedures = IsWarehouseProcedure && IsLinePreviousProcedureMasterProcedureATZL
			? EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(l => l.PreviousProcedures.Cast<PreviousDocument>()).Select(WarehouseProcedureProvider.NewOrNull).ToArray()
			: Array.Empty<IWarehouseProcedure>());
		IReadOnlyCollection<IWarehouseProcedure> warehouseProcedures;

		public bool IsWarehouseProcedure => RandomInvoiceLine.JI_Procedure.SubstringSafe(2, 2) == "71";

		public string InwardProcessingSimplyGrantedAuthorisation => IsInwardProcessingProcedure && IsLinePreviousProcedureMasterProcedureATAV ?
			RandomInvoiceLine.PreviousProcedureMaster.SimplifiedGrantAuthorizationFlag.MapBoolTo10() : string.Empty;

		public IAuthorisation InwardProcessingAuthorisation => CachedValueHelper.GetValue(ref inwardProcessingAuthorisationCached, GetInwardProcessingAuthorisation);
		CachedValue<IAuthorisation> inwardProcessingAuthorisationCached;

		IAuthorisation GetInwardProcessingAuthorisation()
		{
			IAuthorisation result = null;
			if (IsInwardProcessingProcedure && IsLinePreviousProcedureMasterProcedureATAV)
			{
				var authorizationNumber = RandomInvoiceLine.PreviousProcedureMaster.AuthorizationNumber;
				if (!authorizationNumber.IsEmpty)
				{
					result = AuthorisationProvider.New("C601", authorizationNumber);
				}
			}
			return result;
		}

		public string InwardProcessingCustomsOfficeOfSupervision => IsInwardProcessingProcedure && IsLinePreviousProcedureMasterProcedureATAV ?
			RandomInvoiceLine.PreviousProcedureMaster.CSI_CustomsOffice.ToString() : string.Empty;

		public IReadOnlyCollection<IInwardProcessingProcedure> InwardProcessingProcedures => inwardProcessingProcedures ?? (inwardProcessingProcedures = IsInwardProcessingProcedure && IsLinePreviousProcedureMasterProcedureATAV
			? EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(l => l.PreviousProcedures.Cast<PreviousDocument>()).Select(InwardProcessingProcedureProvider.NeworNull).ToArray()
			: Array.Empty<IInwardProcessingProcedure>());
		IReadOnlyCollection<IInwardProcessingProcedure> inwardProcessingProcedures;

		public bool IsInwardProcessingProcedure => RandomInvoiceLine.JI_Procedure.SubstringSafe(2, 2) == "51";

		bool IsLinePreviousProcedureMasterProcedureATZL => RandomInvoiceLine.PreviousProcedureMaster.CSI_Procedure == PreviousProcedureList.Codes._ATZL;

		bool IsLinePreviousProcedureMasterProcedureATAV => RandomInvoiceLine.PreviousProcedureMaster.CSI_Procedure == PreviousProcedureList.Codes._ATAV;

		public bool ProcedureTransferenceSpecified
		{
			get
			{
				var entryInstruction = EntryHeader.EntryInstruction;
				return !entryInstruction.Style4thDigitIs4() && !entryInstruction.SubStyle1stDigitIs1() && !entryInstruction.SubStyle1stDigitIs2() && (IsWarehouseProcedure || IsInwardProcessingProcedure) && !RandomInvoiceLine.PreviousProcedureMaster.CSI_Procedure.IsEmpty;
			}
		}

		public string CountryOfDestination => string.Empty;

		public ZString Annotation => RandomInvoiceLine.AdditionalInfoDescription;

		public IReadOnlyCollection<ZString> ContainerIdentificationNumbers
		{
			get
			{
				if (containerIdentificationNumbers == null)
				{
					if (Declaration.IsContainerised)
					{
						containerIdentificationNumbers = EntryLine.Containers.ToArray();
					}
					else
					{
						containerIdentificationNumbers = Array.Empty<ZString>();
					}
				}

				return containerIdentificationNumbers;
			}
		}
		IReadOnlyCollection<ZString> containerIdentificationNumbers;

		public IDeliveryTerms DeliveryTerms => CachedValueHelper.GetValue(ref deliveryTermsCached, () => LineDeliveryTermsProvider.NewOrNull(RandomInvoiceLine));
		CachedValue<IDeliveryTerms> deliveryTermsCached;

		public ZString WarehouseOwner => CachedValueHelper.GetValue(ref warehouseOwner, () =>
		{
			ZString result;
			if (IsWarehouseProcedure)
			{
				result = RandomInvoiceLine.PreviousProcedureMaster?.AuthorizationNumber ?? ZString.Empty;
			}
			else
			{
				result = ZString.Empty;
			}
			return result;
		});
		CachedValue<ZString> warehouseOwner;

		public ZString ProcessingOwner => CachedValueHelper.GetValue(ref processingOwner, () =>
		{
			ZString result;
			if (IsInwardProcessingProcedure)
			{
				result = RandomInvoiceLine.PreviousProcedureMaster?.AuthorizationNumber ?? ZString.Empty;
			}
			else
			{
				result = ZString.Empty;
			}
			return result;
		});
		CachedValue<ZString> processingOwner;

		public decimal TotalDutiesAndTaxesAmount => 0.0M;
		public string TaxType => string.Empty;
		public decimal PayableTaxAmount => 0.0M;
		public string TaxPaymentMethod => string.Empty;

		public decimal TaxBaseTaxRate => 0.0M;
		public string TaxBaseMeasurementUnitAndQualifier => string.Empty;
		public decimal TaxBaseQuantity => 0.0M;
		public decimal TaxBaseAmount => 0.0M;
		public decimal TaxBaseTaxAmount => 0.0M;
	}
}
