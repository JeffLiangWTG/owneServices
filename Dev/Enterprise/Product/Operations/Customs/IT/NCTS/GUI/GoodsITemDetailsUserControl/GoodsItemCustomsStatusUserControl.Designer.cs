namespace Enterprise.Customs.IT.NCTS.GUI
{
	partial class GoodsItemCustomsStatusUserControl
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
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeleteRestoreToggleButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "BY_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_Status)));
			this.StatusDropEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StatusDropEdit, false);
			this.StatusDropEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 22, true);
			this.StatusDropEdit.TabIndex = 0;
			// 
			// DeleteRestoreToggleButton
			//
			this.DeleteRestoreToggleButton.IsCaptionOverridden = false;
			this.DeleteRestoreToggleButton.Click += DeleteRestoreToggleButton_Click;
			this.DeleteRestoreToggleButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.DeleteRestoreToggleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 0, true);
			this.DeleteRestoreToggleButton.Name = "DeleteRestoreToggleButton";
			this.DeleteRestoreToggleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 22, true);
			this.DeleteRestoreToggleButton.TabIndex = 1;
			this.DeleteRestoreToggleButton.ToolTipCaption = null;
			// 
			// GoodsItemCustomsStatusUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StatusDropEdit);
			this.Controls.Add(this.DeleteRestoreToggleButton);
			this.Name = "GoodsItemCustomsStatusUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		internal ZArchitecture.GUI.ZButton DeleteRestoreToggleButton;
	}
}
