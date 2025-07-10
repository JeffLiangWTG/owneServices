using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Modules.Commodity
{
	public partial class RefCommodityCodeFilterControl
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
			Description.BindTo = "RH_Description";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebRefCommodityCodeFilterBusinessObject)(null)).RH_Description);
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebRefCommodityCodeFilterBusinessObject)(null)).RH_Description);
			Description.BindTo = "RH_Description";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebRefCommodityCodeFilterBusinessObject)(null)).RH_Description);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Web.Modules";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Web.Modules.WebRefCommodityCodeFilterBusinessObject";
			this.Load += new EventHandler(this.Page_Load);
		}
		#endregion
	}
}
