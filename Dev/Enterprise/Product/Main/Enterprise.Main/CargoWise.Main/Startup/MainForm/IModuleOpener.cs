using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Startup
{
	/// <summary>
	/// Implemented by a user interface component that can display modules to the user.
	/// </summary>
	// Obsolete(?) because only one class, MainForm, now implements this.
	// TileNavigationBar used to also implement this but it was removed in https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/commit/6b8952c0e4585204fd3ef6eff9d653cc184d016b?path=%2FEnterprise%2FProduct%2FMain%2FEnterprise.Main%2FNavigationBar%2FTileNavigationBar.cs&gridItemType=2&mpath=%2FEnterprise%2FProduct%2FMain%2FEnterprise.Main%2FNavigationBar%2FTileNavigationBar.cs&opath=%2FEnterprise%2FProduct%2FMain%2FEnterprise.Main%2FNavigationBar%2FTileNavigationBar.cs&mversion=GC6b8952c0e4585204fd3ef6eff9d653cc184d016b&oversion=GC7446a4fe8536b7361d7ddfb4b5308aa7c90632e3&_a=compare
	public interface IModuleOpener
	{
		void OpenModule(MainFormModule module, bool isANewModuleClick);
		void UpdateRecentModules(MainFormModule module);
	}
}
