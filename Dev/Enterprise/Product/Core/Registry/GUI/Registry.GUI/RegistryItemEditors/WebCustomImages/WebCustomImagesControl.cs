using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class WebCustomImagesControl : RegistryZUserControl
	{
#if DEBUG
		public WebCustomImagesControl()
		{
			CaptionRenderingEnabled = true;
		}
#endif

		public WebCustomImagesControl(IRegistryItem registryItem)
		{
			InitializeComponent();
			RegistryItem = registryItem;
			ImageList.Columns.Add((NoResString)"Force single column layout");
			ControlDpiScalingHelper.SetWidth(ImageList.Columns[0], this.ImageList.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(24), false);
			ImageList.HeaderStyle = ColumnHeaderStyle.None;
			PreviewLabel.Text = String.Empty;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public WebTrackerCustomImage[] Value
		{
			get
			{
				return ValueList.ToArray();
			}
			set
			{
				ValueList = value.ToList();
				InitializeUrlComboBox();
				RefreshImageList();
			}
		}
		List<WebTrackerCustomImage> ValueList;

		public IRegistryItem RegistryItem { get; set; }

		#region Refresh list and preview

		void RefreshImageList()
		{
			RefreshImageList(null);
		}

		void RefreshImageList(string selectedItemText)
		{
			ImageList.Items.Clear();
			foreach (var image in ValueList.Where(x => x.Url == CurrentUrl).OrderBy(x => x.Name))
			{
				if (!string.IsNullOrEmpty(image?.Name))
				{
					var item = ImageList.Items.Add(image.Name);
					item.Selected = selectedItemText != null && item.Text == selectedItemText;
				}
			}
		}

		void RefreshPreviewPictureBox(object sender, EventArgs e)
		{
			var selectedItems = GetImageListViewItem().Where(x => x != null && x.Selected);
			if (selectedItems.Count() == 1)
			{
				var image = ValueList.First(x => x.Url == CurrentUrl && x.Name == selectedItems.First().Text);
				using (var stream = new MemoryStream(image.Data))
				{
					RefreshPreviewPictureBox(stream, image.Name);
				}
			}
		}

		internal virtual IEnumerable<ListViewItem> GetImageListViewItem()
		{
			return ImageList.Items.Cast<ListViewItem>();
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
					previewImage.Dispose();
				}
			}
		}

		internal void ClearPreviewPictureBox()
		{
			if (PreviewPictureBox.Image != null)
			{
				PreviewPictureBox.Image.Dispose();
				PreviewPictureBox.Image = null;
				PreviewLabel.Text = string.Empty;
			}
		}

		#endregion

		#region Import, delete and export events

		internal void ImportButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.CheckPathExists = true;
				dialog.CheckFileExists = true;
				dialog.Multiselect = false;
				var extensions = string.Join(";", WebTrackerCustomImage.SupportedFileTypes.Select(x => "*" + x));
				dialog.Filter = string.Format(CultureInfo.CurrentCulture, (NoResString)"Image Files({0})|{0}", extensions);

				var result = dialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					using (var stream = dialog.OpenFile())
					{
						var data = stream.ToByteArray();
						if (IsValidImage(data))
						{
							AddWebTrackerCustomImage(Path.GetFileName(dialog.UnmappedFileName), data);
						}
					}
				}
			}
		}

		protected bool IsValidImage(byte[] data)
		{
			var result = true;
			if (data.Length == 0)
			{
				result = false;
			}

			try
			{
				using (var img = new Bitmap(new MemoryStream(data)))
				{ }
			}
			catch (ArgumentException)
			{
				result = false;
			}

			if (!result)
			{
				Globals.Message.Show(
				Res.GetString("1C811A2E-7CCA-4A94-8DE2-DECA73F36E3B", "The image supplied does not contain a valid image format or it is an empty file, please re-select."),
				Res.GetString("EF748983-59CA-4BC9-AAAC-E7794EA3D151", "Bad Image Format"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return result;
		}

		internal void AddWebTrackerCustomImage(string name, byte[] data)
		{
			var preexisting = ValueList.FirstOrDefault(x => x.Url == CurrentUrl && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
			if (preexisting != null)
			{
				if (Globals.Message.Show(
					Res.GetString("4ebc280b-b89a-4a5e-819d-1785e0d9157c", "There already exists an image with this name. Do you want to override that image?"),
					Res.GetString("cf0d5ad6-ffa5-467a-a570-869f3cb1e6ff", "Image already exists"),
					MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					ValueList.Remove(preexisting);
				}
				else
				{
					return;
				}
			}

			ValueList.Add(new WebTrackerCustomImage(name, CurrentUrl, data));

			NotifyChanges();
			RefreshImageList(name);
		}

		internal void DeleteButton_Click(object sender, EventArgs e)
		{
			var hasDeleted = false;
			foreach (var item in ImageList.Items.Cast<ListViewItem>())
			{
				if (item.Selected)
				{
					var imageToRemove = ValueList.FirstOrDefault(x => x.Name == item.Text && x.Url == CurrentUrl);
					if (imageToRemove != null)
					{
						ValueList.Remove(imageToRemove);
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

		internal void ExportButton_Click(object sender, EventArgs e)
		{
			using (var folderBrowser = new ZFolderBrowserDialog())
			{
				folderBrowser.RequireMappablePath = true;

				if (folderBrowser.ShowDialog() == DialogResult.OK)
				{
					try
					{
						var folderNameSuffix = (string)UrlComboBox.SelectedItem;
						foreach (var c in Path.GetInvalidFileNameChars())
						{
							folderNameSuffix = folderNameSuffix.Replace(c.ToString(), "");
						}
						var folderName = Path.Combine(folderBrowser.MappedSelectedPath, "WebCustomImages_" + folderNameSuffix);
						Directory.CreateDirectory(folderName);

						foreach (var image in ValueList.Where(x => x.Url == CurrentUrl))
						{
							File.WriteAllBytes(Path.Combine(folderName, image.Name), image.Data);
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						if (ex is UnauthorizedAccessException || ex is IOException)
						{
							Globals.Message.Show(
								Res.GetString("edb0d82e-002a-4dbd-ad24-267a644912fa", "An error was encountered while attempting to export data:")
									+ System.Environment.NewLine + ex.Message,
								Res.GetString("7dede977-a7da-4dca-8811-654b67b81a85", "Export"),
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

		#region URL members

		void UrlComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ClearPreviewPictureBox();
			RefreshImageList();
		}

		internal void InitializeUrlComboBox()
		{
			UrlComboBox.Items.Clear();
			UrlComboBox.Items.Add(Res.GetString("d5324f0e-e9fc-4076-987c-ba41c7403c48", "All URLs"));
			var webCustomImageRegistryItem = RegistryItem as WebCustomImagesRegistryItem;
			var range = webCustomImageRegistryItem.UrlsRegistryItem.Value.Union(ValueList.Where(x => !String.IsNullOrEmpty(x.Url)).Select(x => x.Url)).Distinct().ToArray();
			UrlComboBox.Items.AddRange(range);
			UrlComboBox.SelectedIndex = 0;
		}

		string CurrentUrl
		{
			get { return UrlComboBox.SelectedIndex == 0 ? String.Empty : (string)UrlComboBox.SelectedItem; }
		}

		#endregion
	}
}
