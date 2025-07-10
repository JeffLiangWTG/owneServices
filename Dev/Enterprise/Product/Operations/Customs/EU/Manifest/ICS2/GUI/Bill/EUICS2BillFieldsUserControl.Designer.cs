using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class EUICS2BillFieldsUserControl
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
			this.TransportDocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FreightValueAndCurrencyCalcFindBox = new ZArchitecture.GUI.ZCalcFindBox();
			this.ReceptacleIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportDocumentTypeDropEdit.SuspendLayout();
			this.FreightValueAndCurrencyCalcFindBox.SuspendLayout();
			this.ReceptacleIdTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill);
			// 
			// TransportDocumentTypeDropEdit
			// 
			this.TransportDocumentTypeDropEdit.AllowDrop = true;
			this.TransportDocumentTypeDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.TransportDocumentTypeDropEdit, "TransportDocumentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).TransportDocumentType)));
			this.TransportDocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportDocumentTypeDropEdit.Name = "TransportDocumentTypeDropEdit";
			this.TransportDocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 11, true);
			this.TransportDocumentTypeDropEdit.TabIndex = 1;
			// 
			// FreightValueAndCurrencyCalcFindBox
			// 
			this.FreightValueAndCurrencyCalcFindBox.AllowDrop = true;
			this.FreightValueAndCurrencyCalcFindBox.BindToAmount = "ABL_FreightValue";
			this.FreightValueAndCurrencyCalcFindBox.BindToUnit = "ABL_RX_NKFreightValueCurrency";
			this.FreightValueAndCurrencyCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.FreightValueAndCurrencyCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FreightValueAndCurrencyCalcFindBox.Name = "FreightValueAndCurrencyCalcFindBox";
			this.FreightValueAndCurrencyCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.FreightValueAndCurrencyCalcFindBox.TabIndex = 2;
			// 
			// ReceptacleIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReceptacleIdTextBox, "ReceptacleId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ReceptacleId)));
			this.ReceptacleIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReceptacleIdTextBox.Name = "ReceptacleIdTextBox";
			this.ReceptacleIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.ReceptacleIdTextBox.TabIndex = 3;
			//
			// EUICS2BillFieldsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TransportDocumentTypeDropEdit);
			this.Controls.Add(this.FreightValueAndCurrencyCalcFindBox);
			this.Controls.Add(this.ReceptacleIdTextBox);
			this.Name = "EUICS2BillFieldsUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportDocumentTypeDropEdit.ResumeLayout(true);
			this.TransportDocumentTypeDropEdit.PerformLayout();
			this.FreightValueAndCurrencyCalcFindBox.ResumeLayout(true);
			this.FreightValueAndCurrencyCalcFindBox.PerformLayout();
			this.ReceptacleIdTextBox.ResumeLayout(true);
			this.ReceptacleIdTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEdit TransportDocumentTypeDropEdit;
		internal ZArchitecture.GUI.ZCalcFindBox FreightValueAndCurrencyCalcFindBox;
		internal Enterprise.ZArchitecture.ZTextBox ReceptacleIdTextBox;
	}
}
