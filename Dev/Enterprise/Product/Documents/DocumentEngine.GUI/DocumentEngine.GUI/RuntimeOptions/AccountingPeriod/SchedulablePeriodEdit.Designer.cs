namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class SchedulablePeriodEdit
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
			this.ValuePeriodEdit = new Enterprise.ZArchitecture.GUI.ZPeriodEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// EditButton
			// 
			this.EditButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SchedulablePeriodEdit|55bb1498-afaf-4b4f-8b9b-11bc2aa00aa9", "Edit");
			this.EditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 0, true);
			this.EditButton.Name = "EditButton";
			this.EditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 23, true);
			this.EditButton.TabIndex = 1;
			this.EditButton.UseVisualStyleBackColor = true;
			this.EditButton.Click += new System.EventHandler(this.EditButton_Click);
			// 
			// ValuePeriodEdit
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ValuePeriodEdit, false);
			this.ValuePeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.ValuePeriodEdit.Name = "ValuePeriodEdit";
			this.ValuePeriodEdit.TabIndex = 0;
			// 
			// SchedulablePeriodEdit
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ValuePeriodEdit);
			this.Controls.Add(this.EditButton);
			this.Name = "SchedulablePeriodEdit";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 23, true);
			this.Resize += new System.EventHandler(this.SchedulablePeriodEdit_Resize);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton EditButton;
		internal Enterprise.ZArchitecture.GUI.ZPeriodEdit ValuePeriodEdit;
	}
}
