using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ApplicationIdentifierControl))]
	sealed class ApplicationIdentifierControl_Test : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ApplicationIdentifierCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ApplicationIdentifierControl)control).ApplicationIdentifierGrid.ReadOnly;
		}
	}
}
