using System.Linq;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public sealed class LinkMainFormModule : MainFormModule
	{
		#region Constructor

		public LinkMainFormModule(LinkWrapper shortcut)
		{
			RecordKey = shortcut.RecordKey;
			RecordUrl = shortcut.RecordUrl;
			ModuleID = ModuleIDs.AllExcludingClientModules.FirstOrDefault(m => m.Name == shortcut.ModuleName);
			if (ModuleID == null)
			{
				ModuleID = ClientHookLoader.Instance.ClientHook?.NewClientModules.FirstOrDefault(m => m.ID.Name == shortcut.ModuleName)?.ID;
			}

			if (!string.IsNullOrEmpty(shortcut.RecordDescription))
			{
				RecordDescription = shortcut.RecordDescription;
			}
		}

		public LinkMainFormModule(StmLink shortcut)
			: this(new LinkWrapper(shortcut))
		{
		}

		#endregion
	}
}
