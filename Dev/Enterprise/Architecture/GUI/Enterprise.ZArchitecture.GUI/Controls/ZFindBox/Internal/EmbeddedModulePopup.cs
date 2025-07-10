using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Modules;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	/// <summary>
	/// The implementation of FindBox popup forms throughout the system.
	/// </summary>
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	public partial class EmbeddedModulePopup : ZChildForm, IFindBoxPopup, IMainForm
	{
		public EmbeddedModulePopup()
		{
			InitializeComponent();
			IsSelectionMandatory = true;
		}

		public EmbeddedModulePopup(ZFilterModule module, StmModuleFilter filterLayoutToSelect = null, bool shouldLoadLayoutEvenWhenUnsaved = false, string okButtonCaption = null)
			: this()
		{
			Module = module;

			module.SetFormsModalTo(this);
			EmbeddedModulePopupOKButtonStrategy = module.ModuleDecisionProvider;

			if (module.ToolBarButtons != null && module.ToolBarButtons.Length > 0)
			{
				for (var i = Toolstrip.Items.Count - 1; i >= 0; i--)
				{
					Toolstrip.Items[i].Dispose();
				}
				Toolstrip.Items.Clear();

				if (module.FormActionMenu != null)
				{
					foreach (var item in module.FormActionMenu)
					{
						var result = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(item, module);
						result.Font = new Font(OFont.NormalFontName, 9F);
						Toolstrip.Items.Add(result);
					}
				}
			}
			else
			{
				ToolBarPanel.Visible = false;
			}

			PrepareModuleControl(filterLayoutToSelect, shouldLoadLayoutEvenWhenUnsaved);
			SetupTabIndexes();
			FilterControlPanel.Controls.Add(module.EmbeddedControl);

			ToolBarPanel.BackColor = SystemDataRegistry.Instance.ColorTheme.ToolbarColor;

			this.GotFocus += EmbeddedModulePopup_GotFocus;
			this.LostFocus += (sender, e) => ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(this);
			if (okButtonCaption != null)
			{
				OK_Button.Text = okButtonCaption;
			}
		}

		public override string FormCaption => Module?.Description;

		void EmbeddedModulePopup_GotFocus(object sender, EventArgs e)
		{
			ZFormModaliser.SetApplicationActiveForm(this);
		}

#if DEBUG
		public ZFilterModule Module_ForTest
		{
			get { return Module; }
		}
#endif

		protected internal ZFilterModule Module { get; }

		public IEmbeddedModulePopupOKButtonStrategy EmbeddedModulePopupOKButtonStrategy { get; set; }

		public bool RequireAtLeastOneItemToBeSelected
		{
			get { return requireAtLeastOneItemToBeSelected; }
			set
			{
				requireAtLeastOneItemToBeSelected = value;

				OK_Button.Visible = requireAtLeastOneItemToBeSelected;

				Cancel_Button.Text = requireAtLeastOneItemToBeSelected
					? Res.GetString("EmbeddedModulePopup|f5505cdd-eed0-4099-93b4-5edf5e83bb54", "Cancel")
					: Res.GetString("EmbeddedModulePopup|3f72041f-f2e2-49fa-8146-009509af042a", "Close");
			}
		}

		bool requireAtLeastOneItemToBeSelected;

		#region IFindBoxPopup Members

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			SetupModuleForSearch(findBox);
			ZFormModaliser.Show(this, parentForm);
		}

		void SetupModuleForSearch(IFindBox findBox)
		{
			this.FindBox = findBox;

			if (findBox != null)
			{
				var customizableFindBoxPopup = findBox as ICustomizableFindBoxPopup;
				if (customizableFindBoxPopup != null && !string.IsNullOrEmpty(customizableFindBoxPopup.CodeForPopup) && !string.IsNullOrEmpty(customizableFindBoxPopup.PropertyNameForPopup))
				{
					Module.SetInitialCodeForSearch(customizableFindBoxPopup.CodeForPopup, customizableFindBoxPopup.PropertyNameForPopup);
				}
				else if (!string.IsNullOrEmpty(findBox.Code))
				{
					Module.SetInitialCodeForSearch(findBox.Code);
				}

				Module.AdditionalSetupForSearch(findBox.Code);
			}
		}

		public bool AutoSearchAndSelectIfOnlyOneRecord(IFindBox findBox)
		{
			SetupModuleForSearch(findBox);
			Module.AddAdditionalDisplayFilter = (query) => { query.MaximumRows = 2; };
#if !WINZOR
			SafeNativeMethods.ShowWindow(this, ShowWindowsOptions.SW_HIDE);
#endif
			ZDisplayGrid.GetParentFilterControl().FilterStripLoaded();
			Module.AddAdditionalDisplayFilter = null;
			return AutoSelectTheOnlyOne(findBox, this) == SilentSelectResult.FoundOne;
		}

		public void FocusFirstRecord()
		{
			Module.DisplayGrid.Focus();
			var listManager = Module.DisplayGrid.ListManager;
			if (listManager != null && listManager.Count > 0)
			{
				Module.DisplayGrid.Select(0);
			}
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup modulePopup)
		{
			FindBox = findBox;

			if (FindBox != null && !string.IsNullOrEmpty(findBox.Code))
			{
				Module.SetInitialCodeForSearch(findBox.Code);
				Module.ActivateNonEmptyFiltersInitializedFromCode(findBox.Code);
				Module.PerformSearch();

				return AutoSelectTheOnlyOne(findBox, modulePopup);
			}

			return SilentSelectResult.None;
		}

		SilentSelectResult AutoSelectTheOnlyOne(IFindBox findBox, EmbeddedModulePopup modulePopup)
		{
			if (findBox != null && !string.IsNullOrEmpty(findBox.Code))
			{
				if (Module.GridCollection.Count == 1)
				{
					var provider = Module.GetModuleDecisionProviderForFindBox(findBox);

					if (modulePopup != null && modulePopup.Module.CheckValidSelectionForFindBox(new BusinessObjectFactory(), (BusinessObject)Module.GridCollection[0]))
					{
						provider.SetFindBoxCodeDescription((BusinessObject)Module.GridCollection[0]);
						modulePopup.HandleSelection(Module.GridCollection.ToArray());
					}
					return SilentSelectResult.FoundOne;
				}
				else if (Module.GridCollection.Count == 0)
				{
					return SilentSelectResult.FoundNothing;
				}
				else if (Module.GridCollection.Count > 1)
				{
					return SilentSelectResult.MultipleResults;
				}
			}

			return SilentSelectResult.None;
		}

		protected IFindBox FindBox { get; private set; }

		public ZString FindBoxCode
		{
			get { return (FindBox != null) ? FindBox.Code : string.Empty; }
		}

		#endregion

		#region Selected Handlers

		public event SelectedEventHandler Selected;
		public delegate void SelectedEventHandler(object sender, SelectedEventArgs e);

		public class SelectedEventArgs : EventArgs
		{
			public SelectedEventArgs(BusinessObject[] selectedBusinessObjects)
			{
				this.SelectedBusinessObjects = selectedBusinessObjects;
			}

			public readonly BusinessObject[] SelectedBusinessObjects;
		}

		protected void OnSelected(BusinessObject[] selectedBizObjs)
		{
			if (Selected != null)
			{
				Selected(this, new SelectedEventArgs(selectedBizObjs));
			}
		}

		public event EventHandler<FiltersSelectedEventArgs> FiltersSelected;

		public class FiltersSelectedEventArgs : EventArgs
		{
			public FiltersSelectedEventArgs(FilterStripBusinessObject filters, ZFilterModule module)
			{
				Filters = filters;
				Module = module;
			}

			public FilterStripBusinessObject Filters { get; }
			public ZFilterModule Module { get; }
		}

		void OnFiltersSelected(FilterStripBusinessObject filters)
		{
			FiltersSelected?.Invoke(this, new FiltersSelectedEventArgs(filters, Module));
		}

		#endregion

		#region Selection

		protected internal virtual void HandleSelection(BusinessObject[] selectedBizObjs)
		{
			OnSelected(selectedBizObjs);
			Close();
		}

		protected internal virtual void HandleFilterSelection(FilterStripBusinessObject selectedFilters)
		{
			selectedFilters.RunPreSaveValidation();

			if (selectedFilters.HasErrors())
			{
				Globals.Message.ShowError(Res.GetString("d1ea80b5-1d53-4fa6-8190-a1901f68204c", "There are errors. Please correct these before continuing."), Res.GetString("5fb0352b-a7bb-4606-b430-3c46891f7f69", "Errors"));
			}
			else
			{
				OnFiltersSelected(selectedFilters);
				Close();
			}
		}

		void IFindBoxPopup.SelectRowByPK(ZGuid pK)
		{
			var grid = ZDisplayGrid;
			if (grid != null && grid.List != null)
			{
				if (grid.ListManager.IsBindingSuspended)
				{
					// We can't guarantee the contents of the grid are valid, so just don't select anything
					grid.ResetSelection();
				}
				else
				{
					for (var i = grid.List.Count - 1; i >= 0; i--) // from the end as the DataRefreshManager will add it to the end of the list.
					{
						if (((BusinessObject)grid.List[i]).PK == pK)
						{
							((IDataGridToolTipSuspender)grid).SuspendToolTip();
							grid.AllowFocus = false;

							try
							{
								grid.CurrentCell = new DataGridCell(i, 0);
								grid.Select(i);
							}
							finally
							{
								grid.AllowFocus = true;
								((IDataGridToolTipSuspender)grid).ResumeToolTip();
							}

							break;
						}
					}
				}
			}
		}

		#endregion

		#region Implementation

		protected void PrepareModuleControl(StmModuleFilter filterLayoutToSelect, bool shouldLoadLayoutEvenWhenUnsaved)
		{
			var clientHeightDifference = Size.Height - ClientSize.Height;
			var originalMinimumWidth = MinimumSize.Width;
			var originalMinimumHeight = MinimumSize.Height;

			MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(MinimumSize.Width, Module.EmbeddedControl.Height + clientHeightDifference);
			if ((ClientSize.Height - ButtonPanel.Height) < Module.EmbeddedControl.Height)
			{
				ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(MinimumSize.Width, Module.EmbeddedControl.Height + ButtonPanel.Height);
			}

			MinimumSize = new Size(originalMinimumWidth, originalMinimumHeight);

			Module.EmbeddedControl.Dock = DockStyle.Fill;

			if (Module.EmbeddedControl is ZFilterStripBaseControl filterControl)
			{
				filterControl.LayoutToLoad = filterLayoutToSelect;
				filterControl.ShouldLoadLayoutEvenWhenUnsaved = shouldLoadLayoutEvenWhenUnsaved;
				filterControl.ShouldLoadStripsWhenNoLayoutsLoaded = filterLayoutToSelect != null;
			}

			if (Module.EmbeddedControl is StripControl stripControl)
			{
				stripControl.RecentItemsPanel.AllowOverlap(FilterControlPanel);
				stripControl.RecentItemsPanel.AllowOverlap(ToolBarPanel);
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Close();
				return true;
			}
			else
			{
				return base.ProcessCmdKey(ref msg, keyData);
			}
		}

		void SetupTabIndexes()
		{
			foreach (Control control in Controls)
			{
				control.TabIndex++;
			}
			FilterControlPanel.TabIndex = 0;
		}

		void OK_Button_Click(object sender, EventArgs e)
		{
			OnOkButtonClicked();
		}

		protected virtual void OnOkButtonClicked()
		{
			var selectedBizObjs = GetSelectedBusinessObjects();

			if (selectedBizObjs != null && selectedBizObjs.Length > 0)
			{
				if (EmbeddedModulePopupOKButtonStrategy != null)
				{
					EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(selectedBizObjs);
				}
			}
			else if (IsSelectionMandatory)
			{
				Globals.Message.ShowError(Res.GetString("69560b52-b391-4fb1-a1af-3ab19ff2e8da", "Please select an item from the grid."));
			}
			else
			{
				Close();
			}
		}

		public bool IsSelectionMandatory { get; set; }

		protected virtual BusinessObject[] GetSelectedBusinessObjects()
		{
			BusinessObject[] result;
			if (ZDisplayGrid != null)
			{
				result = ZDisplayGrid.GetSelectedElements<BusinessObject>();
			}
			else
			{
				throw new InvalidOperationException();
			}

			return result;
		}

		ZDisplayGrid ZDisplayGrid
		{
			get { return Module.DisplayGrid as ZDisplayGrid; }
		}

		void Cancel_Button_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				this.GotFocus -= EmbeddedModulePopup_GotFocus;
				ZFormModaliser.RemoveAsApplicationActiveFormIfStillActive(this);

				if (Module != null)
				{
					Module.Dispose();
				}
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Test Only

#if DEBUG
		public Button ExposedOKButtonForTesting => OK_Button;

		public Button CancelButtonForTest => Cancel_Button;

		public ToolStripItem FindToolBarButtonByText(string caption)
		{
			foreach (ToolStripItem toolBarButton in Toolstrip.Items)
			{
				if (KMenuItem.StripAcceleratorKeys(toolBarButton.Text) == KMenuItem.StripAcceleratorKeys(caption))
				{
					return toolBarButton;
				}
			}

			return null;
		}

		public ZPanel FilterControlPanel_ForTest
		{
			get { return FilterControlPanel; }
		}
#endif

		#endregion

		#region IMainForm Members

		public void UpdateToolBarDeleteButton(ZEmbeddedModule embeddedModule)
		{
			foreach (ToolStripItem toolBarButton in Toolstrip.Items)
			{
				if (KMenuItem.StripAcceleratorKeys(toolBarButton.Text) == KMenuItem.StripAcceleratorKeys(ZFilterGridModule.DeleteButtonCaptions.Delete) ||
					KMenuItem.StripAcceleratorKeys(toolBarButton.Text) == KMenuItem.StripAcceleratorKeys(ZFilterGridModule.DeleteButtonCaptions.Activate) ||
					KMenuItem.StripAcceleratorKeys(toolBarButton.Text) == KMenuItem.StripAcceleratorKeys(ZFilterGridModule.DeleteButtonCaptions.Deactivate))
				{
					toolBarButton.Text = (embeddedModule as ZFilterGridModule).DeleteButtonText;
					break;
				}
			}
		}

		public INamedModule CurrentModule
		{
			get { return Module; }
		}

		public string CurrentModuleLicenceCheckPointName
		{
			get { return Module != null && Module.LicenceCheckPoint != null ? Module.LicenceCheckPoint.Name : string.Empty; }
		}

		#endregion
	}
}

