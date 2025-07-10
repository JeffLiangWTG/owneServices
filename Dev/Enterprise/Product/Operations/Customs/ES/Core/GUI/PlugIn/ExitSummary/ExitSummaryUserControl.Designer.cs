using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	partial class ExitSummaryUserControl
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
			this.ArrivalNotificationPlaceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MovementGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MovementsGrid)).BeginInit();
			this.MovementsGrid.SuspendLayout();
			this.CustomsOfficeCodeFindBox.SuspendLayout();
			this.ArrivalNotificationDateDateEdit.SuspendLayout();
			this.ExitDateDateEdit.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.CarrierOrgAddressControl.SuspendLayout();
			this.HeaderCustomsOfficeCodeFindBox.SuspendLayout();
			this.HeaderArrivalNotificationDateDateEdit.SuspendLayout();
			this.HeaderExitDateDateEdit.SuspendLayout();
			this.MessagesGroupBox.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingGrid)).BeginInit();
			this.PackingGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.MovementDetailsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ArrivalNotificationPlaceDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MovementGroupBox
			// 
			this.MovementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 173, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1315, 173, true);
			this.TopPanel.Controls.SetChildIndex(this.ReferenceNumberTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.HeaderCustomsOfficeCodeFindBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.HeaderArrivalNotificationDateDateEdit, 0);
			this.TopPanel.Controls.SetChildIndex(this.HeaderArrivalNotificationPlaceTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.HeaderExitDateDateEdit, 0);
			this.TopPanel.Controls.SetChildIndex(this.HeaderTransportIdTextBox, 0);
			// 
			// MovementDetailsPanel
			// 
			this.MovementDetailsPanel.Controls.Add(this.ArrivalNotificationPlaceDropEdit);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.ItemsGroupBox, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.CarrierOrgAddressControl, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.ArrivalNotificationPlaceDropEdit, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.ArrivalNotificationPlaceTextBox, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.ExitDateDateEdit, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.ArrivalNotificationDateDateEdit, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.TransportIdTextBox, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.MovementReferenceNumberTextBox, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.StatusDropEdit, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.CustomsOfficeCodeFindBox, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.CusExitControlHeader);
			// 
			// ArrivalNotificationPlaceDropEdit
			// 
			this.ArrivalNotificationPlaceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ArrivalNotificationPlaceDropEdit, "CusExitDetails.CED_ArrivalNotificationPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_ArrivalNotificationPlace)));
			this.ArrivalNotificationPlaceDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("DDAAD4EA-2F32-4204-AC6B-0682C668BE0D", "Arrival Notification Place");
			this.ArrivalNotificationPlaceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 84, true);
			this.ArrivalNotificationPlaceDropEdit.Name = "ArrivalNotificationPlaceDropEdit";
			this.ArrivalNotificationPlaceDropEdit.ShouldResizeByMaxLength = true;
			this.ArrivalNotificationPlaceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.ArrivalNotificationPlaceDropEdit.TabIndex = 5;
			// 
			// ExitSummaryUserControl
			// 
			this.Name = "ExitSummaryUserControl";
			this.MovementGroupBox.ResumeLayout(false);
			this.MovementGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MovementsGrid)).EndInit();
			this.MovementsGrid.ResumeLayout(false);
			this.MovementsGrid.PerformLayout();
			this.CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeCodeFindBox.PerformLayout();
			this.ArrivalNotificationDateDateEdit.ResumeLayout(true);
			this.ArrivalNotificationDateDateEdit.PerformLayout();
			this.ExitDateDateEdit.ResumeLayout(true);
			this.ExitDateDateEdit.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.CarrierOrgAddressControl.ResumeLayout(true);
			this.CarrierOrgAddressControl.PerformLayout();
			this.HeaderCustomsOfficeCodeFindBox.ResumeLayout(true);
			this.HeaderCustomsOfficeCodeFindBox.PerformLayout();
			this.HeaderArrivalNotificationDateDateEdit.ResumeLayout(true);
			this.HeaderArrivalNotificationDateDateEdit.PerformLayout();
			this.HeaderExitDateDateEdit.ResumeLayout(true);
			this.HeaderExitDateDateEdit.PerformLayout();
			this.MessagesGroupBox.ResumeLayout(false);
			this.MessagesGroupBox.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingGrid)).EndInit();
			this.PackingGrid.ResumeLayout(false);
			this.PackingGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.MovementDetailsPanel.ResumeLayout(false);
			this.MovementDetailsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ArrivalNotificationPlaceDropEdit.ResumeLayout(true);
			this.ArrivalNotificationPlaceDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZDropEdit ArrivalNotificationPlaceDropEdit;
	}
}
