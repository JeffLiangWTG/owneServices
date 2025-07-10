namespace Enterprise.Accounting.GUI.WipAccrual
{
	public partial class WIPForm
	{


		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// JobFindBox
			// 
			this.BindingSource.SetBindingMember(this.JobFindBox, "AL_JH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.WIPAccrual.WIP)(null)).AL_JH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.WIPAccrual.WIP)(null)).JobCollection)));
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.WIPAccrual.WIP);
			// 
			// WIPForm
			// 
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("WIPForm|094cf333-343b-4bda-9313-b4a19c14b215", "WIP");
			this.DataSourceType = typeof(Business.WIPAccrual.WIP);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.WIPAccrual.WIP";
			this.Name = "WIPForm";
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
