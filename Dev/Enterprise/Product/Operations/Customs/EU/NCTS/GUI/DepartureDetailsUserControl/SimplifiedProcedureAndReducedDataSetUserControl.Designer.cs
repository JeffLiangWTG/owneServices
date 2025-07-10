using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class SimplifiedProcedureAndReducedDataSetUserControl
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
			this.SimplifiedNctsProcedureCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReducedDatasetIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// SimplifiedNctsProcedureCheckBox
			// 
			this.SimplifiedNctsProcedureCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SimplifiedNctsProcedureCheckBox, "MovementHeader.IsSimplifiedNctsProcedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.IsSimplifiedNctsProcedure)));
			this.SimplifiedNctsProcedureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 4, true);
			this.SimplifiedNctsProcedureCheckBox.Name = "SimplifiedNctsProcedureCheckBox";
			this.SimplifiedNctsProcedureCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			this.SimplifiedNctsProcedureCheckBox.TabIndex = 0;
			this.SimplifiedNctsProcedureCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReducedDatasetIndicatorCheckBox
			// 
			this.ReducedDatasetIndicatorCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ReducedDatasetIndicatorCheckBox, "MovementHeader.BM_ReducedDatasetIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_ReducedDatasetIndicator)));
			this.ReducedDatasetIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 4, true);
			this.ReducedDatasetIndicatorCheckBox.Name = "ReducedDatasetIndicatorCheckBox";
			this.ReducedDatasetIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 17, true);
			this.ReducedDatasetIndicatorCheckBox.TabIndex = 1;
			this.ReducedDatasetIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// SimplifiedProcedureAndReducedDataSetUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReducedDatasetIndicatorCheckBox);
			this.Controls.Add(this.SimplifiedNctsProcedureCheckBox);
			this.Name = "SimplifiedProcedureAndReducedDataSetUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 33, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox SimplifiedNctsProcedureCheckBox;
		internal ZArchitecture.GUI.ZCheckBox ReducedDatasetIndicatorCheckBox;
	}
}
