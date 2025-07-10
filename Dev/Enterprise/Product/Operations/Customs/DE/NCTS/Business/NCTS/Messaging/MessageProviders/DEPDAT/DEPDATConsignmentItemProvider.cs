using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class DEPDATConsignmentItemProvider : IDEPDATConsignmentItem
	{
		public static DEPDATConsignmentItemProvider NewOrNull(NctsDepartureCargoDesc cargoDesc, IDEPDATHeader headerProvider, IDEPDATHouseConsignment houseConsignmentProvider)
			=> cargoDesc != null && headerProvider != null && houseConsignmentProvider != null ? new DEPDATConsignmentItemProvider(cargoDesc, headerProvider, houseConsignmentProvider) : null;

		DEPDATConsignmentItemProvider(NctsDepartureCargoDesc cargoDesc, IDEPDATHeader headerProvider, IDEPDATHouseConsignment houseConsignmentProvider)
		{
			this.cargoDesc = cargoDesc;
			this.headerProvider = headerProvider;
			this.houseConsignmentProvider = houseConsignmentProvider;
		}

		readonly NctsDepartureCargoDesc cargoDesc;
		readonly IDEPDATHeader headerProvider;
		readonly IDEPDATHouseConsignment houseConsignmentProvider;

		public int GoodsItemNumber => cargoDesc.BY_LineNo;

		public int DeclarationGoodsItemNumber => cargoDesc.BY_DeclarationGoodsItemNumber;

		public string DeclarationType => headerProvider.DeclarationType == NctsDeclarationTypeList.Codes.T ? (string)cargoDesc.BY_Type : null;

		public string CountryOfDispatch => CachedValueHelper.GetValue(ref countryOfDispatch,
			() => string.IsNullOrWhiteSpace(headerProvider.CountryOfDispatch) && string.IsNullOrWhiteSpace(houseConsignmentProvider.CountryOfDispatch)
				? cargoDesc.EffectiveCountryOfDispatch
				: null);
		CachedValue<string> countryOfDispatch;

		public string CountryOfDestination => CachedValueHelper.GetValue(ref countryOfDestination,
			() => string.IsNullOrWhiteSpace(headerProvider.CountryOfDestination) && string.IsNullOrWhiteSpace(houseConsignmentProvider.CountryOfDestination)
				? cargoDesc.EffectiveCountryOfDestination
				: null);
		CachedValue<string> countryOfDestination;

		public string ReferenceNumberUCR => CachedValueHelper.GetValue(ref referenceNumberUCR,
			() => string.IsNullOrWhiteSpace(headerProvider.ReferenceNumberUCR) && string.IsNullOrWhiteSpace(houseConsignmentProvider.ReferenceNumberUCR)
				? cargoDesc.EffectiveReferenceNumberUCR
				: null);
		CachedValue<string> referenceNumberUCR;

		public INCTSPartyIDAddressContact Consignee => CachedValueHelper.GetValue(ref consignee,
			() => headerProvider.Consignee == null && houseConsignmentProvider.Consignee == null && headerProvider.ShouldPopulateConsignee
				? NCTSPartyIDAddressContactProvider.NewOrNull(cargoDesc.EffectiveConsignee, fallback: true)
				: null);
		CachedValue<INCTSPartyIDAddressContact> consignee;

		public string DescriptionOfGoods => cargoDesc.BY_Description;

		public string CusCode => ((string)cargoDesc.BY_CusC4Number).ValueOrNullIfEmpty();

		public string HarmonizedSystemSubheadingCode => ((string)cargoDesc.BY_HarmonisedTariff).SubstringOrNull(0, 6);

		public string CombinedNomenclatureCode => ((string)cargoDesc.BY_HarmonisedTariff).SubstringOrNull(6, 2);

		public IReadOnlyCollection<string> DangerousGoods => dangerousGoods ??
			(dangerousGoods = cargoDesc.UNDGs
				.Select(x => x.Substance?.DG_UNNO.ToString())
				.Where(y => !y.IsEmpty())
				.ToArray());
		IReadOnlyCollection<string> dangerousGoods;

		public decimal GrossMass => new ZWeight(cargoDesc.BY_GrossWeight, cargoDesc.BY_GrossWeightUnit).InKilogramsSafe.Round(3).Normalize();

		public decimal NetMass
		{
			get
			{
				var result = new ZWeight(cargoDesc.BY_NetWeight, cargoDesc.BY_NetWeightUnit).InKilogramsSafe.Round(6).Normalize();
				if (cargoDesc.IsInPhase5TransitionPeriod)
				{
					result = result.Round(3).Normalize();
				}
				return result;
			}
		}

		public string MethodOfPayment => CachedValueHelper.GetValue(ref methodOfPayment,
			GetMethodOfPaymentWhenNotSpecifiedOnHeaderOrConsignment);

		CachedValue<string> methodOfPayment;

		string GetMethodOfPaymentWhenNotSpecifiedOnHeaderOrConsignment()
		{
			return headerProvider.MethodOfPayment is null &&
				   houseConsignmentProvider.MethodOfPayment is null
				? cargoDesc.EffectiveMethodOfPayment
				: null;
		}

		public string SummaryDeclarationIdentificationType => CachedValueHelper.GetValue(ref summaryDeclarationIdentificationType,
			() =>
			{
				string result = null;
				var previousProcedures = cargoDesc.PreviousProcedures.Cast<NctsPreviousDocument>().Where(x => x.IsProcedureN337).ToList();
				if (previousProcedures.Any())
				{
					if (previousProcedures.Any(x => x.CSI_SubType.ToString().In(PreviousDocSubTypeList.Codes.AWB, PreviousDocSubTypeList.Codes.ULD)))
					{
						result = PreviousDocSubTypeList.Codes.AWB;
					}
					else
					{
						result = PreviousDocSubTypeList.Codes.REG;
					}
				}

				return result;
			});
		CachedValue<string> summaryDeclarationIdentificationType;

		public string LRN => Has9DEZPreviousProcedure
				? cargoDesc.PreviousProcedureMaster.CSI_ReferenceNumber2.ValueOrNullIfEmpty()
				: null;

		public string CustomsWarehousingAuthorisationType => Has9DEZPreviousProcedure ?
			EUUniversalLookupsHelper.GetUCCAuthorizationCodeCustomsValue(cargoDesc.Factory, CustomsWarehousingAuthorisationNumber.SubstringOrNull(2, 3)).ValueOrNullIfEmpty()
				: null;

		public string CustomsWarehousingAuthorisationNumber => Has9DEZPreviousProcedure
				? cargoDesc.PreviousProcedureMaster.AuthorizationNumber.ValueOrNullIfEmpty()
				: null;

		public bool InwardProcessingSimplyGrantedAuthorisation => cargoDesc.PreviousProcedureMaster.SimplifiedGrantAuthorizationFlag;

		public string InwardProcessingAuthorisationType => DE.Business.UniversalReferenceConstants.RefCusCodeList.Codes.Code_C601;

		public string InwardProcessingAuthorisationNumber => cargoDesc.PreviousProcedureMaster.AuthorizationNumber;

		public string InwardProcessingCustomsOffice => cargoDesc.PreviousProcedureMaster.CSI_CustomsOffice;

		bool Has9DEZPreviousProcedure => cargoDesc.PreviousProcedureMaster.CSI_Procedure == NctsPreviousProcedureList.Codes._9DEZ;

		#region Collections

		public IReadOnlyCollection<INCTSActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors = cargoDesc.CusSupplyChainActorReferences.Select(x => NCTSActorProvider.NewOrNull(x)).ToArray());
		IReadOnlyCollection<INCTSActor> additionalSupplyChainActors;

		public IReadOnlyCollection<IDEPDATPackage> Packaging =>
			packaging ?? (packaging =
				cargoDesc.Packages
					.Cast<NctsPackage>()
					.Select<NctsPackage, IDEPDATPackage>(DEPDATPackageProvider.NewOrNull)
					.ToArray());

		IReadOnlyCollection<IDEPDATPackage> packaging;

		public IReadOnlyCollection<INCTSDocument> PreviousDocuments =>
			previousDocuments ?? (previousDocuments =
				cargoDesc.PreviousDocuments
					.Cast<NctsPreviousDocument>()
					.Select(NCTSDocumentProvider.NewOrNull)
					.ToArray());

		IReadOnlyCollection<INCTSDocument> previousDocuments;

		public IReadOnlyCollection<INCTSDocument> SupportingDocuments =>
			supportingDocuments ?? (supportingDocuments =
				cargoDesc.SupportingDocuments
					.Select(NCTSDocumentProvider.NewOrNull)
					.ToArray());

		IReadOnlyCollection<INCTSDocument> supportingDocuments;

		public IReadOnlyCollection<INCTSDocument> AdditionalReferences =>
			additionalReferences ?? (additionalReferences =
				cargoDesc.AdditionalInfos
					.Where(e => e.CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference)
					.Select(NCTSDocumentProvider.NewOrNull)
					.ToArray());

		IReadOnlyCollection<INCTSDocument> additionalReferences;

		public IReadOnlyCollection<INCTSAdditionalInformation> AdditionalInformation =>
			additionalInformation ?? (additionalInformation =
				cargoDesc.AdditionalInfos
					.Where(e => e.CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation)
					.Select(NCTSAdditionalInformationProvider.NewOrNull)
					.ToArray());

		IReadOnlyCollection<INCTSAdditionalInformation> additionalInformation;

		public IReadOnlyCollection<IDEPDATSummaryDeclarationReference> SummaryDeclarationReferences
		{
			get
			{
				if (summaryDeclarationReferences == null)
				{
					summaryDeclarationReferences = cargoDesc.PreviousProcedures.Cast<NctsPreviousDocument>().Where(x => x.IsProcedureN337).Select(DEPDATSummaryDeclarationReferenceProvider.NewOrNull).ToArray();
				}

				return summaryDeclarationReferences;
			}
		}
		IReadOnlyCollection<IDEPDATSummaryDeclarationReference> summaryDeclarationReferences;

		public IReadOnlyCollection<IDEPDATCustomsWarehousingReference> CustomsWarehousingReferences =>
			CachedValueHelper.GetValue(ref customsWarehousingReferences, () =>
				cargoDesc.PreviousProcedures
					.Where(x => x.IsProcedure9DEZ)
					.Select(DEPDATCustomsWarehousingReferenceProvider.NewOrNull)
					.ToArray());
		CachedValue<IReadOnlyCollection<IDEPDATCustomsWarehousingReference>> customsWarehousingReferences;

		public IReadOnlyCollection<IDEPDATInwardProcessingReference> InwardProcessingReferences
		{
			get
			{
				if (inwardProcessingReferences == null)
				{
					inwardProcessingReferences = cargoDesc.PreviousProcedures
							.Where(x => x.IsProcedure9DEY)
							.Select(DEPDATInwardProcessingReferenceProvider.NewOrNull).ToArray();
				}

				return inwardProcessingReferences;
			}
		}
		IReadOnlyCollection<IDEPDATInwardProcessingReference> inwardProcessingReferences;

		#endregion
	}
}
