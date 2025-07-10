namespace Enterprise.Customs.IL.Manifest.GUI
{
	partial class AsycudaPackedItemDetailsControl
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
			this.SeqCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GrossWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PackStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UQDropEdit.SuspendLayout();
			this.PackStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Manifest.Business.AsycudaBill);
			// 
			// SeqCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SeqCalcEdit, "PackedItems.API_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_LineNo)));
			this.SeqCalcEdit.DecimalPlaces = 2;
			this.SeqCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 100, true);
			this.SeqCalcEdit.Name = "SeqCalcEdit";
			this.SeqCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 18, true);
			this.SeqCalcEdit.TabIndex = 1;
			this.SeqCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SeqCalcEdit.TrackDisposedAccess = true;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "PackedItems.API_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_GoodsDescription)));
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 140, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 18, true);
			this.GoodsDescriptionTextBox.TabIndex = 2;
			// 
			// GrossWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit, "PackedItems.API_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_GrossWeight)));
			this.GrossWeightCalcEdit.DecimalPlaces = 2;
			this.GrossWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 182, true);
			this.GrossWeightCalcEdit.Name = "GrossWeightCalcEdit";
			this.GrossWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 18, true);
			this.GrossWeightCalcEdit.TabIndex = 3;
			this.GrossWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.GrossWeightCalcEdit.TrackDisposedAccess = true;
			// 
			// UQDropEdit
			// 
			this.UQDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UQDropEdit, "PackedItems.API_GrossWeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_GrossWeightUQ)));
			this.UQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 220, true);
			this.UQDropEdit.Name = "UQDropEdit";
			this.UQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.UQDropEdit.TabIndex = 4;
			// 
			// PackStatusDropEdit
			// 
			this.PackStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackStatusDropEdit, "PackedItems.API_PackStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_PackStatus)));
			this.PackStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 262, true);
			this.PackStatusDropEdit.Name = "PackStatusDropEdit";
			this.PackStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.PackStatusDropEdit.TabIndex = 5;
			// 
			// AsycudaPackedItemDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PackStatusDropEdit);
			this.Controls.Add(this.UQDropEdit);
			this.Controls.Add(this.GrossWeightCalcEdit);
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.SeqCalcEdit);
			this.Name = "AsycudaPackedItemDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 381, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UQDropEdit.ResumeLayout(true);
			this.UQDropEdit.PerformLayout();
			this.PackStatusDropEdit.ResumeLayout(true);
			this.PackStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZCalcEdit SeqCalcEdit;
		internal ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		internal ZArchitecture.ZCalcEdit GrossWeightCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit UQDropEdit;
		internal ZArchitecture.GUI.ZDropEdit PackStatusDropEdit;
	}
}
