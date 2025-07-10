using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public partial class ExportDeclarationTypeProcedureList
	{
		public static CodeDescriptionPairList GetTypeProcedureList(ZString typeTime)
		{
			var list = new CodeDescriptionPairList();
			switch (typeTime)
			{
				case ExportDeclarationTypeTimeList.Codes._00:
					list.AddPair(Codes._000100, ExportDeclarationTypeTimeProcedureList.Descriptions._00000100);
					list.AddPair(Codes._000110, ExportDeclarationTypeTimeProcedureList.Descriptions._00000110);
					list.AddPair(Codes._000200, ExportDeclarationTypeTimeProcedureList.Descriptions._00000200);
					list.AddPair(Codes._000210, ExportDeclarationTypeTimeProcedureList.Descriptions._00000210);
					list.AddPair(Codes._000400, ExportDeclarationTypeTimeProcedureList.Descriptions._00000400);
					list.AddPair(Codes._000901, ExportDeclarationTypeTimeProcedureList.Descriptions._00000901);
					list.AddPair(Codes._000902, ExportDeclarationTypeTimeProcedureList.Descriptions._00000902);
					list.AddPair(Codes._001300, ExportDeclarationTypeTimeProcedureList.Descriptions._00001300);
					list.AddPair(Codes._001310, ExportDeclarationTypeTimeProcedureList.Descriptions._00001310);
					list.AddPair(Codes._001410, ExportDeclarationTypeTimeProcedureList.Descriptions._00001410);
					list.AddPair(Codes._110100, ExportDeclarationTypeTimeProcedureList.Descriptions._00110100);
					list.AddPair(Codes._110110, ExportDeclarationTypeTimeProcedureList.Descriptions._00110110);
					list.AddPair(Codes._110200, ExportDeclarationTypeTimeProcedureList.Descriptions._00110200);
					list.AddPair(Codes._110210, ExportDeclarationTypeTimeProcedureList.Descriptions._00110210);
					list.AddPair(Codes._110400, ExportDeclarationTypeTimeProcedureList.Descriptions._00110400);
					list.AddPair(Codes._111300, ExportDeclarationTypeTimeProcedureList.Descriptions._00111300);
					list.AddPair(Codes._111310, ExportDeclarationTypeTimeProcedureList.Descriptions._00111310);
					list.AddPair(Codes._111410, ExportDeclarationTypeTimeProcedureList.Descriptions._00111410);
					list.AddPair(Codes._120100, ExportDeclarationTypeTimeProcedureList.Descriptions._00120100);
					list.AddPair(Codes._120110, ExportDeclarationTypeTimeProcedureList.Descriptions._00120110);
					list.AddPair(Codes._120200, ExportDeclarationTypeTimeProcedureList.Descriptions._00120200);
					list.AddPair(Codes._120210, ExportDeclarationTypeTimeProcedureList.Descriptions._00120210);
					list.AddPair(Codes._200100, ExportDeclarationTypeTimeProcedureList.Descriptions._00200100);
					list.AddPair(Codes._200110, ExportDeclarationTypeTimeProcedureList.Descriptions._00200110);
					list.AddPair(Codes._200200, ExportDeclarationTypeTimeProcedureList.Descriptions._00200200);
					list.AddPair(Codes._200210, ExportDeclarationTypeTimeProcedureList.Descriptions._00200210);
					list.AddPair(Codes._200400, ExportDeclarationTypeTimeProcedureList.Descriptions._00200400);
					list.AddPair(Codes._201300, ExportDeclarationTypeTimeProcedureList.Descriptions._00201300);
					list.AddPair(Codes._201310, ExportDeclarationTypeTimeProcedureList.Descriptions._00201310);
					list.AddPair(Codes._201410, ExportDeclarationTypeTimeProcedureList.Descriptions._00201410);
					break;
				case ExportDeclarationTypeTimeList.Codes._10:
					list.AddPair(Codes._000000, ExportDeclarationTypeTimeProcedureList.Descriptions._10000000);
					list.AddPair(Codes._000400, ExportDeclarationTypeTimeProcedureList.Descriptions._10000400);
					list.AddPair(Codes._110000, ExportDeclarationTypeTimeProcedureList.Descriptions._10110000);
					list.AddPair(Codes._110400, ExportDeclarationTypeTimeProcedureList.Descriptions._10110400);
					list.AddPair(Codes._200000, ExportDeclarationTypeTimeProcedureList.Descriptions._10200000);
					list.AddPair(Codes._200400, ExportDeclarationTypeTimeProcedureList.Descriptions._10200400);
					break;
				case ExportDeclarationTypeTimeList.Codes._11:
					list.AddPair(Codes._000000, ExportDeclarationTypeTimeProcedureList.Descriptions._11000000);
					list.AddPair(Codes._110000, ExportDeclarationTypeTimeProcedureList.Descriptions._11110000);
					list.AddPair(Codes._120000, ExportDeclarationTypeTimeProcedureList.Descriptions._11120000);
					list.AddPair(Codes._200000, ExportDeclarationTypeTimeProcedureList.Descriptions._11200000);
					break;
				case ExportDeclarationTypeTimeList.Codes._12:
					list.AddPair(Codes._000000, ExportDeclarationTypeTimeProcedureList.Descriptions._12000000);
					list.AddPair(Codes._000400, ExportDeclarationTypeTimeProcedureList.Descriptions._12000400);
					list.AddPair(Codes._110000, ExportDeclarationTypeTimeProcedureList.Descriptions._12110000);
					list.AddPair(Codes._110400, ExportDeclarationTypeTimeProcedureList.Descriptions._12110400);
					list.AddPair(Codes._120000, ExportDeclarationTypeTimeProcedureList.Descriptions._12120000);
					list.AddPair(Codes._200000, ExportDeclarationTypeTimeProcedureList.Descriptions._12200000);
					list.AddPair(Codes._200400, ExportDeclarationTypeTimeProcedureList.Descriptions._12200400);
					break;
				case ExportDeclarationTypeTimeList.Codes._13:
					list.AddPair(Codes._000000, ExportDeclarationTypeTimeProcedureList.Descriptions._13000000);
					break;
				case ExportDeclarationTypeTimeList.Codes._20:
					list.AddPair(Codes._000000, ExportDeclarationTypeTimeProcedureList.Descriptions._20000000);
					break;
			}
			return list;
		}

		public static bool IsIncompleteDeclaration(ZString typeProcedure)
		{
			return typeProcedure.SubstringSafe(4, 1) == "1";
		}

		public static bool Is000000(ZString typeProcedure) => typeProcedure == Codes._000000;

		public static bool IsPresentationOutsideOfficialPlace(BusinessObjectFactory factory, ZString typeTime, ZString typeProcedure)
		{
			return typeTime == ExportDeclarationTypeTimeList.Codes._00 && PresentationOutsideOfficialPlaceSet(factory).Contains(typeProcedure.SubstringSafe(2, 2));
		}

		static ImmutableHashSet<string> PresentationOutsideOfficialPlaceSet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.ExportDeclarationTypeProcedureList | PresentationOutsideOfficialPlaceSet", // Cache Key
				() => ImmutableHashSet.Create("01", "02", "09"));
		}
	}
}
