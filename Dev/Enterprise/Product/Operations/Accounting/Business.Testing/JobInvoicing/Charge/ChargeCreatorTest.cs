using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class ChargeCreatorTest : TestCaseWithFactory
	{
		public void TestUsedAsDependency()
		{
			var invoice = Factory.New<APInvoice>();
			AssertType<ChargeCreator>(invoice.ChargeCreator_ExposedForTestOnly);
			AssertType<ChargeCreator>(new InvoicingBaseTaxRecordParent(invoice).ChargeCreator_ExposedForTestOnly);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestCreateChargeFromJobRelatedRevenueLine()
		{
			AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "AR001", TestObjectCreator.USD, 0.5M, TestObjectCreator.TestOrganisation);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 28.5M, taxRate: TestObjectCreator.GST1);
			var expectedDesc = "Some desc";
			line.AL_Desc = expectedDesc;
			var expectedSupplyType = "LOC";
			line.AL_SupplyType = expectedSupplyType;
			var expectedTaxDate = ZDate.Today.AddDays(3);
			line.AL_TaxDate = expectedTaxDate;
			line.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
			var overridenGovtChargeCode = "OVERRIDDEN";
			line.AL_GovtChargeCode = overridenGovtChargeCode;
			line.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
			line.AL_GE = TestObjectCreator.NonCurrentDepartment.PK;
			line.AL_AW = TestObjectCreator.WHT1.PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_GB_TaxBranch = testObjectCreator.NonCurrentBranch.PK;

			var nonJobRelatedLine = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.NonAccrualChargeCode.PK, 100);

			AssertEquals("Precondition: job.Charges.Count", 0, job.Charges.Count);

			IChargeCreator creator = new ChargeCreator();
			var createdCharge = creator.CreateChargeFromJobRelatedRevenueLine(line, invoice, true);

			job.Charges.Load();
			AssertEquals("job.Charges.Count", 1, job.Charges.Count);
			AssertEquals("createdCharge and charge added to job are the same", createdCharge, job.Charges[0]);
			AssertEquals("JR_AL_ARLine", line.PK, createdCharge.JR_AL_ARLine);
			AssertEquals("JR_Desc", expectedDesc, createdCharge.JR_Desc);
			AssertEquals("JR_GB", TestObjectCreator.NonCurrentBranch.PK, createdCharge.JR_GB);
			AssertEquals("JR_GE", TestObjectCreator.NonCurrentDepartment.PK, createdCharge.JR_GE);
			AssertEquals("JR_RX_NKSellCurrency", "USD", createdCharge.JR_RX_NKSellCurrency);
			AssertEquals("JR_OSSellExRate", 0.5m, createdCharge.JR_OSSellExRate);
			AssertEquals("JR_RX_NKCostCurrency", "USD", createdCharge.JR_RX_NKCostCurrency);
			AssertEquals("JR_OSCostExRate", 0.5m, createdCharge.JR_OSCostExRate);
			AssertEquals("JR_AW_SellWHTRate", TestObjectCreator.WHT1.PK, createdCharge.JR_AW_SellWHTRate);
			AssertEquals("JR_AT_SellGSTRate", TestObjectCreator.GST1.PK, createdCharge.JR_AT_SellGSTRate);
			AssertEquals("JR_SellTaxDate", expectedTaxDate, createdCharge.JR_SellTaxDate);
			AssertEquals("JR_A9_SellVATClass", TestObjectCreator.TaxMsg1.PK, createdCharge.JR_A9_SellVATClass);
			AssertEquals("JR_OH_SellAccount", TestObjectCreator.TestOrganisation.PK, createdCharge.JR_OH_SellAccount);
			AssertEquals("JR_OH_CostAccount", ZGuid.Empty, createdCharge.JR_OH_CostAccount);
			AssertEquals("JR_OC_SellInvoiceContact", ZGuid.Empty, createdCharge.JR_OC_SellInvoiceContact);
			AssertEquals("JR_OA_SellInvoiceAddress", ZGuid.Empty, createdCharge.JR_OA_SellInvoiceAddress);
			AssertEquals("JR_SellGovtChargeCode", overridenGovtChargeCode, createdCharge.JR_SellGovtChargeCode);
			AssertEquals("JR_OSSellAmt", -10m, createdCharge.JR_OSSellAmt);
			AssertEquals("JR_OSSellGSTAmt_Calc", -1m, createdCharge.JR_OSSellGSTAmt_Calc);
			AssertEquals("JR_LocalSellAmt", -20m, createdCharge.JR_LocalSellAmt);
			AssertEquals("JR_Calc_LocalSellTaxAmt", -2m, createdCharge.JR_Calc_LocalSellTaxAmt);
			AssertEquals("JR_InvoiceType", invoice.TransactionCategory, createdCharge.JR_InvoiceType);
			AssertEquals("JR_SellSupplyType", expectedSupplyType, createdCharge.JR_SellSupplyType);
			AssertEquals("JR_GB_SellTaxBranch", testObjectCreator.NonCurrentBranch.PK, createdCharge.JR_GB_SellTaxBranch);

			createdCharge = creator.CreateChargeFromJobRelatedRevenueLine(nonJobRelatedLine, invoice, true);

			AssertNull(createdCharge);
			AssertEquals("job.Charges.Count after passing nonJobRelatedLine", 1, job.Charges.Count);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestInvoiceTypeOfCreatedCharge()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "AR001", TestObjectCreator.USD, 0.5M, TestObjectCreator.TestOrganisation);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 28.5M, taxRate: TestObjectCreator.GST1);

			AssertEquals("Precondition: job.Charges.Count", 0, job.Charges.Count);
			AssertEquals("invoice.TransactionCategory", invoice.TransactionCategory, "CUR");

			IChargeCreator creator = new ChargeCreator();
			var createdChargeFromTaxRecoveryLine = creator.CreateChargeFromJobRelatedRevenueLine(line, invoice, true);

			AssertEquals("JR_InvoiceType from invoice", invoice.TransactionCategory, createdChargeFromTaxRecoveryLine.JR_InvoiceType);

			var createdChargeFromNonTaxRecoveryLine = creator.CreateChargeFromJobRelatedRevenueLine(line, invoice, false);
			AssertNotEquals("JR_InvoiceType from registry", invoice.TransactionCategory, createdChargeFromNonTaxRecoveryLine.JR_InvoiceType);
			AssertEquals("JR_InvoiceType", createdChargeFromNonTaxRecoveryLine.JR_InvoiceType, "FIN");
		}

		public void TestAddressAndContactDetailsCopied()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.USD, 0.5M, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 28.5M, taxRate: TestObjectCreator.VATSPV);

			var address = TestObjectCreator.AALSHI.Addresses.AddNew();
			address.OA_Code = address.OA_Address1 = "Hello";
			var contact = TestObjectCreator.AALSHI.Contacts.AddNew();
			contact.OC_ContactName = "Hello";

			invoice.AH_OA_InvoiceAddressOverride = address.PK;
			invoice.AH_OC_InvoiceContactOverride = contact.PK;

			IChargeCreator creator = new ChargeCreator();
			var createdCharge = creator.CreateChargeFromJobRelatedRevenueLine(line, invoice, true);

			AssertEquals("JR_OC_SellInvoiceContact", address.PK, createdCharge.JR_OA_SellInvoiceAddress);
			AssertEquals("JR_OA_SellInvoiceAddress", contact.PK, createdCharge.JR_OC_SellInvoiceContact);
		}

		public void TestCreateChargeFromJobRelatedRevenueLine_SuspendJobChargeCalculationContext()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			service.ClearServiceCache();

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			newCompany.GC_Code = "DCN";
			newCompany.GC_IsReciprocal = true;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "BJN";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				TestObjectCreator.CreateExchangeRate(testObjectCreator.AED, "BUY", 0.937476611m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(10));

				var shipment = testObjectCreator.CreateShipment("S0001");
				var job = testObjectCreator.CreateJob(shipment, false);
				var invoice = testObjectCreator.CreateInvoice(typeof(ARInvoice), "AR0001", testObjectCreator.AED, 0.937476611m, testObjectCreator.AALSHI);
				invoice.AH_TransactionCategory = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				var line = testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AED, 0.937476611m, 14963.99M, 0m, testObjectCreator.CC3.PK);
				line.AL_JH = job.PK;

				AssertEquals("Precondition: AL_ExchangeRate", 0.937477M, line.AL_ExchangeRate);
				AssertEquals("Precondition: AL_LineAmount", 14028.40M, line.AL_LineAmount);
				AssertContains("Precondition: No additional info collected for the JobChargeOSSellAmtNotEqualRelatedLineOSAmount key", "There is no data collected for this PK", service.GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellAmtNotEqualRelatedLineOSAmount));
				AssertContains("Precondition: No additional info collected for the JobChargeLocalSellAmtNotEqualRelatedLineAmount key", "There is no data collected for this PK", service.GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedLineAmount));

				line.AL_LineAmount = 14963.40m;
				line.CalculateHighPrecisionExchangeRate();
				AssertEquals("AL_ExchangeRates has High Precision Exchange Rate", 0.999960572m, line.AL_ExchangeRate);

				IChargeCreator creator = new ChargeCreator();
				var createdCharge = creator.CreateChargeFromJobRelatedRevenueLine(line, invoice, true);

				AssertContains("No additional info collected for the JobChargeOSSellAmtNotEqualRelatedLineOSAmount key", "There is no data collected for this PK", service.GetInfo(createdCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellAmtNotEqualRelatedLineOSAmount));
				AssertContains("No additional info collected for the JobChargeLocalSellAmtNotEqualRelatedLineAmount key", "There is no data collected for this PK", service.GetInfo(createdCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedLineAmount));
				AssertEquals("JR_OSSellAmt + JR_OSSellGSTAmt_Calc should be the same as AL_OSAmount", line.AL_OSAmount, createdCharge.JR_OSSellAmt + createdCharge.JR_OSSellGSTAmt_Calc);
				AssertEquals("JR_LocalSellAmt should be the same as  AL_LineAmount", line.AL_LineAmount, createdCharge.JR_LocalSellAmt);
				AssertEquals("JR_OSSellExRate", 0.999961m, createdCharge.JR_OSSellExRate);
				//'AL_ExchangeRate' should store high precision exchange rate but 'JR_OSSellExRate' should only store 4 or 6 digits, so they might be different
				AssertNotEquals("JR_OSSellExRate and AL_ExchangeRate are different because of rounding", line.AL_ExchangeRate, createdCharge.JR_OSSellExRate);
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
