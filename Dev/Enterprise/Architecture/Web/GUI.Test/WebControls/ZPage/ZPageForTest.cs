using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZPageForTest : ZPage
	{
		protected override BusinessObject GetNewDataSource()
		{
			return new WebFilterBusinessObjectFactory(Factory).New<OrganisationFilterBusinessObject>();
		}

		public void OnLoad()
		{
			base.OnLoad(EventArgs.Empty);
		}
	}
}
