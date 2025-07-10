using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	class RemoteExcelManager : IExcelManager
	{
		internal RemoteExcelManager(Form parentForm)
		{
			this.parentForm = parentForm;
		}

		public void Edit(string workingFile, int row, int column)
		{
			this.workingFile = workingFile;
			this.readOnly = File.GetAttributes(workingFile).HasFlag(FileAttributes.ReadOnly);
			remoteFile = new RemoteFile(workingFile, File.ReadAllBytes(workingFile), readOnly);

			if (OpenFile(remoteFile))
			{
				editingForm = new EditingExcelForm();
				editingForm.CheckEditFinished += new EditingExcelForm.CheckEditFinishedDelegate(editingForm_CheckEditFinished);
				editingForm.Closed += new EventHandler(EditingForm_Closed);
				ZFormModaliser.Show(editingForm, parentForm);
			}
			else
			{
				remoteFile.Dispose();
				remoteFile = null;

				Globals.Message.Show(Res.GetString("204ae293-ca5e-438e-a5c7-e682265007de", "The file could not be opened."));
			}
		}

		readonly Form parentForm;
		protected EditingExcelForm editingForm;
		RemoteFile remoteFile;
		string workingFile;
		bool readOnly;

		public event ExcelClosedEventHandler ExcelClosed;

		public bool IsSupported
		{
			get { return ObjectFactory.Get<TerminalService>().IsWTSSession && RemoteFile.IsSupported; }
		}

		bool editingForm_CheckEditFinished()
		{
			if (remoteFile != null)
			{
				return !remoteFile.GetStatus().isOpen;
			}
			return true;
		}

		void EditingForm_Closed(object sender, EventArgs e)
		{
			editingForm.Closed -= new EventHandler(EditingForm_Closed);
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

		protected virtual bool OpenFile(RemoteFile file) => file.Open();

		protected virtual byte[] FetchFileData(RemoteFile file) => file.FetchFileData();

#if DEBUG
		public void CloseEditingFormForTest()
		{
			if (editingForm != null)
			{
				editingForm.Close();
				editingForm.Dispose();
			}
			if (remoteFile != null)
			{
				remoteFile.Dispose();
				remoteFile = null;
				ExcelClosed?.Invoke(DialogResult.Cancel);
			}
		}

		public void SetRemoteFile(RemoteFile file)
		{
			if (remoteFile != null)
			{
				remoteFile.Dispose();
			}
			remoteFile = file;
		}

#endif
	}
}
