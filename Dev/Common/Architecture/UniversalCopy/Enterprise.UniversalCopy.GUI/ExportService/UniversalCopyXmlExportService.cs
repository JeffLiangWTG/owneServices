using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.UniversalCopy;
using Enterprise.UniversalCopy.GUI.FileLocator;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.UniversalCopy.GUI.ExportService
{
	public class UniversalCopyXmlExportService
	{
		public UniversalCopyWriteFileLocator FileLocator
		{
			get
			{
				return fileLocator ?? (fileLocator = new UniversalCopyWriteFileLocator());
			}
			set { fileLocator = value; }
		}
		UniversalCopyWriteFileLocator fileLocator;

		public IUserNotification Logger
		{
			get
			{
				return logger ?? (logger = Globals.Message);
			}
			set { logger = value; }
		}
		IUserNotification logger;

		public void Export(CopyTemplateTree copyTemplateTree)
		{
			try
			{
				FileLocator.CopyTemplateTree = copyTemplateTree;
				string displayFileName;

				using (var fileStream = FileLocator.GetFileStream(out displayFileName))
				{
					if (fileStream == null)
					{
						return;
					}
					copyTemplateTree.Serialize(fileStream);
					Logger.ShowInformation(string.Format(CultureInfo.InvariantCulture, (NoResString)"File saved at {0}", displayFileName));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Logger.ShowError(ex.Message);
			}
		}
	}
}
