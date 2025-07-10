using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface ICusTempStorageLine
			{
				ZGuid PK { get; }

				ZGuid TSL_STH { get; set; }
			}
		}
	}
}
