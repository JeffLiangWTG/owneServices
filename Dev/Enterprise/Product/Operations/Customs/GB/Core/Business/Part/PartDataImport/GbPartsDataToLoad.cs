using CargoWise.Types;

namespace Enterprise.Customs.GB.Business
{
	public class GbPartsDataToLoad : Enterprise.MasterFiles.Business.PartsDataToLoad
	{
		public static class Schema
		{
			public const string ECSUPPLEMENT1 = "ECSUPPLEMENT1";
			public const string ECSUPPLEMENT2 = "ECSUPPLEMENT2";
			public const string ECSUPPLEMENT = "ECSUPPLEMENT";
			public const string CPC = "CPC";
			public const string THIRDQTY = "THIRDQTY";
		}

		public ZString ECSUPPLEMENT1;
		public ZString ECSUPPLEMENT2;
		public ZString ECSUPPLEMENT;
		public ZString CPC;
		public ZString THIRDQTY;
	}
}
