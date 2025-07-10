using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlBillingMilestoneSetter))]
	public class StlBillingMilestoneSetterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new StlBillingMilestoneSetter();
		}

		public void TestValidations()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT");
			var db = lic.Database;
			Factory.Save();

			var obj = new StlBillingMilestoneSetter();
			obj.RunPreSaveValidation();

			CombineAssertions(() =>
			{
				AssertHasError(obj.BillingPeriodInfo, "Invalid input. Please enter the date in the format yyyyMM (e.g., 202402).");
				AssertHasError(obj.DatabasePkInfo, "Please enter a Database.");

				obj.BillingPeriod = 200013;
				obj.DatabasePk = ZGuid.NewZGuid();
				AssertHasError(obj.BillingPeriodInfo, "Invalid input. Please enter the date in the format yyyyMM (e.g., 202402).");
				AssertHasError(obj.DatabasePkInfo, "Enter a valid Database.");

				obj.BillingPeriod = 202501;
				obj.DatabasePk = db.PK;
				AssertNoErrors(obj.BillingPeriodInfo);
				AssertNoErrors(obj.DatabasePkInfo);
			});
		}

		[TestDate(2025, 1, 15)]
		public void TestSetMilestone()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN2");
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertSetMilestone(190015, ZGuid.NewZGuid(), "Error: The specified milestone could not be found.");

				AssertSetMilestone(202501, lic1.Database.PK, "Error: The specified milestone could not be found.");

				var stl1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "STL", new ZDateTime(2025, 1, 1), lic1, 1);
				Factory.Save();
				AssertSetMilestone(202501, lic1.Database.PK, "Error: The milestone is complete.");

				stl1.U1_UnitCount = 15;
				var usr = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2025, 1, 1), lic1, 100);
				usr.U1_AH_Invoice = invoice.PK;
				Factory.Save();
				AssertSetMilestone(202501, lic1.Database.PK, "Error: Billing for this client for the specified period has already been completed.");

				usr.U1_AH_Invoice = ZGuid.Empty;
				Factory.Save();
				AssertSetMilestone(202501, lic1.Database.PK, "The milestone was successfully set.");

				var newFactory = new BusinessObjectFactory();
				AssertEquals(1m, newFactory.Load<ClientChargeableUsage>(stl1.PK).U1_UnitCount);
				AssertEquals(100m, newFactory.Load<ClientChargeableUsage>(usr.PK).U1_UnitCount);
			});
		}

		void AssertSetMilestone(ZInt period, ZGuid databasePk, string expectedLog)
		{
			var obj = new StlBillingMilestoneSetter() { BillingPeriod = period, DatabasePk = databasePk };
			var simpleLogger = new SimpleLogger();
			obj.SetMilestone(simpleLogger);
			AssertEquals(expectedLog, simpleLogger.ToString().TrimEnd('\r', '\n'));
		}
	}
}
