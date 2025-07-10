using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlMonthlyUsage))]
	internal class StlMonthlyUsageTest : NonPersistentBusinessObjectTestCase
	{
		internal static StlMonthlyUsage CreateMonthlyUsage(LicenceHeader lic, ZDateTime periodStart, ZDateTime? siteLiveDate = null)
		{
			var delivery = lic.Company.InvoiceDeliveries.FirstOrDefault();
			var invoicedCompany = delivery == null || delivery.L9_OH_InvoiceTo.IsEmpty ? lic.Company
				: delivery.InvoiceTo.LicCompany;
			var databaseUsage = new DatabaseUsage(new UsageOwnerDelivery(new UsageOwner(lic), delivery, invoicedCompany),
				System.Array.Empty<Usage>(), null, null, lic.Database.LicenceSettings.ToArray(), siteLiveDate ?? ZDateTime.Empty, true, new Dictionary<string, PriceList>(), null);
			return new StlMonthlyUsage(lic.Factory, databaseUsage, periodStart, new InvoiceGroup(delivery, invoicedCompany?.LC_OH ?? ZGuid.Empty),
				new SystemBill.TaxGroup(delivery));
		}

		public void TestTotalUsedLicenceUnits()
		{
			var stlMonthlyUsage = GetNewBusinessObject() as StlMonthlyUsage;
			var usageLine = new UsageLine(Factory);
			usageLine.TotalUnitCount = 2;
			stlMonthlyUsage.AddUsageLine(usageLine);
			usageLine.SetLicenceUnits(100);
			AssertEquals(200m, stlMonthlyUsage.TotalUsedLicenceUnits);
			usageLine.SetLicenceUnits(200);
			AssertEquals(400m, stlMonthlyUsage.TotalUsedLicenceUnits);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			return CreateMonthlyUsage(lic, new ZDateTime(2015, 9, 1));
		}

		#endregion
	}
}
