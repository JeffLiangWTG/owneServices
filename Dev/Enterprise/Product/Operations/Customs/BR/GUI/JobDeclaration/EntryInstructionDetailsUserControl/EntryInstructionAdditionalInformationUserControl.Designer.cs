namespace Enterprise.Customs.BR.GUI
{
	partial class EntryInstructionAdditionalInformationUserControl
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
			this.AdditionalInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalInformationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FreeTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FreeTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Splitter = new CargoWise.Windows.UI.KSplitter();
			this.SystemGeneratedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SystemGeneratedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalInformationOptionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AdditionalInformationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalInformationGroupBox.SuspendLayout();
			this.AdditionalInformationPanel.SuspendLayout();
			this.FreeTextGroupBox.SuspendLayout();
			this.SystemGeneratedGroupBox.SuspendLayout();
			this.AdditionalInformationOptionPanel.SuspendLayout();
			this.AdditionalInformationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.CusEntryInstruction);
			// 
			// AdditionalInformationGroupBox
			// 
			this.AdditionalInformationGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("A4161C34-0879-4FFD-9C4F-C6C16F2660EF", "Additional Information");
			this.AdditionalInformationGroupBox.Controls.Add(this.AdditionalInformationPanel);
			this.AdditionalInformationGroupBox.Controls.Add(this.AdditionalInformationOptionPanel);
			this.AdditionalInformationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInformationGroupBox.Name = "AdditionalInformationGroupBox";
			this.AdditionalInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 267, true);
			this.AdditionalInformationGroupBox.TabIndex = 0;
			this.AdditionalInformationGroupBox.TabStop = false;
			// 
			// AdditionalInformationPanel
			// 
			this.AdditionalInformationPanel.Controls.Add(this.FreeTextGroupBox);
			this.AdditionalInformationPanel.Controls.Add(this.Splitter);
			this.AdditionalInformationPanel.Controls.Add(this.SystemGeneratedGroupBox);
			this.AdditionalInformationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 80, true);
			this.AdditionalInformationPanel.Name = "AdditionalInformationPanel";
			this.AdditionalInformationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 184, true);
			this.AdditionalInformationPanel.TabIndex = 0;
			// 
			// FreeTextGroupBox
			// 
			this.FreeTextGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("5430F420-A244-47A0-9594-11DE4E06E10D", "Free Text");
			this.FreeTextGroupBox.Controls.Add(this.FreeTextTextBox);
			this.FreeTextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FreeTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FreeTextGroupBox.Name = "FreeTextGroupBox";
			this.FreeTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 184, true);
			this.FreeTextGroupBox.TabIndex = 0;
			this.FreeTextGroupBox.TabStop = false;
			// 
			// FreeTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.FreeTextTextBox, "AdditionalInformationManual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(null)).AdditionalInformationManual)));
			this.FreeTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FreeTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FreeTextTextBox, false);
			this.FreeTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
			this.FreeTextTextBox.Multiline = true;
			this.FreeTextTextBox.Name = "FreeTextTextBox";
			this.FreeTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.FreeTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 147, true);
			this.FreeTextTextBox.TabIndex = 0;
			// 
			// Splitter
			// 
			this.Splitter.Dock = System.Windows.Forms.DockStyle.Right;
			this.Splitter.DoNotSaveSplitterLayout = false;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 0, true);
			this.Splitter.Name = "Splitter";
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 184, true);
			this.Splitter.TabIndex = 2;
			this.Splitter.TabStop = false;
			// 
			// SystemGeneratedGroupBox
			// 
			this.SystemGeneratedGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("F4259370-B461-48F7-85C5-5D49B2DEBB03", "System Generated");
			this.SystemGeneratedGroupBox.Controls.Add(this.SystemGeneratedTextBox);
			this.SystemGeneratedGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.SystemGeneratedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 0, true);
			this.SystemGeneratedGroupBox.Name = "SystemGeneratedGroupBox";
			this.SystemGeneratedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 184, true);
			this.SystemGeneratedGroupBox.TabIndex = 1;
			this.SystemGeneratedGroupBox.TabStop = false;
			// 
			// SystemGeneratedTextBox
			// 
			this.BindingSource.SetBindingMember(this.SystemGeneratedTextBox, "AdditionalInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(null)).AdditionalInformation)));
			this.SystemGeneratedTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SystemGeneratedTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SystemGeneratedTextBox, false);
			this.SystemGeneratedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
			this.SystemGeneratedTextBox.Multiline = true;
			this.SystemGeneratedTextBox.Name = "SystemGeneratedTextBox";
			this.SystemGeneratedTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.SystemGeneratedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 147, true);
			this.SystemGeneratedTextBox.TabIndex = 0;
			// 
			// AdditionalInformationOptionPanel
			// 
			this.AdditionalInformationOptionPanel.Controls.Add(this.AdditionalInformationDropEdit);
			this.AdditionalInformationOptionPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.AdditionalInformationOptionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
			this.AdditionalInformationOptionPanel.Name = "AdditionalInformationOptionPanel";
			this.AdditionalInformationOptionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 46, true);
			this.AdditionalInformationOptionPanel.TabIndex = 1;
			// 
			// AdditionalInformationDropEdit
			// 
			this.AdditionalInformationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalInformationDropEdit, "AdditionalInformationOptionDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.CusEntryInstruction)(null)).AdditionalInformationOptionDescription)));
			this.AdditionalInformationDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AdditionalInformationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 4, true);
			this.AdditionalInformationDropEdit.Name = "AdditionalInformationDropEdit";
			this.AdditionalInformationDropEdit.ShowDescriptionBox = false;
			this.AdditionalInformationDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.AdditionalInformationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 38, true);
			this.AdditionalInformationDropEdit.TabIndex = 1;
			// 
			// EntryInstructionAdditionalInformationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalInformationGroupBox);
			this.Name = "EntryInstructionAdditionalInformationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 267, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalInformationGroupBox.ResumeLayout(false);
			this.AdditionalInformationGroupBox.PerformLayout();
			this.AdditionalInformationPanel.ResumeLayout(false);
			this.AdditionalInformationPanel.PerformLayout();
			this.FreeTextGroupBox.ResumeLayout(false);
			this.FreeTextGroupBox.PerformLayout();
			this.SystemGeneratedGroupBox.ResumeLayout(false);
			this.SystemGeneratedGroupBox.PerformLayout();
			this.AdditionalInformationOptionPanel.ResumeLayout(false);
			this.AdditionalInformationOptionPanel.PerformLayout();
			this.AdditionalInformationDropEdit.ResumeLayout(true);
			this.AdditionalInformationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox AdditionalInformationGroupBox;
		internal ZArchitecture.GUI.ZPanel AdditionalInformationPanel;
		internal ZArchitecture.GUI.ZPanel AdditionalInformationOptionPanel;
		internal ZArchitecture.GUI.ZGroupBox FreeTextGroupBox;
		internal ZArchitecture.GUI.ZGroupBox SystemGeneratedGroupBox;
		internal ZArchitecture.ZTextBox FreeTextTextBox;
		internal ZArchitecture.ZTextBox SystemGeneratedTextBox;
		internal CargoWise.Windows.UI.KSplitter Splitter;
		internal ZArchitecture.GUI.ZDropEdit AdditionalInformationDropEdit;
	}
}
