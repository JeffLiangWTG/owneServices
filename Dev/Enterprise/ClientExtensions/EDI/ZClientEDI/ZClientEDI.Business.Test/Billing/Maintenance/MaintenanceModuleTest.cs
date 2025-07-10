using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance.Test
{
	[TestedType(typeof(MaintenanceModule))]
	internal class MaintenanceModuleTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUserCount()
		{
			var module = new MaintenanceModule(null, "", null);
			AssertEquals(0, module.UserCount);

			module.UserCount = 11;
			AssertEquals(11, module.UserCount);
		}

		public void TestUnitPrice()
		{
			var module = new MaintenanceModule(null, "", null);
			AssertEquals(ZDecimal.Zero, module.UnitPrice);

			var priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.L7_Price = 11m;
			module = new MaintenanceModule(null, "COR", priceItem);
			AssertEquals(11m, module.UnitPrice);
		}

		public void TestTotalPrice()
		{
			var module = new MaintenanceModule(null, "", null);
			AssertEquals(ZDecimal.Zero, module.TotalPrice);

			var priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.L7_Price = 11m;
			module = new MaintenanceModule(null, "COR", priceItem);
			AssertEquals(0m, module.TotalPrice);

			module.UserCount = 3;
			AssertEquals(3 * 11m, module.TotalPrice);
		}

		public void TestOldSeatMaintenance()
		{
			MaintenancePercentagesForTest percentages = new MaintenancePercentagesForTest(15, 30);

			var priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.L7_Price = 50m;

			var module = new MaintenanceModule(percentages, "COR", priceItem);
			module.OldUserCount = 3;
			module.UserCount = 4;

			AssertEquals(50m * 3 * 0.15m, module.OldSeatMaintenance);

			module.OldUserCount = 1;
			percentages.OldSeatPercent = 16m;
			priceItem.L7_Price = 60m;
			AssertEquals(60m * 1 * 0.16m, module.OldSeatMaintenance);
		}

		public void TestNewSeatMaintenance()
		{
			MaintenancePercentagesForTest percentages = new MaintenancePercentagesForTest(15, 30);

			var priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.L7_Price = 50m;

			var module = new MaintenanceModule(percentages, "COR", priceItem);
			module.OldUserCount = 3;
			module.UserCount = 4;

			AssertEquals(50m * 1 * 0.30m, module.NewSeatMaintenance);

			module.OldUserCount = 1;
			percentages.NewSeatPercent = 16m;
			priceItem.L7_Price = 60m;
			AssertEquals(60m * 3 * 0.16m, module.NewSeatMaintenance);
		}

		public void TestTotalMaintenance()
		{
			MaintenancePercentagesForTest percentages = new MaintenancePercentagesForTest(15, 30);

			var priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.L7_Price = 50m;

			var module = new MaintenanceModule(percentages, "COR", priceItem);
			module.OldUserCount = 3;
			module.UserCount = 4;

			AssertEquals(module.OldSeatMaintenance + module.NewSeatMaintenance, module.TotalMaintenance);

			module.OldUserCount = 1;
			percentages.NewSeatPercent = 16m;
			priceItem.L7_Price = 60m;
			AssertEquals(module.OldSeatMaintenance + module.NewSeatMaintenance, module.TotalMaintenance);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MaintenanceModule();
		}

		class MaintenancePercentagesForTest : IMaintenancePercentages
		{
			public MaintenancePercentagesForTest(decimal oldSeatPercent = 0, decimal newSeatPercent = 0)
			{
				OldSeatPercent = oldSeatPercent;
				NewSeatPercent = newSeatPercent;
			}

			public ZDecimal OldSeatPercent { get; set; }
			public ZDecimal NewSeatPercent { get; set; }
		}

		#endregion
	}
}
