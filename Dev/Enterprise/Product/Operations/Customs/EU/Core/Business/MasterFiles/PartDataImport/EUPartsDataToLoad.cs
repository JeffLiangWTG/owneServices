using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class EUPartsDataToLoad : Customs.Business.GlobalPartsDataToLoad
	{
		public static class Schema
		{
			public const string ECSUPPLEMENT = "ECSUPPLEMENT";
		}

		public ZString ECSUPPLEMENT;
	}
}
