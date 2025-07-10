using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class GmailOAuth2JsonFileUserControl
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
			if (openFileDialog != null)
			{
				openFileDialog.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnChoose = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnClear = new Enterprise.ZArchitecture.GUI.ZButton();
			this.openFileDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			this.txtFileName = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// btnChoose
			// 
			this.btnChoose.IsCaptionOverridden = true;
			this.btnChoose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 1, true);
			this.btnChoose.Name = "btnChoose";
			this.btnChoose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 18, true);
			this.btnChoose.TabIndex = 0;
			this.btnChoose.Text = "Choose";
			this.btnChoose.ToolTipCaption = null;
			this.btnChoose.UseVisualStyleBackColor = true;
			this.btnChoose.Click += new System.EventHandler(this.btnChoose_Click);
			// 
			// btnClear
			// 
			this.btnClear.IsCaptionOverridden = true;
			this.btnClear.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 1, true);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 18, true);
			this.btnClear.TabIndex = 1;
			this.btnClear.Text = "Clear";
			this.btnClear.ToolTipCaption = null;
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.AddExtension = true;
			this.openFileDialog.CheckFileExists = true;
			this.openFileDialog.CheckPathExists = true;
			this.openFileDialog.DefaultExt = "";
			this.openFileDialog.DereferenceLinks = true;
			this.openFileDialog.Filter = "Json files|*.json";
			this.openFileDialog.FilterIndex = 1;
			this.openFileDialog.InitialDirectory = "";
			this.openFileDialog.Multiselect = false;
			this.openFileDialog.ReadOnlyChecked = false;
			this.openFileDialog.RestoreDirectory = false;
			this.openFileDialog.ShowHelp = false;
			this.openFileDialog.SupportMultiDottedExtensions = false;
			this.openFileDialog.Title = "Choose Service Account Key File";
			this.openFileDialog.ValidateNames = true;
			this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.FileDialog_FileOk);
			// 
			// txtFileName
			// 
			this.txtFileName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.txtFileName.Name = "txtFileName";
			this.txtFileName.ReadOnly = true;
			this.txtFileName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 15, true);
			this.txtFileName.TabIndex = 3;
			// 
			// GmailOAuth2JsonFileUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.txtFileName);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.btnChoose);
			this.Name = "GmailOAuth2JsonFileUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZButton btnChoose;
		private ZButton btnClear;
		private ZOpenFileDialog openFileDialog;
		internal ZTextBox txtFileName;
	}
}
