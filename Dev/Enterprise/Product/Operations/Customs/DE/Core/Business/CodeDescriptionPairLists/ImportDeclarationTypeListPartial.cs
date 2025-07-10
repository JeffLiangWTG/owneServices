using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public partial class ImportDeclarationTypeList
	{
		public static bool IsLocalClearanceDateRelevant(string decTypeCode)
		{
			return decTypeCode == Codes.AAV ||
			decTypeCode == Codes.AZ ||
			decTypeCode == Codes.AZL;
		}

		public static bool IsForImportFromSpecialTerritoryEntryStyle(string decTypeCode)
		{
			return decTypeCode == Codes.AZ ||
			decTypeCode == Codes.EZA ||
			decTypeCode == Codes.VZA;
		}

		public static bool IsIndirectRepresentationNotAllowed(string decTypeCode)
		{
			return decTypeCode == Codes.EAV;
		}

		public static bool IsLUZ(string decTypeCode) => decTypeCode == Codes.LUZ;

		public static CodeDescriptionPairList GetSimplifiedDeclarationTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("DE.ImportDeclarationTypeList.GetSimplifiedDeclarationTypeList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.AAV, Descriptions.AAV);
				result.AddPair(Codes.AZ, Descriptions.AZ);
				result.AddPair(Codes.AZL, Descriptions.AZL);
				result.AddPair(Codes.VAV, Descriptions.VAV);
				result.AddPair(Codes.VZA, Descriptions.VZA);
				result.AddPair(Codes.VZL, Descriptions.VZL);
				return result;
			});
		}

		public static bool IsSimplifiedWarehouse(ZString style) => style == Codes.AZL || style == Codes.VZL;

		public static bool IsSimplifiedInwardProcessing(ZString style) => style == Codes.AAV || style == Codes.VAV;

		public static bool IsSimplifiedFreeCirculation(ZString style) => style == Codes.AZ || style == Codes.VZA;
	}
}
