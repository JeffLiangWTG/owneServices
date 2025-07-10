using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module
{
	public class OpenURLActionMethod : OperationalActionMethod
	{
		public OpenURLActionMethod() : base(new ZGuid("505908b4-c40e-44e7-b33c-c80092bfe8c3")) { }

		public override string Name
		{
			get { return Res.GetString("977F1BAA-5C82-4D50-A0F5-6349C27D9D16", "Open URL"); }
		}

		public override string Description
		{
			get { return Res.GetString("e75bfef7-826b-439a-bcda-1587ca89170a", "Opens the URL in a web browser."); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new OpenURLActionMethodApplicator((OpenURLActionMethodSettings)settings, factory);
		}

		public override bool HasSettings
		{
			get { return true; }
		}

		public override OperationalActionMethodSettings NewSetting(BusinessObjectFactory factory)
		{
			return new OpenURLActionMethodSettings();
		}

		public override IComponent NewSettingsControl()
		{
			return new OpenURLActionMethodSettingsControl();
		}

		public override bool RunWithoutUI
		{
			get { return true; }
		}
	}
}
