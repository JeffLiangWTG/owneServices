namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class UCC6TemporaryStorageGrossWeightWithUnitUserControl
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
			this.grossWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.grossWeightUnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.grossWeightUnitDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// grossWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.grossWeightCalcEdit, "Bills.ABL_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_GrossWeight)));
			this.grossWeightCalcEdit.DecimalPlaces = 2;
			this.grossWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.grossWeightCalcEdit.Name = "grossWeightCalcEdit";
			this.grossWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 20, true);
			this.grossWeightCalcEdit.TabIndex = 0;
			this.grossWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.grossWeightCalcEdit.TrackDisposedAccess = true;
			// 
			// grossWeightUnitDropEdit
			// 
			this.grossWeightUnitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.grossWeightUnitDropEdit, "Bills.ABL_GrossWeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_GrossWeightUQ)));
			this.grossWeightUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 0, true);
			this.grossWeightUnitDropEdit.Name = "grossWeightUnitDropEdit";
			this.grossWeightUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			this.grossWeightUnitDropEdit.TabIndex = 1;
			// 
			// UCC6TemporaryStorageGrossWeightWithUnitUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.grossWeightUnitDropEdit);
			this.Controls.Add(this.grossWeightCalcEdit);
			this.Name = "UCC6TemporaryStorageGrossWeightWithUnitUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.grossWeightUnitDropEdit.ResumeLayout(true);
			this.grossWeightUnitDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit grossWeightCalcEdit;
		private ZArchitecture.GUI.ZDropEdit grossWeightUnitDropEdit;
	}
}
