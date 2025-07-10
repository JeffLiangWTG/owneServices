using System;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Web.Utilities.Test
{
	public class TestWebDbEnvironment : BaseDbEnvironment, IDisposable
	{
		public TestWebDbEnvironment()
		{
			existingEnv = DbEnv.Instance;
			isServingWebBasedApp = false;

			DbEnv.SetDbEnvironment(this);
		}
		readonly IDbEnvironment existingEnv;
		bool isServingWebBasedApp;

		public void SetServingWebBasedApp(bool isServingWebBasedApp)
		{
			this.isServingWebBasedApp = isServingWebBasedApp;
		}

		public void Dispose()
		{
			DbEnv.SetDbEnvironment(existingEnv);
		}

		public override bool IsServingWebBasedApp => isServingWebBasedApp;
	}
}
