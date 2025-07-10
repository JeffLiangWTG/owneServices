using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.DocumentEngine.ReflectiveFieldMap;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap
{
	public class MapTreeMenuManager
	{
		public MapTreeMenuManager(DocumentSupporter documentSupporter)
		{
			Argument.NotNull(documentSupporter, "DocumentSupporter documentSupporter");

			this.documentSupporter = documentSupporter;

			TopLevelMenuItem = new ZMenuItem(ResString.GetMultilingualString("MenuItem.DataSourceMaps", "Common Data Source Maps"));
			TopLevelMenuItem.Select += new EventHandler(TopLevelMenuItem_Select);
		}

		void TopLevelMenuItem_Select(object sender, EventArgs e)
		{
			LoadMenuItemsIfNotAlreadyLoaded();
		}

		void LoadMenuItemsIfNotAlreadyLoaded()
		{
			if (!menuItemsLoaded)
			{
				var mapTypes = new DataContextMapList(documentSupporter);
				foreach (var mapType in mapTypes)
				{
					var menuItem = TopLevelMenuItem.MenuItems.Add(mapType.DataContextIdentifier, new EventHandler(MenuItem_Click));
					menuItem.Tag = mapType;
				}

				menuItemsLoaded = true;
			}
		}

		bool menuItemsLoaded;

		void MenuItem_Click(object sender, EventArgs e)
		{
			var menuItem = sender as MenuItem;
			var mapType = menuItem?.Tag as DataContextMapList.MapElement;
			if (mapType != null)
			{
				ShowMapTreeForm(mapType);
			}
		}

		internal void ShowMapTreeForm(DataContextMapList.MapElement mapElement)
		{
			var reflector = new DocDataProviderReflector(mapElement);
			var dataProvider = new ValueProviderMap();
			var wrapper = new DataReflectorValueProviderWrapper(new[] { reflector }, dataProvider, DataReflectorValueProviderWrapper.Mode.Browse);
			var form = new MapTreeForm(wrapper, documentSupporter, mapElement.DataContextIdentifier);
			form.Show();
		}

		readonly DocumentSupporter documentSupporter;
		internal readonly MenuItem TopLevelMenuItem;
	}
}
