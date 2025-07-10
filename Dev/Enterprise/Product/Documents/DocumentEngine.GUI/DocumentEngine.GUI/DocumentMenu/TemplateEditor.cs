using System.Windows.Forms;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	internal sealed class TemplateEditor : ITemplateEditor
	{
		internal TemplateEditor(StmTemplateBase template, Form parentForm)
			: this(template, parentForm, false)
		{
		}

		internal TemplateEditor(StmTemplateBase template, Form parentForm, bool shouldCloseParentFormAfterEdit)
		{
			this.Template = template;
			this.ParentForm = parentForm;
			this.shouldCloseParentFormAfterEdit = shouldCloseParentFormAfterEdit;

			var menuCustomisationForm = parentForm as MenuCustomisationForm;
			if (menuCustomisationForm != null)
			{
				menuCustomisation = menuCustomisationForm.BusinessEntity;
			}
		}

		readonly StmTemplateBase Template;
		readonly Form ParentForm;
		readonly bool shouldCloseParentFormAfterEdit;
		readonly MenuCustomisation menuCustomisation;

		public void Edit()
		{
			Edit(-1, -1);
		}

		public void Edit(int row, int column)
		{
			if (ExcelManager.IsSupported)
			{
				ExcelManager.ExcelClosed += new ExcelClosedEventHandler(manager_ExcelClosed);
				ExcelManager.Edit(WorkingFileManager.SetupWorkingFile(), row, column);
			}
			else
			{
				FileSaveToOpenForm.ShowDialog(new FileSaveToOpenForm.FileGeneratorCallback(WorkingFileManager.SaveToStream),
					WorkingFileManager.TemplateFileName,
					Res.GetString("92155558-2f4d-4fa6-a505-27bfba67d3eb", "You cannot edit the template directly. Save the template to a file, edit the file externally, and reload the changes from the changed file using the Load button."), null, null
				);
			}
		}

		void manager_ExcelClosed(DialogResult dialogResult)
		{
			bool shouldCleanupWorkingFile = true;
			try
			{
				if (dialogResult == DialogResult.OK)
				{
					if (Template.IsDeleted)
					{
						string errorMessage = Res.GetString("8DBC93B2-8260-4E43-9618-6C64462A9D5F", "Error Building Template: Template has been deleted.");
						Globals.Message.ShowError(errorMessage, Res.GetString("3350908E-A98F-47E7-87D0-2AD28FCDBC08", "Error"));
					}
					else if (Template.SO_TemplateType == StmTemplateTypes.Codes.Form)
					{
						Template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(WorkingFileManager.WorkingFilePath);
					}
					else
					{
						using (ExcelInterface.EnableCacheLoadedExcelInterfaceByBinary())
						{
							var userTemplate = new ExcelTemplateReadFromByteArray(Template.SO_Name, "", StmTemplateBase.GetTemplateBlobFromFile(WorkingFileManager.WorkingFilePath));
							DataContextValue dataContextValue = userTemplate.GetDataContextValueFromBlobData();

							if (menuCustomisation != null)
							{
								var validationResult = menuCustomisation.ValidateUpdateTemplate(dataContextValue, Template, userTemplate);

								if (!string.IsNullOrEmpty(validationResult.Message))
								{
									if (validationResult.Success)
									{
										Globals.Message.ShowWarning(validationResult.Message, Res.GetString("F8676C58-C1CF-4C65-8B15-4220A8E10A9A", "Warning Updating Template"));
									}
									else
									{
										var message = Res.GetString("1F91A347-F013-4584-9BE4-A4F47E8FB20D",
											@"The template has following error(s) after modification:
{0}

Click YES to fix the error or click NO to discard the changes (If you have made significant changes in the template, it is recommended to click YES and backup your modifications first.)", validationResult.Message);
										if (Globals.Message.Show(message, Res.GetString("24AE9DF9-F052-469E-85F6-80D02EC1EEF8", "Error Updating Template"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
										{
											ExcelManager.Edit(WorkingFileManager.WorkingFilePath, -1, -1);
											shouldCleanupWorkingFile = false;
										}
									}
								}

								if (validationResult.Success)
								{
									Template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(WorkingFileManager.WorkingFilePath);
									Template.SO_DataContext = dataContextValue.FullDataContext;
								}
							}
							else
							{
								Template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(WorkingFileManager.WorkingFilePath);
							}
						}
					}

					if (shouldCloseParentFormAfterEdit)
					{
						ParentForm.Close();
					}
				}
			}
			catch (TemplateDefinitionException ex)
			{
				string errorMessage = Res.GetString("ee53f5c5-001d-455c-8f42-eac8b93d1701", "Error Building Template: {0}", ex.Message);
				Globals.Message.ShowError(errorMessage, Res.GetString("3350908E-A98F-47E7-87D0-2AD28FCDBC08", "Error"));
			}
			catch (TemplateFileReadException)
			{
				string errorMessage = Res.GetString("9e32e670-cf33-4e46-b400-00596f29dbd9", @"Error opening {0} - This file is currently used by another application.

Please make sure Excel is closed and the file is not locked by any processes before closing the dialog.", WorkingFileManager.WorkingFilePath);
				Globals.Message.ShowError(errorMessage, Res.GetString("7033aca6-bdf8-44cc-85fa-3eb1fe1ca7df", "Error"));
			}
			finally
			{
				if (shouldCleanupWorkingFile)
				{
					WorkingFileManager.CleanupWorkingFile();
				}
			}
		}

		IExcelManager excelManager;
		IExcelManager ExcelManager
		{
			get
			{
#if WINZOR
				return excelManager ?? (excelManager = new WinzorExcelManager(ParentForm));
#else
				var remoteXLS = new RemoteExcelManager(ParentForm);
				return excelManager ?? (excelManager = remoteXLS.IsSupported ? remoteXLS : new ExcelManager(ParentForm));
#endif
			}
		}

#if DEBUG
		internal void SetExcelManagerForTesting(IExcelManager excelManager)
		{
			this.excelManager = excelManager;
		}
#endif

		TemplateTempFileManager fWorkingFileManager;
		TemplateTempFileManager WorkingFileManager
		{
			get { return fWorkingFileManager ?? (fWorkingFileManager = new TemplateTempFileManager(Template)); }
		}
	}
}
