using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class WebThemeSelectorChildControl : RegistryZUserControl
	{
		public WebThemeSelectorChildControl()
		{
			InitializeComponent();
			ImageList.Columns.Add((NoResString)"Force single column layout");
			ControlDpiScalingHelper.SetWidth(ImageList.Columns[0], this.ImageList.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(24), false);
			ImageList.HeaderStyle = ColumnHeaderStyle.None;
			PreviewLabel.Text = String.Empty;
		}

		public void SetReadOnly(bool value)
		{
			CssTextBox.ReadOnly = value;
			ImportButton.ReadOnly = value;
			ExportButton.ReadOnly = value;
			DeleteButton.ReadOnly = value;
			ImportCSSButton.ReadOnly = value;
			ExportCSSButton.ReadOnly = value;
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			SetDefaultValueAsReadOnly();
			ClearPreviewPictureBox();
			RefreshImageList();
		}

		new public WebThemeCustomObject DataSource
		{
			get { return CurrentDataItem as WebThemeCustomObject; }
		}

		void SetDefaultValueAsReadOnly()
		{
			if (DataSource != null && DataSource.ThemeName == WebThemeCustomObject.Schema.DefaultThemeName)
			{
				SetReadOnly(true);
			}
		}

#if DEBUG
		protected virtual
#endif
 void RefreshPreviewPictureBox(Stream stream, string name)
		{
			PreviewLabel.Text = name;
			if (PreviewPictureBox.Image != null)
			{
				PreviewPictureBox.Image.Dispose();
			}
			using (var previewImage = new Bitmap(stream))
			{
				double width = PreviewPictureBox.Width;
				double height = PreviewPictureBox.Height;
				if (previewImage.Width < width && previewImage.Height < height)
				{
					PreviewPictureBox.Image = new Bitmap(previewImage);
				}
				else
				{
					var scale = Math.Min(width / previewImage.Width, height / previewImage.Height);
					var newWidth = (int)(previewImage.Width * scale);
					var newHeight = (int)(previewImage.Height * scale);
					if (newWidth > 0 && newHeight > 0)
					{
						PreviewPictureBox.Image = new Bitmap(previewImage, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(newWidth), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(newHeight)));
					}
					else
					{
						PreviewPictureBox.Image = null;
					}
				}
			}
		}

		void RefreshImageList()
		{
			if (DataSource != null)
			{
				ImageList.Items.Clear();
				foreach (WebCustomThemeImageBusinessObject data in DataSource.ImageCollection)
				{
					ImageList.Items.Add(data.ImageName);
				}
			}
		}

		void RefreshPreviewPictureBox(object sender, EventArgs e)
		{
			if (DataSource != null)
			{
				var selectedItems = ImageList.Items.OfType<ListViewItem>().Where(x => x.Selected);
				if (selectedItems.Count() == 1)
				{
					var image = DataSource.ImageCollection.Cast<WebCustomThemeImageBusinessObject>().FirstOrDefault(i => i.ImageName == ImageList.SelectedItems[0].Text);
					if (image.Data != null)
					{
						using (var stream = new MemoryStream(image.Data))
						{
							RefreshPreviewPictureBox(stream, image.ImageName);
						}
					}
				}
			}
		}

		void ClearPreviewPictureBox()
		{
			if (PreviewPictureBox.Image != null)
			{
				PreviewPictureBox.Image.Dispose();
				PreviewPictureBox.Image = null;
				PreviewLabel.Text = String.Empty;
			}
		}

		#region IMAGE Import, delete and export events

		string[] SupportedFileTypes()
		{
			return new string[] { ".bmp", ".png", ".gif", ".jpg", ".jpeg" };
		}

		void ImportButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.CheckPathExists = true;
				dialog.CheckFileExists = true;
				dialog.Multiselect = false;
				var extensions = String.Join(";", SupportedFileTypes().Select(x => "*" + x));
				dialog.Filter = String.Format(CultureInfo.CurrentCulture, (NoResString)"Image Files({0})|{0}", extensions);

				var result = dialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					using (var stream = dialog.OpenFile())
					{
						var data = stream.ToByteArray();
						if (!IsEmptyImage(data))
						{
							AddWebCustomImage(Path.GetFileName(dialog.UnmappedFileName), data);
						}
					}
				}
			}
		}

		protected bool IsEmptyImage(byte[] data)
		{
			var result = false;
			if (data.Length == 0)
			{
				Globals.Message.Show(
				Res.GetString("e5317327-b6ef-452c-a416-ed9ef261eb71", "The image supplied does not contain a valid image format or it is an empty file, please re-select."),
				Res.GetString("ef552406-1e18-4151-bf9f-d3e69d706901", "Bad Image Format"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				result = true;
			}
			return result;
		}

		internal void AddWebCustomImage(string name, byte[] data)
		{
			var preexisting = DataSource.ImageCollection.Cast<WebCustomThemeImageBusinessObject>().FirstOrDefault(x => name.Equals(x.ImageName, StringComparison.OrdinalIgnoreCase));
			if (preexisting != null)
			{
				if (Globals.Message.Show(
					Res.GetString("4ebc280b-b89a-4a5e-819d-1785e0d9157c", "There already exists an image with this name. Do you want to override that image?"),
					Res.GetString("cf0d5ad6-ffa5-467a-a570-869f3cb1e6ff", "Image already exists"),
					MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					preexisting.Data = data;
				}
				else
				{
					return;
				}
			}
			else
			{
				DataSource.ImageCollection.Add(new WebCustomThemeImageBusinessObject()
				{
					ImageName = name,
					Data = data
				});
			}

			RefreshImageList();
			ClearPreviewPictureBox();
			NotifyChanges();
		}

		void DeleteButton_Click(object sender, EventArgs e)
		{
			var hasDeleted = false;
			foreach (var item in ImageList.Items.OfType<ListViewItem>())
			{
				if (item.Selected)
				{
					var imageToRemove = DataSource.ImageCollection.Cast<WebCustomThemeImageBusinessObject>().FirstOrDefault(image => image.ImageName == item.Text);
					if (imageToRemove != null)
					{
						DataSource.ImageCollection.Remove(imageToRemove);
					}
					hasDeleted = true;
				}
			}
			if (hasDeleted)
			{
				RefreshImageList();
				ClearPreviewPictureBox();
				NotifyChanges();
			}
		}

		void ExportButton_Click(object sender, EventArgs e)
		{
			using (var folderBrowser = new ZFolderBrowserDialog())
			{
				folderBrowser.RequireMappablePath = true;

				if (folderBrowser.ShowDialog() == DialogResult.OK)
				{
					try
					{
						var folderNameSuffix = DataSource.ThemeName;
						foreach (var c in Path.GetInvalidFileNameChars())
						{
							folderNameSuffix = folderNameSuffix.Replace(c.ToString(), "");
						}
						var folderName = Path.Combine(folderBrowser.MappedSelectedPath, "WebCustomImages_" + folderNameSuffix);
						Directory.CreateDirectory(folderName);

						foreach (var image in DataSource.ImageCollection.Cast<WebCustomThemeImageBusinessObject>())
						{
							File.WriteAllBytes(Path.Combine(folderName, image.ImageName), image.Data);
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						if (ex is UnauthorizedAccessException || ex is IOException)
						{
							Globals.Message.Show(
								Res.GetString("101316ee-d35b-48ce-acc6-b7cca976bf65", "An error was encountered while attempting to export data:")
									+ System.Environment.NewLine + ex.Message,
								Res.GetString("ad821ca4-f24f-401c-a1a0-10e63c34205d", "Export"),
								MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
						}
						else
						{
							throw;
						}
					}
				}
			}
		}

		#endregion

		#region CSS Import and export events

		void ImportCSSButton_Click(object sender, EventArgs e)
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
						NotifyChanges();
					}
				}
			}
		}

		void ExportCSSButton_Click(object sender, EventArgs e)
		{
			try
			{
				using (var folderBrowser = new ZFolderBrowserDialog())
				{
					folderBrowser.RequireMappablePath = true;

					if (folderBrowser.ShowDialog() == DialogResult.OK)
					{
						var fileNameSuffix = DataSource.ThemeName;
						fileNameSuffix = fileNameSuffix.Replace(' ', '_');
						foreach (var c in Path.GetInvalidFileNameChars())
						{
							fileNameSuffix = fileNameSuffix.Replace(c.ToString(), "");
						}
						var fileName = Path.Combine(folderBrowser.MappedSelectedPath, "WebCustomCss_" + fileNameSuffix + ".css");

						File.WriteAllText(fileName, CssTextBox.Text.Trim());
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ex is UnauthorizedAccessException || ex is IOException)
				{
					Globals.Message.Show(
						Res.GetString("6fa87f65-af35-40c8-8cf6-8bbdec98f684", "An error was encountered while attempting to export data:")
							+ System.Environment.NewLine + ex.Message,
						Res.GetString("fbc13b6f-61ef-465f-be83-7a194062836c", "Export"),
						MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
				}
				else
				{
					throw;
				}
			}
		}

		#endregion
	}
}
