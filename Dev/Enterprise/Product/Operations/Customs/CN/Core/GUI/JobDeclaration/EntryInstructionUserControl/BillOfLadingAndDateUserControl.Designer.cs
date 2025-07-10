namespace Enterprise.Customs.CN.GUI
{
	partial class BillOfLadingAndDateUserControl
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
			this.BillOfLadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillOfLadingDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BillOfLadingDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.CusEntryInstruction);
			// 
			// BillOfLadingTextBox
			// 
			this.BillOfLadingTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BillOfLadingTextBox, "BillOfLading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).BillOfLading)));
			this.BillOfLadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillOfLadingTextBox.Name = "BillOfLadingTextBox";
			this.BillOfLadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.BillOfLadingTextBox.TabIndex = 0;
			// 
			// BillOfLadingDateDateEdit
			// 
			this.BillOfLadingDateDateEdit.AllowDrop = true;
			this.BillOfLadingDateDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BillOfLadingDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.BillOfLadingDateDateEdit, "BillOfLadingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).BillOfLadingDate)));
			this.BillOfLadingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 0, true);
			this.BillOfLadingDateDateEdit.Name = "BillOfLadingDateDateEdit";
			this.BillOfLadingDateDateEdit.TabIndex = 1;
			// 
			// BillOfLadingAndDateUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BillOfLadingTextBox);
			this.Controls.Add(this.BillOfLadingDateDateEdit);
			this.Name = "BillOfLadingAndDateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BillOfLadingDateDateEdit.ResumeLayout(true);
			this.BillOfLadingDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox BillOfLadingTextBox;
		internal ZArchitecture.GUI.ZDateEdit BillOfLadingDateDateEdit;
	}
}
