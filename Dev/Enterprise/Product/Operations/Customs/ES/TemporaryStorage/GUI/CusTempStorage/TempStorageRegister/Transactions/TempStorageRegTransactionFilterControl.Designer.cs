namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

partial class TempStorageRegTransactionFilterControl
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
		Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TempStorageRegTransactionFilterControl));
		this.NewTransactionButton = new Enterprise.ZArchitecture.GUI.ZButton();
		((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
		this.grid.SuspendLayout();
		this.AddStripButton.SuspendLayout();
		this.FilterStripsPanel.SuspendLayout();
		this.RecentItemsPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.SuspendLayout();
		// 
		// grid
		// 
		this.BindingSource.SetBindingMember(this.grid, "CusTempStorageRegLineTransactionsForFilter");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)).SyncRoot)).PhysicalInOutDate)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)).SyncRoot)).TransactionDate)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)).SyncRoot)).SRT_TransactionType)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)).SyncRoot)).TransactionTypeDescription)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)).SyncRoot)).SRT_GrossWeight)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)).SyncRoot)).SRT_PackageQty)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)).SyncRoot)).SRT_BondAmount)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)).SyncRoot)).SRT_ReferenceType)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)).SyncRoot)).SRT_InternalReferenceType)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)).SyncRoot)).SRT_InternalReferenceNumber)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)).SyncRoot)).SRT_Reference)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)).SyncRoot)).SRT_Comments)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactionsForFilter)).SyncRoot)).SRT_TransactionStatus)));
		this.grid.CaptionVisible = false;
		zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("84CD50C2-52D3-4E29-B778-786D800FC259", "Physical In/Out Date");
		zDateEditColumnStyleInfo2.ColumnName = "PhysicalInOutDate";
		zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
		zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
		zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("BFA7FB2A-395E-4A23-BFAB-6D9472ED3955", "Transaction Date");
		zDateEditColumnStyleInfo3.ColumnName = "TransactionDate";
		zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
		zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
		zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("e24734ff-ff5a-4e48-a992-d4645197fe14", "Transaction Type");
		zDropEditColumnStyleInfo1.ColumnName = "SRT_TransactionType";
		zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
		zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("e24734ff-ff5a-4e48-a992-d4645197fe14", "Transaction Type");
		zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
		zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("3aff30cd-b521-4f0e-8e4f-08e6dd9e62c6", "Transaction Type Description");
		zTextBoxColumnStyleInfo1.ColumnName = "TransactionTypeDescription";
		zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("e24734ff-ff5a-4e48-a992-d4645197fe14", "Transaction Type");
		zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
		zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(164);
		zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("e9bf6e44-164b-4709-b42e-090dec6ed200", "Gross Weight in KGs");
		zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo1.ColumnName = "SRT_GrossWeight";
		zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
		zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
		zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("5a30066b-62b7-4d01-8144-bb729a47cd2b", "Package Quantity");
		zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo2.ColumnName = "SRT_PackageQty";
		zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
		zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
		zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("32C0E378-EE23-4C4F-83B1-97AFAC1A62C4", "Bond Amount");
		zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo4.ColumnName = "SRT_BondAmount";
		zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
		zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
		zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("7cf24eec-3610-4262-99a7-d5902ead7f8f", "Reference Type");
		zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zDropEditColumnStyleInfo2.ColumnName = "SRT_ReferenceType";
		zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("ecf18d26-2ca6-49c5-96d2-1d628780aaaa", "Reference");
		zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
		zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(99);
		zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("0BE6D4BB-C3DB-4082-818B-37872C8BA89E", englishCaption: "Internal Reference Type", englishMediumCaption: "Int. Ref. Type", englishShortCaption: "Int. Type", englishFullDescription: "The Internal Reference Type");
		zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zDropEditColumnStyleInfo4.ColumnName = "SRT_InternalReferenceType";
		zDropEditColumnStyleInfo4.GroupName = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("ecf18d26-2ca6-49c5-96d2-1d628780aaaa", "Reference");
		zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
		zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(99);
		zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("77d3fb0d-6a51-432f-8036-b34537483886", "Internal Reference No.");
		zTextBoxColumnStyleInfo2.ColumnName = "SRT_InternalReferenceNumber";
		zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("ecf18d26-2ca6-49c5-96d2-1d628780aaaa", "Reference");
		zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
		zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
		zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("6b5ea5bd-dbb6-4b0c-b535-4ff206e0ae7c", "Reference Number");
		zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zTextBoxColumnStyleInfo3.ColumnName = "SRT_Reference";
		zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
		zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
		zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("1270eeb3-a649-4cf8-aa75-a37b3e5cd0ec", "Comments");
		zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		zTextBoxColumnStyleInfo4.ColumnName = "SRT_Comments";
		zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
		zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
		zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("B0FDA045-2C36-473A-A0D1-4323F38CD3EE", "Transaction Status");
		zDropEditColumnStyleInfo3.ColumnName = "SRT_TransactionStatus";
		zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
		zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(164);
		this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
		this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
		this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
		this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
		this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
		this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
		this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
		this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
		this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
		this.grid.GridId = "22ED440F-7670-40CD-A492-F3FA7BD9226A";
		this.grid.LayoutKey = "grid";
		this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 30, true);
		this.grid.TabIndex = 0;
		// 
		// FilterStripsPanel
		// 
		this.FilterStripsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 40, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine);
		// 
		// NewTransactionButton
		// 
		this.NewTransactionButton.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("4EA55DDE-4C9A-4B3B-90FC-0E0DE2816E14", "New Transaction");
		this.NewTransactionButton.Image = ((System.Drawing.Image)(resources.GetObject("NewTransactionButton.Image")));
		this.NewTransactionButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.NewTransactionButton.IsCaptionOverridden = false;
		this.NewTransactionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 3, true);
		this.NewTransactionButton.Name = "NewTransactionButton";
		this.NewTransactionButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
		this.NewTransactionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 23, true);
		this.NewTransactionButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
		this.NewTransactionButton.ToolTipCaption = null;
		this.NewTransactionButton.TabIndex = 1;
		this.NewTransactionButton.Click += new System.EventHandler(this.NewTransactionButton_Click);
		// 
		// TempStorageRegTransactionFilterControl
		// 
		this.CaptionRenderingEnabled = true;
		this.Name = "TempStorageRegTransactionFilterControl";
		this.Controls.Add(this.NewTransactionButton);
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 182, true);
		((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
		this.grid.ResumeLayout(false);
		this.grid.PerformLayout();
		this.AddStripButton.ResumeLayout(true);
		this.AddStripButton.PerformLayout();
		this.FilterStripsPanel.ResumeLayout(false);
		this.FilterStripsPanel.PerformLayout();
		this.RecentItemsPanel.ResumeLayout(false);
		this.RecentItemsPanel.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal Enterprise.ZArchitecture.GUI.ZButton NewTransactionButton;
}
