using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.CodeDescriptionPairLists
{
	public partial class ImportSubStyleList
	{
		public static bool IsFinalDeclaration(BusinessObjectFactory factory, ZString variant) => ImportFinalSubstyleSet(factory).Contains(variant);
		static ImmutableHashSet<string> ImportFinalSubstyleSet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ImportSubStyleList|ImportFinalSubstyleSet",
				() => ImmutableHashSet.Create(Codes.A, Codes.B)
			);
		}

		public static bool IsPrematureDeclaration(BusinessObjectFactory factory, ZString variant) => ImportPrematureSubstyleSet(factory).Contains(variant);
		static ImmutableHashSet<string> ImportPrematureSubstyleSet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.DE.Business.CodeDescriptionPairLists.ImportSubStyleList|ImportPrematureSubstyleSet",
				() => ImmutableHashSet.Create(Codes.D, Codes.E, Codes.F)
			);
		}
	}
}
