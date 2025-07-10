namespace Enterprise.DocumentEngine.GUI.DocBuilder
{
	partial class CustomizeSectionForm
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
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.languagesDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.categoriesDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.sectionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.copyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.sectionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sectionsGrid)).BeginInit();
			this.sectionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 438, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.DocBuilder.SectionEditing.CustomizeSectionManager);
			// 
			// languagesDropEdit
			// 
			this.languagesDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.languagesDropEdit, "Language");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.DocBuilder.SectionEditing.CustomizeSectionManager)(null)).Language)));
			this.languagesDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("CustomizeSectionForm|1453d301-7716-4f89-98f8-da233cf00724", "Selected Language Template");
			this.languagesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 365, true);
			this.languagesDropEdit.Name = "languagesDropEdit";
			this.languagesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.languagesDropEdit.TabIndex = 2;
			// 
			// categoriesDropEdit
			// 
			this.categoriesDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.categoriesDropEdit, "CategoryFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.DocBuilder.SectionEditing.CustomizeSectionManager)(null)).CategoryFilter)));
			this.categoriesDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.categoriesDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("CustomizeSectionForm|8ba00494-3858-4f12-8029-73839b25fcaa", "Filter By Category");
			this.categoriesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 19, true);
			this.categoriesDropEdit.Name = "categoriesDropEdit";
			this.categoriesDropEdit.PreBoundMaxLength = 15;
			this.categoriesDropEdit.ShowDescriptionBox = false;
			this.categoriesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.categoriesDropEdit.TabIndex = 0;
			// 
			// sectionsGrid
			// 
			this.sectionsGrid.AllowNavigation = false;
			this.sectionsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.sectionsGrid, "Sections");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.DocBuilder.SectionEditing.CustomizeSectionManager)(null)).Sections)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DocBuilder.TemplateSection)(((System.Collections.IList)(((Enterprise.DocumentEngine.DocBuilder.SectionEditing.CustomizeSectionManager)(null)).Sections)).SyncRoot)).TypeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DocBuilder.TemplateSection)(((System.Collections.IList)(((Enterprise.DocumentEngine.DocBuilder.SectionEditing.CustomizeSectionManager)(null)).Sections)).SyncRoot)).SectionName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DocBuilder.TemplateSection)(((System.Collections.IList)(((Enterprise.DocumentEngine.DocBuilder.SectionEditing.CustomizeSectionManager)(null)).Sections)).SyncRoot)).Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DocBuilder.TemplateSection)(((System.Collections.IList)(((Enterprise.DocumentEngine.DocBuilder.SectionEditing.CustomizeSectionManager)(null)).Sections)).SyncRoot)).Category)));
			this.sectionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("CustomizeSectionForm|92a4f283-2e66-4a7a-a440-9d68043dd637", "Type", "Type", "Type", "");
			zTextBoxColumnStyleInfo1.ColumnName = "TypeCode";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("CustomizeSectionForm|025d5078-5e3e-491a-bd34-3a7059bac6ce", "Name", "Name", "Name", "");
			zTextBoxColumnStyleInfo2.ColumnName = "SectionName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("CustomizeSectionForm|77ce9ed6-4235-40cf-9def-74b7bfbd4143", "Language", "Language", "Language", "");
			zTextBoxColumnStyleInfo3.ColumnName = "Language";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("CustomizeSectionForm|f812c836-aee5-439d-a7a4-9f4c4414d91f", "Category");
			zTextBoxColumnStyleInfo4.ColumnName = "Category";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.sectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.sectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.sectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.sectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.sectionsGrid.GridId = "62fbae0a-366c-4d96-9c18-98c8ed771111";
			this.sectionsGrid.CopySelectedRowsAllowed = true;
			this.sectionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.sectionsGrid.LayoutKey = "zGrid1";
			this.sectionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 45, true);
			this.sectionsGrid.Name = "sectionsGrid";
			this.sectionsGrid.ReadOnly = true;
			this.sectionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 314, true);
			this.sectionsGrid.TabIndex = 1;
			this.sectionsGrid.DoubleClick += new System.EventHandler(this.HandleCopySection);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("CustomizeSectionForm|f56ed6f7-7e3e-4677-bb8e-3ff3615e4d46", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(397, 409, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// copyButton
			// 
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.copyButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("CustomizeSectionForm|87145614-cfac-4505-8e90-1013ef50808a", "Copy");
			this.copyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 409, true);
			this.copyButton.Name = "copyButton";
			this.copyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.copyButton.TabIndex = 1;
			this.copyButton.UseVisualStyleBackColor = true;
			this.copyButton.Click += new System.EventHandler(this.HandleCopySection);
			// 
			// sectionsGroupBox
			// 
			this.sectionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.sectionsGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("CustomizeSectionForm|1e679e5f-eabc-450d-849a-541570902663", "Sections");
			this.sectionsGroupBox.Controls.Add(this.languagesDropEdit);
			this.sectionsGroupBox.Controls.Add(this.categoriesDropEdit);
			this.sectionsGroupBox.Controls.Add(this.sectionsGrid);
			this.sectionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.sectionsGroupBox.Name = "sectionsGroupBox";
			this.sectionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 391, true);
			this.sectionsGroupBox.TabIndex = 0;
			this.sectionsGroupBox.TabStop = false;
			// 
			// CustomizeSectionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 462, true);
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("CustomizeSectionForm|362a4776-8e1b-487b-9ef9-452d8454dcb4", "Copy Section For Customization");
			this.Controls.Add(this.sectionsGroupBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.copyButton);
			this.DataSourceType = typeof(Enterprise.DocumentEngine.DocBuilder.SectionEditing.CustomizeSectionManager);
			this.IsPostOnly = true;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 500, true);
			this.Name = "CustomizeSectionForm";
			this.Text = "Copy Section For Customization";
			this.Controls.SetChildIndex(this.copyButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.sectionsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sectionsGrid)).EndInit();
			this.sectionsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZDropEdit categoriesDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit languagesDropEdit;
		Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		Enterprise.ZArchitecture.GUI.ZGroupBox sectionsGroupBox;
#if DEBUG
		internal
#endif
 Enterprise.ZArchitecture.GUI.ZButton copyButton;
#if DEBUG
		internal
#endif
 Enterprise.ZArchitecture.ZGrid sectionsGrid;
	}
}
