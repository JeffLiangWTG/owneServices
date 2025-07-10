namespace Enterprise.Services.OperationalActions.GUI
{
	partial class FieldFilterControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBox filterTextBox;
			this.insertFieldButton = new Enterprise.ZArchitecture.GUI.ZButton();
			filterTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptor);
			// 
			// filterTextBox
			// 
			filterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(filterTextBox, "Filter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalActionFieldDescriptor)(null)).Filter)));
			filterTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(filterTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			filterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			filterTextBox.Multiline = true;
			filterTextBox.Name = "filterTextBox";
			filterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 96, true);
			filterTextBox.TabIndex = 1;
			filterTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.filterTextBox_KeyDown);
			// 
			// insertFieldButton
			// 
			this.insertFieldButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.insertFieldButton.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("FieldFilterControl|bf75c30e-e488-45c1-bd58-aa6b1b4ab522", "Fields");
			this.insertFieldButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 120, true);
			this.insertFieldButton.Name = "insertFieldButton";
			this.insertFieldButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.insertFieldButton.TabIndex = 2;
			this.insertFieldButton.UseVisualStyleBackColor = true;
			this.insertFieldButton.Click += new System.EventHandler(this.insertFieldButton_Click);
			// 
			// FieldFilterControl
			// 
			this.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.insertFieldButton);
			this.Controls.Add(filterTextBox);
			this.Name = "FieldFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 143, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZButton insertFieldButton;
	}
}
