using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ISupportedForBonded
		{
			bool IsSupportsBondedWarehousing(BusinessObjectFactory factory, ZString countryCode);
		}
	}
}
