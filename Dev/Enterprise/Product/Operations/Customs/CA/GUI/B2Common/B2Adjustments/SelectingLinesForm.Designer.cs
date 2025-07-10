namespace Enterprise.Customs.CA.GUI
{
	partial class SelectingLinesForm
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.SelectingLinesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.OriginalLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OriginalLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SelectingLinesSplitContainer)).BeginInit();
			this.SelectingLinesSplitContainer.Panel1.SuspendLayout();
			this.SelectingLinesSplitContainer.Panel2.SuspendLayout();
			this.SelectingLinesSplitContainer.SuspendLayout();
			this.OriginalLinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OriginalLinesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Dock = System.Windows.Forms.DockStyle.None;
			this.MainStatusBar.Enabled = false;
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 268, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 22, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.ClassificationLineWrapperCollection);
			// 
			// SelectingLinesSplitContainer
			// 
			this.SelectingLinesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectingLinesSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.SelectingLinesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SelectingLinesSplitContainer.Name = "SelectingLinesSplitContainer";
			this.SelectingLinesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SelectingLinesSplitContainer.Panel1
			// 
			this.SelectingLinesSplitContainer.Panel1.Controls.Add(this.OriginalLinesGroupBox);
			// 
			// SelectingLinesSplitContainer.Panel2
			// 
			this.SelectingLinesSplitContainer.Panel2.Controls.Add(this.SelectAllButton);
			this.SelectingLinesSplitContainer.Panel2.Controls.Add(this.OKButton);
			this.SelectingLinesSplitContainer.Panel2.Controls.Add(this.CancelButton);
			this.SelectingLinesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 290, true);
			this.SelectingLinesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(261);
			this.SelectingLinesSplitContainer.TabIndex = 1;
			// 
			// OriginalLinesGroupBox
			// 
			this.OriginalLinesGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5302e97d-f096-491d-8332-c83044d37205", "Entry Original Lines");
			this.OriginalLinesGroupBox.Controls.Add(this.OriginalLinesGrid);
			this.OriginalLinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OriginalLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OriginalLinesGroupBox.Name = "OriginalLinesGroupBox";
			this.OriginalLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 261, true);
			this.OriginalLinesGroupBox.TabIndex = 0;
			this.OriginalLinesGroupBox.TabStop = false;
			// 
			// OriginalLinesGrid
			// 
			this.OriginalLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OriginalLinesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.ClassificationLine1Wrapper)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.ClassificationLine1Wrapper)(null)).B3SubHeaderNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.ClassificationLine1Wrapper)(null)).B3LineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ClassificationLine1Wrapper)(null)).ClassificationNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ClassificationLine1Wrapper)(null)).TariffCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ClassificationLine1Wrapper)(null)).AuthorityNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.ClassificationLine1Wrapper)(null)).ValueForCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ClassificationLine1Wrapper)(null)).ValueForDutyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ClassificationLine1Wrapper)(null)).IsSelected)));
			this.OriginalLinesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "B3SubHeaderNumber";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "B3LineNumber";
			zTextBoxColumnStyleInfo1.ColumnName = "ClassificationNumber";
			zTextBoxColumnStyleInfo2.ColumnName = "TariffCode";
			zTextBoxColumnStyleInfo3.ColumnName = "AuthorityNumber";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "ValueForCurrency";
			zTextBoxColumnStyleInfo4.ColumnName = "ValueForDutyCode";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("38d8807d-92f1-4312-8a1a-09c5fb5b4f35", "Selected?");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsSelected";
			this.OriginalLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OriginalLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.OriginalLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OriginalLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OriginalLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OriginalLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.OriginalLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OriginalLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.OriginalLinesGrid.CopySelectedRowsAllowed = true;
			this.OriginalLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OriginalLinesGrid.GridId = "f052993d-8908-4766-8bb7-c33d83fa3120";
			this.OriginalLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OriginalLinesGrid.LayoutKey = "OriginalLinesGrid";
			this.OriginalLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OriginalLinesGrid.Name = "OriginalLinesGrid";
			this.OriginalLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 242, true);
			this.OriginalLinesGrid.TabIndex = 0;
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b4c39ead-d89a-4baa-8b84-2a6a0dc2652e", "Select All");
			this.SelectAllButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(509, 0, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 25, true);
			this.SelectAllButton.TabIndex = 1;
			this.SelectAllButton.UseVisualStyleBackColor = true;
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b40e3517-d01c-436d-bd55-3a17e6203d2a", "OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(584, 0, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 25, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.UseVisualStyleBackColor = true;
			// 
			// CancelButton
			// 
			this.CancelButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("47d36a9b-e822-4a01-9220-c7f60ebbb005", "Cancel");
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(659, 0, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 25, true);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.UseVisualStyleBackColor = true;
			// 
			// SelectingLinesForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 290, true);
			this.Controls.Add(this.SelectingLinesSplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.ClassificationLineWrapperCollection);
			this.Name = "SelectingLinesForm";
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fb7cd247-9642-412b-9b50-706fb5a04e91", "Selecting Lines Form");
			this.Controls.SetChildIndex(this.SelectingLinesSplitContainer, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SelectingLinesSplitContainer.Panel1.ResumeLayout(false);
			this.SelectingLinesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SelectingLinesSplitContainer)).EndInit();
			this.SelectingLinesSplitContainer.ResumeLayout(false);
			this.OriginalLinesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.OriginalLinesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer SelectingLinesSplitContainer;
		private ZArchitecture.GUI.ZGroupBox OriginalLinesGroupBox;
		private ZArchitecture.ZGrid OriginalLinesGrid;
		private ZArchitecture.GUI.ZButton OKButton;
		private new ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.GUI.ZButton SelectAllButton;
	}
}
