using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Billing.Integration;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Core.Modules;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.ModulePlugIn;
using Enterprise.ZArchitecture.Modules.AutoRefresh;
using Res = Enterprise.ZArchitecture.GUI.Res;
using ResString = Enterprise.ZArchitecture.GUI.ResString;

namespace Enterprise.ZArchitecture.Modules
{
	public abstract class ZFilterGridModule : ZFilterModule, IZFilterGridModule, IFilterGridModuleInternalsForTesting
	{
		protected ZFilterGridModule()
		{
		}

		public bool HasErrors { get => FilterBusinessObject != null && FilterBusinessObject.HasErrors; }

		public int ExactRowCount
			=> SearchManager.DoGetExactRowCountOnExcessResult(GridCollection, ExportQuery);

		protected override FilteredGridLoader CreateSearchManager()
			=> new FilteredGridLoader(FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);

		#region Disable Search

		public void DisableSearch()
		{
			var filterStripControl = EmbeddedControl as ZFilterStripControl;
			if (filterStripControl != null)
			{
				filterStripControl.DisableSearch();
			}
			Grid.Dock = DockStyle.Fill;
			Grid.BringToFront();
		}

		#endregion

		#region Menus

		public static class DeleteButtonCaptions
		{
			public static string Delete
			{
				get { return Res.GetString("201f55ed-5bc2-4eea-8208-bfadfea28f02", "&Delete"); }
			}

			public static string Activate
			{
				get { return Res.GetString("76cea9b6-759d-46f5-b166-1c90c5d501f2", "&Activate"); }
			}

			public static string Deactivate
			{
				get { return Res.GetString("8c3d06f3-a55a-43d0-9ef6-c5abe1160a89", "&Deactivate"); }
			}
		}

		public static class CommonDataTransferCaptions
		{
			public static string FromXmlMenuText
			{
				get { return Res.GetString("1c0c65d1-0f0c-41b3-b87f-1195b1e970f5", "From &XML"); }
			}

			public static string ToXmlMenuText
			{
				get { return Res.GetString("1376e2c3-af00-472c-8626-4944a77f8dfd", "To &XML"); }
			}

			public static string FromCsvMenuText
			{
				get { return Res.GetString("cfa78267-aca5-494c-bd83-478b1038fdea", "From &CSV"); }
			}
		}

		public MenuItem ViewMenuItem { get; protected set; }
		public ZMenuItem NewMenuItem { get; protected set; }
		public MenuItem EditMenuItem { get; protected set; }
		public ZMenuItem CopyMenuItem { get; protected set; }
		public MenuItem DeleteMenuItem { get; protected set; }
		public MenuItem DataTransferMenuItem { get; protected set; }
		public MenuItem ActionsMenuItem { get; protected set; }
		public MenuItem ToggleFilterVisibilityMenuItem { get; protected set; }
		public ZMenuItem ToggleIndexSearchFilterMenuItem { get; protected set; }
		protected MenuItem ActivateMenuItem;
		protected MenuItem DeActivateMenuItem;

		protected virtual bool AllowAdvancedDataAutomationWizard => AllowNew;

		protected virtual Type TypeOfElementsForImportWizard => GridCollection.TypeOfElements;

		public virtual bool AllowCopyFilterGridHyperlinkToClipboard
		{
			get { return true; }
		}

		public bool AllowAddCopyMenuItem => AllowNew && CanBeCopied();

		public bool AllowSendEmailAction => SendEmailInstanceType != null && typeof(ISendEmailActionSource).IsAssignableFrom(SendEmailInstanceType);

		Type sendEmailInstanceType;
		protected virtual Type SendEmailInstanceType
		{
			get
			{
				if (sendEmailInstanceType == null)
				{
					try
					{
						sendEmailInstanceType = TypeOfTopLevelBusinessObject;
					}
					catch (ModuleGuiNotSupportedException) { }
					catch (NotSupportedException) { }
					catch (NotImplementedException) { }
				}

				return sendEmailInstanceType;
			}
		}

		protected virtual MenuItem[] GetNewStandardMenuItems()
		{
			var menu = new List<MenuItem>();

			if (AllowView)
			{
				menu.Add(ViewMenuItem = new ZMenuItem(Res.GetData("ModuleGrid.View", "&View", "Views the selected item read-only."), HandleViewClick, IconTypes.ViewButtonActive, IconTypes.ViewButtonRest));
			}

			if (AllowNew)
			{
				menu.Add(NewMenuItem = new ZMenuItem(Res.GetData("ModuleGrid.New", "&New", "Creates a new item."), HandleNewClick, IconTypes.NewButtonActive, IconTypes.NewButtonRest));
				NewMenuItem.Name = NewMenuItemName;
				AddUniversalCopyMenuItems(NewMenuItem);
				var newMenuItem = NewMenuItem;
				NewMenuItem.Popup += (_, __) => AddUniversalCopyMenuItems(newMenuItem); // There are two New menu items, use local newMenuItem to add universal copy menus to all New menu items
			}

			if (AllowEdit)
			{
				menu.Add(EditMenuItem = new ZMenuItem(Res.GetData("ModuleGrid.Edit", "&Edit", "Edits the selected item (shortcut Enter)"), HandleEditClick, IconTypes.EditButtonActive, IconTypes.EditButtonRest));
			}

			if (AllowAddCopyMenuItem)
			{
				AddCopyMenuItem(menu);
			}

			if (AllowDelete)
			{
				menu.Add(DeleteMenuItem = new ZMenuItem(GetDeleteMenuItemText(), HandleDeleteClick, IconTypes.DeleteButtonActive, IconTypes.DeleteButtonRest));
			}

			return menu.ToArray();
		}

		protected virtual MenuItem[] GetNewAdditionalContextMenuItems()
		{
			return Array.Empty<MenuItem>();
		}

		protected virtual MenuItem[] GetNewAdditionalMenuItems()
		{
			var result = new List<MenuItem>();
			var actionsMenuItems = GetNewActionMenuItems();
			if (actionsMenuItems.Length == 1 && actionsMenuItems[0].Name != PlaceholderForPluginsActionMenuItems)
			{
				result.Add(actionsMenuItems[0]);
			}
			else if (actionsMenuItems.Length > 0)
			{
				ActionsMenuItem = GetActionsTopMenuItem(actionsMenuItems);
				result.Add(ActionsMenuItem);
			}

			foreach (var menuItem in GetAdditionalMenuItems())
			{
				result.Add(menuItem);
			}

			if (AllowToggleFilterVisibilityMenuItem)
			{
				ToggleFilterVisibilityMenuItem = new ZMenuItem(ToggleFilterVisibilityText, ToggleFilterVisibility_Click, IconTypes.CollapseButtonActive, IconTypes.CollapseButtonRest);
				result.Add(ToggleFilterVisibilityMenuItem);
			}
			if (AllowToggleIndexSearchFilterMenuItemVisibility && ObjectFactory.Get<IGlowRegistry>().IsGlowIndexSearchAllowedForModule(ID.Name) && FilterBusinessObject?.HasIndexSearchFields == true)
			{
				var text = GlowIndexQueryService.Business.GlowModuleToCW1ModuleConverter.CheckIsVerifiedModule(ID) ?
					ToggleIndexSearchFilterText : ToggleUnverifiedIndexSearchFilterText;
				ToggleIndexSearchFilterMenuItem = new ZMenuItem(text, ToggleIndexSearchFilter_Click, IconTypes.ToggleOn, IconTypes.ToggleOn);
				ToggleIndexSearchFilterMenuItem.Visible = false;
				result.Add(ToggleIndexSearchFilterMenuItem);
			}

			return result.ToArray();
		}

		internal MenuItem GetActionsTopMenuItem(MenuItem[] actionsMenuItems)
		{
			var result = new ZMenuItem(Res.GetData("ModuleGrid.Actions", "&Actions"), IconTypes.ActionsButtonActive, IconTypes.ActionsButtonRest);

			result.MenuItems.AddRange(actionsMenuItems.Where(m => m != null).ToArray());
			result.Popup += delegate
			{
				ZFormUtilities.EnsureSelectedControlValueCommitted(Grid.FindForm());
				result.MenuItems.RemoveByKey(PlaceholderForPluginsActionMenuItems);
				if (result.Tag != null)
				{
					foreach (var item in (MenuItem[])result.Tag)
					{
						result.MenuItems.Remove(item);
					}
				}

				var actionCopyMenuItem = result.MenuItems.FindByName(CopyFilterGridHyperlinkToClipboardMenuItem.CopySelectedHyperlinksToClipboard);
				var pluginActionMenuItems = Plugins.GetActionMenuItemsToAdd();

				PostProcessActionMenuItems(pluginActionMenuItems);

				if (actionCopyMenuItem != null)
				{
					result.MenuItems.InsertRange(actionCopyMenuItem.Index + 1, pluginActionMenuItems);
				}
				else
				{
					result.MenuItems.AddRange(pluginActionMenuItems);
				}

				result.Tag = pluginActionMenuItems;
			};

			return result;
		}

		protected virtual void PostProcessActionMenuItems(MenuItem[] actionMenuItems) { }

		void ToggleFilterVisibility_Click(object sender, EventArgs args)
		{
			if (FilterStripEmbeddedControl != null)
			{
				FilterStripEmbeddedControl.IsFilterVisible = !FilterStripEmbeddedControl.IsFilterVisible;

				if (ToggleFilterVisibilityButton != null)
				{
					if (FilterStripEmbeddedControl.IsFilterVisible)
					{
						ToggleFilterVisibilityButton.ImageIndex = Icons.GetImageIndex(IconTypes.CollapseButtonRest);
						ToggleFilterVisibilityButton.ActiveIconType = IconTypes.CollapseButtonActive;
					}
					else
					{
						ToggleFilterVisibilityButton.ImageIndex = Icons.GetImageIndex(IconTypes.ExpandButtonRest);
						ToggleFilterVisibilityButton.ActiveIconType = IconTypes.ExpandButtonActive;
					}
				}

				var visible = FilterBusinessObject.HasIndexSearchFields && FilterStripEmbeddedControl.IsFilterVisible;
				SetToggleIndexSearchVisible(visible);
			}
		}

		void ToggleIndexSearchFilter_Click(object sender, EventArgs args)
		{
			FilterBusinessObject.SearchType = FilterBusinessObject.SearchType == SearchType.Index ? SearchType.Sql : SearchType.Index;
			FilterBusinessObject.LoadLayout(null);
			FilterStripEmbeddedControl?.ResetFindButton();
		}

		internal ZFilterStripBaseControl FilterStripEmbeddedControl => (ZFilterStripBaseControl)EmbeddedControl;

		protected override void SearchTypeChanged(SearchType searchType)
		{
			base.SearchTypeChanged(searchType);

			if (ToggleIndexSearchFilterButton != null)
			{
				if (searchType == SearchType.Index)
				{
					ToggleIndexSearchFilterButton.ImageIndex = Icons.GetImageIndex(IconTypes.ToggleOn);
					ToggleIndexSearchFilterButton.ActiveIconType = IconTypes.ToggleOn;

					var filterVisible = FilterStripEmbeddedControl?.IsFilterVisible ?? false;
					SetToggleIndexSearchVisible(filterVisible);
				}
				else
				{
					ToggleIndexSearchFilterButton.ImageIndex = Icons.GetImageIndex(IconTypes.ToggleOff);
					ToggleIndexSearchFilterButton.ActiveIconType = IconTypes.ToggleOff;
				}
			}
		}

		void SetToggleIndexSearchVisible(bool visible)
		{
			if (ToggleIndexSearchFilterMenuItem != null)
			{
				ToggleIndexSearchFilterMenuItem.Visible = visible;
			}
			if (ToggleIndexSearchFilterButton != null)
			{
				ToggleIndexSearchFilterButton.Visible = visible;
			}
		}

		protected override void OnGlowIndexQueryErrorAction(string errorMessage)
		{
			Globals.Message.ShowError(errorMessage, Res.GetString("cfae3163-5915-4e06-b9f5-de93aa580f98", "Index Search Error"));
		}

		protected virtual MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>();
			var dataTransfer = GetDataTransferMenuItem();

			if (dataTransfer != null)
			{
				result.Add(dataTransfer);
			}

			if (AllowCopyFilterGridHyperlinkToClipboard)
			{
				result.Add(new CopyFilterGridHyperlinkToClipboardMenuItem(this));
			}

			if (AllowEdit)
			{
				var pluginActionMenuItems = Plugins.GetActionMenuItemsToAdd();
				if (pluginActionMenuItems != null && pluginActionMenuItems.Any())
				{
					result.Add(new ZMenuItem { Name = PlaceholderForPluginsActionMenuItems });
				}

				result.AddRange(GetAdditionalActionsMenuItemProviders());

				if (AllowDefaultActivateDeactivate)
				{
					result.Add(new ZMenuItem(ZMenuItem.Separator));
					result.Add(ActivateMenuItem = new ZMenuItem((NoResString)DeleteButtonCaptions.Activate, ActivateBO));
					result.Add(DeActivateMenuItem = new ZMenuItem((NoResString)DeleteButtonCaptions.Deactivate, DeActivateBO));
				}
			}

			return result.ToArray();
		}

		void AddADAWMenuItem(Menu.MenuItemCollection menuItems)
		{
			var menuItem = new ZMenuItem(ResString.GetMultilingualString("685d538e-ba5f-4342-862f-97177b8c3047", "Advanced Data Automation Wizard"));
			menuItem.Name = GlowIntegrationMenuItem;
			menuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("8fdc6f86-bfff-4a6b-bd7c-5af9850f1d6f", "Loading...")));
			menuItem.Popup += (s, a) =>
			{
				if (!typeof(ZGrid.ShortcutPopupEventArgs).IsAssignableFrom(a.GetType()))
				{
					GlowDataWizardIntegration.HandleADAWMenuItemPopup(menuItem, TypeOfElementsForImportWizard, ShowImportWizard, ShowImportMappingWizard);
				}
			};
			menuItems.Add(menuItem);
		}

		void ShowImportWizard(IDataTransferMapping mapping)
		{
			var importWizardResult = GlowDataWizardIntegration.ShowEmbeddedImportWizard(mapping, () => PerformSearch());

			if (!string.IsNullOrEmpty(importWizardResult))
			{
				Globals.Message.ShowError(importWizardResult);
			}
		}

		void ShowImportMappingWizard(IDataTransferMapping mapping = null)
		{
			var importMappingWizardResult = ShowImportMappingWizardCore(TypeOfElementsForImportWizard, mapping);

			if (!string.IsNullOrEmpty(importMappingWizardResult))
			{
				Globals.Message.ShowError(importMappingWizardResult);
			}
		}

#if DEBUG
		internal virtual
#endif
		string ShowImportMappingWizardCore(Type typeOfElements, IDataTransferMapping mapping)
		{
			return GlowDataWizardIntegration.ShowImportMappingWizard(typeOfElements, mapping);
		}

		const string PlaceholderForPluginsActionMenuItems = "PlaceholderForPluginsActionMenuItems"; // menu item key
		const string GlowIntegrationMenuItem = "GlowIntegrationMenuItem"; // menu item key

		IEnumerable<MenuItem> GetAdditionalMenuItems()
		{
			return AdditionalTopLevelMenuItemProviders.SelectMany(p => p.GetMenuItems(this)).Where(m => m != null);
		}

		IEnumerable<MenuItem> GetAdditionalActionsMenuItemProviders()
		{
			return AdditionalActionsMenuItemProviders.SelectMany(p => p.GetMenuItems(this)).Where(m => m != null);
		}

		bool SetButtonDetailFromAdditionalMenuItemProviders(MenuItem item, ref IconTypes buttonImage, ref IconTypes buttonImageActive, ref string buttonToolTip)
		{
			foreach (var provider in AdditionalTopLevelMenuItemProviders)
			{
				if (provider.TryGetButtonDetail(item, ref buttonImage, ref buttonImageActive, ref buttonToolTip))
				{
					return true;
				}
			}
			return false;
		}

		IEnumerable<IFilterGridMenuItemProvider> AdditionalActionsMenuItemProviders
		{
			get { return ((IEnumerable)ObjectFactory.Get("FilterGridActionsMenuItemProviders")).Cast<IFilterGridMenuItemProvider>(); }
		}

		IEnumerable<IFilterGridTopLevelMenuItemProvider> AdditionalTopLevelMenuItemProviders
		{
			get { return additionalTopLevelMenuItemProviders ?? (additionalTopLevelMenuItemProviders = ((IEnumerable)ObjectFactory.Get("FilterGridTopLevelMenuItemProviders")).Cast<IFilterGridTopLevelMenuItemProvider>()); }
		}
		IEnumerable<IFilterGridTopLevelMenuItemProvider> additionalTopLevelMenuItemProviders;

		protected MenuItem[] UniversalDataTransferMenuItems
		{
			get
			{
				return GetUniversalDataTransferMenuItems() ?? Array.Empty<MenuItem>();
			}
		}

		protected virtual MenuItem[] GetUniversalDataTransferMenuItems()
		{
			return null;
		}

		MenuItem GetDataTransferMenuItem()
		{
			if (ImportMenuItems.Count == 0 && ExportMenuItems.Count == 0 && UniversalDataTransferMenuItems.Length == 0)
			{
				return null;
			}

			var dataTransferMenuItem = new ZMenuItem(ResString.GetMultilingualString("ModuleGrid.DataTransfer", "D&ata Transfer"));
			dataTransferMenuItem.Name = ExportXmlMenuItemHelper.DataTransferMenuItemName;

			dataTransferMenuItem.MenuItems.Add("");

			AddDataTransferMenuItem(dataTransferMenuItem.MenuItems);

			foreach (var menuItem in UniversalDataTransferMenuItems)
			{
				dataTransferMenuItem.MenuItems.Add(menuItem);
			}

			dataTransferMenuItem.Popup += (o, e) =>
			{
				AddNativeDataTransferMenuItem(dataTransferMenuItem.MenuItems);
			};

			if (AllowAdvancedDataAutomationWizard && GlowDataWizardIntegration.IsADAWEnabled())
			{
				dataTransferMenuItem.MenuItems.Add("-");
				AddADAWMenuItem(dataTransferMenuItem.MenuItems);
			}

			DataTransferMenuItem = dataTransferMenuItem;
			return dataTransferMenuItem;
		}

		void AddDataTransferMenuItem(Menu.MenuItemCollection menuItems)
		{
			if (!menuItemIsEmpty(menuItems))
			{
				return;
			}

			menuItems.Clear();

			if (ImportMenuItems.Count > 0)
			{
				foreach (var item in ImportMenuItems)
				{
					menuItems.Add(new ZMenuItem(item.Text, item.OnClick));
				}
				menuItems.Add("-");
			}

			if (ExportMenuItems.Count > 0)
			{
				foreach (var item in ExportMenuItems)
				{
					menuItems.Add(new ZMenuItem(item.Text, item.OnClick));
				}
			}
		}

		bool menuItemIsEmpty(Menu.MenuItemCollection menuItems)
		{
			return menuItems.Count == 1 && string.IsNullOrEmpty(menuItems[0].Text);
		}

		#region Native Data Transfer

		internal void AddNativeDataTransferMenuItem(Menu.MenuItemCollection menuItems)
		{
			if (menuItems[ExportXmlMenuItemHelper.NativeSchemaMenuItemName] != null)
			{
				return;
			}

			importNativeXmlMenuItem = null;
			exportNativeXmlMenuItem = null;
			nativeXmlSchemasMenuItem = null;

			if (ImportNativeXmlMenuItem != null || ExportNativeXmlMenuItem != null || NativeXmlSchemasMenuItem != null)
			{
				var index = menuItems.Count;
				for (var i = 0; i < menuItems.Count; i++)
				{
					if (menuItems[i].Text == "-")
					{
						index = i;
					}
				}

				if (menuItems.Count > 0 && index == menuItems.Count)
				{
					menuItems.Add(0, new ZMenuItem("-"));
					index = 0;
				}

				if (ImportNativeXmlMenuItem != null)
				{
					menuItems.Add(index++, ImportNativeXmlMenuItem);
				}
				if (ExportNativeXmlMenuItem != null)
				{
					menuItems.Add(index++, ExportNativeXmlMenuItem);
				}
				if (NativeXmlSchemasMenuItem != null)
				{
					menuItems.Add(index, NativeXmlSchemasMenuItem);
				}
			}
		}

		IImportService NativeXmlImportService
		{
			get
			{
				if (nativeXmlImportService == null)
				{
					nativeXmlImportService = GetNativeXmlImportService();
				}
				return nativeXmlImportService;
			}
		}
		IImportService nativeXmlImportService;

		IExportService NativeXmlExportService
		{
			get
			{
				if (nativeXmlExportService == null)
				{
					nativeXmlExportService = GetNativeXmlExportService();
				}
				return nativeXmlExportService;
			}
		}
		IExportService nativeXmlExportService;

		protected ZMenuItem NativeXmlSchemasMenuItem
		{
			get
			{
				if (nativeXmlSchemasMenuItem == null)
				{
					nativeXmlSchemasMenuItem = GetNativeXMLSchemasMenuItem();
				}
				return nativeXmlSchemasMenuItem;
			}
		}
		ZMenuItem nativeXmlSchemasMenuItem;

		protected ZMenuItem ImportNativeXmlMenuItem
		{
			get
			{
				if (importNativeXmlMenuItem == null)
				{
					importNativeXmlMenuItem = GetImportDataTransferMenuItem();
				}
				return importNativeXmlMenuItem;
			}
		}
		ZMenuItem importNativeXmlMenuItem;

		protected ZMenuItem ExportNativeXmlMenuItem
		{
			get
			{
				if (exportNativeXmlMenuItem == null)
				{
					exportNativeXmlMenuItem = GetExportDataTransferMenuItem();
				}
				return exportNativeXmlMenuItem;
			}
		}
		ZMenuItem exportNativeXmlMenuItem;

		public bool HasImportNativeXmlMenuItem
		{
			get
			{
				var tableName = GetTableNameForNativeDataSetToShow();
				return NativeXmlImportService != null && NativeXmlImportService.CanBeImported(tableName);
			}
		}

		public bool HasExportNativeXmlMenuItem
		{
			get
			{
				return NativeXmlExportService != null && NativeXmlExportService.CanBeExported(GridCollection.TypeOfElements);
			}
		}

		protected override FilterModuleMenuItemDescriptorCollection ExportMenuItems
		{
			get
			{
				if (fExportMenuItems == null)
				{
					_ = base.ExportMenuItems;
					if (Env.CurrentUser.IsSupportUser) // unpublished feature (yet)
					{
						//I THINK this is faster on average than instantiating GridCollection -
						//does have to go and look up and instantiate by reflection a ZController but that does less Other Stuff
						//It would also be nice if this ran less often, just for performance reasons...
						var referenceXMLExport = GridCollection.TypeOfElements.GetCustomAttribute<Customs.Shared.EnableRefererenceXMLExporterAttribute>();
						if (referenceXMLExport != null)
						{
							fExportMenuItems.Add(ResString.GetMultilingualString("D0F801CA-7CE9-4860-BD6E-1A62EF705E82", "Export Reference XML"), new EventHandler(ExportReferenceXML));
							fExportMenuItems.Add(ResString.GetMultilingualString("FEBD6686-E56B-4251-818C-6CEACFC374C4", "Upload Reference XML"), new EventHandler(UploadReferenceXML));
						}
					}
				}
				return fExportMenuItems;
			}
		}

		ZMenuItem GetNativeXMLSchemasMenuItem()
		{
			if (!HasImportNativeXmlMenuItem)
			{
				return null;
			}

			var tableName = GetTableNameForNativeDataSetToShow();
			var nativeSchemaMenuItemText = ResString.GetMultilingualString("078a3e1b-7df8-4f17-9a38-57eb1d0b178e", "Native XML Schemas");

			var menuItem = new ZMenuItem(nativeSchemaMenuItemText); // Schemas is a real word
			menuItem.Name = ExportXmlMenuItemHelper.NativeSchemaMenuItemName;
			menuItem.MenuItems.Add(0, new ZMenuItem(ResString.GetMultilingualString("4e1ab8c9-6cef-4b70-82af-0782f23365d7", "Generate {0} XSD", tableName), (o, e) => { NativeXmlImportService.GenerateAndSaveXSD(tableName); }));
			menuItem.MenuItems.Add(1, new ZMenuItem(ResString.GetMultilingualString("337ac568-2bbd-4181-961e-b850f5447d8e", "Generate All Native XSDs"), (o, e) => { NativeXmlImportService.GenerateAndSaveXSD(null); }));

			return menuItem;
		}

		protected virtual Func<DataTable, DataRow> FilterOnMultiRowResult => null;

		ZMenuItem GetExportDataTransferMenuItem()
		{
			if (!HasExportNativeXmlMenuItem)
			{
				return null;
			}

			var nativeDataExportMenuText = ResString.GetMultilingualString("07a55cd1-ace8-48d7-b7f1-bc3455495954", "Export Native XML");
			EventHandler handler = SecurityCheckerForExportingNativeXML((o, e) =>
			{
				NativeXmlExportService.ExportWithSave(SelectedBusinessObjects, FilterOnMultiRowResult);
			}).OnClick;

#if DEBUG
			NativeXmlMenuItems.Add(nativeDataExportMenuText, handler);
#endif

			var menuItem = new ZMenuItem(nativeDataExportMenuText, handler);
			menuItem.Name = ExportXmlMenuItemHelper.NativeExportMenuItemName;
			return menuItem;
		}

		ZMenuItem GetImportDataTransferMenuItem()
		{
			if (!HasImportNativeXmlMenuItem)
			{
				return null;
			}

			var nativeDataImportMenuText = ResString.GetMultilingualString("e68251c4-672f-4de6-9818-030bbe641b28", "Import Native XML");
			EventHandler handler = SecurityCheckerForImportingNativeXML((o, e) => { NativeXmlImportService.Import(); }).OnClick;

#if DEBUG
			NativeXmlMenuItems.Add(nativeDataImportMenuText, handler);
#endif

			var menuItem = new ZMenuItem(nativeDataImportMenuText, handler);
			menuItem.Name = ExportXmlMenuItemHelper.NativeImportMenuItemName;
			return menuItem;
		}

#if DEBUG
		public FilterModuleMenuItemDescriptorCollection NativeXmlMenuItems
		{
			get
			{
				if (nativeXmlMenuItems == null)
				{
					nativeXmlMenuItems = new FilterModuleMenuItemDescriptorCollection();
				}
				return nativeXmlMenuItems;
			}
		}
		FilterModuleMenuItemDescriptorCollection nativeXmlMenuItems;
#endif

		#region GetNativeXmlImportService()

#if DEBUG
		internal virtual
#endif
 IImportService GetNativeXmlImportService()
		{
			return ObjectFactory.GetDesignerSafe<IImportService>("NativeXmlImportService");
		}

#if DEBUG
		internal virtual
#endif
		IExportService GetNativeXmlExportService()
		{
			return ObjectFactory.GetDesignerSafe<IExportService>("NativeXmlExportService");
		}

		#endregion

		string GetTableNameForNativeDataSetToShow()
		{
			var type = GridCollection.TypeOfElements;
			var tableName = BusinessObjectFactory.GetTableNameFromType(type, false);
			if (this.ID == ModuleId.RefCurrency.GetModuleID())
			{
				tableName = "RefExchangeRate";
			}
			return tableName;
		}

		protected internal SecurityCheckpoint ImportNativeXMLSecurityCheckpoint
		{
			get
			{
				var result = SecurityCheckpoint;
				var security = EnvProxy.Instance.Security;
				if (result != security.None)
				{
					result = security.FindOrCreateImportNativeXmlCheckPoint(SecurityCheckpoint) as SecurityCheckpoint;
				}
				return result;
			}
		}

		protected internal SecurityCheckpoint ExportNativeXMLSecurityCheckpoint
		{
			get
			{
				var result = SecurityCheckpoint;
				var security = EnvProxy.Instance.Security;
				if (result != security.None)
				{
					result = security.FindOrCreateExportNativeXmlCheckPoint(SecurityCheckpoint) as SecurityCheckpoint;
				}
				return result;
			}
		}

		protected virtual ImportSecurityChecker SecurityCheckerForImportingNativeXML(EventHandler handler)
		{
			return new ImportSecurityChecker(handler, ImportNativeXMLSecurityCheckpoint);
		}

		protected virtual ImportSecurityChecker SecurityCheckerForExportingNativeXML(EventHandler handler)
		{
			return new ImportSecurityChecker(handler, ExportNativeXMLSecurityCheckpoint);
		}

		#endregion

		protected override void PopulateImportMenuItems(FilterModuleMenuItemDescriptorCollection menuItems)
		{
			var importCollectionInfoProvider = this as IImportCollectionInfoProvider;
			if (importCollectionInfoProvider != null)
			{
				var eventHandler = (EventHandler)SecurityCheckerForImportingData(ImportDataClick).OnClick;
				menuItems.Add(ResString.GetMultilingualString("f372b7ff-1d88-44ab-8ceb-e4e4a99619d2", "&Import By Data Wizard"), eventHandler);
			}
		}

		#endregion

		#region Grid and Context Menu

		public override ZFilterGrid DisplayGrid
		{
			get { return Grid; }
		}

		public virtual BusinessObject[] GetSelectedBusinessObjects()
		{
			if (!IsEmbeddedControlConstructed)
			{ return Array.Empty<BusinessObject>(); }
			return Grid?.SelectedElements ?? Array.Empty<BusinessObject>();
		}

		protected internal ZDisplayGrid Grid
		{
			get
			{
				return SetupAndGetGrid();
			}
		}

		protected virtual ZDisplayGrid SetupAndGetGridCore()
		{
			if (!Globals.IsUserInteractive)
			{
				ErrorReporter.ReportOnce("CantCreateWinformsFromNonInteractive", "Attempted to create a ZDisplayGrid in a non-interactive environment.\r\nGlobals.IsUserInteractive was set to false, stack trace: " + Globals.IsUserInteractiveStack);
			}
			else if (!inSetupAndGetGrid)
			{
				inSetupAndGetGrid = true;
				try
				{
					var embeddedControl = (IFilterControl)EmbeddedControl;
					var result = (ZDisplayGrid)embeddedControl.FilteredGrid;
					result.CopySelectedRowsAllowed = CheckCopySelectedRowsAllowed();
					OnSetupAndGetGrid(result);
					SetupTaskMenu(result);

					if (!AllowCustomiseColumn)
					{
						result.DisableCustomiseMenuItem();
					}

					return result;
				}
				finally
				{
					inSetupAndGetGrid = false;
				}
			}
			return null;
		}

		public ZDisplayGrid SetupAndGetGrid()
		{
			return SetupAndGetGridCore();
		}

		protected virtual void OnSetupAndGetGrid(ZDisplayGrid grid)
		{
		}

		protected virtual void SetupTaskMenu(ZDisplayGrid grid)
		{
			if (SupportsWorkflow)
			{
				ObjectFactory.Get<IModuleGridTasksStatusChangerFactory>().Create(grid).Initialise();
			}
		}

		protected virtual bool CheckCopySelectedRowsAllowed()
		{
			return ExportSecurityCheckpoint == null || ExportSecurityCheckpoint.IsAllowed;
		}

		bool inSetupAndGetGrid;

		protected void SetupGridContextMenu(ZDisplayGrid grid)
		{
			if (ContextMenu != null && ContextMenu.Length > 0)
			{
				NonActionContextMenuItems = new MenuItem[grid.ContextMenu.MenuItems.Count];
				grid.ContextMenu.MenuItems.CopyTo(NonActionContextMenuItems, 0);

				int i;
				for (i = 0; i < ContextMenu.Length; i++)
				{
					ContextMenu[i].Visible = true;
					grid.ContextMenu.MenuItems.Add(i, ContextMenu[i]);
				}

				grid.ContextMenu.MenuItems.Add(i, new ZMenuItem("-"));

				if (DeleteMenuItem != null)
				{
					grid.DeleteMenuItemModule = DeleteMenuItem;
				}

				if (CopyMenuItem != null)
				{
					grid.CopyMenuItem = CopyMenuItem;
				}
			}
		}

		protected MenuItem[] ContextMenu
		{
			get
			{
				if (fContextMenu == null && HasActions)
				{
					var list = new List<MenuItem>();

					if (HasNotLoadedImportMenuItems)
					{
						AddInterfaceConnectorMenuItems();
						AddExtraImportExportMenuItems();
					}
					if (LimitedColumns == null)
					{
						list.AddRange(GetNewStandardMenuItems());
						list.Add(new ZMenuItem("-"));
						list.AddRange(GetNewAdditionalMenuItems());
						list.AddRange(GetNewAdditionalContextMenuItems());
						if (AllowAutoRefresh)
						{
							list.Add(AutoRefreshMenuItem);
						}
						AddNewTemplateRecordMenuItem();
					}

					fContextMenu = list.ToArray();
				}
				return fContextMenu;
			}
		}

		MenuItem[] NonActionContextMenuItems;
		MenuItem[] fContextMenu;

		void ImportDataClick(object sender, EventArgs e)
		{
			var controller = GetNewController(null);
			var checkpoint = (ISecurityCheckpoint)controller.GetCheckPointForNew(null);
			if (checkpoint.IsAllowed)
			{
				RunImportDataWizard();
			}
			else
			{
				checkpoint.ShowError();
			}
		}

		protected virtual DataTransferProcessor GetDataImportWizardProcessor(IImportCollectionInfo collectionInfo)
		{
			return null;
		}

		protected virtual void RunImportDataWizard()
		{
			var importInfoProvider = (IImportCollectionInfoProvider)this;
			var importInfo = importInfoProvider.ImportCollectionInfo;
			var processor = GetDataImportWizardProcessor(importInfo);
			if (processor != null)
			{
				var form = new MultistepDataImportWizardForm(importInfo, importInfoProvider.ContextKey, new DataTransferProcessor[] { processor }, ID.Description.ToString());
				form.Cancelled += (s, e) => { processor.Rollback(); };
				form.Show();

#if DEBUG
				lastShownDataImportWizardFormForTesting = form;
#endif
			}
			else
			{
				Grid.RunImportDataTool(false, ID.Description.ToString());
			}
		}

#if DEBUG
		internal MultistepDataImportWizardForm lastShownDataImportWizardFormForTesting;
#endif

		#endregion

		#region ToolBar

		public override ToolBarButton[] ToolBarButtons
		{
			get
			{
				if (fToolBarButtons == null && HasActions)
				{
					var list = new List<ToolBarButton>();
					ZToolBarButton newToolButton = null;

					foreach (var item in FormActionMenu)
					{
						var buttonToolTip = "";
						var tmp = item as ZMenuItem;
						if (item == NewMenuItem)
						{
							if (newToolButton == null)
							{
								newToolButton = new ZToolBarButton(KMenuItem.StripAcceleratorKeysButKeepAmpersandInText(item.Text), IconTypes.NewButtonRest, item.PerformClick);
								newToolButton.ActiveIconType = IconTypes.NewButtonActive;
								newToolButton.Enabled = NewMenuItem.Enabled;
								list.Add(newToolButton);
							}

							var itemCopy = item.CloneMenu();
							itemCopy.Text = itemCopy.Text.Replace("New", "");
							if (newToolButton.DropDownMenu == null)
							{
								newToolButton.DropDownMenu = new ContextMenu();
							}

							newToolButton.DropDownMenu.MenuItems.Add(itemCopy);

							newToolButton.Style = ToolBarButtonStyle.DropDownButton;
						}
						else if (item == DataTransferMenuItem)
						{
							if (ImportMenuItems.Count > 0 || ExportMenuItems.Count > 0)
							{
								list.Add(ImportExportDataToolBarButton);
							}
						}

						else if (item.Text != "-" && item != null)
						{
							var buttonImage = tmp.RestIcon != new IconTypes() ? tmp.RestIcon : IconTypes.ViewButtonRest;
							var buttonImageActive = tmp.ActiveIcon != new IconTypes() ? tmp.ActiveIcon : IconTypes.None;
							if (tmp.CaptionResourceString != null)
							{
								buttonToolTip = tmp.CaptionResourceString.FullDescription;
							}
							else
							{
								if (!SetButtonDetailFromAdditionalMenuItemProviders(item, ref buttonImage, ref buttonImageActive, ref buttonToolTip))
								{
									SetupButtonDetailForItem(item, ref buttonImage, ref buttonImageActive, ref buttonToolTip);
								}
							}

							ZToolBarButton button;
							if (item.MenuItems.Count > 0)
							{
								button = new ZToolBarButton(KMenuItem.StripAcceleratorKeysButKeepAmpersandInText(item.Text), buttonImage, item.MenuItems[0].PerformClick);
								button.ActiveIconType = buttonImageActive;
								button.DropDownMenu = new ContextMenu();
								foreach (MenuItem subItem in item.MenuItems)
								{
									button.DropDownMenu.MenuItems.Add(subItem.CloneMenu());
								}
								button.Style = ToolBarButtonStyle.DropDownButton;
							}
							else
							{
								button = new ZToolBarButton(KMenuItem.StripAcceleratorKeysButKeepAmpersandInText(item.Text), buttonImage, item.PerformClick);
								button.ActiveIconType = buttonImageActive;
								if (item == ToggleFilterVisibilityMenuItem)
								{
									ToggleFilterVisibilityButton = button;
								}
								if (item == ToggleIndexSearchFilterMenuItem)
								{
									ToggleIndexSearchFilterButton = button;
									ToggleIndexSearchFilterButton.Visible = item.Visible;
								}
							}
							button.ToolTipText = buttonToolTip;
							button.Enabled = item.Enabled;

							if (item == ActionsMenuItem && button.DropDownMenu != null && button.DropDownMenu.MenuItems != null && button.DropDownMenu.MenuItems.Count > 0)
							{
								if (button.DropDownMenu.MenuItems[0].Text == "-")
								{
									button.DropDownMenu.MenuItems.RemoveAt(0);
								}
							}

							if (item == ActionsMenuItem && button.DropDownMenu == null)
							{
								button.Visible = false;
							}

							list.Add(button);
						}
					}
					fToolBarButtons = list.ToArray();
				}

				return fToolBarButtons;
			}
		}

		protected virtual void SetupButtonDetailForItem(MenuItem item, ref IconTypes buttonImage, ref IconTypes buttonImageActive, ref string buttonToolTip)
		{
		}

		ZToolBarButton ToggleFilterVisibilityButton;
		ZToolBarButton ToggleIndexSearchFilterButton;

		static ResourceStringData ToggleFilterVisibilityText { get { return Res.GetData("FilterStrip|ToolStrip|ButtonHideShowFilters", "Hide/Show Filters", "Hides or Shows the Filter functionality."); } }
		internal static string ActionsMenuItemText { get { return Res.GetString("FilterStrip|ToolStrip|ButtonActions", "Actions"); } }

		static ResourceStringData ToggleIndexSearchFilterText => Res.GetData("FilterStrip|ToolStrip|ToggleIndexSearchFilter", "Index Search", "Switch Between Index Search Filter and SQL Filter.");

		static ResourceStringData ToggleUnverifiedIndexSearchFilterText => Res.GetData("FilterStrip|ToolStrip|ToggleIndexSearchFilterUnverified", "Index Search (Beta)", @"Switch Between Index Search Filter and SQL Filter.
Beta:
When enabling a module that is still in Beta, module searches may have limited functionality.
Please report any issues that are encountered.");

		ZToolBarButton ImportExportDataToolBarButton
		{
			get
			{
				if (fImportExportDataToolBarButton == null)
				{
					fImportExportDataToolBarButton = new ZToolBarButton(Res.GetString("ModuleGrid.DataTransfer", "D&ata Transfer"), IconTypes.ActionsButtonRest, new EventHandler(DefaultImportExportAction));
					fImportExportDataToolBarButton.ActiveIconType = IconTypes.ActionsButtonActive;
					fImportExportDataToolBarButton.Style = ToolBarButtonStyle.DropDownButton;

					fImportExportDataToolBarButton.DropDownMenu = new ContextMenu();
					fImportExportDataToolBarButton.DropDownMenu = MergeImportExportMenuItem(fImportExportDataToolBarButton.DropDownMenu, ImportMenuItems);
					if (ImportMenuItems.Count > 0 && ExportMenuItems.Count > 0)
					{
						fImportExportDataToolBarButton.DropDownMenu.MenuItems.Add("-");
					}
					fImportExportDataToolBarButton.DropDownMenu = MergeImportExportMenuItem(fImportExportDataToolBarButton.DropDownMenu, ExportMenuItems);
					fImportExportDataToolBarButton.ToolTipText = Res.GetString("00549E83-61E3-4e8f-9098-2B6DE453A2F9", "Click For Importing and Exporting Options");
				}

				return fImportExportDataToolBarButton;
			}
		}

		ToolBarButton[] fToolBarButtons;
		ZToolBarButton fImportExportDataToolBarButton;

		void OnDoNothing(object sender, EventArgs e)
		{
		}

		protected virtual void AddCopyMenuItem(List<MenuItem> menu)
		{
			// Creates the main menu item for Copy. This is the top level one.
			// Either on the context menu Copy, or the toolbar Copy. 
			CopyMenuItem = new ZMenuItem(Res.GetData("ModuleGrid.Copy", "&Copy", "Creates a new item based on the currently selected item."), HandleTemplateCopyClick, IconTypes.CopyButtonActive, IconTypes.CopyButtonRest);
			menu.Add(CopyMenuItem);

			AddReversableCopySubMenuItem();
			AddFinalCopySubMenuItems();
		}

		void AddReversableCopySubMenuItem()
		{
			if (CanBeReversed())
			{
				CopyMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ModuleGrid.Reverse", "&Reverse"), HandleCopyAndReverseClick));
			}
		}

		/// <summary>
		/// Called to add any remaining submenu items. The default implementation
		/// adds a submenu 'Copy' if other submenus were added.
		/// </summary>
		protected virtual void AddFinalCopySubMenuItems()
		{
			if (CopyMenuItem.MenuItems.Count > 0)
			{
				CopyMenuItem.MenuItems.Add(0, new ZMenuItem(ResString.GetMultilingualString("ModuleGrid.Copy2", "&Copy"), HandleTemplateCopyClick));
			}
		}

		Menu MergeImportExportMenuItem(Menu menuToMergeTo, FilterModuleMenuItemDescriptorCollection importExportMenu)
		{
			foreach (var item in importExportMenu)
			{
				menuToMergeTo.MenuItems.Add(new ZMenuItem(item.Text, item.OnClick));
			}

			return menuToMergeTo;
		}

		#endregion

		#region Form Action Menu

		public override MenuItem[] FormActionMenu
		{
			get
			{
				if (fFormActionMenu == null && HasActions)
				{
					if (HasNotLoadedImportMenuItems)
					{
						AddInterfaceConnectorMenuItems();
						AddExtraImportExportMenuItems();
					}
					var list = new List<MenuItem>();

					if (LimitedColumns == null)
					{
						list.AddRange(GetNewStandardMenuItems());
						list.Add(new ZMenuItem("-"));
						list.AddRange(GetNewAdditionalMenuItems());
						AddNewTemplateRecordMenuItem();
					}

					fFormActionMenu = list.ToArray();
				}
				return fFormActionMenu;
			}
		}

		MenuItem[] fFormActionMenu;

		bool HasNotLoadedImportMenuItems
		{
			get
			{
				var importCollectionInfoProvider = this as IImportCollectionInfoProvider;
				var text = ResString.GetMultilingualString("f372b7ff-1d88-44ab-8ceb-e4e4a99619d2", "&Import By Data Wizard");
				return ImportMenuItems.Count == 0 ||
						(importCollectionInfoProvider != null && ImportMenuItems.Count == 1 && ImportMenuItems[0].Text.GetUnresolvedString() == text.GetUnresolvedString());
			}
		}

		protected virtual void AddInterfaceConnectorMenuItems()
		{
		}

		void IZFilterGridModule.AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorMenuItems();
		}

		protected virtual void AddExtraImportExportMenuItems()
		{
		}
		#endregion

		#region Plugins

		public ZModulePluginCollection Plugins
		{
			get { return plugins ?? (plugins = new ZModulePluginCollection(this)); }
		}
		ZModulePluginCollection plugins;

		#endregion

		#region Handling New/Edit/View/etc

		void SetupGrid(ZDisplayGrid grid)
		{
			SetupGridContextMenu(grid);

			grid.SetParentFilterGridModule(this);

			grid.MouseDown += new MouseEventHandler(Grid_MouseDown);
			grid.SelectedRowsChangedInMouseDown += new EventHandler(Grid_CurrentCellChanged);
			grid.KeyDown += new KeyEventHandler(Grid_KeyDown);
			grid.IsWholeRowSelectedOnClick = true;
			grid.RemoveAction = RemoveAction.NoRemovePossible;

			grid.CurrentCellChanged += new EventHandler(Grid_CurrentCellChanged);
			grid.AfterBind += new EventHandler(Grid_AfterBind);
		}

		enum DeleteButtonType { Activate, Deactivate, Delete }

		void Grid_AfterBind(object sender, EventArgs e)
		{
			if (Grid.ElementTypeFromCollection != null && typeof(ICancellable).IsAssignableFrom(Grid.ElementTypeFromCollection) && PreventDeleteAttribute.IsTrue(Grid.ElementTypeFromCollection))
			{
				UpdateDeleteButtonInfo(DeleteButtonType.Deactivate);
				IMainForm form = null;
				var parent = Grid.Parent;
				while (parent != null)
				{
					form = parent as IMainForm;
					if (form != null)
					{
						form.UpdateToolBarDeleteButton(this);
						break;
					}
					parent = parent.Parent;
				}
			}
		}

		void Grid_CurrentCellChanged(object sender, EventArgs e)
		{
			if (Grid.SelectedRowCount < 1 && Grid.CurrentRowIndex < Grid.DataGridRowsLength && Grid.CurrentRowIndex > -1)
			{
				Grid.Select(Grid.CurrentRowIndex);
			}

			var selectedElement = Grid.SelectedElements.FirstOrDefault();
			if (selectedElement != null)
			{
				var templateRecord = GetTemplateRecord(selectedElement);
				var cancellable = (templateRecord ?? selectedElement) as ICancellable;

				if (cancellable != null && PreventDeleteAttribute.IsTrue(selectedElement.GetType()))
				{
					var deleteButtonType = cancellable.IsCancelled ? DeleteButtonType.Activate : DeleteButtonType.Deactivate;

					UpdateDeleteButtonInfo(deleteButtonType);

					if (MainForm != null)
					{
						MainForm.UpdateToolBarDeleteButton(this);
					}
				}
			}
		}

		void UpdateDeleteButtonInfo(DeleteButtonType deleteButtonType)
		{
			switch (deleteButtonType)
			{
				case DeleteButtonType.Activate:
					DeleteButtonText = DeleteButtonCaptions.Activate;
					DeleteButtonImage = IconTypes.BlackWhite_Tick;
					DeleteButtonImageActive = IconTypes.BlackWhite_Tick;
					DeleteButtonToolTipText = Res.GetString("35177936-08A1-4535-8A9C-28A60824F2C9", "Activates the selected item");
					break;
				case DeleteButtonType.Deactivate:
					DeleteButtonText = DeleteButtonCaptions.Deactivate;
					DeleteButtonImage = IconTypes.ClearButtonActive;
					DeleteButtonImageActive = IconTypes.ClearButtonActive;
					DeleteButtonToolTipText = Res.GetString("42450807-A68E-4F53-AD35-868D385E2623", "Deactivates the selected item after viewing its details read-only (shortcut Del)");
					break;
				case DeleteButtonType.Delete:
					DeleteButtonText = DeleteButtonCaptions.Delete;
					DeleteButtonImage = IconTypes.DeleteButtonRest;
					DeleteButtonImageActive = IconTypes.DeleteButtonActive;
					DeleteButtonToolTipText = GetDeleteMenuItemText().Caption;
					break;
			}
		}

		public string DeleteButtonText { get; private set; }
		public string DeleteButtonToolTipText { get; private set; }
		public IconTypes DeleteButtonImage { get; private set; }
		public IconTypes DeleteButtonImageActive { get; private set; }

#if DEBUG
		protected virtual
#endif
 IMainForm MainForm
		{
			get
			{
				if (mainForm == null)
				{
					mainForm = LocateMainForm() as IMainForm;
				}
				return mainForm;
			}
		}
		IMainForm mainForm;

		public Form LocateMainForm()
		{
			foreach (var form in ZApplication.GetOpenForms())
			{
				if (form is IMainForm)
				{
					if ((form as EmbeddedModulePopup) != null && (form as EmbeddedModulePopup).Module == this)
					{
						return form;
					}
				}
			}

			foreach (var form in ZApplication.GetOpenForms())
			{
				if (form is IMainForm)
				{
					return form;
				}
			}

			return null;
		}

		void Grid_KeyDown(object sender, KeyEventArgs e)
		{
			HandleKeyDown(e.KeyData);
		}

		void Grid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 2 && e.Button == MouseButtons.Left)
			{
				if (Grid.HitTest(e.X, e.Y).Row > -1)
				{
					HandleEnterOrDoubleClick();
				}
			}
		}

		#endregion

		#region Recent Items

		public bool DoNotShowRecentItems { get; set; }

#if DEBUG
		public bool ShowRecentItemsCoreNotOverriden;
		public
#else
		protected
#endif
		bool ShowRecentItems
		{
			get { return !DoNotShowRecentItems && ShowRecentItemsCore(); }
		}

		protected virtual bool ShowRecentItemsCore()
		{
			try
			{
				var controller = GetNewController(null);
#if DEBUG
				ShowRecentItemsCoreNotOverriden = true;
#endif
				return controller != null && controller.ModuleID != null;
			}
			catch (NotImplementedException)
			{
				return false;
			}
			catch (ModuleGuiNotSupportedException)
			{
				return false;
			}
			catch (NotSupportedException)
			{
				return false;
			}
		}

		public ModuleIdentifier RecentItemsModuleID
		{
			get { return GetRecentItemsModuleIDCore(); }
		}

		protected virtual ModuleIdentifier GetRecentItemsModuleIDCore()
		{
			return ID;
		}

		#endregion

		#region Implementation

		protected internal virtual BusinessObject GetFirstBizOInList()
		{
			return GridCollection.Cast<BusinessObject>().FirstOrDefault();
		}

		protected internal virtual bool ShouldLoadTop1WhenGridEmpty
		{
			get
			{
				return true;
			}
		}

		protected override SortInfo DefaultSortOrder
		{
			get
			{
				var hasSortInfo = (Grid != null && Grid.ColumnStyles.Count > 0);
				return hasSortInfo ? new SortInfo(((ZGridColumnInfo)Grid.ColumnStyles[0]).ColumnName, ListSortDirection.Ascending) : null;
			}
		}

		protected override BusinessObject CurrentBusinessObjectInGrid
		{
			get
			{
				var hasCurrent = (Grid != null && Grid.ListManager != null && Grid.ListManager.Position >= 0);
				return hasCurrent ? (BusinessObject)Grid.ListManager.GetCurrent() : null;
			}
		}

		protected override BusinessObject[] SelectedBusinessObjects
		{
			get
			{
				var result = Array.Empty<BusinessObject>();
				if (Grid != null)
				{
					result = Grid.SelectedElements;
				}
				return result;
			}
		}

		protected override BusinessObjectReader CollectionForExport
		{
			get { return Grid.ReaderForExcelExport; }
		}

		protected override void ExportVisibleIntoAndOpenExcel()
		{
			Grid.ExportVisibleIntoAndOpenExcel();
		}

		protected override void ExportIntoAndOpenExcel()
		{
			Grid.ExportIntoAndOpenExcel();
		}

		protected override Control GetNewEmbeddedControl()
		{
			var result = base.GetNewEmbeddedControl();
			var filterControl = result as IFilterControl;
			if (filterControl != null)
			{
				SetupGrid((ZDisplayGrid)filterControl.FilteredGrid);

				if (ShowRecentItems)
				{
					filterControl.LoadRecentItems();
				}

				SetToggleIndexSearchVisible(FilterBusinessObject.HasIndexSearchFields);
			}

			return result;
		}

		void FilterControl_PerformSearch(object sender, EventArgs e)
		{
			PerformSearch();
		}

		protected override void SuspendGridLayout()
		{
			Grid.SuspendLayout();
		}

		protected override void ResumeGridLayout()
		{
			Grid.ResumeLayout();
		}

		protected override void ForceGridPreFetch()
		{
			Grid.ForcePreFetch();
		}

		protected virtual ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("ModuleGrid.Delete", "&Delete", "Deletes the selected item after viewing its details read-only (shortcut Del)");
		}

		#region Activate/Deactivate

		protected void ActivateBO(object sender, EventArgs e)
		{
			ActivateDeactivate(true);
		}

		protected void DeActivateBO(object sender, EventArgs e)
		{
			ActivateDeactivate(false);
		}

		protected virtual SecurityCheckpoint GetCheckpointForActivateDeactivate(BusinessObject[] selectedObjects)
		{
			var controller = GetNewController(null);
			var checkpoint = controller != null && selectedObjects.Length > 0 ? controller.GetCheckPointForEdit(selectedObjects[0]) : SecurityCheckpoint;
			return checkpoint;
		}

		void ActivateDeactivate(bool activate)
		{
			var selectedObjects = GetSelectedBusinessObjects();
			var checkpoint = GetCheckpointForActivateDeactivate(selectedObjects);

			if (activate)
			{
				BusinessObjectActivatorInstance.Activate(selectedObjects, checkpoint);
			}
			else
			{
				BusinessObjectActivatorInstance.Deactivate(selectedObjects, checkpoint);
			}
		}

		BusinessObjectActivator businessObjectActivator;
		BusinessObjectActivator BusinessObjectActivatorInstance
		{
			get
			{
				if (businessObjectActivator == null)
				{
					businessObjectActivator = GetNewBusinessObjectActivator();
				}
				return businessObjectActivator;
			}
		}

		protected virtual BusinessObjectActivator GetNewBusinessObjectActivator()
		{
			return new BusinessObjectActivator();
		}

		#endregion

		#endregion

		#region Perform Search

		public IAsyncStrategy AsyncStrategy { get; set; }

		protected internal sealed override void PerformSearch(PerformSearchEventArgs performSearchArgs = null)
		{
			OnBeforePerformSearchCore();
			var startTime = ZDateTime.Now;
			var type = GridCollection.TypeOfElements;
			var query = GetDisplayResultsQuery();
			var searchResult = PerformSearchCore(type, query);
			var endTime = ZDateTime.Now;
			BindSearchResultToGrid(GridCollection, searchResult);
			OnAfterPerformSearch();
			StartOrStopAutoRefreshTimer(endTime - startTime);
		}

		protected internal Task AsyncTask { get; private set; }
		protected internal CancellationTokenSource AsyncTaskCancellationTokenSource { get; private set; }

		protected internal override void PerformSearchAsync(PerformSearchAsyncEventArgs performSearchArgs)
		{
			OnBeforePerformSearchCore();
			if (AsyncTaskCancellationTokenSource != null)
			{
				AsyncTaskCancellationTokenSource.Cancel();
				AsyncTaskCancellationTokenSource = null;
				performSearchArgs?.OnAfterPerformSearchAsync?.Invoke();
				return;
			}

			var grid = Grid; // Ensure that the grid is initialized.
			var type = GridCollection.TypeOfElements;
			var query = GetDisplayResultsQuery();
			performSearchArgs?.OnBeginPerformSearchAsync?.Invoke();

			var cancellationTokenSource = new CancellationTokenSource();
			AsyncTaskCancellationTokenSource = cancellationTokenSource;
			AsyncTask = (AsyncStrategy ?? DefaultAsyncStrategy.Get()).DoAsync(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var stopwatch = new Stopwatch();
					stopwatch.Start();
					var searchResult = PerformSearchCore(type, query);
					stopwatch.Stop();
					searchResult.Factory?.RelinquishThreadOwnership();
					grid.BeginInvokeSafe(new Action(() =>
					{
						if (!cancellationTokenSource.IsCancellationRequested)
						{
							searchResult.Factory?.TakeThreadOwnership();
							performSearchArgs?.OnAfterPerformSearchAsync?.Invoke();
							BindSearchResultToGrid(GridCollection, searchResult);
							OnAfterPerformSearch();
							performSearchArgs?.OnAfterPerformSearch?.Invoke();
							StartOrStopAutoRefreshTimer(stopwatch.Elapsed);
							AsyncTask = null;
							AsyncTaskCancellationTokenSource = null;
						}
					}));
				}
			});
		}

		/// <summary>
		/// This method can be called on both the GUI thread an async thread, so must be thread-safe unless IsModuleAllowAsync return false.
		/// </summary>
		protected virtual PerformSearchResult PerformSearchCore(Type type, ZQuery query)
		{
			if (FilterBusinessObject.SearchType == SearchType.Sql && SecondaryServerConnectionForModuleSearchDetailsProvider.IsSecondaryDbEnabled)
			{
				return LoadCollectionFromSecondaryConnection(type, query);
			}

			return LoadCollectionFromPrimaryConnection(type, query);
		}

		PerformSearchResult LoadCollectionFromSecondaryConnection(Type type, ZQuery query)
		{
			var secondaryServerName = SecondaryServerConnectionForModuleSearchDetailsProvider.CurrentSecondaryServerName;
			var secondaryDbConnection = SecondaryServerConnectionForModuleSearchDetailsProvider.GetDbServer(secondaryServerName);
			secondaryDbConnection ??= Db.NewExtraRestrictedReaderConnection(secondaryServerName, Db.DatabaseName);

			return TryLoadCollectionFromSecondaryConnectionAndUpdateConnectionMap(type, query, secondaryDbConnection, secondaryServerName);
		}

		PerformSearchResult TryLoadCollectionFromSecondaryConnectionAndUpdateConnectionMap(Type type, ZQuery query, DbConnection secondaryDbConnection, string secondaryServerName)
		{
			try
			{
				secondaryDbConnection.EnsureIsOpen();
				SecondaryServerConnectionForModuleSearchDetailsProvider.AddDbServer(secondaryServerName, secondaryDbConnection);
				var factory = new BusinessObjectFactory(secondaryDbConnection) { NameForDebugging = $"[SecondarySQLServer]Module : {ID.Name}" };

				return LoadCollection(factory, type, query);
			}
			catch (Exception ex)
			{
				SecondaryServerConnectionForModuleSearchDetailsProvider.RemoveDbServer(secondaryServerName);

				Globals.Message.ShowWarning(Res.GetString("1C3B8ED5-5FA1-44B6-B0D9-71E7BEB0B402", $"Secondary server \"{secondaryServerName}\" unavailable, will perform search on primary server. Exception Message: {ex.Message}"), Res.GetString("D92A5B0D-6124-426F-B6CD-25A3C2BE5A21", "Secondary Server Unavailable"));
				return LoadCollectionFromPrimaryConnection(type, query);
			}
		}

		PerformSearchResult LoadCollectionFromPrimaryConnection(Type type, ZQuery query)
		{
			var factory = SearchManager.GetNewFactory();
			return LoadCollection(factory, type, query);
		}

		protected virtual IEnumerable<BusinessObject> GetUnsavedBusinessObjectsInMainFactory(Type type, ZQuery query) => null;

		/// <summary>
		/// This method can be called on both the GUI thread an async thread, so must be thread-safe unless IsModuleAllowAsync return false.
		/// </summary>
		protected virtual PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			return SearchManager.PerformSearch(factory, type, query);
		}

		protected void BindSearchResultToGrid(IBusinessObjectCollection collection, PerformSearchResult searchResult)
		{
			try
			{
				IsPerformingSearch = true;
				BindSearchResultToGridCore(collection, searchResult);
			}
			finally
			{
				IsPerformingSearch = false;
			}
		}

		void BindSearchResultToGridCore(IBusinessObjectCollection collection, PerformSearchResult searchResult)
		{
			SuspendGridLayout();
			try
			{
				switch (searchResult.Type)
				{
					case PerformSearchResultType.Success:
					case PerformSearchResultType.MaxRowsExceeded:
					case PerformSearchResultType.LimitedRunError:
						PushItemsIntoCollection(collection, searchResult);
						break;
					case PerformSearchResultType.CustomGridLoad:
						OnCustomGridLoad(collection, searchResult);
						break;

					case PerformSearchResultType.TooManyParameters:
						var message = Res.GetString("FilterStripControl|TooManyParameters", "The incoming request has too many parameters. The server supports a maximum of 2100 parameters. Reduce the number of parameters and resend the request.");
						Globals.Message.ShowWarning(message, Res.GetString("FilterStripControl|SearchResults", "Search Results"));

						break;
					case PerformSearchResultType.QueryTooComplicated:
						Globals.Message.ShowWarning(ZGUIConstants.GetQueryTooComplicatedError(), Res.GetString("FilterStripControl|SearchResults", "Search Results"));
						break;
					case PerformSearchResultType.SqlException258:
						// Do nothing as:
						// The important should have already been done at the time the exception occurred
						break;
					case PerformSearchResultType.MinimumRowSizeExceeds:
						var minimumRowSizeExceedsMessage = Res.GetString("FilterStripControl|MinimumRowSizeExceeds",
							@"The query processor could not produce a query plan because a worktable is required, and its minimum row size exceeds the maximum allowable of 8060 bytes.
A typical reason why a worktable is required is a GROUP BY or ORDER BY clause in the query.
If the query has a GROUP BY or ORDER BY clause, consider reducing the number and/or size of the fields in the clause.
Consider using prefix (LEFT()) or hash (CHECKSUM()) of fields for grouping or prefix for ordering.
Note however that this will change the behavior of the query.");
						Globals.Message.ShowWarning(minimumRowSizeExceedsMessage, Res.GetString("FilterStripControl|SearchResults", "Search Results"));
						break;

					default:
						throw new InvalidOperationException(FormattableString.Invariant($"Unknown result type: {searchResult}"));
				}
			}
			finally
			{
				ForceGridPreFetch();
				ResumeGridLayout();
			}
		}

		protected void PushItemsIntoCollection(IBusinessObjectCollection collection, PerformSearchResult searchResult)
		{
			PushItemsIntoCollectionCore(collection, searchResult, DefaultSortOrder);
			SetNotificationsOnRowsAsAppropriate();
		}

		protected virtual void PushItemsIntoCollectionCore(IBusinessObjectCollection collection, PerformSearchResult searchResult, SortInfo sort)
		{
			SearchManager.PushItemsIntoCollection(collection, searchResult, sort);
		}

		protected virtual void OnCustomGridLoad(IBusinessObjectCollection collection, PerformSearchResult searchResult)
		{
			// Hook for all of the hackers that like to touch the grid collection.
		}

		protected virtual void OnBeforePerformSearchCore()
		{
		}

		void OnAfterPerformSearch()
		{
			HasSearched = true;
			OnAfterPerformSearchCore();

			ObjectFactory.Get<ISearchPerformedUsageCollector>()?.Report(
				FilterBusinessObject.SearchType == SearchType.Index ? SearchPerformedType.ModuleIndexSearch : SearchPerformedType.ModuleSql,
				$"{ID.ID}({ID.Description.GetLocalizedValue(Constants.Languages.English)})",
				string.Join("|", FilterBusinessObject.ActiveModuleFilters.Select(f => f.Description)));
		}

		protected virtual void OnAfterPerformSearchCore()
		{
		}

		void StartOrStopAutoRefreshTimer(TimeSpan queryTime)
		{
			var isSlowQuery = (queryTime.TotalSeconds > AutoRefreshSlowQueryInSeconds);

			if (AutoRefreshManager.Instance.IsAutoRefreshEnabled(ID))
			{
				if (isSlowQuery && previousQueryWasSlow)
				{
					StopAutoRefresh(AutoRefreshWarningType.SlowQuery);
				}
				else
				{
					StartAutoRefresh(AutoRefreshManager.Instance.GetAutoRefreshTimeOut(ID));
				}
			}
			else
			{
				SetAutoRefreshWarning(AutoRefreshWarningType.None);
			}

			previousQueryWasSlow = isSlowQuery;
		}

		void SetAutoRefreshWarning(AutoRefreshWarningType warningType)
		{
			var filterStripControl = EmbeddedControl as ZFilterStripControl;
			if (filterStripControl != null)
			{
				filterStripControl.AutoRefreshWarning = warningType;
			}
		}

		const double AutoRefreshSlowQueryInSeconds = 5;
		bool previousQueryWasSlow;

		#endregion

		#region Workflow Type

		public virtual string WorkflowType
		{
			get { return string.Empty; }
		}

		public Type GetElementType()
		{
			return BusinessObjectCollection.GetElementTypeFromCollectionType(GridCollection.GetType());
		}

		#endregion

		#region Auto-Refresh

		protected void StartAutoRefresh(byte timeout)
		{
			if (!AutoRefreshManager.Instance.IsAutoRefreshEnabled(ID))
			{
				AutoRefreshManager.Instance.SetAutoRefreshTimeOut(ID, true, timeout);
			}
			UpdateAutoRefreshMenuItemText();
			SetAutoRefreshTimerInterval(timeout);
			SetAutoRefreshWarning(AutoRefreshWarningType.None);
			AutoRefreshTimer.Start(); // timer should be started as soon as the user turns it on
			AutoRefreshMenuItem.Checked = true;
		}

		protected void StopAutoRefresh(AutoRefreshWarningType warningType)
		{
			SetAutoRefreshWarning(warningType);
			if (IsAutoRefreshTimerCreated)
			{
				AutoRefreshTimer.Stop();
			}

			AutoRefreshMenuItem.Checked = false;
			if (warningType != AutoRefreshWarningType.SlowQuery)
			{
				AutoRefreshManager.Instance.SetAutoRefreshTimeOut(ID, false, AutoRefreshManager.Instance.GetAutoRefreshTimeOut(ID));
			}
		}

		#region HandleAutoRefreshClick

		protected void HandleAutoRefreshClick(object sender, EventArgs e)
		{
			var autoRefreshModuleGrids = EnvProxy.Instance.Security.AutoRefreshModuleGrids;
			if (autoRefreshModuleGrids.IsAllowed)
			{
				var item = (MenuItem)sender;
				item.Checked = !item.Checked;

				var currentTimeout = AutoRefreshManager.Instance.GetAutoRefreshTimeOut(ID);

				if (item.Checked)
				{
					using (var autoRefreshForm = new AutoRefreshForm(currentTimeout))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(autoRefreshForm) == DialogResult.OK)
						{
							StartAutoRefresh(autoRefreshForm.BizO.AutoRefreshTimeOut);
						}
						else
						{
							item.Checked = false;
						}
					}
				}
				else
				{
					StopAutoRefresh(AutoRefreshWarningType.None);
				}
			}
			else
			{
				autoRefreshModuleGrids.ShowError();
			}
		}

		#endregion

		#region Menu Item

		void UpdateAutoRefreshMenuItemText()
		{
			AutoRefreshMenuItem.Caption = GetAutoRefreshMenuItemText();
		}

		MultilingualString GetAutoRefreshMenuItemText()
		{
			var timeOut = AutoRefreshManager.Instance.GetAutoRefreshTimeOut(ID);
			return ResString.GetMultilingualString("FE10A0EA-BF0B-42f2-8773-587C6D2F8BFD", "Auto &Refresh (every {0})", AutoRefreshManager.Instance.GetTimeoutDescription(timeOut));
		}

		protected ZFilterGridMenuItem AutoRefreshMenuItem
		{
			get
			{
				if (fAutoRefreshMenuItem == null)
				{
					fAutoRefreshMenuItem = new ZFilterGridMenuItem(GetAutoRefreshMenuItemText(), HandleAutoRefreshClick)
					{
						ShowInToolbar = false,
						Checked = AutoRefreshManager.Instance.IsAutoRefreshEnabled(ID)
					};
				}
				return fAutoRefreshMenuItem;
			}
		}

		ZFilterGridMenuItem fAutoRefreshMenuItem;

		#endregion

		#region Timer

		bool IsAutoRefreshTimerCreated
		{
			get { return fAutoRefreshTimer != null; }
		}

		protected System.Windows.Forms.Timer AutoRefreshTimer
		{
			get
			{
				if (fAutoRefreshTimer == null)
				{
					fAutoRefreshTimer = new System.Windows.Forms.Timer();
					fAutoRefreshTimer.Tick += new EventHandler(fAutoRefreshTimer_Tick);
					SetAutoRefreshTimerInterval(AutoRefreshManager.Instance.GetAutoRefreshTimeOut(ID));
				}
				return fAutoRefreshTimer;
			}
		}

		void SetAutoRefreshTimerInterval(byte minutes)
		{
			AutoRefreshTimer.Interval = 60000 * minutes;
		}

		protected void fAutoRefreshTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				if (ConnectionIsAliveOrCanBeRestored(((IDbConnected)Factory).Connection))
				{
					if (ValidateCustomSqlFilter())
					{
						PerformSearch();
					}
					else
					{
						StopAutoRefresh(AutoRefreshWarningType.ErrorQuery);
					}
				}
			}
			catch (DatabaseUpgradeException)
			{
				if (IsAutoRefreshTimerCreated)
				{
					AutoRefreshTimer.Stop();
				}
			}
		}

		bool ValidateCustomSqlFilter()
		{
			if (FilterBusinessObject != null)
			{
				foreach (var moduleFilter in FilterBusinessObject.ActiveModuleFilters)
				{
					var customSqlFilter = moduleFilter as ModuleSQLFilter;
					if (customSqlFilter != null)
					{
						return customSqlFilter.IsValidSql(true);
					}
				}
			}
			return true;
		}

		bool ConnectionIsAliveOrCanBeRestored(IDbReconnectionHandling connection)
		{
			if ((connection.State & ConnectionState.Open) != ConnectionState.Open)
			{
				try
				{
					connection.CloseAndReopenConnection();
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					return false;
				}
			}

			return (connection.State & ConnectionState.Open) == ConnectionState.Open;
		}

		System.Windows.Forms.Timer fAutoRefreshTimer;

		#endregion

		#region AllowAutoRefresh

		public bool AllowAutoRefresh
		{
			get { return AllowAutoRefreshCore; }
		}

		protected virtual bool AllowAutoRefreshCore
		{
			get { return true; }
		}

		#endregion

		#endregion

		#region GetDisplayResultsQuery

		protected override ZQuery GetDisplayResultsQuery()
		{
			var query = base.GetDisplayResultsQuery();
			query.IncludeBlob(GridBlobFields);
			return query;
		}

		HashSet<SchemaColumn> GridBlobFields
		{
			get
			{
				if (gridBlobFields == null)
				{
					gridBlobFields = new HashSet<SchemaColumn>();

					if (Globals.IsUserInteractive && Grid != null)
					{
						foreach (ZGridColumnInfo columnInfo in Grid.ColumnStyles)
						{
							var keyCalculator = new ResourceStringKeyCalculator(Grid, columnInfo.ColumnName);
							if (keyCalculator.ShemaColumnObject != null && keyCalculator.ShemaColumnObject.IsLargeBinaryOrText)
							{
								gridBlobFields.Add(keyCalculator.ShemaColumnObject);
							}
						}
					}
				}
				return gridBlobFields;
			}
		}
		HashSet<SchemaColumn> gridBlobFields;

		#endregion

		void AddUniversalCopyMenuItems(ZMenuItem newMenuItem)
		{
			try
			{
				var copyManager = ObjectFactory.New<IModuleUniversalCopyManager>(this);
				copyManager.AddMenuItems(newMenuItem, HandleNewClick);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ModuleUniversalCopyManager_AddMenuItems", ex);
			}
		}

		protected virtual bool AllowCustomiseColumn
		{
			get { return true; }
		}

		public void RunPreSaveValidation()
		{
			FilterBusinessObject.RunPreSaveValidation();
		}

		public Enterprise.Integration.ZArchitecture.IModuleFilterCollection ModuleFilters { get => FilterBusinessObject.ModuleFilters; }

		internal BusinessObject LoadTop1MatchingFilterInNewFactory()
		{
			return LoadMatchingFilterInNewFactory().FirstOrDefault();
		}

		internal BusinessObject[] LoadMatchingFilterInNewFactory()
		{
			return LoadCollection(this.GetNewFactoryForInternal(), GetElementType(), ExportQuery).LoadedRows;
		}

		#region Template record

		bool IZFilterGridModule.AllowTemplateRecords => SupportTemplateRecords;

		protected virtual bool SupportTemplateRecords => false;

		public bool AllowLoadTemplateRecords { get; set; }

		internal static bool IsTemplateRecord(IBusiness bizo)
		{
			var templateRecordProvider = bizo as ITemplateRecordProvider;
			return templateRecordProvider?.IsTemplateRecord ?? bizo is ITemplateRecord;
		}

		internal static BusinessObject GetTemplateRecord(IBusiness bizo)
		{
			var templateRecordProvider = bizo as ITemplateRecordProvider;
			if (templateRecordProvider != null)
			{
				return templateRecordProvider.TemplateRecord as BusinessObject;
			}

			var templateRecord = bizo as ITemplateRecord;
			if (templateRecord != null)
			{
				return templateRecord as BusinessObject;
			}

			return null;
		}

		protected virtual ResourceStringData NewTemplateRecordText
		{
			get
			{
				return Res.GetData("ModuleGrid.NewTemplateRecord", "New Template Record", "Creates a new template record.");
			}
		}

		void AddNewTemplateRecordMenuItem()
		{
			if (!SupportTemplateRecords || !AllowLoadTemplateRecords)
			{
				return;
			}

			if (NewMenuItem.MenuItems.Count == 0)
			{
				var newItem = new ZMenuItem(NewMenuItem.CaptionResourceString, HandleNewClick, NewMenuItem.ActiveIcon, NewMenuItem.RestIcon);
				NewMenuItem.MenuItems.Add(newItem);
			}

			var newTemplateRecordMenuItem = new ZMenuItem(NewTemplateRecordText, HandleNewTemplateRecordClick);
			NewMenuItem.MenuItems.Add(newTemplateRecordMenuItem);
		}

		void HandleNewTemplateRecordClick(object sender, EventArgs e)
		{
			ShowNewFormCore(GetNewTemplateRecordBusinessObject);
		}

		BusinessObject GetNewTemplateRecordBusinessObject()
		{
			if (!SupportTemplateRecords)
			{
				throw new InvalidOperationException(string.Format(Culture.Current, "{0} does not support template records", GetType().FullName));
			}

			var newBusinessObject = GetNewTemplateRecordBusinessObjectCore();
			if (newBusinessObject == null)
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("c4884e11-bf97-470f-8922-8f3312c42d00", "New template record cannot be created."));
				throw new InvalidOperationException("GetNewTemplateRecordBusinessObjectCore() returned null");
			}

			return newBusinessObject;
		}

		protected virtual BusinessObject GetNewTemplateRecordBusinessObjectCore()
		{
			return null;
		}

		public BusinessObject LoadFromTemplateRecordPk(BusinessObjectFactory localFactory, ZGuid templateRecordPk)
		{
			if (!SupportTemplateRecords)
			{
				throw new InvalidOperationException(string.Format(Culture.Current, "{0} does not support template records", GetType().FullName));
			}

			if (GridCollection != null && GridCollection.TypeOfElements != null)
			{
				var typeOfElements = GridCollection.TypeOfElements;
				var cachedBizo = ((IBusinessObjectFactoryInternals)localFactory).AllBusinessObjects
					.Where(typeOfElements.IsInstanceOfType)
					.FirstOrDefault(bizo => bizo is ITemplateRecordProvider templateRecordProvider && templateRecordProvider.IsTemplateRecord &&
						templateRecordProvider.TemplateRecord is IIdentified templateRecord && templateRecord.Identifier == templateRecordPk);
				if (cachedBizo != null)
				{
					return cachedBizo;
				}
			}

			return LoadFromTemplateRecordPkCore(localFactory, templateRecordPk);
		}

		protected virtual BusinessObject LoadFromTemplateRecordPkCore(BusinessObjectFactory localFactory, ZGuid templateRecordPk)
		{
			return null;
		}

		protected override bool CanReloadWithFilter(BusinessObjectFactory newFactory, BusinessObject selectedBusinessObject, ZQuery filter)
		{
			if (SupportTemplateRecords && selectedBusinessObject is ITemplateRecordProvider templateRecordProvider && templateRecordProvider.IsTemplateRecord)
			{
				return true;
			}

			return base.CanReloadWithFilter(newFactory, selectedBusinessObject, filter);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (fAutoRefreshTimer != null)
				{
					fAutoRefreshTimer.Dispose();
				}

				if (fAutoRefreshMenuItem != null)
				{
					fAutoRefreshMenuItem.Dispose();
				}

				if (fImportExportDataToolBarButton != null)
				{
					fImportExportDataToolBarButton.Dispose();
				}

				if (plugins != null)
				{
					plugins.Dispose();
				}
			}
			base.Dispose(isDisposing);
		}

		#endregion

		#region Testing Only
#if DEBUG

		ZDisplayGrid IFilterGridModuleInternalsForTesting.Grid
		{
			get { return Grid; }
		}

		ZQuery IFilterGridModuleInternalsForTesting.GetDisplayResultsQuery()
		{
			return GetDisplayResultsQuery();
		}

		void IFilterGridModuleInternalsForTesting.ClearGridBlobFields()
		{
			gridBlobFields = null;
		}

#endif
		#endregion
	}
}
