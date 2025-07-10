namespace Enterprise.Registry.GUI
{
	partial class ImportOverridesForm
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
		new void InitializeComponent()
		{
			this.browseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.levelTreeView = new Enterprise.ZArchitecture.GUI.ZTreeView();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.compareButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 178, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.RegistryImportBusinessObject);
			// 
			// browseButton
			// 
			this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.browseButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("be9d9490-27f3-45b7-930e-a2676bcf129a", "Browse");
			this.browseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(428, 25, true);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.browseButton.TabIndex = 1;
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// zTextBox1
			// 
			this.zTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox1, "FilePath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.GUI.RegistryImportBusinessObject)(null)).FilePath)));
			this.zTextBox1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f1307fe6-5480-40c8-b073-cc0bdabcc140", "File to Load");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 27, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ReadOnly = true;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 19, true);
			this.zTextBox1.TabIndex = 0;
			// 
			// levelTreeView
			// 
			this.levelTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.levelTreeView.HideSelection = false;
			this.levelTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 53, true);
			this.levelTreeView.Name = "levelTreeView";
			this.levelTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 89, true);
			this.levelTreeView.TabIndex = 2;
			this.levelTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.levelTreeView_AfterSelect);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.AutoSize = true;
			this.cancelButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e5db176c-e162-4494-a7c2-df03b6238083", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(428, 149, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// compareButton
			// 
			this.compareButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.compareButton.AutoSize = true;
			this.compareButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("87174f62-c52f-44f3-97a9-f2f4006b24e5", "Compare");
			this.compareButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 148, true);
			this.compareButton.Name = "compareButton";
			this.compareButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.compareButton.TabIndex = 3;
			this.compareButton.UseVisualStyleBackColor = true;
			this.compareButton.Click += new System.EventHandler(this.compareButton_Click);
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ad9d280d-55f3-410c-8458-4eb249ca2e22", "Level to Apply To:");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 53, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 14, true);
			this.zLabel2.TabIndex = 5;
			// 
			// ImportOverridesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b05f17e3-c170-4e9f-a3e0-ee8d331809d7", "Select File");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 202, true);
			this.Controls.Add(this.compareButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.levelTreeView);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.zTextBox1);
			this.DataSourceType = typeof(Enterprise.Registry.GUI.RegistryImportBusinessObject);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 220, true);
			this.Name = "ImportOverridesForm";
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.browseButton, 0);
			this.Controls.SetChildIndex(this.levelTreeView, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.compareButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton browseButton;
		private ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.GUI.ZButton cancelButton;
		internal ZArchitecture.GUI.ZButton compareButton;
		internal ZArchitecture.GUI.ZTreeView levelTreeView;
		private ZArchitecture.ZLabel zLabel2;

	}
}
