using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsPreviousDocumentPhase4Lookups : EU.NCTS.Business.NctsPreviousDocumentPhase4Lookups
	{
		public NctsPreviousDocumentPhase4Lookups(NctsPreviousDocument parent) : base(parent)
		{
		}

		protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

		public override CodeDescriptionPairList SubTypeList
		{
			get
			{
				return Factory.GetCachedValue(string.Format(Culture.Invariant, "ES.NctsPreviousDocumentPhase4Lookups"), () =>
				{
					var result = new PreviousDocumentClassList();
					result.RemoveCode(PreviousDocumentClassList.Codes.PreviousAdministrativeReferenceNCTS);
					return result;
				});
			}
		}

		#region Implementation

		public override ICollection CodeList
		{
			get
			{
				ZString countryCode = Parent.ImportExportParent?.DataGroupingCode ?? ZString.Empty;
				var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS;

				return countryCode.IsEmpty ?
					new CodeDescriptionPairList() :
					Factory.GetCachedValue("CodeList_RefCusCodeListTypes_" + codeType + "_" + countryCode, () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddRange(ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, countryCode, codeType, ZDateTime.Today, additionalFilter: CreateRefCusListRefCusCodeListAttributeFiltersNotExistZZE_ZXE_NKName(UniversalReferenceConstants.RefCusCodeListAttributes.Names.Level)));
						result.SortByDescription();
						return result;
					});
			}
		}

		ZQuery CreateRefCusListRefCusCodeListAttributeFiltersNotExistZZE_ZXE_NKName(ZString nameNotExist)
		{
			var result = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
			var notExistsNameSubQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, true);
			notExistsNameSubQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, nameNotExist);
			result.AddSubQuery(notExistsNameSubQuery, JoinCondition.And);

			return result;
		}

		#endregion
	}
}
