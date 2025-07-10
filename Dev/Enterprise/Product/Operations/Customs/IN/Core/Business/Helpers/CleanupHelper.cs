using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business.Helpers
{
	public static class CleanupHelper
	{
		public static void CleanupIfNotApplicable(bool isApplicable, params ZPropertyInfo[] infos)
		{
			if (!isApplicable)
			{
				foreach (var propertyInfo in infos.Where(x => !x.Value.IsEmpty))
				{
					propertyInfo.Value = propertyInfo.DefaultValue;
				}
			}
		}
	}
}
