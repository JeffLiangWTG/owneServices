namespace Enterprise.ZArchitecture.GUI.Internal
{
	partial class ZTimeRangeControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.ToTimeEdit = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripTimeEdit();
            this.FromTimeEdit = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripTimeEdit();
            this.PropertySearchDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.PropertySearchDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // ToTimeEdit
            // 
            this.ToTimeEdit.AllowDrop = true;
            this.ToTimeEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZTimeRangeControl|51df9c84-a77e-4a3a-b856-4a83cf50d135", "To");
            this.ToTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 25, true);
            this.ToTimeEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 0, true);
            this.ToTimeEdit.Name = "ToTimeEdit";
            this.ToTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 16, true);
            this.ToTimeEdit.TabIndex = 5;
            this.ToTimeEdit.Visible = false;
            // 
            // FromTimeEdit
            // 
            this.FromTimeEdit.AllowDrop = true;
            this.FromTimeEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZTimeRangeControl|3cebc90b-93a8-415c-a023-41f5f747da29", "From");
            this.FromTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 25, true);
            this.FromTimeEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 0, true);
            this.FromTimeEdit.Name = "FromTimeEdit";
            this.FromTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 16, true);
            this.FromTimeEdit.TabIndex = 3;
            this.FromTimeEdit.Visible = false;
            // 
            // PropertySearchDropEdit
            // 
            this.PropertySearchDropEdit.AllowDrop = true;
            this.PropertySearchDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.PropertySearchDropEdit.FormattingEnabled = false;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PropertySearchDropEdit, false);
            this.PropertySearchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 3, true);
            this.PropertySearchDropEdit.MaxItemsToShowInDropDown = 28;
            this.PropertySearchDropEdit.Name = "PropertySearchDropEdit";
            this.PropertySearchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 16, true);
            this.PropertySearchDropEdit.TabIndex = 0;
            // 
            // ZTimeRangeControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.PropertySearchDropEdit);
            this.Controls.Add(this.ToTimeEdit);
            this.Controls.Add(this.FromTimeEdit);
            this.Name = "ZTimeRangeControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 48, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PropertySearchDropEdit.ResumeLayout(true);
            this.PropertySearchDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZFilterStripDropEdit PropertySearchDropEdit;
		public ZFilterStripTimeEdit FromTimeEdit;
		public ZFilterStripTimeEdit ToTimeEdit;
	}
}
