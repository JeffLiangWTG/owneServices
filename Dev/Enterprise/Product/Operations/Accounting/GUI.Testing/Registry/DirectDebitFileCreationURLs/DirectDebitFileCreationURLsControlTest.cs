using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(DirectDebitFileCreationURLsControl))]
	class DirectDebitFileCreationURLsControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		public override void TestBoundListsAreNotLoadedOnAccess()
		{
			Assert(true);
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new DirectDebitFileCreationURLCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DirectDebitFileCreationURLsControl)control).DirectDebitFileURLGrid_ForTestOnly.ReadOnly;
		}
	}
}
