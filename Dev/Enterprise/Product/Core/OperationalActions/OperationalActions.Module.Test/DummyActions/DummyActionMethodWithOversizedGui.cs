using System.ComponentModel;
using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.GUI.Testing;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class DummyActionMethodWithOversizedGui : OperationalActionMethod
	{
		public DummyActionMethodWithOversizedGui()
			: base(TestingConstants.DummyActionMethodWithOversizedGUI) { }

		public override string Name
		{
			get { return "Dummy Action Method With Over-sized GUI"; }
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
			return new DummyApplicatorControl()
			{
				MinimumSize = new Size(1500, 1500),
			};
		}
		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new DummyOperationalActionMethodApplicator(Name, factory, (DummyOperationalActionMethodSettings)settings);
		}

		public override FilterRequirementList GetFilterRequirements()
		{
			FilterRequirementList result = base.GetFilterRequirements();

			result.Add(FilterConstants.Country, new string[]
			{
				Constants.CountryCodes.Australia,
			});

			return result;
		}
	}
}
