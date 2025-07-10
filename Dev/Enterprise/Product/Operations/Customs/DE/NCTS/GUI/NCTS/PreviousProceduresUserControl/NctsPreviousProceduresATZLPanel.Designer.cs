namespace Enterprise.Customs.DE.NCTS.GUI
{
	partial class NctsPreviousProceduresATZLPanel
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
			this.LocalReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorizationNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AuthorizationNumberDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// LocalReferenceTextBox
			// 
			this.LocalReferenceTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocalReferenceTextBox, "PreviousProcedureMaster.CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedureMaster.CSI_ReferenceNumber2)));
			this.LocalReferenceTextBox.CaptionResourceString = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("BAA39D9C-B187-49EC-90E0-7DC5E8BCFCD9", "Local Reference");
			this.LocalReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LocalReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 3, true);
			this.LocalReferenceTextBox.Name = "LocalReferenceTextBox";
			this.LocalReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.LocalReferenceTextBox.TabIndex = 1;
			// 
			// AuthorizationNumberDropEdit
			// 
			this.AuthorizationNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationNumberDropEdit, "PreviousProcedureMaster.AuthorizationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedureMaster.AuthorizationNumber)));
			this.AuthorizationNumberDropEdit.CaptionResourceString = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("7C2F1E9A-D108-4160-BD4E-C5FF521EED6A", "Auth. No.");
			this.AuthorizationNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 3, true);
			this.AuthorizationNumberDropEdit.Name = "AuthorizationNumberDropEdit";
			this.AuthorizationNumberDropEdit.ShowDescriptionBox = false;
			this.AuthorizationNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.AuthorizationNumberDropEdit.TabIndex = 2;
			// 
			// NctsPreviousProceduresATZLPanel
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LocalReferenceTextBox);
			this.Controls.Add(this.AuthorizationNumberDropEdit);
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 43, true);
			this.Name = "NctsPreviousProceduresATZLPanel";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 27, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AuthorizationNumberDropEdit.ResumeLayout(true);
			this.AuthorizationNumberDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZTextBox LocalReferenceTextBox;
		internal ZArchitecture.GUI.ZDropEdit AuthorizationNumberDropEdit;

	}
}
