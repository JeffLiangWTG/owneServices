using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(IncotermsControl))]
	sealed class IncotermsControlTest : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new IncoTermChargeCodesCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((IncotermsControl)control).IncoTermChargesGrid.ReadOnly;
		}
	}
}
