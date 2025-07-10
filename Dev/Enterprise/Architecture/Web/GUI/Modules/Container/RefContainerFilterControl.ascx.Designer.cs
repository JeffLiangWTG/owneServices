using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public partial class RefContainerFilterControl
	{
		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Details.BindTo = "RC_Description";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.RefContainerFilterBusinessObject)(null)).RC_Description);

			StartsWithRadioButton.BindTo = "RC_DescriptionStartsWith";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.RefContainerFilterBusinessObject)(null)).RC_DescriptionStartsWith);

			ContainsRadioButton.BindTo = "RC_DescriptionContains";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.RefContainerFilterBusinessObject)(null)).RC_DescriptionContains);

			ShippingMode.BindTo = "RC_ShippingMode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.RefContainerFilterBusinessObject)(null)).RC_ShippingMode);

			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Web.GUI";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Web.Modules.OrganisationFilterBusinessObject";
		}
		#endregion
	}
}
