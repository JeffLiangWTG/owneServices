using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	internal class ThumbNailSizeContextMenu : ContextMenu
	{
		public ThumbNailSizeContextMenu()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			numberOfThumbnailsPerRow = ThumbnailSettings.NumberOfThumbnails;
			CreateMenu();
		}

		public int NumberOfThumbnailsPerRow
		{
			get { return numberOfThumbnailsPerRow; }
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				try
				{
					SaveSettings();
				}
				catch (SqlException)
				{
					// To avoid leak, the dispose process should be continued
				}
			}

			base.Dispose(isNotFinalizing);
		}

		#region Implementation

		void CreateMenu()
		{
			menuItemToNumberOfThumbnailsMapping.Clear();
			thumbnailSubMenu = AddMenuItem(this, ResString.GetMultilingualString("ThumbNails|ImagesAcrossPage", "No. Images across page"));

			AddMenuItem(thumbnailSubMenu, 0, ResString.GetMultilingualString("ThumbNails|AutoFit", "Auto Fit"));
			AddSeparator(thumbnailSubMenu);

			for (int index = 1; index <= 5; index++)
			{
				AddMenuItem(thumbnailSubMenu, index, ResString.GetMultilingualString("ThumbNails|Across", "{0} across", index));
			}
		}

		protected virtual void SaveSettings()
		{
			var registrySettings = ThumbnailSettings;
			registrySettings.NumberOfThumbnails = numberOfThumbnailsPerRow;
			ThumbnailSettings = registrySettings;
		}

		MenuItem AddMenuItem(Menu target, int index, MultilingualString caption)
		{
			MenuItem newItem = AddMenuItem(target, caption);
			newItem.Click += new EventHandler(OnMenuItemClicked);

			menuItemToNumberOfThumbnailsMapping[newItem] = index;
			if (numberOfThumbnailsPerRow == index)
			{
				newItem.Checked = true;
			}

			return newItem;
		}

		MenuItem AddMenuItem(Menu target, MultilingualString caption)
		{
			MenuItem newItem = new ZMenuItem(caption);
			target.MenuItems.Add(newItem);
			return newItem;
		}

		void AddSeparator(Menu target)
		{
			// don't add another separator if the last item was a separator
			if (target.MenuItems[target.MenuItems.Count - 1].Text != Constants.SeparatorMenuText)
			{
				AddMenuItem(target, (NoResString)Constants.SeparatorMenuText);
			}
		}

		protected void OnMenuItemClicked(object sender, EventArgs e)
		{
			MenuItem selectedMenuItem = (MenuItem)sender;
			CheckOneOnly(selectedMenuItem);
			numberOfThumbnailsPerRow = menuItemToNumberOfThumbnailsMapping[selectedMenuItem];

			if (MenuItemClicked != null)
			{
				MenuItemClicked(sender, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Ensures only the specified item is ticked in the context menu
		/// </summary>
		void CheckOneOnly(MenuItem itemToCheck)
		{
			if (thumbnailSubMenu != null)
			{
				foreach (MenuItem curItem in thumbnailSubMenu.MenuItems)
				{
					curItem.Checked = (itemToCheck == curItem);
				}
			}
		}

		#endregion

		DMThumbnailSettingsStruct ThumbnailSettings
		{
			get
			{
				if (!DesignModeFinder.IsDesigning)
				{
					return Env.Registry.DMThumbnailSettings;
				}

				return new DMThumbnailSettingsStruct { NumberOfThumbnails = 5, ThumbNailViewActive = false };
			}
			set
			{
				if (!DesignModeFinder.IsDesigning)
				{
					var settings = Env.Registry.DMThumbnailSettings;
					if (settings.ThumbNailViewActive != value.ThumbNailViewActive || settings.NumberOfThumbnails != value.NumberOfThumbnails)
					{
						Env.Registry.DMThumbnailSettings = value;
					}
				}
			}
		}

		readonly Dictionary<MenuItem, int> menuItemToNumberOfThumbnailsMapping = new Dictionary<MenuItem, int>();
		int numberOfThumbnailsPerRow;
		MenuItem thumbnailSubMenu;

		public event EventHandler MenuItemClicked;
	}
}
