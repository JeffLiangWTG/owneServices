using CargoWise.Types;

namespace Enterprise.Customs.BR.Manifest.Business
{
	static class MercanteHelper
	{
		internal static int GetLoadType(ZString containerMode)
		{
			var loadType = 0;
			switch (containerMode)
			{
				case Core.Constants.ContainerModes.Containerised:
					loadType = MercanteConstants.LoadType.Containerised;
					break;
				case Core.Constants.ContainerModes.BreakBulk:
					loadType = MercanteConstants.LoadType.BreakBulk;
					break;
				case Core.Constants.ContainerModes.Bulk:
					loadType = MercanteConstants.LoadType.Bulk;
					break;
			}
			return loadType;
		}
	}
}
