using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IExRateValidationHelper
			{
				void CheckIfAsPublishedIsInRightFormat(ZDecimal exRate, ZString asPublished, ZPropertyInfo asPublishedPropertyInfo);
			}
		}
	}
}
