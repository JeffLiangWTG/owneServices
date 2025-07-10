namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class SchedulableDateEdit
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
            this.EditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ValueDateEdit = new ZDateEditWithShortFormatAndLongBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ValueDateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // EditButton
            // 
            this.EditButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SchedulableDateEdit|fcfe0e48-c9fa-4670-9f03-24dcc8e2ba83", "Edit");
            this.EditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 0, true);
            this.EditButton.Name = "EditButton";
            this.EditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 23, true);
            this.EditButton.TabIndex = 1;
            this.EditButton.ToolTipCaption = null;
            this.EditButton.UseVisualStyleBackColor = true;
            this.EditButton.Click += new System.EventHandler(this.EditButton_Click);
            // 
            // ValueDateEdit
            // 
            this.ValueDateEdit.AllowDrop = true;
            this.ValueDateEdit.AutoCompleteMonthThreshold = 1;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ValueDateEdit, false);
            this.ValueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
            this.ValueDateEdit.Name = "ValueDateEdit";
            this.ValueDateEdit.TabIndex = 2;
            // 
            // SchedulableDateEdit
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ValueDateEdit);
            this.Controls.Add(this.EditButton);
            this.Name = "SchedulableDateEdit";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 23, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ValueDateEdit.ResumeLayout(true);
            this.ValueDateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton EditButton;
		internal ZDateEditWithShortFormatAndLongBox ValueDateEdit;
	}
}
