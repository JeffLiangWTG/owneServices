namespace Enterprise.Accounting.Registry.GUI
{
	public partial class DirectDebitFileCreationURLsControl
	{

		#region Designer generated code

		void InitializeComponent()
		{
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DirectDebitFileURLGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DirectDebitFileURLGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.DirectDebitFileCreationURL);
			// 
			// DirectDebitFileURLGrid
			// 
			this.DirectDebitFileURLGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DirectDebitFileURLGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.DirectDebitFileCreationURL)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Registry.Business.DirectDebitFileCreationURL)(null)).BankAccountPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.DirectDebitFileCreationURL)(null)).BankAccountList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.DirectDebitFileCreationURL)(null)).BankAccountDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Registry.Business.DirectDebitFileCreationURL)(null)).AllowAutoDDR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.DirectDebitFileCreationURL)(null)).DDRFormat)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.DirectDebitFileCreationURL)(null)).BankWebsite)));
			this.DirectDebitFileURLGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitFileCreationURLsControl|901ee0c0-afcc-4939-a305-053ff1913a40", "Bank Account");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "BankAccountPK";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitFileCreationURLsControl|3dd6e0d5-91e7-4a74-a5da-1b5dcf39003c", "Bank Account Description");
			zTextBoxColumnStyleInfo1.ColumnName = "BankAccountDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitFileCreationURLsControl|a4bcb56d-1215-4434-8f28-cc1f1a950679", "Allow Auto DDR");
			zCheckBoxColumnStyleInfo1.ColumnName = "AllowAutoDDR";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitFileCreationURLsControl|7e2308f5-b72c-41a9-8bf1-a4564943a5a1", "DDR File Format");
			zTextBoxColumnStyleInfo2.ColumnName = "DDRFormat";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitFileCreationURLsControl|015803ca-d0ab-4dd7-b17e-35b1377a2b68", "URL");
			zTextBoxColumnStyleInfo3.ColumnName = "BankWebsite";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.DirectDebitFileURLGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.DirectDebitFileURLGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DirectDebitFileURLGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DirectDebitFileURLGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DirectDebitFileURLGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DirectDebitFileURLGrid.GridId = "D54BDD4E-0ECF-4B27-B841-35138B4BBBA6";
			this.DirectDebitFileURLGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DirectDebitFileURLGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DirectDebitFileURLGrid.LayoutKey = "zGrid1";
			this.DirectDebitFileURLGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DirectDebitFileURLGrid.Name = "DirectDebitFileURLGrid";
			this.DirectDebitFileURLGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 293, true);
			this.DirectDebitFileURLGrid.TabIndex = 0;
			// 
			// DirectDebitFileCreationURLsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DirectDebitFileURLGrid);
			this.Name = "DirectDebitFileCreationURLsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 293, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DirectDebitFileURLGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		ZArchitecture.ZGrid DirectDebitFileURLGrid;

	}
}
