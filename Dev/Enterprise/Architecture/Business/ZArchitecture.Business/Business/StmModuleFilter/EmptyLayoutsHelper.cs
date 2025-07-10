using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class EmptyLayoutsHelper : FilterStripLayoutsHelper
	{
		protected override ZGuid GetCurrentUserPk()
		{
			return ZGuid.Empty;
		}

		protected override ZString GetCurrentUserTablePrefix()
		{
			return ZString.Empty;
		}
	}
}
