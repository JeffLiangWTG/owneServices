using Enterprise.Customs.EU.TemporaryStorage.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class TempStorageRegisterForm
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
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Text = "TempStorageRegisterForm";
			this.BindingSource.DataSourceType = typeof(CusTempStorageRegHeader);
			this.CaptionRenderingEnabled = true;
			this.DataSourceType = typeof(CusTempStorageRegHeader);
		}

		#endregion
	}
}
