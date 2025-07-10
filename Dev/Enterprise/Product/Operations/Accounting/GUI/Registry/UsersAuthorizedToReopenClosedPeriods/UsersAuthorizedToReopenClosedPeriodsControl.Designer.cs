namespace Enterprise.Accounting.Registry.GUI
{
	public partial class UsersAuthorizedToReopenClosedPeriodsControl
	{


		#region Designer generated code
		void InitializeComponent()
		{
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.UsersAuthorizedToReopenClosedPeriodsGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UsersAuthorizedToReopenClosedPeriodsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.UsersAuthorizedToReopenClosedPeriods);
			// 
			// UsersAuthorizedToReopenClosedPeriodsGrid
			// 
			this.UsersAuthorizedToReopenClosedPeriodsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UsersAuthorizedToReopenClosedPeriodsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.UsersAuthorizedToReopenClosedPeriods)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.UsersAuthorizedToReopenClosedPeriods)(null)).StaffPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.UsersAuthorizedToReopenClosedPeriods)(null)).LoginName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.UsersAuthorizedToReopenClosedPeriods)(null)).FullName)));
			this.UsersAuthorizedToReopenClosedPeriodsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UsersAuthorizedToReopenClosedPeriodsControl|6beb516f-930e-4d71-82c4-f050e7d74a75", "Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "StaffPK";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UsersAuthorizedToReopenClosedPeriodsControl|c78c0b5d-1913-4a4f-ae57-c2c700ac5eed", "Login Name");
			zTextBoxColumnStyleInfo1.ColumnName = "LoginName";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("UsersAuthorizedToReopenClosedPeriodsControl|f92440cf-c3e8-43f0-ae47-9093d7ab7384", "Full Name");
			zTextBoxColumnStyleInfo2.ColumnName = "FullName";
			this.UsersAuthorizedToReopenClosedPeriodsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.UsersAuthorizedToReopenClosedPeriodsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UsersAuthorizedToReopenClosedPeriodsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.UsersAuthorizedToReopenClosedPeriodsGrid.GridId = "58468f2f-36ae-48a3-802e-8ce3ef1055db";
			this.UsersAuthorizedToReopenClosedPeriodsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UsersAuthorizedToReopenClosedPeriodsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UsersAuthorizedToReopenClosedPeriodsGrid.LayoutKey = "zGrid1";
			this.UsersAuthorizedToReopenClosedPeriodsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UsersAuthorizedToReopenClosedPeriodsGrid.Name = "UsersAuthorizedToReopenClosedPeriodsGrid";
			this.UsersAuthorizedToReopenClosedPeriodsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 293, true);
			this.UsersAuthorizedToReopenClosedPeriodsGrid.TabIndex = 0;
			// 
			// UsersAuthorizedToReopenClosedPeriodsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UsersAuthorizedToReopenClosedPeriodsGrid);
			this.Name = "UsersAuthorizedToReopenClosedPeriodsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 293, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UsersAuthorizedToReopenClosedPeriodsGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		internal ZArchitecture.ZGrid UsersAuthorizedToReopenClosedPeriodsGrid;
	}
}
