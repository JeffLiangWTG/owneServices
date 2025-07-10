using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;
using ResString = Enterprise.ZArchitecture.GUI.UserControls.ResString;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	[DefaultDataSourceBindingMember(null)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ImageSelectionControl /*SuppressCodeSmell Reason=This can be bound to any type of business object as it is a reusable control*/ : ZUserControl, IBindTo
	{
		protected ZButton BrowseButton;
		protected ZButton ClearImageButton;
		protected ZButton SaveImageButton;
		protected ZOpenFileDialog FileDialog;
		protected ZSaveFileDialog SaveFileDialog;
		readonly Container components;
		protected ZButton ViewModeButton;
		protected ZPictureBox fPictureBox;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Type Filter;")]
		public ImageSelectionControl()
		{
			InitializeComponent();
			FileDialog.Filter = DefaultFileDialogFilter;
			SaveFileDialog.AddExtension = true;
			SaveFileDialog.Filter = "PNG (*.PNG)|*.PNG";
			SaveFileDialog.DefaultExt = "PNG";
			SaveFileDialog.FileName = "Logo.PNG";
			ViewModeButton.ToolTipCaption = StretchModeTooltip;
			SetDataSourceBinding("ImageObjectForBinding", ".");
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Image Image
		{
			get { return (Image)ImageObjectForBinding; }
			set { ImageObjectForBinding = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object ImageObjectForBinding
		{
			get { return fPictureBox.Image; }
			set
			{
				fPictureBox.Image = (value is Image) ? (Image)value : null;
				OnImageObjectForBindingChanged();
			}
		}

		public event EventHandler ImageObjectForBindingChanged;

		protected virtual void OnImageObjectForBindingChanged()
		{
			if (ImageObjectForBindingChanged != null)
			{
				ImageObjectForBindingChanged(this, EventArgs.Empty);
			}
		}

		[DefaultValue(true)]
		public bool CanSelectImage
		{
			get { return fCanSelectImage; }
			set
			{
				BrowseButton.Visible = value;
				ClearImageButton.Visible = value;
				SaveImageButton.Visible = value;
				ViewModeButton.Visible = value;
				fCanSelectImage = value;
				fPictureBox.Dock = value ? DockStyle.None : DockStyle.Fill;
			}
		}
		bool fCanSelectImage = true;

		public bool ReadOnly
		{
			get { return !BrowseButton.Enabled; }
			set
			{
				BrowseButton.Enabled = !value;
				ClearImageButton.Enabled = !value;
				SaveImageButton.Enabled = !value;
				ViewModeButton.Enabled = !value;
			}
		}

		[DefaultValue(DefaultFileDialogFilter)]
		public string FileDialogFilter
		{
			get { return FileDialog.Filter; }
			set { FileDialog.Filter = value; }
		}

		public event EventHandler ImageObjectChangedByUser;

		#region IBindTo Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindTo
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		#endregion

		#region IDataBoundControl Members

		public override Type DataSourceType
		{
			get { return typeof(Image); }
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Type Filter")]
		const string DefaultFileDialogFilter = "Image Files (*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG";

		void BrowseButton_Click(object sender, EventArgs e)
		{
			FileDialog.ShowDialog();
		}

		void ClearImageButton_Click(object sender, EventArgs e)
		{
			var changed = (ImageObjectForBinding != null);
			ImageObjectForBinding = null;
			if (changed)
			{
				ImageObjectChangedByUser?.Invoke(this, null);
			}
		}

		void SaveImageButton_Click(object sender, EventArgs e)
		{
			if (Image == null)
			{
				return;
			}
			SaveFileDialog.ShowDialog();
		}

		bool showZoomViewMode;
		ResourceStringData StretchModeCaption => Res.GetData("1103825f-53d7-453d-8373-5a46bb211d30", "Stretch View");
		ResourceString StretchModeTooltip => ResString.GetMultilingualString("BFE5841C-44AC-40ED-B996-0AB9D1E9C921", "Showing Stretch mode. Click to switch to Zoom mode.");
		ResourceStringData ZoomModeCaption => Res.GetData("28EC272C-4FC6-441B-B256-864D00BE5A95", "Zoom View");
		ResourceString ZoomModeTooltip => ResString.GetMultilingualString("5CD0F8A9-BEFC-48FB-8123-84D5DDC29BC2", "Showing Zoom mode. Click to switch to Stretch mode.");
		void ViewModeButton_Click(object sender, EventArgs e)
		{
			showZoomViewMode = !showZoomViewMode;
			if (showZoomViewMode)
			{
				this.ViewModeButton.CaptionResourceString = ZoomModeCaption;
				this.ViewModeButton.ToolTipCaption = ZoomModeTooltip;
				this.ViewModeButton.UpdateCaption();
				this.ViewModeButton.RefreshCaptionLabel();
				this.fPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
			}
			else
			{
				this.ViewModeButton.CaptionResourceString = StretchModeCaption;
				this.ViewModeButton.ToolTipCaption = StretchModeTooltip;
				this.ViewModeButton.UpdateCaption();
				this.ViewModeButton.RefreshCaptionLabel();
				this.fPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (FileDialog != null)
				{
					FileDialog.Dispose();
				}
				if (SaveFileDialog != null)
				{
					SaveFileDialog.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void FileDialog_FileOk(object sender, CancelEventArgs e)
		{
			var previousValue = Image;
			HandleImageFromFileDialog(FileDialog.ForceLocalFile(), FileDialog.UnmappedFileName);
			if (previousValue != Image)
			{
				ImageObjectChangedByUser?.Invoke(this, null);
			}
		}

		void SaveFileDialog_FileOk(object sender, CancelEventArgs e)
		{
			try
			{
				using (var stream = SaveFileDialog.OpenFile())
				{
					Image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				}
			}
			catch (IOException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (ExternalException ex)
			{
				var errorMessage = new StringBuilder();
				errorMessage.AppendLine($"The image path to be saved: {SaveFileDialog.UnmappedFileName}");
				errorMessage.AppendLine($"Image dimensions - Height: {Image.Height}, Width: {Image.Width}");
				errorMessage.AppendLine($"Image format: {Image.RawFormat}");
				ErrorReporter.ReportOnce("A generic error occurred in GDI+ on ImageSelectionControl", errorMessage.ToString(), ex);
				throw;
			}
		}

		void HandleImageFromFileDialog(string path, string displayFileName)
		{
			try
			{
				FileInfo fi = new FileInfo(path);
				if (fi.Length > 20971520)
				{
					RaiseImageFromFileDialogError(displayFileName, Res.GetString("a1747d73-9b9f-4c1b-bc57-dbfce4858482", "The size of the image cannot be larger than 20MB."));
				}
				else
				{
					Image = ImageHelper.GetRotatedImageByExifOrientationIfRequired(path);
					if (Image.Width > MaxImageDimension || Image.Height > MaxImageDimension)
					{
						Image.Dispose();
						Image = null;
						RaiseImageFromFileDialogError(displayFileName, Res.GetString("A20ADF15-765B-49F5-987A-4D6380D77E08", "The image dimensions exceed the maximum allowed size of {0} pixels.", MaxImageDimension));
					}
				}
			}
			catch (OutOfMemoryException ex)
			{
				RaiseImageFromFileDialogError(displayFileName, ex.Message);
			}
			catch (ArgumentException ex)
			{
				RaiseImageFromFileDialogError(displayFileName, ex.Message);
			}
			catch (ExternalException ex)
			{
				RaiseImageFromFileDialogError(displayFileName, ex.Message);
			}
		}
		const int MaxImageDimension = 65535;

		protected virtual void RaiseImageFromFileDialogError(string fileName, string errorMessage)
		{
			Globals.Message.ShowError(Res.GetString("415a99ee-ba58-4624-aeaf-0510b58e69e9", "The following error occurred when attempting to load the image file \"{0}\":\r\n\r\n{1}\r\n\r\nThe image file might be corrupted or not in the correct format.", fileName, errorMessage));
		}

		#endregion

		#region Metadata

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ImageSelectionControl>()
			.Property<object>("ImageObjectForBinding", null, true)
			.Result;
		}

		#endregion
	}
}
