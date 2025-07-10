using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyLoggedUsingServerTimeWithSaveDelay : DummyLogged
	{
		public DummyLoggedUsingServerTimeWithSaveDelay(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public override void OnSaving()
		{
			System.Threading.Thread.Sleep(1000);
			base.OnSaving();
		}
	}
}
