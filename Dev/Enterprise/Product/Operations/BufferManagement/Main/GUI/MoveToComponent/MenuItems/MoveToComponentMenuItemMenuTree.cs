using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class MoveToComponentMenuItemMenuTree : ZMenuItem
	{
		public MoveToComponentMenuItemMenuTree(ProcessHeaderCollectionGetter processHeaders)
			: base(ResString.GetMultilingualString("87D851BA-6613-4B7A-8F7A-3F0D78F1E2B8", "Move To Component"))
		{
			this.processHeaderCollectionGetter = processHeaders;
			Setup();
		}

		readonly ProcessHeaderCollectionGetter processHeaderCollectionGetter;
		List<ProcessHeader> processHeaders;

		void Setup()
		{
			MenuItems.Add(new ZMenuItem("-"));

			Popup += (e, s) =>
			{
				MenuItems.Clear();
				var addedComponents = new List<ZGuid>();
				processHeaders = processHeaderCollectionGetter().ToList();

				foreach (var header in processHeaders)
				{
					var system = header.BMSystem;
					foreach (var component in system.Components.OrderBy(c => c.FC_DisplaySequence).ToList())
					{
						if (!addedComponents.Contains(component.PK))
						{
							addedComponents.Add(component.PK);
							var menuItem = new ZMenuItem(component.FC_Name);

							if (component.FC_IsActive)
							{
								menuItem.Tag = component;
								menuItem.Click += MenuItem_Click;
							}
							else
							{
								menuItem.Enabled = false;
							}

							MenuItems.Add(menuItem);
						}
					}
				}
			};
		}

		void MenuItem_Click(object sender, EventArgs e)
		{
			var menuitem = sender as ZMenuItem;
			if (menuitem != null)
			{
				MoveToComponent((BMComponent)menuitem.Tag);
			}
			else
			{
				MoveToComponent((BMComponent)((ZToolStripMenuItem)sender).Tag);
			}
		}

		void MoveToComponent(BMComponent component)
		{
			if (processHeaders.Count > 0)
			{
				IProcessHeaderExtensions.GroupMoveToComponent(processHeaders.ToArray(), component);
				processHeaders[0].Factory.SaveHandlingZSaveExceptions();
			}
		}

#if DEBUG
		public void OnPopup()
		{
			base.OnPopup(null);
		}
#endif
	}
}
