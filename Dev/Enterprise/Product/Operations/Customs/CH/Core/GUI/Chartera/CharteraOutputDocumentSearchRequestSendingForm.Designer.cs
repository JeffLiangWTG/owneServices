namespace Enterprise.Customs.CH.GUI;

partial class CharteraOutputDocumentSearchRequestSendingForm
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private new void InitializeComponent()
    {
        this.CreationTimeFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
        this.CreationTimeToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
        this.HistoryQueryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
        this.CancelButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.CreationTimeFromDateEdit.SuspendLayout();
        this.CreationTimeToDateEdit.SuspendLayout();
        this.SuspendLayout();
        //
        // MainStatusBar
        //
        this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
        this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 24, true);
        this.MainStatusBar.TabIndex = 5;
        //
        // BindingSource
        //
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.CharteraOutputDocumentSearchSendingObject);
        //
        // CreationTimeFromDateEdit
        //
        this.CreationTimeFromDateEdit.AllowDrop = true;
        this.CreationTimeFromDateEdit.AutoCompleteMonthThreshold = 1;
        this.BindingSource.SetBindingMember(this.CreationTimeFromDateEdit, "CreationTimeFrom");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.CharteraOutputDocumentSearchSendingObject)(null)).CreationTimeFrom)));
        this.CreationTimeFromDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
        this.CreationTimeFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 12, true);
        this.CreationTimeFromDateEdit.Name = "CreationTimeFromDateEdit";
        this.CreationTimeFromDateEdit.TabIndex = 0;
        //
        // CreationTimeToDateEdit
        //
        this.CreationTimeToDateEdit.AllowDrop = true;
        this.CreationTimeToDateEdit.AutoCompleteMonthThreshold = 1;
        this.BindingSource.SetBindingMember(this.CreationTimeToDateEdit, "CreationTimeTo");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.CharteraOutputDocumentSearchSendingObject)(null)).CreationTimeTo)));
        this.CreationTimeToDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
        this.CreationTimeToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 12, true);
        this.CreationTimeToDateEdit.Name = "CreationTimeToDateEdit";
        this.CreationTimeToDateEdit.TabIndex = 1;
        //
        // HistoryQueryCheckBox
        //
        this.BindingSource.SetBindingMember(this.HistoryQueryCheckBox, "IsHistoricalQuery");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.CharteraOutputDocumentSearchSendingObject)(null)).IsHistoricalQuery)));
        this.HistoryQueryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 38, true);
        this.HistoryQueryCheckBox.Name = "HistoryQueryCheckBox";
        this.HistoryQueryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 24, true);
        this.HistoryQueryCheckBox.TabIndex = 2;
        //
        // SendButton
        //
        this.SendButton.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("d645f058-85ce-4939-a56e-64a0f2d2fad9", "Send");
        this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 74, true);
        this.SendButton.Name = "SendButton";
        this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
        this.SendButton.TabIndex = 3;
        this.SendButton.ToolTipCaption = null;
        this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
        //
        // CancelButton2
        //
        this.CancelButton2.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("8db86651-8efe-4abd-a043-3be56ed8a0b4", "Cancel");
        this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 74, true);
        this.CancelButton2.Name = "CancelButton2";
        this.CancelButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
        this.CancelButton2.TabIndex = 4;
        this.CancelButton2.ToolTipCaption = null;
        this.CancelButton2.Click += new System.EventHandler(this.CancelButton2_Click);
        //
        // CharteraOutputDocumentSearchRequestSendingForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.AutoScroll = true;
        this.CaptionRenderingEnabled = true;
        this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 136, true);
        this.Controls.Add(this.CreationTimeFromDateEdit);
        this.Controls.Add(this.CreationTimeToDateEdit);
        this.Controls.Add(this.HistoryQueryCheckBox);
        this.Controls.Add(this.SendButton);
        this.Controls.Add(this.CancelButton2);
        this.DataSourceType = typeof(Enterprise.Customs.CH.Business.CharteraOutputDocumentSearchSendingObject);
        this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 173, true);
        this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 173, true);
        this.Name = "CharteraOutputDocumentSearchRequestSendingForm";
        this.Text = "CharteraOutputDocumentSearchRequestSendingForm";
        this.Controls.SetChildIndex(this.CancelButton2, 0);
        this.Controls.SetChildIndex(this.SendButton, 0);
        this.Controls.SetChildIndex(this.HistoryQueryCheckBox, 0);
        this.Controls.SetChildIndex(this.CreationTimeToDateEdit, 0);
        this.Controls.SetChildIndex(this.CreationTimeFromDateEdit, 0);
        this.Controls.SetChildIndex(this.MainStatusBar, 0);
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.CreationTimeFromDateEdit.ResumeLayout(true);
        this.CreationTimeFromDateEdit.PerformLayout();
        this.CreationTimeToDateEdit.ResumeLayout(true);
        this.CreationTimeToDateEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal Enterprise.ZArchitecture.GUI.ZDateEdit CreationTimeFromDateEdit;
    internal Enterprise.ZArchitecture.GUI.ZDateEdit CreationTimeToDateEdit;
    internal Enterprise.ZArchitecture.GUI.ZCheckBox HistoryQueryCheckBox;
    internal Enterprise.ZArchitecture.GUI.ZButton SendButton;
    internal Enterprise.ZArchitecture.GUI.ZButton CancelButton2;
}
