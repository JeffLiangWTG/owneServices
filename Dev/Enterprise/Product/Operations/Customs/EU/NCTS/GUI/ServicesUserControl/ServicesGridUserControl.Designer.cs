
namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class ServicesGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ServicesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).BeginInit();
			this.ServicesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.IHaveServices);
			// 
			// ServicesGrid
			// 
			this.ServicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ServicesGrid, "Services");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_OH_Contractor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ServiceProviderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_OA_Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_SubLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Booked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Completed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Duration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_RX_NKServiceRateCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_MeasurementBasis)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceNote)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_References)));
			this.ServicesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "ES_ServiceCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ES_OH_Contractor";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("0316471f-8ed5-43b7-a1dc-88325f9cfc7a", "Location", "Serv. Location", "Service Location", "");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ServiceProviderPK";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zAddressDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("ab105da6-efd1-4062-b064-93e519de9ed3", "Serv. Loc.", "Serv. Loc. Address", "Service Location Address", "");
			zAddressDropEditColumnStyleInfo1.ColumnName = "ES_OA_Location";
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("3a6f8b2d-c1a7-4dae-9d2f-3dd0f4a6d84f", "Sub Loc.", "", "Sub Location", "");
			zTextBoxColumnStyleInfo1.ColumnName = "ES_SubLocation";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(126);
			zDateEditColumnStyleInfo1.ColumnName = "ES_Booked";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ES_ServiceCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo2.ColumnName = "ES_Completed";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTimeEditExColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("7012f39a-436f-44be-b931-b04f3ed08195", "Duration", "Serv. Duration", "Service Duration", "");
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.ColumnName = "ES_Duration";
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("03e359cf-b027-480a-be89-1c9302a58b3a", "Rate", "Serv. Rate", "Service Rate", "");
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "ES_ServiceRate";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("9ebb79c8-1ba0-4040-a9a5-97e4bc5d8d7c", "Currency", "Serv. Rate Currency", "Service Rate Currency", "");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ES_RX_NKServiceRateCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("994d398d-8e30-4e60-b7e7-2c45acc5d136", "Meas. Basis", "", "Measurement Basis", "");
			zDropEditColumnStyleInfo2.ColumnName = "ES_MeasurementBasis";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("7899c117-9cb7-43b4-a625-f5d082be40e6", "Notes", "Serv. Notes", "Service Notes", "");
			zTextBoxColumnStyleInfo2.ColumnName = "ES_ServiceNote";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("788d675c-302f-41f7-894e-5a59c984ea4e", "Reference", "Serv. Reference", "Service Reference", "");
			zTextBoxColumnStyleInfo3.ColumnName = "ES_References";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			this.ServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ServicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesGrid.GridId = "BC5C1061-FC3C-411E-B588-FF29E770026F";
			this.ServicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServicesGrid.LayoutKey = "ServicesGrid";
			this.ServicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServicesGrid.Name = "ServicesGrid";
			this.ServicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 183, true);
			this.ServicesGrid.TabIndex = 1;
			// 
			// ServicesGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ServicesGrid);
			this.Name = "ServicesGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 183, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).EndInit();
			this.ServicesGrid.ResumeLayout(false);
			this.ServicesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid ServicesGrid;
	}
}
