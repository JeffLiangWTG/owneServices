namespace Enterprise.Customs.BE.GUI
{
	partial class ContainerDetailUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SealPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalSealPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WeightsGroupBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.DetailTabControl.SuspendLayout();
			this.DeliveryModeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SealPartyDropEdit.SuspendLayout();
			this.AdditionalSealPartyDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// edContainerNum
			//
			this.edContainerNum.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("EEF5F640-3114-4A3C-9FFB-9A0C7F239FBE", "Container", "[UCC 7/10] Container");
			//
			// ExportContainerTypeGuidFindBox
			//
			this.ExportContainerTypeGuidFindBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("309AE149-0F11-4FA5-AF28-A9412600763A", "Type", "[UCC 7/11] Type");
			//
			// edSealNum
			this.edSealNum.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 46, true);
			this.edSealNum.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("4405F50E-9B89-4D6F-B0E6-7245AFD886F0", "Seal", "[7/18] Seal Number.");
			this.edSealNum.TabIndex = 4;
			// 
			// WeightsGroupBox
			// 
			this.WeightsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 97, true);
			this.WeightsGroupBox.TabIndex = 8;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 235, true);
			this.DetailsGroupBox.TabIndex = 9;
			// 
			// DetailTabControl
			// 
			this.DetailTabControl.TabIndex = 10;
			// 
			// DeliveryModeDropEdit
			// 
			this.DeliveryModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 25, true);
			this.DeliveryModeDropEdit.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.Declaration.CusContainer);
			// 
			// edSecondSealNum
			// 
			this.BindingSource.SetBindingMember(this.edSecondSealNum, "CO_SecondSeal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_SecondSeal)));
			this.edSecondSealNum.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("8d927ab0-bf89-4c5d-a99b-e6149baaf272", "2nd Seal", "[UCC 7/18] 2nd Seal");
			this.edSecondSealNum.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 67, true);
			this.edSecondSealNum.Name = "edSecondSealNum";
			this.edSecondSealNum.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.edSecondSealNum.TabIndex = 6;
			// 
			// SealPartyDropEdit
			// 
			this.SealPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SealPartyDropEdit, "SealPartyForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseCusContainer)(null)).JobContainer.JC_SealParty)));
			this.SealPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 46, true);
			this.SealPartyDropEdit.Name = "SealPartyDropEdit";
			this.SealPartyDropEdit.ShouldResizeByMaxLength = true;
			this.SealPartyDropEdit.ShowDescriptionBox = false;
			this.SealPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.SealPartyDropEdit.TabIndex = 5;
			// 
			// AdditionalSealPartyDropEdit
			// 
			this.AdditionalSealPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalSealPartyDropEdit, "AdditionalSealPartyForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseCusContainer)(null)).JobContainer.JC_AdditionalSealParty)));
			this.AdditionalSealPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 67, true);
			this.AdditionalSealPartyDropEdit.Name = "AdditionalSealPartyDropEdit";
			this.AdditionalSealPartyDropEdit.ShouldResizeByMaxLength = true;
			this.AdditionalSealPartyDropEdit.ShowDescriptionBox = false;
			this.AdditionalSealPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.AdditionalSealPartyDropEdit.TabIndex = 7;
			// 
			// ContainerDetailUserControl
			// 
			this.Controls.Add(this.AdditionalSealPartyDropEdit);
			this.Controls.Add(this.SealPartyDropEdit);
			this.Controls.Add(this.edSecondSealNum);
			this.Name = "ContainerDetailUserControl";
			this.WeightsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailTabControl.ResumeLayout(false);
			this.DeliveryModeDropEdit.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SealPartyDropEdit.ResumeLayout(false);
			this.AdditionalSealPartyDropEdit.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.GUI.ZDropEdit SealPartyDropEdit;
		ZArchitecture.GUI.ZDropEdit AdditionalSealPartyDropEdit;
	}
}
