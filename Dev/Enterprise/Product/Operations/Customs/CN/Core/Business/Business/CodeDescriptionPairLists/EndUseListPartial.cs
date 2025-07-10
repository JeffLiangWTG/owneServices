namespace Enterprise.Customs.CN.Business
{
	public partial class EndUseList
	{
		public static string[] GetRequiredCargoAttributesByCIQEndUse(string ciqEndUse)
		{
			string[] cargoAttributes = null;

			switch (ciqEndUse)
			{
				case Codes.Edible:
				case Codes.Cosmetics:
					cargoAttributes = new[] { CargoAttributeList.Codes._14, CargoAttributeList.Codes._15 };
					break;
			}

			return cargoAttributes;
		}
	}
}
