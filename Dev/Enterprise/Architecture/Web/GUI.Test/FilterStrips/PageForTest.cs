using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing
{
	public class PageForTest : ZPage
	{
		protected override BusinessObject GetNewDataSource()
		{
			return new DummyFilterStripBusinessObject();
		}

		public void OnLoad()
		{
			base.OnLoad(EventArgs.Empty);
		}
	}
}
