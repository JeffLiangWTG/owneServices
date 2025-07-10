namespace Enterprise.Customs.DE.GUI
{
	partial class REGLineToSplitUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.LineNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomerReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackagesAndPackageTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OwnerReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ATBNumberCodeFindBoxWithSelectedEvent = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxWithSelectedEvent();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ATBNumberCodeFindBoxWithSelectedEvent.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine);
			// 
			// LineNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LineNumberTextBox, "TSL_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(null)).TSL_LineNo)));
			this.LineNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 27, true);
			this.LineNumberTextBox.Name = "LineNumberTextBox";
			this.LineNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.LineNumberTextBox.TabIndex = 1;
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "Dec.STH_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(null)).Dec.STH_MessageStatus)));
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 50, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.MessageStatusTextBox.TabIndex = 2;
			// 
			// CustomerReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomerReferenceTextBox, "RelatedRegLineCustomerReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(null)).RelatedRegLineCustomerReference)));
			this.CustomerReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(461, 3, true);
			this.CustomerReferenceTextBox.Name = "CustomerReferenceTextBox";
			this.CustomerReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.CustomerReferenceTextBox.TabIndex = 3;
			// 
			// PackagesAndPackageTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PackagesAndPackageTypeTextBox, "RelatedRegLinePackagesAndPackageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(null)).RelatedRegLinePackagesAndPackageType)));
			this.PackagesAndPackageTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(461, 27, true);
			this.PackagesAndPackageTypeTextBox.Name = "PackagesAndPackageTypeTextBox";
			this.PackagesAndPackageTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.PackagesAndPackageTypeTextBox.TabIndex = 4;
			// 
			// OwnerReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.OwnerReferenceTextBox, "RelatedRegLineOwnerReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(null)).RelatedRegLineOwnerReference)));
			this.OwnerReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(757, 3, true);
			this.OwnerReferenceTextBox.Name = "OwnerReferenceTextBox";
			this.OwnerReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.OwnerReferenceTextBox.TabIndex = 5;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "RelatedRegLineGoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(null)).RelatedRegLineGoodsDescription)));
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(757, 27, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 6;
			// 
			// ATBNumberCodeFindBoxWithSelectedEvent
			// 
			this.ATBNumberCodeFindBoxWithSelectedEvent.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ATBNumberCodeFindBoxWithSelectedEvent, "FormattedOwnerReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(null)).FormattedOwnerReferenceNumber)));
			this.ATBNumberCodeFindBoxWithSelectedEvent.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 3, true);
			this.ATBNumberCodeFindBoxWithSelectedEvent.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.EU.DE.ImportFromSumARegister;
			this.ATBNumberCodeFindBoxWithSelectedEvent.Name = "ATBNumberCodeFindBoxWithSelectedEvent";
			this.ATBNumberCodeFindBoxWithSelectedEvent.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.EU.DE.ImportFromSumARegister;
			this.ATBNumberCodeFindBoxWithSelectedEvent.ParentType = null;
			this.ATBNumberCodeFindBoxWithSelectedEvent.PreBoundMaxLength = 30;
			this.ATBNumberCodeFindBoxWithSelectedEvent.ShouldResize = false;
			this.ATBNumberCodeFindBoxWithSelectedEvent.ShowDescriptionBox = false;
			this.ATBNumberCodeFindBoxWithSelectedEvent.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.ATBNumberCodeFindBoxWithSelectedEvent.TabIndex = 0;
			this.ATBNumberCodeFindBoxWithSelectedEvent.Selected += new Enterprise.ZArchitecture.GUI.Internal.EmbeddedModulePopup.SelectedEventHandler(this.FormattedATBNumberCodeFindBoxWithSelectedEvent_SelectedChanged);
			// 
			// REGLineToSplitUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ATBNumberCodeFindBoxWithSelectedEvent);
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.OwnerReferenceTextBox);
			this.Controls.Add(this.PackagesAndPackageTypeTextBox);
			this.Controls.Add(this.CustomerReferenceTextBox);
			this.Controls.Add(this.MessageStatusTextBox);
			this.Controls.Add(this.LineNumberTextBox);
			this.Name = "REGLineToSplitUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(955, 86, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ATBNumberCodeFindBoxWithSelectedEvent.ResumeLayout(true);
			this.ATBNumberCodeFindBoxWithSelectedEvent.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox LineNumberTextBox;
		internal ZArchitecture.ZTextBox MessageStatusTextBox;
		internal ZArchitecture.ZTextBox CustomerReferenceTextBox;
		internal ZArchitecture.ZTextBox PackagesAndPackageTypeTextBox;
		internal ZArchitecture.ZTextBox OwnerReferenceTextBox;
		internal ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		internal ZArchitecture.GUI.ZCodeFindBoxWithSelectedEvent ATBNumberCodeFindBoxWithSelectedEvent;
	}
}
