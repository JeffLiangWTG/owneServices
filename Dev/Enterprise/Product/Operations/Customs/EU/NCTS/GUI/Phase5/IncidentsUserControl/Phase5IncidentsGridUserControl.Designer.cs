using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5IncidentsGridUserControl
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
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.IncidentsGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IncidentsGrid)).BeginInit();
            this.IncidentsGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.EnRouteIncidentCollection);
            // 
            // IncidentsGrid
            // 
            this.IncidentsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.IncidentsGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_IncidentCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_Information)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_CustomsStatus)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_EndorsementDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_EndorsementAuthority)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_EndorsementPlace)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_EndorsementCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_TransportAtDepartureType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_TransportAtDepartureID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_RN_NKTransportAtDepartureIDNationality)));
			this.IncidentsGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.ColumnName = "BN_IncidentCode";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo1.ColumnName = "BN_Information";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.ColumnName = "BN_CustomsStatus";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo1.ColumnName = "BN_EndorsementDate";
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.ColumnName = "BN_EndorsementAuthority";
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.ColumnName = "BN_EndorsementPlace";
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo2.ColumnName = "BN_EndorsementCountryCode";
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "BN_TransportAtDepartureType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "BN_TransportAtDepartureID";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "BN_RN_NKTransportAtDepartureIDNationality";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.IncidentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.IncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.IncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.IncidentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.IncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.IncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.IncidentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.IncidentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.IncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.IncidentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.IncidentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IncidentsGrid.GridId = "501abf4e-171b-4131-8325-f69906772b38";
            this.IncidentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.IncidentsGrid.LayoutKey = "IncidentsGrid";
            this.IncidentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.IncidentsGrid.Name = "IncidentsGrid";
            this.IncidentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1476, 90, true);
            this.IncidentsGrid.TabIndex = 0;
            // 
            // Phase5IncidentsGridUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.IncidentsGrid);
            this.Name = "Phase5IncidentsGridUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1476, 90, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IncidentsGrid)).EndInit();
            this.IncidentsGrid.ResumeLayout(false);
            this.IncidentsGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid IncidentsGrid;
	}
}
