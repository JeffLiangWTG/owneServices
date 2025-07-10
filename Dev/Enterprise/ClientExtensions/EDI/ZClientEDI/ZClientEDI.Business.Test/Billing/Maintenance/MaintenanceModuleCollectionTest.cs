using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance.Test
{
	[TestedType(typeof(MaintenanceModuleCollection))]
	internal sealed class MaintenanceModuleCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MaintenanceModuleCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#region Implementation

		protected override MaintenanceModuleCollection GetCollectionToTest()
		{
			return new MaintenanceModuleCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MaintenanceModule();
		}

		#endregion
	}
}
