using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.GUI
{
	public class GridUniversalCopyManager : UniversalCopyManager, IGridUniversalCopyManager
	{
		public GridUniversalCopyManager(ZGrid grid)
			: base(GetElementType(grid), GetGridModuleIdentifier(grid), GetRunOnMainThreadValue(grid))
		{
			Grid = grid;
			grid.Disposed += GridDisposed;
		}

		static Type GetElementType(ZGrid grid)
		{
			if (grid is ZFilterGrid filterGrid)
			{
				return filterGrid.ParentModule?.GetType().GetCustomAttribute<UniversalCopyInstanceTypeAttribute>()?.InstanceType ??
							grid.ElementType;
			}

			return grid.ElementType;
		}

		internal ZGrid Grid { get; private set; }

		ZFilterModule FilterModule
		{
			get { return LocalModule as ZFilterModule; }
		}

		static ModuleIdentifier GetGridModuleIdentifier(ZGrid grid)
		{
			var filterGrid = grid as ZFilterGrid;
			if (filterGrid != null && filterGrid.ModuleID != ModuleIDs.NotAssigned)
			{
				return filterGrid.ModuleID;
			}

			if (grid.List != null)
			{
				var moduleIdAttribute = grid.List.GetType().GetCustomAttributes(typeof(ModuleIDAttribute), true).Cast<ModuleIDAttribute>().FirstOrDefault();
				return moduleIdAttribute != null ? moduleIdAttribute.ModuleIdentifier : null;
			}

			return null;
		}

		static bool GetRunOnMainThreadValue(ZGrid grid)
		{
			if (grid is ZFilterGrid filterGrid)
			{
				if (filterGrid.ParentModule is ZFilterModule filterModule)
				{
					return filterModule.ShowFormsFromMainThread;
				}
			}

			return false;
		}

		#region Security

		public override bool AllowsUniversalCopy
		{
			get
			{
				var result = base.AllowsUniversalCopy;

				if (result)
				{
					var parentModule = GetParentModule();

					try
					{
						result = parentModule.Module != null
						? parentModule.Module.AllowNew && parentModule.Module.AllowUniversalCopy &&
						   // For example, MenusGrid in DocumentCustomisationForm and ReportGrid, their ParentModule are both StmMenuItemModule.
						   // ReportGrid is readonly, it should not AllowsUniversalCopy. MenusGrid is not readonly, it should AllowsUniversalCopy
						   (!Grid.ReadOnly || FilterModule.GetNewController() != null)
						: Grid.List.AllowNew && Grid.List.AllowEdit && !Grid.ReadOnly;
					}
					catch
					{
						result = false;
					}

					if (parentModule.Dispose)
					{
						parentModule.Module?.Dispose();
					}
				}

				return result;
			}
		}

		protected (ZModule Module, bool Dispose) GetParentModule()
		{
			ZModule module = null;
			var dispose = false;

			if (Grid is ZFilterGrid filterGrid && filterGrid.ParentModule != null)
			{
				module = filterGrid.ParentModule;
			}
			else
			{
				if (Grid.List != null)
				{
					var attributes = (ModuleIDAttribute[])Grid.List.GetType().GetCustomAttributes(typeof(ModuleIDAttribute), true);
					var attribute = attributes.Length > 0 ? attributes[0] : null;

					if (attribute != null && attribute.ModuleIdentifier != ModuleIDs.NotAssigned && !attribute.UniversalCopyMenuIgnoreModuleId)
					{
						module = ZModuleFactory.Instance.CreateNew(attribute.ModuleIdentifier) as ZModule;
						dispose = true;
					}
				}
			}
			return (module, dispose);
		}

		internal protected override ZModule LocalModule
		{
			get
			{
				var filterGrid = Grid as ZFilterGrid;
				if (filterGrid != null)
				{
					return filterGrid.ParentModule; // Do not store it in local field to prevent its Disposal
				}

				return base.LocalModule;
			}
		}

		protected override ZModule GetNewLocalModule()
		{
			ZModule localModule = base.GetNewLocalModule();

			if (localModule == null)
			{
				var form = Grid.FindForm() as ZForm;
				if (form != null && form.ControllerID != null)
				{
					var controller = ZControllerFactory.Instance.CreateNew(form.ControllerID) as ZController;
					if (controller != null && controller.ModuleID != null && controller.ModuleID != ModuleIDs.NotAssigned)
					{
						localModule = ZModuleFactory.Instance.Create(controller.ModuleID);
					}
				}
			}

			return localModule;
		}

		#endregion

		#region Menus

		#region Populate menu

		ContextMenu popupCopyMenu;

		public void AddMenuItems()
		{
			if (AllowsUniversalCopy)
			{
				gridUniversalMenuItem = AddNewGridUniversalMenuItem(Grid);
				var includeCopySchedulesItem = Grid.DataSource?.GetType() != typeof(ProcessTaskTemplate);
				AddMenuItems(gridUniversalMenuItem, includeEditMenuItems: true, includeCopySchedulesItem);
			}
		}

		protected ZMenuItem gridUniversalMenuItem;

		static ZMenuItem AddNewGridUniversalMenuItem(ZGrid grid)
		{
			var universalCopyMenuItem = new OwnerDrawMenuItem(ResString.GetMultilingualString("d525592d-2ca0-46fe-98b0-64612cfb0791", "Universal Copy"))
			{
				DrawShortcutFromTag = true,
				Tag = TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString((Keys)(int)Shortcut.CtrlShiftC),
				Name = "UniversalCopy"
			};

			int menuIndex = GetMenuItemIndex(grid.ContextMenu.MenuItems, grid.CopyMenuItem, 1);
			if (menuIndex < 0)
			{
				menuIndex = GetMenuItemIndex(grid.ContextMenu.MenuItems, grid.DeleteMenuItemModule);
			}
			if (menuIndex < 0)
			{
				menuIndex = GetMenuItemIndex(grid.ContextMenu.MenuItems, grid.DeleteMenuItem);
			}
			if (menuIndex < 0)
			{
				menuIndex = 0;
			}
			grid.ContextMenu.MenuItems.Add(menuIndex, universalCopyMenuItem);

			return universalCopyMenuItem;
		}

		static int GetMenuItemIndex(Menu.MenuItemCollection collection, MenuItem menuItem, int shift = 0)
		{
			int menuIndex = menuItem != null && menuItem.Visible ? collection.IndexOf(menuItem) : -1;
			if (menuIndex >= 0)
			{
				menuIndex += shift;
			}
			return menuIndex;
		}

		protected override void AddMenuItemsCore(bool includeEditMenuItems, bool includeCopySchedulesItem, bool lazyPopulateMenuItems)
		{
			base.AddMenuItemsCore(includeEditMenuItems, includeCopySchedulesItem, lazyPopulateMenuItems);
			AddKeyHandlerPopupCopyMenuItem();
		}

		void AddKeyHandlerPopupCopyMenuItem()
		{
			if (popupCopyMenu == null)
			{
				popupCopyMenu = new ContextMenu();

				var hiddenUniversalCopyMenuItem = new ZMenuItem((NoResString)"Popup Universal Copy", PopupUniversalCopyClick)
				{
					Shortcut = Shortcut.CtrlShiftC,
					Visible = false
				};
				Grid.ContextMenu.MenuItems.Add(gridUniversalMenuItem != null ? gridUniversalMenuItem.Index + 1 : 0, hiddenUniversalCopyMenuItem);

#if DEBUG
				TypeDescriptor.AddAttributes(hiddenUniversalCopyMenuItem, new SuppressFormsLocalizedTestAttribute());
#endif
			}
		}

		void PopupUniversalCopyClick(object sender, EventArgs e)
		{
			if (LocalModule?.AllowUniversalCopy ?? false)
			{
				if (!IsCopyMenuItemPopulated)
				{
					PopulateCopyMenuItems();
				}

				if (popupCopyMenu.MenuItems.Count == 0)
				{
					Globals.Message.ShowInformation(Res.GetString("70fa1a98-b835-48e4-a85b-c3800eee713f", "There are no copy templates to use."));
					return;
				}

				Point point;
				int currentCellColumnNumber;
				if (Grid.DataGridRowsLength > 0 && (currentCellColumnNumber = Grid.CurrentCell.ColumnNumber) >= 0 && Grid.CurrentCell.RowNumber >= 0)
				{
					var currentCellBounds = Grid.GetCurrentCellBounds();

					int x = currentCellColumnNumber >= Grid.FirstVisibleColumn && currentCellColumnNumber < (Grid.FirstVisibleColumn + Grid.VisibleColumnCount)
						? currentCellBounds.Left
						: Grid.Width / 2;

					point = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(x, currentCellBounds.Bottom, false);
				}
				else
				{
					point = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(Grid.Width / 2, Grid.Height / 2, false);
				}

				popupCopyMenu.Show(Grid, point);
			}
		}

		protected override void PopulateCopyMenuItemsCore()
		{
			base.PopulateCopyMenuItemsCore();
			AddKeyHandlerPopupCopyMenuItem();
			PopulateCopyMenuItems(popupCopyMenu.MenuItems);
		}

		#endregion

		#region Menu Handlers

		#region Copy

		protected override bool CopyMenuClicked_TryGetCopyTargets(out IEnumerable<BusinessObject> copyTargets)
		{
			if (Grid.SelectedRowCount == 0 && (Grid.ListManager == null || Grid.ListManager.Position < 0 || Grid.ListManager.GetCurrent() == null))
			{
				Globals.Message.ShowInformation(Res.GetString("e4082c63-feee-42b8-93ae-6b49496c0475", "Please select one or more rows to copy."));
				copyTargets = null;
				return false;
			}

			copyTargets = Grid.SelectedRowCount > 0 ? Grid.SelectedElements : new[] { Grid.ListManager.GetCurrent() as BusinessObject };
			return true;
		}

		protected override bool CopyMenuClicked_CheckCanCopy()
		{
			if (Grid.ReadOnly && FilterModule == null)
			{
				Globals.Message.ShowError(Res.GetString("730a0d55-ebb6-496e-9f03-9d70b92d5001", "Cannot copy elements because this grid is not editable and system cannot determine type of edit form to use for copied elements."));
				return false;
			}

			return true;
		}

		protected override BusinessObject CopyMenuClicked_GetSourceElement(BusinessObject selectedElement)
		{
			if (Grid.ReadOnly)
			{
				var controller = FilterModule.GetNewController(selectedElement);
				var elementType =
					selectedElement.GetType().GetCustomAttribute<UniversalCopyInstanceTypeAttribute>()?.InstanceType ??
					(controller != null ? controller.TypeOfTopLevelBusinessObject : selectedElement.GetType());
				var newFactory = controller != null ? controller.Factory : selectedElement.Factory.CreateNewFactory();
				return ImportIntoAnotherFactory(selectedElement, elementType, newFactory);
			}
			else
			{
				return base.CopyMenuClicked_GetSourceElement(selectedElement);
			}
		}

		protected override void CopyMenuClicked_OnNewElement(BusinessObject newElement)
		{
			if (newElement != null)
			{
				if (Grid.ReadOnly)
				{
					var formShowingArgs = new FormShowingForElementArgs(newElement);
					OnFormShowingForNewElement(formShowingArgs);
					if (!formShowingArgs.Cancelled)
					{
						ZController controller = FilterModule.GetNewController(newElement);
						controller.ShowFormForNewEntity(newElement);
					}
				}
				else
				{
					using (BusinessObjectUniversalCopyFactoryService.EnsureServiceIsSetUp(newElement.Factory))
					{
						Grid.List.Add(newElement);
					}
				}
			}
			else if (Grid.SelectedRowCount == 1)
			{
				ShowElementWasNotCopiedInformation();
			}
		}

		protected override BusinessObjectFactory GetDefaultFactoryForCopyManager()
		{
			if (!Grid.ReadOnly)
			{
				return null;
			}

			return base.GetDefaultFactoryForCopyManager();
		}

		#endregion

		#region Schedule

		protected override bool SchedulesMenuSelect_TryGetScheduleTarget(out BusinessObject scheduleTarget)
		{
			if (Grid.SelectedElements.Length == 1)
			{
				scheduleTarget = Grid.SelectedElements[0];
				return true;
			}

			scheduleTarget = null;
			return false;
		}

		protected override bool CreateScheduleMenuClicked_TryGetScheduleTarget(out BusinessObject scheduleTarget)
		{
			if (Grid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowInformation(Res.GetString("bf8d1d54-b118-444b-b320-466c72d96e96", "Select exactly one element to create a copy schedule"));
			}
			else
			{
				var copyObject = Grid.SelectedElements[0];
				if (!copyObject.IsInDatabase)
				{
					Globals.Message.ShowInformation(Res.GetString("520ffd8f-9be2-4009-a8d4-53e1f71841a2", "Please save the element before creating a copy schedule"));
				}
				else
				{
					scheduleTarget = copyObject;
					return true;
				}
			}

			scheduleTarget = null;
			return false;
		}

		#endregion

		#endregion

		#endregion

		#region Disposing

		void GridDisposed(object sender, EventArgs e)
		{
			Dispose();
			Grid.Disposed -= GridDisposed;
		}

		#endregion
	}
}
