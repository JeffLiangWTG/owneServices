using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.GUI.XmlExport;
using Enterprise.Client.DFD.Export;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.DFD.GUI
{
	public class DFDFlatFileXmlExportGUIWrapper : FlatFileXmlExportGUIWrapper
	{
		public DFDFlatFileXmlExportGUIWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override AccountingTransactionsDataExporter DataExporter
		{
			get
			{
				if (fExporter == null)
				{
					fExporter = new DFDXmlARTransactionsExporter(Factory);
				}
				return fExporter;
			}
		}
		DFDXmlARTransactionsExporter fExporter;

		protected override string InitialDirectory
		{
			get { return (DFDDataRegistry.Instance.ARTransactionsExportDirectory.IsEmpty) ? new ZString(base.InitialDirectory) : DFDDataRegistry.Instance.ARTransactionsExportDirectory; }
		}

		protected override bool IsOKToExport()
		{
			return true;
		}

		protected override void CreateFileAndExport(Stream stream)
		{
			throw new InvalidOperationException();
		}

		protected void CreateFileAndExport(ZString selectedPath)
		{
			string tempPathForExport = Env.TempPath + "DFD";
			if (!Directory.Exists(tempPathForExport))
			{
				Directory.CreateDirectory(tempPathForExport);
			}

			DirectoryInfo directoryPath = new DirectoryInfo(tempPathForExport);

			string tempFileName = Env.GetTempFileName(tempPathForExport);
			List<FileInfo> tempFiles = new List<FileInfo>();

			try
			{
				using (ProgressForm = new ProgressForm())
				{
					ShowProgressForm();
					ExportAndShowResultsToUser(File.Open(tempFileName, FileMode.Open, FileAccess.ReadWrite));

					tempFiles.AddRange(directoryPath.GetFiles("*.tmp"));

					for (int counter = 0; counter < tempFiles.Count; counter++)
					{
						tempFiles[counter].CopyTo(selectedPath + "\\" + ((DFDXmlARTransactionsExporter)DataExporter).FileNames[tempFiles[counter].FullName].ToString().Replace("/", "") + ".xml");
					}
				}
			}
			finally
			{
				foreach (FileInfo tempFile in tempFiles)
				{
					tempFile.Delete();
				}
			}
		}

		protected override void LoadFormAndExport()
		{
			using (ZFolderBrowserDialog dialog = new ZFolderBrowserDialog())
			{
				dialog.SelectedPath = InitialDirectory;

				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK && DataExporter.IsTransactionsExistInBatch)
				{
					string selectedPath = ZOpenFileDialog.IsRemote ? dialog.MappedSelectedPath : dialog.UnmappedSelectedPath;
					CreateFileAndExport(selectedPath);
				}
				else
				{
					Globals.Message.ShowInformation("No Transactions were exported.", "There are currently no transactions to export");
				}
			}
		}
	}
}
