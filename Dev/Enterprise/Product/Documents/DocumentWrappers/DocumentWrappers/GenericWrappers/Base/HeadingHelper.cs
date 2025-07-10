using CargoWise.Types;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public static class HeadingHelper
	{
		public static ZString GetHouseBillHeading(ZString transportMode)
		{
			if (transportMode == Core.Constants.TransportModes.Air)
			{
				return Res.GetString("a8cd1b5b-474f-4d48-90e4-38238f43f0a7", "HAWB");
			}
			else
			{
				return Res.GetString("190102b6-69bf-475a-866f-4eacba8a3b0a", "House Bill");
			}
		}

		public static ZString GetMasterBillHeading(ZString transportMode)
		{
			if (transportMode == Core.Constants.TransportModes.Air)
			{
				return Res.GetString("175e7977-5d63-44a5-b014-0797052ad80e", "MAWB");
			}
			else
			{
				return Res.GetString("6e961a44-c519-4284-b8dc-8719577e946e", "Master Bill");
			}
		}
	}
}
