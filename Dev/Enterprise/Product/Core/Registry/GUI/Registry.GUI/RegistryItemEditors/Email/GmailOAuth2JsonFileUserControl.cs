using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class GmailOAuth2JsonFileUserControl : ZUserControl
	{
		public GmailOAuth2JsonFileUserControl()
		{
			InitializeComponent();
		}

		public GmailOAuth2JsonFile JsonFile
		{
			get
			{
				return jsonFile ??= new GmailOAuth2JsonFile();
			}
			set
			{
				jsonFile = value;
				txtFileName.Text = jsonFile?.FileName ?? string.Empty;
			}
		}

		GmailOAuth2JsonFile jsonFile;
		
		internal void btnChoose_Click(object sender, EventArgs e)
		{
			openFileDialog.ShowDialog();
		}

		void FileDialog_FileOk(object sender, CancelEventArgs e)
		{
			var signatureName = openFileDialog.UnmappedFileName;
			try
			{
				using (var file = openFileDialog.OpenFile())
				using (var memoryStream = new MemoryStream())
				{
					file.CopyTo(memoryStream);
					var jsonText = Encoding.UTF8.GetString(memoryStream.ToArray());
					var fileName = Path.GetFileName(signatureName);
					JsonFile = new GmailOAuth2JsonFile
					{
						JsonText = jsonText,
						FileName = fileName
					};
					txtFileName.Text = JsonFile.FileName;
				}
			}
			catch (Exception ex)
			{
				Globals.Message.ShowError(Res.GetString("240D0757-91EC-4C0A-83EB-943F00C1CA6D",
					@"Error while reading service account key file.
Error Message: {0}", ex.Message));
			}
		}

		internal void btnClear_Click(object sender, EventArgs e)
		{
			JsonFile = null;
		}
	}
}
