using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.GUI.ReflectiveFieldMap;
using Enterprise.DocumentEngine.MacroEvaluator;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	partial class DocumentCustomisationForm : MenuCustomisationForm
	{
		public DocumentCustomisationForm(DocumentMenuCustomisation businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();

			RegisterEventHandlers();

			this.Saved += DocumentCustomisationForm_Saved;

			childMenusUsedGrid.AfterBind += delegate
			{ AddGridTrackingEvents(childMenusUsedGrid, delegate { SelectChildMenuBasedOnPivot(); }); };
			docTypesUsedGrid.AfterBind += delegate
			{ AddGridTrackingEvents(docTypesUsedGrid, delegate { SelectDocTypeBasedOnPivot(); }); };
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			PivotAndChildMenuTabControl.SelectedTab = menuTemplatesTabPage;

			docTypesDescriptionLabel.Text = Res.GetString("DocumentCustomizationForm|docTypesDescriptionLabel", "Any eDoc types used in this document pack will include eDocs that are attached to this form only. If you would like to add eDocs from another form (e.g. Shipment eDocs from a Consol form), you must create another menu on that level (e.g. Shipment) with the eDocs to include and add it as a menu on 'Other Documents Used for Selected Menu'.");

			int indexToAddAt = MainMenu.MenuItems.IndexOf(HelpMenuItem);
			menuManager = new MapTreeMenuManager(businessEntity.Parent.DocumentSupporter);
			MainMenu.MenuItems.Add(indexToAddAt, menuManager.TopLevelMenuItem);

			docConfigsGrid.DoubleClick += new EventHandler(EditDocConfigButton_Click);

			MenusGrid.ContextMenu.MenuItems.Add(Res.GetString("DocumentMenuCustomisationForm|d9011c5e-83d5-4d4b-9755-0c87ce8a67b2", "Evaluate Filter"), new EventHandler(ShowFilterEvaluator));
			MenusGrid.ContextMenu.MenuItems.Add(Res.GetString("DocumentMenuCustomisationForm|ed952e6f-a9ed-4af8-9575-a9e10b73a144", "Evaluate Macro"), new EventHandler(ShowMacroEvaluator));
			HandleDocumentDataSourceMapsMenus();
		}

		#region Child Menu and Doc Type Drag & Drop

		internal bool isAvailableChildMenusDragging;
		internal bool isChildMenusUsedGridDragging;
		internal bool isAvailableDocTypesGridDragging;
		internal bool isDocTypesUsedGridDragging;

		void RegisterEventHandlers()
		{
			availableChildMenusGrid.AllowDrop = true;
			availableChildMenusGrid.MouseDown += new MouseEventHandler(availableChildMenusGrid_MouseDown);
			availableChildMenusGrid.MouseUp += new MouseEventHandler(availableChildMenusGrid_MouseUp);

			childMenusUsedGrid.AllowDrop = true;
			childMenusUsedGrid.MouseDown += new MouseEventHandler(childMenusUsedGrid_MouseDown);
			childMenusUsedGrid.MouseUp += new MouseEventHandler(childMenusUsedGrid_MouseUp);
			childMenusUsedGrid.DragDrop += new DragEventHandler(childMenusUsedGrid_DragDrop);
			childMenusUsedGrid.DragEnter += new DragEventHandler(childMenusUsedGrid_DragEnter);

			availableDocTypesGrid.AllowDrop = true;
			availableDocTypesGrid.MouseDown += new MouseEventHandler(availableDocTypesGrid_MouseDown);
			availableDocTypesGrid.MouseUp += new MouseEventHandler(availableDocTypesGrid_MouseUp);

			docTypesUsedGrid.AllowDrop = true;
			docTypesUsedGrid.MouseDown += new MouseEventHandler(docTypesUsedGrid_MouseDown);
			docTypesUsedGrid.MouseUp += new MouseEventHandler(docTypesUsedGrid_MouseUp);
			docTypesUsedGrid.DragDrop += new DragEventHandler(docTypesUsedGrid_DragDrop);
			docTypesUsedGrid.DragEnter += new DragEventHandler(docTypesUsedGrid_DragEnter);

			DeliveryRestrictionDropEdit.LastSelectedItemChanged += DeliveryRestrictionDropEdit_LastSelectedItemChanged;
		}

		#region availableChildMenusGrid EventHandler

		internal void availableChildMenusGrid_MouseUp(object sender, MouseEventArgs e)
		{
			isAvailableChildMenusDragging = false;
		}

		internal void availableChildMenusGrid_MouseDown(object sender, MouseEventArgs e)
		{
			isAvailableChildMenusDragging = true;
		}

		#endregion

		#region childMenusUsedGrid EventHandler

		internal void childMenusUsedGrid_MouseUp(object sender, MouseEventArgs e)
		{
			isChildMenusUsedGridDragging = false;
		}

		internal void childMenusUsedGrid_MouseDown(object sender, MouseEventArgs e)
		{
			isChildMenusUsedGridDragging = true;
		}

		internal void childMenusUsedGrid_DragDrop(object sender, DragEventArgs e)
		{
			if (isAvailableChildMenusDragging && !isChildMenusUsedGridDragging)
			{
				AddChildMenuPivot();
			}
			isChildMenusUsedGridDragging = false;
		}

		internal void childMenusUsedGrid_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
		}

		#endregion

		#region availableDocTypesGrid EventHandler

		internal void availableDocTypesGrid_MouseUp(object sender, MouseEventArgs e)
		{
			isAvailableDocTypesGridDragging = false;
		}

		internal void availableDocTypesGrid_MouseDown(object sender, MouseEventArgs e)
		{
			isAvailableDocTypesGridDragging = true;
		}

		#endregion

		#region docTypesUsedGrid EventHandler

		internal void docTypesUsedGrid_MouseUp(object sender, MouseEventArgs e)
		{
			isDocTypesUsedGridDragging = false;
		}

		internal void docTypesUsedGrid_MouseDown(object sender, MouseEventArgs e)
		{
			isChildMenusUsedGridDragging = true;
		}

		internal void docTypesUsedGrid_DragDrop(object sender, DragEventArgs e)
		{
			if (isAvailableDocTypesGridDragging && !isDocTypesUsedGridDragging)
			{
				AddDocTypePivot();
			}
			isDocTypesUsedGridDragging = false;
		}

		internal void docTypesUsedGrid_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
		}

		#endregion

		#region DeliveryRestrictionDropEdit EventHandler

		void DeliveryRestrictionDropEdit_LastSelectedItemChanged(object sender, EventArgs e)
		{
			if (CurrentMenu != null)
			{
				CurrentMenu.DeactivateDeliveryRestrictionsCancelEventHandler -= CurrentMenu_DeactivateDeliveryRestrictionsCancelEventHandler;
				CurrentMenu.DeactivateDeliveryRestrictionsCancelEventHandler += CurrentMenu_DeactivateDeliveryRestrictionsCancelEventHandler;
			}
		}

		void CurrentMenu_DeactivateDeliveryRestrictionsCancelEventHandler(object sender, CancelEventArgs e)
		{
			var message = ResString.GetMultilingualString("A42F2880-6D5C-4285-A81E-44729B5D79B2", @"The Delivery Restrictions tab is not available when the CNH - Movement Restricted/Credit on Hold restriction has been set on the Details tab.

Are you sure you want to continue?");
			var caption = ResString.GetMultilingualString("08F441A5-2A2C-47AE-B6BB-CAAA3300074A", "Confirm Delivery Restriction Type Change");
			e.Cancel = Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Question, DialogResult.OK) == DialogResult.Cancel;
		}

		#endregion

		#endregion

		readonly MapTreeMenuManager menuManager;
		internal MenuItem DocumentDataSourceMapsMenuItem;
		void HandleDocumentDataSourceMapsMenus()
		{
			DocumentDataSourceMapsMenuItem = new ZMenuItem(ResString.GetMultilingualString("6A281D2A-8C77-42DC-A35D-49E5843B4E3E", "Document Data Source Maps"));
			DocumentDataSourceMapsMenuItem.Select += DocumentDataSourceMapsMenuItemSelect;
			MenusGrid.ContextMenu.MenuItems.Add(DocumentDataSourceMapsMenuItem);
			AddDefaultEmptyDocumentDataSourceMapMenuItem();
		}

		void AddDefaultEmptyDocumentDataSourceMapMenuItem()
		{
			DocumentDataSourceMapsMenuItem.MenuItems.Clear();
			DocumentDataSourceMapsMenuItem.MenuItems.Add(ResString.GetMultilingualString("182E6161-97D0-4C3F-A88E-92720CDE6330", "No Data Source Map found")).Enabled = false;
		}

		readonly HashSet<ZString> previousDataContexts = new HashSet<ZString>();
		readonly HashSet<ZString> currentDataContexts = new HashSet<ZString>();
		IStmMenuItem previousMenuItem;
		bool shouldGetWrappers;
		void DocumentDataSourceMapsMenuItemSelect(object sender, EventArgs e)
		{
			if (CurrentTemplate == null)
			{
				return;
			}

			CombineAllSelectedDataContextsTo(currentDataContexts);
			if (currentDataContexts.SetEquals(previousDataContexts) && CurrentMenu == previousMenuItem && !shouldGetWrappers)
			{
				return;
			}

			CombineAllSelectedDataContextsTo(previousDataContexts);
			previousMenuItem = CurrentMenu;

			DocumentDataSourceMapsMenuItem.MenuItems.Clear();

			var wrappers = new List<(ZString dataContextIdentifier, ZString dataContextOnly, DocumentWrapper wrapper)>();
			wrappers.AddRange(GetWrapperInfo(previousDataContexts).Distinct());

			if (!wrappers.Any())
			{
				shouldGetWrappers = true;
				AddDefaultEmptyDocumentDataSourceMapMenuItem();
			}
			else
			{
				shouldGetWrappers = false;
				RenameDuplicateIdentifier(wrappers).ForEach(AddWrapperMenuItem);
			}

			void AddWrapperMenuItem((ZString dataContextIdentifier, ZString dataContextOnly, DocumentWrapper wrapper) wrapperTuple)
			{
				var mapElement = new DataContextMapList.MapElement(wrapperTuple.dataContextOnly, wrapperTuple.wrapper.GetType());
				var menuItem = DocumentDataSourceMapsMenuItem.MenuItems.Add(wrapperTuple.dataContextIdentifier, WrapperMenuItem_Click);
				menuItem.Tag = mapElement;

				void WrapperMenuItem_Click(object senderForClick, EventArgs eForClick)
				{
					var menuItemForClick = senderForClick as MenuItem;
					var mapElementForClick = menuItemForClick?.Tag as DataContextMapList.MapElement;
					if (mapElementForClick != null)
					{
						menuManager.ShowMapTreeForm(mapElementForClick);
					}
				}
			}
		}

		IEnumerable<(ZString dataContextIdentifier, ZString dataContextOnly, DocumentWrapper wrapper)> GetWrapperInfo(IEnumerable<ZString> dataContexts)
		{
			var wappers = new List<(ZString dataContextIdentifier, ZString dataContextOnly, DocumentWrapper wrapper)>();
			dataContexts.ForEach(dataContext =>
			{
				BusinessEntity.Parent.DocumentSupporter.GetDocumentWrappers(new DataContextValue(dataContext).DataContext, CurrentMenu).ForEach(
					wrapper =>
					{
						wappers.Add((dataContext + " | " + wrapper.GetType().Name, dataContext, wrapper));
					}
				);
			});
			return wappers;
		}

		internal List<(ZString dataContextIdentifier, ZString dataContextOnly, DocumentWrapper wrapper)> RenameDuplicateIdentifier(IEnumerable<(ZString dataContextIdentifier, ZString dataContextOnly, DocumentWrapper wrapper)> wrappers)
		{
			return wrappers.GroupBy(x => x.dataContextIdentifier)
				.SelectMany(g => g.Count() == 1
					? g.Take(1)
					: g.Select((x, i) => (dataContextIdentifier: new ZString(x.dataContextIdentifier + (i + 1)), dataContextOnly: x.dataContextOnly, wrapper: x.wrapper)))
				.ToList();
		}

		internal void CombineAllSelectedDataContextsTo(HashSet<ZString> dataContexts)
		{
			if (dataContexts == null)
			{
				dataContexts = new HashSet<ZString>();
			}
			dataContexts.Clear();
			dataContexts.Add(CurrentTemplate.SO_DataContext);
			CurrentMenu?.Documents?.OfType<StmMenuTemplatePivotBase>().ForEach(d =>
			{
				dataContexts.Add(d.Template.SO_DataContext);
				dataContexts.UnionWith(d.DocConfigs.OfType<StmMenuDocumentConfig>().Select(c => c.S3_OverrideDataContext)
					.Where(c => !string.IsNullOrEmpty(c)));
			});
		}

		void DocumentCustomisationForm_Saved(object sender, EventArgs e)
		{
			ObjectFactory.Get<ILicenceConsumptionLogCreator>().CreateLog(Env.Licence.DocumentCustomisation);
		}

		public new DocumentMenuCustomisation BusinessEntity
		{
			get { return (DocumentMenuCustomisation)base.BusinessEntity; }
		}

		protected override string MenuGridCaption
		{
			get { return Res.GetString("DocumentCustomizationForm|MenuGridCaption", "Document Menus"); }
		}

		#region Child Menus

		void AddChildMenuPivot()
		{
			if (CurrentChildMenus.Any())
			{
				var pivots = new List<StmMenuMenuPivotBase>();
				CurrentChildMenus.ForEach(menu =>
				{
					pivots.Add(BusinessEntity.AddChildMenuPivot(CurrentMenu, menu));
				});
				if (pivots.Any())
				{
					pivots.ForEach(pivot =>
					{
						childMenusUsedGrid.Select(childMenusUsedGrid.List.IndexOf(pivot));
					});
					childMenusUsedGrid.ListManager.Position = childMenusUsedGrid.List.IndexOf(childMenusUsedGrid.SelectedElements.First());
				}
			}
		}

		void AddChildMenuPivotButton_Click(object sender, EventArgs e)
		{
			AddChildMenuPivot();
		}

		void RemoveChildMenuPivotButton_Click(object sender, EventArgs e)
		{
			var isSelected = IsCurrentlySelectedElementsNotNull(CurrentChildMenuPivots);
			if (isSelected)
			{
				var canNotDeleteMenuNames = new ZStringBuilder();
				CurrentChildMenuPivots.Where(pivot => pivot != null).ForEach(pivot =>
				{
					if (!pivot.CanDelete)
					{
						canNotDeleteMenuNames.AppendLine(pivot.SF_Calc_ChildName);
					}
					else
					{
						pivot.Delete();
					}
				});
				var menuNames = canNotDeleteMenuNames.ToString();
				if (!string.IsNullOrEmpty(menuNames))
				{
					Globals.Message.ShowError(ResString.GetMultilingualString("FF894094-CABF-4DCE-A8F0-5D84A210CFE4", "You cannot remove the following system-defined document.\r\n{0}", menuNames), ResString.GetMultilingualString("6d9e76a1-6896-4d33-950e-e6141f891ab2", "Error"));
				}
			}
		}

		#endregion

		#region Doc Configs

		void DocumentCustomisationForm_Resize(object sender, EventArgs e)
		{
			ControlDpiScalingHelper.SetHeight(ref docConfigsGroupBox, (TemplatesUsedGroupBox.Height / 2) + (docConfigsPanel.Height / 2) - ControlDpiScalingHelper.ScaleToCurrentDpiY(4), false);
		}

		void CopyDocConfigButton_Click(object sender, EventArgs e)
		{
			StmMenuDocumentConfig copy = BusinessEntity.CopyDocConfig(CurrentDocConfig, CurrentTemplatePivot);
			docConfigsGrid.SelectSingleElement(copy);
			ShowDocConfigForm(CurrentDocConfig);
		}

		void EditDocConfigButton_Click(object sender, EventArgs e)
		{
			ShowDocConfigForm(CurrentDocConfig);
		}

		void NewDocConfigButton_Click(object sender, EventArgs e)
		{
			ShowDocConfigForm(CurrentTemplatePivot);
		}

		void RefreshDocConfigsVisibility()
		{
			StmMenuTemplatePivotBase pivot = CurrentTemplatePivot;
			bool docConfigsVisible = pivot != null && pivot.Template != null && pivot.Template.IsDocBuilderStyle;
			docConfigsGroupBox.Visible = docConfigsVisible;
			docConfigsSplitter.Visible = docConfigsVisible;
		}

		void ShowDocConfigForm(IStmMenuDocumentConfigSource docConfigSource)
		{
			if (docConfigSource != null)
			{
				TemporaryStmMenuDocumentConfig docConfig = TemporaryStmMenuDocumentConfig.New(docConfigSource);
				using (StmMenuDocumentConfigForm docConfigForm = new StmMenuDocumentConfigForm(docConfig, BusinessEntity.Menus.Parent))
				{
					ZFormModaliser.ShowDialogWithoutDispose(docConfigForm);
					StmMenuDocumentConfig committedDocConfig = docConfigForm.CommittedDocConfig;
					if (committedDocConfig != null)
					{
						CurrentTemplatePivot.DocConfigs.SortByFallback();
						docConfigsGrid.SelectSingleElement(committedDocConfig);
					}
				}
			}
		}

		#endregion

		#region Doc Types

		void AddDocTypePivot()
		{
			if (CurrentDocTypes.Any())
			{
				var pivots = new List<DocumentStmMenuEDocs>();
				CurrentDocTypes.ForEach(docType =>
				{
					pivots.Add(BusinessEntity.AddDocTypePivot(CurrentMenu, docType));
				});
				if (pivots.Any())
				{
					docTypesUsedGrid.UnSelectAll();
					pivots.ForEach(pivot =>
					{
						docTypesUsedGrid.Select(docTypesUsedGrid.List.IndexOf(pivot));
					});
					docTypesUsedGrid.ListManager.Position = docTypesUsedGrid.List.IndexOf(docTypesUsedGrid.SelectedElements.First());
				}
			}
		}

		void AddDocTypePivotButton_Click(object sender, EventArgs e)
		{
			AddDocTypePivot();
		}

		void RemoveDocTypePivotButton_Click(object sender, EventArgs e)
		{
			if (CurrentDocTypePivots.Any())
			{
				CurrentDocTypePivots.Where(pivot => pivot != null).ForEach(pivot =>
				{
					pivot.Delete();
				});
			}
		}

		#endregion

		#region Elements

		protected override void AddTemplatePivot()
		{
			base.AddTemplatePivot();
			RefreshDocConfigsVisibility();
		}

		protected internal IEnumerable<StmMenuItemBase> CurrentChildMenus
		{
			get { return GetCurrentlySelectedElements<StmMenuItemBase>(availableChildMenusGrid); }
		}

		protected internal IEnumerable<StmMenuMenuPivotBase> CurrentChildMenuPivots
		{
			get { return GetCurrentlySelectedElements<StmMenuMenuPivotBase>(childMenusUsedGrid); }
		}

		protected internal IEnumerable<RefDocType> CurrentDocTypes
		{
			get { return GetCurrentlySelectedElements<RefDocType>(availableDocTypesGrid); }
		}

		protected internal IEnumerable<DocumentStmMenuEDocs> CurrentDocTypePivots
		{
			get { return GetCurrentlySelectedElements<DocumentStmMenuEDocs>(docTypesUsedGrid); }
		}

		StmMenuMenuPivotBase CurrentChildMenuPivot
		{
			get { return GetCurrentlySelectedElement<StmMenuMenuPivotBase>(childMenusUsedGrid); }
		}

		StmMenuDocumentConfig CurrentDocConfig
		{
			get { return GetCurrentlySelectedElement<StmMenuDocumentConfig>(docConfigsGrid); }
		}

		DocumentStmMenuEDocs CurrentDocTypePivot
		{
			get { return GetCurrentlySelectedElement<DocumentStmMenuEDocs>(docTypesUsedGrid); }
		}

		internal new DocumentCommand CurrentMenu
		{
			get { return (DocumentCommand)base.CurrentMenu; }
		}

		protected override void OnTemplatesUsedGridAfterBind()
		{
			base.OnTemplatesUsedGridAfterBind();
			RefreshDocConfigsVisibility();
		}

		void SelectChildMenuBasedOnPivot()
		{
			StmMenuMenuPivotBase pivot = CurrentChildMenuPivot;
			if (pivot != null)
			{
				availableChildMenusGrid.SelectSingleElement(pivot.Outward);
			}
		}

		void SelectDocTypeBasedOnPivot()
		{
			DocumentStmMenuEDocs pivot = CurrentDocTypePivot;
			if (pivot != null)
			{
				availableDocTypesGrid.SelectSingleElement(pivot.DocType);
			}
		}

		protected override void TrackGridsWhenMenuGridPositionChanges()
		{
			base.TrackGridsWhenMenuGridPositionChanges();
			if (PivotAndChildMenuTabControl.SelectedTab == menuChildMenusTabPage)
			{
				SelectChildMenuBasedOnPivot();
			}
			else if (PivotAndChildMenuTabControl.SelectedTab == menuDocTypesTabPage)
			{
				SelectDocTypeBasedOnPivot();
			}
		}

		protected override void TrackTemplatesUsedGrid()
		{
			base.TrackTemplatesUsedGrid();
			RefreshDocConfigsVisibility();
		}

		#endregion

		#region Macro / Filter Evaluator

		void ShowMacroEvaluator(object sender, EventArgs e)
		{
			if (MacroEvaluatorManager != null)
			{
				MacroEvaluatorManager.MenuItem = CurrentMenu;
				MacroEvaluatorManager.Macro = ZString.Empty;
				MacroEvaluatorManager.Output = ZString.Empty;
				ZFormModaliser.Show(new MacroEvaluatorForm(MacroEvaluatorManager), this);
			}
		}

		MacroEvaluatorManager MacroEvaluatorManager
		{
			get
			{
				if (macroEvaluatorManager == null && BusinessEntity.Parent != null)
				{
					macroEvaluatorManager = new MacroEvaluatorManager(((BusinessObject)BusinessEntity.Parent).Factory, BusinessEntity.Parent);
				}

				return macroEvaluatorManager;
			}
		}
		MacroEvaluatorManager macroEvaluatorManager;

		void ShowFilterEvaluator(object sender, EventArgs e)
		{
			if (FilterEvaluatorManager != null)
			{
				FilterEvaluatorManager.Macro = CurrentMenu != null ? CurrentMenu.SU_FilterList.ToString() : string.Empty;
				ZFormModaliser.Show(new MacroEvaluatorForm(FilterEvaluatorManager), this);
			}
		}

		FilterEvaluatorManager FilterEvaluatorManager
		{
			get
			{
				if (filterEvaluatorManager == null && BusinessEntity.Parent != null)
				{
					filterEvaluatorManager = new FilterEvaluatorManager(((BusinessObject)BusinessEntity.Parent).Factory, BusinessEntity.Parent);
				}

				return filterEvaluatorManager;
			}
		}
		FilterEvaluatorManager filterEvaluatorManager;

		#endregion

		void copyConfigToNewDocButton_Click(object sender, EventArgs e)
		{
			StmMenuItem copy = BusinessEntity.CopyConfigToNewDoc(CurrentMenu, CurrentTemplatePivots);
			MenusGrid.SelectSingleElement(copy);
		}
	}
}
