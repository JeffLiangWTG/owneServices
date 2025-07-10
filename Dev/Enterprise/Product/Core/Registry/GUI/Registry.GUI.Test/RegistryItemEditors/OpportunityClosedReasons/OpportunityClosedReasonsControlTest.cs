using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OpportunityClosedReasonsControl))]
	sealed class OpportunityClosedReasonsControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new OpportunityClosedReasonsCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var opportunityClosedReasonsControl = (OpportunityClosedReasonsControl)control;

			return (opportunityClosedReasonsControl.Controls.Find("MainGrid", true)[0] as ZGrid).ReadOnly &&
				(opportunityClosedReasonsControl.Controls.Find("SubGrid", true)[0] as ZGrid).ReadOnly;
		}
	}
}
