using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	partial class TemplateHistoryForm
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            this.HisotoryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.TemplateHistoryDetailGrid = new Enterprise.ZArchitecture.ZGrid();
            this.EmptyLabel = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.HisotoryGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TemplateHistoryDetailGrid)).BeginInit();
            this.TemplateHistoryDetailGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 275, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(849, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.Business.StmTemplateBase);
            // 
            // HisotoryGroupBox
            // 
            this.HisotoryGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("TemplateHistoryForm|4e66ab0d-24eb-459d-8812-4bc3f3ede278", "History Changes");
            this.HisotoryGroupBox.Controls.Add(this.EmptyLabel);
            this.HisotoryGroupBox.Controls.Add(this.TemplateHistoryDetailGrid);
            this.HisotoryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HisotoryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.HisotoryGroupBox.Name = "HisotoryGroupBox";
            this.HisotoryGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, 3, 7, 7, true);
            this.HisotoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(849, 299, true);
            this.HisotoryGroupBox.TabIndex = 0;
            this.HisotoryGroupBox.TabStop = false;
            // 
            // TemplateHistoryDetailGrid
            // 
            this.TemplateHistoryDetailGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.TemplateHistoryDetailGrid, "TemplateHistories");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.StmTemplateBase)(null)).TemplateHistories)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IeDocBase)(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.StmTemplateBase)(null)).TemplateHistories)).SyncRoot)).FileName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Integration.IeDocBase)(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.StmTemplateBase)(null)).TemplateHistories)).SyncRoot)).LastEditedUser)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Integration.IeDocBase)(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.StmTemplateBase)(null)).TemplateHistories)).SyncRoot)).LastEdited)));
            this.TemplateHistoryDetailGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("TemplateHistoryForm|4e66ab0d-24eb-459d-8812-4bc3f3ede278", "File Name");
            zTextBoxColumnStyleInfo1.ColumnName = "FileName";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("TemplateHistoryForm|28b21aeb-dece-4229-bfd2-f92dfc9c5208", "Last Edited By");
            zTextBoxColumnStyleInfo2.ColumnName = "LastEditedUser";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("TemplateHistoryForm|070982e1-ae96-4423-b948-95fa8fd05014", "Date(UTC)");
            zDateEditColumnStyleInfo1.ColumnName = "LastEdited";
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
            this.TemplateHistoryDetailGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.TemplateHistoryDetailGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.TemplateHistoryDetailGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.TemplateHistoryDetailGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TemplateHistoryDetailGrid.GridId = "7e4b3ae8-150f-46ec-8505-f58ce813b975";
            this.TemplateHistoryDetailGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.TemplateHistoryDetailGrid.LayoutKey = "TemplateHistoryDetailGrid";
            this.TemplateHistoryDetailGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 15, true);
            this.TemplateHistoryDetailGrid.Name = "TemplateHistoryDetailGrid";
            this.TemplateHistoryDetailGrid.ReadOnly = true;
            this.TemplateHistoryDetailGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 277, true);
            this.TemplateHistoryDetailGrid.TabIndex = 0;
            this.TemplateHistoryDetailGrid.Visible = false;
            // 
            // EmptyLabel
            // 
            this.EmptyLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EmptyLabel.Visible = false;
            this.EmptyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.EmptyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 15, true);
            this.EmptyLabel.Name = "EmptyLabel";
            this.EmptyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 277, true);
            this.EmptyLabel.TabIndex = 1;
            this.EmptyLabel.Text = Res.GetString("TemplateHistoryForm|d52be52e-42ea-43f5-a89c-4d7aa83f0b16", "The template history is empty.");
            this.EmptyLabel.UseMnemonic = false;
            this.EmptyLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.EmptyLabel.Font = new Font(EmptyLabel.Font, FontStyle.Bold);
            // 
            // TemplateHistoryForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(849, 299, true);
            this.Controls.Add(this.HisotoryGroupBox);
            this.DataSourceType = typeof(Enterprise.DocumentEngine.Business.StmTemplateBase);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 140, true);
            this.Name = "TemplateHistoryForm";
            this.Text = Res.GetString("db35f494-0f20-492d-9bb6-b15e9f1e1462","Template History");;
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TemplateHistoryForm_KeyPress);
            this.Controls.SetChildIndex(this.HisotoryGroupBox, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.HisotoryGroupBox.ResumeLayout(false);
            this.HisotoryGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TemplateHistoryDetailGrid)).EndInit();
            this.TemplateHistoryDetailGrid.ResumeLayout(false);
            this.TemplateHistoryDetailGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private	Enterprise.ZArchitecture.GUI.ZGroupBox HisotoryGroupBox;
		internal ZGrid TemplateHistoryDetailGrid;
		internal ZLabel EmptyLabel;
	}
}
