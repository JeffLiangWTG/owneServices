using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiBillingTransactionFlattened))]
	public class EdiBillingTransactionFlattenedTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidations()
		{
			//ABC/PL0  ABC/PL1
			UsageBillingSettingsTest.SetupValidTestRegistry();
			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "AUS", "SYD");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN2", "AU2", "MEL");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "EN3", "AU3", "BRN");
			var lic4 = BillingTestHelper.CreateLicence(Factory, "EN4", "AU4", "ADL");
			lic1.Database.LD_Product = "ABC";
			lic2.Database.LD_Product = ProductTypes.Codes.CargoWiseOne;
			lic3.Database.LD_Product = ProductTypes.Codes.CargoWiseNext;
			lic4.Database.LD_Product = ProductTypes.Codes.CargoWise;
			var priceList = BillingTestHelper.CreatePriceList(lic1);
			priceList.L6_SystemCode = "PL0";
			BillingTestHelper.AddPriceItem(priceList, new UsageCodeKey("PL0", "P01"), "TRA", 100);
			BillingTestHelper.AddPriceItem(priceList, new UsageCodeKey("PL0", "P02"), "TRA", 200);
			Factory.Save();

			var transaction = new EdiBillingTransactionFlattened(Factory);

			void Assert(string expected)
			{
				transaction.RunPreSaveValidation();
				var notifications = string.Join("\r\n", transaction.Notifications.Select(x => $"{x.Message}").OrderBy(x => x));
				AssertEquals(expected, notifications);
			}

			Assert(@"Error - BillableCount: Please enter a Number of Billed Items.
Error - Category: Please enter a Category.
Error - ClientNumber: Please enter a Client Number.
Error - PriceItemCode: Please enter a Price Item Code.
Error - Reference1: Please enter a Reference 1.
Error - ReportingSource: Please enter a Reporting Source.
Error - ServiceOccuredUTC: Please enter a Service Occurred UTC.");

			transaction.Category = "123";
			transaction.PriceItemCode = "456";
			transaction.BillableCount = -1;
			transaction.ReportingSource = "789";
			transaction.ClientNumber = "AAA.CCC";
			transaction.Reference1 = "REF1";
			Assert(@"Error - BillableCount: Number of Billed Items cannot be negative.
Error - Category: Enter a valid Category.
Error - ClientNumber: Invalid Client Number.
Error - ReportingSource: Enter a valid Reporting Source.
Error - ServiceOccuredUTC: Please enter a Service Occurred UTC.");

			transaction.Category = "ABC";
			transaction.PriceItemCode = "PXX";
			transaction.BillableCount = 5;
			transaction.ReportingSource = "ABC";
			transaction.ClientNumber = lic2.Database.DatabaseId;
			transaction.ServiceOccuredUTC = ZDateTime.Today;
			transaction.Reference1 = "REF1";
			Assert(@"Error - ClientNumber: Invalid Client Number.
Error - PriceItemCode: Enter a valid Price Item Code.");

			transaction.Category = "ABC";
			transaction.PriceItemCode = "P01";
			transaction.BillableCount = 5;
			transaction.ReportingSource = "ABC";
			transaction.ClientNumber = lic1.Database.DatabaseId;
			transaction.ServiceOccuredUTC = ZDateTime.Today;
			transaction.Reference1 = "REF1";
			Assert("");

			transaction.Category = "ABC";
			transaction.PriceItemCode = "P01";
			transaction.BillableCount = 5;
			transaction.ReportingSource = "ABC";
			transaction.ClientNumber = "";
			transaction.ServiceOccuredUTC = ZDateTime.Today;
			transaction.Reference1 = "REF1";
			Assert("Error - ClientNumber: Please enter a Client Number.");

			transaction.Category = "ABC";
			transaction.PriceItemCode = "P01";
			transaction.BillableCount = 5;
			transaction.ReportingSource = "ABC";
			transaction.ClientNumber = $"{lic1.Database.DatabaseId}.CC1";
			transaction.ServiceOccuredUTC = ZDateTime.Today;
			transaction.Reference1 = "REF1";
			Assert("");
		}
	}
}
