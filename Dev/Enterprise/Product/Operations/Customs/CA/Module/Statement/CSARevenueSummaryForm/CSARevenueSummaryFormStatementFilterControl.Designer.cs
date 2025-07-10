namespace Enterprise.Customs.CA.Module
{
	public partial class CSARevenueSummaryFormStatementFilterControl
	{
		protected override void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusStatementHeader)(null)).B2_StatementNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusStatementHeader)(null)).B2_OH_Importer);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusStatementHeader)(null)).B2_ImporterCustomsID);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.CusStatementHeader)(null)).B2_PeriodStartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.CusStatementHeader)(null)).B2_PeriodEndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusStatementHeader)(null)).B2_Status);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusStatementHeader)(null)).B2_SystemCreateUser);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusStatementHeader)(null)).B2_SystemCreateTimeUtc);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusStatementHeader)(null)).B2_SystemLastEditUser);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusStatementHeader)(null)).B2_SystemLastEditTimeUtc);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("B7B3E19B-A532-42BB-9495-2B622CAC89CA", "Statement Number", "RSF Statement Number");
			zTextBoxColumnStyleInfo1.ColumnName = "B2_StatementNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("C33785B8-B2AE-4380-B6EE-DDE44A9B30C6", "Importer", "Importer Name");
			zTextBoxColumnStyleInfo2.ColumnName = "ImporterName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.CA.Module.Res.GetData("CSARevenueSummaryFormStatementFilterControl|3D869A32-494C-46A5-8DD6-CBF26998EEA3", "Importer");
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("AB8677C8-FE6E-4D53-B671-A1AD0A3385F0", "Importer BN", "Importer Business Number");
			zTextBoxColumnStyleInfo3.ColumnName = "B2_ImporterCustomsID";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("A7EDB3A4-CBFF-46DC-B2DB-2476FB3C1786", "Period", "RSF Period");
			zTextBoxColumnStyleInfo4.ColumnName = "Period";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("603A4282-D450-4BE2-81D3-8D7059A5E256", "Status", "RSF Status");
			zTextBoxColumnStyleInfo5.ColumnName = "B2_Status";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("C46883A8-BC5F-42E2-A8F6-24EBF5B08F4E", "Importer Code", "Importer Organization Code");
			zTextBoxColumnStyleInfo6.ColumnName = "Importer+OH_Code";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Customs.CA.Module.Res.GetData("CSARevenueSummaryFormStatementFilterControl|3D869A32-494C-46A5-8DD6-CBF26998EEA3", "Importer");
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CusStatementHeader);
			// 
			// CSARevenueSummaryFormStatementFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "CSARevenueSummaryFormStatementFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
