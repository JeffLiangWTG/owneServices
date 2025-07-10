using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class WebCustomCssControl : ZUserControl
	{
		public WebCustomCssControl()
		{
			InitializeComponent();

			CssTextBox.TextChanged += CssTextBox_TextChanged;
		}

		void CssTextBox_TextChanged(object sender, EventArgs e)
		{
			var currentItem = CurrentCustomCss;
			if (currentItem != null && currentItem.Data != CssTextBox.Text)
			{
				currentItem.HasChanges = true;
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility", Justification = "We're not changing the result, just casting")]
		internal new WebCustomsCssCollectionWrapper CurrentDataItem => (WebCustomsCssCollectionWrapper)base.CurrentDataItem;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			InitializeUrlComboBox();
		}

		#region Import and export events

		void ImportButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.CheckPathExists = true;
				dialog.CheckFileExists = true;
				dialog.Multiselect = false;
				dialog.Filter = (NoResString)"Cascading Style Sheets (*.css)|*.css";

				var result = dialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					using (var stream = dialog.OpenFile())
					{
						CssTextBox.Text = stream.WriteToString();
					}
				}
			}
		}

		void ExportButton_Click(object sender, EventArgs e)
		{
			try
			{
				var exportFolder = GetFolderForExport();
				if (exportFolder != null)
				{
					var fileNameSuffix = CurrentCustomCss.Url;
					fileNameSuffix = fileNameSuffix.Replace(' ', '_');
					foreach (var c in Path.GetInvalidFileNameChars())
					{
						fileNameSuffix = fileNameSuffix.Replace(c.ToString(), "");
					}
					var fileName = Path.Combine(exportFolder, "WebCustomCss_" + fileNameSuffix + ".css");

					File.WriteAllText(fileName, CssTextBox.Text.Trim());
				}
			}
			catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
			{
				Globals.Message.Show(
					Res.GetString("edb0d82e-002a-4dbd-ad24-267a644912fa", "An error was encountered while attempting to export data:")
						+ System.Environment.NewLine + ex.Message,
					Res.GetString("7dede977-a7da-4dca-8811-654b67b81a85", "Export"),
					MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
			}
		}

		protected virtual string GetFolderForExport()
		{
			using (var folderBrowser = new ZFolderBrowserDialog())
			{
				folderBrowser.RequireMappablePath = true;

				return folderBrowser.ShowDialog() == DialogResult.OK ? folderBrowser.MappedSelectedPath : null;
			}
		}

		#endregion

		#region URL members and refreshing

		void UrlComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			var currentItem = CurrentCustomCss;
			if (currentItem != null)
			{
				CssTextBox.SetDataBinding(currentItem, "Data");
			}
		}

		void InitializeUrlComboBox()
		{
			UrlComboBox.Items.Clear();
			if (CurrentDataItem != null)
			{
				UrlComboBox.Items.AddRange(CurrentDataItem.Collection.ToArray());
				UrlComboBox.SelectedIndex = 0;
			}
		}

		WebCustomCssBusinessObject CurrentCustomCss => (WebCustomCssBusinessObject)UrlComboBox.SelectedItem;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Literal CSS.")]
		internal const string CssImportStatement = "@import url(\"DefaultStyle.css\");\r\n";

		#endregion
	}
}
