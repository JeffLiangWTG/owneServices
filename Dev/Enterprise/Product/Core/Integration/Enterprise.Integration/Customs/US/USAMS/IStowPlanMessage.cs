using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class USAMS
			{
				public interface IStowPlanMessage
				{
					ZInt CountAcceptedContainersWhichPreviouslyNotAccepted();
				}
			}
		}
	}
}