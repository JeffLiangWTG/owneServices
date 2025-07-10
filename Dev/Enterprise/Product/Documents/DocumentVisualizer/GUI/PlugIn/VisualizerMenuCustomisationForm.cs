using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.DocumentEngine.GUI.ReflectiveFieldMap;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Business.Reflection;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.GUI
{
	public partial class VisualizerMenuCustomisationForm : MenuCustomisationForm
	{
		public VisualizerMenuCustomisationForm(VisualizerMenuCustomisation menuCustomisation)
			: base(menuCustomisation)
		{
			this.menuCustomisation = menuCustomisation;

			InitializeComponent();

			Init();
		}

		readonly VisualizerMenuCustomisation menuCustomisation;

		protected override string MenuGridCaption
		{
			get { return Res.GetString("VisualizerMenuCustomisationForm|MenuGridCaption", "Forms"); }
		}

		void Init()
		{
			menuDetailsTab.TabVisible = false;

			PivotAndChildMenuTabControl.TabPages.Remove(visualizerMenuDetailsTab);
			PivotAndChildMenuTabControl.TabPages.Insert(visualizerMenuDetailsTab, 0);

			if (DesignModeFinder.IsDesigning)
			{
				return;
			}

			if (!GlbStaff.CurrentUser.GS_IsDeveloper)
			{
				visualizerMenuPathTextBox.IsLanguageEditingEnabled = false;

				MenusGrid.AfterBind += delegate
				{
					DisableMenusGridLanguageEditingForNonDeveloper();
				};
			}

			var isVisibleOnWebColumn = new ZCheckBoxColumnStyleInfo();
			isVisibleOnWebColumn.CaptionResourceString = Res.GetData("c82b3e89-96b4-4168-bfdc-5948c111e7a9", "Web Visible", "Is Web Visible", "Specifies the visibility of this menu.");
			isVisibleOnWebColumn.ColumnName = "SU_IsVisibleOnWeb";
			isVisibleOnWebColumn.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref isVisibleOnWebColumn, 45, true);
			MenusGrid.ColumnStyles.Add(isVisibleOnWebColumn);

			var menuPathColumn = new ZTranslatableTextBoxColumnStyleInfo();
			menuPathColumn.CaptionResourceString = Res.GetData("7eed7c72-7c84-4b60-a372-6bfc3b5e7f62", "Menu Path");
			menuPathColumn.ColumnName = "SU_MenuPath";
			ControlDpiScalingHelper.SetWidth(ref menuPathColumn, 190, true);

			this.MenusGrid.ColumnStyles.Add(menuPathColumn);

			availableTemplatesGrid.ColumnStyles.Clear();

			var templateNameColumn = new ZTextBoxColumnStyleInfo();
			templateNameColumn.CaptionResourceString = Res.GetData("d67ea124-cbe7-44bb-97dd-25cd5c7c4b2d", "Template Name");
			templateNameColumn.ColumnName = StmTemplateSchema.Constants.SO_Name;
			templateNameColumn.IsReadOnly = false;
			ControlDpiScalingHelper.SetWidth(ref templateNameColumn, 180, true);
			availableTemplatesGrid.ColumnStyles.Add(templateNameColumn);

			var isSystemColumn = new ZCheckBoxColumnStyleInfo();
			isSystemColumn.CaptionResourceString = Res.GetData("A1074499-1113-4C93-B524-C51AB03536C0", "System");
			isSystemColumn.ColumnName = StmTemplateSchema.Constants.SO_IsSystemDefined;
			isSystemColumn.IsReadOnly = false;
			ControlDpiScalingHelper.SetWidth(ref templateNameColumn, 180, true);
			availableTemplatesGrid.ColumnStyles.Add(isSystemColumn);

			var dataContextColumn = new ZTextBoxColumnStyleInfo();
			dataContextColumn.CaptionResourceString = Res.GetData("08158b4f-8da2-43a7-ba87-9ed5027f6479", "Data Context");
			dataContextColumn.ColumnName = StmTemplateSchema.Constants.SO_DataContext;
			dataContextColumn.IsReadOnly = true;
			dataContextColumn.IsVisible  = false;
			ControlDpiScalingHelper.SetWidth(ref dataContextColumn, 180, true);
			availableTemplatesGrid.ColumnStyles.Add(dataContextColumn);

			var dataStoreNameColumn = new ZTextBoxColumnStyleInfo();
			dataStoreNameColumn.CaptionResourceString = Res.GetData("773f22bc-f5e1-4172-8ff2-69b0b1878246", "Data Store Name");
			dataStoreNameColumn.ColumnName = StmMenuTemplatePivotSchema.Constants.SI_DataStoreName;
			dataStoreNameColumn.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref dataStoreNameColumn, 120, true);

			var templateFilterColumn = new ZTextBoxColumnStyleInfo();
			templateFilterColumn.CaptionResourceString = Res.GetData("771b1029-4e64-44e4-ae64-1c4908ce351e", "Filter");
			templateFilterColumn.ColumnName = StmMenuTemplatePivotSchema.Constants.SI_MenuTemplateFilter;
			templateFilterColumn.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref templateFilterColumn, 120, true);

			var templatePrintCopyTypeColumn = new ZDropEditColumnStyleInfo();
			templatePrintCopyTypeColumn.CaptionResourceString = Res.GetData("c340c14d-e165-475d-9f8f-0feec917f863", "Type");
			templatePrintCopyTypeColumn.ColumnName = StmMenuTemplatePivotSchema.Constants.SI_PrintCopyType;
			templatePrintCopyTypeColumn.BindToList = "PrintCopyTypeList";
			templatePrintCopyTypeColumn.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref templatePrintCopyTypeColumn, 120, true);

			var templateIndexColumn = new ZCalcEditColumnStyleInfo();
			templateIndexColumn.CaptionResourceString = Res.GetData("e1e17d23-4888-4dfb-ba06-0534bb4fc3b9", "Index");
			templateIndexColumn.ColumnName = StmMenuTemplatePivotSchema.Constants.SI_Index;
			templateIndexColumn.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref templateIndexColumn, 20, true);

			TemplatesUsedGrid.ColumnStyles.Add(dataStoreNameColumn);
			TemplatesUsedGrid.ColumnStyles.Add(templateIndexColumn);
			TemplatesUsedGrid.ColumnStyles.Add(templatePrintCopyTypeColumn);
			TemplatesUsedGrid.ColumnStyles.Add(templateFilterColumn);

			if (menuCustomisation.Parent != null)
			{
				var filterEvaluatorMenuItem = new ZMenuItem(
					ResString.GetMultilingualString("3797B33E-F6E9-4063-98AD-C2A15FA3192E", "Show Macro Evaluator"),
					RunFilterEvaluator);

				MenusGrid.ContextMenu.MenuItems.Add(0, filterEvaluatorMenuItem);

				var sourceMapMenuItem = new ZMenuItem(Res.GetData("6D3CB5C5-FD8A-446A-A903-8F655890A2C5", "Data Source Map"), (s, e) => CreateMapTreeForm());
				availableTemplatesGrid.ContextMenu.MenuItems.Add(sourceMapMenuItem);
			}
		}

		void CreateMapTreeForm()
		{
			var template = CurrentTemplate as StmTemplate;
			var message = string.Empty;

			var parent = menuCustomisation.Parent;
			var parameters = new DocDataObjectParameters(string.Empty, string.Empty);
			var docDataObject = parent.GetSupporter()?.GetDocDataObject(parent, template.SO_DataContext, parameters);

			if (template.SO_DataContext == Integration.DataContext.UXML)
			{
				message = Res.GetString("1e226bf5-b087-4447-bd54-2f5a057ee67b", "DataContext '{0}' is not supported.", template.SO_DataContext);
			}
			else if (!docDataObject.HasValue)
			{
				message = Res.GetString("aa17c2d9-bdc9-4014-8f62-d492f3bf40cb", "Data Field Map is currently not available on this module.");
			}
			else if (docDataObject.Value.IsLeft)
			{
				message = docDataObject.Value.Left;
			}
			else
			{
				var reflector = new VisualizerDocDataProviderReflector(docDataObject.Value.Right.GetType());
				var wrapper = new DataReflectorValueProviderWrapper(new[] { reflector }, new VisualizerValueProviderMap(), DataReflectorValueProviderWrapper.Mode.Browse);

				var form = new MapTreeForm(wrapper);
				form.macrosTab.TabVisible = false;
				form.Show();
			}

			if (!string.IsNullOrWhiteSpace(message))
			{
				Globals.Message.ShowInformation(message);
			}
		}

		void DisableMenusGridLanguageEditingForNonDeveloper()
		{
			foreach (var columnName in new[]
			{
				StmMenuItemSchema.Constants.SU_MenuName,
				StmMenuItemSchema.Constants.SU_Hint,
				StmMenuItemSchema.Constants.SU_MenuPath
			})
			{
				var menuPathColumnStyle = this.MenusGrid.Columns[columnName].ColumnStyle as ZTranslatableTextBoxColumnStyle;
				if (menuPathColumnStyle?.EditControl != null)
				{
					((ZTranslatableTextControl)menuPathColumnStyle.EditControl).IsLanguageEditingEnabled = false;
				}
			}
		}

		void RunFilterEvaluator(object sender, EventArgs e)
		{
			var menuItem = CurrentMenu as VisualizerMenuItem;

			if (menuItem == null)
			{
				return;
			}

			var scope = menuCustomisation.Parent.CreateFilterEvaluatorScope();

			var documentToolsService = new DocumentToolsService();

			documentToolsService.ShowMacroEvaluator(
				FilterExtensions.FilterContext,
				scope,
				menuItem.SU_FilterList);
		}

		protected override void RunMenuItemClickHandler(object sender, EventArgs e)
		{
			if (CurrentMenu is VisualizerMenuItem menuItem)
			{
				var moduleId = GetModuleIdentifier();
				var command = menuCustomisation.Parent.GetVisualizableDocumentCommand(menuItem, moduleId);

				command.Execute();
			}
		}

		ModuleIdentifier GetModuleIdentifier()
		{
			ModuleIdentifier moduleIdentifier = null;

			if (menuCustomisation.Parent is IControllerIDProvider controllerIdProvider)
			{
				var controllerID = controllerIdProvider.ControllerID;

				var controller = controllerID != null
					? ZControllerFactory.Create(controllerID)
					: null;

				moduleIdentifier = controller?.ModuleIDForDocumentSecurity;
			}

			return moduleIdentifier;
		}
	}
}
