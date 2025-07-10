using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class DummyActionMethodWithoutGui : OperationalActionMethod
	{
		public DummyActionMethodWithoutGui()
			: base(TestingConstants.DummyActionMethodWithoutGUI) { }

		public override string Name
		{
			get { return "Dummy Action Method Without GUI"; }
		}

		public override string Description
		{
			get
			{
				return
					"A dummy defined process strictly for the purpose of testing the operational " +
					"actions system (not for general use)." +
					"";
			}
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new DummyOperationalActionMethodApplicator(Name, factory, null) { Excuse = "No Excuse" };
		}

		public override SecurityCheckpoint[] GetRequiredSecurityCheckpoints()
		{
			return new SecurityCheckpoint[]
			{
				Env.Security.Schedules,
				Env.Security.AgencyBillOfLading,
			};
		}
		public override LicenceCheckpoint[] GetRequiredLicenceCheckpoints()
		{
			return new LicenceCheckpoint[]
			{
				Env.Licence.ShippingManagerBillOfLading,
				Env.Licence.Forwarder,
			};
		}
	}
}
