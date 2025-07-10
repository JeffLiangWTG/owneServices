namespace Enterprise.Customs.BR.GUI
{
	partial class CusGoodsCatalogUserControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ComplementaryDescriptionTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.LocalPartNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LocalPartNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessageStatusDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.ForeignOperatorsUserControl = new Enterprise.Customs.BR.GUI.ForeignOperatorsUserControl();
			this.AuthorityInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AttributesUserControl = new Enterprise.Customs.BR.GUI.AttributesUserControl();
			this.GoodsCatalogGroupBox.SuspendLayout();
			this.TariffNumFindBox.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			this.ConsigneeFindBox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ComplementaryDescriptionTextBox.SuspendLayout();
			this.LocalPartNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LocalPartNumbersGrid)).BeginInit();
			this.LocalPartNumbersGrid.SuspendLayout();
			this.ForeignOperatorsUserControl.SuspendLayout();
			this.AuthorityInfoGroupBox.SuspendLayout();
			this.AttributesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// GoodsCatalogGroupBox
			// 
			this.GoodsCatalogGroupBox.Controls.Add(this.ForeignOperatorsUserControl);
			this.GoodsCatalogGroupBox.Controls.Add(this.AuthorityInfoGroupBox);
			this.GoodsCatalogGroupBox.Controls.Add(this.ComplementaryDescriptionTextBox);
			this.GoodsCatalogGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1135, 295, true);
			this.GoodsCatalogGroupBox.Controls.SetChildIndex(this.CatalogCodeTextBox, 0);
			this.GoodsCatalogGroupBox.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.GoodsCatalogGroupBox.Controls.SetChildIndex(this.TypeDropEdit, 0);
			this.GoodsCatalogGroupBox.Controls.SetChildIndex(this.TariffNumFindBox, 0);
			this.GoodsCatalogGroupBox.Controls.SetChildIndex(this.ConsigneeFindBox, 0);
			this.GoodsCatalogGroupBox.Controls.SetChildIndex(this.ComplementaryDescriptionTextBox, 0);
			this.GoodsCatalogGroupBox.Controls.SetChildIndex(this.AuthorityInfoGroupBox, 0);
			this.GoodsCatalogGroupBox.Controls.SetChildIndex(this.ForeignOperatorsUserControl, 0);
			// 
			// TariffNumFindBox
			// 
			this.TariffNumFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 57, true);
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 121, true);
			// 
			// ConsigneeFindBox
			// 
			this.ConsigneeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 89, true);
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 20, true);
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			// 
			// AuthorityIdentifierTextBox
			// 
			this.AuthorityIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 20, true);
			this.AuthorityIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			// 
			// VersionTextBox
			// 
			this.VersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 48, true);
			this.VersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.CusGoodsCatalog);
			// 
			// ComplementaryDescriptionTextBox
			// 
			this.ComplementaryDescriptionTextBox.AllowDrop = true;
			this.ComplementaryDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ComplementaryDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 171, true);
			this.ComplementaryDescriptionTextBox.Name = "ComplementaryDescriptionTextBox";
			this.ComplementaryDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 23, true);
			this.ComplementaryDescriptionTextBox.TabIndex = 5;
			// 
			// LocalPartNumbersGroupBox
			//
			this.LocalPartNumbersGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("b44718cc-0ec8-405c-843c-0ce920117faf", "Local Part Numbers");
			this.LocalPartNumbersGroupBox.Controls.Add(this.LocalPartNumbersGrid);
			this.LocalPartNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.LocalPartNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 295, true);
			this.LocalPartNumbersGroupBox.Name = "LocalPartNumbersGroupBox";
			this.LocalPartNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 449, true);
			this.LocalPartNumbersGroupBox.TabIndex = 3;
			this.LocalPartNumbersGroupBox.TabStop = false;
			// 
			// LocalPartNumbersGrid
			// 
			this.LocalPartNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LocalPartNumbersGrid, "LocalPartNumbers");
			this.LocalPartNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CGI_Reference";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.LocalPartNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LocalPartNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LocalPartNumbersGrid.GridId = "9f8cb5b6-c4b4-4643-8608-34e8a8e3cbe5";
			this.LocalPartNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LocalPartNumbersGrid.LayoutKey = "LocalPartNameNumbersGrid";
			this.LocalPartNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LocalPartNumbersGrid.Name = "LocalPartNumbersGrid";
			this.LocalPartNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 430, true);
			this.LocalPartNumbersGrid.TabIndex = 0;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "CGC_MessageStatus");
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 48, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.PreBoundMaxLength = 1;
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.MessageStatusDropEdit.TabIndex = 4;
			// 
			// ForeignOperatorsUserControl
			// 
			this.ForeignOperatorsUserControl.AllowDrop = true;
			this.ForeignOperatorsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ForeignOperatorsUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ForeignOperatorsUserControl, ".");
			this.ForeignOperatorsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 19, true);
			this.ForeignOperatorsUserControl.Name = "ForeignOperatorsUserControl";
			this.ForeignOperatorsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 268, true);
			this.ForeignOperatorsUserControl.TabIndex = 3;
			// 
			// AuthorityInfoGroupBox
			// 
			this.AuthorityInfoGroupBox.AutoSize = true;
			this.AuthorityInfoGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("8BF9F9FE-A3B7-4943-ADC2-4EEDC276251A", "Authority Details");
			this.AuthorityInfoGroupBox.Controls.Add(this.StatusDropEdit);
			this.AuthorityInfoGroupBox.Controls.Add(this.VersionTextBox);
			this.AuthorityInfoGroupBox.Controls.Add(this.MessageStatusDropEdit);
			this.AuthorityInfoGroupBox.Controls.Add(this.AuthorityIdentifierTextBox);
			this.AuthorityInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 200, true);
			this.AuthorityInfoGroupBox.Name = "AuthorityInfoGroupBox";
			this.AuthorityInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 87, true);
			this.AuthorityInfoGroupBox.TabIndex = 9;
			this.AuthorityInfoGroupBox.TabStop = false;
			this.AuthorityInfoGroupBox.Controls.SetChildIndex(this.AuthorityIdentifierTextBox, 0);
			this.AuthorityInfoGroupBox.Controls.SetChildIndex(this.MessageStatusDropEdit, 0);
			this.AuthorityInfoGroupBox.Controls.SetChildIndex(this.VersionTextBox, 0);
			this.AuthorityInfoGroupBox.Controls.SetChildIndex(this.StatusDropEdit, 0);
			// 
			// AttributesUserControl
			// 
			this.AttributesUserControl.AllowDrop = true;
			this.AttributesUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AttributesUserControl, "Attributes");
			this.AttributesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 301, true);
			this.AttributesUserControl.Name = "AttributesUserControl";
			this.AttributesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(953, 440, true);
			this.AttributesUserControl.TabIndex = 0;
			this.AttributesUserControl.TabStop = false;
			// 
			// CusGoodsCatalogUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.LocalPartNumbersGroupBox);
			this.Controls.Add(this.AttributesUserControl);
			this.Name = "CusGoodsCatalogUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1135, 744, true);
			this.Controls.SetChildIndex(this.AttributesUserControl, 0);
			this.Controls.SetChildIndex(this.GoodsCatalogGroupBox, 0);
			this.Controls.SetChildIndex(this.LocalPartNumbersGroupBox, 0);
			this.GoodsCatalogGroupBox.ResumeLayout(false);
			this.GoodsCatalogGroupBox.PerformLayout();
			this.TariffNumFindBox.ResumeLayout(true);
			this.TariffNumFindBox.PerformLayout();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			this.ConsigneeFindBox.ResumeLayout(true);
			this.ConsigneeFindBox.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ComplementaryDescriptionTextBox.ResumeLayout(true);
			this.ComplementaryDescriptionTextBox.PerformLayout();
			this.LocalPartNumbersGroupBox.ResumeLayout(false);
			this.LocalPartNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LocalPartNumbersGrid)).EndInit();
			this.LocalPartNumbersGrid.ResumeLayout(false);
			this.LocalPartNumbersGrid.PerformLayout();
			this.ForeignOperatorsUserControl.ResumeLayout(true);
			this.ForeignOperatorsUserControl.PerformLayout();
			this.AuthorityInfoGroupBox.ResumeLayout(false);
			this.AuthorityInfoGroupBox.PerformLayout();
			this.AttributesUserControl.ResumeLayout(true);
			this.AttributesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Customs.GUI.LongTextControl ComplementaryDescriptionTextBox;
		internal ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		internal ZArchitecture.GUI.ZGroupBox LocalPartNumbersGroupBox;
		internal ZArchitecture.ZGrid LocalPartNumbersGrid;
		internal ForeignOperatorsUserControl ForeignOperatorsUserControl;
		internal ZArchitecture.GUI.ZGroupBox AuthorityInfoGroupBox;
		internal AttributesUserControl AttributesUserControl;
	}
}
