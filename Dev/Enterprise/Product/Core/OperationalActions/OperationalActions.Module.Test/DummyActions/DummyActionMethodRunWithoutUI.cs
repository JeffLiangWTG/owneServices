using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class DummyActionMethodRunWithoutUI : OperationalActionMethod
	{
		public DummyActionMethodRunWithoutUI() : base(TestingConstants.DummyActionMethodRunWithoutUI) { }

		public override string Name
		{
			get { return "Dummy Action Method Run Without UI"; }
		}

		public override string Description
		{
			get { return "A dummy defined process strictly for the purpose of testing the operational actions system (not for general use)."; }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new DummyOperationalActionMethodApplicator(Name, factory, null) { Excuse = "No Excuse" };
		}

		public override SecurityCheckpoint[] GetRequiredSecurityCheckpoints()
		{
			return new[]
			{
				Env.Security.Schedules,
				Env.Security.AgencyBillOfLading
			};
		}

		public override LicenceCheckpoint[] GetRequiredLicenceCheckpoints()
		{
			return new[]
			{
				Env.Licence.ShippingManagerBillOfLading,
				Env.Licence.Forwarder
			};
		}

		public override bool RunWithoutUI
		{
			get { return true; }
		}
	}
}
