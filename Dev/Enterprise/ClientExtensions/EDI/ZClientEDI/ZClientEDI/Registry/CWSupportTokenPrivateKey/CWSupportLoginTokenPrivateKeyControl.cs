using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WTG.IdentitySecurity;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public enum DataLoadState
	{
		Unknown,
		NoData,
		DataExists,
		DataCleared
	}

	public partial class CWSupportLoginTokenPrivateKeyControl : ZUserControl
	{
		public CWSupportLoginTokenPrivateKeyControl()
		{
			InitializeComponent();
			State = DataLoadState.NoData;
		}

		public void SetFileData(byte[] value)
		{
			fBinaryFileData = value;

			if (IsBinaryFileDataEmpty)
			{
				State = DataLoadState.NoData;
			}
			else
			{
				State = DataLoadState.DataExists;
			}
		}

		public DataLoadState State
		{
			get { return fState; }
			set
			{
				fState = value;
				switch (value)
				{
					case DataLoadState.NoData:
						UserFeedbackLabel.Text = Res.GetString("460FEF3E-F06B-4620-A3C0-EEC781D2133E", "No Data");
						break;

					case DataLoadState.DataExists:
						UserFeedbackLabel.Text = Res.GetString("444FD82E-07B4-4ED7-B402-2D6102FEADC5", "Data Exists");
						break;

					case DataLoadState.DataCleared:
						UserFeedbackLabel.Text = Res.GetString("D06DE848-A5CA-489B-BA54-CBE3B3AFEB29", "Data Cleared");
						break;

					default:
						UserFeedbackLabel.Text = "";
						break;
				}
			}
		}
		DataLoadState fState;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Array still required for the Registry Editor")]
		public byte[] FileDataAsBinary => fBinaryFileData;
		byte[] fBinaryFileData;

		bool IsBinaryFileDataEmpty => fBinaryFileData == null || fBinaryFileData.Length == 0;

		#region ReadOnly

		public bool ReadOnly
		{
			get { return !LoadButton.Enabled; }
			set
			{
				LoadButton.Enabled = !value;
				ClearButton.Enabled = !value;
			}
		}

		#endregion

		void LoadButton_Click(object sender, EventArgs e)
		{
			using (var stream = LoadFile())
			{
				if (stream != null)
				{
					var bytes = stream.ToByteArray();
					var fileString = BinaryToString(bytes);

					if (RSAKeyProvider.ImportPrivateKey(fileString) != null)
					{
						fBinaryFileData = bytes;
						State = DataLoadState.DataExists;
					}
					else
					{
						Globals.Message.ShowWarning(Res.GetString("CE438258-CFC3-47E1-AB7E-209B74A2DB6A", "Invalid Data, please upload a file with PEM data."));
					}
				}
			}
		}

		protected virtual Stream LoadFile()
		{
			if (FileDialog.ShowDialog() == DialogResult.OK)
			{
				return FileDialog.OpenFile();
			}
			return null;
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			if (!IsBinaryFileDataEmpty)
			{
				fBinaryFileData = null;
				State = DataLoadState.DataCleared;
			}
		}

		static string BinaryToString(byte[] binaryData)
		{
			return (binaryData != null) ? Encoding.UTF8.GetString(binaryData) : string.Empty;
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (FileDialog != null)
				{
					FileDialog.Dispose();
				}
			}

			base.Dispose(isNotFinalizing);
		}
	}
}
