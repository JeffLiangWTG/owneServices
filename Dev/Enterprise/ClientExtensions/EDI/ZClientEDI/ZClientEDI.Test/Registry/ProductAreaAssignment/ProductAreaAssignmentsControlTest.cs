using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI
{
	[TestedType(typeof(ProductAreaAssignmentsControl))]
	class ProductAreaAssignmentsControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ProductAreaAssignmentCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			ProductAreaAssignmentsControl control = (ProductAreaAssignmentsControl)control1;
			return control.AssignmentGrid.ReadOnly;
		}
	}
}
