using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.DocBuilder.SectionEditing;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.GUI.DocBuilder;
using Enterprise.DocumentEngine.GUI.DocumentDelivery;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	public abstract partial class MenuCustomisationForm : ZForm, IMenuCustomisationForm, IButtonPostTextOverride, IButtonCloseTextOverride
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		const string SystemDocumentElementsName = "System Document Elements";

		protected MenuCustomisationForm(MenuCustomisation businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();

			RegisterEventHandlers();

			MenusGrid.AfterBind += delegate
			{ MenusGrid.ListManager.PositionChanged += delegate { TrackGridsWhenMenuGridPositionChanges(); }; };
			TemplatesUsedGrid.AfterBind += delegate
			{ OnTemplatesUsedGridAfterBind(); };

			ZFormPostingButtonsStrategy.SetupPosting(this, zPostingButtonsUserControl1);

			HideEditMenuItems();

			businessEntity.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(BusinessEntity_HasChangesChanged);

			MenuItem runMenuItem = new ZMenuItem(ResString.GetMultilingualString("48909A95-B542-44F8-B93D-8FD9EA0F537A", "Run"), RunMenuItemClickHandler);
			MenusGrid.ContextMenu.MenuItems.Add(0, runMenuItem);

			menusGroupBox.Text = MenuGridCaption;

			AddUseReportWriterMenuItemForTemplate();
			AddSaveAsMenuItemForTemplate();
			AddCopySectionContextMenuItemToTemplatesGrid();
			AddTemplateHistoryMenuItem();
			menuTemplatesBottomWithReportPanel.Visible = EnableReportWriter;
			menuTemplatesBottomNoReportPanel.Visible = !menuTemplatesBottomWithReportPanel.Visible;

#if DEBUG
			SetUpForDebugMode();
#endif

			((ZDropEditInternals)primaryDocumentPicker).SetControlWidth(323);
		}

		internal bool isAvailableTemplatesDragging;
		internal bool isTemplatesUsedGridDragging;
		void RegisterEventHandlers()
		{
			availableTemplatesGrid.AllowDrop = true;
			availableTemplatesGrid.MouseDown += new MouseEventHandler(availableTemplatesGrid_MouseDown);
			availableTemplatesGrid.MouseUp += new MouseEventHandler(availableTemplatesGrid_MouseUp);

			TemplatesUsedGrid.AllowDrop = true;
			TemplatesUsedGrid.MouseDown += new MouseEventHandler(TemplatesUsedGrid_MouseDown);
			TemplatesUsedGrid.MouseUp += new MouseEventHandler(TemplatesUsedGrid_MouseUp);
			TemplatesUsedGrid.DragDrop += new DragEventHandler(TemplatesUsedGrid_DragDrop);
			TemplatesUsedGrid.DragEnter += new DragEventHandler(TemplatesUsedGrid_DragEnter);
		}
		#region Template Drag & Drop
		#region availableTemplatesGrid EventHandler

		internal void availableTemplatesGrid_MouseUp(object sender, MouseEventArgs e)
		{
			isAvailableTemplatesDragging = false;
		}

		internal void availableTemplatesGrid_MouseDown(object sender, MouseEventArgs e)
		{
			isAvailableTemplatesDragging = true;
		}

		#endregion

		#region TemplatesUsedGrid EventHandler

		internal void TemplatesUsedGrid_MouseUp(object sender, MouseEventArgs e)
		{
			isTemplatesUsedGridDragging = false;
		}

		internal void TemplatesUsedGrid_MouseDown(object sender, MouseEventArgs e)
		{
			isTemplatesUsedGridDragging = true;
		}

		internal void TemplatesUsedGrid_DragDrop(object sender, DragEventArgs e)
		{
			if (isAvailableTemplatesDragging && !isTemplatesUsedGridDragging)
			{
				AddTemplatePivot();
			}
			isTemplatesUsedGridDragging = false;
		}

		internal void TemplatesUsedGrid_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
		}

		#endregion
		#endregion
		protected override bool AllowNew
		{
			get { return false; }
		}

		void AddTemplateHistoryMenuItem()
		{
			var viewDataVersionLogsMenuItem = new ZMenuItem(ResString.GetMultilingualString("92ea6507-c026-4c76-b55e-1a009ec0c9f0", "&View Template History..."), TemplateHistoryMenuItem_Click);
			availableTemplatesGrid.ContextMenu.MenuItems.Add(viewDataVersionLogsMenuItem);
		}

		void TemplateHistoryMenuItem_Click(object sender, EventArgs e)
		{
			var template = CurrentTemplate;
			if (template != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new TemplateHistoryForm(template));
			}
		}

		void AddSaveAsMenuItemForTemplate()
		{
			var menuItem = CreateSaveAsMenuItem(() => { return availableTemplatesGrid.ListManager.GetCurrent() as StmTemplate; });
			var contextMenu = availableTemplatesGrid.ContextMenu;
			contextMenu.MenuItems.Add(0, menuItem);
		}

		void AddUseReportWriterMenuItemForTemplate()
		{
			if (EnableReportWriter)
			{
				var menuItem = CreatedUseReportWriterMenuItem();
				var contextMenu = availableTemplatesGrid.ContextMenu;
				contextMenu.MenuItems.Add(0, menuItem);
			}
		}

		bool EnableReportWriter
		{
			get { return SystemDataRegistry.Instance.EnableReportWriter.Value; }
		}

		MenuItem CreatedUseReportWriterMenuItem()
		{
			var result = new ZMenuItem(ResString.GetMultilingualString("{63E96610-207A-4F1F-8FC6-DBA825A0FFA7}", "Use Report Writer..."), new EventHandler((sender, e) =>
			{
				var template = CurrentTemplate;
				if (template != null)
				{
					ZFormModaliser.ShowDialogAndDispose(new ReportWriter.ReportWriterForm(new Enterprise.ExcelTemplates.ExcelTemplateReadFromStmTemplateTable(template)));
				}
			}));

			return result;
		}

		static MenuItem CreateSaveAsMenuItem(Func<StmTemplate> getTemplate)
		{
			var result = new ZMenuItem(ResString.GetMultilingualString("8f632093-5374-4bea-bf89-fe36e6704944", "Save As..."), new EventHandler((sender, e) =>
			{
				var template = getTemplate();
				if (template != null)
				{
					using (var dialog = new ZSaveFileDialog())
					{
						var filename = Path.GetFileName(template.SO_ExcelTemplatePath);
						var extension = Path.GetExtension(filename);
						if (string.IsNullOrEmpty(extension))
						{
							//Assume format is XLS if we couldn't retrieve the extension
							extension = AttachmentTypeList.Codes.Xls;
						}
						if (string.IsNullOrEmpty(filename))
						{
							//Use template name if filename is empty
							filename = MakeFilenameSafe.MakeSafe(template.SO_Name + extension, '_');
						}
						dialog.FileName = filename;
						dialog.Filter = (NoResString)"Excel Templates|*" + extension;
						dialog.OverwritePrompt = true;

						if (dialog.ShowDialog() == DialogResult.OK)
						{
							using (var stream = dialog.OpenFile())
							{
								stream.Write(template.SO_Template, 0, template.SO_Template.Length);
								Globals.Message.Show(ResString.GetMultilingualString("c290e692-999c-4618-83fc-6a8a4285a03b", "Template has been saved to {0}.", dialog.UnmappedFileName), ResString.GetMultilingualString("aba66940-49ee-4f0e-98ef-5f06126dbd36", "Save Successful"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
							}
						}
					}
				}
			}));

			return result;
		}

		void AddCopySectionContextMenuItemToTemplatesGrid()
		{
			var contextMenu = availableTemplatesGrid.ContextMenu;
			var customizeMenuItemText = ResString.GetMultilingualString("46ab32db-e598-4ce8-ac54-d6fdeac33d21", "Copy Section for Customization");
			var customizeMenuItem = new ZMenuItem(customizeMenuItemText, new EventHandler((sender, e) =>
			{
				var selectedTemplate = availableTemplatesGrid.ListManager.GetCurrent() as StmTemplateBase;

				if (selectedTemplate != null)
				{
					if (selectedTemplate.IsDocBuilderStyle)
					{
						var manager = new CustomizeSectionManager(BusinessEntity.Factory, selectedTemplate.DocBuilderLanguageCode);
						var form = new CustomizeSectionForm(manager);

						ZFormModaliser.ShowDialogAndDispose(form);

						if (form.IsAtLeastOneSectionCopied)
						{
							var customizedTemplate = StmTemplateBase.GetDocBuilderTemplate(manager.Factory, DocBuilderTemplateType.Customized, manager.Language);
							if (customizedTemplate != null)
							{
								BusinessEntity.AvailableTemplates.Load();

								var position = availableTemplatesGrid.ListManager.List.IndexOf(customizedTemplate);
								availableTemplatesGrid.ListManager.Position = position;
							}
						}
					}
				}
			}));

			contextMenu.MenuItems.Add(0, customizeMenuItem);
			contextMenu.Popup += (sender, e) =>
			{
				customizeMenuItem.Visible = false;

				var selectedTemplate = availableTemplatesGrid.ListManager.GetCurrent() as StmTemplateBase;
				if (selectedTemplate != null)
				{
					if (selectedTemplate.IsDocBuilderStyle)
					{
						customizeMenuItem.Visible = true;
					}
				}
			};
		}

		protected virtual void RunMenuItemClickHandler(object sender, EventArgs e)
		{
			try
			{
				var hasExtraDialogFormOpened = false;
				using (ZFormModaliser.SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs(ProcessForm))
				using ((CurrentMenu as DocumentCommand)?.Parent?.DocumentSupporter.InitialiseFetchStrategy())
				{
					var printTask = BusinessEntity.GetPrintTask(CurrentMenu);
					printTask.Run(Env.Security.None, true);
				}

				void ProcessForm(object form)
				{
					if (!(form is DocDeliveryForm))
					{
						hasExtraDialogFormOpened = true;
					}
					else if (hasExtraDialogFormOpened)
					{
						((IDocumentDeliveryView)form).HideBackgroundDeliveryCheckBox();
					}
				}
			}
			catch (ExcelInterfaceException exception)
			{
				switch (exception.Type)
				{
					case ExcelInterfaceExceptionType.TooManyCellStyles:
						Globals.Message.ShowError(ErrorMessageTooManyCellFormats);
						break;

					default:
						throw;
				}
			}
			catch (InvalidMenuTemplateFilterException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		internal static string ErrorMessageTooManyCellFormats
		{
			get
			{
				return ResString.GetMultilingualString("59fffd25-30d8-417f-bc8b-9d6e754d1363", @"Template contains over 4,000 different combinations of cell formats.

To resolve this problem, simplify the formatting in the workbook. For example, the following are suggestions for simplifying formatting:

 - Use a standard font.
 - Using the same font for all cells reduces the number of formatting combinations. 
 - If you use borders in a worksheet, use them consistently.");
			}
		}

		protected MenuCustomisationForm()
		{
			InitializeComponent();
		}

		void emailSubjectTextBoxExpandButton_Click(object sender, EventArgs e)
		{
			emailSubjectTextBox.Focus();
			var form = new ZTextBoxPopupForm(emailSubjectTextBox);
			if (emailSubjectTextBox.ReadOnly)
			{
				form.SetReadOnlyIncludingChildren();
			}
			ZFormModaliser.Show(form, this);
		}

#if DEBUG
		internal DummyTemplateEditor dummyTemplateEditorForTesting;

		internal bool testingGridColour;

		internal string testTemplateFilePath;

		internal bool ResizeRedrawForTesting
		{
			get { return base.ResizeRedraw; }
			set { base.ResizeRedraw = value; }
		}
#endif

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.HasChangesChanged -= new EventHandler<HasChangesChangedEventArgs>(BusinessEntity_HasChangesChanged);
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Form Setup

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void HideEditMenuItems()
		{
			ActionsMenuItem.Visible = false;

			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.FileNewMenuItemName, false);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.FileDeleteMenuItemName, false);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.FileSeperator2MenuItemName, false);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.FileSeperator3MenuItemName, false);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.FileSeperator1MenuItemName, false);
		}

		protected virtual string MenuGridCaption
		{
			get { return string.Empty; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode)
			{
				TrackGridsWhenMenuGridPositionChanges();
			}
		}

		void PivotAndChildMenuTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			TrackGridsWhenMenuGridPositionChanges();
		}

		#endregion

		#region Elements

		public new MenuCustomisation BusinessEntity
		{
			get { return (MenuCustomisation)base.BusinessEntity; }
		}

		protected StmMenuItemBase CurrentMenu
		{
			get { return GetCurrentlySelectedElement<StmMenuItemBase>(MenusGrid); }
		}

		protected internal IEnumerable<StmTemplateBase> CurrentTemplates
		{
			get { return GetCurrentlySelectedElements<StmTemplateBase>(availableTemplatesGrid); }
		}

		protected internal StmTemplateBase CurrentTemplate
		{
			get { return GetCurrentlySelectedElement<StmTemplateBase>(availableTemplatesGrid); }
		}

		internal StmMenuTemplatePivotBase CurrentTemplatePivot
		{
			get { return GetCurrentlySelectedElement<StmMenuTemplatePivotBase>(TemplatesUsedGrid); }
		}

		internal IEnumerable<StmMenuTemplatePivotBase> CurrentTemplatePivots
		{
			get { return GetCurrentlySelectedElements<StmMenuTemplatePivotBase>(TemplatesUsedGrid); }
		}

		protected void AddGridTrackingEvents(ZGrid grid, EventHandler handler)
		{
			grid.Enter += handler;
			grid.ListManager.PositionChanged += handler;
		}

		protected T GetCurrentlySelectedElement<T>(ZGrid grid) where T : BusinessObject
		{
			T result = null;
			if ((grid.ListManager != null))
			{
				result = (T)grid.ListManager.GetCurrent();
			}
			return result;
		}

		protected IEnumerable<T> GetCurrentlySelectedElements<T>(ZGrid grid) where T : BusinessObject
		{
			IEnumerable<T> result = grid.SelectedElements.Cast<T>();
			if (!result.Any())
			{
				return new T[] { GetCurrentlySelectedElement<T>(grid) };
			}
			return result;
		}

		protected virtual void OnTemplatesUsedGridAfterBind()
		{
			AddGridTrackingEvents(TemplatesUsedGrid, delegate
			{ TrackTemplatesUsedGrid(); });
		}

		void SelectTemplate(StmTemplateBase template)
		{
			availableTemplatesGrid.SelectSingleElement(template);
		}

		protected virtual void TrackGridsWhenMenuGridPositionChanges()
		{
			if (PivotAndChildMenuTabControl.SelectedTab == menuTemplatesTabPage)
			{
				TrackTemplatesUsedGrid();
			}
		}

		protected virtual void TrackTemplatesUsedGrid()
		{
			StmMenuTemplatePivotBase pivot = CurrentTemplatePivot;
			if (pivot != null)
			{
				var template = pivot.Template;
				if (template != null)
				{
					if (template.IsDocBuilderStyle && CurrentTemplate != null && CurrentTemplate.IsDocBuilderStyle)
					{
						return;
					}

					SelectTemplate(template);
				}
			}
		}

		#endregion

		#region Templates

		protected virtual void AddTemplatePivot()
		{
			if (CurrentTemplates.Any())
			{
				var pivots = new List<StmMenuTemplatePivotBase>();
				CurrentTemplates.ForEach(template =>
				{
					pivots.Add(BusinessEntity.AddTemplatePivot(CurrentMenu, template));
				});
				if (pivots.Any())
				{
					TemplatesUsedGrid.UnSelectAll();
					pivots.ForEach(pivot =>
					{
						TemplatesUsedGrid.Select(TemplatesUsedGrid.List.IndexOf(pivot));
					});
					TemplatesUsedGrid.ListManager.Position = TemplatesUsedGrid.List.IndexOf(TemplatesUsedGrid.SelectedElements.First());
				}
			}
		}

		void AddTemplatePivotButton_Click(object sender, EventArgs e)
		{
			AddTemplatePivot();
		}

		void CopyTemplateButton_Click(object sender, EventArgs e)
		{
			var copier = new MenuCustomizationTemplateCopier(BusinessEntity);
			var copiedTemplate = copier.CopyTemplate(CurrentTemplate);
			if (copiedTemplate != null)
			{
				SelectTemplate(copiedTemplate);
			}
		}

		void EditTemplateButton_Click(object sender, EventArgs e)
		{
			StmTemplateBase template = CurrentTemplate;

#if DEBUG
			if (ControlDpiScalingHelper.DpiX != ControlDpiScalingHelper.BaseDpiX || ControlDpiScalingHelper.DpiY != ControlDpiScalingHelper.BaseDpiY)
			{
				Globals.Message.ShowInformation("Templates should be edited with Windows scaling set to 100%. Please revert to scaling 100%, log off and log back in, then edit the template.", "Scaling Issue");
			}
#endif

			if (template != null)
			{
				if (!template.CanModifyExcelTemplate)
				{
					Globals.Message.ShowWarning(ResString.GetMultilingualString("a7278451-5282-4238-8986-f4450679e36b", "This template is read-only, so it will be shown in Excel in read-only mode.\r\n\r\nAny changes you make will not be saved back to {0}.", Core.Constants.ProductName),
						ResString.GetMultilingualString("fe02feb5-4d1a-401a-9622-9e10f06134b1", "Warning"));
				}
				var editor = GetNewTemplateEditor(template);
				editor.Edit();
			}
			else
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("f3f3e1f5-bb18-4e4f-97ee-8edcd34b162f", "You need to select a template record before using the Edit button."),
					ResString.GetMultilingualString("6d9e76a1-6896-4d33-950e-e6141f891ab0", "Error"));
			}
		}

		ITemplateEditor GetNewTemplateEditor(StmTemplateBase template)
		{
#if DEBUG
			if (dummyTemplateEditorForTesting != null)
			{
				dummyTemplateEditorForTesting.Template = template;
				return dummyTemplateEditorForTesting;
			}
#endif

			return new TemplateEditor(template, this);
		}

		void LoadTemplateButton_Click(object sender, EventArgs e)
		{
			string errorCaption = ResString.GetMultilingualString("23b7e0c0-252d-452d-b898-878aa30efa7b", "Error Loading Template");
			string warningCaption = ResString.GetMultilingualString("D7600A69-5A24-4F24-8398-3F374DE18F09", "Warning Loading Template");
			StmTemplateBase template = CurrentTemplate;

			if ((template != null) && template.CanModifyExcelTemplate)
			{
				string templateFilePath = null;

#if DEBUG
				if (!template.SO_ExcelTemplatePath.IsEmpty)
				{
					string lastTemplateFilePath = template.ExcelTemplateFullPath;
					if (File.Exists(lastTemplateFilePath))
					{
						if (Globals.Message.Show("Do you want to re-load from '" + lastTemplateFilePath + "'? Click 'No' to select another file.", "Load Template", MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes)
						{
							templateFilePath = lastTemplateFilePath;
						}
					}
				}
#endif

				if (templateFilePath == null)
				{
					templateFilePath = PromptForTemplateFile();
				}

				if (templateFilePath != null)
				{
					using (ZOpenFileDialog.ForceLocalFile(ref templateFilePath))
					{
						WaitableActionInvoker.Invoke<TemplateFileReadException>(errorCaption, this, delegate
						{
							ReturnResult result = BusinessEntity.UpdateTemplateRecord(template, templateFilePath);
							if (!string.IsNullOrEmpty(result.Message))
							{
								if (result.Success)
								{
									Globals.Message.ShowWarning(result.Message, warningCaption);
								}
								else
								{
									Globals.Message.ShowError(result.Message, errorCaption);
								}
							}
						});
					}
				}
			}
			else
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("bf396b0e-c948-4e96-8433-24f23d9dc9bf", "You need to select a template record that is not grayed out before using Load button."), errorCaption);
			}
		}

		void NewTemplateButton_Click(object sender, EventArgs e)
		{
			AddNewTemplate(PromptForTemplateFile());
		}

		void AddNewTemplate(string templateFilePath)
		{
			if (!string.IsNullOrEmpty(templateFilePath))
			{
				using (ZOpenFileDialog.ForceLocalFile(ref templateFilePath))
				{
					WaitableActionInvoker.Invoke<TemplateFileReadException>(ResString.GetMultilingualString("e4cad456-30ba-40cd-ac54-f78165aa01a1", "Error Opening Template"), this, delegate
					{
						ReturnResult<StmTemplateBase> result = BusinessEntity.AddNewTemplate(templateFilePath);
						if (result.Success)
						{
							SelectTemplate(result.Value);
						}
						else
						{
							Globals.Message.ShowError(result.Message, ResString.GetMultilingualString("d585bf16-f886-4d37-8664-c2229f9ad733", "Template Error"));
						}
					});
				}
			}
		}

		void ReportWriterTemplateButton_Click(object sender, EventArgs e)
		{
			var form = ReportWriter.ReportWriterForm.NewFromTemplate();
			var mainBizObj = form.BusinessEntity;
			ZFormModaliser.ShowDialogAndDispose(form);
			if (!mainBizObj.Filename.IsEmpty && File.Exists(mainBizObj.Filename))
			{
				AddNewTemplate(mainBizObj.Filename);
			}
		}

		string PromptForTemplateFile()
		{
			string result;

			using (var fileDialog = new ZOpenFileDialog())
			{
				fileDialog.DefaultExt = (NoResString)"xls";
				fileDialog.Filter = (NoResString)"Excel Templates (*.xls;*.xlsx)|*.xls;*.xlsx|All files (*.*)|*.*";
				fileDialog.Title = ResString.GetMultilingualString("45cab951-8abd-4ea9-919f-254f58730bf3", "Select Template File");
#if DEBUG
				result = PromptDeveloperForTemplateFile(fileDialog);
#else
				result = (fileDialog.ShowDialog() == DialogResult.OK) ? fileDialog.UnmappedFileName : null;
#endif
			}

			return result;
		}

		void RemoveTemplatePivotButton_Click(object sender, EventArgs e)
		{
			var isSelected = IsCurrentlySelectedElementsNotNull(CurrentTemplatePivots);
			if (!isSelected)
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("ce13c852-c2ef-462e-be42-07e797365f15", "Please select a template link to remove."), ResString.GetMultilingualString("6d9e76a1-6896-4d33-950e-e6141f891ab1", "Error"));
			}
			else
			{
				var canNotDeleteDocumentTitles = new ZStringBuilder();
				CurrentTemplatePivots.Where(pivot => pivot != null).ForEach(pivot =>
				{
					if (pivot.SI_DocumentTitleInfo.ReadOnly)
					{
						canNotDeleteDocumentTitles.AppendLine(pivot.SI_DocumentTitle);
					}
					else
					{
						pivot.Delete();
					}
				});
				var documentTitles = canNotDeleteDocumentTitles.ToString();
				if (!string.IsNullOrEmpty(documentTitles))
				{
					Globals.Message.ShowError(ResString.GetMultilingualString("AAF5671F-5B0E-4A59-9A75-B1269E44FB50", "You cannot remove the following system-defined template from a document or report.\r\n{0}", documentTitles), ResString.GetMultilingualString("6d9e76a1-6896-4d33-950e-e6141f891ab2", "Error"));
				}
			}
		}

		protected bool IsCurrentlySelectedElementsNotNull<T>(IEnumerable<T> data)
		{
			if (data == null || !data.Any())
			{
				return false;
			}
			var allCount = data.Count();
			var nullCount = data.Count(x => x == null);
			return allCount > nullCount;
		}

		#endregion

		#region Version Control
#if DEBUG

		void AvailableTemplatesGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			e.Colour = GetColour((StmTemplateBase)e.ObjectAtRow);
		}

		void SaveChangesButton_Click(object sender, EventArgs e)
		{
			StmTemplateBase template = CurrentTemplate;
			try
			{
				BusinessEntity.SaveForChanges(template);
			}
			catch (SourceControlException ex)
			{
				Globals.Message.ShowError(ex.Message, "Save Changes Error");
			}
		}

		void MarkTemplateEditableButton_Click(object sender, EventArgs e)
		{
			StmTemplateBase template = CurrentTemplate;
			if (BusinessEntity.CanCheckOutTemplate(template))
			{
				if (template.SO_ExcelTemplatePath.IsEmpty || !File.Exists(template.ExcelTemplateFullPath))
				{
					Globals.Message.ShowInformation("The selected template does not know about the Excel template file path yet. Please select an Excel template so that the file path will be stored.", "Missing Template");
					string templateFilePath = PromptForTemplateFile();
					if (templateFilePath != null)
					{
						template.ExcelTemplateFullPath = templateFilePath;
					}
					else
					{
						return;
					}
				}

				try
				{
					BusinessEntity.CheckOutTemplate(template);
				}
				catch (SourceControlException ex)
				{
					Globals.Message.ShowError(ex.Message, "Mark Template Editable Error");
				}
			}
		}

		internal Color GetColour(StmTemplateBase template)
		{
			bool isCheckedOutByMe;
			try
			{
				var isFileReadOnly = (File.GetAttributes(template.ExcelTemplateFullPath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
				if (!isFileReadOnly)
				{
					isCheckedOutByMe = template.IsCheckedOutByMe;
				}
				else
				{
					isCheckedOutByMe = false;
				}
			}
			catch
			{
				isCheckedOutByMe = false;
			}

			return (Globals.IsTest && !testingGridColour) || !isCheckedOutByMe ? Color.Empty : Color.Crimson;
		}

		string PromptDeveloperForTemplateFile(ZOpenFileDialog fileDialog)
		{
			string result = null;

			if (Globals.IsTest)
			{
				result = testTemplateFilePath;
			}
#if !WINZOR
			else if (ZOpenFileDialog.IsRemote)
			{
				Globals.Message.ShowError("Template file development is not allowed with Remote Desktop Services local file dialogs.");
			}
#endif
			else
			{
				string templatesDirectory = BuildConstants.GetLocalPath(MenuCustomisation.TemplatesDirectory);
				fileDialog.InitialDirectory = templatesDirectory;

				if (fileDialog.ShowDialog() == DialogResult.OK)
				{
					if (Globals.IsTest)
					{
						result = testTemplateFilePath;
					}
					else
					{
						string fileName = fileDialog.UnmappedFileName;
						if (fileName.ToUpper().StartsWith(templatesDirectory.ToUpper()))
						{
							result = fileName;
						}
						else
						{
							Globals.Message.ShowError("Please select a file within the templates source directory (" + templatesDirectory + ").", "Invalid Template Path");
						}
					}
				}
			}

			return result;
		}

		void SetUpForDebugMode()
		{
			// This line should never be required again.
			// checkInTemplateButton.Visible = true;
			// Left here in case someone is searching for what originally set this value.
			saveChangesButton.Visible = true;
			markTemplateEditableButton.Visible = true;
			undoTemplateChangesButton.Visible = true;

			saveChangesButton.Click += new EventHandler(SaveChangesButton_Click);
			markTemplateEditableButton.Click += new EventHandler(MarkTemplateEditableButton_Click);
			undoTemplateChangesButton.Click += new EventHandler(UndoTemplateChangesButton_Click);

			ZTextBoxColumnStyleInfo columnInfo = new ZTextBoxColumnStyleInfo();
			columnInfo.Caption = "Template Restriction";
			columnInfo.ColumnName = "SO_TemplateRestriction";
			columnInfo.ToolTip = "Comment to display when creating a copy of this template.";
			ControlDpiScalingHelper.SetWidth(ref columnInfo, 120, true);
			TypeDescriptor.AddAttributes(columnInfo, new SuppressFormsLocalizedTestAttribute());

			availableTemplatesGrid.ColumnStyles.Add(columnInfo);
			availableTemplatesGrid.ColourDeciding += AvailableTemplatesGrid_ColourDeciding;
			availableTemplatesGrid.AfterBind += AvailableTemplatesGrid_AfterBind;
		}

		void AvailableTemplatesGrid_AfterBind(object sender, EventArgs e)
		{
			availableTemplatesGrid.ListManager.PositionChanged += AvailableTemplatesGrid_ListManager_PositionChanged;
		}

		void AvailableTemplatesGrid_ListManager_PositionChanged(object sender, EventArgs e)
		{
			var currentTemplate = availableTemplatesGrid.ListManager.GetCurrent() as StmTemplateBase;
			if (currentTemplate != null)
			{
				markTemplateEditableButton.Enabled = currentTemplate.SO_Name != SystemDocumentElementsName;
			}
		}

		void UndoTemplateChangesButton_Click(object sender, EventArgs e)
		{
			try
			{
				BusinessEntity.UndoCheckOutTemplate(CurrentTemplate);
			}
			catch (SourceControlException ex)
			{
				Globals.Message.ShowError(ex.Message, "Undo Template Changes Error");
			}
		}

#endif
		#endregion

		#region IMenuCustomisationForm Members

		IMenuEditable IMenuCustomisationForm.EditableBusinessEntity
		{
			get { return BusinessEntity; }
		}

		#endregion

		#region IButtonTextOverride Members

		string IButtonPostTextOverride.PostButtonText
		{
			get { return ResString.GetMultilingualString("PostingButtonText|SaveAndClose", "Save && Close"); }
		}

		string IButtonCloseTextOverride.CloseButtonText
		{
			get { return ResString.GetMultilingualString("PostingButtonText|Close", "Close"); }
		}

		#endregion

		#region Save / Validation

		protected override void SaveInternal()
		{
			base.SaveInternal();
			var currentCustomisationVersion = RawDataRegistry.Instance.DocumentCustomisationVersionNumber.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			RawDataRegistry.Instance.DocumentCustomisationVersionNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, currentCustomisationVersion + 1);
		}

		void BusinessEntity_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (BusinessEntity.HasChanges)
			{
				TemplateCache.Instance.Clear();
			}
		}

		#endregion
	}

#if DEBUG
	sealed class DummyTemplateEditor : ITemplateEditor
	{
		internal DummyTemplateEditor()
		{
			Log = new List<string>();
		}
		public readonly List<string> Log;
		public StmTemplateBase Template;

		public void Edit()
		{
			Log.Add(string.Format("Template [{0}] edited with no row and no column", Template.SO_Name));
		}

		public void Edit(int row, int column)
		{
			Log.Add(string.Format("Template [{0}] edited at Row: {1} Column: {2}", Template.SO_Name, row, column));
		}
	}
#endif
}
