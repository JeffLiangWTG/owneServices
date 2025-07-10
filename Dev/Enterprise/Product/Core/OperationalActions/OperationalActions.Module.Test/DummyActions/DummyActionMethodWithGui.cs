using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.GUI.Testing;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class DummyActionMethodWithGui : OperationalActionMethod
	{
		public DummyActionMethodWithGui()
			: base(TestingConstants.DummyActionMethodWithGUI) { }

		public override string Name
		{
			get { return "Dummy Action Method With GUI"; }
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

		public override bool HasControl
		{
			get { return true; }
		}
		public override IComponent NewGuiControl()
		{
			return new DummyApplicatorControl();
		}
		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new DummyOperationalActionMethodApplicator(Name, factory, (DummyOperationalActionMethodSettings)settings);
		}

		public override bool HasSettings
		{
			get { return true; }
		}
		public override IComponent NewSettingsControl()
		{
			return new DummySettingsControl();
		}
		public override OperationalActionMethodSettings NewSetting(BusinessObjectFactory factory)
		{
			return new DummyOperationalActionMethodSettings();
		}

		public override SecurityCheckpoint[] GetRequiredSecurityCheckpoints()
		{
			return new SecurityCheckpoint[]
			{
				Env.Security.Schedules,
				Env.Security.Forwarding,
			};
		}
		public override LicenceCheckpoint[] GetRequiredLicenceCheckpoints()
		{
			return new LicenceCheckpoint[]
			{
				Env.Licence.ShippingManagerBookings,
				Env.Licence.Forwarder,
			};
		}
	}
}
