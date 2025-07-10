namespace Enterprise.Services.OperationalActions.GUI
{
	partial class FieldFindBoxGridControl
	{
		void InitializeComponent()
		{
			this.fieldNameTextBox = new Enterprise.ZArchitecture.ZTextBox.Bare();
			this.lookupButton = new FieldFindBoxButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// fieldNameTextBox
			// 
			this.fieldNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.fieldNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.fieldNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.fieldNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.fieldNameTextBox.Name = "fieldNameTextBox";
			this.fieldNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 13, true);
			this.fieldNameTextBox.TabIndex = 0;
			this.fieldNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CheckForSpecialKeys);
			this.fieldNameTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.FieldNameTextBox_Validating);
			this.fieldNameTextBox.Enter += new System.EventHandler(this.FieldNameTextBox_Enter);
			this.fieldNameTextBox.TextChanged += new System.EventHandler(this.FieldNameTextBox_TextChanged);
			// 
			// lookupButton
			// 
			this.lookupButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.lookupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 0, true);
			this.lookupButton.Name = "lookupButton";
			this.lookupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 20, true);
			this.lookupButton.TabIndex = 1;
			this.lookupButton.TabStop = false;
			this.lookupButton.Click += new System.EventHandler(this.LookupButton_Click);
			this.lookupButton.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CheckForSpecialKeys);
			// 
			// FieldFindBoxGridControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.fieldNameTextBox);
			this.Controls.Add(this.lookupButton);
			this.Name = "FieldFindBoxGridControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private FieldFindBoxButton lookupButton;
		private Enterprise.ZArchitecture.ZTextBox.Bare fieldNameTextBox;
	}
}
