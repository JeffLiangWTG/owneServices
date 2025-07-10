using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class ExportEntryFilerIDControl
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
			this.EntryFilerIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IDTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Customs.US.ExportEntryFilerID);
			// 
			// EntryFilerIDTextBox
			// 
			this.EntryFilerIDTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EntryFilerIDTextBox, "EntryFilerID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Customs.US.ExportEntryFilerID)(null)).EntryFilerID)));
			this.EntryFilerIDTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c22cfbbe-4526-4d8b-a69f-e3857d3b25e0", "Entry Filer ID");
			this.EntryFilerIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 50, true);
			this.EntryFilerIDTextBox.Name = "EntryFilerIDTextBox";
			this.EntryFilerIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 20, true);
			this.EntryFilerIDTextBox.TabIndex = 1;
			// 
			// IDTypeDropEdit
			// 
			this.IDTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IDTypeDropEdit, "EntryFilerIDType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.Customs.US.ExportEntryFilerID)(null)).EntryFilerIDType)));
			this.IDTypeDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("46174ba7-39f4-49a6-8b59-fda5b0e980e7", "ID Type");
			this.IDTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 21, true);
			this.IDTypeDropEdit.Name = "IDTypeDropEdit";
			this.IDTypeDropEdit.PreBoundMaxLength = 2;
			this.IDTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.IDTypeDropEdit.TabIndex = 0;
			// 
			// ExportEntryFilerIDControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IDTypeDropEdit);
			this.Controls.Add(this.EntryFilerIDTextBox);
			this.Name = "ExportEntryFilerIDControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 83, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.ZTextBox EntryFilerIDTextBox;
		private ZArchitecture.GUI.ZDropEdit IDTypeDropEdit;
	}
}
