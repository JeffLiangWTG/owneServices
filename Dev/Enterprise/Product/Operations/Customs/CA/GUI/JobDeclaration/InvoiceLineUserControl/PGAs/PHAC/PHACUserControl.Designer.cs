namespace Enterprise.Customs.CA.GUI
{
	partial class PHACUserControl
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
		private void InitializeComponent()
		{
			this.CategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IntendedUseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExemptPathogenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UNDGGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LpcoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LPCOGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			this.MainGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CategoryDropEdit.SuspendLayout();
			this.IntendedUseCodeDropEdit.SuspendLayout();
			this.UNDGGuidFindBox.SuspendLayout();
			this.LpcoGroupBox.SuspendLayout();
			this.LPCOGridUserControl.SuspendLayout();
			this.MainGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.PHACPGAHeader);
			// 
			// CategoryDropEdit
			// 
			this.CategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "CA_Category");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.PHACPGAHeader)(null)).CA_Category)));
			this.CategoryDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("020326bb-5818-4d6e-badb-c012467cacb2", "Category");
			this.CategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(537, 13, true);
			this.CategoryDropEdit.Name = "CategoryDropEdit";
			this.CategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CategoryDropEdit.TabIndex = 2;
			// 
			// IntendedUseCodeDropEdit
			// 
			this.IntendedUseCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IntendedUseCodeDropEdit, "CA_IntendedUseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.PHACPGAHeader)(null)).CA_IntendedUseCode)));
			this.IntendedUseCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0918b6d1-8a3b-46c6-8d67-977b916508cc", "Intended Use");
			this.IntendedUseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 13, true);
			this.IntendedUseCodeDropEdit.Name = "IntendedUseCodeDropEdit";
			this.IntendedUseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.IntendedUseCodeDropEdit.TabIndex = 1;
			// 
			// ExemptPathogenCheckBox
			// 
			this.ExemptPathogenCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExemptPathogenCheckBox, "CA_ExceptPathogenToxin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.CA.Business.PHACPGAHeader)(null)).CA_ExceptPathogenToxin)));
			this.ExemptPathogenCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("da7ada2b-a54b-4895-8246-7b192afd1cf4", "Exempt for Pathogen/Toxin License Number");
			this.ExemptPathogenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExemptPathogenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(537, 41, true);
			this.ExemptPathogenCheckBox.Name = "ExemptPathogenCheckBox";
			this.ExemptPathogenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 17, true);
			this.ExemptPathogenCheckBox.TabIndex = 4;
			this.ExemptPathogenCheckBox.UseVisualStyleBackColor = true;
			// 
			// UNDGGuidFindBox
			// 
			this.UNDGGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UNDGGuidFindBox, "DangerousGoodsDGSubs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.PHACPGAHeader)(null)).DangerousGoodsDGSubs)));
			this.UNDGGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2fd9cbe1-18f9-4a12-b436-d13c9b543926", "UNDG Code");
			this.UNDGGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 39, true);
			this.UNDGGuidFindBox.Name = "UNDGDropEdit";
			this.UNDGGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.UNDGGuidFindBox.TabIndex = 3;
			// 
			// LpcoGroupBox
			// 
			this.LpcoGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("35AA886B-B203-4A34-A7FE-F83183E6E5D5", "LPCOs");
			this.LpcoGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.LpcoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LpcoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LpcoGroupBox.Name = "LpcoGroupBox";
			this.LpcoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 641, true);
			this.LpcoGroupBox.TabIndex = 1;
			this.LpcoGroupBox.TabStop = false;
			// 
			// LPCOGridUserControl
			// 
			this.LPCOGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.PHACPGAHeader)(null)).LPCOViews)));
			this.LPCOGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LPCOGridUserControl.Name = "LPCOGridUserControl";
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1088, 622, true);
			this.LPCOGridUserControl.TabIndex = 11;

			// 
			// LeftGroupBox1
			// 
			this.MainGroupBox1.Controls.Add(this.UNDGGuidFindBox);
			this.MainGroupBox1.Controls.Add(this.ExemptPathogenCheckBox);
			this.MainGroupBox1.Controls.Add(this.IntendedUseCodeDropEdit);
			this.MainGroupBox1.Controls.Add(this.CategoryDropEdit);
			this.MainGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox1.Name = "LeftGroupBox1";
			this.MainGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 151, true);
			this.MainGroupBox1.TabIndex = 0;
			this.MainGroupBox1.TabStop = false;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.SplitContainer.IsSplitterFixed = true;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.MainGroupBox1);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 796, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(50);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.LpcoGroupBox);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(50);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(121);
			this.SplitContainer.TabIndex = 8;
			// 
			// PHACUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "PHACUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 99, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CategoryDropEdit.ResumeLayout(true);
			this.CategoryDropEdit.PerformLayout();
			this.LpcoGroupBox.ResumeLayout(false);
			this.LpcoGroupBox.PerformLayout();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.IntendedUseCodeDropEdit.ResumeLayout(true);
			this.IntendedUseCodeDropEdit.PerformLayout();
			this.UNDGGuidFindBox.ResumeLayout(true);
			this.UNDGGuidFindBox.PerformLayout();
			this.MainGroupBox1.ResumeLayout(false);
			this.MainGroupBox1.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit CategoryDropEdit;
		private ZArchitecture.GUI.ZDropEdit IntendedUseCodeDropEdit;
		private ZArchitecture.GUI.ZCheckBox ExemptPathogenCheckBox;
		private ZArchitecture.GUI.ZGuidFindBox UNDGGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox LpcoGroupBox;
		internal LPCOGridUserControl LPCOGridUserControl;
		internal ZArchitecture.GUI.ZGroupBox MainGroupBox1;
		protected CargoWise.Windows.UI.KSplitContainer SplitContainer;
	}
}
