namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class AdditionalArrivalNotificationDetailsUserControl
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
			this.SimplifiedProcedureCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AutomaticCompletionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AutomaticTranshipmentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TIRArrivalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TIRPartialUnloadingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TIRCarnetPageIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
			// 
			// SimplifiedProcedureCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SimplifiedProcedureCheckBox, "ArrivalMovementHeader.IsSimplifiedNctsProcedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.IsSimplifiedNctsProcedure)));
			this.SimplifiedProcedureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 23, true);
			this.SimplifiedProcedureCheckBox.Name = "SimplifiedProcedureCheckBox";
			this.SimplifiedProcedureCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 24, true);
			this.SimplifiedProcedureCheckBox.TabIndex = 0;
			// 
			// AutomaticCompletionCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AutomaticCompletionCheckBox, "ESNctsHeader.CEN_AutomaticCompletion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ESNctsHeader.CEN_AutomaticCompletion)));
			this.AutomaticCompletionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 23, true);
			this.AutomaticCompletionCheckBox.Name = "AutomaticCompletionCheckBox";
			this.AutomaticCompletionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 24, true);
			this.AutomaticCompletionCheckBox.TabIndex = 1;
			// 
			// AutomaticTranshipmentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AutomaticTranshipmentCheckBox, "ESNctsHeader.CEN_AutomaticTranshipment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ESNctsHeader.CEN_AutomaticTranshipment)));
			this.AutomaticTranshipmentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 49, true);
			this.AutomaticTranshipmentCheckBox.Name = "AutomaticTranshipmentCheckBox";
			this.AutomaticTranshipmentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 24, true);
			this.AutomaticTranshipmentCheckBox.TabIndex = 2;
			// 
			// TIRArrivalCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TIRArrivalCheckBox, "ESNctsHeader.CEN_TIRArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ESNctsHeader.CEN_TIRArrival)));
			this.TIRArrivalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 49, true);
			this.TIRArrivalCheckBox.Name = "TIRArrivalCheckBox";
			this.TIRArrivalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 24, true);
			this.TIRArrivalCheckBox.TabIndex = 3;
			// 
			// TIRPartialUnloadingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TIRPartialUnloadingCheckBox, "ESNctsHeader.CEN_TIRPartialUnloading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ESNctsHeader.CEN_TIRPartialUnloading)));
			this.TIRPartialUnloadingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 75, true);
			this.TIRPartialUnloadingCheckBox.Name = "TIRPartialUnloadingCheckBox";
			this.TIRPartialUnloadingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 24, true);
			this.TIRPartialUnloadingCheckBox.TabIndex = 4;
			// 
			// TIRCarnetPageIntEdit
			// 
			this.BindingSource.SetBindingMember(this.TIRCarnetPageIntEdit, "ESNctsHeader.CEN_TIRCarnetPage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ESNctsHeader.CEN_TIRCarnetPage)));
			this.TIRCarnetPageIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 77, true);
			this.TIRCarnetPageIntEdit.Name = "TIRCarnetPageIntEdit";
			this.TIRCarnetPageIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.TIRCarnetPageIntEdit.TabIndex = 5;
			// 
			// AdditionalArrivalNotificationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SimplifiedProcedureCheckBox);
			this.Controls.Add(this.AutomaticCompletionCheckBox);
			this.Controls.Add(this.AutomaticTranshipmentCheckBox);
			this.Controls.Add(this.TIRArrivalCheckBox);
			this.Controls.Add(this.TIRPartialUnloadingCheckBox);
			this.Controls.Add(this.TIRCarnetPageIntEdit);
			this.Name = "AdditionalArrivalNotificationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 112, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox SimplifiedProcedureCheckBox;
		internal ZArchitecture.GUI.ZCheckBox AutomaticCompletionCheckBox;
		internal ZArchitecture.GUI.ZCheckBox AutomaticTranshipmentCheckBox;
		internal ZArchitecture.GUI.ZCheckBox TIRArrivalCheckBox;
		internal ZArchitecture.GUI.ZCheckBox TIRPartialUnloadingCheckBox;
		internal ZArchitecture.GUI.ZIntEdit TIRCarnetPageIntEdit;
	}
}
