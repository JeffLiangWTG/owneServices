using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZTestPageInternal : ZTestPage
	{
		protected override BusinessObject GetNewDataSource()
		{
			return Factory.New(typeof(DummyBusinessObject));
		}
	}
}
