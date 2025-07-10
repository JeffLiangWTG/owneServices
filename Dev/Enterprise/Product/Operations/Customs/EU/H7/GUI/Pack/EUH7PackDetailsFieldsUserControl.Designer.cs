using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7PackDetailsFieldsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.GoodsDescriptionTextBox = new ZTextBox();
			this.PackUQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PackQtyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MarksAndNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WeightCalcDropEdit = new ZArchitecture.GUI.ZCalcDropEdit();
			this.VolumeCalcDropEdit = new ZArchitecture.GUI.ZCalcDropEdit();
			this.PackUQDropEdit.SuspendLayout();
			this.WeightCalcDropEdit.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.H7.Business.AsycudaPack);
			//
			// GoodDescriptionTextBox;
			//
			this.GoodsDescriptionTextBox.AllowDrop = false;
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "APA_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(null)).APA_GoodsDescription)));
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 225, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 18, true);
			this.GoodsDescriptionTextBox.TabIndex = 1;
			// 
			// PackQtyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PackQtyCalcEdit, "APA_PackQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(null)).APA_PackQty)));
			this.PackQtyCalcEdit.DecimalPlaces = 2;
			this.PackQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 42, true);
			this.PackQtyCalcEdit.Name = "PackQtyCalcEdit";
			this.PackQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PackQtyCalcEdit.TabIndex = 2;
			this.PackQtyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PackQtyCalcEdit.MaxLength = 8;
			// 
			// PackUQDropEdit
			// 
			this.PackUQDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackUQDropEdit, "APA_PackUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(null)).APA_PackUQ)));
			this.PackUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 68, true);
			this.PackUQDropEdit.Name = "PackUQDropEdit";
			this.PackUQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.PackUQDropEdit.TabIndex = 3;
			// 
			// MarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.MarksAndNumbersTextBox, "APA_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(null)).APA_MarksAndNumbers)));
			this.MarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 94, true);
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 20, true);
			this.MarksAndNumbersTextBox.TabIndex = 4;
			//
			// WeightCalcDropEdit
			//
			this.WeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
			this.WeightCalcDropEdit.BindToAmount = "APA_Weight";
			this.WeightCalcDropEdit.BindToUnit = "APA_WeightUQ";
			this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 105, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.WeightCalcDropEdit.TabIndex = 5;
			this.WeightCalcDropEdit.UnitPreBoundMaxLength = 4;
			this.WeightCalcDropEdit.Decimals = 3;
			//
			// VolumeCalcDropEdit
			//
			this.VolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			this.VolumeCalcDropEdit.BindToAmount = "APA_Volume";
			this.VolumeCalcDropEdit.BindToUnit = "APA_VolumeUQ";
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 105, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 6;
			this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			this.VolumeCalcDropEdit.Decimals = 3;
			//
			// EUH7PackDetailsFieldsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.PackQtyCalcEdit);
			this.Controls.Add(this.PackUQDropEdit);
			this.Controls.Add(this.MarksAndNumbersTextBox);
			this.Controls.Add(this.WeightCalcDropEdit);
			this.Controls.Add(this.VolumeCalcDropEdit);
			this.Name = "EUH7PackDetailsFieldsUserControl";

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackUQDropEdit.ResumeLayout(true);
			this.PackUQDropEdit.PerformLayout();
			this.WeightCalcDropEdit.ResumeLayout(true);
			this.WeightCalcDropEdit.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZTextBox GoodsDescriptionTextBox;
		internal ZDropEdit PackUQDropEdit;
		internal ZCalcEdit PackQtyCalcEdit;
		internal ZTextBox MarksAndNumbersTextBox;
		internal ZCalcDropEdit WeightCalcDropEdit;
		internal ZCalcDropEdit VolumeCalcDropEdit;
	}
}
