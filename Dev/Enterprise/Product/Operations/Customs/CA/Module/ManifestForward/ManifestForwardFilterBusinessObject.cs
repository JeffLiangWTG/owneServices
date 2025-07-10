using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	class ManifestForwardFilterBusinessObject : EDIMessageFilterBusinessObject
	{
		protected GenAddOnColumnQueryHelper SimpleQueryHelper
		{
			get { return simpleQueryHelper ?? (simpleQueryHelper = new GenAddOnColumnQueryHelper(typeof(EDIMessage))); }
		}
		GenAddOnColumnQueryHelper simpleQueryHelper;

		internal new static class Constants
		{
			public const string HouseCCN = "CCN - House";
			public const string PrimaryCCN = "CCN - Primary";
			public const string SNPType = "SNP Type";
			public const string SubLocation = "Sub-Location";
			public const string CBSAOffice = "CBSA Office";

			public static MultilingualString HouseCCNMultilingualDescription => ResString.GetMultilingualString("CA|ManifestForwardFilterBusinessObject|HouseCCN", HouseCCN);
			public static MultilingualString PrimaryCCNMultilingualDescription => ResString.GetMultilingualString("CA|ManifestForwardFilterBusinessObject|PrimaryCCN", PrimaryCCN);
			public static MultilingualString SNPTypeMultilingualDescription => ResString.GetMultilingualString("CA|ManifestForwardFilterBusinessObject|SNPType", SNPType);
			public static MultilingualString SubLocationMultilingualDescription => ResString.GetMultilingualString("CA|ManifestForwardFilterBusinessObject|SubLocation", SubLocation);
			public static MultilingualString CBSAOfficeMultilingualDescription => ResString.GetMultilingualString("CA|ManifestForwardFilterBusinessObject|CBSAOffice", CBSAOffice);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			var filterPart = result.AddTextFilter(Constants.PrimaryCCN, (c, v) => SimpleQueryHelper.GetQueryHandlingBlanks(ACIHouseBillMessage.Schema.PrimaryCCN, c, v))
				.WithMaxLengthOf<ModuleTextFilter>(GenAddOnColumnSchema.XA_Data);
			filterPart.Category = FilterCategories.NumbersAndReferences;
			filterPart.MultilingualDescription = Constants.PrimaryCCNMultilingualDescription;

			result.AddNumberFilter(Constants.HouseCCN, GetHouseCCNQuery).WithMaxLengthOf<ModuleNumberFilter>(EDIMessageSchema.EM_ApplicationReference)
				.MultilingualDescription = Constants.HouseCCNMultilingualDescription;

			filterPart = result.AddTextFilter(Constants.SNPType, (c, v) => SimpleQueryHelper.GetQueryHandlingBlanks(EDIMessage.Schema.SNPType, c, v), new SecondaryNotifyPartyTypeList())
				.WithMaxLengthOf<ModuleTextFilter>(GenAddOnColumnSchema.XA_Data);
			filterPart.Category = FilterCategories.ModesAndTypes;
			filterPart.MultilingualDescription = Constants.SNPTypeMultilingualDescription;

			var nkFilter = result.AddNkFilter(Constants.SubLocation, (c, v) => SimpleQueryHelper.GetQueryHandlingBlanks(EDIMessage.Schema.SubLocation, c, v), ModuleIDs.Customs.CA.SubLocation, new CACSubLocationCollection(Factory)).WithMaxLengthOf<ModuleNkFilter>(GenAddOnColumnSchema.XA_Data);
			nkFilter.Category = FilterCategories.Locations;
			nkFilter.MultilingualDescription = Constants.SubLocationMultilingualDescription;

			nkFilter = result.AddNkFilter(Constants.CBSAOffice, (c, v) => SimpleQueryHelper.GetQueryHandlingBlanks(EDIMessage.Schema.CBSAOffice, c, v), ModuleIDs.Customs.Universal.ZZRefCusCodeList, GetOffices()).WithMaxLengthOf<ModuleNkFilter>(GenAddOnColumnSchema.XA_Data);
			nkFilter.Category = FilterCategories.Locations;
			nkFilter.MultilingualDescription = Constants.CBSAOfficeMultilingualDescription;

			return result;
		}

		ZQuery GetHouseCCNQuery(SQLComparisonOperator @operator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.AddToFilter_PossiblyCommaSeparated(EDIMessageSchema.EM_ApplicationReference, @operator, value.Replace(" ", ""));
			return query;
		}

		protected override ZBool ShouldAddApplicationCodeFilter
		{
			get { return false; }
		}

		protected override bool ShouldAddApplicationReferenceFilter
		{
			get { return false; }
		}

		protected override ZBool ShouldAddDirectionFilter
		{
			get { return false; }
		}

		protected override ZBool ShouldAddMessageTypeSubTypeFilter
		{
			get { return false; }
		}

		protected override ZBool ShouldAddEHubIDFilters
		{
			get { return false; }
		}

		ZZRefCusCodeListCombinedCollection GetOffices()
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
		}
	}
}
