using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EUExitControl
		{
			public interface ICusExitHeaderLoader
			{
				(ZQuery mainQuery, ZQuery secondaryQuery) GetLoadQuery(ZGuid parentId, ZString parentTableCode, ZInt? clusterKey = null);
				ICusExitHeader[] Load(bool fetchOnlyFromLocalCache, ZGuid parentId, ZString parentTableCode, ZInt? clusterKey = null);
			}
		}
	}
}
