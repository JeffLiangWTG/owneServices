using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class Phase5ArrivalNotificationTabUserControl
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
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        this.MovementReferenceNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
        this.MovementReferenceNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.MovementReferenceNumbersGrid)).BeginInit();
        this.MovementReferenceNumbersGrid.SuspendLayout();
        this.MovementReferenceNumbersGroupBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsHeader);
        // 
        // MovementReferenceNumbersGrid
        // 
        this.MovementReferenceNumbersGrid.AllowNavigation = false;
        this.MovementReferenceNumbersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.MovementReferenceNumbersGrid, "ArrivalMovementHeader.MovementReferenceNumbers");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.MovementReferenceNumbers)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CH.NCTS.Business.MovementReferenceNumberSupportingInfo)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.MovementReferenceNumbers)).SyncRoot)).CSI_LineNo)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.MovementReferenceNumberSupportingInfo)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.MovementReferenceNumbers)).SyncRoot)).CSI_ReferenceNumber)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.MovementReferenceNumberSupportingInfo)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.MovementReferenceNumbers)).SyncRoot)).CSI_Status)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.MovementReferenceNumberSupportingInfo)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.MovementReferenceNumbers)).SyncRoot)).CSI_Description)));
        this.MovementReferenceNumbersGrid.CaptionVisible = false;
        zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
        zTextBoxColumnStyleInfo1.ColumnName = "CSI_LineNo";
        zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
        zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber";
        zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
        zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        zDropEditColumnStyleInfo1.ColumnName = "CSI_Status";
        zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
        zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
        zTextBoxColumnStyleInfo3.ColumnName = "CSI_Description";
        zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
        this.MovementReferenceNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.MovementReferenceNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.MovementReferenceNumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.MovementReferenceNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
        this.MovementReferenceNumbersGrid.GridId = "3BE83582-8A14-49B3-880C-967AC60E15A7";
        this.MovementReferenceNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.MovementReferenceNumbersGrid.LayoutKey = "MovementReferenceNumbersGrid";
        this.MovementReferenceNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 23, true);
        this.MovementReferenceNumbersGrid.Name = "MovementReferenceNumbersGrid";
        this.MovementReferenceNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1295, 259, true);
        this.MovementReferenceNumbersGrid.TabIndex = 4;
        // 
        // MovementReferenceNumbersGroupBox
        // 
        this.MovementReferenceNumbersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.MovementReferenceNumbersGroupBox.CaptionResourceString = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("07D53FD3-1922-4D82-8E69-69C75EF44C74", "Movement Reference Numbers");
        this.MovementReferenceNumbersGroupBox.Controls.Add(this.MovementReferenceNumbersGrid);
        this.MovementReferenceNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 363, true);
        this.MovementReferenceNumbersGroupBox.Name = "MovementReferenceNumbersGroupBox";
        this.MovementReferenceNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1305, 287, true);
        this.MovementReferenceNumbersGroupBox.TabIndex = 3;
        this.MovementReferenceNumbersGroupBox.TabStop = false;
        // 
        // Phase5ArrivalNotificationTabUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.Controls.Add(this.MovementReferenceNumbersGroupBox);
        this.Name = "Phase5ArrivalNotificationTabUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1309, 652, true);
        this.Controls.SetChildIndex(this.MovementReferenceNumbersGroupBox, 0);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.MovementReferenceNumbersGrid)).EndInit();
        this.MovementReferenceNumbersGrid.ResumeLayout(false);
        this.MovementReferenceNumbersGrid.PerformLayout();
        this.MovementReferenceNumbersGroupBox.ResumeLayout(false);
        this.MovementReferenceNumbersGroupBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.ZGrid MovementReferenceNumbersGrid;
    internal ZGroupBox MovementReferenceNumbersGroupBox;
}
