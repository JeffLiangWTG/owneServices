namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class CustomJobConversationMessageUserControl
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		void InitializeComponent()
		{
			this.ConversationMessageGrid = new ZArchitecture.GUI.ZDisplayGrid();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.ConversationMessageGrid)).BeginInit();
			this.ConversationMessageGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ConversationMessageGrid
			// 
			this.ConversationMessageGrid.AllowBeginDrag = false;
			this.ConversationMessageGrid.AllowDragDropWithChanges = false;
			this.ConversationMessageGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConversationMessageGrid, ".");
			zCheckBoxColumnStyleInfo1.Caption = " ";
			zCheckBoxColumnStyleInfo1.ColumnName = "IsChecked";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zDateEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("c8c79c97-7f71-4191-9f34-f7e20c4f5d5b", "Date");
			zDateEditColumnStyleInfo1.ColumnName = "PostedTimeUtc";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("1719fc1e-7216-4e36-acaa-b32ec72b626f", "Work Item Number");
			zTextBoxColumnStyleInfo3.ColumnName = "WorkItemNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zMultiLineTextBoxColumnInfo2.CaptionResourceString = ZClientEDI.Res.GetData("597b1872-257b-43c3-b768-60d4b0161141", "eConversation");
			zMultiLineTextBoxColumnInfo2.ColumnName = "Body";
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			zMultiLineTextBoxColumnInfo2.IsReadOnly = true;
			this.ConversationMessageGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ConversationMessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ConversationMessageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ConversationMessageGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.ConversationMessageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.ConversationMessageGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ConversationMessageGrid.TabIndex = 3;
			this.ConversationMessageGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CustomJobConversationMessage);
			// 
			// CustomJobConversationMessageUserControl
			// 
			this.Name = "CustomJobConversationMessageUserControl";
			this.Controls.Add(ConversationMessageGrid);
			((System.ComponentModel.ISupportInitialize)(this.ConversationMessageGrid)).EndInit();
			this.ConversationMessageGrid.ResumeLayout(false);
			this.ConversationMessageGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected ZArchitecture.GUI.ZDisplayGrid ConversationMessageGrid;

		#endregion
	}
}
