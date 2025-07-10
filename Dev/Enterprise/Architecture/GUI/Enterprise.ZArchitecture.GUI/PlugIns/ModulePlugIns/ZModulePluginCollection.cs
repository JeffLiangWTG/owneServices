using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.ModulePlugIn
{
	[System.Diagnostics.DebuggerDisplay("Count = {plugins.Count}")]
	public sealed partial class ZModulePluginCollection : IDisposable, IEnumerable<ZModulePlugin>
	{
		public ZModulePluginCollection(ZFilterGridModule module)
		{
			this.module = module;
		}

		public void Add(ControllerID controllerId)
		{
			if (controllerId == null)
			{
				throw new ArgumentNullException(nameof(controllerId));
			}

			if (plugins.ContainsKey(controllerId))
			{
				throw new InvalidOperationException(string.Format("'{0}' has already been added", controllerId));
			}

			haveUnloadedPlugins = true;
			plugins.Add(controllerId, null);
		}

		public MenuItem[] GetActionMenuItemsToAdd()
		{
			var result = new List<MenuItem>();

			foreach (var plugin in this)
			{
				var items = plugin.GetActionMenuItemToAdd();
				if (items != null)
				{
					result.AddRange(items);
				}
			}

			return result.ToArray();
		}
		public MenuItem[] GetButtonGridMenuItemsToAdd(GUI.ZModuleButtonGrid grid)
		{
			var result = new List<MenuItem>();

			foreach (var plugin in this)
			{
				var item = plugin.GetButtonGridMenuItemToAdd(grid);
				if (item != null)
				{
					result.Add(item);
				}
			}

			return result.ToArray();
		}

		public ZModulePlugin GetPlugin(ControllerID controllerId)
		{
			ZModulePlugin result;
			if (plugins.TryGetValue(controllerId, out result) && result == null) // contains the controllerId but the plugin is not yet loaded.
			{
				result = LoadPlugin(controllerId);

				if (result == null)
				{
					plugins.Remove(controllerId);
				}
				else
				{
					plugins[controllerId] = result;
				}
			}
			return result;
		}

		void LoadPlugins()
		{
			if (haveUnloadedPlugins)
			{
				haveUnloadedPlugins = false;
				var pluginsToLoad = new List<ControllerID>();

				foreach (var pair in plugins)
				{
					if (pair.Value == null)
					{
						pluginsToLoad.Add(pair.Key);
					}
				}

				foreach (var controllerId in pluginsToLoad)
				{
					var plugin = LoadPlugin(controllerId);

					if (plugin == null)
					{
						plugins.Remove(controllerId);
					}
					else
					{
						plugins[controllerId] = plugin;
					}
				}
			}
		}

		ZModulePlugin LoadPlugin(ControllerID controllerId)
		{
			try
			{
				var controller = ZControllerFactory.Create(controllerId);
				return controller.GetModulePlugin(module);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ZModulePluginCollection.LoadPlugin:" + controllerId.Name, "Exception caught attempting to load a module plugin", ex);
				return null; // returning null will result in the plugin being skipped.
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			foreach (var plugin in plugins.Values)
			{
				if (plugin != null)
				{
					try
					{
						plugin.Dispose();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce(string.Format("exception caught disposing '{0}'", plugin.GetType().FullName), ex);
					}
				}
			}
		}

		#endregion

		#region IEnumerable<ZModulePlugin> Members

		public IEnumerator<ZModulePlugin> GetEnumerator()
		{
			LoadPlugins();
			return plugins.Values.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		bool haveUnloadedPlugins;
#if DEBUG
		internal
#endif
		readonly ZFilterGridModule module;
#if DEBUG
		internal
#endif
		readonly Dictionary<ControllerID, ZModulePlugin> plugins = new Dictionary<ControllerID, ZModulePlugin>();
	}
}
