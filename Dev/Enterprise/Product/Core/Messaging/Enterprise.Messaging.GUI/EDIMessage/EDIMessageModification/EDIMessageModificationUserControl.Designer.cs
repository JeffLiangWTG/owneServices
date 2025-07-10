namespace Enterprise.Messaging.GUI
{
	partial class EDIMessageModificationUserControl
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
            this.MessageSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
            this.RawMessageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.RawMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.MessageInterpretationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.MessageInterpretationTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.MessageSplitContainer.Panel1.SuspendLayout();
            this.MessageSplitContainer.Panel2.SuspendLayout();
            this.MessageSplitContainer.SuspendLayout();
            this.RawMessageGroupBox.SuspendLayout();
            this.MessageInterpretationGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Messaging.Business.EDIMessage);
            // 
            // MessageSplitContainer
            // 
            this.MessageSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MessageSplitContainer.Name = "MessageSplitContainer";
            // 
            // MessageSplitContainer.Panel1
            // 
            this.MessageSplitContainer.Panel1.Controls.Add(this.RawMessageGroupBox);
            this.MessageSplitContainer.Panel1MinSize = 200;
            // 
            // MessageSplitContainer.Panel2
            // 
            this.MessageSplitContainer.Panel2.Controls.Add(this.MessageInterpretationGroupBox);
            this.MessageSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 254, true);
            this.MessageSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(254);
            this.MessageSplitContainer.TabIndex = 0;
            this.MessageSplitContainer.TabStop = false;
            // 
            // RawMessageGroupBox
            // 
			this.RawMessageGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageModificationUserControl|19ff1b7f-3540-442d-95a6-0ea3b81b9ef2", "Raw Message");
            this.RawMessageGroupBox.Controls.Add(this.RawMessageTextBox);
            this.RawMessageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RawMessageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.RawMessageGroupBox.Name = "RawMessageGroupBox";
            this.RawMessageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 254, true);
            this.RawMessageGroupBox.TabIndex = 0;
            this.RawMessageGroupBox.TabStop = false;
            // 
            // RawMessageTextBox
            // 
            this.BindingSource.SetBindingMember(this.RawMessageTextBox, "EM_MessageText");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageText)));
            this.RawMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.RawMessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RawMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.RawMessageTextBox.Multiline = true;
            this.RawMessageTextBox.Name = "RawMessageTextBox";
            this.RawMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.RawMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 235, true);
            this.RawMessageTextBox.TabIndex = 0;
            // 
            // MessageInterpretationGroupBox
            // 
			this.MessageInterpretationGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageModificationUserControl|0af1f779-2aa7-4252-b600-c28e38a023b7", "Message Interpretation");
            this.MessageInterpretationGroupBox.Controls.Add(this.MessageInterpretationTextBox);
            this.MessageInterpretationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageInterpretationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MessageInterpretationGroupBox.Name = "MessageInterpretationGroupBox";
            this.MessageInterpretationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 254, true);
            this.MessageInterpretationGroupBox.TabIndex = 0;
            this.MessageInterpretationGroupBox.TabStop = false;
            // 
            // MessageInterpretationTextBox
            // 
            this.BindingSource.SetBindingMember(this.MessageInterpretationTextBox, "EM_MessageInterpretation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageInterpretation)));
            this.MessageInterpretationTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageInterpretationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.MessageInterpretationTextBox.Multiline = true;
            this.MessageInterpretationTextBox.Name = "MessageInterpretationTextBox";
            this.MessageInterpretationTextBox.ReadOnly = true;
            this.MessageInterpretationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.MessageInterpretationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 235, true);
            this.MessageInterpretationTextBox.TabIndex = 0;
            this.MessageInterpretationTextBox.TabStop = false;
            // 
            // EDIMessageModificationUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.MessageSplitContainer);
            this.Name = "EDIMessageModificationUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 254, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.MessageSplitContainer.Panel1.ResumeLayout(false);
            this.MessageSplitContainer.Panel2.ResumeLayout(false);
            this.MessageSplitContainer.ResumeLayout(false);
            this.RawMessageGroupBox.ResumeLayout(false);
            this.RawMessageGroupBox.PerformLayout();
            this.MessageInterpretationGroupBox.ResumeLayout(false);
            this.MessageInterpretationGroupBox.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer MessageSplitContainer;
        private Enterprise.ZArchitecture.GUI.ZGroupBox RawMessageGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MessageInterpretationGroupBox;
		private Enterprise.ZArchitecture.ZTextBox MessageInterpretationTextBox;
        private ZArchitecture.ZTextBox RawMessageTextBox;
	}
}
