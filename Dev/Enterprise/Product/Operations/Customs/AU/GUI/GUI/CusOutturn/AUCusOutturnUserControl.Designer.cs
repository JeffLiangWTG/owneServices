namespace Enterprise.Customs.AU.GUI
{
	partial class AUCusOutturnUserControl
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
		internal void InitializeComponent()
		{
			this.ResponsiblePartyIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ResponsiblePartyIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.ResponsiblePartyIDTextBox);
			this.zPanel1.Controls.Add(this.ResponsiblePartyIDLabel);
			this.zPanel1.Controls.SetChildIndex(this.ResponsiblePartyIDLabel, 0);
			this.zPanel1.Controls.SetChildIndex(this.ResponsiblePartyIDTextBox, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusUnderbond);
			// 
			// ResponsiblePartyIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ResponsiblePartyIDTextBox, "AU_OutturnResponsiblePartyID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusUnderbond)(null)).AU_OutturnResponsiblePartyID)));
			this.ResponsiblePartyIDTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("062a8422-cb5c-46c7-8421-c9ef2b295837", "Responsible Party ID");
			this.ResponsiblePartyIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(715, 4, true);
			this.ResponsiblePartyIDTextBox.Name = "ResponsiblePartyIDTextBox";
			this.ResponsiblePartyIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.ResponsiblePartyIDTextBox.TabIndex = 4;
			// 
			// ResponsiblePartyIDLabel
			// 
			this.ResponsiblePartyIDLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ResponsiblePartyIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(605, 3, true);
			this.ResponsiblePartyIDLabel.Name = "ResponsiblePartyIDLabel";
			this.ResponsiblePartyIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 23, true);
			this.ResponsiblePartyIDLabel.TabIndex = 1;
			this.ResponsiblePartyIDLabel.Text = "Responsible Party ID:";
			// 
			// AUCusOutturnUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "AUCusOutturnUserControl";
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal Enterprise.ZArchitecture.ZTextBox ResponsiblePartyIDTextBox;
		Enterprise.ZArchitecture.ZLabel ResponsiblePartyIDLabel;
		#endregion
	}
}
