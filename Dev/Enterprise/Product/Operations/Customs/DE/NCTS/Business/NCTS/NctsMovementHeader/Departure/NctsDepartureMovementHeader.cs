using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsDepartureMovementHeader : EU.NCTS.Business.NctsDepartureMovementHeader
		, Integration.Customs.DE.IDepartureMovementHeader
	{
		public NctsDepartureMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsHeader Header => (NctsHeader)base.Header;

		public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

		#region Properties

		public override ZString BM_AdditionalText
		{
			get => base.BM_AdditionalText;
			set
			{
				base.BM_AdditionalText = value;
				GoodsItems.MarkAsNeedingValidation();
			}
		}

		public override ZString BM_BTAIndicator
		{
			get => base.BM_BTAIndicator;
			set
			{
				base.BM_BTAIndicator = value;
				if (BM_PlaceOfUnloading_ReadOnly)
				{
					BM_PlaceOfUnloading = string.Empty;
				}
				GoodsItems.MarkAsNeedingValidation();
			}
		}

		[ReadOnlyMember(nameof(BM_PlaceOfUnloading_ReadOnly))]
		public override ZString BM_PlaceOfUnloading
		{
			get => base.BM_PlaceOfUnloading;
			set => base.BM_PlaceOfUnloading = value;
		}

		protected bool BM_PlaceOfUnloading_ReadOnly => BM_BTAIndicator == SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
		#endregion

		public new NctsDepartureMovementHeaderLookups Lookups => (NctsDepartureMovementHeaderLookups)base.Lookups;

		public new INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems => (INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;

		protected override INctsCommonCargoDescCollection<NctsCommonCargoDesc> CreateGoodsItems() => new NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(this);

		protected override CusInBondMoveHeaderLookups GetNewPhase5Lookups() => new NctsDepartureMovementHeaderLookups(this);

		protected override CusInBondMoveHeaderLookups GetNewPhase4Lookups() => new NctsDepartureMovementHeaderLookups(this);

		protected override NctsDepartureMovementHeaderPhase5Validation GetNewPhase5Validation() => new NctsDepartureMovementHeaderValidation(this);

		public new INctsGuaranteeCollection<Guarantee> Guarantees => (INctsGuaranteeCollection<Guarantee>)base.Guarantees;

		protected override INctsGuaranteeCollection<NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<Guarantee>(this);

		public new INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;

		protected override INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetSupportingDocuments() => new NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

		protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);

		protected override Type CusInBondCargoDescTypeCore => typeof(NctsDepartureCargoDesc);

		protected override void ClearGoodsLocationWhenIsNotSimplifiedNctsProcedure()
		{
			GoodsLocation.Address.Delete();
			GoodsLocation.Delete();
		}

		public new IDepartureCusTransportMeansCollection<DepartureCusTransportMeans> AdditionalTransportAtBorderList => (DepartureCusTransportMeansCollection<DepartureCusTransportMeans>)base.AdditionalTransportAtBorderList;

		protected override IDepartureCusTransportMeansCollection<EU.NCTS.Business.DepartureCusTransportMeans> GetNewCusTransportMeansCollection() => new DepartureCusTransportMeansCollection<DepartureCusTransportMeans>(this);

		protected override ZString[] TransportAtDepartureTypesRequiringUpperCaseIDs() => new ZString[]
		{
			NctsTransportTypeOfIdList.Codes._10,
			NctsTransportTypeOfIdList.Codes._20,
			NctsTransportTypeOfIdList.Codes._21,
			NctsTransportTypeOfIdList.Codes._30,
			NctsTransportTypeOfIdList.Codes._40,
			NctsTransportTypeOfIdList.Codes._41,
			NctsTransportTypeOfIdList.Codes._80
		};

		protected override void CustomsOfficesForDeparture_ListChanged(object sender, ListChangedEventArgs e)
		{
			base.CustomsOfficesForDeparture_ListChanged(sender, e);

			if (IsSimplifiedNctsProcedure)
			{
				Header.UpdateGoodsLocationAdditionalIdentifier(GoodsLocation, GoodsLocationDescriptionInfo);
			}
		}

		protected override EU.NCTS.Business.GuaranteeTransactionCoordinator GetNewGuaranteeTransactionCoordinator() => new GuaranteeTransactionCoordinator(this);

		[CodeAlive("WI00734488 - Following conversation with Daniel (DJC) regarding DE requirments, overriding to maintain current functionality but leaving the code and TestDefaultDepartureLocationCodeFromCusAuthorisationIfBlank() for refactor or removal if not needed")]
		protected override void DefaultDepartureLocationCodeFromCusAuthorisationIfBlank(CusAuthorisationHeader authorizationToUse)
		{
			//if (Header.IsPhase5 && Header.Configuration.IsDefaultingOfPhase5AuthorisedLocationFromACRAuthorisationEnabled)
			//{
			//	if ((GoodsLocation?.CGL_AdditionalIdentifier ?? ZString.Empty).IsEmpty)
			//	{
			//		CusAuthorisationRule locationCode = null;
			//		var locationCodes = authorizationToUse.CusAuthorisationRules?.Where(r => r.CPR_RuleCode == EU.Business.CusAuthorisationRuleTypeList.Codes.Location && !r.CPR_ValueFrom.IsEmpty);
			//		if (locationCodes?.Count() == 1)
			//		{
			//			locationCode = locationCodes.Single();
			//		}
			//		else if (locationCodes?.Count() > 1)
			//		{
			//			var locationRules = locationCodes.Where(lc => lc.LinkedCusAuthorisationRules.Any(r => r.CPR_RuleCode == Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice && r.CPR_ValueFrom == DepartureCustomsOfficeCode));
			//			if (locationRules?.Count() == 1)
			//			{
			//				locationCode = locationRules.Single();
			//			}
			//		}

			//		if (locationCode != null)
			//		{
			//			GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			//			GoodsLocation.CGL_AdditionalIdentifier = locationCode.CPR_ValueFrom.SubstringSafe(0, CusGoodsLocationSchema.CGL_AdditionalIdentifier.MaxLength);
			//		}
			//	}
			//}
		}

		void Integration.Customs.DE.IDepartureMovementHeader.DeleteGuaranteeTransactions()
		{
			GuaranteeTransactionCoordinator.DeleteTransactions();
		}

		#region FetchHints

		public override void AddCusGoodsLocationAdditionalIdentifierDescriptionFetchHintsForViewCore()
		{
			Factory.AddFetchHint(CusAuthorizationUsageSchema.AGC_ParentID, PK);

			var query = new ZQuery(CusCodeDataSchema.CY_ParentID, PK)
					.AddToFilter(CusCodeDataSchema.CY_Type, EU.Business.CusCodeDataTypeList.Codes.OfficeCode);

			Factory.AddFetchHint(CusCodeDataSchema.Instance, query);
		}

		#endregion
	}
}
