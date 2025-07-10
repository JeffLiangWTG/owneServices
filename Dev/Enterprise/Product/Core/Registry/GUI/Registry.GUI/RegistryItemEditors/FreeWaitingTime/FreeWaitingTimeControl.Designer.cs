namespace Enterprise.Registry.GUI
{
	internal partial class FreeWaitingTimeControl : RegistryBusinessObjectTemplateZUserControl
	{
		ZArchitecture.ZGrid FreeWaitingTimeGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			this.FreeWaitingTimeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FreeWaitingTimeGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.FreeWaitingTime);
			// 
			// FreeWaitingTimeGrid
			// 
			this.FreeWaitingTimeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FreeWaitingTimeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.FreeWaitingTime)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.FreeWaitingTime)(null)).CNTType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.FreeWaitingTime)(null)).ContainerTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.FreeWaitingTime)(null)).DropMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.FreeWaitingTime)(null)).DropModes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Registry.Business.FreeWaitingTime)(null)).CFS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Registry.Business.FreeWaitingTime)(null)).CNE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Registry.Business.FreeWaitingTime)(null)).CNR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Registry.Business.FreeWaitingTime)(null)).CYD)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Registry.Business.FreeWaitingTime)(null)).CTO)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Registry.Business.FreeWaitingTime)(null)).Other)));
			this.FreeWaitingTimeGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "ContainerTypes";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6a265f37-ac10-4f7f-9c72-ea5a5002ecd9", "Container Type");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CNTType";
			zDropEditColumnStyleInfo1.BindToList = "DropModes";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d62822b5-f29a-4b2a-aaf8-72be52d5a32c", "Drop Mode");
			zDropEditColumnStyleInfo1.ColumnName = "DropMode";
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.Caption = "";
			zTimeEditExColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("cc42a733-ce86-45c2-a574-762bec9b9fd6", "CFS");
			zTimeEditExColumnStyleInfo1.ColumnName = "CFS";
			zTimeEditExColumnStyleInfo2.AllowNegative = false;
			zTimeEditExColumnStyleInfo2.Caption = "";
			zTimeEditExColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b6636fd8-a419-4e4b-ab67-0f9c289b5b9b", "CNE");
			zTimeEditExColumnStyleInfo2.ColumnName = "CNE";
			zTimeEditExColumnStyleInfo3.AllowNegative = false;
			zTimeEditExColumnStyleInfo3.Caption = "";
			zTimeEditExColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("eab8c01e-3306-4bb3-87bc-8af51e882f9f", "CNR");
			zTimeEditExColumnStyleInfo3.ColumnName = "CNR";
			zTimeEditExColumnStyleInfo4.AllowNegative = false;
			zTimeEditExColumnStyleInfo4.Caption = "";
			zTimeEditExColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4627c186-dace-4487-85c4-895f910e0efe", "CYD");
			zTimeEditExColumnStyleInfo4.ColumnName = "CYD";
			zTimeEditExColumnStyleInfo5.AllowNegative = false;
			zTimeEditExColumnStyleInfo5.Caption = "";
			zTimeEditExColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("96f3cf09-5294-49aa-965e-90e51af320b9", "CTO");
			zTimeEditExColumnStyleInfo5.ColumnName = "CTO";
			zTimeEditExColumnStyleInfo6.AllowNegative = false;
			zTimeEditExColumnStyleInfo6.Caption = "";
			zTimeEditExColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("bed17a7b-d752-41dc-bcfd-9790f6d2dd63", "Other");
			zTimeEditExColumnStyleInfo6.ColumnName = "Other";
			this.FreeWaitingTimeGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FreeWaitingTimeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FreeWaitingTimeGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.FreeWaitingTimeGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo2);
			this.FreeWaitingTimeGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo3);
			this.FreeWaitingTimeGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo4);
			this.FreeWaitingTimeGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo5);
			this.FreeWaitingTimeGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo6);
			this.FreeWaitingTimeGrid.CopySelectedRowsAllowed = true;
			this.FreeWaitingTimeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FreeWaitingTimeGrid.GridId = "80af5ce3-8ef6-41b7-8b24-523b14c32dc0";
			this.FreeWaitingTimeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FreeWaitingTimeGrid.LayoutKey = "DateAndReferenceGrid";
			this.FreeWaitingTimeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FreeWaitingTimeGrid.Name = "FreeWaitingTimeGrid";
			this.FreeWaitingTimeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 245, true);
			this.FreeWaitingTimeGrid.TabIndex = 7;
			// 
			// FreeWaitingTimeControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FreeWaitingTimeGrid);
			this.Name = "FreeWaitingTimeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 245, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FreeWaitingTimeGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
