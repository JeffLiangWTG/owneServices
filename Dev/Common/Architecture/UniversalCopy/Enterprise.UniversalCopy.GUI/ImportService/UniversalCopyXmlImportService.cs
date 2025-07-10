using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.UniversalCopy;
using Enterprise.UniversalCopy.GUI.FileLocator;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.UniversalCopy.GUI.ImportService
{
	public class UniversalCopyXmlImportService
	{
		public UniversalCopyReadFileLocator FileLocator
		{
			get
			{
				return fileLocator ?? (fileLocator = new UniversalCopyReadFileLocator());
			}
			set { fileLocator = value; }
		}
		UniversalCopyReadFileLocator fileLocator;

		public IUserNotification Logger
		{
			get
			{
				return logger ?? (logger = Globals.Message);
			}
			set { logger = value; }
		}
		IUserNotification logger;

		public CopyTemplateTree Import(bool replace)
		{
			string displayFileName;
			using (var fileStream = FileLocator.GetFileStream(out displayFileName))
			{
				CopyTemplateTree result = null;

				string errorMessage = Res.GetString("ee1dfece-86d6-486e-bd19-2b20bf4a92d3", "The file {0} is an invalid Universal Copy file to import.", displayFileName);
				if (fileStream == null)
				{
					return null;
				}
				var confirmToImport = true;
				if (replace)
				{
					string confirmationMessage = Res.GetString("7495a4cb-6580-43ab-8acf-de481292198a", "Are you sure you want to import copy template configuration from file {0}?All existing configuration will be lost.", displayFileName);
					confirmToImport = Globals.Message.ShowConfirmation(confirmationMessage, Res.GetString("e5dfe68d-15c9-415e-9290-947997e17d56", "Import from File"), "Y", MessageBoxIcon.Question) == DialogResult.OK;
				}

				if (confirmToImport)
				{
					try
					{
						var copyTemplateTree = CopyTemplateTree.Deserialize(fileStream);
						if (copyTemplateTree == null)
						{
							result = null;
						}
						else
						{
							result = copyTemplateTree;
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						errorMessage += string.Format(CultureInfo.InvariantCulture, (NoResString)"Exception occurs with message {0}", ex.Message);
						result = null;
					}

					if (result == null)
					{
						Logger.ShowError(errorMessage);
					}
				}
				return result;
			}
		}
	}
}
