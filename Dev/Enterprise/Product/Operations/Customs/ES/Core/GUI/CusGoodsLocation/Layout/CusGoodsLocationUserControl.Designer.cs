namespace Enterprise.Customs.ES.GUI
{
	partial class CusGoodsLocationUserControl
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
			this.ESAuthorizationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ESAuthorizationCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusGoodsLocation);
			// 
			// ESAuthorizationCodeFindBox
			// 
			this.ESAuthorizationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ESAuthorizationCodeFindBox, "Address.AuthorisationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusGoodsLocation)(null)).Address.AuthorisationNumber)));
			this.ESAuthorizationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ESAuthorizationCodeFindBox.Name = "ESAuthorizationCodeFindBox";
			this.ESAuthorizationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ESAuthorizationCodeFindBox.ParentType = null;
			this.ESAuthorizationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ESAuthorizationCodeFindBox.TabIndex = 0;
			// 
			// CusGoodsLocationUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ESAuthorizationCodeFindBox);
			this.Name = "CusGoodsLocationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 466, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ESAuthorizationCodeFindBox.ResumeLayout(true);
			this.ESAuthorizationCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox ESAuthorizationCodeFindBox;
	}
}
