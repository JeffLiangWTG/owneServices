using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class OrganisationFilterControl : BaseUserControl
	{
		protected ZDropDownList DetailsList;
		protected ZTextBox Details;
		protected ZRadioButton StartsWithRadioButton;
		protected ZRadioButton ContainsRadioButton;

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
			Details.BindTo = "OH_Details";
			Details.BindTo = "OH_Details";
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Web.GUI";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Web.Modules.OrganisationFilterBusinessObject";
		}
		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			DetailsList.AutoPostBack = true;
			DetailsList.SelectedIndexChanged += DetailsList_SelectedIndexChanged;
		}

		void DetailsList_SelectedIndexChanged(object sender, EventArgs e)
		{
			DetailsList.Bind(Page.DataSource);
			Details.Bind(Page.DataSource);
		}
	}
}
