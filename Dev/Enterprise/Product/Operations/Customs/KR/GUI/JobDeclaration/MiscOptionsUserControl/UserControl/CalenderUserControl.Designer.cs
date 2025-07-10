
namespace Enterprise.Customs.KR.GUI
{
	partial class CalenderUserControl
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
			this.StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.Label = new Enterprise.ZArchitecture.ZLabel();
			this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StartDateEdit.SuspendLayout();
			this.EndDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// StartDateEdit
			// 
			this.StartDateEdit.AllowDrop = true;
			this.StartDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.StartDateEdit, "UnderbondMovementArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).UnderbondMovementArrivalDate)));
			this.StartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			this.StartDateEdit.Dock = System.Windows.Forms.DockStyle.Left;
			this.StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StartDateEdit.Name = "StartDateEdit";
			this.StartDateEdit.TabIndex = 0;
			// 
			// Label
			// 
			this.Label.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 0, true);
			this.Label.Name = "Label";
			this.Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 20, true);
			this.Label.TabIndex = 1;
			this.Label.Text = "~";
			// 
			// EndDateEdit
			// 
			this.EndDateEdit.AllowDrop = true;
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EndDateEdit, "UnderbondMovementDepartureDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).UnderbondMovementDepartureDate)));
			this.EndDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			this.EndDateEdit.Dock = System.Windows.Forms.DockStyle.Right;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EndDateEdit, false);
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 0, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 2;
			// 
			// CalenderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EndDateEdit);
			this.Controls.Add(this.Label);
			this.Controls.Add(this.StartDateEdit);
			this.Name = "CalenderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StartDateEdit.ResumeLayout(true);
			this.StartDateEdit.PerformLayout();
			this.EndDateEdit.ResumeLayout(true);
			this.EndDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZDateEdit StartDateEdit;
		public ZArchitecture.ZLabel Label;
		public ZArchitecture.GUI.ZDateEdit EndDateEdit;
	}
}
