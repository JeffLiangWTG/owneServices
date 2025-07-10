using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.Environment.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	class RegistryImportBusinessObject : NonPersistentBusinessObject
	{
		internal readonly IFileDialog openFileDialog;

		readonly IRegistryItemSaveHandler saveHandler;
		readonly IEnumerable<IRegistryItem> registryItems;

		public RegistryImportBusinessObject(IEnumerable<IRegistryItem> registryItems, IFileDialog openFileDialog, IRegistryItemSaveHandler saveHandler)
		{
			Argument.NotNull(openFileDialog, nameof(openFileDialog));
			Argument.NotNull(saveHandler, nameof(saveHandler));
			Argument.NotNull(registryItems, nameof(registryItems));

			openFileDialog.Filter = CommonFileDialogFilters.XML;
			this.openFileDialog = openFileDialog;

			this.registryItems = registryItems;
			this.saveHandler = saveHandler;
		}

		public void OpenBrowseForm(IWin32Window owner)
		{
			ZFormModaliser.ShowCommonDialogWithoutDispose(openFileDialog, owner);
			loadedRegistryItems = null;

			RefreshBinding();
		}

		public ZString FilePath
		{
			get { return openFileDialog.UnmappedFileName; }
#if DEBUG
			set
			{
				openFileDialog.FileName = value;
				loadedRegistryItems = null;
			}
#endif
		}

		IOverrideLevel overrideLevel;
		public IOverrideLevel OverrideLevel
		{
			get { return overrideLevel; }
			set
			{
				overrideLevel = value;
				loadedRegistryItems = null;
			}
		}

		LoadedRegistryItems loadedRegistryItems;
		public LoadedRegistryItems LoadedRegistryItems
		{
			get
			{
#if DEBUG
				if (exceptionToThrowWhenLoadingItemsForTest != null)
				{ throw exceptionToThrowWhenLoadingItemsForTest; }
#endif

				if (loadedRegistryItems == null)
				{
					using (var stream = openFileDialog.OpenFile())
					{
						loadedRegistryItems = saveHandler.LoadItems(registryItems, stream);
					}
				}

				return loadedRegistryItems;
			}
		}

		public RegistryComparisonBusinessObject Comparison
		{
			get { return LoadedRegistryItems.AsComparison(OverrideLevel); }
		}

#if DEBUG
		internal Exception exceptionToThrowWhenLoadingItemsForTest;
#endif
	}
}
