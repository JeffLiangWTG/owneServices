using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	partial class ECCCOzoneUserControl
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
			this.LPCOGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CASNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SplitContainerHorizontal = new CargoWise.Windows.UI.KSplitContainer();
			this.ComponentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ComponentUserControl = new Enterprise.Customs.CA.GUI.ComponentUserControl();
			this.LPCOGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LPCOGridUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainerHorizontal)).BeginInit();
			this.SplitContainerHorizontal.Panel1.SuspendLayout();
			this.SplitContainerHorizontal.Panel2.SuspendLayout();
			this.SplitContainerHorizontal.SuspendLayout();
			this.ComponentGroupBox.SuspendLayout();
			this.ComponentUserControl.SuspendLayout();
			this.LPCOGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.ECCCPGAHeader);
			// 
			// LPCOGridUserControl
			// 
			this.LPCOGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).LPCOViews)));
			this.LPCOGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LPCOGridUserControl.Name = "LPCOGridUserControl";
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 156, true);
			this.LPCOGridUserControl.TabIndex = 0;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.DetailsGroupBox);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.SplitContainerHorizontal);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 239, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(60);
			this.SplitContainer.TabIndex = 2;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.CASNumberTextBox);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 60, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			this.DetailsGroupBox.Text = "Details";
			// 
			// CASNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CASNumberTextBox, "CA_CASNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_CASNumber)));
			this.CASNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 31, true);
			this.CASNumberTextBox.Name = "CASNumberTextBox";
			this.CASNumberTextBox.CaptionResourceString = Res.GetData("60486f4b-fe9e-4457-af4b-56d5b0b614e9", "CAS Number");
			this.CASNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 20, true);
			this.CASNumberTextBox.TabIndex = 0;
			// 
			// SplitContainerHorizontal
			// 
			this.SplitContainerHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainerHorizontal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainerHorizontal.Name = "SplitContainerHorizontal";
			// 
			// SplitContainerHorizontal.Panel1
			// 
			this.SplitContainerHorizontal.Panel1.Controls.Add(this.ComponentGroupBox);
			// 
			// SplitContainerHorizontal.Panel2
			// 
			this.SplitContainerHorizontal.Panel2.Controls.Add(this.LPCOGroupBox);
			this.SplitContainerHorizontal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 175, true);
			this.SplitContainerHorizontal.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(336);
			this.SplitContainerHorizontal.TabIndex = 2;
			// 
			// ComponentGroupBox
			// 
			this.ComponentGroupBox.Controls.Add(this.ComponentUserControl);
			this.ComponentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComponentGroupBox.Name = "ComponentGroupBox";
			this.ComponentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 175, true);
			this.ComponentGroupBox.TabIndex = 1;
			this.ComponentGroupBox.TabStop = false;
			this.ComponentGroupBox.Text = "Regulated Substances";
			// 
			// ComponentUserControl
			// 
			this.ComponentUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComponentUserControl, "Components");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.ComponentCollection)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).Components)));
			this.ComponentUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ComponentUserControl.Name = "ComponentUserControl";
			this.ComponentUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 156, true);
			this.ComponentUserControl.TabIndex = 0;
			// 
			// LPCOGroupBox
			// 
			this.LPCOGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.LPCOGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LPCOGroupBox.Name = "LPCOGroupBox";
			this.LPCOGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 175, true);
			this.LPCOGroupBox.TabIndex = 2;
			this.LPCOGroupBox.TabStop = false;
			this.LPCOGroupBox.Text = "LPCOs";
			// 
			// ECCCOzoneUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SplitContainer);
			this.Name = "ECCCOzoneUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 239, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.SplitContainerHorizontal.Panel1.ResumeLayout(false);
			this.SplitContainerHorizontal.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainerHorizontal)).EndInit();
			this.SplitContainerHorizontal.ResumeLayout(false);
			this.SplitContainerHorizontal.PerformLayout();
			this.ComponentGroupBox.ResumeLayout(false);
			this.ComponentGroupBox.PerformLayout();
			this.ComponentUserControl.ResumeLayout(true);
			this.ComponentUserControl.PerformLayout();
			this.LPCOGroupBox.ResumeLayout(false);
			this.LPCOGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private CargoWise.Windows.UI.KSplitContainer SplitContainerHorizontal;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox LPCOGroupBox;
		private ZTextBox CASNumberTextBox;
		internal LPCOGridUserControl LPCOGridUserControl;
		private ZGroupBox ComponentGroupBox;
		private ComponentUserControl ComponentUserControl;
	}
}
