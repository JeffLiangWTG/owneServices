using Enterprise.ZArchitecture.GUI;


namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class IncidentTriageChecklistItemForm
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

		protected override void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 402, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 380, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 380, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 380, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 402, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem)(null)).IMC_IsPublished)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem)(null)).PublishedDescriptionText)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem)(null)).IMC_SupportDescription)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem)(null)).IMC_Category)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem)(null)).IMC_ResponseType)));
			// 
			// IncidentTriageChecklistItemForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 458, true);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem);
			this.DataSourceTypeName = "Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 408, true);
			this.Name = "IncidentTriageChecklistItemForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "Checklist Item";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.isPublishedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.publishedDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.supportDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.categoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.responseTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.tableLayoutPanel1 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel3 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel4 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel5 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainTabPage.SuspendLayout();
			this.categoryDropEdit.SuspendLayout();
			this.responseTypeDropEdit.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.zPanel3.SuspendLayout();
			this.zPanel4.SuspendLayout();
			this.zPanel5.SuspendLayout();
			this.MainTabPage.Controls.Add(this.tableLayoutPanel1);
			// 
			// isPublishedCheckBox
			// 
			this.isPublishedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isPublishedCheckBox, "IMC_IsPublished");
			this.isPublishedCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("f4901a7c-24d8-4a68-8402-684265b6ed8f", "Publish to portal");
			this.isPublishedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isPublishedCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.isPublishedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 0, true);
			this.isPublishedCheckBox.Name = "isPublishedCheckBox";
			this.isPublishedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.isPublishedCheckBox.TabIndex = 2;
			this.isPublishedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.isPublishedCheckBox.UseVisualStyleBackColor = false;
			// 
			// publishedDescriptionTextBox
			// 
			this.publishedDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.publishedDescriptionTextBox, "PublishedDescriptionText");
			this.publishedDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.publishedDescriptionTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
			this.publishedDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 2, true);
			this.publishedDescriptionTextBox.Multiline = true;
			this.publishedDescriptionTextBox.Name = "publishedDescriptionTextBox";
			this.publishedDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.publishedDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 150, true);
			this.publishedDescriptionTextBox.TabIndex = 8;
			// 
			// supportDescriptionTextBox
			// 
			this.supportDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.supportDescriptionTextBox, "IMC_SupportDescription");
			this.supportDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.supportDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 2, true);
			this.supportDescriptionTextBox.Multiline = true;
			this.supportDescriptionTextBox.Name = "supportDescriptionTextBox";
			this.supportDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.supportDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 150, true);
			this.supportDescriptionTextBox.TabIndex = 1;
			// 
			// categoryDropEdit
			// 
			this.categoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.categoryDropEdit, "IMC_Category");
			this.categoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 0, true);
			this.categoryDropEdit.Name = "categoryDropEdit";
			this.categoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 17, true);
			this.categoryDropEdit.TabIndex = 4;
			// 
			// responseTypeDropEdit
			// 
			this.responseTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.responseTypeDropEdit, "IMC_ResponseType");
			this.responseTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 2, true);
			this.responseTypeDropEdit.Name = "responseTypeDropEdit";
			this.responseTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 17, true);
			this.responseTypeDropEdit.TabIndex = 6;
			this.responseTypeDropEdit.TabStop = false;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.zPanel1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.zPanel2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.zPanel3, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.zPanel4, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.zPanel5, 0, 4);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1039, 400);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// zPanel1
			// 
			this.zPanel1.AutoSize = true;
			this.zPanel1.Controls.Add(this.categoryDropEdit);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 19, true);
			this.zPanel1.TabIndex = 9;
			// 
			// zPanel2
			// 
			this.zPanel2.AutoSize = true;
			this.zPanel2.Controls.Add(this.responseTypeDropEdit);
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 25, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 21, true);
			this.zPanel2.TabIndex = 10;
			// 
			// zPanel3
			// 
			this.zPanel3.Controls.Add(this.supportDescriptionTextBox);
			this.zPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 51, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 95, true);
			this.zPanel3.TabIndex = 0;
			// 
			// zPanel4
			// 
			this.zPanel4.AutoSize = true;
			this.zPanel4.Controls.Add(this.isPublishedCheckBox);
			this.zPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 149, true);
			this.zPanel4.Name = "zPanel4";
			this.zPanel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 16, true);
			this.zPanel4.TabIndex = 0;
			// 
			// zPanel5
			// 
			this.zPanel5.Controls.Add(this.publishedDescriptionTextBox);
			this.zPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 169, true);
			this.zPanel5.Name = "zPanel5";
			this.zPanel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 95, true);
			this.zPanel5.TabIndex = 11;
			this.MainTabPage.PerformLayout();
			this.categoryDropEdit.ResumeLayout(true);
			this.categoryDropEdit.PerformLayout();
			this.responseTypeDropEdit.ResumeLayout(true);
			this.responseTypeDropEdit.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			this.zPanel4.ResumeLayout(false);
			this.zPanel4.PerformLayout();
			this.zPanel5.ResumeLayout(false);
			this.zPanel5.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);
		}

		#endregion

		private ZCheckBox isPublishedCheckBox;
		private ZArchitecture.ZTextBox publishedDescriptionTextBox;
		private ZArchitecture.ZTextBox supportDescriptionTextBox;
		private ZDropEdit categoryDropEdit;
		private ZDropEdit responseTypeDropEdit;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private ZArchitecture.ZLabel responseTypeLabel;
		private ZArchitecture.ZLabel categoryLabel;
		private ZPanel zPanel1;
		private ZPanel zPanel3;
		private ZPanel zPanel4;
		private ZPanel zPanel2;
		private ZPanel zPanel5;
	}
}
