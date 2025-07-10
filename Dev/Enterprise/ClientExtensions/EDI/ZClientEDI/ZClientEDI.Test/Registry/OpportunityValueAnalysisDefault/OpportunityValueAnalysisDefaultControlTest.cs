using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI
{
	[TestedType(typeof(OpportunityValueAnalysisDefaultControl))]
	class OpportunityValueAnalysisDefaultControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new OpportunityValueAnalysisDefaultCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((OpportunityValueAnalysisDefaultControl)control).gridRates.ReadOnly;
		}
	}
}
