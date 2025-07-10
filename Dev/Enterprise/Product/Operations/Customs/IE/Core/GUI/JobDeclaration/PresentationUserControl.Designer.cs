namespace Enterprise.Customs.IE.GUI
{
	partial class PresentationUserControl
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
			this.PresentationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StartDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PresentationGroupBox.SuspendLayout();
			this.StartDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.Declaration.JobDeclaration);
			// 
			// PresentationGroupBox
			// 
			this.PresentationGroupBox.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("6f893db1-a39f-4ab5-8955-cbafca788b28", "Presentation");
			this.PresentationGroupBox.Controls.Add(this.StartDateDateEdit);
			this.PresentationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PresentationGroupBox.Name = "PresentationGroupBox";
			this.PresentationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 37, true);
			this.PresentationGroupBox.TabIndex = 0;
			this.PresentationGroupBox.TabStop = false;
			// 
			// StartDateDateEdit
			// 
			this.StartDateDateEdit.AllowDrop = true;
			this.StartDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.StartDateDateEdit, "ZG_PresentationStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.Business.Declaration.JobDeclaration)(null)).ZG_PresentationStartDate)));
			this.StartDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			this.StartDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 11, true);
			this.StartDateDateEdit.Name = "StartDateDateEdit";
			this.StartDateDateEdit.TabIndex = 0;
			// 
			// PresentationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PresentationGroupBox);
			this.Name = "PresentationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 37, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PresentationGroupBox.ResumeLayout(false);
			this.PresentationGroupBox.PerformLayout();
			this.StartDateDateEdit.ResumeLayout(true);
			this.StartDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox PresentationGroupBox;
		private ZArchitecture.GUI.ZDateEdit StartDateDateEdit;
	}
}
