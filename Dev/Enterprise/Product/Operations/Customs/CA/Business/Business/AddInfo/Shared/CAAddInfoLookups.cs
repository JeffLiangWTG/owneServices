//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCAAddInfoLookups
//
//    This class should be used for overriding collections in AutoCAAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	using CargoWise.Types;
	using Enterprise.Customs.Common.CA;
	using Enterprise.Customs.Common.US;
	using Enterprise.Customs.Universal;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;

	public class CAAddInfoLookups : AutoCAAddInfoLookups
	{
		public CAAddInfoLookups(AutoCAAddInfo parent)
			: base(parent)
		{
		}

		public new AutoCAAddInfo Parent
		{
			get { return (AutoCAAddInfo)base.Parent; }
		}

		public ZZRefCusCodeListCombinedCollection CBSAOffices
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public CodeDescriptionPairList ReasonForExportCodes
		{
			get { return Factory.GetCachedValue<ReasonForExportList>(); }
		}

		public RefCurrencyCollection DeclaredCurrencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public CodeDescriptionPairList CanadianProvinces => LookupsHelper.CanadianProvinces(Factory);

		public CodeDescriptionPairList CFIAStatesOfOrigin => LookupsHelper.CFIAStatesOfOrigin(Factory);

		public CodeDescriptionPairList CasualImportCommodity => LookupsHelper.CasualImportCommodityList(Factory);

		public CodeDescriptionPairList ImportReasonCodes => LookupsHelper.ImportReasonCodes(Factory);

		public ValueForDutyCodes ValueForDutyCodes => LookupsHelper.ValueForDutyCodes(Factory);

		public CodeDescriptionPairList TreatmentCodes
		{
			get
			{
				return LookupsHelper.TreatmentCodes(Factory);
			}
		}

		public CACFIAEndUseCodesCollection CFIAEndUseCodes => LookupsHelper.CFIAEndUseCodes(Factory);

		public CACFIAMiscCodesCollection CFIAMiscIDCodes => LookupsHelper.CFIAMiscIDCodes(Factory);

		public ZZRefCusCodeListCombinedCollection USPortOfExitList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ZZRefCusCodeListCombinedCollection TradeZones
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, UniversalReferenceConstants.RefCusCodeListType.Codes.CACTradeZone, ZDateTime.Today); }
		}

		public CodeDescriptionPairList GSTStatusCodes => LookupsHelper.GSTStatusCodes(Factory);

		public CodeDescriptionPairList SIMACodes
		{
			get { return Factory.GetCachedValue<SIMACodes>(); }
		}

		public CodeDescriptionPairList ETExemptionCodes => LookupsHelper.ETExemptionCodes(Factory);

		protected CodeDescriptionPairList StatesOfOriginBase(bool isExport, ZString origin) => LookupsHelper.StatesOfOriginBase(Factory, isExport, origin);

		public CodeDescriptionPairList ATDExemptionCodes
		{
			get { return Factory.GetCachedValue<ATDExemptionCodes>(); }
		}

		public CodeDescriptionPairList AmendmentCodes
		{
			get { return Factory.GetCachedValue<EManifestAmendmentReasonCodes>(); }
		}

		public CodeDescriptionPairList CountryCodes
		{
			get
			{
				return Factory.GetCachedValue("CountryCodes", () =>
				{
					var result = new CodeDescriptionPairList();
					var collection = new RefCountryCollection(Factory);
					result.AddRange(collection);
					return result;
				});
			}
		}

		public CodeDescriptionPairList USStateCodes
		{
			get { return Factory.GetCachedValue<USStatesList>(); }
		}

		public CodeDescriptionPairList OGDStatusCodes
		{
			get { return Factory.GetCachedValue<AVSStatusList>(); }
		}

		public CodeDescriptionPairList CAInitiatedByList
		{
			get { return Factory.GetCachedValue<CAInitiatedByList>(); }
		}

		public CodeDescriptionPairList CAExceptionCodeList
		{
			get { return Factory.GetCachedValue<CAExceptionCodeList>(); }
		}

		public CodeDescriptionPairList CA_PGAIndicatorList => LookupsHelper.CA_PGAIndicatorList(Factory);

		public CodeDescriptionPairList AmendmentToList
		{
			get { return Factory.GetCachedValue<AmendmentToList>(); }
		}

		public CodeDescriptionPairList ModelYearList
		{
			get { return Factory.GetCachedValue("ModelYearList", () => YearListHelper.GetYearList(ZDateTime.Today.Year + 2)); }
		}
	}
}
