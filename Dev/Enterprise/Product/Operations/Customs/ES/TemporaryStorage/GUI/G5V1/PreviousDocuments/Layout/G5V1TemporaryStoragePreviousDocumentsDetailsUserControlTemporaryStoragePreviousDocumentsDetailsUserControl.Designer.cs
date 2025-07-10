namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class G5V1TemporaryStoragePreviousDocumentsDetailsUserControl
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
			this.ReferenceNumber2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStoragePreviousDocument);
			// 
			// ReferenceNumber2TextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumber2TextBox, "CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStoragePreviousDocument)(null)).CSI_ReferenceNumber)));
			this.ReferenceNumber2TextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("18B01BD3-7C3D-40EC-B136-D5F007ADA086", "Flight Number");
			this.ReferenceNumber2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 17, true);
			this.ReferenceNumber2TextBox.Name = "ReferenceNumber2TextBox";
			this.ReferenceNumber2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
			this.ReferenceNumber2TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ReferenceNumber2TextBox.TabIndex = 1;
			// 
			// G5V1TemporaryStoragePreviousDocumentsDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ReferenceNumber2TextBox);
			this.Name = "G5V1TemporaryStoragePreviousDocumentsDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.ZTextBox ReferenceNumber2TextBox;
	}
}
