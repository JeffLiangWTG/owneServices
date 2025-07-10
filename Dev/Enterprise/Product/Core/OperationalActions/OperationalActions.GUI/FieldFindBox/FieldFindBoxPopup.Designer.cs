namespace Enterprise.Services.OperationalActions.GUI
{
	partial class FieldFindBoxPopup
	{
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
			Enterprise.ZArchitecture.GUI.ZButton cancelButton;
			Enterprise.ZArchitecture.GUI.ZButton okButton;
			CargoWise.Windows.UI.KSplitContainer treeDescriptionSplitPanel;
			Enterprise.ZArchitecture.ZTextBox descriptionTextBox;
			Enterprise.ZArchitecture.GUI.ZPanel spacerPanel;
			Enterprise.ZArchitecture.ZTextBox captionTextBox;
			Enterprise.ZArchitecture.ZTextBox fieldTextBox;
			Enterprise.ZArchitecture.GUI.ZPanel topPanel;
			Enterprise.ZArchitecture.GUI.ZPanel midPanel;
			this.fieldTree = new Enterprise.Services.OperationalActions.GUI.FieldTreeView();
			bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			treeDescriptionSplitPanel = new CargoWise.Windows.UI.KSplitContainer();
			descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			spacerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			captionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			fieldTextBox = new Enterprise.ZArchitecture.ZTextBox();
			topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			midPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			bottomPanel.SuspendLayout();
			treeDescriptionSplitPanel.Panel1.SuspendLayout();
			treeDescriptionSplitPanel.Panel2.SuspendLayout();
			treeDescriptionSplitPanel.SuspendLayout();
			topPanel.SuspendLayout();
			midPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 430, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.FieldModule);
			// 
			// bottomPanel
			// 
			bottomPanel.Controls.Add(cancelButton);
			bottomPanel.Controls.Add(okButton);
			bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 394, true);
			bottomPanel.Name = "bottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 36, true);
			bottomPanel.TabIndex = 2;
			// 
			// cancelButton
			// 
			cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			cancelButton.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("FieldFindBoxPopup|7357e237-a304-4080-9898-089cd2f4fc8b", "Cancel");
			cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 8, true);
			cancelButton.Name = "cancelButton";
			cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			cancelButton.TabIndex = 1;
			cancelButton.UseVisualStyleBackColor = true;
			cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			okButton.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("FieldFindBoxPopup|516abc50-5d90-4548-939a-973bae74a98a", "OK");
			okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 8, true);
			okButton.Name = "okButton";
			okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			okButton.TabIndex = 0;
			okButton.UseVisualStyleBackColor = true;
			okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// treeDescriptionSplitPanel
			// 
			treeDescriptionSplitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			treeDescriptionSplitPanel.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			treeDescriptionSplitPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			treeDescriptionSplitPanel.Name = "treeDescriptionSplitPanel";
			treeDescriptionSplitPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// treeDescriptionSplitPanel.Panel1
			// 
			treeDescriptionSplitPanel.Panel1.Controls.Add(this.fieldTree);
			// 
			// treeDescriptionSplitPanel.Panel2
			// 
			treeDescriptionSplitPanel.Panel2.Controls.Add(descriptionTextBox);
			treeDescriptionSplitPanel.Panel2.Controls.Add(spacerPanel);
			treeDescriptionSplitPanel.Panel2.Controls.Add(captionTextBox);
			treeDescriptionSplitPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 354, true);
			treeDescriptionSplitPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(246);
			treeDescriptionSplitPanel.TabIndex = 1;
			// 
			// fieldTree
			// 
			this.fieldTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fieldTree.HideSelection = false;
			this.fieldTree.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.fieldTree.Name = "fieldTree";
			this.fieldTree.PathSeparator = ".";
			this.fieldTree.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 246, true);
			this.fieldTree.TabIndex = 0;
			this.fieldTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.fieldTree_BeforeExpand);
			this.fieldTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.fieldTree_AfterSelect);
			// 
			// descriptionTextBox
			// 
			this.BindingSource.SetBindingMember(descriptionTextBox, "SelectedDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.FieldModule)(null)).SelectedDescription)));
			descriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			descriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			descriptionTextBox.Multiline = true;
			descriptionTextBox.Name = "descriptionTextBox";
			descriptionTextBox.ReadOnly = true;
			descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 81, true);
			descriptionTextBox.TabIndex = 3;
			descriptionTextBox.TabStop = false;
			// 
			// spacerPanel
			// 
			spacerPanel.Dock = System.Windows.Forms.DockStyle.Top;
			spacerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			spacerPanel.Name = "spacerPanel";
			spacerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 3, true);
			spacerPanel.TabIndex = 0;
			// 
			// captionTextBox
			// 
			this.BindingSource.SetBindingMember(captionTextBox, "SelectedCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.FieldModule)(null)).SelectedCaption)));
			captionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			captionTextBox.Dock = System.Windows.Forms.DockStyle.Top;
			captionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			captionTextBox.Name = "captionTextBox";
			captionTextBox.ReadOnly = true;
			captionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 20, true);
			captionTextBox.TabIndex = 2;
			captionTextBox.TabStop = false;
			// 
			// fieldTextBox
			// 
			fieldTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(fieldTextBox, "SelectedFieldName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.FieldModule)(null)).SelectedFieldName)));
			fieldTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			fieldTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 8, true);
			fieldTextBox.Name = "fieldTextBox";
			fieldTextBox.ReadOnly = true;
			fieldTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 20, true);
			fieldTextBox.TabIndex = 1;
			fieldTextBox.TabStop = false;
			// 
			// topPanel
			// 
			topPanel.Controls.Add(fieldTextBox);
			topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			topPanel.Name = "topPanel";
			topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 40, true);
			topPanel.TabIndex = 0;
			// 
			// midPanel
			// 
			midPanel.Controls.Add(treeDescriptionSplitPanel);
			midPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			midPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			midPanel.Name = "midPanel";
			midPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0, true);
			midPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 354, true);
			midPanel.TabIndex = 1;
			// 
			// FieldFindBoxPopup
			// 
			this.AcceptButton = okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = cancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 454, true);
			this.Controls.Add(midPanel);
			this.Controls.Add(bottomPanel);
			this.Controls.Add(topPanel);
			this.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.FieldModule);
			this.Name = "FieldFindBoxPopup";
			this.Text = "FieldFindBoxPopup";
			this.Controls.SetChildIndex(topPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(bottomPanel, 0);
			this.Controls.SetChildIndex(midPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			bottomPanel.ResumeLayout(false);
			treeDescriptionSplitPanel.Panel1.ResumeLayout(false);
			treeDescriptionSplitPanel.Panel2.ResumeLayout(false);
			treeDescriptionSplitPanel.Panel2.PerformLayout();
			treeDescriptionSplitPanel.ResumeLayout(false);
			topPanel.ResumeLayout(false);
			topPanel.PerformLayout();
			midPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		private Enterprise.Services.OperationalActions.GUI.FieldTreeView fieldTree;
	}
}
