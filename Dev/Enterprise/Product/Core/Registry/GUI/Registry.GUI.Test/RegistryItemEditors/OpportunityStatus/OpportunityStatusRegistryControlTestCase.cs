using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OpportunityStatusRegistryControl))]
	sealed class OpportunityStatusRegistryControlTestCase : RegistryZUserControlTestCase
	{
		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return new OpportunityStatusCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((OpportunityStatusRegistryControl)control).OpportunityStatusGrid.ReadOnly;
		}
	}
}
