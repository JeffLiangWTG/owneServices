using System.Collections.Generic;
using System.Linq;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management
{
	public static class ActivityDataContextManagerHelper
	{
		public static IEnumerable<IActivityDataContextManager> GetActivityDataContextManagers(DataContextType type)
		{
			return DataContextManagersFactory.All.OfType<IActivityDataContextManager>().Where(x => x.DoesManageDataContextType(type));
		}
	}
}
