namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CcsukMainHouseUserControlHelpers
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
			this.DeliveryTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.deliveryUserControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.DeliveryUserControl();
			this.SplitsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.splitConsignmentUserControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.SplitConsignmentGridUserControl();
			this.RemovalsAndFallbackTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.underbondUserControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.UnderbondUserControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ccsukMessagesUserControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.CcsukMessagesUserControl();
			this.MainCcsukTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.CHCTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.communityHandlingCodesUserControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.CommunityHandlingCodesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeliveryTabPage.SuspendLayout();
			this.SplitsTabPage.SuspendLayout();
			this.RemovalsAndFallbackTabPage.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.MainCcsukTabControl.SuspendLayout();
			this.CHCTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB);
			// 
			// DeliveryTabPage
			//
			this.DeliveryTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("8ff2d086-a011-4816-9a76-21143f4f47bb", "Receipt/Delivery");
			this.DeliveryTabPage.Controls.Add(this.deliveryUserControl1);
			this.DeliveryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeliveryTabPage.Name = "DeliveryTabPage";
			this.DeliveryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DeliveryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 676, true);
			this.DeliveryTabPage.TabIndex = 4;
			this.DeliveryTabPage.UseVisualStyleBackColor = true;
			// 
			// deliveryUserControl1
			// 
			this.deliveryUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.deliveryUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)))));
			this.deliveryUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.deliveryUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.deliveryUserControl1.Name = "deliveryUserControl1";
			this.deliveryUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 403, true);
			this.deliveryUserControl1.TabIndex = 0;
			// 
			// SplitsTabPage
			//
			this.SplitsTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("f02ca6ba-0258-4e79-908a-1c205403f255", "Splits");
			this.SplitsTabPage.Controls.Add(this.splitConsignmentUserControl1);
			this.SplitsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SplitsTabPage.Name = "SplitsTabPage";
			this.SplitsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SplitsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 676, true);
			this.SplitsTabPage.TabIndex = 3;
			this.SplitsTabPage.UseVisualStyleBackColor = true;
			// 
			// splitConsignmentUserControl1
			// 
			this.splitConsignmentUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.splitConsignmentUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)))));
			this.splitConsignmentUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitConsignmentUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.splitConsignmentUserControl1.Name = "splitConsignmentUserControl1";
			this.splitConsignmentUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(951, 670, true);
			this.splitConsignmentUserControl1.TabIndex = 0;
			// 
			// RemovalsAndFallbackTabPage
			//
			this.RemovalsAndFallbackTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("a9512d63-df9f-4953-ae5b-c7498f0672f8", "Removals and Fallback");
			this.RemovalsAndFallbackTabPage.Controls.Add(this.underbondUserControl1);
			this.RemovalsAndFallbackTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RemovalsAndFallbackTabPage.Name = "RemovalsAndFallbackTabPage";
			this.RemovalsAndFallbackTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RemovalsAndFallbackTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 676, true);
			this.RemovalsAndFallbackTabPage.TabIndex = 1;
			this.RemovalsAndFallbackTabPage.UseVisualStyleBackColor = true;
			// 
			// underbondUserControl1
			// 
			this.underbondUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.underbondUserControl1, ".");
			this.underbondUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.underbondUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.underbondUserControl1.Name = "underbondUserControl1";
			this.underbondUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 403, true);
			this.underbondUserControl1.TabIndex = 0;
			// 
			// MessagesTabPage
			//
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("628426eb-250a-4e96-9a00-c181dd9dc5bc", "Messages");
			this.MessagesTabPage.Controls.Add(this.ccsukMessagesUserControl1);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 676, true);
			this.MessagesTabPage.TabIndex = 2;
			// 
			// ccsukMessagesUserControl1
			// 
			this.ccsukMessagesUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ccsukMessagesUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)))));
			this.ccsukMessagesUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ccsukMessagesUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ccsukMessagesUserControl1.Name = "ccsukMessagesUserControl1";
			this.ccsukMessagesUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 676, true);
			this.ccsukMessagesUserControl1.TabIndex = 0;
			// 
			// MainCcsukTabControl
			// 
			this.MainCcsukTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainCcsukTabControl.Controls.Add(this.SplitsTabPage);
			this.MainCcsukTabControl.Controls.Add(this.RemovalsAndFallbackTabPage);
			this.MainCcsukTabControl.Controls.Add(this.MessagesTabPage);
			this.MainCcsukTabControl.Controls.Add(this.DeliveryTabPage);
			this.MainCcsukTabControl.Controls.Add(this.CHCTabPage);
			this.MainCcsukTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainCcsukTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainCcsukTabControl.Name = "MainCcsukTabControl";
			this.MainCcsukTabControl.SelectedIndex = 0;
			this.MainCcsukTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 703, true);
			this.MainCcsukTabControl.TabIndex = 0;
			// 
			// CHCTabPage
			//
			this.CHCTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("8b091e4c-9707-4d88-8151-2240c093fc66", "Handling Codes");
			this.CHCTabPage.Controls.Add(this.communityHandlingCodesUserControl1);
			this.CHCTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CHCTabPage.Name = "CHCTabPage";
			this.CHCTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CHCTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 676, true);
			this.CHCTabPage.TabIndex = 5;
			this.CHCTabPage.UseVisualStyleBackColor = true;
			// 
			// communityHandlingCodesUserControl1
			// 
			this.communityHandlingCodesUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.communityHandlingCodesUserControl1, ".");
			this.communityHandlingCodesUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.communityHandlingCodesUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.communityHandlingCodesUserControl1.Name = "communityHandlingCodesUserControl1";
			this.communityHandlingCodesUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(951, 670, true);
			this.communityHandlingCodesUserControl1.TabIndex = 0;
			// 
			// CcsukMainHouseUserControlHelpers
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainCcsukTabControl);
			this.Name = "CcsukMainHouseUserControlHelpers";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 703, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeliveryTabPage.ResumeLayout(false);
			this.SplitsTabPage.ResumeLayout(false);
			this.RemovalsAndFallbackTabPage.ResumeLayout(false);
			this.MessagesTabPage.ResumeLayout(false);
			this.MainCcsukTabControl.ResumeLayout(false);
			this.CHCTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZTabPage DeliveryTabPage;
		private DeliveryUserControl deliveryUserControl1;
		private ZArchitecture.GUI.ZTabPage SplitsTabPage;
		private SplitConsignmentGridUserControl splitConsignmentUserControl1;
		private ZArchitecture.GUI.ZTabPage RemovalsAndFallbackTabPage;
		private UnderbondUserControl underbondUserControl1;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private CcsukMessagesUserControl ccsukMessagesUserControl1;
		private ZArchitecture.GUI.ZTabControl MainCcsukTabControl;
		private ZArchitecture.GUI.ZTabPage CHCTabPage;
		private CommunityHandlingCodesUserControl communityHandlingCodesUserControl1;


	}
}
