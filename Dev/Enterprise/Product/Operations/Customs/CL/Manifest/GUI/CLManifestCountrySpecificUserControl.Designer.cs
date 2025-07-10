namespace Enterprise.Customs.CL.Manifest.GUI
{
	partial class CLManifestCountrySpecificUserControl
	{
		private void InitializeComponent()
		{
			this.IsTrampCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TranshipmentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IsTrampCheckBox.SuspendLayout();
			this.TranshipmentTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CL.Manifest.Business.AsycudaManifestHeader);
			// 
			// TransshipmentCheckBox
			//
			this.BindingSource.SetBindingMember(this.IsTrampCheckBox, "AMA_IsTramp");
			this.IsTrampCheckBox.AutoSize = true;
			this.IsTrampCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Left;
			this.IsTrampCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsTrampCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 68, true);
			this.IsTrampCheckBox.Name = "IsTrampCheckBox";
			this.IsTrampCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsTrampCheckBox.TabIndex = 8;
			this.IsTrampCheckBox.UseVisualStyleBackColor = true;
			// 
			// TranshipmentTypeDropEdit
			// 
			this.TranshipmentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TranshipmentTypeDropEdit, "AMA_TransshipmentType");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CL.Manifest.Business.AsycudaManifestHeader)(null)).AMA_TransshipmentType)));
			this.TranshipmentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 68, true);
			this.TranshipmentTypeDropEdit.Name = "TranshipmentTypeDropEdit";
			this.TranshipmentTypeDropEdit.PreBoundMaxLength = 3;
			this.TranshipmentTypeDropEdit.ShouldResizeByMaxLength = true;
			this.TranshipmentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.TranshipmentTypeDropEdit.TabIndex = 6;
			// 
			// CLBillCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.IsTrampCheckBox);
			this.Controls.Add(this.TranshipmentTypeDropEdit);
			this.Name = "CLManifestCountrySpecificUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IsTrampCheckBox.ResumeLayout(true);
			this.IsTrampCheckBox.PerformLayout();
			this.TranshipmentTypeDropEdit.ResumeLayout(true);
			this.TranshipmentTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZArchitecture.GUI.ZCheckBox IsTrampCheckBox;
		internal ZArchitecture.GUI.ZDropEdit TranshipmentTypeDropEdit;
	}
}
