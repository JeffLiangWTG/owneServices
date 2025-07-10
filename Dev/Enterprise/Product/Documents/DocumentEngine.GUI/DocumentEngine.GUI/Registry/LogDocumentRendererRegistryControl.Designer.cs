namespace Enterprise.DocumentEngine.GUI.Registry
{
	partial class LogDocumentRendererRegistryControl
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
			this.generateCallStacksCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.logFilePathTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.clearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.chooseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.LogDocumentRenderer.LogDocumentRendererRegistry);
			// 
			// generateCallStacksCheckBox
			// 
			this.generateCallStacksCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.generateCallStacksCheckBox, "GenerateCallStacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngineCore.Registry.LogDocumentRenderer.LogDocumentRendererRegistry)(null)).GenerateCallStacks)));
			this.generateCallStacksCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("LogDocumentRendererRegistryControl|95a6c669-a468-4137-88bb-01fb5e01e686", "Generate Call Stacks");
			this.generateCallStacksCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.generateCallStacksCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 82, true);
			this.generateCallStacksCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 10, 3, 3, true);
			this.generateCallStacksCheckBox.Name = "generateCallStacksCheckBox";
			this.generateCallStacksCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 24, true);
			this.generateCallStacksCheckBox.TabIndex = 3;
			this.generateCallStacksCheckBox.UseVisualStyleBackColor = true;
			// 
			// logFilePathTextBox
			// 
			this.logFilePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.logFilePathTextBox, "LogFilePath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.LogDocumentRenderer.LogDocumentRendererRegistry)(null)).LogFilePath)));
			this.logFilePathTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("LogDocumentRendererRegistryControl|750068ec-d1f3-4922-9f10-eceddbd8f28c", "Log File Path");
			this.logFilePathTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.logFilePathTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.logFilePathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 20, true);
			this.logFilePathTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.logFilePathTextBox.Name = "logFilePathTextBox";
			this.logFilePathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 20, true);
			this.logFilePathTextBox.TabIndex = 0;
			// 
			// clearButton
			// 
			this.clearButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("LogDocumentRendererRegistryControl|6c091f17-be1f-4b33-906c-9d3b9cc3d5df", "Clear");
			this.clearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 46, true);
			this.clearButton.Name = "clearButton";
			this.clearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.clearButton.TabIndex = 2;
			this.clearButton.UseVisualStyleBackColor = true;
			this.clearButton.Click += new System.EventHandler(this.HandleClearButtonClick);
			// 
			// chooseButton
			// 
			this.chooseButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("LogDocumentRendererRegistryControl|6392a7a7-f452-4273-9b9a-13671ee0795e", "Choose...");
			this.chooseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 46, true);
			this.chooseButton.Name = "chooseButton";
			this.chooseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.chooseButton.TabIndex = 1;
			this.chooseButton.UseVisualStyleBackColor = true;
			this.chooseButton.Click += new System.EventHandler(this.HandleChooseButtonClick);
			// 
			// LogDocumentRendererRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.clearButton);
			this.Controls.Add(this.chooseButton);
			this.Controls.Add(this.generateCallStacksCheckBox);
			this.Controls.Add(this.logFilePathTextBox);
			this.Name = "LogDocumentRendererRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 365, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox logFilePathTextBox;
		private ZArchitecture.GUI.ZCheckBox generateCallStacksCheckBox;
		private ZArchitecture.GUI.ZButton clearButton;
		private ZArchitecture.GUI.ZButton chooseButton;
	}
}
