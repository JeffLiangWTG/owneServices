namespace Enterprise.Customs.CN.GUI
{
	partial class AttachmentsUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.AttachmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AttachmentsGrid)).BeginInit();
			this.AttachmentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.CusEntryInstruction);
			// 
			// AttachmentsGrid
			// 
			this.AttachmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AttachmentsGrid, "Attachments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).Attachments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.EntryInstructionAttachment)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).Attachments)).SyncRoot)).AttachmentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.EntryInstructionAttachment)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).Attachments)).SyncRoot)).AttachmentTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.EntryInstructionAttachment)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).Attachments)).SyncRoot)).AttachmentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CN.Business.EntryInstructionAttachment)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).Attachments)).SyncRoot)).EDoc)));
			this.AttachmentsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "AttachmentType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.ColumnName = "AttachmentTypeDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "AttachmentNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidDropEditColumnStyleInfo1.ColumnName = "EDoc";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.AttachmentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AttachmentsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.AttachmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttachmentsGrid.GridId = "ceefdbc8-7ee1-43b0-b6bd-7fbaf25d1f6d";
			this.AttachmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AttachmentsGrid.LayoutKey = "AttachmentsGrid";
			this.AttachmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AttachmentsGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.AttachmentsGrid.Name = "AttachmentsGrid";
			this.AttachmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 228, true);
			this.AttachmentsGrid.TabIndex = 0;
			// 
			// AttachmentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AttachmentsGrid);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.Name = "AttachmentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 228, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AttachmentsGrid)).EndInit();
			this.AttachmentsGrid.ResumeLayout(false);
			this.AttachmentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid AttachmentsGrid;
	}
}
