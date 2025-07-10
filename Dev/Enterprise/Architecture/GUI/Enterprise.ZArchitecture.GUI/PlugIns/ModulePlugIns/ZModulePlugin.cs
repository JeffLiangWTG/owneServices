using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.ModulePlugIn
{
	/// <summary>
	/// Module Level PlugIn - allows functionality to be plugged into to FilterModules.
	/// Useful as it uses ControllerIDs and thus can use reflection to access un-referenced assemblies.
	/// </summary>
	public abstract class ZModulePlugin : IDisposable
	{
		protected ZModulePlugin() { }

		~ZModulePlugin()
		{
			Dispose(false);
		}

		public List<MenuItem> GetActionMenuItemToAdd()
		{
			return GetActionMenuItemToAddCore();
		}

		public MenuItem GetButtonGridMenuItemToAdd(GUI.ZModuleButtonGrid grid)
		{
			return GetButtonGridMenuItemToAddCore(grid);
		}

		public bool RequiresMultiSelect
		{
			get { return RequiresMultiSelectCore; }
		}

		protected virtual List<MenuItem> GetActionMenuItemToAddCore()
		{
			return null;
		}

		protected virtual MenuItem GetButtonGridMenuItemToAddCore(GUI.ZModuleButtonGrid grid)
		{
			return null;
		}

		protected virtual bool RequiresMultiSelectCore
		{
			get { return false; }
		}

		#region IDisposable Members

		protected virtual void Dispose(bool isDisposing)
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
