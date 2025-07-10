using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public partial class ExportLicenseGroupList
	{
		public static bool IsSingleBAFA(BusinessObjectFactory factory, ZString type) => SingleBAFASet(factory).Contains(type);

		static ImmutableHashSet<string> SingleBAFASet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ExportLicenseGroupList | SingleBAFASet", // Cache Key
				() => ImmutableHashSet.Create(
						Codes._3LLA231, Codes._3LLA82, Codes._3LLB231, Codes._3LLB81E, Codes._3LLC231, Codes._3LLC81E,
						Codes.E020231, Codes.E020FWE, Codes.X002231, Codes.X002DEE
					)
				);
		}

		public static bool IsComplementaryBAFA(BusinessObjectFactory factory, ZString type) => ComplementaryBAFASet(factory).Contains(type);

		static ImmutableHashSet<string> ComplementaryBAFASet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ExportLicenseGroupList | ComplementaryBAFASet", // Cache Key
				() => ImmutableHashSet.Create(
						Codes._3LLB81K
					)
				);
		}

		public static bool IsSingleBAFAAF(BusinessObjectFactory factory, ZString type) => SingleBAFAAFSet(factory).Contains(type);

		static ImmutableHashSet<string> SingleBAFAAFSet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ExportLicenseGroupList | SingleBAFAAFSet", // Cache Key
				() => ImmutableHashSet.Create(
						Codes.C064DE, Codes.E990DEE
					)
				);
		}

		public static bool IsEmbargoBAFA(BusinessObjectFactory factory, ZString type) => EmbargoBAFASet(factory).Contains(type);

		static ImmutableHashSet<string> EmbargoBAFASet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ExportLicenseGroupList | EmbargoBAFASet", // Cache Key
				() => ImmutableHashSet.Create(
						Codes.C052AF, Codes.C052BY, Codes.C052GN, Codes.C052GW, Codes.C052IR, Codes.C052KP, Codes.C052LY, Codes.C052MM, Codes.C052RU,
						Codes.C052SD, Codes.C052SS, Codes.C052SY, Codes.C052UA, Codes.C052VE, Codes.C052ZW, Codes.C069KP, Codes.C070LY
					)
				);
		}

		public static bool IsGeneralEU(BusinessObjectFactory factory, ZString type) => GeneralEUSet(factory).Contains(type);

		static ImmutableHashSet<string> GeneralEUSet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ExportLicenseGroupList | GeneralEUSet", // Cache Key
				() => ImmutableHashSet.Create(
						Codes.C068, Codes.X002E01, Codes.X002E02, Codes.X002E03, Codes.X002E04, Codes.X002E05, Codes.X002E06
					)
				);
		}

		public static bool IsGeneralDE(BusinessObjectFactory factory, ZString type) => GeneralDESet(factory).Contains(type);

		static ImmutableHashSet<string> GeneralDESet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ExportLicenseGroupList | GeneralDESet", // Cache Key
				() => ImmutableHashSet.Create(
						Codes.X002A09, Codes.X002A10, Codes.X002A12, Codes.X002A13, Codes.X002A14, Codes.X002A16, Codes.X002A17,
						Codes._3LLCA18, Codes._3LLCA19, Codes._3LLCA20, Codes._3LLCA21, Codes._3LLCA22, Codes._3LLCA23, Codes._3LLCA24, Codes._3LLCA25, Codes._3LLCA26, Codes._3LLCA27
					)
				);
		}

		public static bool IsSandCBAFA(BusinessObjectFactory factory, ZString type) => SandCBAFASet(factory).Contains(type);

		static ImmutableHashSet<string> SandCBAFASet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ExportLicenseGroupList | SandCBAFASet", // Cache Key
				() => ImmutableHashSet.Create(
						Codes._3LLA231, Codes._3LLA82, Codes._3LLB231, Codes._3LLB81E, Codes._3LLB81S, Codes._3LLC231, Codes._3LLC81E, Codes._3LLC81S,
						Codes.X002231, Codes.X002DEE, Codes.X002DES
					)
				);
		}

		public static bool IsSingleBAFAFWV(BusinessObjectFactory factory, ZString type) => SingleBAFAFWVSet(factory).Contains(type);

		static ImmutableHashSet<string> SingleBAFAFWVSet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ExportLicenseGroupList | SingleBAFAFWVSet", // Cache Key
				() => ImmutableHashSet.Create(
						Codes.E020231, Codes.E020FWE
					)
				);
		}

		public static bool IsSystemBAFA(BusinessObjectFactory factory, ZString type) => SystemBAFASet(factory).Contains(type);

		static ImmutableHashSet<string> SystemBAFASet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ExportLicenseGroupList | SystemBAFASet", // Cache Key
				() => ImmutableHashSet.Create(
						Codes._3LLA231, Codes._3LLA82, Codes._3LLB231, Codes._3LLB81E, Codes._3LLB81K, Codes._3LLB81S, Codes._3LLC231, Codes._3LLC81E, Codes._3LLC81S,
						Codes.C052AF, Codes.C052BY, Codes.C052GN, Codes.C052GW, Codes.C052IR, Codes.C052KP, Codes.C052LY, Codes.C052MM, Codes.C052RU,
						Codes.C052SD, Codes.C052SS, Codes.C052SY, Codes.C052UA, Codes.C052VE, Codes.C052ZW, Codes.C064DE, Codes.C069KP, Codes.C070LY,
						Codes.E020231, Codes.E020FWE, Codes.E020FWS, Codes.E990DEE, Codes.E990DES, Codes.X002231, Codes.X002DEE, Codes.X002DES
					)
				);
		}

		public static bool IsMilitaryWeapons(BusinessObjectFactory factory, ZString type) => MilitaryWeaponsSet(factory).Contains(type);

		static ImmutableHashSet<string> MilitaryWeaponsSet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ExportLicenseGroupList | MilitaryWeaponsSet", // Cache Key
				() => ImmutableHashSet.Create(
						Codes._3LLB231, Codes._3LLB81E, Codes._3LLB81K, Codes._3LLB81S
					)
				);
		}

		public static bool IsNonMilitaryWeapons(BusinessObjectFactory factory, ZString type) => NonMilitaryWeaponsSet(factory).Contains(type);

		static ImmutableHashSet<string> NonMilitaryWeaponsSet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ExportLicenseGroupList | NonMilitaryWeaponsSet", // Cache Key
				() => ImmutableHashSet.Create(
						Codes._3LLC231, Codes._3LLC81E, Codes._3LLC81S
					)
				);
		}

		public static bool IsZeroNoticeBAFA(BusinessObjectFactory factory, ZString type) => ZeroNoticeBAFASet(factory).Contains(type);

		static ImmutableHashSet<string> ZeroNoticeBAFASet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ExportLicenseGroupList | ZeroNoticeBAFASet", // Cache Key
				() => ImmutableHashSet.Create(
						Codes._3LLDNB
					)
				);
		}

		public static bool IsReferenceBAFAAWV(BusinessObjectFactory factory, ZString type) => ReferenceBAFAAWVSet(factory).Contains(type);

		static ImmutableHashSet<string> ReferenceBAFAAWVSet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ExportLicenseGroupList | ReferenceBAFAAWVSet", // Cache Key
				() => ImmutableHashSet.Create(
						Codes.E020AWV
					)
				);
		}
	}
}
