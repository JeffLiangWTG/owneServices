namespace Enterprise.Customs.ES.GUI
{
	partial class ShowPendingDebtTransactionsForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo26 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ResultsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ResultsGrid)).BeginInit();
			this.ResultsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 272, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 16, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction);
			// 
			// ResultsGrid
			// 
			this.ResultsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ResultsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).CPL_TransactionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).CPL_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).TransactionTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).CPL_Reference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).CPL_TranValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).CPL_Comment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).CPL_AppId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).CPL_TransactionStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).TransactionStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).CPL_Procedure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).CPL_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).CPL_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).CPL_SystemLastEditUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction)(null)).CPL_SystemLastEditTimeUtc)));
			this.ResultsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("175AEB45-7652-4C4A-A855-B8B3F01A5953", "Transaction Date");
			zTextBoxColumnStyleInfo14.ColumnName = "CPL_TransactionDate";
			zTextBoxColumnStyleInfo14.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("E18B4E86-8C12-405A-A988-54104695C26C", "Type");
			zTextBoxColumnStyleInfo15.ColumnName = "CPL_TransactionType";
			zTextBoxColumnStyleInfo15.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo15.IsReadOnly = true;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("982C27AD-DE8D-460F-8C88-51A20B1C7F2D", "Type Description");
			zTextBoxColumnStyleInfo16.ColumnName = "TransactionTypeDescription";
			zTextBoxColumnStyleInfo16.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo16.IsReadOnly = true;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("ECC19E03-2560-423E-9453-097E10C472FC", "Reference");
			zTextBoxColumnStyleInfo17.ColumnName = "CPL_Reference";
			zTextBoxColumnStyleInfo17.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo17.IsReadOnly = true;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("838CD64C-B95C-4660-A061-A3A61E563A41", "Value");
			zCalcEditColumnStyleInfo2.ColumnName = "CPL_TranValue";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("C56B30C2-9324-4EBB-866F-C19859B30B2E", "Comment");
			zTextBoxColumnStyleInfo18.ColumnName = "CPL_Comment";
			zTextBoxColumnStyleInfo18.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo18.IsReadOnly = true;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("0FF8F15B-9EFF-41A2-8DD4-B715F237FE9B", "Application Id");
			zTextBoxColumnStyleInfo19.ColumnName = "CPL_AppId";
			zTextBoxColumnStyleInfo19.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo19.IsReadOnly = true;
			zTextBoxColumnStyleInfo19.IsVisible = false;
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("655A3EE1-69E2-4C96-9DF9-8A7CB94AD852", "Status");
			zTextBoxColumnStyleInfo20.ColumnName = "CPL_TransactionStatus";
			zTextBoxColumnStyleInfo20.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo20.IsReadOnly = true;
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("E89708B6-C97C-4227-85EF-5D0B7682BB16", "Status Description");
			zTextBoxColumnStyleInfo21.ColumnName = "TransactionStatusDescription";
			zTextBoxColumnStyleInfo21.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo21.IsReadOnly = true;
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("224DC2F2-4763-4603-93CA-02A8D27A085D", "Procedure");
			zTextBoxColumnStyleInfo22.ColumnName = "CPL_Procedure";
			zTextBoxColumnStyleInfo22.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo22.IsReadOnly = true;
			zTextBoxColumnStyleInfo22.IsVisible = false;
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo23.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("98C51C33-02A3-46E9-9FBE-214FD1EE64E3", "Created By");
			zTextBoxColumnStyleInfo23.ColumnName = "CPL_SystemCreateUser";
			zTextBoxColumnStyleInfo23.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo23.IsReadOnly = true;
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo24.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("7FB1C700-37BC-4E8D-8BE1-C84CE13D1B37", "Created Time");
			zTextBoxColumnStyleInfo24.ColumnName = "CPL_SystemCreateTimeUtc";
			zTextBoxColumnStyleInfo24.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo24.IsReadOnly = true;
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo25.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("4D4CD245-1591-434F-BA4B-06576CAB36F4", "Last Edit");
			zTextBoxColumnStyleInfo25.ColumnName = "CPL_SystemLastEditUser";
			zTextBoxColumnStyleInfo25.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo25.IsReadOnly = true;
			zTextBoxColumnStyleInfo25.IsVisible = false;
			zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo26.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("C4729337-FDA6-419E-A67A-B1DC0F00CC09", "Last Edited Time");
			zTextBoxColumnStyleInfo26.ColumnName = "CPL_SystemLastEditTimeUtc";
			zTextBoxColumnStyleInfo26.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo26.IsReadOnly = true;
			zTextBoxColumnStyleInfo26.IsVisible = false;
			zTextBoxColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.ResultsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
			this.ResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo26);
			this.ResultsGrid.GridId = "B3E484BD-788F-4C2A-9CCE-DAA7D1AF1B2B";
			this.ResultsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ResultsGrid.LayoutKey = "ResultsGrid";
			this.ResultsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultsGrid.Name = "ResultsGrid";
			this.ResultsGrid.ReadOnly = true;
			this.ResultsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultsGrid.TabIndex = 0;
			// 
			// ShowPendingDebtTransactionsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("B63CBAE6-2A05-4169-B53A-35815836EEBC", "Pending Debt Transactions");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 288, true);
			this.Controls.Add(this.ResultsGrid);
			this.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusGuaranteeLineTransaction);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
			this.Name = "ShowPendingDebtTransactionsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
	}
}
