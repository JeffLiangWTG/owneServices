using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class TextDialogForm : ZChildForm
	{
		public TextDialogForm(string title, string pem, SecretLabelTypes secretLabelType = SecretLabelTypes.Secret)
		{
			InitializeComponent();
			Title = title;
			SecretString = pem;
			Secret.ReadOnly = true;
			SetAndCloseButton.Visible = false;
			SaveButton.Visible = true;
			SetSecretLabel(secretLabelType);
		}

		public TextDialogForm(string title, string successMessage, Func<string, string> validationFunction, SecretLabelTypes secretLabelType = SecretLabelTypes.Secret)
		{
			InitializeComponent();
			Title = title;
			SuccessMessage = successMessage;
			Secret.ReadOnly = false;
			SetAndCloseButton.Visible = true;
			SaveButton.Visible = false;
			this.validationFunction = validationFunction;
			SetSecretLabel(secretLabelType);
		}

		public override string FormHeading
		{
			get { return Res.GetString("e8126405-e3f2-42f1-861c-a3292c5d3fe0", "{0}", Title); }
		}

		void SetAndCloseButton_Click(object sender, EventArgs e)
		{
			var result = validationFunction?.Invoke(SecretString);
			if (!string.IsNullOrEmpty(result))
			{
				Globals.Message.ShowWarning(result);
				return;
			}

			Globals.Message.Show(SuccessMessage);
			DialogResult = DialogResult.OK;
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			using (var fileDialog = new ZSaveFileDialog
			{
				DefaultExt = (NoResString)"pem",
				Filter = (NoResString)"Privacy enhanced mail files|*.pem"
			})
			{
				if (fileDialog.ShowDialog() == DialogResult.OK)
				{
					SaveContentToFile(fileDialog.OpenFile(), SecretString);
				}
			}
		}

		void SaveContentToFile(Stream fileStream, string content)
		{
			using (var sw = new StreamWriter(fileStream))
			{
				sw.Write(content);
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void SetSecretLabel(SecretLabelTypes secretLabelType)
		{
			switch (secretLabelType)
			{
				case SecretLabelTypes.Certificate:
					SecretLabel.CaptionResourceString = Res.GetData("d9fae389-1fa4-4219-9eab-970a51dacd32", "Certificate");
					break;
				case SecretLabelTypes.CSR:
					SecretLabel.CaptionResourceString = Res.GetData("0a5307d1-0649-4a69-9b92-d1df6b9ab8a3", "CSR");
					break;
				default:
					break;
			}
		}

		readonly Func<string, string> validationFunction;

		public ZString SecretString
		{
			get
			{
				return Secret.Text;
			}
			set
			{
				Secret.Text = value;
			}
		}

		ZString _successMessage;
		ZString SuccessMessage
		{
			get
			{
				return _successMessage;
			}
			set
			{
				_successMessage = value;
			}
		}

		ZString _title;
		ZString Title
		{
			get
			{
				return _title;
			}
			set
			{
				_title = value;
			}
		}

		public enum SecretLabelTypes { Certificate, CSR, Secret }
	}
}
