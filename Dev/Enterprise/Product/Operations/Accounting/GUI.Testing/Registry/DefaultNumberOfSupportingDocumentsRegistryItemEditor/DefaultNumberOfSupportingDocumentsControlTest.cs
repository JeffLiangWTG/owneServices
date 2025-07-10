using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(DefaultNumberOfSupportingDocumentsControl))]
	class DefaultNumberOfSupportingDocumentsControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DefaultNumberOfSupportingDocumentsCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DefaultNumberOfSupportingDocumentsControl)control).DefaultNumberOfSupportingDocumentsGrid.ReadOnly;
		}
	}
}
