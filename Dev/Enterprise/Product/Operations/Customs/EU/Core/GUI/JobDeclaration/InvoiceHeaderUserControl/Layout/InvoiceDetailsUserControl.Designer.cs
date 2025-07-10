namespace Enterprise.Customs.EU.GUI
{
	partial class InvoiceDetailsUserControl
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
			this.AgreedPlaceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransportChargesMethodOfPaymentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IncoTermsAgreedPlaceLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AgreedPlaceCodeFindBox.SuspendLayout();
			this.TransportChargesMethodOfPaymentDropEdit.SuspendLayout();
			this.IncoTermsAgreedPlaceLongTextControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader);
			// 
			// AgreedPlaceCodeFindBox
			// 
			this.AgreedPlaceCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgreedPlaceCodeFindBox, "ZG_AgreedPlaceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(null)).ZG_AgreedPlaceCode)));
			this.AgreedPlaceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 30, true);
			this.AgreedPlaceCodeFindBox.Name = "AgreedPlaceCodeFindBox";
			this.AgreedPlaceCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AgreedPlaceCodeFindBox.ParentType = null;
			this.AgreedPlaceCodeFindBox.PreBoundMaxLength = 5;
			this.AgreedPlaceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.AgreedPlaceCodeFindBox.TabIndex = 0;
			// 
			// TransportChargesMethodOfPaymentDropEdit
			// 
			this.TransportChargesMethodOfPaymentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportChargesMethodOfPaymentDropEdit, "ZG_TransportChargesMethodOfPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(null)).ZG_TransportChargesMethodOfPayment)));
			this.TransportChargesMethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 56, true);
			this.TransportChargesMethodOfPaymentDropEdit.Name = "TransportChargesMethodOfPaymentDropEdit";
			this.TransportChargesMethodOfPaymentDropEdit.PreBoundMaxLength = 1;
			this.TransportChargesMethodOfPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.TransportChargesMethodOfPaymentDropEdit.TabIndex = 1;
			// 
			// IncoTermsAgreedPlaceLongTextControl
			//
			this.BindingSource.SetBindingMember(this.IncoTermsAgreedPlaceLongTextControl, "IncoTermsAgreedPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(null)).IncoTermsAgreedPlace)));
			this.IncoTermsAgreedPlaceLongTextControl.AllowDrop = true;
			this.IncoTermsAgreedPlaceLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.IncoTermsAgreedPlaceLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 2, true);
			this.IncoTermsAgreedPlaceLongTextControl.Name = "IncoTermsAgreedPlaceLongTextControl";
			this.IncoTermsAgreedPlaceLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.IncoTermsAgreedPlaceLongTextControl.TabIndex = 2;
			// 
			// InvoiceDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransportChargesMethodOfPaymentDropEdit);
			this.Controls.Add(this.AgreedPlaceCodeFindBox);
			this.Controls.Add(this.IncoTermsAgreedPlaceLongTextControl);
			this.Name = "InvoiceDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 93, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AgreedPlaceCodeFindBox.ResumeLayout(true);
			this.AgreedPlaceCodeFindBox.PerformLayout();
			this.TransportChargesMethodOfPaymentDropEdit.ResumeLayout(true);
			this.TransportChargesMethodOfPaymentDropEdit.PerformLayout();
			this.IncoTermsAgreedPlaceLongTextControl.ResumeLayout(true);
			this.IncoTermsAgreedPlaceLongTextControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox AgreedPlaceCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit TransportChargesMethodOfPaymentDropEdit;
		internal Enterprise.Customs.GUI.LongTextControl IncoTermsAgreedPlaceLongTextControl;
	}
}
