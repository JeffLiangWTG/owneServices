namespace Enterprise.Security.ActiveDirectory.GUI
{
	partial class OUPickerControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.DirectoryTreeView = new Enterprise.ZArchitecture.GUI.ZTreeView();
			this.SelectedOUTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// DirectoryTreeView
			// 
			this.DirectoryTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DirectoryTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.DirectoryTreeView.Name = "DirectoryTreeView";
			this.DirectoryTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 433, true);
			this.DirectoryTreeView.TabIndex = 0;
			// 
			// SelectedOUTextBox
			// 
			this.SelectedOUTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SelectedOUTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SelectedOUTextBox, false);
			this.SelectedOUTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 441, true);
			this.SelectedOUTextBox.Name = "SelectedOUTextBox";
			this.SelectedOUTextBox.ReadOnly = true;
			this.SelectedOUTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.SelectedOUTextBox.TabIndex = 1;
			// 
			// OUPickerControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SelectedOUTextBox);
			this.Controls.Add(this.DirectoryTreeView);
			this.Name = "OUPickerControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 464, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZTreeView DirectoryTreeView;
		private ZArchitecture.ZTextBox SelectedOUTextBox;
	}
}
