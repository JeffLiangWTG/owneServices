using System.IO;
using CargoWise.IO;
using Enterprise.DocumentEngine.Business;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	class TemplateTempFileManager
	{
		internal TemplateTempFileManager(StmTemplateBase template)
		{
			this.template = template;
			WorkingFilePath = string.Empty;
		}

		readonly StmTemplateBase template;

		FileAttributes originalWorkingFileAttributes;
		internal string WorkingFilePath
		{
			get;
			private set;
		}

		internal string SetupWorkingFile()
		{
#if DEBUG
			if (template.SO_IsSystemDefined)
			{
				WorkingFilePath = template.ExcelTemplateFullPath;
			}
			else
#endif
			{
				var extension = Path.GetExtension(template.SO_ExcelTemplatePath);
				if (string.IsNullOrEmpty(extension))
				{
					//Assume format is XLS if we couldn't retrieve the extension
					extension = AttachmentTypeList.Codes.Xls;
				}
				WorkingFilePath = Temp.GetTempFileNameWithExtension(extension);
				if (File.Exists(WorkingFilePath))
				{
					File.Delete(WorkingFilePath);
				}

				SaveToFile(WorkingFilePath);
			}

			LockWorkingFileIfRequired();

			return WorkingFilePath;
		}

		void SaveToFile(string filePath)
		{
			using (var tempateStream = new ExcelTemplateReadFromStmTemplateTable(template).GetAsTemplateStream())
			{
				tempateStream.CopyToFile(filePath);
			}
		}

		internal void SaveToStream(Stream stream)
		{
			using (stream)
			using (var tempateStream = new ExcelTemplateReadFromStmTemplateTable(template).GetAsTemplateStream())
			{
				tempateStream.CopyTo(stream);
			}
		}

		internal string TemplateFileName
		{
			get { return Path.GetFileName(template.SO_ExcelTemplatePath); }
		}

		void LockWorkingFileIfRequired()
		{
			if (!template.CanModifyExcelTemplate)
			{
				originalWorkingFileAttributes = File.GetAttributes(WorkingFilePath);
				if (originalWorkingFileAttributes != FileAttributes.ReadOnly)
				{
					File.SetAttributes(WorkingFilePath, FileAttributes.ReadOnly);
				}
			}
		}

		internal void CleanupWorkingFile()
		{
			if (!string.IsNullOrEmpty(WorkingFilePath))
			{
				if (!template.IsDeleted && template.SO_IsSystemDefined && File.Exists(WorkingFilePath))
				{
					File.SetAttributes(WorkingFilePath, originalWorkingFileAttributes);
				}
				else
				{
					if (File.Exists(WorkingFilePath))
					{
						var mes = string.Empty;
						if (!TempFile.TryDelete(WorkingFilePath, out mes, false))
						{
							var error = Res.GetString("c7ae7604-dced-4f71-b589-c2c8a029d843", "{0} has attempted to delete a temporary file. This has been unsuccessful. You do not need to take any further action.", Core.Constants.ProductName);
							Globals.Message.Show(error + mes);
						}
					}

					WorkingFilePath = "";
				}
			}
		}
	}
}
