using System.ComponentModel;
using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.GUI.Testing;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class DummyActionMethodWithLargeGui : OperationalActionMethod
	{
		public DummyActionMethodWithLargeGui()
			: base(TestingConstants.DummyActionMethodWithLargeGUI) { }

		public override string Name
		{
			get { return "Dummy Action Method With Large GUI"; }
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
				MinimumSize = new Size(500, 500),
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
				Constants.CountryCodes.NewZealand,
				Constants.CountryCodes.Singapore,
			});

			return result;
		}
	}
}
