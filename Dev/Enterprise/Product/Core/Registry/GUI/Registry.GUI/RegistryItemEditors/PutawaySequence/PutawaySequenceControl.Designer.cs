using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class PutawaySequenceControl
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
		void InitializeComponent()
		{
			this.PutawayAlgorithmSequenceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProductAreaCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ClientAreaCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LocationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PickFaceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LocationSortOrderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LevelCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RowCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ColumnCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PutawayAlgorithmSequenceGroupBox.SuspendLayout();
			this.LocationSortOrderGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Warehouse.PutawaySequence);
			// 
			// PutawayAlgorithmSequenceGroupBox
			// 
			this.PutawayAlgorithmSequenceGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PutawaySequenceControl|7bf0b21e-459f-4b2d-b25f-8c159c527d08", "Algorithm Sequence");
			this.PutawayAlgorithmSequenceGroupBox.Controls.Add(this.ProductAreaCalcEdit);
			this.PutawayAlgorithmSequenceGroupBox.Controls.Add(this.ClientAreaCalcEdit);
			this.PutawayAlgorithmSequenceGroupBox.Controls.Add(this.LocationCalcEdit);
			this.PutawayAlgorithmSequenceGroupBox.Controls.Add(this.PickFaceCalcEdit);
			this.PutawayAlgorithmSequenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.PutawayAlgorithmSequenceGroupBox.Name = "PutawayAlgorithmSequenceGroupBox";
			this.PutawayAlgorithmSequenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 125, true);
			this.PutawayAlgorithmSequenceGroupBox.TabIndex = 0;
			this.PutawayAlgorithmSequenceGroupBox.TabStop = false;
			// 
			// ProductAreaCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ProductAreaCalcEdit, "ProductArea");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PutawaySequence)(null)).ProductArea)));
			this.ProductAreaCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PutawaySequenceControl|34d11249-e3b9-4265-9335-83c20aa94e8a", "Product Area");
			this.ProductAreaCalcEdit.Decimals = 0;
			this.ProductAreaCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 96, true);
			this.ProductAreaCalcEdit.Name = "ProductAreaCalcEdit";
			this.ProductAreaCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.ProductAreaCalcEdit.TabIndex = 7;
			this.ProductAreaCalcEdit.Text = "0";
			this.ProductAreaCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ClientAreaCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ClientAreaCalcEdit, "ClientArea");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PutawaySequence)(null)).ClientArea)));
			this.ClientAreaCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PutawaySequenceControl|c088432d-fe82-4372-9097-e2e3dc32ee7a", "Client Area");
			this.ClientAreaCalcEdit.Decimals = 0;
			this.ClientAreaCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 70, true);
			this.ClientAreaCalcEdit.Name = "ClientAreaCalcEdit";
			this.ClientAreaCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.ClientAreaCalcEdit.TabIndex = 5;
			this.ClientAreaCalcEdit.Text = "0";
			this.ClientAreaCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LocationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LocationCalcEdit, "Location");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PutawaySequence)(null)).Location)));
			this.LocationCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PutawaySequenceControl|fc45e331-be7e-4662-b6b7-6b9d2a6f15a6", "Location Fallback");
			this.LocationCalcEdit.Decimals = 0;
			this.LocationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 18, true);
			this.LocationCalcEdit.Name = "LocationCalcEdit";
			this.LocationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.LocationCalcEdit.TabIndex = 1;
			this.LocationCalcEdit.Text = "0";
			this.LocationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PickFaceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PickFaceCalcEdit, "PickFace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PutawaySequence)(null)).PickFace)));
			this.PickFaceCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PutawaySequenceControl|0095f184-4c02-499b-8d8b-b5be732c6dc9", "Pick Faces");
			this.PickFaceCalcEdit.Decimals = 0;
			this.PickFaceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 44, true);
			this.PickFaceCalcEdit.Name = "PickFaceCalcEdit";
			this.PickFaceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.PickFaceCalcEdit.TabIndex = 3;
			this.PickFaceCalcEdit.Text = "0";
			this.PickFaceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LocationSortOrderGroupBox
			// 
			this.LocationSortOrderGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PutawaySequenceControl|1ea020b8-9bf2-484b-b978-6329d4cfde59", "Location Sort");
			this.LocationSortOrderGroupBox.Controls.Add(this.LevelCalcEdit);
			this.LocationSortOrderGroupBox.Controls.Add(this.RowCalcEdit);
			this.LocationSortOrderGroupBox.Controls.Add(this.ColumnCalcEdit);
			this.LocationSortOrderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 3, true);
			this.LocationSortOrderGroupBox.Name = "LocationSortOrderGroupBox";
			this.LocationSortOrderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 103, true);
			this.LocationSortOrderGroupBox.TabIndex = 1;
			this.LocationSortOrderGroupBox.TabStop = false;
			// 
			// LevelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LevelCalcEdit, "Level");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PutawaySequence)(null)).Level)));
			this.LevelCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PutawaySequenceControl|15d0b419-80f3-4b0f-a367-3a788b87d58a", "Level");
			this.LevelCalcEdit.Decimals = 0;
			this.LevelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 70, true);
			this.LevelCalcEdit.Name = "LevelCalcEdit";
			this.LevelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.LevelCalcEdit.TabIndex = 5;
			this.LevelCalcEdit.Text = "0";
			this.LevelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RowCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RowCalcEdit, "Row");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PutawaySequence)(null)).Row)));
			this.RowCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PutawaySequenceControl|37b92f09-f475-4a52-b162-35610a67063e", "Row");
			this.RowCalcEdit.Decimals = 0;
			this.RowCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 18, true);
			this.RowCalcEdit.Name = "RowCalcEdit";
			this.RowCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.RowCalcEdit.TabIndex = 1;
			this.RowCalcEdit.Text = "0";
			this.RowCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ColumnCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ColumnCalcEdit, "Column");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.PutawaySequence)(null)).Column)));
			this.ColumnCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PutawaySequenceControl|d81e45cf-314c-4ba8-b833-71802daec731", "Column");
			this.ColumnCalcEdit.Decimals = 0;
			this.ColumnCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 44, true);
			this.ColumnCalcEdit.Name = "ColumnCalcEdit";
			this.ColumnCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.ColumnCalcEdit.TabIndex = 3;
			this.ColumnCalcEdit.Text = "0";
			this.ColumnCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PutawaySequenceControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LocationSortOrderGroupBox);
			this.Controls.Add(this.PutawayAlgorithmSequenceGroupBox);
			this.Name = "PutawaySequenceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 139, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PutawayAlgorithmSequenceGroupBox.ResumeLayout(false);
			this.PutawayAlgorithmSequenceGroupBox.PerformLayout();
			this.LocationSortOrderGroupBox.ResumeLayout(false);
			this.LocationSortOrderGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox PutawayAlgorithmSequenceGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit ProductAreaCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit ClientAreaCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit PickFaceCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit LocationCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox LocationSortOrderGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit LevelCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit RowCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit ColumnCalcEdit;
	}
}
