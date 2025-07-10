namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class MultipleEmailTemplatesControl
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
			this.notificationEmailTemplateRegistryControl1 = new Enterprise.Registry.GUI.NotificationEmailTemplateRegistryControl();
			this.CategoryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CategoryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CategoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CategoryGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.CodeDescriptionEmailTemplateCollection);
			// 
			// notificationEmailTemplateRegistryControl1
			// 
			this.notificationEmailTemplateRegistryControl1.AllowDrop = true;
			this.notificationEmailTemplateRegistryControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.notificationEmailTemplateRegistryControl1, "EmailTemplate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Registry.Business.NotificationEmailTemplate)(((Enterprise.Client.EDI.Registry.Business.CodeDescriptionEmailTemplate)(null)).EmailTemplate)));
			this.notificationEmailTemplateRegistryControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 179, true);
			this.notificationEmailTemplateRegistryControl1.Name = "notificationEmailTemplateRegistryControl1";
			this.notificationEmailTemplateRegistryControl1.ReadOnly = false;
			this.notificationEmailTemplateRegistryControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 310, true);
			this.notificationEmailTemplateRegistryControl1.TabIndex = 0;
			// 
			// CategoryGroupBox
			// 
			this.CategoryGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CategoryGroupBox.Controls.Add(this.CategoryGrid);
			this.CategoryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.CategoryGroupBox.Name = "CategoryGroupBox";
			this.CategoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 169, true);
			this.CategoryGroupBox.TabIndex = 1;
			this.CategoryGroupBox.TabStop = false;
			this.CategoryGroupBox.Text = "Category";
			// 
			// CategoryGrid
			// 
			this.CategoryGrid.AllowNavigation = false;
			this.CategoryGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CategoryGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.CodeDescriptionEmailTemplate)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.CodeDescriptionEmailTemplate)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.CodeDescriptionEmailTemplate)(null)).EnglishDescription)));
			this.CategoryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Code";
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo2.Caption = "Template Description";
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.CategoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CategoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CategoryGrid.GridId = "579951ad-3e92-4e6a-9615-3e431472d49d";
			this.CategoryGrid.CopySelectedRowsAllowed = true;
			this.CategoryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CategoryGrid.LayoutKey = "zGrid1";
			this.CategoryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 20, true);
			this.CategoryGrid.Name = "CategoryGrid";
			this.CategoryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 120, true);
			this.CategoryGrid.TabIndex = 0;
			// 
			// MultipleEmailTemplatesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CategoryGroupBox);
			this.Controls.Add(this.notificationEmailTemplateRegistryControl1);
			this.Name = "MultipleEmailTemplatesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 498, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CategoryGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CategoryGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.Registry.GUI.NotificationEmailTemplateRegistryControl notificationEmailTemplateRegistryControl1;
		private ZArchitecture.GUI.ZGroupBox CategoryGroupBox;
		internal ZArchitecture.ZGrid CategoryGrid;
	}
}
