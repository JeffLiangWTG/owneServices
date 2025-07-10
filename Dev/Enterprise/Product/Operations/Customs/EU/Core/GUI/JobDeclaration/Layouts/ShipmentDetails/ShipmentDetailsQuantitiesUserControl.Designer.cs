namespace Enterprise.Customs.EU.GUI
{
	partial class ShipmentDetailsQuantitiesUserControl
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
            this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.TotalNoOfPacksCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.WeightCalcDropEdit.SuspendLayout();
            this.VolumeCalcDropEdit.SuspendLayout();
            this.TotalNoOfPacksCalcDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // WeightCalcDropEdit
            // 
            this.WeightCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_TotalWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_TotalWeightUnit)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Lookups.WeightUnitList)));
            this.WeightCalcDropEdit.BindToAmount = "JE_TotalWeight";
            this.WeightCalcDropEdit.BindToList = "Lookups.WeightUnitList";
            this.WeightCalcDropEdit.BindToUnit = "JE_TotalWeightUnit";
            this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 0, true);
            this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
            this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
            this.WeightCalcDropEdit.TabIndex = 2;
            this.WeightCalcDropEdit.UnitPreBoundMaxLength = 2;
            // 
            // VolumeCalcDropEdit
            // 
            this.VolumeCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_TotalVolume)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_TotalVolumeUnit)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Lookups.VolumeUnitList)));
            this.VolumeCalcDropEdit.BindToAmount = "JE_TotalVolume";
            this.VolumeCalcDropEdit.BindToList = "Lookups.VolumeUnitList";
            this.VolumeCalcDropEdit.BindToUnit = "JE_TotalVolumeUnit";
            this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 0, true);
            this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
            this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
            this.VolumeCalcDropEdit.TabIndex = 3;
            this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
            // 
            // TotalNoOfPacksCalcDropEdit
            // 
            this.TotalNoOfPacksCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TotalNoOfPacksCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_TotalNoOfPacks)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_TotalNoOfPacksPackType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Lookups.JE_TotalNoOfPacksPackType_List)));
            this.TotalNoOfPacksCalcDropEdit.BindToAmount = "JE_TotalNoOfPacks";
            this.TotalNoOfPacksCalcDropEdit.BindToList = "Lookups.JE_TotalNoOfPacksPackType_List";
            this.TotalNoOfPacksCalcDropEdit.BindToUnit = "JE_TotalNoOfPacksPackType";
            this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.TotalNoOfPacksCalcDropEdit.Name = "TotalNoOfPacksCalcDropEdit";
            this.TotalNoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
            this.TotalNoOfPacksCalcDropEdit.TabIndex = 1;
            this.TotalNoOfPacksCalcDropEdit.UnitPreBoundMaxLength = 3;
            // 
            // ShipmentDetailsQuantitiesUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.TotalNoOfPacksCalcDropEdit);
            this.Controls.Add(this.VolumeCalcDropEdit);
            this.Controls.Add(this.WeightCalcDropEdit);
            this.Name = "ShipmentDetailsQuantitiesUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.WeightCalcDropEdit.ResumeLayout(true);
            this.WeightCalcDropEdit.PerformLayout();
            this.VolumeCalcDropEdit.ResumeLayout(true);
            this.VolumeCalcDropEdit.PerformLayout();
            this.TotalNoOfPacksCalcDropEdit.ResumeLayout(true);
            this.TotalNoOfPacksCalcDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit VolumeCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit TotalNoOfPacksCalcDropEdit;
	}
}
