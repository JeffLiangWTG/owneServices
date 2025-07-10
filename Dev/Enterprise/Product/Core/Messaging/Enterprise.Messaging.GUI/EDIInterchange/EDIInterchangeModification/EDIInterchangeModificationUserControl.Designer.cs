namespace Enterprise.Messaging.GUI
{
	partial class EDIInterchangeModificationUserControl
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (fileUploadDialog != null)
				{
					fileUploadDialog.Dispose();
				}
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
			this.TopSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.HeaderTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HeaderTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BottomSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.BodyTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zButtonUploadFile = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BodyTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FooterTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FooterTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.fileUploadDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopSplitContainer.Panel1.SuspendLayout();
			this.TopSplitContainer.Panel2.SuspendLayout();
			this.TopSplitContainer.SuspendLayout();
			this.HeaderTextGroupBox.SuspendLayout();
			this.BottomSplitContainer.Panel1.SuspendLayout();
			this.BottomSplitContainer.Panel2.SuspendLayout();
			this.BottomSplitContainer.SuspendLayout();
			this.BodyTextGroupBox.SuspendLayout();
			this.FooterTextGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Messaging.Business.EDIInterchange);
			// 
			// TopSplitContainer
			// 
			this.TopSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopSplitContainer.Name = "TopSplitContainer";
			this.TopSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// TopSplitContainer.Panel1
			// 
			this.TopSplitContainer.Panel1.Controls.Add(this.HeaderTextGroupBox);
			this.TopSplitContainer.Panel1MinSize = 100;
			// 
			// TopSplitContainer.Panel2
			// 
			this.TopSplitContainer.Panel2.Controls.Add(this.BottomSplitContainer);
			this.TopSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 315, true);
			this.TopSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.TopSplitContainer.TabIndex = 0;
			this.TopSplitContainer.TabStop = false;
			// 
			// HeaderTextGroupBox
			// 
			this.HeaderTextGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIInterchangeModificationUserControl|51adbb88-c67c-4c18-854c-65e56b723193", "Header Text");
			this.HeaderTextGroupBox.Controls.Add(this.HeaderTextTextBox);
			this.HeaderTextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderTextGroupBox.Name = "HeaderTextGroupBox";
			this.HeaderTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 100, true);
			this.HeaderTextGroupBox.TabIndex = 2;
			this.HeaderTextGroupBox.TabStop = false;
			// 
			// HeaderTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.HeaderTextTextBox, "EI_HeaderText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIInterchange)(null)).EI_HeaderText)));
			this.HeaderTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HeaderTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.HeaderTextTextBox.Multiline = true;
			this.HeaderTextTextBox.Name = "HeaderTextTextBox";
			this.HeaderTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.HeaderTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 81, true);
			this.HeaderTextTextBox.TabIndex = 0;
			// 
			// BottomSplitContainer
			// 
			this.BottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BottomSplitContainer.Name = "BottomSplitContainer";
			this.BottomSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// BottomSplitContainer.Panel1
			// 
			this.BottomSplitContainer.Panel1.Controls.Add(this.BodyTextGroupBox);
			this.BottomSplitContainer.Panel1MinSize = 100;
			// 
			// BottomSplitContainer.Panel2
			// 
			this.BottomSplitContainer.Panel2.Controls.Add(this.FooterTextGroupBox);
			this.BottomSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 211, true);
			this.BottomSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.BottomSplitContainer.TabIndex = 0;
			this.BottomSplitContainer.TabStop = false;
			// 
			// BodyTextGroupBox
			// 
			this.BodyTextGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIInterchangeModificationUserControl|d3c951a5-283c-443d-85f9-1bb1b9316560", "Body Text");
			this.BodyTextGroupBox.Controls.Add(this.zButtonUploadFile);
			this.BodyTextGroupBox.Controls.Add(this.BodyTextTextBox);
			this.BodyTextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BodyTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BodyTextGroupBox.Name = "BodyTextGroupBox";
			this.BodyTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 100, true);
			this.BodyTextGroupBox.TabIndex = 3;
			this.BodyTextGroupBox.TabStop = false;
			// 
			// zButtonUploadFile
			// 
			this.zButtonUploadFile.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIInterchangeModificationUserControl|1c11b8dc-520c-4ea1-8845-4036f0779efc", "Upload Body Text from File", "Upload Body Text from File", "Upload Body Text from File", "");
			this.zButtonUploadFile.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(39, 46, true);
			this.zButtonUploadFile.Name = "zButtonUploadFile";
			this.zButtonUploadFile.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 23, true);
			this.zButtonUploadFile.TabIndex = 1;
			this.zButtonUploadFile.UseVisualStyleBackColor = true;
			this.zButtonUploadFile.Visible = false;
			this.zButtonUploadFile.Click += new System.EventHandler(this.zButtonUploadFile_Click);
			// 
			// BodyTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.BodyTextTextBox, "EI_BodyText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIInterchange)(null)).EI_BodyText)));
			this.BodyTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BodyTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BodyTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BodyTextTextBox.Multiline = true;
			this.BodyTextTextBox.Name = "BodyTextTextBox";
			this.BodyTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.BodyTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 81, true);
			this.BodyTextTextBox.TabIndex = 0;
			this.BodyTextTextBox.Visible = false;
			// 
			// FooterTextGroupBox
			// 
			this.FooterTextGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIInterchangeModificationUserControl|a2039f91-61cb-4686-96b2-1956445740e9", "Footer Text");
			this.FooterTextGroupBox.Controls.Add(this.FooterTextTextBox);
			this.FooterTextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FooterTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FooterTextGroupBox.Name = "FooterTextGroupBox";
			this.FooterTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 107, true);
			this.FooterTextGroupBox.TabIndex = 4;
			this.FooterTextGroupBox.TabStop = false;
			// 
			// FooterTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.FooterTextTextBox, "EI_FooterText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIInterchange)(null)).EI_FooterText)));
			this.FooterTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FooterTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FooterTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FooterTextTextBox.Multiline = true;
			this.FooterTextTextBox.Name = "FooterTextTextBox";
			this.FooterTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.FooterTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 88, true);
			this.FooterTextTextBox.TabIndex = 0;
			// 
			// fileUploadDialog
			// 
			this.fileUploadDialog.AddExtension = false;
			this.fileUploadDialog.RestoreDirectory = true;
			// 
			// EDIInterchangeModificationUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TopSplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 315, true);
			this.Name = "EDIInterchangeModificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 315, true);
			this.Load += new System.EventHandler(this.EDIInterchangeModificationUserControl_Load);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopSplitContainer.Panel1.ResumeLayout(false);
			this.TopSplitContainer.Panel2.ResumeLayout(false);
			this.TopSplitContainer.ResumeLayout(false);
			this.HeaderTextGroupBox.ResumeLayout(false);
			this.HeaderTextGroupBox.PerformLayout();
			this.BottomSplitContainer.Panel1.ResumeLayout(false);
			this.BottomSplitContainer.Panel2.ResumeLayout(false);
			this.BottomSplitContainer.ResumeLayout(false);
			this.BodyTextGroupBox.ResumeLayout(false);
			this.BodyTextGroupBox.PerformLayout();
			this.FooterTextGroupBox.ResumeLayout(false);
			this.FooterTextGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer TopSplitContainer;
		private Enterprise.ZArchitecture.GUI.ZGroupBox HeaderTextGroupBox;
		private Enterprise.ZArchitecture.ZTextBox HeaderTextTextBox;
		private CargoWise.Windows.UI.KSplitContainer BottomSplitContainer;
		private Enterprise.ZArchitecture.GUI.ZGroupBox BodyTextGroupBox;
		private Enterprise.ZArchitecture.ZTextBox BodyTextTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox FooterTextGroupBox;
		private Enterprise.ZArchitecture.ZTextBox FooterTextTextBox;
		private ZArchitecture.GUI.ZButton zButtonUploadFile;
		private Enterprise.ZArchitecture.GUI.ZOpenFileDialog fileUploadDialog;
	}
}
