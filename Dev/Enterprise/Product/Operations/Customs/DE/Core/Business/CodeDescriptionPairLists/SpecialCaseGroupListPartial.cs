using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CodeDescriptionPairLists
{
	public partial class SpecialCaseGroupList
	{
		public static bool IsInGroups1to12And20(BusinessObjectFactory factory, ZString group) => Groups1to12And20(factory).Contains(group);

		static ImmutableHashSet<string> Groups1to12And20(BusinessObjectFactory factory)
			=> factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.SpecialCaseGroupList | RequiredSpecialCaseGroupForProcedure_4054_4254", // Cache Key
				() => ImmutableHashSet.Create(
					SpecialCaseGroupList.Codes._01,
					SpecialCaseGroupList.Codes._02,
					SpecialCaseGroupList.Codes._03,
					SpecialCaseGroupList.Codes._04,
					SpecialCaseGroupList.Codes._05,
					SpecialCaseGroupList.Codes._06,
					SpecialCaseGroupList.Codes._07,
					SpecialCaseGroupList.Codes._08,
					SpecialCaseGroupList.Codes._09,
					SpecialCaseGroupList.Codes._10,
					SpecialCaseGroupList.Codes._11,
					SpecialCaseGroupList.Codes._12,
					SpecialCaseGroupList.Codes._20
				));
	}
}
