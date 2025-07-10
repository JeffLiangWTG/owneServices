using System;
using System.IO;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataTransfer.GUI
{
	class XmlDataTransferExporterGUI : IXmlDataTransferExporterGUI
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Extension Filter, File Extension")]
		public ZDialogResult ShowSaveFileDialog(string defaultFileName, string initialDirectory)
		{
			using (var dialog = new ZSaveFileDialog
			{
				CheckPathExists = true,
				Filter = "Xml Files *.xml|*.xml",
				DefaultExt = "xml",
				AddExtension = true,
				FileName = defaultFileName,
				InitialDirectory = initialDirectory
			})
			{
				var result = ZFormModaliser.ShowCommonDialogWithoutDispose(dialog);
				UnmappedFile = dialog.UnmappedFileName;
				return (ZDialogResult)result;
			}
		}

		public string UnmappedFile { get; private set; }

		public bool IsLocalFile => !ZSaveFileDialog.IsRemote;

		public Stream OpenFile() => ZSaveFileDialog.OpenFile(UnmappedFile);

		public IDisposable ShowProgressForm(IProgressSupporter progressSupporter, int totalCount)
		{
			var progressForm = new ProgressForm();
			progressForm.Status = Res.GetString("a2c35dda-05e3-4ae5-8c3d-b7c4094bd11b", "Exporting... This can take some time...");
			progressForm.ShowCancelButton = false;
			progressForm.ShowProgressBar = true;
			progressForm.Show();

			EventHandler progressHandler = null;
			int doneCount = 0;
			if (progressSupporter != null)
			{
				progressSupporter.Progress +=
					progressHandler =
					delegate
					{
						doneCount += 100;
						int percentComplete = doneCount / totalCount;
						if (percentComplete > 100)
						{
							percentComplete = 100;
						}
						if (percentComplete > progressForm.PercentComplete)
						{
							progressForm.PercentComplete = percentComplete;
							progressForm.Invalidate();
							progressForm.Update();
						}
					};
				progressForm.Disposed += (s, e) => progressSupporter.Progress -= progressHandler;
			}

			return progressForm;
		}
	}
}
