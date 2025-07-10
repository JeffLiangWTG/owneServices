using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	/// <summary>
	/// Returns an object that provides the source location for advising clients of how to rectify empty/missing groups
	/// </summary>
	public class GroupSourceLocator : IGroupSourceLocator
	{
		GroupSourceLocator(MultilingualString location)
		{
			Location = location;
		}

		/// <summary>
		/// Create locator from group. 
		/// </summary>
		/// <param name="group"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is for postmasters and for developers")]
		public static IGroupSourceLocator GetFromGroup(IGlbGroup group)
		{
			if (group != null)
			{
				return new GroupSourceLocator((NoResString)("'" + group.GG_Desc + "'" + " (code: '" + group.GG_Code + "')"));
			}
			return null;
		}

		/// <summary>
		/// Create locator for registry items. Use Env.Registry.RawRegistry.[RegistryName] for old registry items
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IGroupSourceLocator GetFromRegistryItem(IRegistryItem item)
		{
			if (item != null)
			{
				return new GroupSourceLocator(((IMultilingualRegistryItem)item).LocationMultilingual);
			}
			return null;
		}

		public MultilingualString Location { get; private set; }
	}
}
