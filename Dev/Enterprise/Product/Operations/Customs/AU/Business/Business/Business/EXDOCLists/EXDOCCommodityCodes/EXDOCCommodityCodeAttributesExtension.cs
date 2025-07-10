namespace Enterprise.Customs.AU.Declaration.Business
{
	partial class EXDOCCommodityCodeAttributes
	{
		public static string GetAttributeNameFromThreeCharCode(string threeCharCode)
		{
			string result;
			switch (threeCharCode)
			{
				case EXDOCCommodityCodes.Codes.Dairy:
					result = Codes.Dairy;
					break;
				case EXDOCCommodityCodes.Codes.Eggs:
					result = Codes.Eggs;
					break;
				case EXDOCCommodityCodes.Codes.Fish:
					result = Codes.Fish;
					break;
				case EXDOCCommodityCodes.Codes.GrainsAndPlants:
					result = Codes.GrainsAndPlants;
					break;
				case EXDOCCommodityCodes.Codes.Horticulture:
					result = Codes.Horticulture;
					break;
				case EXDOCCommodityCodes.Codes.InedibleMeat:
					result = Codes.InedibleMeat;
					break;
				case EXDOCCommodityCodes.Codes.Meat:
					result = Codes.Meat;
					break;
				case EXDOCCommodityCodes.Codes.SkinsAndHides:
					result = Codes.SkinsAndHides;
					break;
				case EXDOCCommodityCodes.Codes.Wool:
					result = Codes.Wool;
					break;
				default:
					result = string.Empty;
					break;
			}
			return result;
		}
	}
}
