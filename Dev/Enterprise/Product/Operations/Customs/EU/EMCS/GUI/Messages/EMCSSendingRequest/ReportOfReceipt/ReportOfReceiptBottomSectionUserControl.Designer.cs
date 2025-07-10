namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class ReportOfReceiptBottomSectionUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.ReportOfReceiptGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ArrivalDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReceiptResultDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ComplementaryInformationWrapTextBox = new Enterprise.Customs.GUI.WordWrappingTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReportOfReceiptGroupBox.SuspendLayout();
			this.ArrivalDate.SuspendLayout();
			this.ReceiptResultDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.ReportOfReceiptSendingActionParent);
			// 
			// ReportOfReceiptGroupBox
			// 
			this.ReportOfReceiptGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("E916DB41-FBB6-4D9C-9A94-062BDDA7990C", "Send Report of Receipt");
			this.ReportOfReceiptGroupBox.Controls.Add(this.ArrivalDate);
			this.ReportOfReceiptGroupBox.Controls.Add(this.ReceiptResultDropEdit);
			this.ReportOfReceiptGroupBox.Controls.Add(this.ComplementaryInformationWrapTextBox);
			this.ReportOfReceiptGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportOfReceiptGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportOfReceiptGroupBox.Name = "ReportOfReceiptGroupBox";
			this.ReportOfReceiptGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 154, true);
			this.ReportOfReceiptGroupBox.TabIndex = 0;
			this.ReportOfReceiptGroupBox.TabStop = false;
			// 
			// ArrivalDate
			// 
			this.ArrivalDate.AllowDrop = true;
			this.ArrivalDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ArrivalDate, "SendingObjectsCollection.ArrivalDate");
			this.ArrivalDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ArrivalDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 16, true);
			this.ArrivalDate.Name = "ArrivalDate";
			this.ArrivalDate.TabIndex = 0;
			// 
			// ReceiptResultDropEdit
			// 
			this.ReceiptResultDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceiptResultDropEdit, "SendingObjectsCollection.ReceiptResult");
			this.ReceiptResultDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 43, true);
			this.ReceiptResultDropEdit.Name = "ReceiptResultDropEdit";
			this.ReceiptResultDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 20, true);
			this.ReceiptResultDropEdit.TabIndex = 1;
			// 
			// ComplementaryInformationWrapTextBox
			// 
			this.ComplementaryInformationWrapTextBox.AcceptsReturn = true;
			this.ComplementaryInformationWrapTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComplementaryInformationWrapTextBox, "SendingObjectsCollection.ComplementaryInformation");
			this.ComplementaryInformationWrapTextBox.CaptionResourceString = null;
			this.ComplementaryInformationWrapTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.ComplementaryInformationWrapTextBox, 9);
			this.ComplementaryInformationWrapTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 71, true);
			this.ComplementaryInformationWrapTextBox.Multiline = true;
			this.ComplementaryInformationWrapTextBox.Name = "ComplementaryInformationWrapTextBox";
			this.ComplementaryInformationWrapTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ComplementaryInformationWrapTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 74, true);
			this.ComplementaryInformationWrapTextBox.TabIndex = 2;
			// 
			// ReportOfReceiptBottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReportOfReceiptGroupBox);
			this.Name = "ReportOfReceiptBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 154, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReportOfReceiptGroupBox.ResumeLayout(false);
			this.ReportOfReceiptGroupBox.PerformLayout();
			this.ArrivalDate.ResumeLayout(true);
			this.ArrivalDate.PerformLayout();
			this.ReceiptResultDropEdit.ResumeLayout(true);
			this.ReceiptResultDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Customs.GUI.WordWrappingTextBox ComplementaryInformationWrapTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ReceiptResultDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit ArrivalDate;
		internal ZArchitecture.GUI.ZGroupBox ReportOfReceiptGroupBox;

		#endregion
	}
}
