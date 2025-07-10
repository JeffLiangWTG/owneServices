using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPackageLookups : Customs.Business.CusInvPackLookups
	{
		public NctsPackageLookups(NctsPackage parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList UnitTypeList => GetPackageUnitTypeList(Factory);

		public static CodeDescriptionPairList GetPackageUnitTypeList(BusinessObjectFactory factory)
		{
			return Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				UNPackTypeStartDate);
		}

		public static CodeDescriptionPairList GetPackageUnitTypeList(BusinessObjectFactory factory, string language)
		{
			return Universal.RefCusCodeListTypes.GetCachedList(factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				UNPackTypeStartDate, languageCode: language);
		}

		public CodeDescriptionPairList BulkPackageUnitTypeList
		{
			get
			{
				return Universal.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory,
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
					UNPackTypeStartDate,
					false,
					Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk);
			}
		}

		public CodeDescriptionPairList UnpackedPackageUnitTypeList
		{
			get
			{
				return Universal.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory,
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
					UNPackTypeStartDate,
					false,
					Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk);
			}
		}

		public override CodeDescriptionPairList TypeOfDifferenceList => ((NctsPackage)Parent).B5_B5_ParentPackage.IsEmpty ? UnloadedStates : base.TypeOfDifferenceList;

		public CodeDescriptionPairList UnloadedStates => UnloadedStatesCore;

		protected virtual CodeDescriptionPairList UnloadedStatesCore
		{
			get
			{
				var package = (NctsPackage)Parent;
				var isNew = package.B5_TypeOfDifferenceInfo.OriginalValue.ToString() == NctsUnloadedStateList.Codes.NEW;

				return Factory.GetCachedValue($"UnloadedStatesCore_{isNew}_{ShouldIncludeDIFInUnloadedStatesList}", () =>
				{
					var exclusions = GetExclusions();

					var list = NctsUnloadedStateList.CreateConfigurableList(exclusions);
					if (!isNew)
					{
						list.RemoveCode(NctsUnloadedStateList.Codes.NEW);
					}
					return list;
				});
			}
		}

		IEnumerable<string> GetExclusions()
		{
			if (!ShouldIncludeDIFInUnloadedStatesList)
			{
				yield return NctsUnloadedStateList.Codes.DIF;
			}
			yield return NctsUnloadedStateList.Codes.DAM;
}

		protected virtual ZBool ShouldIncludeDIFInUnloadedStatesList => false;
	}
}
