using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Accounting.Module.AccountingFilterStripBusinessObject;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APEnquiryFilterBusinessObject))]
	public class APEnquiryFilterBusinessObjectTestCase : AccountingFilterStripBusinessObjectTestCase
	{
		#region Decimal Places

		public void TestZDecimalsHaveCorrectDecimalPlaces()
		{
			var enquiry = new APEnquiryFilterBusinessObject();

			var localList = new List<string>
				{
					nameof(enquiry.CreditBalance),
					nameof(enquiry.CreditLimit),
					nameof(enquiry.CurrentStandardOutstanding),
					nameof(enquiry.FirstAgeingStandardOutstanding),
					nameof(enquiry.LastPaymentReceipt),
					nameof(enquiry.LastPurchaseSale),
					nameof(enquiry.LYRPurchaseSales),
					nameof(enquiry.MTD_PTDSales),
					nameof(enquiry.SecondAgeingStandardOutstanding),
					nameof(enquiry.ThirdAgeingStandardOutstanding),
					nameof(enquiry.ThirdAgeingStandardOutstanding),
					nameof(enquiry.YTDPurchaseSales),
				};

			var tester = new DecimalPlacesAttributeTester(enquiry);
			tester.CheckLocalCurrency(localList, nameof(enquiry.LocalDecimals));
		}

		public void TestDecimalPlacesAttributeApplyToAllZDecimalProperties()
		{
			var properties = typeof(APEnquiryFilterBusinessObject).GetProperties().Where(x => x.PropertyType == typeof(ZDecimal)).ToList();
			Assert(properties.All(x => Attribute.IsDefined(x, typeof(DecimalPlacesAttribute))));
		}

		#endregion

		#region TestOrganizationFilter

		public void TestOrganisatonWithAddressFilter()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();

			OrgAddress testAddress = Factory.NewWithValidTestData<OrgAddress>();
			testAddress.OA_Code = "XYZ";
			testAddress.OA_OH = testOrg.PK;

			APInvoice testInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			testInvoice1.AH_OH = testOrg.PK;
			testInvoice1.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
			testInvoice1.AH_TransactionNum = "00002544";

			APInvoice testInvoice2 = Factory.NewWithValidTestData<APInvoice>();
			testInvoice2.AH_OH = testOrg.PK;
			testInvoice2.AH_OA_InvoiceAddressOverride = testAddress.PK;
			testInvoice2.AH_ConsolidatedInvoiceRef = "S0002544/A";

			Factory.Save();

			OrgWithAddressFilter orgWithAddressFilter;
			orgWithAddressFilter = ((OrgWithAddressFilter)TestFilterBizO["Organization and Address"]);
			orgWithAddressFilter.Organization = testOrg.PK;
			orgWithAddressFilter.Address = testAddress.PK;
			orgWithAddressFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain 1 invoice", 1, testTransactions.Count);
			Assert("Collection contains invoice 2", testTransactions.Contains(testInvoice2.PK));

			orgWithAddressFilter.Address = ZGuid.Empty;
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain 2 invoice", 2, testTransactions.Count);
			Assert("Collection contains invoice 1", testTransactions.Contains(testInvoice1.PK));
			Assert("Collection contains invoice 2", testTransactions.Contains(testInvoice2.PK));

			orgWithAddressFilter.Address = testOrg.AddressForSendingARDocuments.PK;
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain 1 invoice", 1, testTransactions.Count);
			Assert("Collection contains invoice 1", testTransactions.Contains(testInvoice1.PK));
		}

		public void TestOrganizationFilter()
		{
			OrgHeader testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader testOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			GlbBranch testBranch = Factory.NewWithValidTestData<GlbBranch>();

			APInvoice testInvoice = Factory.New<APInvoice>();
			TestObjectCreator.FillInvoiceWithMinimumTestData(testInvoice);
			testInvoice.AH_OH = testOrg1.PK;
			testInvoice.AH_LocalExTaxAmount = 100M;

			APJournal testJournal = Factory.New<APJournal>();
			testJournal.AH_TransactionNum = "00001000";
			testJournal.AH_OH = testOrg2.PK;

			APInvoice testAPInv = Factory.New<APInvoice>();
			testAPInv.AH_OH = testOrg1.PK;
			testAPInv.AH_LocalExTaxAmount = 50M;
			testAPInv.AH_TransactionNum = "(";
			testAPInv.AH_GB = testBranch.PK;

			Factory.Save();

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)TestFilterBizO["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = testOrg1.PK;
			ZQuery query = TestFilterBizO.Filter;
			APTransactionHeaderCollection transactions = new APTransactionHeaderCollection(Factory, query);
			transactions.Load();
			AssertEquals("There should only be one transaction", 1, transactions.Count);
			Assert("That transaction should be from TestInvoice", transactions.Contains(testInvoice.PK));
		}

		public void TestFilterWorksCorrectlyWhenCalledTwice()
		{
			OrgHeader testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			APEnquiryFilterBusinessObject aPEnquiryFilterBusinessObject = new APEnquiryFilterBusinessObject();
			ModuleFilterCollection filters = aPEnquiryFilterBusinessObject.ModuleFilters;

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)filters["Organisation"];

			AssertEquals("Organization PK on the filter is invalid", false, orgFilter.Property.IsValid);
			AssertEquals("Organization PK on the filter is null", ZGuid.Empty, orgFilter.Property);

			orgFilter.IsActive = true;
			orgFilter.Property = testOrg1.PK;

			AssertEquals("Organization PK on the filter is valid", true, orgFilter.Property.IsValid);
			AssertEquals("Organization PK on the filter is TestOrg1's PK", testOrg1.PK, orgFilter.Property);

			ZQuery query = TestFilterBizO.Filter;
			APTransactionHeaderCollection transactions = new APTransactionHeaderCollection(Factory, query);
			transactions.Load();

			AssertEquals("Organization PK on the filter is valid", true, orgFilter.Property.IsValid);
			AssertEquals("Organization PK on the filter is TestOrg1's PK", testOrg1.PK, orgFilter.Property);

			filters = aPEnquiryFilterBusinessObject.ModuleFilters;
			filters = aPEnquiryFilterBusinessObject.ModuleFilters;

			AssertEquals("[When it gets called twice] Organization PK on the filter is valid", true, orgFilter.Property.IsValid);
			AssertEquals("[When it gets called twice] Organization PK on the filter is TestOrg1's PK", testOrg1.PK, orgFilter.Property);

			query = TestFilterBizO.Filter;
			transactions = new APTransactionHeaderCollection(Factory, query);
			transactions.Load();

			AssertEquals("[When it gets called twice] Organization PK on the filter is valid", true, orgFilter.Property.IsValid);
			AssertEquals("[When it gets called twice] Organization PK on the filter is TestOrg1's PK", testOrg1.PK, orgFilter.Property);
		}

		public void TestFilterByOrganisationWorksAfterFilterIsChanged()
		{
			OrgHeader testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader testOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			APInvoice testInvoice = Factory.New<APInvoice>();
			testInvoice.AH_OH = testOrg1.PK;
			testInvoice.AH_LocalExTaxAmount = 100M;

			APJournal testJournal = Factory.New<APJournal>();
			testJournal.AH_OH = testOrg2.PK;

			APEnquiryFilterBusinessObject filterBizO = new APEnquiryFilterBusinessObject();
			ModuleFilterCollection filters = filterBizO.ModuleFilters;

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)filters["Organisation"];

			orgFilter.Property = testOrg1.PK;
			orgFilter.IsActive = true;

			ZQuery query = filterBizO.Filter;
			APTransactionHeaderCollection transactions = new APTransactionHeaderCollection(Factory, query);
			transactions.Load();

			AssertEquals("There should only be one transaction", 1, transactions.Count);
			Assert("That transaction should be from TestInvoice", transactions.Contains(testInvoice.PK));
			AssertEquals("Transaction collection should not contain transaction from testOrg2", false, transactions.Contains(testJournal));

			orgFilter = (ModuleGuidFilter)filterBizO.ModuleFilters["Organisation"];

			orgFilter.Property = testOrg1.PK;

			query = filterBizO.Filter;
			transactions = new APTransactionHeaderCollection(Factory, query);
			transactions.Load();

			AssertEquals("There should only be one transaction", 1, transactions.Count);
			Assert("That transaction should be from TestInvoice", transactions.Contains(testInvoice.PK));
			AssertEquals("Transaction collection should not contain transaction from testOrg2", false, transactions.Contains(testJournal));
		}

		public void TestOrganizationSummary_WhenGridHasColourScheme()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var testInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, testOrg);
			testInvoice.AH_InvoiceDate = new ZDateTime(2012, 1, 1);
			TestObjectCreator.CreateInvoiceLine(testInvoice, TestObjectCreator.AUD, 1m, -100m, 0m, 0m);
			testInvoice.AH_TransactionNum = "001212";
			testInvoice.AH_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			APEnquiryFilterBusinessObject filterBizO = new APEnquiryFilterBusinessObject();
			ModuleFilterCollection filters = filterBizO.ModuleFilters;

			GridColourStripBusinessObject gridColourFilterBizO = new GridColourStripBusinessObject(filterBizO, null, null);

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)filters["Organisation"];
			filterBizO.ModuleFiltersAreCreated_ForTestOnly = true; //PreCondition

			orgFilter.Property = testOrg.PK;
			orgFilter.IsActive = true;

			ZQuery query = filterBizO.Filter;

			AssertEquals("Check if LastPurchaseSale is set", -100m, filterBizO.LastPurchaseSale);

			AssertEquals("FOrganisationFilter_ForTestOnly should be set PK of testOrg", testOrg.PK, filterBizO.FOrganisationFilter_ForTestOnly.Property.ToGuid());
			AssertEquals("OrganisationPK should contain the same value", testOrg.PK, filterBizO.OrganisationPK);
			filters = gridColourFilterBizO.ModuleFilters;
			Assert("FOrganisationFilter_ForTestOnly is reset to empty", filterBizO.FOrganisationFilter_ForTestOnly.Property.IsEmpty);
			AssertEquals("OrganisationPK should not be reset, it should still hold testOrg", testOrg.PK, filterBizO.OrganisationPK);

			AssertEquals("LastPurchaseSale is should still be set", -100m, filterBizO.LastPurchaseSale);
			filterBizO.ResetInformationalFields(); //PerformSearch operation calls APEnquiryFilterBusinessObject.ResetInformationalFields()

			AssertEquals("LastPurchaseSale should still be set even if fOrganisationFilter is reset to blank.", -100m, filterBizO.LastPurchaseSale);
		}

		public void TestOrganisationPK()
		{
			OrgHeader testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			APEnquiryFilterBusinessObject aPEnquiryFilterBusinessObject = new APEnquiryFilterBusinessObject();
			ModuleFilterCollection filters = aPEnquiryFilterBusinessObject.ModuleFilters;
			ModuleGuidFilter orgFilter = (ModuleGuidFilter)filters["Organisation"];
			AssertEquals("Organization PK set to null", Guid.Empty, aPEnquiryFilterBusinessObject.OrganisationPK);

			orgFilter.IsActive = true;
			orgFilter.Property = testOrg1.PK;
			ZQuery query = TestFilterBizO.Filter;
			APTransactionHeaderCollection transactions = new APTransactionHeaderCollection(Factory, query);
			transactions.Load();
			AssertEquals("Organization PK set to TestOrg1", testOrg1.PK, aPEnquiryFilterBusinessObject.OrganisationPK);
		}

		public void TestResetInformationalFields()
		{
			OrgHeader org1 = TestObjectCreator.AALSHI;
			org1.CompanyData.OB_IsCreditor = true;
			org1.CompanyData.OB_APCreditLimit = 500m;
			APInvoice invoice1 = Factory.New<APInvoice>();
			invoice1.AH_OH = org1.PK;
			invoice1.AH_InvoiceDate = new ZDateTime(2004, 1, 1);
			TestObjectCreator.CreateInvoiceLine(invoice1, invoice1.TransactionCurrency, invoice1.AH_ExchangeRate, 100m, 0m, 0m);
			invoice1.AH_TransactionNum = "1";
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			OrgHeader org2 = TestObjectCreator.ABIGAS;
			org2.CompanyData.OB_IsCreditor = true;
			org2.CompanyData.OB_APCreditLimit = 1000m;
			APInvoice invoice2 = Factory.New<APInvoice>();
			invoice2.AH_OH = org2.PK;
			invoice2.AH_InvoiceDate = new ZDateTime(2004, 1, 1);
			TestObjectCreator.CreateInvoiceLine(invoice2, invoice2.TransactionCurrency, invoice2.AH_ExchangeRate, 200m, 0m, 0m);
			invoice2.AH_TransactionNum = "2";
			invoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			APEnquiryFilterBusinessObject aPEnquiryFilterBusinessObject = new APEnquiryFilterBusinessObject();
			ModuleFilterCollection filters = aPEnquiryFilterBusinessObject.ModuleFilters;
			ModuleGuidFilter orgFilter = (ModuleGuidFilter)filters["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = org1.PK;
			ZQuery query = TestFilterBizO.Filter;
			APTransactionHeaderCollection transactions = new APTransactionHeaderCollection(Factory, query);
			transactions.Load();

			AssertEquals("CreditLimit is from org1", 500M, aPEnquiryFilterBusinessObject.CreditLimit);
			AssertEquals("LastPurchaseSale is from org1", 100M, aPEnquiryFilterBusinessObject.LastPurchaseSale);
			AssertEquals("CreditBalance is from org1", 400M, aPEnquiryFilterBusinessObject.CreditBalance);

			aPEnquiryFilterBusinessObject = new APEnquiryFilterBusinessObject();
			filters = aPEnquiryFilterBusinessObject.ModuleFilters;
			orgFilter = (ModuleGuidFilter)filters["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = org2.PK;
			query = TestFilterBizO.Filter;
			transactions = new APTransactionHeaderCollection(Factory, query);
			transactions.Load();

			AssertEquals("CreditLimit is from org2", 1000M, aPEnquiryFilterBusinessObject.CreditLimit);
			AssertEquals("LastPurchaseSale is from org2", 200M, aPEnquiryFilterBusinessObject.LastPurchaseSale);
			AssertEquals("CreditBalance is from org2", 800M, aPEnquiryFilterBusinessObject.CreditBalance);

			aPEnquiryFilterBusinessObject = new APEnquiryFilterBusinessObject();
			filters = aPEnquiryFilterBusinessObject.ModuleFilters;
			orgFilter = (ModuleGuidFilter)filters["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = org1.PK;
			query = TestFilterBizO.Filter;
			transactions = new APTransactionHeaderCollection(Factory, query);
			transactions.Load();

			AssertEquals("CreditLimit is from org1", 500M, aPEnquiryFilterBusinessObject.CreditLimit);
			AssertEquals("LastPurchaseSale is from org1", 100M, aPEnquiryFilterBusinessObject.LastPurchaseSale);
			AssertEquals("CreditBalance is from org1", 400M, aPEnquiryFilterBusinessObject.CreditBalance);
		}

		#endregion

		#region TestHasRelatedClaimFilter

		public void TestHasRelatedClaimFilter()
		{
			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			var invoice2 = Factory.NewWithValidTestData<APInvoice>();
			var invoice3 = Factory.NewWithValidTestData<APInvoice>();
			var invoice4 = Factory.NewWithValidTestData<APInvoice>();

			Factory.Save();

			var payablesEnquiryFilterBizo = new APEnquiryFilterBusinessObject();
			var filters = payablesEnquiryFilterBizo.ModuleFilters;

			var hasRelatedClimFilter = ((ModuleTextFilter)filters["Has Related Claim"]);
			hasRelatedClimFilter.Property = "No AP Claim";
			hasRelatedClimFilter.IsActive = true;
			var testTransactions = new TransactionHeaderCollection(Factory, payablesEnquiryFilterBizo.Filter);
			testTransactions.Load();
			AssertEquals("All four transactions should be contained in the list", 4, testTransactions.Count);
			AssertContainsExactElementsInAnyOrder("All four transactions should be contained in the list", new[] { invoice1, invoice2, invoice3, invoice4 }, testTransactions);

			hasRelatedClimFilter.Property = "Has AP Claim";
			testTransactions = new TransactionHeaderCollection(Factory, payablesEnquiryFilterBizo.Filter);
			testTransactions.Load();
			AssertEquals("No AP Claim for any of this invoice", 0, testTransactions.Count);

			var claim1 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim1.AY_AH = invoice1.PK;
			var claim2 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim2.AY_AH = invoice2.PK;

			Factory.Save();

			hasRelatedClimFilter.Property = "Has AP Claim";
			testTransactions = new TransactionHeaderCollection(Factory, payablesEnquiryFilterBizo.Filter);
			testTransactions.Load();
			AssertEquals("Should return invoice1 and invoice2", 2, testTransactions.Count);
			AssertContainsExactElementsInAnyOrder("Should return invoice1 and invoice2", new[] { invoice1, invoice2 }, testTransactions);

			hasRelatedClimFilter.Property = "No AP Claim";
			testTransactions = new TransactionHeaderCollection(Factory, payablesEnquiryFilterBizo.Filter);
			testTransactions.Load();
			AssertEquals("Should return invoice3 and invoice4", 2, testTransactions.Count);
			AssertContainsExactElementsInAnyOrder("Should return invoice3 and invoice4", new[] { invoice3, invoice4 }, testTransactions);
		}

		#endregion

		#region TestLastPurchaseSale

		public void TestLastPurchaseSale()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			GlbBranch testBranch = Factory.NewWithValidTestData<GlbBranch>();

			APInvoice testInvoice1 = Factory.New<APInvoice>();
			testInvoice1.AH_OH = testOrg.PK;
			testInvoice1.AH_InvoiceDate = new ZDateTime(2004, 1, 1);
			TestObjectCreator.CreateInvoiceLine(testInvoice1, testInvoice1.TransactionCurrency, testInvoice1.AH_ExchangeRate, -1m, 0m, 0m);
			testInvoice1.AH_TransactionNum = "%";
			testInvoice1.AH_GB = GlbBranch.CurrentBranch.PK;

			APInvoice testInvoice = Factory.New<APInvoice>();
			testInvoice.AH_OH = testOrg.PK;
			testInvoice.AH_InvoiceDate = new ZDateTime(2004, 2, 2);
			TestObjectCreator.CreateInvoiceLine(testInvoice, testInvoice.TransactionCurrency, testInvoice.AH_ExchangeRate, -2m, 0m, 0m);
			testInvoice.AH_TransactionNum = "@";
			testInvoice.AH_GB = GlbBranch.CurrentBranch.PK;

			APInvoice testInvoice2 = Factory.New<APInvoice>();
			testInvoice2.AH_OH = testOrg.PK;
			testInvoice2.AH_InvoiceDate = new ZDateTime(2004, 3, 3);
			TestObjectCreator.CreateInvoiceLine(testInvoice2, testInvoice2.TransactionCurrency, testInvoice2.AH_ExchangeRate, -3m, 0m, 0m);
			testInvoice2.AH_TransactionNum = "#";
			testInvoice2.AH_GB = testBranch.PK;
			testInvoice2.Lines[0].AL_GB = testInvoice2.Company.Branches[0].PK;

			APJournal testJournal = Factory.New<APJournal>();
			testJournal.AH_OH = testOrg.PK;
			testJournal.AH_InvoiceDate = new ZDateTime(2004, 4, 4);
			testJournal.AH_InvoiceAmount = 4;
			testJournal.AH_OutstandingAmount = 4;
			testJournal.AH_OSTotal = 4;
			testJournal.AH_TransactionNum = "!";
			testJournal.AH_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)TestFilterBizO["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = testOrg.PK;
			AssertEquals("Last purchase amount from TestOrg should be 2", new ZDecimal(-2), TestFilterBizO.LastPurchaseSale);
		}

		#endregion

		#region TestLastPaymentReceipt

		public void TestLastPaymentReceipt()
		{
			GlbBranch testBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();

			APPayment testPayment = Factory.New<APPayment>();
			testPayment.AH_OH = testOrg.PK;
			testPayment.AH_GB = GlbBranch.CurrentBranch.PK;
			testPayment.AH_InvoiceAmount = 10;
			testPayment.AH_OutstandingAmount = 10;
			testPayment.AH_OSTotal = 10;
			testPayment.AH_InvoiceDate = new ZDateTime(2003, 1, 2);
			testPayment.AH_TransactionNum = "#";

			APPayment testPayment2 = Factory.New<APPayment>();
			testPayment2.AH_OH = testOrg.PK;
			testPayment2.AH_GB = GlbBranch.CurrentBranch.PK;
			testPayment2.AH_InvoiceAmount = 9;
			testPayment2.AH_OutstandingAmount = 9;
			testPayment2.AH_OSTotal = 9;
			testPayment2.AH_InvoiceDate = new ZDateTime(2003, 2, 3);
			testPayment2.AH_TransactionNum = "$";

			APInvoice testInvoice = Factory.New<APInvoice>();
			testInvoice.AH_OH = testOrg.PK;
			testInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			testInvoice.AH_InvoiceAmount = 11;
			testInvoice.AH_OutstandingAmount = 11;
			testInvoice.AH_OSTotal = 11;
			testInvoice.AH_InvoiceDate = new ZDateTime(2003, 3, 4);
			testInvoice.AH_TransactionNum = "!";

			Factory.Save();

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)TestFilterBizO["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = testOrg.PK;
			AssertEquals("Last Payment by TestOrg should be 9", new ZDecimal(9), TestFilterBizO.LastPaymentReceipt);
		}

		#endregion

		#region TestLastPaymentReceiptDate

		public void TestLastPaymentReceiptDate()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			GlbBranch testBranch = Factory.NewWithValidTestData<GlbBranch>();

			APPayment testPayment = Factory.New<APPayment>();
			testPayment.AH_OH = testOrg.PK;
			testPayment.AH_InvoiceDate = new ZDateTime(2002, 4, 5);
			testPayment.AH_TransactionNum = "@";
			testPayment.AH_GB = GlbBranch.CurrentBranch.PK;

			APPayment testPayment2 = Factory.New<APPayment>();
			testPayment2.AH_OH = testOrg.PK;
			testPayment2.AH_InvoiceDate = new ZDateTime(2002, 5, 6);
			testPayment2.AH_TransactionNum = "$";
			testPayment2.AH_GB = GlbBranch.CurrentBranch.PK;

			APPayment testPayment3 = Factory.New<APPayment>();
			testPayment3.AH_OH = testOrg.PK;
			testPayment3.AH_InvoiceDate = new ZDateTime(2002, 6, 7);
			testPayment3.AH_TransactionNum = "%";
			testPayment3.AH_GB = testBranch.PK;

			APJournal testJournal = Factory.New<APJournal>();
			testJournal.AH_OH = testOrg.PK;
			testJournal.AH_InvoiceDate = new ZDateTime(2002, 7, 8);
			testJournal.AH_TransactionNum = "^";
			testJournal.AH_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)TestFilterBizO["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = testOrg.PK;
			var expected = new ZDateTime(2002, 5, 6).Date.ToString();
			AssertEquals("Last payment date should be 06-May-02", expected, TestFilterBizO.LastPaymentReceiptDate);
		}

		#endregion

		#region TestPaymentStatusFilter

		public void TestPaymentStatusFilter()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();

			APInvoice testInvoice = Factory.New<APInvoice>();
			TestObjectCreator.FillInvoiceWithMinimumTestData(testInvoice);
			TestObjectCreator.CreateInvoiceLine(testInvoice, testInvoice.TransactionCurrency, testInvoice.AH_ExchangeRate, 25m, 0, 0m);
			testInvoice.AH_OH = testOrg.PK;

			APPayment testPayment = Factory.New<APPayment>();
			testPayment.AH_OutstandingAmount = 100M;
			testPayment.AH_InvoiceAmount = 25M;
			testPayment.AH_GSTAmount = 95M;
			testPayment.AH_OSTotal = 120M;
			testPayment.AH_OH = testOrg.PK;
			AccTransactionMatchLink matchLink = ((IMatching)testPayment).CurrentMatchGroup.AddNew();
			matchLink.AP_Amount = 20M;
			matchLink.AP_AH = testPayment.PK;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -20M;

			AccTransactionMatchLink matchingLink = ((IMatching)testPayment).CurrentMatchGroup.AddNew();
			matchingLink.AP_AH = headerToMatch.PK;
			matchingLink.AP_Amount = -20M;
			APPayment testPayment2 = Factory.New<APPayment>();
			testPayment2.AH_OutstandingAmount = 0M;
			testPayment2.AH_FullyPaidDate = ZDateTime.Now;
			testPayment2.AH_OH = testOrg.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(testPayment);
			Factory.Save();

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)TestFilterBizO["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = testOrg.PK;

			ModuleTextFilter paymentStatusFilter = (ModuleTextFilter)TestFilterBizO["Payment Status"];
			paymentStatusFilter.IsActive = true;

			paymentStatusFilter.Property = AccountingUtils.PaymentStatusTypes.PartPaid;
			ZQuery query = TestFilterBizO.Filter;
			APTransactionHeaderCollection aPTransactions = new APTransactionHeaderCollection(Factory, query);
			aPTransactions.Load();
			AssertEquals("APTransactions should contain one transaction", 1, aPTransactions.Count);
			Assert("APTransactions should contain TestPayment", aPTransactions.Contains(testPayment.PK));

			paymentStatusFilter.Property = AccountingUtils.PaymentStatusTypes.Unpaid;
			query = TestFilterBizO.Filter;
			aPTransactions = new APTransactionHeaderCollection(Factory, query);
			aPTransactions.Load();
			AssertEquals("APTransactions should contain two transactions", 2, aPTransactions.Count);
			Assert("APTransactions should contain TestInvoice", aPTransactions.Contains(testInvoice.PK));
			Assert("APTransactions should contain TestPayment", aPTransactions.Contains(testPayment.PK));

			paymentStatusFilter.Property = AccountingUtils.PaymentStatusTypes.All;
			query = TestFilterBizO.Filter;
			aPTransactions = new APTransactionHeaderCollection(Factory, query);
			aPTransactions.Load();
			AssertEquals("APTransactions should contain 3 transactions", 3, aPTransactions.Count);
		}

		#endregion

		#region TestPaymentStatusList

		public void TestPaymentStatusList()
		{
			AssertEquals("Payment Status list should contain 4 values", 4, TestFilterBizO.PaymentStatusList_ForTestOnly.Count);
			Assert("Payment Status list should contain All value", TestFilterBizO.PaymentStatusList_ForTestOnly.ContainsCode(AccountingUtils.PaymentStatusTypes.All));
			Assert("Payment Status list should contain Unpaid value", TestFilterBizO.PaymentStatusList_ForTestOnly.ContainsCode(AccountingUtils.PaymentStatusTypes.Unpaid));
			Assert("Payment Status list should contain Paid value", TestFilterBizO.PaymentStatusList_ForTestOnly.ContainsCode(AccountingUtils.PaymentStatusTypes.Unpaid));
			Assert("Payment Status list should contain PartPaid value", TestFilterBizO.PaymentStatusList_ForTestOnly.ContainsCode(AccountingUtils.PaymentStatusTypes.PartPaid));
		}

		#endregion

		#region TestBranchManagementCodeFilter

		public void TestBranchManagementCodeFilter()
		{
			var codeCollection = new BranchManagementCodeDescriptionBoolCollection();
			codeCollection.Add("BRA", null, true);
			codeCollection.Add("BRB", null, true);
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeCollection);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_AccountingGroupCode = "BRA";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_AccountingGroupCode = "BRB";

			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_GB = branch1.PK;
			var invoice2 = Factory.NewWithValidTestData<APInvoice>();
			invoice2.AH_GB = branch2.PK;

			Factory.Save();

			var branchManagementCodeFilter = (ModuleTextFilter)TestFilterBizO["Branch Management Code"];
			branchManagementCodeFilter.Property = "BRA";
			branchManagementCodeFilter.IsActive = true;
			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Collection should Contain invoice1", new[] { invoice1 }, collection);

			branchManagementCodeFilter.Property = "BRB";
			collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Collection should Contain invoice2", new[] { invoice2 }, collection);
		}

		#endregion

		public void TestStandardPaymentTerms()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsCreditor = true;
			orgHeader.CompanyData.OB_APPaymentTermDays = 30;
			orgHeader.CompanyData.OB_APPaymentTerms = InvoiceTermsList.FromInvoiceDate.Code;
			Factory.Save();

			var enquiry = new APEnquiryFilterBusinessObject();
			AssertNullOrEmpty(enquiry.StandardPaymentTerms);

			var filters = enquiry.ModuleFilters;
			var orgFilter = (ModuleGuidFilter)filters["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = orgHeader.PK;
			enquiry.SetInfoValues();
			AssertEquals("30/INV", enquiry.StandardPaymentTerms);
		}

		public void TestAgingOptionsList()
		{
			var enquiry = new APEnquiryFilterBusinessObject();
			AssertEquals(AccountingConstants.AgingOptions.CodeList, enquiry.AgingDateTypeList);
		}

		public void TestAgingDateType()
		{
			var enquiry = new APEnquiryFilterBusinessObject();
			AssertEquals(AccountingConfigurationRegistry.Instance.AgingOptionPayables.Value, enquiry.AgingDateType);
		}

		public void TestAgingDateTypeInfoValueChanged()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = testObjectCreator.CreateAPInvoice<APInvoice>("AP00010002", testObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, testObjectCreator.Creditor1);
			var periodManagementTestHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodManagementTestHelper.SetupPeriods();
			var firstAgeDate = periodManagementTestHelper.PreviousOpenPeriod.AM_StartDate.AddDays(5);
			invoice.AH_InvoiceDate = firstAgeDate;
			invoice.AH_PostDate = firstAgeDate;
			Factory.Save();

			var apEnquiryFilterBusinessObject = new APEnquiryFilterBusinessObject();
			var filters = apEnquiryFilterBusinessObject.ModuleFilters;
			var orgFilter = (ModuleGuidFilter)filters["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = testObjectCreator.Creditor1.PK;

			apEnquiryFilterBusinessObject.AgingDateType = "AAA";
			AssertEquals(decimal.Zero, apEnquiryFilterBusinessObject.FirstAgeingStandardOutstanding);

			apEnquiryFilterBusinessObject.AgingDateType = AccountingConstants.AgingOptions.InvoiceDate;
			AssertEquals(110.00m, apEnquiryFilterBusinessObject.FirstAgeingStandardOutstanding);
		}

		#region TestTaxTransactionsFilters

		public void TestServiceCodeFilterVisibility_WhenCompanyHasAPWithholdTaxEnabled()
		{
			var apEnquiryFilterBusinessObject = new APEnquiryFilterBusinessObject();
			var filters = apEnquiryFilterBusinessObject.ModuleFilters;
			var serviceCodeFilter = (ModuleTextFilter)filters["Service Code"];

			AssertEquals("Pre-condition: IsAPWithHoldTaxEnabled", false, GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled());
			AssertNull("When not IsAPWHTEnabled", serviceCodeFilter);

			SetupTaxConfigurationWithSPRSuperType();

			apEnquiryFilterBusinessObject = new APEnquiryFilterBusinessObject();
			filters = apEnquiryFilterBusinessObject.ModuleFilters;
			serviceCodeFilter = (ModuleTextFilter)filters["Service Code"];

			AssertEquals(true, GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled());
			AssertNotNull(serviceCodeFilter);
		}

		public void TestServiceCodeAndNotionalWHTFilterCollection()
		{
			SetupTaxConfigurationWithSPRSuperType();
			var transaction1 = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);
			CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "AAAAAAAAAAA", false, false);

			var notionalWHTFilter = (ModuleFlagsFilter)TestFilterBizO["Notional WHT"];
			AssertNotNull("Pre-condition: IsAPWithHoldTaxEnabled", notionalWHTFilter);
			notionalWHTFilter.IsActive = true;
			notionalWHTFilter.Property0 = true;

			var serviceCodeFilter = (ModuleTextFilter)TestFilterBizO["Service Code"];
			AssertNotNull("Pre-condition: IsAPWithHoldTaxEnabled", serviceCodeFilter);
			serviceCodeFilter.IsActive = true;
			serviceCodeFilter.Property = "ServiceCode";

			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertContainsExactElementsInExactOrder(collection, new[] { transaction1 });

			var transaction2 = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode2", false, false);

			collection.Load();

			AssertContainsExactElementsInAnyOrder(collection, new[] { transaction1, transaction2 });
		}

		public void TestTaxTransactionsFilterSubGroup()
		{
			SetupTaxConfigurationWithSPRSuperType();

			var notionalWHTFilter = (ModuleFlagsFilter)TestFilterBizO["Notional WHT"];
			AssertNotNull("Pre-condition: IsAPWithHoldTaxEnabled", notionalWHTFilter);

			var serviceCodeFilter = (ModuleTextFilter)TestFilterBizO["Service Code"];
			AssertNotNull("Pre-condition: IsAPWithHoldTaxEnabled", serviceCodeFilter);

			AssertEquals(notionalWHTFilter.SubGroup.GetType(), typeof(TaxTransactionFilterSubGroup));
			AssertEquals(serviceCodeFilter.SubGroup.GetType(), typeof(TaxTransactionFilterSubGroup));

			AssertEquals(notionalWHTFilter.SubGroup, serviceCodeFilter.SubGroup);
		}

		public void TestNotionalWHTFilterVisibility_WhenCompanyHasAPWithholdTaxEnabled()
		{
			var apEnquiryFilterBusinessObject = new APEnquiryFilterBusinessObject();
			var filters = apEnquiryFilterBusinessObject.ModuleFilters;
			var notionalWHTFilter = (ModuleFlagsFilter)filters["Notional WHT"];

			AssertEquals("Pre-condition: IsAPWithHoldTaxEnabled", false, GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled());
			AssertNull("When not IsAPWHTEnabled", notionalWHTFilter);

			SetupTaxConfigurationWithSPRSuperType();

			apEnquiryFilterBusinessObject = new APEnquiryFilterBusinessObject();
			filters = apEnquiryFilterBusinessObject.ModuleFilters;
			notionalWHTFilter = (ModuleFlagsFilter)filters["Notional WHT"];

			AssertEquals(true, GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled());
			AssertNotNull(notionalWHTFilter);
		}

		public void TestNotionalWHTFilter_ReturnsWhenValidTransactionDetails()
		{
			SetupTaxConfigurationWithSPRSuperType();
			var transaction1 = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);
			var transaction2 = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);
			var transaction3 = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);

			GetNotionalWHTFilter();

			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(collection, new[] { transaction1, transaction2, transaction3 });
		}

		public void TestNotionalWHTFilter_ReturnsValidTransactions_WhenSuperTypeIsSPR()
		{
			SetupTaxConfigurationWithSPRSuperType();
			CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.SalesTax.Code, "ServiceCode", false, false);
			GetNotionalWHTFilter();

			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals(collection.Count, 0);

			var transaction = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);

			collection.Load();

			AssertContainsExactElementsInAnyOrder(collection, new[] { transaction });
		}

		public void TestNotionalWHTFilter_ReturnsValidTransactions_WhenTransactionIsNOTRealised()
		{
			SetupTaxConfigurationWithSPRSuperType();
			CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, true);
			GetNotionalWHTFilter();

			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals(collection.Count, 0);

			var transaction = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);

			collection.Load();

			AssertContainsExactElementsInAnyOrder(collection, new[] { transaction });
		}

		public void TestNotionalWHTFilter_ReturnsValidTransactions_WhenTransactionIsNOTCancelled()
		{
			SetupTaxConfigurationWithSPRSuperType();
			CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", true, false);
			GetNotionalWHTFilter();

			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals(collection.Count, 0);

			var transaction = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.StandardPaymentRetention.Code, "ServiceCode", false, false);

			collection.Load();

			AssertContainsExactElementsInAnyOrder(collection, new[] { transaction });
		}

		public void TestNotionalWHTFilterUnchecked()
		{
			SetupTaxConfigurationWithSPRSuperType();
			var nonNotionalWHTTransaction = CreateInvoiceWithTaxTransactionsAndSaveInDB(TaxSuperTypeList.SalesTax.Code, "AAAAAAA", false, false);

			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals(collection.Count, 1);
			var notionalWHTFilter = (ModuleFlagsFilter)TestFilterBizO["Notional WHT"];
			notionalWHTFilter.IsActive = true;
			notionalWHTFilter.Property0 = false;

			collection.Load();

			AssertContainsExactElementsInAnyOrder(collection, new[] { nonNotionalWHTTransaction });

			notionalWHTFilter.Property0 = true;
			collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();

			AssertEquals(collection.Count, 0);
		}

		ModuleFlagsFilter GetNotionalWHTFilter(bool isTrue = true)
		{
			var notionalWHTFilter = (ModuleFlagsFilter)TestFilterBizO["Notional WHT"];
			notionalWHTFilter.IsActive = true;
			notionalWHTFilter.Property0 = isTrue;

			return notionalWHTFilter;
		}

		void SetupTaxConfigurationWithSPRSuperType()
		{
			ObjectCreator.CreateTestPeriods(ZDate.Today.AddMonths(-1));
			taxConfig = TaxFrameworkTestObjectCreator.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperTypeList.StandardPaymentRetention.Code);
			taxConfig.ETC_AG_TaxControlAccount = ObjectCreator.GSTInputControlAccount().PK;
		}

		APInvoice CreateInvoiceWithTaxTransactionsAndSaveInDB(ZString superType, ZString serviceCode, ZBool isCancelled, ZBool isRealized)
		{
			var invoice = ObjectCreator.CreateInvoice(typeof(APInvoice));

			var taxRecord = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters
			{
				TransactionHeader = invoice,
				TaxConfiguration = taxConfig,
				LocalTaxAmount = 100,
				OsTaxAmount = 100M,
				ServiceCode = serviceCode,
				ServiceCodeDescription = "ServiceCodeDescription",
				Ledger = TaxConfigurationLedgers.AccountsPayable.Code,
				RealisationDate = isRealized ? ZDate.Today : ZDate.Empty,
				TaxSuperType = superType,
				IsCancelled = isCancelled,
				TaxBasis = TaxBasisList.PostingOnMatching.Code,
			});

			var transactionLine = ObjectCreator.CreateInvoiceLine(invoice, invoice.AH_OSExTaxAmount);

			var pivot = TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxRecord.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(transactionLine));
			pivot.ATP_LocalTaxAmount = 100;

			Factory.Save();

			return invoice as APInvoice;
		}

		TestObjectCreator objectCreator;
		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));

		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		AccTaxConfiguration taxConfig;

		#endregion

		#region TestInvoicePaymentReferenceNumberFiltering

		public void TestInvoicePaymentReferenceNumberFiltering()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice1 = testObjectCreator.CreateAPInvoice<APInvoice>("AP00010001", testObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, testObjectCreator.Creditor1);
			var invoice2 = testObjectCreator.CreateAPInvoice<APInvoice>("AP00010002", testObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, testObjectCreator.Creditor1);
			invoice1.InvoiceRemittanceReference = "00001000";
			invoice2.InvoiceRemittanceReference = "00001001";

			Factory.Save();

			var invoicePaymentReferenceNumberFilter = ((ModuleTextFilter)TestFilterBizO["Invoice Remittance Reference"]);
			invoicePaymentReferenceNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			invoicePaymentReferenceNumberFilter.Property = "00001000";
			invoicePaymentReferenceNumberFilter.IsActive = true;

			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(60), invoicePaymentReferenceNumberFilter.MaxLength);

			var testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain one invoice", 1, testTransactions.Count);
			Assert("Collection contains invoice1", testTransactions.Contains(invoice1.PK));
			Assert("Collection doesn't contains invoice2", !testTransactions.Contains(invoice2.PK));
		}

		#endregion

		#region TestMatchStatusAndReasonFilter

		public void TestMatchStatusAndReasonFilter()
		{
			var matchingCollection = new IMatchingCollection(Factory);
			var payment = Factory.New<APPayment>();
			var payment2 = Factory.New<APPayment>();
			payment.AH_MatchStatus = "XXX";
			payment.AH_MatchStatusReasonCode = "ADV";
			payment2.AH_MatchStatus = "UAC";
			payment2.AH_MatchStatusReasonCode = "YYY";
			matchingCollection.Add(payment);
			matchingCollection.Add(payment2);
			Factory.Save();

			var statusFilter = ((ModuleTextFilter)TestFilterBizO["Match Status"]);
			statusFilter.IsActive = true;
			statusFilter.Property = "UAC";
			var testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Should contain payment2 only", 1, testTransactions.Count);
			Assert("Should contain payment2 only", testTransactions.Contains(payment2.PK));

			statusFilter.IsActive = false;
			var statusReasonFilter = ((ModuleTextFilter)TestFilterBizO["Match Status Reason"]);
			statusReasonFilter.IsActive = true;
			statusReasonFilter.Property = "ADV";
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Should contain payment only", 1, testTransactions.Count);
			Assert("Should contain payment only", testTransactions.Contains(payment.PK));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestFilterBizO = (APEnquiryFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		protected APEnquiryFilterBusinessObject TestFilterBizO;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new APEnquiryFilterBusinessObject();
		}

		#endregion
	}
}
