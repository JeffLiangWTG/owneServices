using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	partial class ZFilterControlImages
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZFilterControlImages));
			this.ImageList = new System.Windows.Forms.ImageList(this.components);
			// 
			// ImageList
			// 
			ZImageListDpiScalingHelper.SetScaledImagesFromImageListStreamer(this.ImageList, ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList.ImageStream"))));
			this.ImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.ImageList.Images.SetKeyName(0, "PublishedLayout");
			this.ImageList.Images.SetKeyName(1, "PublishedLayoutForDropList");
			this.ImageList.Images.SetKeyName(2, "ColumnLayouts");
			this.ImageList.Images.SetKeyName(3, "SystemLayout");
			this.ImageList.Images.SetKeyName(4, "SystemLayoutForDropList");
			this.ImageList.Images.SetKeyName(5, "FilterCategoryRed");
			this.ImageList.Images.SetKeyName(6, "FilterCategoryGreen");
			this.ImageList.Images.SetKeyName(7, "FilterCategoryBlue");
			this.ImageList.Images.SetKeyName(8, "FilterCategoryBrown");
			this.ImageList.Images.SetKeyName(9, "FilterCategoryGrey");
			this.ImageList.Images.SetKeyName(10, "FilterCategories");
			this.ImageList.Images.SetKeyName(11, "FilterStrip");
			this.ImageList.Images.SetKeyName(12, "FavoriteSelected");
			this.ImageList.Images.SetKeyName(13, "FavoriteUnselected");

		}

		#endregion

		public System.Windows.Forms.ImageList ImageList;
	}
}
