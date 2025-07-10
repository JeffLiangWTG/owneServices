namespace Enterprise.Services.OperationalActions.Module
{
	partial class ShowEditNoteActionMethodSettingsControl
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
			this.textBoxNoteDescription = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Module.ShowEditNoteActionMethodSettings);
			// 
			// textBoxNoteDescription
			// 
			this.textBoxNoteDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.textBoxNoteDescription, "NoteDescription");
			this.textBoxNoteDescription.CaptionResourceString = Enterprise.Services.OperationalActions.Module.Res.GetData("4c4606b8-de40-4a4e-88e6-206f45bcea3c", "Note Description", "Description of a note to be shown in the popup window.");
			this.textBoxNoteDescription.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.textBoxNoteDescription, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.textBoxNoteDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.textBoxNoteDescription.Name = "textBoxNoteDescription";
			this.textBoxNoteDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.textBoxNoteDescription.TabIndex = 1;
			// 
			// ShowEditNoteActionMethodSettingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.textBoxNoteDescription);
			this.Name = "ShowEditNoteActionMethodSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox textBoxNoteDescription;
	}
}
