namespace Enterprise.DocumentEngine.GUI
{
	partial class MacroEvaluatorForm
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
			this.DataContextZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MacroZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OutputZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EvaluateZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CopyToClipboardZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DataContextZPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DataContextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MainZPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DataContextZDropEdit.SuspendLayout();
			this.DataContextZPanel.SuspendLayout();
			this.MainZPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 312, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.MacroEvaluator.EvaluatorManager);
			// 
			// DataContextZDropEdit
			// 
			this.DataContextZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DataContextZDropEdit, "DataContextCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.MacroEvaluator.MacroEvaluatorManager)(null)).DataContextCode)));
			this.DataContextZDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MacroEvaluatorForm|4399d4eb-6c96-4abc-9731-e1405f9833a3", "Data Context");
			this.DataContextZDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DataContextZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 6, true);
			this.DataContextZDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 0, true);
			this.DataContextZDropEdit.Name = "DataContextZDropEdit";
			this.DataContextZDropEdit.ShowDescriptionBox = false;
			this.DataContextZDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.DataContextZDropEdit.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.DataContextZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.DataContextZDropEdit.TabIndex = 1;
			// 
			// MacroZTextBox
			// 
			this.MacroZTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MacroZTextBox, "Macro");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.MacroEvaluator.EvaluatorManager)(null)).Macro)));
			this.MacroZTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MacroEvaluatorForm|9de9c0d0-730c-4f6a-b4f7-f53aefe8b0b9", "Macro");
			this.MacroZTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MacroZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 3, true);
			this.MacroZTextBox.Multiline = true;
			this.MacroZTextBox.Name = "MacroZTextBox";
			this.MacroZTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MacroZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(605, 84, true);
			this.MacroZTextBox.TabIndex = 2;
			// 
			// OutputZTextBox
			// 
			this.OutputZTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OutputZTextBox, "Output");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.MacroEvaluator.EvaluatorManager)(null)).Output)));
			this.OutputZTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MacroEvaluatorForm|853cfadb-b4eb-43e3-93d4-a9852674f82c", "Output");
			this.OutputZTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OutputZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 93, true);
			this.OutputZTextBox.Multiline = true;
			this.OutputZTextBox.Name = "OutputZTextBox";
			this.OutputZTextBox.ReadOnly = true;
			this.OutputZTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OutputZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(605, 152, true);
			this.OutputZTextBox.TabIndex = 3;
			// 
			// EvaluateZButton
			// 
			this.EvaluateZButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.EvaluateZButton.AutoSize = true;
			this.EvaluateZButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MacroEvaluatorForm|9a78455f-371a-459a-a016-b7f13d248345", "&Evaluate");
			this.EvaluateZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(423, 251, true);
			this.EvaluateZButton.Name = "EvaluateZButton";
			this.EvaluateZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.EvaluateZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.EvaluateZButton.TabIndex = 4;
			this.EvaluateZButton.ToolTipCaption = null;
			this.EvaluateZButton.UseVisualStyleBackColor = true;
			this.EvaluateZButton.Click += new System.EventHandler(this.EvaluateZButton_Click);
			// 
			// CopyToClipboardZButton
			// 
			this.CopyToClipboardZButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyToClipboardZButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MacroEvaluatorForm|433c7e3f-751b-41ec-bbdf-7370a3cca9f0", "Copy To Clipboard");
			this.CopyToClipboardZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 251, true);
			this.CopyToClipboardZButton.Name = "CopyToClipboardZButton";
			this.CopyToClipboardZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CopyToClipboardZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 23, true);
			this.CopyToClipboardZButton.TabIndex = 5;
			this.CopyToClipboardZButton.ToolTipCaption = null;
			this.CopyToClipboardZButton.UseVisualStyleBackColor = true;
			this.CopyToClipboardZButton.Click += new System.EventHandler(this.CopyToClipboard);
			// 
			// CloseZButton
			// 
			this.CloseZButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseZButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MacroEvaluatorForm8b47222b-f464-4a16-915c-1dc7f6737926", "&Close");
			this.CloseZButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(631, 251, true);
			this.CloseZButton.Name = "CloseZButton";
			this.CloseZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseZButton.TabIndex = 6;
			this.CloseZButton.ToolTipCaption = null;
			this.CloseZButton.UseVisualStyleBackColor = true;
			this.CloseZButton.Click += new System.EventHandler(this.CloseZButton_Click);
			// 
			// DataContextZPanel
			// 
			this.DataContextZPanel.Controls.Add(this.DataContextZDropEdit);
			this.DataContextZPanel.Controls.Add(this.DataContextLabel);
			this.DataContextZPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DataContextZPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DataContextZPanel.Name = "DataContextZPanel";
			this.DataContextZPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 32, true);
			this.DataContextZPanel.TabIndex = 7;
			// 
			// DataContextLabel
			// 
			this.DataContextLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MacroEvaluatorForm|45EC05FF-C048-455A-A937-DD4A7EA6BDB3", "The menu filter only uses current job as data context");
			this.DataContextLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DataContextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 9, true);
			this.DataContextLabel.Name = "DataContextLabel";
			this.DataContextLabel.AutoSize = true;
			this.DataContextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 19, true);
			this.DataContextLabel.TabIndex = 8;
			// 
			// MainZPanel
			// 
			this.MainZPanel.Controls.Add(this.MacroZTextBox);
			this.MainZPanel.Controls.Add(this.OutputZTextBox);
			this.MainZPanel.Controls.Add(this.CloseZButton);
			this.MainZPanel.Controls.Add(this.EvaluateZButton);
			this.MainZPanel.Controls.Add(this.CopyToClipboardZButton);
			this.MainZPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainZPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.MainZPanel.Name = "MainZPanel";
			this.MainZPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 280, true);
			this.MainZPanel.TabIndex = 2;
			// 
			// MacroEvaluatorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseZButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MacroEvaluatorForm|94da2187-8646-4d89-8347-67eb0df64383", "Macro Evaluator");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 336, true);
			this.Controls.Add(this.MainZPanel);
			this.Controls.Add(this.DataContextZPanel);
			this.DataSourceType = typeof(Enterprise.DocumentEngine.MacroEvaluator.EvaluatorManager);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 279, true);
			this.Name = "MacroEvaluatorForm";
			this.Text = "Macro Evaluator";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DataContextZPanel, 0);
			this.Controls.SetChildIndex(this.MainZPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DataContextZDropEdit.ResumeLayout(true);
			this.DataContextZDropEdit.PerformLayout();
			this.DataContextZPanel.ResumeLayout(false);
			this.DataContextZPanel.PerformLayout();
			this.MainZPanel.ResumeLayout(false);
			this.MainZPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit DataContextZDropEdit;
		private ZArchitecture.ZTextBox MacroZTextBox;
		private ZArchitecture.ZTextBox OutputZTextBox;
		private ZArchitecture.GUI.ZButton EvaluateZButton;
		private ZArchitecture.GUI.ZButton CopyToClipboardZButton;
		private ZArchitecture.GUI.ZButton CloseZButton;
		private ZArchitecture.GUI.ZPanel DataContextZPanel;
		private ZArchitecture.GUI.ZPanel MainZPanel;
		private ZArchitecture.ZLabel DataContextLabel;
	}
}