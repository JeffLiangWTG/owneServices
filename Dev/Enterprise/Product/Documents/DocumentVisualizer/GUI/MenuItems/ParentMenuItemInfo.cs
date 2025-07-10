using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	public sealed class ParentMenuItemInfo : IMenuItemInfo
	{
		public string Name { get; set; }
		public IEnumerable<IMenuItemInfo> SubMenus { get; set; }
		public Action<ZMenuItem> OnPopup { get; set; }
	}
}
