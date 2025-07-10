namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class EMCSDispatchWarehouseDocAddressControl
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DispatchWarehouseDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DispatchReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DispatchWarehouseDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration);
			// 
			// DispatchWarehouseDocAddressControl
			// 
			this.DispatchWarehouseDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DispatchWarehouseDocAddressControl, "DispatchWarehouseDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).DispatchWarehouseDocumentaryAddress)));
			this.DispatchWarehouseDocAddressControl.BindToOrganisations = "Lookups+DispatchWarehouseList";
			this.DispatchWarehouseDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("6f226ee9-7ea1-4f31-89ed-974fb9bd8b86", "Dispatch Warehouse");
			this.DispatchWarehouseDocAddressControl.IsCustomHeight = true;
			this.DispatchWarehouseDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DispatchWarehouseDocAddressControl.Name = "DispatchWarehouseDocAddressControl";
			this.DispatchWarehouseDocAddressControl.ReadOnly = false;
			this.DispatchWarehouseDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DispatchWarehouseDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 180, true);
			this.DispatchWarehouseDocAddressControl.TabIndex = 0;
			this.DispatchWarehouseDocAddressControl.ValidationJustForced = false;
			// 
			// DispatchReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.DispatchReferenceTextBox, "ZG_DispatchReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).ZG_DispatchReference)));
			this.DispatchReferenceTextBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("6d9ae7bd-e2a0-492c-99fd-f446e6f79dff", "Ref.");
			this.DispatchReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(45, 185, true);
			this.DispatchReferenceTextBox.Name = "DispatchReferenceTextBox";
			this.DispatchReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 20, true);
			this.DispatchReferenceTextBox.TabIndex = 1;
			// 
			// EMCSDispatchWarehouseDocAddressControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DispatchWarehouseDocAddressControl);
			this.Controls.Add(this.DispatchReferenceTextBox);
			this.Name = "EMCSDispatchWarehouseDocAddressControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 205, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DispatchWarehouseDocAddressControl.ResumeLayout(true);
			this.DispatchWarehouseDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox DispatchReferenceTextBox;
		private MasterFiles.GUI.ZDocAddressControl DispatchWarehouseDocAddressControl;
	}
}
