using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public class WinzorExcelManager : IExcelManager
	{
		internal WinzorExcelManager(Form parentForm)
		{
			this.parentForm = parentForm;
		}

		public void Edit(string workingFile, int row, int column)
		{
			this.workingFile = workingFile;
			this.readOnly = File.GetAttributes(workingFile).HasFlag(FileAttributes.ReadOnly);
			remoteFile = ObjectFactory.Get<IRemoteFile>(nameof(IRemoteFile), workingFile, File.ReadAllBytes(workingFile), readOnly);

			if (OpenFile(remoteFile))
			{
				editingForm = new EditingExcelForm();
				editingForm.CheckEditFinished += new EditingExcelForm.CheckEditFinishedDelegate(editingForm_CheckEditFinished);
				editingForm.Closed += EditingForm_Closed;
				ZFormModaliser.Show(editingForm, parentForm);
			}
			else
			{
				remoteFile.Dispose();
				remoteFile = null;

				Globals.Message.Show(Res.GetString("204ae293-ca5e-438e-a5c7-e682265007de", "The file could not be opened."));
			}
		}

		public bool IsSupported => true;

		public event ExcelClosedEventHandler ExcelClosed;

		bool editingForm_CheckEditFinished()
		{
			if (remoteFile != null)
			{
				return !remoteFile.GetIsOpenStatus();
			}
			return true;
		}

		void EditingForm_Closed(object sender, EventArgs e)
		{
			editingForm.Closed -= EditingForm_Closed;
			var result = editingForm.DialogResult;
			try
			{
				if (!readOnly)
				{
					var data = FetchFileData(remoteFile);
					if (data != null)
					{
						File.WriteAllBytes(workingFile, data);
					}
					else
					{
						result = DialogResult.Cancel;
					}
				}
			}
			catch (OperationCanceledException)
			{
				result = DialogResult.Cancel;
			}
			finally
			{
				remoteFile.Dispose();
				remoteFile = null;

				ExcelClosed?.Invoke(result);
			}
		}

		readonly Form parentForm;
		internal EditingExcelForm editingForm;
		IRemoteFile remoteFile;
		string workingFile;
		bool readOnly;

		protected virtual bool OpenFile(IRemoteFile file) => file.Open();

		protected virtual byte[] FetchFileData(IRemoteFile file) => file.FetchFileData();
	}
}
