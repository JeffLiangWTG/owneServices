namespace Enterprise.Customs.EU.GUI
{
	partial class FirstPlaceOfUseOrProcessingUserControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FirstPlaceOfUseOrProcessingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FirstPlaceOfUseOrProcessingControl = new Enterprise.Customs.EU.GUI.PlaceOfUseOrProcessingControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FirstPlaceOfUseOrProcessingGroupBox.SuspendLayout();
			this.FirstPlaceOfUseOrProcessingControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// FirstPlaceOfUseOrProcessingGroupBox
			//
			this.FirstPlaceOfUseOrProcessingGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("69509A43-C8D0-4741-B927-3E3B60877248", "First Place of Use or Processing", "[Annex A 4/5] Dates, Times, Periods and Places > First Place of Use or Processing" +
		"");
			this.FirstPlaceOfUseOrProcessingGroupBox.Controls.Add(this.FirstPlaceOfUseOrProcessingControl);
			this.FirstPlaceOfUseOrProcessingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FirstPlaceOfUseOrProcessingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FirstPlaceOfUseOrProcessingGroupBox.Name = "FirstPlaceOfUseOrProcessingGroupBox";
			this.FirstPlaceOfUseOrProcessingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 50, true);
			this.FirstPlaceOfUseOrProcessingGroupBox.TabIndex = 0;
			this.FirstPlaceOfUseOrProcessingGroupBox.TabStop = false;
			// 
			// FirstPlaceOfUseOrProcessingControl
			//
			this.FirstPlaceOfUseOrProcessingControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("96D8E915-7AC4-4F05-A1BC-FF4765C94C47", "First Place of Use or Processing");
			this.FirstPlaceOfUseOrProcessingControl.AllowDrop = true;
			this.FirstPlaceOfUseOrProcessingControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FirstPlaceOfUseOrProcessingControl, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.IFirstPlaceOfUseOrProcessingProvider)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)))));
			this.FirstPlaceOfUseOrProcessingControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.FirstPlaceOfUseOrProcessingControl.Name = "FirstPlaceOfUseOrProcessingControl";
			this.FirstPlaceOfUseOrProcessingControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 25, true);
			this.FirstPlaceOfUseOrProcessingControl.TabIndex = 0;
			// 
			// FirstPlaceOfUseOrProcessingUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FirstPlaceOfUseOrProcessingGroupBox);
			this.Name = "FirstPlaceOfUseOrProcessingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 50, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FirstPlaceOfUseOrProcessingGroupBox.ResumeLayout(false);
			this.FirstPlaceOfUseOrProcessingGroupBox.PerformLayout();
			this.FirstPlaceOfUseOrProcessingControl.ResumeLayout(true);
			this.FirstPlaceOfUseOrProcessingControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZGroupBox FirstPlaceOfUseOrProcessingGroupBox;
		internal PlaceOfUseOrProcessingControl FirstPlaceOfUseOrProcessingControl;
	}
}
