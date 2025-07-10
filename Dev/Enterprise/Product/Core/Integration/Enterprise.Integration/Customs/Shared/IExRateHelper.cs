using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IExRateHelper
			{
				ZInt DecimalPlacesForSellRate(ZString condition);
			}
		}
	}
}
