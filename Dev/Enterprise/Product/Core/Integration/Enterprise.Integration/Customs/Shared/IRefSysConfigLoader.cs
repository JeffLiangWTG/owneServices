using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IRefSysConfigLoader
			{
				ZString GetStringValue(ZString configCode, ZDateTime? startDate = null);
				ZBool GetBoolValue(ZString configCode, ZDateTime? startDate = null);
			}
		}
	}
}
