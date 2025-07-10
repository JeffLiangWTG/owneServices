using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module
{
	public class RunProgramActionMethod : OperationalActionMethod
	{
		public RunProgramActionMethod() : base(new ZGuid("f0441232-1ee6-40f4-b758-5a8fc5d755e1")) { }

		public override string Name
		{
			get { return Res.GetString("702388DC-ACCC-4171-AC5A-5A88EE3A1927", "Run Program"); }
		}

		public override string Description
		{
			get { return Res.GetString("e731b531-e649-42ba-ab8d-741da4de3e3f", "Run a Program with the given arguments."); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new RunProgramActionMethodApplicator((RunProgramActionMethodSettings)settings, factory);
		}

		public override bool HasSettings
		{
			get { return true; }
		}

		public override SecurityCheckpoint[] GetRequiredSecurityCheckpoints()
		{
			return new SecurityCheckpoint[] { Env.Security.RunExternalProgramFromOperationalActions };
		}

		public override OperationalActionMethodSettings NewSetting(BusinessObjectFactory factory)
		{
			return new RunProgramActionMethodSettings();
		}

		public override IComponent NewSettingsControl()
		{
			return new RunProgramActionMethodSettingsControl();
		}

		public override bool RunWithoutUI
		{
			get { return true; }
		}
	}
}
