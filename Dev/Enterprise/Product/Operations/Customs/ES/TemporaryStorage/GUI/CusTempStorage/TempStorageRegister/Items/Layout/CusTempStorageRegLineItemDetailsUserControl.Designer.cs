namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class CusTempStorageRegLineItemDetailsUserControl
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
			this.GoodsItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TariffTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CusC4NumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader);
			// 
			// GoodsItemNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GoodsItemNumberCalcEdit, "CusTempStorageRegLines.RegLineItemPivots.RegLineItem.SRI_GoodsItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).RegLineItemPivots)).SyncRoot)).RegLineItem.SRI_GoodsItemNumber)));
			this.GoodsItemNumberCalcEdit.DecimalPlaces = 0;
			this.GoodsItemNumberCalcEdit.Decimals = 0;
			this.GoodsItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 12, true);
			this.GoodsItemNumberCalcEdit.Name = "GoodsItemNumberCalcEdit";
			this.GoodsItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.GoodsItemNumberCalcEdit.TabIndex = 1;
			this.GoodsItemNumberCalcEdit.Text = "0";
			this.GoodsItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.GoodsItemNumberCalcEdit.TrackDisposedAccess = true;
			// 
			// TariffTextBox
			// 
			this.BindingSource.SetBindingMember(this.TariffTextBox, "CusTempStorageRegLines.RegLineItemPivots.RegLineItem.FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).RegLineItemPivots)).SyncRoot)).RegLineItem.FormattedTariff)));
			this.TariffTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 38, true);
			this.TariffTextBox.Name = "TariffTextBox";
			this.TariffTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.TariffTextBox.TabIndex = 2;
			// 
			// CusC4NumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CusC4NumberTextBox, "CusTempStorageRegLines.RegLineItemPivots.RegLineItem.SRI_CusC4Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).RegLineItemPivots)).SyncRoot)).RegLineItem.SRI_CusC4Number)));
			this.CusC4NumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 64, true);
			this.CusC4NumberTextBox.Name = "CusC4NumberTextBox";
			this.CusC4NumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CusC4NumberTextBox.TabIndex = 3;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "CusTempStorageRegLines.RegLineItemPivots.RegLineItem.SRI_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).RegLineItemPivots)).SyncRoot)).RegLineItem.SRI_GoodsDescription)));
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 90, true);
			this.GoodsDescriptionTextBox.Multiline = true;
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 45, true);
			this.GoodsDescriptionTextBox.TabIndex = 4;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).RegLineItemPivots)).SyncRoot)).SRV_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).RegLineItemPivots)).SyncRoot)).RegLine.SRL_GrossWeightUQ)));
			this.GrossWeightCalcDropEdit.BindToAmount = "CusTempStorageRegLines.RegLineItemPivots.SRV_GrossWeight";
			this.GrossWeightCalcDropEdit.BindToUnit = "CusTempStorageRegLines.RegLineItemPivots.RegLine.SRL_GrossWeightUQ";
			this.GrossWeightCalcDropEdit.Decimals = 6;
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 141, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.GrossWeightCalcDropEdit.TabIndex = 5;
			// 
			// CusTempStorageRegLineItemDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.CusC4NumberTextBox);
			this.Controls.Add(this.TariffTextBox);
			this.Controls.Add(this.GoodsItemNumberCalcEdit);
			this.Name = "CusTempStorageRegLineItemDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 229, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZCalcEdit GoodsItemNumberCalcEdit;
		internal Enterprise.ZArchitecture.ZTextBox TariffTextBox;
		internal ZArchitecture.ZTextBox CusC4NumberTextBox;
		internal ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		internal ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
	}
}
