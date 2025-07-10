using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class SupplierBuyerLinkInvoiceChargesPopulatorTest : TestCaseWithFactory
	{
		public void TestPopulateCharges_NoExistingCharge_ExpJob()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceHeader.Charges.RemoveAndDeleteAll();
			AssertEquals("To make sure InvoiceHeader has no InvoiceCharge before populating.", false, invoiceHeader.Charges.Count > 0);
			invoiceHeader.PopulateCharges();
			CombineAssertions(() =>
			{
				AssertEquals("InvoiceHeader should have populated charges.", 1, invoiceHeader.Charges.Count);
				var createdCharge = invoiceHeader.Charges[0];
				AssertEquals("Created InvoiceCharge should have correct Type.", UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, createdCharge.J7_ChargeType);
				AssertEquals("Created InvoiceCharge should be System.", true, createdCharge.J7_IsCalculated);
				AssertEquals("Created InvoiceCharge should have correct Percentage.", 2.33m, createdCharge.J7_Percentage);
			});
		}

		public void TestPopulateCharges_NoExistingCharge_ImpJob()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.Charges.RemoveAndDeleteAll();
			AssertEquals("To make sure InvoiceHeader has no InvoiceCharge before populating.", false, invoiceHeader.Charges.Count > 0);
			invoiceHeader.PopulateCharges();
			CombineAssertions(() =>
			{
				var invoiceHeaderCharges = invoiceHeader.Charges.Cast<InvoiceCharge>();
				AssertEquals("InvoiceHeader should have populated charges.", 2, invoiceHeaderCharges.Count());

				var createdBcmCharge = invoiceHeaderCharges.FirstOrDefault(c => c.J7_ChargeType == UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge);
				AssertNotNull("Should have created a BCM charge.", createdBcmCharge);
				AssertEquals("Created BCM InvoiceCharge should be System.", true, createdBcmCharge.J7_IsCalculated);
				AssertEquals("Created BCM InvoiceCharge should have correct Percentage.", 1.22m, createdBcmCharge.J7_Percentage);
				AssertEquals("Created BCM InvoiceCharge should be included in Invoice.", false, createdBcmCharge.J7_IsNotIncludedInInvoice);
				AssertEquals("Created BCM InvoiceCharge should be included in Invoice lines.", true, createdBcmCharge.J7_IsIncludedInITOT);

				var createdRlfCharge = invoiceHeaderCharges.FirstOrDefault(c => c.J7_ChargeType == UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge);
				AssertNotNull("Should have created a RLF charge.", createdRlfCharge);
				AssertEquals("Created RLF InvoiceCharge should be System.", true, createdRlfCharge.J7_IsCalculated);
				AssertEquals("Created RLF InvoiceCharge should have correct Percentage.", 2.33m, createdRlfCharge.J7_Percentage);
			});
		}

		public void TestPopulateCharges_UpdateExistingSystemCalculatedCharges()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.Charges.RemoveAndDeleteAll();
			var existingBcmCharge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge);
			existingBcmCharge.J7_Percentage = 5.66m;
			existingBcmCharge.J7_IsCalculated = true;
			var existingRlfCharge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge);
			existingRlfCharge.J7_Percentage = 7.88m;
			existingRlfCharge.J7_IsCalculated = true;

			CombineAssertions("To make sure exisitng charges have empty Percentages.", () =>
			{
				AssertEquals("Bcm Charge", 5.66m, existingBcmCharge.J7_Percentage);
				AssertEquals("Rlf Charge", 7.88m, existingRlfCharge.J7_Percentage);
			});

			invoiceHeader.PopulateCharges();
			CombineAssertions(() =>
			{
				AssertEquals("No charge should be added or removed.", true, invoiceHeader.Charges.Count == 2);
				AssertEquals("No charge should be added or removed, existingBcmCharge should still be in the collection.", true, invoiceHeader.Charges.Contains(existingBcmCharge));
				AssertEquals("No charge should be added or removed, existingRlfCharge should still be in the collection.", true, invoiceHeader.Charges.Contains(existingRlfCharge));
				AssertEquals("BCM InvoiceCharge should have correct Percentage.", 1.22m, existingBcmCharge.J7_Percentage);
				AssertEquals("RLF InvoiceCharge should have correct Percentage.", 2.33m, existingRlfCharge.J7_Percentage);
			});
		}

		public void TestPopulateCharges_RemoveExistingSystemCalculatedCharges()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceHeader.Charges.RemoveAndDeleteAll();
			var existingBcmCharge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge);
			existingBcmCharge.J7_Percentage = 5.66m;
			existingBcmCharge.J7_IsCalculated = true;
			var existingRlfCharge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge);
			existingRlfCharge.J7_Percentage = 7.88m;
			existingRlfCharge.J7_IsCalculated = true;
			CombineAssertions("To make sure exisitng charges have valid Percentages before processing.", () =>
			{
				AssertEquals("Bcm Charge", 5.66m, existingBcmCharge.J7_Percentage);
				AssertEquals("Rlf Charge", 7.88m, existingRlfCharge.J7_Percentage);
			});

			link.OL_BuyingCommissionPercentage = 5.66m;
			link.OL_RoyaltyPercentage = 0;
			invoiceHeader.PopulateCharges();

			AssertEquals("Both the 2 Charges should be removed. BCM should be removed for no rule applied(EXP), RLF should be removed for empty Percantage from rule.", false, invoiceHeader.Charges.Count > 0);
		}

		public void TestPopulateCharges_RemoveExistingSystemCalculatedCharges_EmptyLink()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.Charges.RemoveAndDeleteAll();
			var existingBcmCharge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge);
			existingBcmCharge.J7_Percentage = 5.66m;
			existingBcmCharge.J7_IsCalculated = true;
			var existingRlfCharge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge);
			existingRlfCharge.J7_Percentage = 7.88m;
			existingRlfCharge.J7_IsCalculated = true;
			var existingCBRCharge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge);
			existingCBRCharge.J7_Percentage = 9.99m;
			existingCBRCharge.J7_IsCalculated = true;
			var existingNoneSystemCalculatedCharge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge);
			existingNoneSystemCalculatedCharge.J7_Percentage = 1.22m;

			var newSupplier = Factory.New<OrgHeader>();
			invoiceHeader.JZ_OH_Supplier = newSupplier.PK;
			AssertSame(
				"All SystemCalculated charges should have been deleted for no Link is found. The only none-system calculated charge should still in the collection.",
				existingNoneSystemCalculatedCharge,
				invoiceHeader.Charges[0]
			);
		}

		public void TestPopulateCharges_NotChangeNoneSystemCalculatedCharges()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.Charges.RemoveAndDeleteAll();
			var existingBcmCharge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge);
			existingBcmCharge.J7_Percentage = 5.66m;
			existingBcmCharge.J7_IsCalculated = false;
			var existingRlfCharge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge);
			existingRlfCharge.J7_Percentage = 7.88m;
			existingRlfCharge.J7_IsCalculated = false;

			CombineAssertions("To make sure exisitng charges have valid Percentages before processing.", () =>
			{
				AssertEquals("Bcm Charge", 5.66m, existingBcmCharge.J7_Percentage);
				AssertEquals("Rlf Charge", 7.88m, existingRlfCharge.J7_Percentage);
			});

			link.OL_RoyaltyPercentage = link.OL_BuyingCommissionPercentage = 3.44m;
			invoiceHeader.PopulateCharges();

			CombineAssertions("Should NOT change non-systemcalculated charges. Being non-systemcalculated means the user has chosen to config this type of charge manually.", () =>
			{
				AssertEquals("No charge should be added or removed.", 2, invoiceHeader.Charges.Count);
				AssertEquals("Bcm Charge J7_Percentage", 5.66m, existingBcmCharge.J7_Percentage);
				AssertEquals("Bcm Charge J7_IsCalculated", false, existingBcmCharge.J7_IsCalculated);
				AssertEquals("Rlf Charge J7_Percentage", 7.88m, existingRlfCharge.J7_Percentage);
				AssertEquals("Rlf Charge J7_IsCalculated", false, existingRlfCharge.J7_IsCalculated);
			});
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		OrgSupplierBuyerLink link;

		protected override void SetUp()
		{
			base.SetUp();

			link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			link.OL_RN_NKImporterCountry = GlbBranch.CurrentBranch.Country.Code;
			link.OL_BuyingCommissionPercentage = 1.22m;
			link.OL_RoyaltyPercentage = 2.33m;
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = link.OL_OH_Supplier;
			declaration.JE_OH_Buyer = link.OL_OH_Buyer;
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			invoiceHeader = declaration.Invoices.AddNew();
		}
	}
}
