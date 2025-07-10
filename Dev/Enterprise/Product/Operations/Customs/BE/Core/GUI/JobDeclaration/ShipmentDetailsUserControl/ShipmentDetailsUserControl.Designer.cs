using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI
{
	partial class ShipmentDetailsUserControl
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
			this.LocationOfGoodsCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PresentationStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ShipmentDetailsOriginUserControl = new Enterprise.Customs.BE.GUI.ShipmentDetailsOriginUserControl();
			this.ShipmentDetailsFinalDestinationUserControl = new Enterprise.Customs.BE.GUI.ShipmentDetailsFinalDestinationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LocationOfGoodsCodeFindBox.SuspendLayout();
			this.PresentationStartDateEdit.SuspendLayout();
			this.ShipmentDetailsOriginUserControl.SuspendLayout();
			this.ShipmentDetailsFinalDestinationUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BE.Business.Declaration.JobDeclaration);
			// 
			// LocationOfGoodsCodeFindBox
			// 
			this.LocationOfGoodsCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationOfGoodsCodeFindBox, "JE_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).JE_LocationOfGoods)));
			this.LocationOfGoodsCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 21, true);
			this.LocationOfGoodsCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.LocationOfGoodsCodeFindBox.Name = "LocationOfGoodsCodeFindBox";
			this.LocationOfGoodsCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LocationOfGoodsCodeFindBox.ParentType = null;
			this.LocationOfGoodsCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 15, true);
			this.LocationOfGoodsCodeFindBox.TabIndex = 0;
			// 
			// PresentationStartDateEdit
			// 
			this.PresentationStartDateEdit.AllowDrop = true;
			this.PresentationStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.PresentationStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PresentationStartDateEdit, "ZG_PresentationStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).ZG_PresentationStartDate)));
			this.PresentationStartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PresentationStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 16, true);
			this.PresentationStartDateEdit.Name = "PresentationStartDateEdit";
			this.PresentationStartDateEdit.TabIndex = 1;
			//
			// ShipmentDetailsOriginUserControl
			//
			this.ShipmentDetailsOriginUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsOriginUserControl, ".");
			this.ShipmentDetailsOriginUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.ShipmentDetailsOriginUserControl.Name = "ShipmentDetailsOriginUserControl";
			this.ShipmentDetailsOriginUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.ShipmentDetailsOriginUserControl.TabIndex = 25;
			//
			// ShipmentDetailsFinalDestinationUserControl
			//
			this.ShipmentDetailsFinalDestinationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsFinalDestinationUserControl, ".");
			this.ShipmentDetailsFinalDestinationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentDetailsFinalDestinationUserControl.Name = "ShipmentDetailsFinalDestinationUserControl";
			this.ShipmentDetailsFinalDestinationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.ShipmentDetailsFinalDestinationUserControl.TabIndex = 25;
			//
			// ShipmentDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LocationOfGoodsCodeFindBox);
			this.Controls.Add(this.PresentationStartDateEdit);
			this.Controls.Add(this.ShipmentDetailsOriginUserControl);
			this.Controls.Add(this.ShipmentDetailsFinalDestinationUserControl);
			this.Name = "ShipmentDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 78, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LocationOfGoodsCodeFindBox.ResumeLayout(true);
			this.LocationOfGoodsCodeFindBox.PerformLayout();
			this.PresentationStartDateEdit.ResumeLayout(true);
			this.PresentationStartDateEdit.PerformLayout();
			this.ShipmentDetailsOriginUserControl.ResumeLayout(true);
			this.ShipmentDetailsOriginUserControl.PerformLayout();
			this.ShipmentDetailsFinalDestinationUserControl.ResumeLayout(true);
			this.ShipmentDetailsFinalDestinationUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZCodeFindBox LocationOfGoodsCodeFindBox;
		internal ZDateEdit PresentationStartDateEdit;
		internal ShipmentDetailsOriginUserControl ShipmentDetailsOriginUserControl;
		internal ShipmentDetailsFinalDestinationUserControl ShipmentDetailsFinalDestinationUserControl;

		#endregion
	}
}
