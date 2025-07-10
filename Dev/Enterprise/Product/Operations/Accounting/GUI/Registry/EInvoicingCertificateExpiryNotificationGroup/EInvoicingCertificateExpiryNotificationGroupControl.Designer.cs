using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class EInvoicingCertificateExpiryNotificationGroupControl
	{
		protected ZArchitecture.GUI.ZGuidFindBox NotificationGroupGuidFindBox;

		#region Component Designer generated code

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.NotificationGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AlertDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NotificationGroupGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.EInvoicingCertificateExpiryNotificationGroup);
			// 
			// OrganisationGuidFindBox
			// 
			this.NotificationGroupGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotificationGroupGuidFindBox, "NotificationGroup");
			this.NotificationGroupGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("EInvoicingCertificateExpiryNotificationGroupControl|4CDD70E0-A227-4E21-9D22-BA0B279D5651", "Group");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Registry.Business.EInvoicingCertificateExpiryNotificationGroup)(null)).NotificationGroup)));
			this.NotificationGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 3, true);
			this.NotificationGroupGuidFindBox.Name = "NotificationGroupGuidFindBox";
			this.NotificationGroupGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.NotificationGroupGuidFindBox.ParentType = null;
			this.NotificationGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 17, true);
			this.NotificationGroupGuidFindBox.TabIndex = 1;
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AlertDaysCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AlertDaysCalcEdit, "AlertDays");
			this.AlertDaysCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("EInvoicingCertificateExpiryNotificationGroupControl|63805BAD-5598-41CC-93A2-CC597F03ECB7", "Days");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.EInvoicingCertificateExpiryNotificationGroup)(null)).AlertDays)));
			this.AlertDaysCalcEdit.DecimalPlaces = 2;
			this.AlertDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 27, true);
			this.AlertDaysCalcEdit.Name = "AlertDaysCalcEdit";
			this.AlertDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 17, true);
			this.AlertDaysCalcEdit.TabIndex = 25;
			this.AlertDaysCalcEdit.Text = "0";
			this.AlertDaysCalcEdit.DecimalPlaces = 0;
			this.AlertDaysCalcEdit.AllowNegative = false;
			this.AlertDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.AlertDaysCalcEdit.TrackDisposedAccess = true;
			// 
			// EInvoicingCertificateExpiryNotificationGroupControl
			// 
			this.Controls.Add(this.AlertDaysCalcEdit);
			this.Controls.Add(this.NotificationGroupGuidFindBox);
			this.Name = "EInvoicingCertificateExpiryNotificationGroupControl";
			this.CaptionRenderingEnabled = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 65, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NotificationGroupGuidFindBox.ResumeLayout(true);
			this.NotificationGroupGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit AlertDaysCalcEdit;
	}
}
