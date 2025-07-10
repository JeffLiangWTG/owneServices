using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class DummyPage : ZPage
	{
		public void OnLoad()
		{
			OnLoad(EventArgs.Empty);
		}

		protected override BusinessObject GetNewDataSource()
		{
			return new DummyFilterStripBusinessObject();
		}

		public DummyFilterStripBusinessObject GetNewDummyFilterStripBizO()
		{
			return new DummyFilterStripBusinessObject();
		}
	}
}
