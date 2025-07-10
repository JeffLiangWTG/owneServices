using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class ActivitiesAndProceduresUserControl
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
            this.ActivitiesAndProceduresGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.DetailsOfPlannedActivitiesTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ActivitiesAndProceduresGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // ActivitiesAndProceduresGroupBox
            // 
            this.ActivitiesAndProceduresGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ActivitiesAndProceduresGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("3D1F5373-12E8-4111-9153-01944ABE5BC4", "Activities and Procedures");
            this.ActivitiesAndProceduresGroupBox.Controls.Add(this.DetailsOfPlannedActivitiesTextBox);
            this.ActivitiesAndProceduresGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ActivitiesAndProceduresGroupBox.Name = "ActivitiesAndProceduresGroupBox";
            this.ActivitiesAndProceduresGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 101, true);
            this.ActivitiesAndProceduresGroupBox.TabIndex = 1;
            this.ActivitiesAndProceduresGroupBox.TabStop = false;
            // 
            // DetailsOfPlannedActivitiesTextBox
            // 
            this.DetailsOfPlannedActivitiesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.DetailsOfPlannedActivitiesTextBox, "CustomsEntryInstructions.DetailsOfPlannedActivities");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DetailsOfPlannedActivities)));
            this.DetailsOfPlannedActivitiesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 37, true);
            this.DetailsOfPlannedActivitiesTextBox.Name = "DetailsOfPlannedActivitiesTextBox";
            this.DetailsOfPlannedActivitiesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 15, true);
            this.DetailsOfPlannedActivitiesTextBox.TabIndex = 3;
            // 
            // ActivitiesAndProceduresUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ActivitiesAndProceduresGroupBox);
            this.Name = "ActivitiesAndProceduresUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 104, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ActivitiesAndProceduresGroupBox.ResumeLayout(false);
            this.ActivitiesAndProceduresGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZGroupBox ActivitiesAndProceduresGroupBox;
		internal ZArchitecture.ZTextBox DetailsOfPlannedActivitiesTextBox;
	}
}
