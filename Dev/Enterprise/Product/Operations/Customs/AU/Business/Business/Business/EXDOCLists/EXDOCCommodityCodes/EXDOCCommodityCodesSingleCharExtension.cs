namespace Enterprise.Customs.AU.Declaration.Business
{
	partial class EXDOCCommodityCodesSingleChar
	{
		public static string GetSingleCharFromThreeCharCode(string threeCharCode)
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
					result = Codes.GrainsAndSeeds;
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

		public static string GetThreeCharFromSingleCharCode(string singleCharCode)
		{
			string result;
			switch (singleCharCode)
			{
				case Codes.Dairy:
					result = EXDOCCommodityCodes.Codes.Dairy;
					break;
				case Codes.Eggs:
					result = EXDOCCommodityCodes.Codes.Eggs;
					break;
				case Codes.Fish:
					result = EXDOCCommodityCodes.Codes.Fish;
					break;
				case Codes.GrainsAndSeeds:
					result = EXDOCCommodityCodes.Codes.GrainsAndPlants;
					break;
				case Codes.Horticulture:
					result = EXDOCCommodityCodes.Codes.Horticulture;
					break;
				case Codes.InedibleMeat:
					result = EXDOCCommodityCodes.Codes.InedibleMeat;
					break;
				case Codes.Meat:
					result = EXDOCCommodityCodes.Codes.Meat;
					break;
				case Codes.SkinsAndHides:
					result = EXDOCCommodityCodes.Codes.SkinsAndHides;
					break;
				case Codes.Wool:
					result = EXDOCCommodityCodes.Codes.Wool;
					break;
				default:
					result = string.Empty;
					break;
			}
			return result;
		}
	}
}
