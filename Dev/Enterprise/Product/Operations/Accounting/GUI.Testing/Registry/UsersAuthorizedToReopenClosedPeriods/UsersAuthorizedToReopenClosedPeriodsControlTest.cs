using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(UsersAuthorizedToReopenClosedPeriodsControl))]
	class UsersAuthorizedToReopenClosedPeriodsControlTest : Enterprise.Registry.GUI.Testing.RegistryBusinessObjectTemplateZUserControlTestCase
	{
		public override void TestBoundListsAreNotLoadedOnAccess()
		{
			Assert(true);
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new UsersAuthorizedToReopenClosedPeriodsCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((UsersAuthorizedToReopenClosedPeriodsControl)control).UsersAuthorizedToReopenClosedPeriodsGrid.ReadOnly;
		}
	}
}
