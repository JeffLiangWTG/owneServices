using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

#if DEBUG
using CargoWise.BuildTools;
using Enterprise.DocumentEngine.Build;
#endif

namespace Enterprise.DocumentEngine
{
	public abstract class MenuCustomisation : NonPersistentBusinessObject, IDocumentSupportable, IObsoleteValidation, IMenuEditable
	{
		protected MenuCustomisation(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static string TemplatesDirectory
		{
			get
			{
				return @"Enterprise\Product\Documents\ExcelTemplates";
			}
		}

		protected abstract string Description { get; }
		protected abstract PrintTask GetPrintTaskCore(StmMenuItemBase menuItem);
		protected abstract StmMenuItemBaseCollection InitialiseMenus();
		protected abstract StmTemplateBaseCollection InitialiseAvailableTemplates();
		protected abstract string ValidateNewTemplate(DataContextValue dataContext);

		public virtual MenuEditingMode EditingMode
		{
			get { return editingMode; }
			set
			{
				editingMode = value;
				if (availableTemplates != null)
				{
					availableTemplates.EditingMode = value;
				}
				if (menus != null)
				{
					menus.EditingMode = value;
				}
			}
		}
		MenuEditingMode editingMode;

		public StmTemplateBaseCollection AvailableTemplates
		{
			get
			{
				if (availableTemplates == null)
				{
					availableTemplates = InitialiseAvailableTemplates();
					availableTemplates.Load();
					availableTemplates.EditingMode = EditingMode;
					RegisterEditableChildObject(availableTemplates);
				}
				return availableTemplates;
			}
		}
		StmTemplateBaseCollection availableTemplates;

		public StmMenuItemBaseCollection Menus
		{
			get
			{
				if (menus == null)
				{
					menus = InitialiseMenus();
					menus.Load();
					menus.EditingMode = EditingMode;
					RegisterEditableChildObject(menus);
				}
				return menus;
			}
		}
		StmMenuItemBaseCollection menus;

		public ReturnResult<StmTemplateBase> AddNewTemplate(string templateFilePath)
		{
			var result = new ReturnResult<StmTemplateBase>();

			try
			{
				StmTemplateBase newTemplate = Menus.LoadNewTemplateFromFile(templateFilePath);
				DataContextValue dataContextValue = GetDataContextValue(newTemplate.GetExcelTemplate());
				string error = ValidateNewTemplate(dataContextValue);

				if (string.IsNullOrEmpty(error))
				{
					if (SetDataContextFromTemplate)
					{
						newTemplate.SO_DataContext = dataContextValue.FullDataContext;
					}

					newTemplate.SO_Name = Path.GetFileNameWithoutExtension(templateFilePath);
					AvailableTemplates.Add(newTemplate);
					newTemplate.SO_ExcelTemplatePath = Path.GetFileName(templateFilePath);
#if DEBUG
					if (!IgnoreIsDebugCheckForTest)
					{
						UpdateExcelTemplatePath(newTemplate, templateFilePath);
					}
#endif
					result.Success = true;
					result.Value = newTemplate;
				}
				else
				{
					newTemplate.Delete();
					result.Message = error;
					result.Success = false;
				}
			}
			catch (ExcelInterfaceException exception)
			{
				switch (exception.Type)
				{
					case ExcelInterfaceExceptionType.FileFormatNotSupported:
						result.Success = false;
						result.Message = exception.Message;
						break;

					default:
						throw;
				}
			}

			return result;
		}
#if DEBUG
		public bool IgnoreIsDebugCheckForTest;
		public string DescriptionForTest => Description;
#endif

		protected virtual bool SetDataContextFromTemplate
		{
			get { return true; }
		}

		public StmMenuTemplatePivotBase AddTemplatePivot(StmMenuItemBase menu, StmTemplateBase template)
		{
			StmMenuTemplatePivotBase result;
			if ((menu != null) && (template != null))
			{
				result = menu.Documents.AddNew();
				result.SI_SO = template.PK;
				result.SI_SU = menu.PK;
				result.SI_DocumentTitle = (NoResString)"New " + Description;
				result.EditingMode = EditingMode;
				SetAdditionalProperties(menu, result);

				result.Validation.ValidateAll();
			}
			else
			{
				result = null;
			}
			return result;
		}

		protected virtual void SetAdditionalProperties(StmMenuItemBase menu, StmMenuTemplatePivotBase pivot)
		{
		}

		public StmTemplateBase CopyTemplate(StmTemplateBase template)
		{
			StmTemplateBase result = template.NewCopy();
			AvailableTemplates.Add(result);
			return result;
		}

		DataContextValue GetDataContextValue(ExcelTemplate excelTemplate)
		{
			SectionRepository sectionRepository = new SectionRepository(excelTemplate);
			return new DataContextValue(sectionRepository.DataContext);
		}

		public PrintTask GetPrintTask(StmMenuItemBase menuItem)
		{
			return GetPrintTaskCore(menuItem);
		}

		public ReturnResult UpdateTemplateRecord(StmTemplateBase template, string templateFilePath)
		{
			ReturnResult result = new ReturnResult();

			try
			{
				ZBlob newTemplateData = StmTemplateBase.GetTemplateBlobFromFile(templateFilePath);
				ExcelTemplate userTemplate = new ExcelTemplateReadFromByteArray(template.SO_Name, templateFilePath, newTemplateData);
				DataContextValue dataContextValue = GetDataContextValue(userTemplate);

				result = ValidateUpdateTemplate(dataContextValue, template, userTemplate);
				if (result.Success)
				{
					UpdateTemplateRecordCore(template, newTemplateData, templateFilePath, dataContextValue);
				}
			}
			catch (ExcelInterfaceException exception)
			{
				switch (exception.Type)
				{
					case ExcelInterfaceExceptionType.FileFormatNotSupported:
						result.Success = false;
						result.Message = exception.Message;
						break;

					default:
						throw;
				}
			}

			return result;
		}

		public ReturnResult ValidateUpdateTemplate(DataContextValue dataContextValue, StmTemplateBase template, ExcelTemplate modifiedTemplate)
		{
			ReturnResult result = new ReturnResult();
			result.Message = ValidateNewTemplate(dataContextValue);

			if (!string.IsNullOrEmpty(result.Message))
			{
				result.Success = false;
			}
			else
			{
				result = ValidateUpdateTemplateCore(dataContextValue, template, modifiedTemplate);
			}
			return result;
		}

		protected virtual ReturnResult ValidateUpdateTemplateCore(DataContextValue dataContextValue, StmTemplateBase template, ExcelTemplate modifiedTemplate)
		{
			ReturnResult result = new ReturnResult();
			result.Success = true;
			return result;
		}

		void UpdateTemplateRecordCore(StmTemplateBase template, ZBlob newTemplateData, string templateFilePath, DataContextValue dataContextValue)
		{
			template.SO_Template = newTemplateData;
			template.SO_DataContext = dataContextValue.FullDataContext;

			var extension = Path.GetExtension(templateFilePath);
			if (string.IsNullOrEmpty(extension))
			{
				//Assume format is XLS if we couldn't retrieve the extension
				extension = "." + AttachmentTypeList.Codes.Xls;
			}
			template.SO_ExcelTemplatePath = MakeFilenameSafe.MakeSafe(template.SO_Name + extension, '_');

#if DEBUG
			UpdateExcelTemplatePath(template, templateFilePath);
#endif
		}

		#region Check In/Out
#if DEBUG
		public bool CanCheckOutTemplate(StmTemplateBase template)
		{
			return (template != null) && (!template.IsCheckedOutByMe);
		}

		public void SaveForChanges(StmTemplateBase template)
		{
			if (template != null)
			{
				if (template.SO_Name == Core.Constants.SectionRepositoryTemplateNames.User)
				{
					var fileManager = new CustomizedDocumentElementsFileManager();
					var customizedDocumentElementsFilePath = fileManager.GetFilePath();
					AddToChanges(template, customizedDocumentElementsFilePath);
					return;
				}
				else if (template.IsCheckedOutByMe)
				{
					var changeController = new DocumentsSetupController(false);
					if (SourceControl.EnterpriseDatabase.IsDifferent(template.ExcelTemplateFullPath))
					{
						changeController.FullCheckOut();  //checks if latests schema and increments version
						changeController.FullSave();
					}
					else
					{
						CargoWise.BuildTools.SourceControl.EnterpriseDatabase.UndoCheckOut(template.ExcelTemplateFullPath, false);
					}
					template.IsCheckedOutByMe = false;
				}
			}
		}

		void AddToChanges(StmTemplateBase template, string filePath)
		{
			var fileDir = Path.GetDirectoryName(filePath);
			Directory.CreateDirectory(fileDir);

			using (FileStream outputStream = File.Open(filePath, FileMode.Create))
			{
				byte[] templateAsByteArray = new ExcelTemplateReadFromStmTemplateTable(template).GetAsByteArray();
				outputStream.Write(templateAsByteArray, 0, templateAsByteArray.Length);
			}
			SourceControl.EnterpriseDatabase.PendAdd(filePath);
		}

		public void CheckOutTemplate(StmTemplateBase template)
		{
			if (CanCheckOutTemplate(template))
			{
				CargoWise.BuildTools.SourceControl.EnterpriseDatabase.CheckOut(template.ExcelTemplateFullPath, false);
				template.IsCheckedOutByMe = true;
			}
		}

		public void UndoCheckOutTemplate(StmTemplateBase template)
		{
			if ((template != null) && (template.IsCheckedOutByMe))
			{
				CargoWise.BuildTools.SourceControl.EnterpriseDatabase.UndoCheckOut(template.ExcelTemplateFullPath, false);
				template.IsCheckedOutByMe = false;
			}
		}

		void UpdateExcelTemplatePath(StmTemplateBase template, string templateFilePath)
		{
			if (template.SO_IsSystemDefined || templateFilePath.StartsWith(CargoWise.BuildTools.BuildConstants.LocalEnterprisePath, StringComparison.OrdinalIgnoreCase))
			{
				template.ExcelTemplateFullPath = templateFilePath;
			}
		}
#endif
		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return DocumentSupporter; }
		}

		protected abstract DocumentSupporter DocumentSupporter { get; }

		#endregion
	}
}
