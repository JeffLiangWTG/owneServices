using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicenceBillingDiscount))]
	internal class ClientLicenceBillingDiscountTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBooleanProperties()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();

			discount.L5_Type = BillingConstants.DiscountType.Volume;
			AssertBooleanProperties(discount, true, false, false, false, false, false, false, false);

			discount.L5_Type = BillingConstants.DiscountType.IncrementalVolume;
			AssertBooleanProperties(discount, false, true, false, false, false, false, false, false);

			discount.L5_Type = BillingConstants.DiscountType.Prepayment;
			AssertBooleanProperties(discount, false, false, true, false, false, false, false, false);

			discount.L5_Type = BillingConstants.DiscountType.Commitment;
			AssertBooleanProperties(discount, false, false, false, true, false, false, false, false);

			discount.L5_Type = BillingConstants.DiscountType.Special;
			AssertBooleanProperties(discount, false, false, false, false, true, false, false, false);

			discount.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			AssertBooleanProperties(discount, false, false, false, false, false, true, false, false);

			discount.L5_Type = BillingConstants.DiscountType.Capped;
			AssertBooleanProperties(discount, false, false, false, false, false, false, true, false);

			discount.L5_Type = BillingConstants.DiscountType.MinimumFee;
			AssertBooleanProperties(discount, false, false, false, false, false, false, false, true);
		}

		void AssertBooleanProperties(ClientLicenceBillingDiscount discount,
			bool isVolume, bool isIncrementalVolume, bool isPrepayment, bool isCommitment, bool isSpecial, bool isModuleSpecific, bool isCapped, bool isMinimumFee)
		{
			AssertEquals(isVolume, discount.IsVolume);
			AssertEquals(isIncrementalVolume, discount.IsIncrementalVolume);
			AssertEquals(isPrepayment, discount.IsPrepayment);
			AssertEquals(isCommitment, discount.IsCommitment);
			AssertEquals(isSpecial, discount.IsSpecial);
			AssertEquals(isModuleSpecific, discount.IsModuleSpecific);
			AssertEquals(isCapped, discount.IsCapped);
			AssertEquals(isMinimumFee, discount.IsMinimumFee);
		}

		public void TestCalculate()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_Discount = 20m;

			DiscountCalculation calculation = discount.Calculate(100m);
			AssertCalculation(calculation, 100m, 20m, "Special Discount: -20.00 (-20% * 100.00)", "Special Discount of 20%");

			discount.L5_Description = "Hello";
			calculation = discount.Calculate(100m);
			AssertCalculation(calculation, 100m, 20m, "Hello Special Discount: -20.00 (-20% * 100.00)", "Special Discount of 20%");

			calculation = discount.Calculate(100.33m);
			AssertCalculation(calculation, 100.33m, 20.07m, "Hello Special Discount: -20.07 (-20% * 100.33)", "Special Discount of 20%");
		}

		public void TestCalculatePrepayment()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_Type = BillingConstants.DiscountType.Prepayment;
			discount.L5_BreakAmount = 100m;
			discount.L5_Discount = 20m;

			DiscountCalculation calculation = discount.Calculate(50m);
			AssertCalculation(calculation, 100m, 0m, "Prepayment Minimum Spend: 100.00", "");

			calculation = discount.Calculate(100m);
			AssertCalculation(calculation, 100m, 0m, "Prepayment Minimum Spend: 100.00", "");

			calculation = discount.Calculate(160m);
			AssertCalculation(calculation, 160m, 12m, "Prepayment Discount: -12.00 (-20% * 60.00)", "Prepayment Discount of 20%");
		}

		public void TestCalculateCommitment()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			discount.L5_Type = BillingConstants.DiscountType.Commitment;
			discount.L5_BreakAmount = 100m;
			discount.L5_Discount = 20m;

			DiscountCalculation calculation = discount.Calculate(50m);
			AssertCalculation(calculation, 100m, 20m, "Commitment Discount: -20.00 (-20% * 100.00)", "Commitment Discount of 20%");

			calculation = discount.Calculate(100m);
			AssertCalculation(calculation, 100m, 20m, "Commitment Discount: -20.00 (-20% * 100.00)", "Commitment Discount of 20%");

			calculation = discount.Calculate(200m);
			AssertCalculation(calculation, 200m, 40m, "Commitment Discount: -40.00 (-20% * 200.00)", "Commitment Discount of 20%");

			discount.L5_Discount = 0m;
			calculation = discount.Calculate(50m);
			AssertCalculation(calculation, 100m, 0m, "Commitment Minimum Spend: 100.00", "");

			calculation = discount.Calculate(100m);
			AssertCalculation(calculation, 100m, 0m, "", "");

			calculation = discount.Calculate(200m);
			AssertCalculation(calculation, 200m, 0m, "", "");

			// Commitment in LicenceUnits
			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(anotherFactory, "AAA");
			ClientLicenceBillingDiscount discountWithOrg = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discountWithOrg.L5_BreakUnits = BillingConstants.DiscountBreakUnit.LicenceUnits;
			discountWithOrg.L5_Type = BillingConstants.DiscountType.Commitment;
			discountWithOrg.L5_Discount = 20m;
			discountWithOrg.L5_BreakAmount = 100m;
			discountWithOrg.L5_StartDate = new ZDate(2001, 1, 10);
			discountWithOrg.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			calculation = discountWithOrg.Calculate(10000m, 5.1234m, 50m);
			AssertCalculation(calculation, 512.3400m, 102.47m, "Commitment Discount: -102.47 (-20% * 512.34)", "Commitment Discount of 20%");
		}

		public void TestCalculateIncrementalVolume()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_Type = BillingConstants.DiscountType.IncrementalVolume;
			discount.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			discount.L5_Discount = 10m;
			discount.L5_Units = 100;

			DiscountCalculation calculation = discount.CalculateIncrementalVolume(100, 0.10m);
			AssertCalculation(calculation, 10m, 1m, "Incremental Volume Discount: 1.00 (100 Units @ 10%)", "Incremental Volume Discount");

			calculation = discount.CalculateIncrementalVolume(99, 0.10m);
			AssertCalculation(calculation, 9.9m, 0.99m, "Incremental Volume Discount: 0.99 (99 Units @ 10%)", "Incremental Volume Discount");
		}

		public void TestCalculateCapped()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_Type = BillingConstants.DiscountType.Capped;
			discount.L5_Discount = 20m;
			discount.L5_BreakAmount = 10m;

			DiscountCalculation calculation = discount.Calculate(100m);
			AssertCalculation(calculation, 50m, 10m, "Capped Discount: -10.00 (-20% * 50.00)", "Capped Discount of 20%");
			AssertEquals(calculation.DiscountAmount, discount.CappedDiscountAmount);

			discount.L5_BreakAmount = 50m;
			calculation = discount.Calculate(100m);
			AssertCalculation(calculation, 100m, 20m, "Capped Discount: -20.00 (-20% * 100.00)", "Capped Discount of 20%");
			AssertEquals(calculation.DiscountAmount, discount.CappedDiscountAmount);

			discount.L5_BreakAmount = 23406.38m;
			discount.L5_Discount = 7m;
			calculation = discount.Calculate(500000m);
			AssertCalculation(calculation, 334376.86m, 23406.38m, "Capped Discount: -23,406.38 (-7% * 334,376.86)", "Capped Discount of 7%");
			AssertEquals(calculation.DiscountAmount, discount.CappedDiscountAmount);
		}

		public void TestCalculateModuleSpecific()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			discount.L5_Discount = 20m;

			DiscountCalculation calculation = discount.CalculateModuleSpecific(100m, "    Hello world!");
			AssertCalculation(calculation, 100m, 20m, "Hello world! Module Specific Discount: -20.00 (-20% * 100.00)", "Module Specific Discount of 20%");
		}

		public void TestCalculateMinumumFee()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_Type = BillingConstants.DiscountType.MinimumFee;
			discount.L5_BreakAmount = 200m;
			discount.L5_Units = 100;

			DiscountCalculation calculation = discount.CalculateMinimumFee(0, 4m);
			AssertCalculation(calculation, 200m, 0m, "Minimum Fee: 200.00", "");

			calculation = discount.CalculateMinimumFee(100, 4m);
			AssertCalculation(calculation, 200m, 0m, "Minimum Fee: 200.00", "");

			calculation = discount.CalculateMinimumFee(150, 4m);
			AssertCalculation(calculation, 400m, 0m, "Minimum Fee: 200.00 + 50 Transactions @ 4.00 Per Transaction", "");

			discount.L5_Description = "Description";
			calculation = discount.CalculateMinimumFee(150, 4m);
			AssertCalculation(calculation, 400m, 0m, "Description Minimum Fee: 200.00 + 50 Transactions @ 4.00 Per Transaction", "");

			discount.L5_Type = "XXX";
			calculation = discount.CalculateMinimumFee(150, 4m);
			AssertCalculation(calculation, 0m, 0m, "", "");
		}

		public void TestCalculateOnDemandMinimumFee()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.L5_Type = BillingConstants.DiscountType.MinimumFee;
			discount.L5_BreakAmount = 600m;
			discount.L5_Units = 5;

			DiscountCalculation calculation = discount.CalculateOnDemandMinimumFee(0);
			AssertCalculation(calculation, 600m, 0m, "Minimum Fee: 600.00", "");

			calculation = discount.CalculateOnDemandMinimumFee(5);
			AssertCalculation(calculation, 600m, 0m, "Minimum Fee: 600.00", "");

			calculation = discount.CalculateOnDemandMinimumFee(7);
			AssertCalculation(calculation, 840m, 0m, "Minimum Fee: 600.00 + 2 Users @ 120.00 Per User", "");

			discount.L5_Description = "Description";
			calculation = discount.CalculateOnDemandMinimumFee(7);
			AssertCalculation(calculation, 840m, 0m, "Description Minimum Fee: 600.00 + 2 Users @ 120.00 Per User", "");

			discount.L5_Units = 0;
			discount.L5_Description = "";
			calculation = discount.CalculateOnDemandMinimumFee(999);
			AssertCalculation(calculation, 600m, 0m, "Minimum Fee: 600.00", "");

			discount.L5_Type = "XXX";
			calculation = discount.CalculateOnDemandMinimumFee(150);
			AssertCalculation(calculation, 0m, 0m, "", "");
		}

		public void TestCalculateSurcharge()
		{
			ClientLicenceBillingDiscount surcharge = Factory.New<ClientLicenceBillingDiscount>();
			surcharge.L5_Type = BillingConstants.DiscountType.Surcharge;
			surcharge.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			surcharge.L5_Description = "Legacy Product";
			surcharge.L5_Discount = -20m;

			DiscountCalculation calculation = surcharge.Calculate(100m);
			AssertCalculation(calculation, 100m, -20m, "Legacy Product Surcharge: 20.00 (20% * 100.00)", "Legacy Product Surcharge of 20%");

			calculation = surcharge.Calculate(100.33m);
			AssertCalculation(calculation, 100.33m, -20.07m, "Legacy Product Surcharge: 20.07 (20% * 100.33)", "Legacy Product Surcharge of 20%");
		}

		void AssertCalculation(DiscountCalculation calculation, ZDecimal amount, ZDecimal discountedAmount, ZString description, ZString invoiceDescription)
		{
			AssertEquals(amount, calculation.Amount);
			AssertEquals(discountedAmount, calculation.DiscountAmount);

			if (!description.IsEmpty)
			{
				AssertEquals(1, calculation.DiscountDescriptions.Count());
				AssertEquals(description, calculation.DiscountDescriptions.ToArray()[0]);
			}
			else
			{
				AssertEquals(0, calculation.DiscountDescriptions.Count());
			}

			if (!invoiceDescription.IsEmpty)
			{
				AssertEquals(1, calculation.InvoiceDescriptions.Count());
				AssertEquals(invoiceDescription, calculation.InvoiceDescriptions.ToArray()[0]);
			}
			else
			{
				AssertEquals(0, calculation.InvoiceDescriptions.Count());
			}
		}

		public void TestIsDateRangeMatched()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			ZDateTime startDate = new ZDateTime(2010, 01, 01);
			ZDateTime endDate = new ZDateTime(2010, 12, 01);
			AssertEquals(true, discount.IsDateRangeMatched(startDate, endDate));

			discount.L5_StartDate = startDate.AddMonths(-1);
			AssertEquals(true, discount.IsDateRangeMatched(startDate, endDate));

			discount.L5_StartDate = startDate.AddMonths(1);
			AssertEquals(false, discount.IsDateRangeMatched(startDate, endDate));

			discount.L5_StartDate = ZDateTime.Empty;
			discount.L5_EndDate = endDate.AddMonths(1);
			AssertEquals(true, discount.IsDateRangeMatched(startDate, endDate));

			discount.L5_EndDate = endDate.AddMonths(-1);
			AssertEquals(false, discount.IsDateRangeMatched(startDate, endDate));

			discount.L5_StartDate = startDate;
			discount.L5_EndDate = endDate;
			AssertEquals(true, discount.IsDateRangeMatched(startDate, endDate));

			discount.L5_StartDate = startDate.AddMonths(-1);
			discount.L5_EndDate = endDate.AddMonths(1);
			AssertEquals(true, discount.IsDateRangeMatched(startDate, endDate));

			discount.L5_StartDate = startDate;
			discount.L5_EndDate = endDate.AddMonths(-1);
			AssertEquals(false, discount.IsDateRangeMatched(startDate, endDate));

			discount.L5_StartDate = startDate.AddMonths(1);
			discount.L5_EndDate = endDate;
			AssertEquals(false, discount.IsDateRangeMatched(startDate, endDate));
		}

		[TestDate(2010, 07, 15)]
		public void TestInitDateRange()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			AssertEquals(ZDateTime.Empty, discount.L5_StartDate);
			AssertEquals(ZDateTime.Empty, discount.L5_EndDate);

			discount.L5_StartDate = ZDateTime.Today;
			discount.InitDateRange();
			AssertEquals("No changes", ZDateTime.Today, discount.L5_StartDate);
			AssertEquals("No changes", ZDateTime.Empty, discount.L5_EndDate);

			discount.L5_StartDate = ZDateTime.Empty;
			discount.L5_EndDate = ZDateTime.Today;
			discount.InitDateRange();
			AssertEquals("No changes", ZDateTime.Empty, discount.L5_StartDate);
			AssertEquals("No changes", ZDateTime.Today, discount.L5_EndDate);

			discount.L5_StartDate = ZDateTime.Empty;
			discount.L5_EndDate = ZDateTime.Empty;
			discount.L5_Duration = 0;
			discount.InitDateRange();
			AssertEquals("No changes", ZDateTime.Empty, discount.L5_StartDate);
			AssertEquals("No changes", ZDateTime.Empty, discount.L5_EndDate);

			discount.L5_Duration = 12;
			discount.InitDateRange();
			AssertEquals("New date", new ZDateTime(2010, 06, 01), discount.L5_StartDate);
			AssertEquals("New date", new ZDateTime(2011, 05, 31), discount.L5_EndDate);

			discount.InitDateRange();
			AssertEquals("No changes", new ZDateTime(2010, 06, 01), discount.L5_StartDate);
			AssertEquals("No changes", new ZDateTime(2011, 05, 31), discount.L5_EndDate);
		}

		public void TestCreateCappedDiscountUsageLog()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicenceBillingDiscount discount = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_Type = BillingConstants.DiscountType.Capped;
			discount.L5_Discount = 20m;
			discount.L5_BreakAmount = 100m;

			discount.Calculate(200m);
			AssertEquals(40m, discount.CappedDiscountAmount);

			ZGuid invoicePK = ZGuid.NewZGuid();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			discount.CreateCappedDiscountUsageLog(anotherFactory, invoicePK);
			anotherFactory.Save();

			ZQuery cappedDisountLogQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Capped Discount Used");
			StmALog[] cappedDisountLogs = organisation.Logs.Find(cappedDisountLogQuery);
			AssertEquals("No capped discount usage logs created -- organisation not in database", 0, cappedDisountLogs.Length);

			Factory.Save();

			discount.L5_Type = "XXX";
			discount.CreateCappedDiscountUsageLog(anotherFactory, invoicePK);
			anotherFactory.Save();

			cappedDisountLogs = organisation.Logs.Find(cappedDisountLogQuery);
			AssertEquals("No capped discount usage logs created -- discount is not capped", 0, cappedDisountLogs.Length);

			discount.L5_Type = BillingConstants.DiscountType.Capped;
			discount.CreateCappedDiscountUsageLog(anotherFactory, invoicePK);
			anotherFactory.Save();

			cappedDisountLogs = organisation.Logs.Find(cappedDisountLogQuery);
			AssertEquals("Capped discount usage log created", 1, cappedDisountLogs.Length);

			string expectedReference = string.Format(CultureInfo.CurrentCulture, "Capped Discount Used:40.00 InvoicePK:{0}", invoicePK);
			AssertEquals("Capped discount usage log", expectedReference, cappedDisountLogs[0].SL_Reference);

			discount.Calculate(100m);
			AssertEquals(20m, discount.CappedDiscountAmount);

			invoicePK = ZGuid.NewZGuid();
			discount.CreateCappedDiscountUsageLog(anotherFactory, invoicePK);
			anotherFactory.Save();

			cappedDisountLogs = organisation.Logs.Find(cappedDisountLogQuery);
			AssertEquals("Capped discount usage log created", 2, cappedDisountLogs.Length);

			expectedReference = string.Format(CultureInfo.CurrentCulture, "Capped Discount Used:20.00 InvoicePK:{0}", invoicePK);
			AssertEquals("Capped discount usage log", true, cappedDisountLogs.Any(x => x.SL_Reference == expectedReference));
		}

		// Disables critical validation "Missing Reversing Transaction for this canceled transaction".
		[SuspendCriticalValidation]
		public void TestCappedDiscount_L5_BreakAmount()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicenceBillingDiscount discount = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_Type = BillingConstants.DiscountType.Capped;
			discount.L5_Discount = 10m;
			discount.L5_BreakAmount = 1000m;

			ARInvoice randomInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			randomInvoice1.AH_OH = organisation.PK;

			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_OH = organisation.PK;

			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_OH = organisation.PK;

			ARInvoice invoice3 = Factory.NewWithValidTestData<ARInvoice>();
			invoice3.AH_OH = organisation.PK;

			Factory.Save();

			AssertEquals("Precondition", 1000m, discount.L5_BreakAmount);

			discount.Calculate(1000m);
			AssertEquals(100m, discount.CappedDiscountAmount);

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			discount.CreateCappedDiscountUsageLog(anotherFactory, invoice1.PK);
			anotherFactory.Save();

			AssertEquals("Break amount calculated", 900m, discount.L5_BreakAmount);

			discount.Calculate(2000m);
			AssertEquals(200m, discount.CappedDiscountAmount);
			discount.CreateCappedDiscountUsageLog(anotherFactory, invoice2.PK);

			discount.Calculate(3000m);
			AssertEquals(300m, discount.CappedDiscountAmount);
			discount.CreateCappedDiscountUsageLog(anotherFactory, invoice3.PK);

			anotherFactory.Save();

			AssertEquals("Break amount calculated", 400m, discount.L5_BreakAmount);

			invoice1.AH_IsCancelled = true;
			Factory.Save();
			AssertEquals("Break amount calculated, excluding cancelled invoice usage", 500m, discount.L5_BreakAmount);

			invoice3.AH_IsCancelled = true;
			Factory.Save();
			AssertEquals("Break amount calculated, excluding cancelled invoice usage", 800m, discount.L5_BreakAmount);

			discount.L5_BreakAmount = 2000m;
			AssertEquals("Setting break amount for capped discount adjusts base.L5_BreakAmount", 2000m, discount.L5_BreakAmount);
			Factory.Save();

			ClientLicenceBillingDiscount discountReloaded = anotherFactory.Load<ClientLicenceBillingDiscount>(discount.PK);
			AssertEquals("Break amount for capped discount -- with re-calculation", 2000m, discountReloaded.L5_BreakAmount);

			discountReloaded.L5_Type = "XXX";
			AssertEquals("Actual break amount value in database -- without re-calculation", 2200m, discountReloaded.L5_BreakAmount);
		}

		public void TestPropertiesReadOnly()
		{
			EDIOrgHeader testHeader = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicenceBillingDiscount billingDiscount = testHeader.LicCompany.SelfBilling.BillingDiscounts.AddNew();

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			foreach (ZPropertyInfo propertyInfo in billingDiscount.ZPropertyInfoHash)
			{
				if (propertyInfo.Name != billingDiscount.L5_ModuleCodeInfo.Name
					&& propertyInfo.Name != billingDiscount.L5_SubCodeInfo.Name
					&& propertyInfo.Name != billingDiscount.L5_UnitsInfo.Name)
				{
					AssertEquals(false, propertyInfo.ReadOnly);
				}
			}

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			foreach (ZPropertyInfo propertyInfo in billingDiscount.ZPropertyInfoHash)
			{
				AssertEquals(true, propertyInfo.ReadOnly);
			}
		}

		public void TestL5_ModuleCode_Readonly()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();

			string[] expectedNotReadonly = new string[]
			{
				BillingConstants.DiscountType.ModuleSpecific
			};

			AssertInfoReadonlyDueToType(discount, discount.L5_ModuleCodeInfo, expectedNotReadonly, false);
		}

		public void TestL5_BreakAmount_ReadOnly()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;

			string[] expectedReadonly = new string[]
			{
				BillingConstants.DiscountType.IncrementalVolume,
				BillingConstants.DiscountType.Special,
				BillingConstants.DiscountType.Surcharge,
				BillingConstants.DiscountType.ModuleSpecific,
				BillingConstants.DiscountType.WiseCloud
			};

			AssertInfoReadonlyDueToType(discount, discount.L5_BreakAmountInfo, expectedReadonly, true);

			discount.L5_SystemCode = BillingConstants.BillingSystem.ImporterSecurityFiling;

			expectedReadonly = new string[]
			{
				BillingConstants.DiscountType.Commitment,
				BillingConstants.DiscountType.IncrementalVolume,
				BillingConstants.DiscountType.Special,
				BillingConstants.DiscountType.Surcharge,
				BillingConstants.DiscountType.ModuleSpecific,
				BillingConstants.DiscountType.WiseCloud
			};

			AssertInfoReadonlyDueToType(discount, discount.L5_BreakAmountInfo, expectedReadonly, true);
		}

		void AssertInfoReadonlyDueToType(ClientLicenceBillingDiscount discount, ZPropertyInfo info, string[] expected, bool expectedState)
		{
			foreach (string type in expected)
			{
				discount.L5_Type = type;
				AssertEquals(info.Name + ".ReadOnly " + type, expectedState, info.ReadOnly);
			}

			foreach (ICodeDescription pair in BillingConstants.GetDiscountTypeList())
			{
				if (!expected.Contains(pair.Code))
				{
					discount.L5_Type = pair.Code;
					AssertEquals(info.Name + ".ReadOnly " + pair.Code, !expectedState, info.ReadOnly);
				}
			}
		}

		public void TestL5_Units_Readonly()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;

			string[] expectedNotReadonly = new string[]
			{
				BillingConstants.DiscountType.MinimumFee,
				BillingConstants.DiscountType.IncrementalVolume,
			};

			AssertInfoReadonlyDueToType(discount, discount.L5_UnitsInfo, expectedNotReadonly, false);

			discount.L5_SystemCode = BillingConstants.BillingSystem.ImporterSecurityFiling;

			expectedNotReadonly = new string[]
			{
				BillingConstants.DiscountType.Commitment,
				BillingConstants.DiscountType.MinimumFee,
				BillingConstants.DiscountType.IncrementalVolume,
			};

			AssertInfoReadonlyDueToType(discount, discount.L5_UnitsInfo, expectedNotReadonly, false);
		}

		public void TestL5_Discount_Readonly()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();

			string[] expectedReadonly = new string[]
			{
				BillingConstants.DiscountType.MinimumFee
			};

			AssertInfoReadonlyDueToType(discount, discount.L5_DiscountInfo, expectedReadonly, true);
		}

		public void TestLogChanges()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicenceBillingDiscount discount = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_Description = "DiscoLog12345";
			Factory.Save();
			AssertEquals("new pricelist not logged", 0, organisation.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "DiscoLog")).Length);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			EDIOrgHeader orgReloaded = factory2.Load<EDIOrgHeader>(organisation.PK);
			discount = orgReloaded.LicCompany.SelfBilling.BillingDiscounts[0];
			discount.L5_SystemCode = "AAA";
			discount.L5_Type = BillingConstants.DiscountType.Commitment;
			discount.L5_Discount = 20m;
			discount.L5_BreakAmount = 32000m;
			discount.L5_StartDate = new ZDateTime(2010, 3, 2);
			discount.L5_EndDate = new ZDateTime(2010, 9, 2);

			var discount2 = orgReloaded.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount2.L5_SystemCode = "BBB";
			discount2.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			discount2.L5_Description = "DiscoMod12345";
			discount2.L5_Discount = 11m;
			discount2.L5_BreakAmount = 1024m;
			discount2.L5_Units = 32;
			discount2.L5_ModuleCode = "COR";
			discount2.L5_StartDate = new ZDateTime(2010, 4, 2);

			factory2.Save();

			StmALog log = orgReloaded.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "DiscoLog"))[0];
			string expected = "Edit Discount DiscoLog 20% Sys=AAA Typ=COM Brk=32000 St=" + discount.L5_StartDate.ToShortDateString()
				+ " En=" + discount.L5_EndDate.ToShortDateString();
			AssertEquals(expected, log.SL_Reference);
			AssertEquals(Events.EditedARecordCode, log.SL_SE_NKEvent);

			StmALog log2 = orgReloaded.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "DiscoMod"))[0];
			expected = "Add Discount DiscoMod 11% Sys=BBB Typ=MOD Brk=1024 Unt=32 Mod=COR St=" + discount2.L5_StartDate.ToShortDateString();
			AssertEquals(expected, log2.SL_Reference);
			AssertEquals(Events.EditedARecordCode, log2.SL_SE_NKEvent);
		}

		public void TestL5_DiscountCode_ReadOnly()
		{
			var stdCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicenceBillingDiscount discount = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			AssertEquals("expect readonly for regular discount", true, discount.L5_DiscountCodeInfo.ReadOnly);

			discount = stdCompany.SelfBilling.BillingDiscounts.AddNew();
			AssertEquals("expect not readonly for discount on std company", false, discount.L5_DiscountCodeInfo.ReadOnly);
		}

		public void TestL5_SubCode_ReadOnly()
		{
			var company = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var discount = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			AssertEquals("expect readonly", true, discount.L5_SubCodeInfo.ReadOnly);

			discount.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			AssertEquals("expect not readonly for system code AirlineMessaging", false, discount.L5_SubCodeInfo.ReadOnly);

			discount.L5_SystemCode = BillingConstants.BillingSystem.DeniedPartyScreening;
			AssertEquals("expect readonly for system code DeniedPartyScreening", true, discount.L5_SubCodeInfo.ReadOnly);

			discount.L5_SystemCode = BillingConstants.BillingSystem.ClientMapping;
			AssertEquals("expect not readonly for system code ClientMapping", false, discount.L5_SubCodeInfo.ReadOnly);

			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			AssertEquals("expect readonly for system code ODM", true, discount.L5_SubCodeInfo.ReadOnly);

			discount.L5_SystemCode = BillingConstants.BillingSystem.ABMCustoms;
			AssertEquals("expect not readonly for system code ABMCustoms", false, discount.L5_SubCodeInfo.ReadOnly);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			return organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}

		#endregion
	}
}
