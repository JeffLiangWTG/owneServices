using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class ContainersUserControl
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
			this.edSecondSealNum = new Enterprise.ZArchitecture.ZTextBox();
			this.IsControlCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsUnloadedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WeightsGroupBox.SuspendLayout();
			this.ExportContainerModeDropEdit.SuspendLayout();
			this.ExportContainerTypeGuidFindBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.DetailTabControl.SuspendLayout();
			this.DeliveryModeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ImportTabPage
			// 
			this.ImportTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ImportTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.ImportTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ImportTabPage_InitializeTab));
			// 
			// ExportTabPage
			// 
			this.ExportTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ExportTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.ExportTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ExportTabPage_InitializeTab));
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.IsControlCheckBox);
			this.DetailsGroupBox.Controls.Add(this.IsUnloadedCheckBox);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 135, true);
			this.DetailsGroupBox.Controls.SetChildIndex(this.IsUnloadedCheckBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.IsControlCheckBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.ImportIsSealOkCheckBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.ExportIsEmptyContainerCheckBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.ExportIsDamagedCheckBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JC_IsShipperOwnedCheckBox, 0);
			// 
			// OutturnTabPage
			// 
			this.OutturnTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.OutturnTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.OutturnTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.OutturnTabPage_InitializeTab));
			// 
			// DeliveryModeDropEdit
			// 
			this.DeliveryModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 46, true);
			this.DeliveryModeDropEdit.TabIndex = 5;
			// 
			// VGMTabPage
			// 
			this.VGMTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.VGMTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.VGMTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.VGMTabPage_InitializeTab));
			// 
			// FreightRatesTabPage
			// 
			this.FreightRatesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.FreightRatesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.FreightRatesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.FreightRatesTabPage_InitializeTab));
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.CusContainer);
			// 
			// edSecondSealNum
			// 
			this.BindingSource.SetBindingMember(this.edSecondSealNum, "CO_SecondSeal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusContainer)(null)).CO_SecondSeal)));
			this.edSecondSealNum.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 46, true);
			this.edSecondSealNum.Name = "edSecondSealNum";
			this.edSecondSealNum.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 17, true);
			this.edSecondSealNum.TabIndex = 4;
			// 
			// IsControlCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsControlCheckBox, "ZG_IsControl");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.CusContainer)(null)).ZG_IsControl)));
			this.IsControlCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsControlCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 111, true);
			this.IsControlCheckBox.Name = "IsControlCheckBox";
			this.IsControlCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.IsControlCheckBox.TabIndex = 7;
			// 
			// IsUnloadedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsUnloadedCheckBox, "ZG_IsUnloaded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.CusContainer)(null)).ZG_IsUnloaded)));
			this.IsUnloadedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsUnloadedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 111, true);
			this.IsUnloadedCheckBox.Name = "IsUnloadedCheckBox";
			this.IsUnloadedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.IsUnloadedCheckBox.TabIndex = 8;
			// 
			// ContainersUserControl
			// 
			this.Controls.Add(this.edSecondSealNum);
			this.Name = "ContainersUserControl";
			this.Controls.SetChildIndex(this.edContainerNum, 0);
			this.Controls.SetChildIndex(this.edSecondSealNum, 0);
			this.Controls.SetChildIndex(this.DetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.WeightsGroupBox, 0);
			this.Controls.SetChildIndex(this.DetailTabControl, 0);
			this.Controls.SetChildIndex(this.edSealNum, 0);
			this.Controls.SetChildIndex(this.ExportContainerTypeGuidFindBox, 0);
			this.Controls.SetChildIndex(this.ExportContainerModeDropEdit, 0);
			this.Controls.SetChildIndex(this.DeliveryModeDropEdit, 0);
			this.WeightsGroupBox.ResumeLayout(false);
			this.WeightsGroupBox.PerformLayout();
			this.ExportContainerModeDropEdit.ResumeLayout(true);
			this.ExportContainerModeDropEdit.PerformLayout();
			this.ExportContainerTypeGuidFindBox.ResumeLayout(true);
			this.ExportContainerTypeGuidFindBox.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.DetailTabControl.ResumeLayout(false);
			this.DetailTabControl.PerformLayout();
			this.DeliveryModeDropEdit.ResumeLayout(true);
			this.DeliveryModeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		new void ImportTabPage_InitializeTab(object sender, System.EventArgs e)
		{
            // 
            // ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
            // 
            this.ImportTabPage.SuspendLayout();
            this.ImportDeliverEmptyToAddressControl.SuspendLayout();
            // 
            // ImportDeliverEmptyToAddressControl
            // 
            this.ImportDeliverEmptyToAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 17, true);
            this.ImportTabPage.PerformLayout();
            this.ImportDeliverEmptyToAddressControl.ResumeLayout(true);
            this.ImportDeliverEmptyToAddressControl.PerformLayout();
            this.ImportTabPage.ResumeLayout(true);

        }

        new void ExportTabPage_InitializeTab(object sender, System.EventArgs e)
		{
            // 
            // ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
            // 
            this.ExportTabPage.SuspendLayout();
            this.ExportEmptyReqByDateEdit.SuspendLayout();
            this.ExportPickupEmptyFromAddressControl.SuspendLayout();
            this.RelatedContainerLoadListFindBox.SuspendLayout();
            // 
            // ExportPickupEmptyFromAddressControl
            // 
            this.ExportPickupEmptyFromAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 17, true);
            // 
            // RelatedContainerLoadListFindBox
            // 
            this.RelatedContainerLoadListFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 17, true);
            this.ExportTabPage.PerformLayout();
            this.ExportEmptyReqByDateEdit.ResumeLayout(true);
            this.ExportEmptyReqByDateEdit.PerformLayout();
            this.ExportPickupEmptyFromAddressControl.ResumeLayout(true);
            this.ExportPickupEmptyFromAddressControl.PerformLayout();
            this.RelatedContainerLoadListFindBox.ResumeLayout(true);
            this.RelatedContainerLoadListFindBox.PerformLayout();
            this.ExportTabPage.ResumeLayout(true);

        }

        void OutturnTabPage_InitializeTab(object sender, System.EventArgs e)
		{
            // 
            // ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
            // 
            this.OutturnTabPage.SuspendLayout();
            this.OutturnTabPage.PerformLayout();
            this.OutturnTabPage.ResumeLayout(true);

        }

        void VGMTabPage_InitializeTab(object sender, System.EventArgs e)
		{
            // 
            // ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
            // 
            this.VGMTabPage.SuspendLayout();
            this.VGMTabPage.PerformLayout();
            this.VGMTabPage.ResumeLayout(true);

        }

        void FreightRatesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
            // 
            // ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
            // 
            this.FreightRatesTabPage.SuspendLayout();
            this.FreightRatesTabPage.PerformLayout();
            this.FreightRatesTabPage.ResumeLayout(true);

        }

        #endregion

        protected ZTextBox edSecondSealNum;
		protected ZCheckBox IsUnloadedCheckBox;
		protected ZCheckBox IsControlCheckBox;
	}
}
