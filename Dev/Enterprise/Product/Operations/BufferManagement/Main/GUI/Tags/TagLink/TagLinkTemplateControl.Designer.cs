namespace Enterprise.BufferManagement.GUI
{
	partial class TagLinkTemplateControl
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
			this.magnitudeDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.definitionDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.tagTemplateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tagTemplateGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.TagLinkTemplate);
			// 
			// magnitudeDropEdit
			// 
			this.magnitudeDropEdit.AllowDrop = true;
			this.magnitudeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.magnitudeDropEdit, "TGL_TGM_Magnitude");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.TagLinkTemplate)(null)).TGL_TGM_Magnitude)));
			this.magnitudeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 48, true);
			this.magnitudeDropEdit.Name = "magnitudeDropEdit";
			this.magnitudeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.magnitudeDropEdit.TabIndex = 1;
			// 
			// definitionDropEdit
			// 
			this.definitionDropEdit.AllowDrop = true;
			this.definitionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.definitionDropEdit, "TagDefinitionPk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.TagLinkTemplate)(null)).TagDefinitionPk)));
			this.definitionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 22, true);
			this.definitionDropEdit.Name = "definitionDropEdit";
			this.definitionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.definitionDropEdit.TabIndex = 0;
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "TGL_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.TagLinkTemplate)(null)).TGL_Description)));
			this.descriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 100, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.descriptionTextBox.TabIndex = 3;
			// 
			// tagTemplateGroupBox
			// 
			this.tagTemplateGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("29c022a8-aa77-4400-8ed4-532e394d7375", "Tag Template");
			this.tagTemplateGroupBox.Controls.Add(this.zCalcEdit1);
			this.tagTemplateGroupBox.Controls.Add(this.magnitudeDropEdit);
			this.tagTemplateGroupBox.Controls.Add(this.descriptionTextBox);
			this.tagTemplateGroupBox.Controls.Add(this.definitionDropEdit);
			this.tagTemplateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tagTemplateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tagTemplateGroupBox.Name = "tagTemplateGroupBox";
			this.tagTemplateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 133, true);
			this.tagTemplateGroupBox.TabIndex = 4;
			this.tagTemplateGroupBox.TabStop = false;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "TGL_Magnitude");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.TagLinkTemplate)(null)).TGL_Magnitude)));
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 74, true);
			this.zCalcEdit1.MaxValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zCalcEdit1.TabIndex = 2;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TagLinkTemplateControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.tagTemplateGroupBox);
			this.Name = "TagLinkTemplateControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 133, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tagTemplateGroupBox.ResumeLayout(false);
			this.tagTemplateGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZTextBox descriptionTextBox;
		private ZArchitecture.GUI.ZGroupBox tagTemplateGroupBox;
		private ZArchitecture.ZCalcEdit zCalcEdit1;
		private ZArchitecture.GUI.ZGuidDropEdit magnitudeDropEdit;
		private ZArchitecture.GUI.ZGuidDropEdit definitionDropEdit;


	}
}
