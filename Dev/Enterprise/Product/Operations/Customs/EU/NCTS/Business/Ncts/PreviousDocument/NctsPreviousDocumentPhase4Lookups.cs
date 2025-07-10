using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPreviousDocumentPhase4Lookups : NctsPreviousDocumentLookups
	{
		public NctsPreviousDocumentPhase4Lookups(NctsPreviousDocument parent)
			: base(parent)
		{
		}

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
						result.AddRange(ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, countryCode, codeType, ZDateTime.Today));
						result.SortByDescription();
						return result;
					});
			}
		}
	}
}
