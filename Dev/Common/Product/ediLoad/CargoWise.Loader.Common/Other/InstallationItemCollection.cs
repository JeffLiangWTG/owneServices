using System.Collections.Generic;

namespace CargoWise.Loader.Common
{
	public sealed class InstallationItemCollection : List<InstallationItem>
	{
		public IEnumerable<InstallationItem> GetDepthFirstEnumerable()
		{
			foreach (InstallationItem thisLevelItem in this)
			{
				foreach (InstallationItem deeperItem in thisLevelItem.Dependencies.GetDepthFirstEnumerable())
				{
					yield return deeperItem;
				}
				yield return thisLevelItem;
			}
			yield break;
		}
	}
}