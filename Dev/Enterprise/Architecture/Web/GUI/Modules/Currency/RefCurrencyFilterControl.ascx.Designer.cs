using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Modules.Currency
{
	public partial class RefCurrencyFilterControl
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
			Description.BindTo = "RX_Desc";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((WebRefCurrencyFilterBusinessObject)(null)).RX_Desc);
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((WebRefCurrencyFilterBusinessObject)(null)).RX_Desc);
			Description.BindTo = "RX_Desc";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((WebRefCurrencyFilterBusinessObject)(null)).RX_Desc);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Web.GUI.Modules.Currency";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Web.GUI.Modules.Currency.WebRefCurrencyFilterBusinessObject";
			this.Load += new EventHandler(this.Page_Load);
		}
		#endregion
	}
}
