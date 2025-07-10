using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(BulkAddDiscountBizO))]
	public class BulkAddDiscountBizOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateAndSave()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.CreateAndLoadLicenceForOrg();
			org1.LicCompany.LicEnterprise.LE_EnterpriseCode = "AAA";

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.CreateAndLoadLicenceForOrg();
			org2.LicCompany.LicEnterprise.LE_EnterpriseCode = "BBB";

			Factory.Save();

			var bizO = new BulkAddDiscountBizO(new BusinessObject[] { org1, org2 });
			var newDiscount1 = bizO.NewDiscountCollection.AddNew();

			var result = bizO.ValidateAndSave();
			AssertEquals("Has validation error", false, result);

			var loadedOrg1 = bizO.Factory.Load<EDIOrgHeader>(org1.PK);
			var loadedOrg2 = bizO.Factory.Load<EDIOrgHeader>(org2.PK);

			AssertEquals("New discount is added to org", 1, loadedOrg1.LicCompany.SelfBilling.BillingDiscounts.Count);
			AssertEquals("New discount is added to org", 1, loadedOrg2.LicCompany.SelfBilling.BillingDiscounts.Count);

			var newDiscount2 = bizO.NewDiscountCollection.AddNew();
			result = bizO.ValidateAndSave();
			AssertEquals("Has validation error", false, result);
			AssertEquals("New discount is added to org", 2, loadedOrg1.LicCompany.SelfBilling.BillingDiscounts.Count);
			AssertEquals("New discount is added to org", 2, loadedOrg2.LicCompany.SelfBilling.BillingDiscounts.Count);

			bizO.NewDiscountCollection.RemoveAndDelete(newDiscount1);
			result = bizO.ValidateAndSave();
			AssertEquals("Has validation error", false, result);
			AssertEquals("Has validation error", true, newDiscount2.SystemCodeInfo.HasErrors());
			AssertEquals("Has validation error", true, newDiscount2.DiscountTypeInfo.HasErrors());
			AssertEquals("Has validation error", true, newDiscount2.BreakUnitsInfo.HasErrors());
			AssertEquals("Has validation error", true, newDiscount2.DiscountInfo.HasErrors());
			AssertEquals("New discount is removed from org", 1, loadedOrg1.LicCompany.SelfBilling.BillingDiscounts.Count);
			AssertEquals("New discount is removed from org", 1, loadedOrg2.LicCompany.SelfBilling.BillingDiscounts.Count);
			AssertEquals("New discount is not yet saved", false, loadedOrg1.LicCompany.SelfBilling.BillingDiscounts[0].IsInDatabase);
			AssertEquals("New discount is not yet saved", false, loadedOrg2.LicCompany.SelfBilling.BillingDiscounts[0].IsInDatabase);

			newDiscount2.SystemCode = BillingConstants.BillingSystem.ODM;
			newDiscount2.DiscountType = BillingConstants.DiscountType.Special;
			newDiscount2.BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			newDiscount2.Discount = 15.0;
			newDiscount2.Description = "Special discount";

			result = bizO.ValidateAndSave();
			AssertEquals(true, result);
			AssertEquals(1, loadedOrg1.LicCompany.SelfBilling.BillingDiscounts.Count);
			AssertEquals(1, loadedOrg2.LicCompany.SelfBilling.BillingDiscounts.Count);
			AssertEquals("New discount is saved", true, loadedOrg1.LicCompany.SelfBilling.BillingDiscounts[0].IsInDatabase);
			AssertEquals("New discount is saved", true, loadedOrg2.LicCompany.SelfBilling.BillingDiscounts[0].IsInDatabase);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BulkAddDiscountBizO(System.Array.Empty<BusinessObject>());
		}
	}
}
