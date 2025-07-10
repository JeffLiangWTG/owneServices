using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(DefaultFreightPercentageControl))]
	sealed class DefaultFreightPercentageControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new DefaultFreightPercentageCollection(Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
			=> ((DefaultFreightPercentageControl)control).DefaultFreightPercentageGrid.ReadOnly;
	}
}
