namespace Enterprise.PAVE.MENT.GUI
{
	partial class DataCollectionDetailsControl
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
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.isSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.openLinkedEntityZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.linkedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.attributeDescriptionTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.queryDescriptionTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.faultyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.activeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.codeTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("136f6f79-6252-47a9-bc9b-d6675b3af0cc", "Details");
			this.zGroupBox1.Controls.Add(this.isSystemCheckBox);
			this.zGroupBox1.Controls.Add(this.openLinkedEntityZButton);
			this.zGroupBox1.Controls.Add(this.linkedCheckBox);
			this.zGroupBox1.Controls.Add(this.attributeDescriptionTextEdit);
			this.zGroupBox1.Controls.Add(this.queryDescriptionTextEdit);
			this.zGroupBox1.Controls.Add(this.faultyCheckBox);
			this.zGroupBox1.Controls.Add(this.activeCheckBox);
			this.zGroupBox1.Controls.Add(this.codeTextEdit);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 140, true);
			this.zGroupBox1.TabIndex = 3;
			this.zGroupBox1.TabStop = false;
			// 
			// isSystemCheckBox
			// 
			this.isSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isSystemCheckBox, "MAQ_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).MAQ_IsSystem)));
			this.isSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 21, true);
			this.isSystemCheckBox.Name = "isSystemCheckBox";
			this.isSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.isSystemCheckBox.TabIndex = 7;
			this.isSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// openLinkedEntityZButton
			// 
			this.openLinkedEntityZButton.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("ba56c345-97fd-46e7-b49c-8ef0ea874c1e", "Open Linked Entity");
			this.openLinkedEntityZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 17, true);
			this.openLinkedEntityZButton.Name = "openLinkedEntityZButton";
			this.openLinkedEntityZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 23, true);
			this.openLinkedEntityZButton.TabIndex = 6;
			this.openLinkedEntityZButton.UseVisualStyleBackColor = true;
			this.openLinkedEntityZButton.Click += new System.EventHandler(this.openLinkedEntityZButton_Click);
			// 
			// linkedCheckBox
			// 
			this.linkedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.linkedCheckBox, "Linked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Linked)));
			this.linkedCheckBox.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("ea9d8f24-cccc-490d-adf1-c8edb0ec9b45", "Linked");
			this.linkedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.linkedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(423, 19, true);
			this.linkedCheckBox.Name = "linkedCheckBox";
			this.linkedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 17, true);
			this.linkedCheckBox.TabIndex = 5;
			this.linkedCheckBox.UseVisualStyleBackColor = true;
			// 
			// attributeDescriptionTextEdit
			// 
			this.attributeDescriptionTextEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.attributeDescriptionTextEdit, "MAQ_AttributeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).MAQ_AttributeDescription)));
			this.attributeDescriptionTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.attributeDescriptionTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 42, true);
			this.attributeDescriptionTextEdit.Multiline = true;
			this.attributeDescriptionTextEdit.Name = "attributeDescriptionTextEdit";
			this.attributeDescriptionTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 26, true);
			this.attributeDescriptionTextEdit.TabIndex = 2;
			// 
			// queryDescriptionTextEdit
			// 
			this.queryDescriptionTextEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.queryDescriptionTextEdit, "MAQ_QueryDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).MAQ_QueryDescription)));
			this.queryDescriptionTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.queryDescriptionTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 74, true);
			this.queryDescriptionTextEdit.Multiline = true;
			this.queryDescriptionTextEdit.Name = "queryDescriptionTextEdit";
			this.queryDescriptionTextEdit.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.queryDescriptionTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 59, true);
			this.queryDescriptionTextEdit.TabIndex = 4;
			// 
			// faultyCheckBox
			// 
			this.faultyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.faultyCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.faultyCheckBox, "MAQ_IsFaulty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).MAQ_IsFaulty)));
			this.faultyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.faultyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 21, true);
			this.faultyCheckBox.Name = "faultyCheckBox";
			this.faultyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 17, true);
			this.faultyCheckBox.TabIndex = 3;
			this.faultyCheckBox.UseVisualStyleBackColor = true;
			// 
			// activeCheckBox
			// 
			this.activeCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.activeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.activeCheckBox, "MAQ_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).MAQ_IsActive)));
			this.activeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.activeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 21, true);
			this.activeCheckBox.Name = "activeCheckBox";
			this.activeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.activeCheckBox.TabIndex = 4;
			this.activeCheckBox.UseVisualStyleBackColor = true;
			// 
			// codeTextEdit
			// 
			this.BindingSource.SetBindingMember(this.codeTextEdit, "MAQ_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).MAQ_Code)));
			this.codeTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 19, true);
			this.codeTextEdit.Name = "codeTextEdit";
			this.codeTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.codeTextEdit.TabIndex = 0;
			// 
			// DataCollectionDetailsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "DataCollectionDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 140, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZTextBox attributeDescriptionTextEdit;
		private ZArchitecture.ZTextBox queryDescriptionTextEdit;
		private ZArchitecture.GUI.ZCheckBox faultyCheckBox;
		private ZArchitecture.GUI.ZCheckBox activeCheckBox;
		private ZArchitecture.ZTextBox codeTextEdit;
		private ZArchitecture.GUI.ZCheckBox linkedCheckBox;
		private ZArchitecture.GUI.ZButton openLinkedEntityZButton;
		private ZArchitecture.GUI.ZCheckBox isSystemCheckBox;
	}
}
