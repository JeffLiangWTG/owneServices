namespace Enterprise.Registry.GUI
{
	partial class CreateMissingProductsRegistryControl
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
			this.optionGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.noRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.yesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.defaultRelationshipDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.defaultRelationshipLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.optionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CreateMissingProductsInfo);
			// 
			// OptionGroupBox
			// 
			this.optionGroupBox.Controls.Add(this.noRadioButton);
			this.optionGroupBox.Controls.Add(this.yesRadioButton);
			this.optionGroupBox.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.optionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.optionGroupBox.Name = "optionGroupBox";
			this.optionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 57, true);
			this.optionGroupBox.TabIndex = 0;
			this.optionGroupBox.TabStop = false;
			// 
			// NoRadioButton
			// 
			this.noRadioButton.AutoCheck = false;
			this.noRadioButton.AutoSize = true;
			this.noRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.noRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 24, true);
			this.noRadioButton.Name = "noRadioButton";
			this.noRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.noRadioButton.TabIndex = 3;
			this.noRadioButton.TabStop = true;
			this.noRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("E8BD9311-E811-4E04-A33F-0812FF229009", "No");
			this.noRadioButton.BackColor = System.Drawing.Color.Transparent;
			// 
			// YesRadioButton
			// 
			this.yesRadioButton.AutoCheck = false;
			this.yesRadioButton.AutoSize = true;
			// The line(s) below are a compile-time check for a binding member.Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CreateMissingProductsInfo)(null)).IsOverrideToYes)));
			this.BindingSource.SetBindingMember(this.yesRadioButton, "IsOverrideToYes");
			this.yesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.yesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 24, true);
			this.yesRadioButton.Name = "yesRadioButton";
			this.yesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.yesRadioButton.TabIndex = 2;
			this.yesRadioButton.TabStop = true;
			this.yesRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("F4FD3153-45E0-4202-ABF0-5B3DA9403780", "Yes");
			this.yesRadioButton.BackColor = System.Drawing.Color.Transparent;
			// 
			// defaultRelationshipLabel
			// 
			this.defaultRelationshipLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.defaultRelationshipLabel.AutoSize = true;
			this.defaultRelationshipLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 91, true);
			this.defaultRelationshipLabel.Name = "defaultRelationshipLabel";
			this.defaultRelationshipLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 13, true);
			this.defaultRelationshipLabel.TabIndex = 4;
			this.defaultRelationshipLabel.Text = "Default Relationship";
			this.defaultRelationshipLabel.UseMnemonic = false;
			// 
			// defaultRelationshipDropEdit
			// 
			this.defaultRelationshipDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.defaultRelationshipDropEdit, "DefaultRelationship");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.CreateMissingProductsInfo)(null)).DefaultRelationship)));
			this.defaultRelationshipDropEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.defaultRelationshipDropEdit, false);
			this.defaultRelationshipDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 91, true);
			this.defaultRelationshipDropEdit.Name = "defaultRelationshipDropEdit";
			this.defaultRelationshipDropEdit.PreBoundMaxLength = 3;
			this.defaultRelationshipDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 20, true);
			this.defaultRelationshipDropEdit.TabIndex = 7;
			// 
			// CreateMissingProductsRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.defaultRelationshipDropEdit);
			this.Controls.Add(this.optionGroupBox);
			this.Controls.Add(this.defaultRelationshipLabel);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 176, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.optionGroupBox.ResumeLayout(false);
			this.optionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected ZArchitecture.GUI.ZDropEdit defaultRelationshipDropEdit;
		CargoWise.Windows.UI.KGroupBox optionGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton noRadioButton;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton yesRadioButton;
		Enterprise.ZArchitecture.ZLabel defaultRelationshipLabel;
	}
}
