namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7StandAloneDeclarationUserControl
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
			this.StandAloneDeclarationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConvertButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.H7.Business.AsycudaBill);
			// 
			// StandAloneDeclarationTextBox
			// 
			this.BindingSource.SetBindingMember(this.StandAloneDeclarationTextBox, "EntrySummaryReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).EntrySummaryReferenceNumber)));
			this.StandAloneDeclarationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StandAloneDeclarationTextBox.Name = "StandAloneDeclarationTextBox";
			this.StandAloneDeclarationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.StandAloneDeclarationTextBox.TabIndex = 0;
			// 
			// ConvertButton
			// 
			this.ConvertButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 0, true);
			this.ConvertButton.Name = "ConvertButton";
			this.ConvertButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
			this.ConvertButton.BackgroundImage = Enterprise.ZArchitecture.GUI.Icons.GetImage(Enterprise.ZArchitecture.Modules.IconTypes.NewButtonActive);
			this.ConvertButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.ConvertButton.Click += new System.EventHandler(this.ConvertButton_Click);
			this.ConvertButton.ToolTipCaption = Enterprise.ZArchitecture.Core.ResString.GetMultilingualString("55001def-b78f-4ad3-ae7a-171d581322ef", "Convert to Stand Alone Declaration");
			this.ConvertButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ConvertButton.FlatAppearance.BorderSize = 0;
			this.ConvertButton.BackColor = System.Drawing.Color.Transparent;
			this.ConvertButton.TabIndex = 1;
			// 
			// EditButton
			// 
			this.EditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 0, true);
			this.EditButton.Name = "EditButton";
			this.EditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
			this.EditButton.BackgroundImage = Enterprise.ZArchitecture.GUI.Icons.GetImage(Enterprise.ZArchitecture.Modules.IconTypes.EditButtonActive);
			this.EditButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.EditButton.Click += new System.EventHandler(this.EditButton_Click);
			this.EditButton.ToolTipCaption = Enterprise.ZArchitecture.Core.ResString.GetMultilingualString("4fe26ff9-861b-4b6a-ae95-841354a76000", "Edit Stand Alone Declaration");
			this.EditButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.EditButton.FlatAppearance.BorderSize = 0;
			this.EditButton.BackColor = System.Drawing.Color.Transparent;
			this.EditButton.TabIndex = 2;
			// 
			// StandAloneDeclarationUserControl
			//
			base.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EditButton);
			this.Controls.Add(this.ConvertButton);
			this.Controls.Add(this.StandAloneDeclarationTextBox);
			this.Name = "StandAloneDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 30, true);
			((System.ComponentModel.ISupportInitialize)base.BindingSource).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.ZTextBox StandAloneDeclarationTextBox;
		private ZArchitecture.GUI.ZButton ConvertButton;
		private ZArchitecture.GUI.ZButton EditButton;
	}
}
