namespace Enterprise.Customs.ES.GUI
{
	partial class ShowResultsGridForm
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
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ResultsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ButtonOK = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ResultsGrid)).BeginInit();
			this.ResultsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 225, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 16, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.WriteOffResult);
			// 
			// ResultsGrid
			// 
			this.ResultsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ResultsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.WriteOffResult)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.WriteOffResult)(null)).JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.WriteOffResult)(null)).Mrn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.WriteOffResult)(null)).GuaranteeStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.WriteOffResult)(null)).DeclarationStatus)));
			this.ResultsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("17E62CA9-8467-40B9-BBB4-072580BBF409", "Job Number");
			zTextBoxColumnStyleInfo1.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("1B15A3FC-A1FB-4D85-A97E-2153BBCB644B", "MRN");
			zTextBoxColumnStyleInfo2.ColumnName = "Mrn";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("1F12EFF5-0823-40E2-BE71-533D690CE293", "Guarantee Status");
			zTextBoxColumnStyleInfo3.ColumnName = "GuaranteeStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("C12DCFC6-CD26-4CF3-ABC1-CFDD5EE22A89", "Declaration Status");
			zTextBoxColumnStyleInfo4.ColumnName = "DeclarationStatus";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ResultsGrid.GridId = "71b715e3-4716-477f-b3a7-a3f179dd7b8f";
			this.ResultsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ResultsGrid.LayoutKey = "ResultsGrid";
			this.ResultsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultsGrid.Name = "ResultsGrid";
			this.ResultsGrid.ReadOnly = true;
			this.ResultsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 175, true);
			this.ResultsGrid.TabIndex = 0;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ButtonOK.IsCaptionOverridden = true;
			this.ButtonOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 196, true);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ButtonOK.TabIndex = 1;
			this.ButtonOK.Text = Enterprise.Customs.ES.GUI.Res.GetString("3E83D94E-F013-4349-967E-B8C14100379B", "OK");
			this.ButtonOK.ToolTipCaption = null;
			this.ButtonOK.UseVisualStyleBackColor = true;
			// 
			// ShowResultsGridForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 241, true);
			this.Controls.Add(this.ResultsGrid);
			this.Controls.Add(this.ButtonOK);
			this.DataSourceType = typeof(Enterprise.Customs.ES.Business.WriteOffResult);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ShowResultsGridForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("624AC065-2386-4056-A133-7A523EA0E64F", "Results");
			this.Controls.SetChildIndex(this.ButtonOK, 0);
			this.Controls.SetChildIndex(this.ResultsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ResultsGrid)).EndInit();
			this.ResultsGrid.ResumeLayout(false);
			this.ResultsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid ResultsGrid;
		private Enterprise.ZArchitecture.GUI.ZButton ButtonOK;
	}
}
