namespace Enterprise.Customs.CO.Manifest.GUI
{
	partial class COManifestCountrySpecificUserControl
	{
		private void InitializeComponent()
		{
            this.TravelDocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CargoDispositionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeliveryModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MultimodalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PrecursorsCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.CarriersLiabilityCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.TravelDocumentTypeDropEdit.SuspendLayout();
            this.CargoDispositionDropEdit.SuspendLayout();
			this.DeliveryModeDropEdit.SuspendLayout();
			this.MultimodalCheckBox.SuspendLayout();
			this.PrecursorsCheckBox.SuspendLayout();
			this.CarriersLiabilityCheckBox.SuspendLayout();
			this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CO.Manifest.Business.AsycudaManifestHeader);
            // 
            // TravelDocumentTypeDropEdit
            // 
            this.TravelDocumentTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TravelDocumentTypeDropEdit, "TravelDocumentType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CO.Manifest.Business.AsycudaManifestHeader)(null)).TravelDocumentType)));
            this.TravelDocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 10, true);
            this.TravelDocumentTypeDropEdit.Name = "TravelDocumentTypeDropEdit";
            this.TravelDocumentTypeDropEdit.PreBoundMaxLength = 2;
            this.TravelDocumentTypeDropEdit.ShouldResizeByMaxLength = true;
            this.TravelDocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.TravelDocumentTypeDropEdit.TabIndex = 1;
			// 
			// CargoDispositionDropEdit
			// 
			this.CargoDispositionDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CargoDispositionDropEdit, "CargoDisposition");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CO.Manifest.Business.AsycudaManifestHeader)(null)).CargoDisposition)));
            this.CargoDispositionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 36, true);
            this.CargoDispositionDropEdit.Name = "CargoDispositionDropEdit";
            this.CargoDispositionDropEdit.PreBoundMaxLength = 2;
            this.CargoDispositionDropEdit.ShouldResizeByMaxLength = true;
            this.CargoDispositionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.CargoDispositionDropEdit.TabIndex = 2;
			// 
			// DeliveryModeDropEdit
			// 
			this.DeliveryModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryModeDropEdit, "DeliveryMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CO.Manifest.Business.AsycudaManifestHeader)(null)).DeliveryMode)));
			this.DeliveryModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 36, true);
			this.DeliveryModeDropEdit.Name = "DeliveryModeDropEdit";
			this.DeliveryModeDropEdit.ShouldResizeByMaxLength = true;
			this.DeliveryModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.DeliveryModeDropEdit.TabIndex = 3;
			// 
			// MultimodalCheckBox
			//
			this.BindingSource.SetBindingMember(this.MultimodalCheckBox, "Multimodal");
			this.MultimodalCheckBox.AutoSize = true;
			this.MultimodalCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Left;
			this.MultimodalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MultimodalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 68, true);
			this.MultimodalCheckBox.Name = "MultimodalCheckBox";
			this.MultimodalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.MultimodalCheckBox.TabIndex = 4;
			this.MultimodalCheckBox.UseVisualStyleBackColor = true;
			// 
			// CarriersLiabilityCheckBox
			//
			this.BindingSource.SetBindingMember(this.CarriersLiabilityCheckBox, "CarriersLiability");
			this.CarriersLiabilityCheckBox.AutoSize = true;
			this.CarriersLiabilityCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Left;
			this.CarriersLiabilityCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CarriersLiabilityCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 88, true);
			this.CarriersLiabilityCheckBox.Name = "CarriersLiabilityCheckBox";
			this.CarriersLiabilityCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.CarriersLiabilityCheckBox.TabIndex = 5;
			this.CarriersLiabilityCheckBox.UseVisualStyleBackColor = true;
			// 
			// COManifestCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			// 
			// PrecursorsCheckBox
			//
			this.BindingSource.SetBindingMember(this.PrecursorsCheckBox, "Precursors");
			this.PrecursorsCheckBox.AutoSize = true;
			this.PrecursorsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Left;
			this.PrecursorsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrecursorsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 108, true);
			this.PrecursorsCheckBox.Name = "PrecursorsCheckBox";
			this.PrecursorsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.PrecursorsCheckBox.TabIndex = 6;
			this.PrecursorsCheckBox.UseVisualStyleBackColor = true;
			// 
			// COManifestCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.TravelDocumentTypeDropEdit);
			this.Controls.Add(this.CargoDispositionDropEdit);
			this.Controls.Add(this.DeliveryModeDropEdit);
			this.Controls.Add(this.MultimodalCheckBox);
			this.Controls.Add(this.PrecursorsCheckBox);
			this.Controls.Add(this.CarriersLiabilityCheckBox);
			this.Name = "COManifestCountrySpecificUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 68, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.TravelDocumentTypeDropEdit.ResumeLayout(true);
            this.TravelDocumentTypeDropEdit.PerformLayout();
            this.CargoDispositionDropEdit.ResumeLayout(true);
            this.CargoDispositionDropEdit.PerformLayout();
			this.DeliveryModeDropEdit.ResumeLayout(true);
			this.DeliveryModeDropEdit.PerformLayout();
			this.MultimodalCheckBox.ResumeLayout();
			this.MultimodalCheckBox.PerformLayout();
			this.PrecursorsCheckBox.ResumeLayout(false);
			this.PrecursorsCheckBox.PerformLayout();
			this.CarriersLiabilityCheckBox.ResumeLayout();
			this.CarriersLiabilityCheckBox.PerformLayout();
			this.ResumeLayout(false);
            this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZDropEdit TravelDocumentTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CargoDispositionDropEdit;
		internal ZArchitecture.GUI.ZDropEdit DeliveryModeDropEdit;
		internal ZArchitecture.GUI.ZCheckBox MultimodalCheckBox;
		internal ZArchitecture.GUI.ZCheckBox PrecursorsCheckBox;
		internal ZArchitecture.GUI.ZCheckBox CarriersLiabilityCheckBox;
	}
}
