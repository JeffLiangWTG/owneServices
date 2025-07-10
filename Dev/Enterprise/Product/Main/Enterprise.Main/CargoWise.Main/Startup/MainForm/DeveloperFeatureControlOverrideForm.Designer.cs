using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Main;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Startup;

partial class DeveloperFeatureControlOverrideForm
{
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private IContainer components = null;

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
	protected new void InitializeComponent()
	{
		Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
		this.btnReset = new Enterprise.ZArchitecture.GUI.ZButton();
		this.btnSave = new Enterprise.ZArchitecture.GUI.ZButton();
		this.btnCancel = new Enterprise.ZArchitecture.GUI.ZButton();
		this.btnSelectNone = new Enterprise.ZArchitecture.GUI.ZButton();
		this.btnSelectAll = new Enterprise.ZArchitecture.GUI.ZButton();
		this.FeaturesGrid = new Enterprise.ZArchitecture.ZGrid();
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.zPanel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.FeaturesGrid)).BeginInit();
		this.FeaturesGrid.SuspendLayout();
		this.SuspendLayout();
		// 
		// MainStatusBar
		// 
		this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 528, true);
		this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 24, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Startup.DeveloperFeatureControlOverrideForm.FeatureDataToBind);
		// 
		// zPanel1
		// 
		this.zPanel1.Controls.Add(this.btnReset);
		this.zPanel1.Controls.Add(this.btnSave);
		this.zPanel1.Controls.Add(this.btnCancel);
		this.zPanel1.Controls.Add(this.btnSelectNone);
		this.zPanel1.Controls.Add(this.btnSelectAll);
		this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 491, true);
		this.zPanel1.Name = "zPanel1";
		this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 37, true);
		this.zPanel1.TabIndex = 6;
		// 
		// btnReset
		// 
		this.btnReset.CaptionResourceString = CargoWise.Main.Res.GetData("5355075e-672c-4255-9bfd-29a4c89b9fdb", "Reset");
		this.btnReset.IsCaptionOverridden = true;
		this.btnReset.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 8, true);
		this.btnReset.Name = "btnReset";
		this.btnReset.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
		this.btnReset.TabIndex = 10;
		this.btnReset.ToolTipCaption = null;
		this.btnReset.UseVisualStyleBackColor = true;
		this.btnReset.Click += new System.EventHandler(this.Reset_Click);
		// 
		// btnSave
		// 
		this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
		this.btnSave.CaptionResourceString = CargoWise.Main.Res.GetData("68b51087-f51a-4823-966a-12777c5bcf08", "Save");
		this.btnSave.IsCaptionOverridden = true;
		this.btnSave.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(672, 7, true);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
		this.btnSave.TabIndex = 8;
		this.btnSave.ToolTipCaption = null;
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(this.Save_Click);
		// 
		// btnCancel
		// 
		this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
		this.btnCancel.CaptionResourceString = CargoWise.Main.Res.GetData("9d4dd6b8-519d-4f6c-92d8-a5a006407603", "Cancel");
		this.btnCancel.IsCaptionOverridden = true;
		this.btnCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(726, 7, true);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
		this.btnCancel.TabIndex = 9;
		this.btnCancel.ToolTipCaption = null;
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(this.Cancel_Click);
		// 
		// btnSelectNone
		// 
		this.btnSelectNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
		this.btnSelectNone.CaptionResourceString = CargoWise.Main.Res.GetData("0f7a983c-67d9-47de-842d-d0bbc8406c47", "Select None");
		this.btnSelectNone.IsCaptionOverridden = true;
		this.btnSelectNone.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 8, true);
		this.btnSelectNone.Name = "btnSelectNone";
		this.btnSelectNone.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 23, true);
		this.btnSelectNone.TabIndex = 7;
		this.btnSelectNone.ToolTipCaption = null;
		this.btnSelectNone.UseVisualStyleBackColor = true;
		this.btnSelectNone.Click += new System.EventHandler(this.SelectNone_Click);
		// 
		// btnSelectAll
		// 
		this.btnSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
		this.btnSelectAll.CaptionResourceString = CargoWise.Main.Res.GetData("a18bae64-de51-45c4-b396-412ec7e6486a", "Select All");
		this.btnSelectAll.IsCaptionOverridden = true;
		this.btnSelectAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
		this.btnSelectAll.Name = "btnSelectAll";
		this.btnSelectAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
		this.btnSelectAll.TabIndex = 6;
		this.btnSelectAll.ToolTipCaption = null;
		this.btnSelectAll.UseVisualStyleBackColor = true;
		this.btnSelectAll.Click += new System.EventHandler(this.SelectAll_Click);
		// 
		// FeaturesGrid
		// 
		this.FeaturesGrid.AllowNavigation = false;
		this.BindingSource.SetBindingMember(this.FeaturesGrid, "Data");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Startup.DeveloperFeatureControlOverrideForm.FeatureDataToBind)(null)).Data)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Startup.DeveloperFeatureControlOverrideForm.FeatureData)(((System.Collections.IList)(((Enterprise.Startup.DeveloperFeatureControlOverrideForm.FeatureDataToBind)(null)).Data)).SyncRoot)).Enabled)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Startup.DeveloperFeatureControlOverrideForm.FeatureData)(((System.Collections.IList)(((Enterprise.Startup.DeveloperFeatureControlOverrideForm.FeatureDataToBind)(null)).Data)).SyncRoot)).Code)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Startup.DeveloperFeatureControlOverrideForm.FeatureData)(((System.Collections.IList)(((Enterprise.Startup.DeveloperFeatureControlOverrideForm.FeatureDataToBind)(null)).Data)).SyncRoot)).FeatureStage)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Startup.DeveloperFeatureControlOverrideForm.FeatureData)(((System.Collections.IList)(((Enterprise.Startup.DeveloperFeatureControlOverrideForm.FeatureDataToBind)(null)).Data)).SyncRoot)).Description)));
		this.FeaturesGrid.CaptionVisible = false;
		zCheckBoxColumnStyleInfo1.CaptionResourceString = CargoWise.Main.Res.GetData("b1e447aa-b9cd-43ce-b7e0-20920769544b", "Enabled");
		zCheckBoxColumnStyleInfo1.ColumnName = "Enabled";
		zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
		zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
		zTextBoxColumnStyleInfo1.CaptionResourceString = CargoWise.Main.Res.GetData("56c94745-1369-4bf1-b9a9-f5b0612bb6d4", "Code");
		zTextBoxColumnStyleInfo1.ColumnName = "Code";
		zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
		zTextBoxColumnStyleInfo1.IsReadOnly = true;
		zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
		zTextBoxColumnStyleInfo2.CaptionResourceString = CargoWise.Main.Res.GetData("84070ee0-0b3c-49da-981f-6ea8f691ca3d", "Feature Stage");
		zTextBoxColumnStyleInfo2.ColumnName = "FeatureStage";
		zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
		zTextBoxColumnStyleInfo2.IsReadOnly = true;
		zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zTextBoxColumnStyleInfo3.CaptionResourceString = CargoWise.Main.Res.GetData("f2b0a1d4-5c8e-4f7b-9a6c-0d1e2f3a5b8c", "Description");
		zTextBoxColumnStyleInfo3.ColumnName = "Description";
		zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
		zTextBoxColumnStyleInfo3.IsReadOnly = true;
		zTextBoxColumnStyleInfo3.IsSortable = false;
		zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		this.FeaturesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
		this.FeaturesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
		this.FeaturesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
		this.FeaturesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
		this.FeaturesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.FeaturesGrid.GridId = "43c6b069-331b-45b0-b603-58a3d652dff4";
		this.FeaturesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.FeaturesGrid.LayoutKey = "FeaturesGrid";
		this.FeaturesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.FeaturesGrid.Name = "FeaturesGrid";
		this.FeaturesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 491, true);
		this.FeaturesGrid.TabIndex = 7;
		// 
		// DeveloperFeatureControlOverrideForm
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.CaptionResourceString = CargoWise.Main.Res.GetData("1356794f-c2ee-4518-9ead-a0a883674c52", "Developer Feature Control Override");
		this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 552, true);
		this.Controls.Add(this.FeaturesGrid);
		this.Controls.Add(this.zPanel1);
		this.DataSourceType = typeof(Enterprise.Startup.DeveloperFeatureControlOverrideForm.FeatureDataToBind);
		this.Name = "DeveloperFeatureControlOverrideForm";
		this.Text = "Developer Feature Control Override";
		this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DeveloperFeatureControlOverrideForm_FormClosing);
		this.Controls.SetChildIndex(this.MainStatusBar, 0);
		this.Controls.SetChildIndex(this.zPanel1, 0);
		this.Controls.SetChildIndex(this.FeaturesGrid, 0);
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.zPanel1.ResumeLayout(false);
		this.zPanel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.FeaturesGrid)).EndInit();
		this.FeaturesGrid.ResumeLayout(false);
		this.FeaturesGrid.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}
	#endregion


	private ZArchitecture.GUI.ZPanel zPanel1;
	private ZArchitecture.GUI.ZButton btnSave;
	private ZArchitecture.GUI.ZButton btnCancel;
	private ZArchitecture.GUI.ZButton btnSelectNone;
	private ZArchitecture.GUI.ZButton btnSelectAll;
	private ZGrid FeaturesGrid;
	private ZButton btnReset;

	public class FeatureData : NonPersistentBusinessObject
	{
		private ZBool enabled;
		public ZString Code { get; set; }
		public ZString FeatureStage { get; set; }
		public ZString Description { get; set; }

		public ZBool Enabled
		{
			get => enabled;
			set
			{
				SetNonPersistentPropertyValue(EnabledInfo, ref enabled, value);
			}
		}
		public ZPropertyInfo EnabledInfo => GetZPropertyInfo(nameof(Enabled));
		public string Parameter { get; set; }
	}
	public class FeatureDataCollection : NonPersistentBusinessObjectCollection<FeatureData>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FeatureData();
		}
	}
	public class FeatureDataToBind
	{
		public FeatureDataCollection Data { get; }
		public FeatureDataToBind(FeatureDataCollection data)
		{
			Data = data;
		}
	}
}
