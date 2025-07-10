using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Startup.DotNetVersionSwitch
{
	interface IDotNetVersionSwitchManager
	{
		IDotNetVersionManager GetDefaultNetVersionItem();
		IEnumerable<ZToolStripMenuItem> GetSwitchVersionMenuItems();
		bool IsNetVersionSwitchEnabled();
		bool EnforcedNetCoreVersionForUser();
	}

	class DotNetVersionSwitchManager : IDotNetVersionSwitchManager
	{
		readonly bool _isRunningTest;
		readonly IDotNetVersionManager[] _availableNetVersions;

		public DotNetVersionSwitchManager(bool isRunningTest, IDotNetVersionManager[] availableNetVersions = null)
		{
			_isRunningTest = isRunningTest;
			_availableNetVersions = availableNetVersions ?? GetDefaultAvailableNetVersions();

			IDotNetVersionManager[] GetDefaultAvailableNetVersions() =>
			[
				DotNetFrameworkVersionManager.Instance,
				DotNetCore8VersionManager.Instance
			];
		}

		public bool IsNetVersionSwitchEnabled()
		{
			if (_isRunningTest)
			{
				return false;
			}

			if (RawDataRegistry.Instance.GetRegistryOptionForNetVersionSwitch() is RegistryOptions.IsHidden)
			{
				return false;
			}
			return true;
		}

		public IDotNetVersionManager GetDefaultNetVersionItem()
		{
			if (EnforcedNetCoreVersionForUser())
			{
				return DotNetCore8VersionManager.Instance;
			}

			switch (DataRegistry.Instance.DefaultDotNetVersionOnLaunch)
			{
				case DotNetBuildVersionTargetTypeList.Codes.NetFramework48:
					return DotNetFrameworkVersionManager.Instance;
				case DotNetBuildVersionTargetTypeList.Codes.NetCore8:
					return DotNetCore8VersionManager.Instance;
				default:
					break;
			}
			return DotNetFrameworkVersionManager.Instance;
		}

		public IEnumerable<ZToolStripMenuItem> GetSwitchVersionMenuItems()
		{
			if (!IsNetVersionSwitchEnabled())
			{
				return Enumerable.Empty<ZToolStripMenuItem>();
			}

			if (!DataRegistry.Instance.EnableDotNetVersionSwitchMenu)
			{
				return Enumerable.Empty<ZToolStripMenuItem>();
			}

			var menuItems = new List<ZToolStripMenuItem>();
			foreach (var netVersionItem in _availableNetVersions)
			{
				if (netVersionItem.IsVersionCurrentRunning())
				{
					continue;
				}
				menuItems.Add(
					new ZToolStripMenuItem(
						netVersionItem.MenuCaption,
						(_, _) => netVersionItem.LaunchCurrentVersion()));
			}
			return menuItems;
		}

		public bool EnforcedNetCoreVersionForUser()
		{
			return GlbStaff.CurrentUser.CheckNetCoreVersionEnforcedForUser();
		}
	}
}
