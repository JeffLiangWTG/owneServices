namespace Enterprise.Customs.BR.GUI
{
	partial class ForeignOperatorsUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ForeignOperatorsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ForeignOperatorsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ForeignOperatorsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ForeignOperatorsGrid)).BeginInit();
			this.ForeignOperatorsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.CusGoodsCatalog);
			// 
			// ForeignOperatorsGroupBox
			// 
			this.ForeignOperatorsGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("F0A769A5-503B-4C08-8B66-15481D01B0A6", "Foreign Operators");
			this.ForeignOperatorsGroupBox.Controls.Add(this.ForeignOperatorsGrid);
			this.ForeignOperatorsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ForeignOperatorsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ForeignOperatorsGroupBox.Name = "ForeignOperatorsGroupBox";
			this.ForeignOperatorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 105, true);
			this.ForeignOperatorsGroupBox.TabIndex = 0;
			this.ForeignOperatorsGroupBox.TabStop = false;
			// 
			// ForeignOperatorsGrid
			// 
			this.ForeignOperatorsGrid.AllowNavigation = false;
			this.ForeignOperatorsGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.ForeignOperatorsGrid, "ForeignOperators");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusGoodsCatalog)(null)).ForeignOperators)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.ForeignOperator)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusGoodsCatalog)(null)).ForeignOperators)).SyncRoot)).IsKnow)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ForeignOperator)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusGoodsCatalog)(null)).ForeignOperators)).SyncRoot)).CountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ForeignOperator)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusGoodsCatalog)(null)).ForeignOperators)).SyncRoot)).AuthorityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ForeignOperator)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusGoodsCatalog)(null)).ForeignOperators)).SyncRoot)).CGI_CustomsStatusDescription)));
			this.ForeignOperatorsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "IsKnow";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CountryCode";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "AuthorityCode";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.ColumnName = "CGI_CustomsStatusDescription";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ForeignOperatorsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ForeignOperatorsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ForeignOperatorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ForeignOperatorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ForeignOperatorsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ForeignOperatorsGrid.GridId = "cab722cc-c96d-46e4-8c92-fb73de220293";
			this.ForeignOperatorsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ForeignOperatorsGrid.LayoutKey = "ForeignOperatorsGrid";
			this.ForeignOperatorsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ForeignOperatorsGrid.Name = "ForeignOperatorsGrid";
			this.ForeignOperatorsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 86, true);
			this.ForeignOperatorsGrid.TabIndex = 0;
			// 
			// ForeignOperatorsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ForeignOperatorsGroupBox);
			this.Name = "ForeignOperatorsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 105, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ForeignOperatorsGroupBox.ResumeLayout(false);
			this.ForeignOperatorsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ForeignOperatorsGrid)).EndInit();
			this.ForeignOperatorsGrid.ResumeLayout(false);
			this.ForeignOperatorsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ForeignOperatorsGroupBox;
		internal ZArchitecture.ZGrid ForeignOperatorsGrid;
	}
}
