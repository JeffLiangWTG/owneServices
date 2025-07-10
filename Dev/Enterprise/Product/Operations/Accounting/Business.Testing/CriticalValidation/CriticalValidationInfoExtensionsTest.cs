using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CriticalValidation.Testing
{
	public class CriticalValidationInfoExtensionsTest : TestCaseWithFactory
	{
		public void TestGetTransactionMatchLinkGroupInfoAboutCompanyIssue()
		{
			TransactionMatchLinkGroup group = new TransactionMatchLinkGroup(Factory);

			var mainCompany = GlbCompany.CurrentCompany.PK;
			var otherCompany1 = TestObjectCreator.NonCurrentCompany.PK;

			var header1 = SetUpMatchLink(Guid.NewGuid().ToString(), 5,
				SetUpTransactionHeader(Guid.NewGuid().ToString(), "AR", "INV", 10, new DateTime(2003, 3, 3), new DateTime(2003, 3, 4), "0001")
				, group
			).TransactionHeader;
			header1.AH_GC = mainCompany;

			var header2 = SetUpMatchLink(Guid.NewGuid().ToString(), 5,
				SetUpTransactionHeader(Guid.NewGuid().ToString(), "AR", "INV", 10, new DateTime(2003, 3, 3), new DateTime(2003, 3, 4), "0001")
				, group
			).TransactionHeader;
			header2.AH_GC = mainCompany;

			var header3 = SetUpMatchLink(Guid.NewGuid().ToString(), 5,
				SetUpTransactionHeader(Guid.NewGuid().ToString(), "AR", "INV", 10, new DateTime(2003, 3, 3), new DateTime(2003, 3, 4), "0001")
				, group
			).TransactionHeader;
			header3.AH_GC = otherCompany1;

			var info = group.GetTransactionMatchLinkGroupInfoAboutCompanyIssue((
				mainCompany: mainCompany,
				otherCompany: new ZGuid[] { otherCompany1 }
			));
			AssertEquals("other company's data number incorrect", Regex.Matches(info, "AH_GC =").Count, 1);
			AssertEquals(Regex.Match(info, "MainCompany = (?<mainCompany>[^\\,]*),").Groups["mainCompany"].Value.Trim(), mainCompany.ToString());
		}

		public void TestGetTransactionMatchLinkGroupInfoAboutBalanceIssue()
		{
			TransactionMatchLinkGroup group = new TransactionMatchLinkGroup(Factory);
			AccTransactionHeader header1 = SetUpTransactionHeader("1ca892c8-9957-4810-85f3-fdee3baf7001", "AR", "INV", 10, new DateTime(2003, 3, 3), new DateTime(2003, 3, 4), "0001");
			TransactionMatchLink link1 = SetUpMatchLink("1ca892c8-9957-4810-85f3-fdee3baf6001", 5, header1, group);

			string expectedInfo = string.Format(CultureInfo.InvariantCulture, @"Transaction Match Group Balance = 5, Number of Match Links = 1

Match Link 0:
	PK = 1ca892c8-9957-4810-85f3-fdee3baf6001
	Type = TransactionMatchLink
	Types around row = TransactionMatchLink
	Factory Instance = {0}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None


Properties:", link1.Factory._Instance);
			Assert(group.GetTransactionMatchLinkGroupInfoAboutBalanceIssue().Contains(expectedInfo));

			expectedInfo = string.Format(CultureInfo.InvariantCulture, @"
Transaction 0:
	PK = 1ca892c8-9957-4810-85f3-fdee3baf7001
	Type = AccTransactionHeader
	Types around row = AccTransactionHeader|ARInvoice
	Factory Instance = {0}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None


Properties:", header1.Factory._Instance);
			Assert(group.GetTransactionMatchLinkGroupInfoAboutBalanceIssue().Contains(expectedInfo));

			expectedInfo = string.Format(CultureInfo.InvariantCulture, @"LocalPartialPaymentAmount: 0");
			Assert(group.GetTransactionMatchLinkGroupInfoAboutBalanceIssue().Contains(expectedInfo));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		public void TestGetTransactionMatchLinkGroupInfoAboutBalanceIssueForNullHeader()
		{
			TransactionMatchLinkGroup group = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink matchLink = (TransactionMatchLink)Factory.New(typeof(TransactionMatchLink), new Guid("1ca892c8-9957-4810-85f3-fdee3baf7010"));
			group.Add(matchLink);
			string expectedInfo = string.Format(CultureInfo.InvariantCulture, @"Transaction Match Group Balance = 0, Number of Match Links = 1

Match Link 0:
	PK = 1ca892c8-9957-4810-85f3-fdee3baf7010
	Type = TransactionMatchLink
	Types around row = TransactionMatchLink
	Factory Instance = {0}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None


Properties:
 AP_AH = 00000000-0000-0000-0000-000000000000
 AP_Amount = 0
 AP_GSTRealised = 0
 AP_MatchDate = 
 AP_MatchGroupNum = 
 AP_MatchPeriod = 0
 AP_OSAmount = 0
 AP_Reason = 
 AP_SystemCreateTimeUtc = 
 AP_SystemCreateUser = 
 AP_SystemLastEditTimeUtc = 
 AP_SystemLastEditUser = 

Transaction Header is null.", matchLink.Factory._Instance);
			AssertContains(expectedInfo, group.GetTransactionMatchLinkGroupInfoAboutBalanceIssue());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		TransactionMatchLink SetUpMatchLink(string pk, ZDecimal amount, AccTransactionHeader header, TransactionMatchLinkGroup group)
		{
			TransactionMatchLink matchLink = (TransactionMatchLink)Factory.New(typeof(TransactionMatchLink), new Guid(pk));
			matchLink.AP_Amount = amount;
			matchLink.AP_AH = header.PK;
			group.Add(matchLink);
			return matchLink;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		AccTransactionHeader SetUpTransactionHeader(string pk, string ledger, string transactionType, decimal amount, ZDateTime invoiceDate, ZDateTime postDate, ZString transactionNum)
		{
			AccTransactionHeader transaction = (AccTransactionHeader)Factory.New(typeof(AccTransactionHeader), new Guid(pk));
			transaction.AH_Ledger = ledger;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_InvoiceAmount = amount;
			transaction.AH_OutstandingAmount = amount;
			transaction.AH_OSTotal = amount;
			transaction.AH_InvoiceDate = invoiceDate;
			transaction.AH_PostDate = postDate;
			transaction.AH_GC = GlbCompany.CurrentCompany.PK;
			transaction.AH_GB = GlbBranch.CurrentBranch.PK;
			transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;
			transaction.AH_GSTAmount = amount / 10;
			transaction.AH_RX_NKTransactionCurrency = "AUD";
			transaction.AH_FullyPaidDate = postDate;
			transaction.AH_TransactionNum = transactionNum;
			return transaction;
		}

		[TestDate(2017, 09, 28)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		public void TestGetJobConsolCostInfo()
		{
			var loader1 = new Job.Loader(Factory.New<ForwardingShipment>());
			var job1 = loader1.TryCreateWithoutMutexForTestOnly();

			var loader2 = new Job.Loader(Factory.New<ForwardingShipment>());
			var job2 = loader2.TryCreateWithoutMutexForTestOnly();

			JobConsolCost consolCost = (JobConsolCost)Factory.New(typeof(JobConsolCost), new Guid("1ca892c8-9957-4810-85f3-fdee3baf7005"));
			ZDateTime consolCostDate = new ZDateTime(2009, 11, 20);
			using (consolCost.ReportSettingParentSuspender.GetSuspender())
			{
				consolCost.E6_ParentID = new ZGuid("23c1a688-2672-4284-af00-cd68b7196911");
				consolCost.E6_ParentTableCode = "JK";
			}
			consolCost.E6_AC_ChargeCode = Factory.New<AccChargeCode>().PK;
			consolCost.E6_SupplyType = "LOC";
			consolCost.ChargeCode.AC_Code = "QWE";
			consolCost.E6_InvoiceNum = "1256";
			consolCost.E6_OH_Creditor = Factory.New<OrgHeader>().PK;
			consolCost.Creditor.OH_Code = "CRDORG";
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			consolCost.Creditor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			consolCost.E6_GB_CostTaxBranch = Factory.New<GlbBranch>().PK;
			consolCost.CostTaxBranch.GB_Code = "TB1";
			consolCost.E6_InvoiceDate = consolCostDate;
			consolCost.E6_RX_NKCurrency = "AUD";
			consolCost.E6_AT_TaxRate = new ZGuid("54e4ccc9-4cae-4a8d-a311-da189a6eadcf");
			consolCost.E6_TaxDate = ZDate.Today.AddDays(1);
			consolCost.E6_ExchangeRate = 2.05M;
			consolCost.E6_OSCostAmount = 10M;
			consolCost.E6_LocalCostAmount = 20.5M;
			consolCost.E6_PPDCLT = "ALL";
			consolCost.E6_ApportionmentMethod = "SHP";
			consolCost.E6_CostReference = "ABC";
			consolCost.E6_PaymentDate = consolCostDate.AddDays(10);
			consolCost.E6_PaymentType = "EFT";
			consolCost.E6_ChequeOrReference = "REF";
			consolCost.E6_AB_BankAccount = new ZGuid("2dacf199-1717-42dc-a317-523e0ad624f9");
			consolCost.E6_AK_ChequeBook = new ZGuid("066a46ba-5a0e-4f65-b17d-de459a885fac");
			consolCost.E6_AH_ARInvoice = new ZGuid("8a7ea625-d941-4290-b8fb-8703e483b2bf");
			consolCost.E6_AH_APInvoice = new ZGuid("8ba7ad03-74f2-489f-a3a4-b48dcd71ae42");
			consolCost.E6_OSGSTAmount_Calc = 1M;
			consolCost.E6_IsForCollectInvoice = true;

			string expectedInfo = string.Format(CultureInfo.InvariantCulture,
@"Job Consol Cost:
	PK = 1ca892c8-9957-4810-85f3-fdee3baf7005
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {0}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = QWE, Invoice # = 1256, Invoice Date = 20-Nov-09 00:00:00, Currency = AUD, OS Cost Amount = 10, GST is Overridden = Yes, OS GST Amount = 1, Exchange Rate = 2.05, Local Cost Amount = 20.5, Creditor = CRDORG, PPDCLT = ALL, Apportionment Method = SHP, Supplier Cost Reference = ABC, Payment Date = 30-Nov-09 00:00:00, Payment Type = EFT, Cheque # = REF, Bank Account = 2dacf199-1717-42dc-a317-523e0ad624f9, Cheque Book = 066a46ba-5a0e-4f65-b17d-de459a885fac, Tax Rate = 54e4ccc9-4cae-4a8d-a311-da189a6eadcf, Tax Date = {1}, AR Invoice = 8a7ea625-d941-4290-b8fb-8703e483b2bf, AP Invoice = 8ba7ad03-74f2-489f-a3a4-b48dcd71ae42, Is For Collect Invoice = Y, Consol = 23c1a688-2672-4284-af00-cd68b7196911, Is Final = No, Tax Code = , Supply Type = LOC, Tax Branch = TB1.
Parent collections:
<Posted transaction is not found>
", consolCost.Factory._Instance, ZDate.Today.AddDays(1));
			AssertEquals("Info should be as expected.", expectedInfo, consolCost.GetJobConsolCostInfo());
			ApportionSplitCharge charge = (ApportionSplitCharge)Factory.New(typeof(ApportionSplitCharge), new Guid("dc43d79d-1409-4be8-b7ef-5ded14d753ed"));
			charge.FillWithValidTestData();
			charge.JR_JH = job1.PK;
			charge.JR_RX_NKCostCurrency = "AUD";
			charge.JR_RX_NKSellCurrency = "USD";
			consolCost.ApportionmentCharges.Add(charge);
			charge = (ApportionSplitCharge)Factory.New(typeof(ApportionSplitCharge), new Guid("4637a974-05f6-453b-a128-681ec3764aa8"));
			charge.FillWithValidTestData();
			charge.JR_JH = job2.PK;
			consolCost.ApportionmentCharges.Add(charge);
			expectedInfo = string.Format(CultureInfo.InvariantCulture,
@"Job Consol Cost:
	PK = 1ca892c8-9957-4810-85f3-fdee3baf7005
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {0}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = QWE, Invoice # = 1256, Invoice Date = 20-Nov-09 00:00:00, Currency = AUD, OS Cost Amount = 10, GST is Overridden = Yes, OS GST Amount = 1, Exchange Rate = 2.05, Local Cost Amount = 20.5, Creditor = CRDORG, PPDCLT = ALL, Apportionment Method = SHP, Supplier Cost Reference = ABC, Payment Date = 30-Nov-09 00:00:00, Payment Type = EFT, Cheque # = REF, Bank Account = 2dacf199-1717-42dc-a317-523e0ad624f9, Cheque Book = 066a46ba-5a0e-4f65-b17d-de459a885fac, Tax Rate = 54e4ccc9-4cae-4a8d-a311-da189a6eadcf, Tax Date = {1}, AR Invoice = 8a7ea625-d941-4290-b8fb-8703e483b2bf, AP Invoice = 8ba7ad03-74f2-489f-a3a4-b48dcd71ae42, Is For Collect Invoice = Y, Consol = 23c1a688-2672-4284-af00-cd68b7196911, Is Final = No, Tax Code = , Supply Type = LOC, Tax Branch = TB1.
Parent collections:
Apportionment Charges (2):
Charge: PK = dc43d79d-1409-4be8-b7ef-5ded14d753ed, Type = ApportionSplitCharge|Charge, Charge Type = , Job PK = {2}, Job Number = , Charge Code = 6GQVHOGKXL, Charge Code Type = , Cost Account = , OS Cost Amount = 0, Local Cost Amount = 0, OS Cost Exchange Rate = 1, Cost GST is Overridden = Yes, OS Cost GST Amount = 0, OS Cost WHT Amount = 0, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = 00000000-0000-0000-0000-000000000000, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 0, Local Sell Amount = 0, OS Sell GST Amount = 0, OS Sell WHT Amount = 0, Is Revenue Posted = No, AR Line = 00000000-0000-0000-0000-000000000000, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = 1ca892c8-9957-4810-85f3-fdee3baf7005, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = AUD, Sell Currency = USD, Is In DB = No, Has Changes = Yes, Is Saved By Factory = Yes, Cost GST Rate = , Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , Cost Tax Date = , Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = None.
Charge: PK = 4637a974-05f6-453b-a128-681ec3764aa8, Type = ApportionSplitCharge|Charge, Charge Type = , Job PK = {3}, Job Number = , Charge Code = N51N0S01T7, Charge Code Type = , Cost Account = , OS Cost Amount = 0, Local Cost Amount = 0, OS Cost Exchange Rate = 1, Cost GST is Overridden = Yes, OS Cost GST Amount = 0, OS Cost WHT Amount = 0, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = 00000000-0000-0000-0000-000000000000, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 0, Local Sell Amount = 0, OS Sell GST Amount = 0, OS Sell WHT Amount = 0, Is Revenue Posted = No, AR Line = 00000000-0000-0000-0000-000000000000, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = 1ca892c8-9957-4810-85f3-fdee3baf7005, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = , Sell Currency = , Is In DB = No, Has Changes = Yes, Is Saved By Factory = Yes, Cost GST Rate = , Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , Cost Tax Date = , Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = None.
<Posted transaction is not found>
", consolCost.Factory._Instance, ZDate.Today.AddDays(1), job1.PK, job2.PK);
			AssertEquals("Info should be with apportion charges.", expectedInfo, consolCost.GetJobConsolCostInfo());
			consolCost = (JobConsolCost)new BusinessObjectFactory().New(typeof(JobConsolCost), new Guid("1ca892c8-9957-4810-85f3-fdee3baf7005"));
			consolCost.FillWithValidTestData();
			consolCost.E6_SupplyType = "LOX";
			consolCost.E6_InvoiceNum = "1256";
			consolCost.E6_RX_NKCurrency = "AUD";
			consolCost.E6_ExchangeRate = 2.05M;
			consolCost.E6_ApportionmentMethod = "SHP";
			consolCost.E6_IsForCollectInvoice = true;
			consolCost.E6_GB_CostTaxBranch = consolCost.Factory.NewWithValidTestData<GlbBranch>().PK;
			consolCost.CostTaxBranch.GB_Code = "TB2";
			consolCost.Factory.Save();
			consolCost.E6_InvoiceNum = "";
			consolCost.E6_RX_NKCurrency = "USD";
			consolCost.E6_ExchangeRate = 2M;
			consolCost.E6_ApportionmentMethod = "";
			consolCost.E6_IsForCollectInvoice = false;
			expectedInfo = string.Format(CultureInfo.InvariantCulture,
@"Job Consol Cost:
	PK = 1ca892c8-9957-4810-85f3-fdee3baf7005
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {1}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = None

	Charge Code = 6GQVHOGKXL, Invoice # = , Invoice Date = , Currency = USD, OS Cost Amount = 0, GST is Overridden = No, OS GST Amount = 0, Exchange Rate = 2, Local Cost Amount = 0, Creditor = , PPDCLT = , Apportionment Method = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 00000000-0000-0000-0000-000000000000, Is For Collect Invoice = N, Consol = {0}, Is Final = No, Tax Code = , Supply Type = LOX, Tax Branch = TB2.
	Fields with changes: E6_ApportionmentMethod (SHP, ), E6_ExchangeRate (2.05, 2), E6_InvoiceNum (1256, ), E6_IsForCollectInvoice (Y, N), E6_RX_NKCurrency (AUD, USD).
Parent collections:
", consolCost.E6_ParentID, consolCost.Factory._Instance);
			AssertMultilineASCIIEquals("Info for saved object should be with changed fields.", expectedInfo, consolCost.GetJobConsolCostInfo());
		}

		[TestDate(2009, 11, 20)]
		public void TestGetJobConsolCostInfoWithEmptyOrNullChargeCollection()
		{
			var chargeCode = TestObjectCreator.CC3;
			var creditor = TestObjectCreator.AALSHI;
			var consol = TestObjectCreator.CreateConsol();
			var parameters = consol.GetTaxCalculationParameters();
			parameters.Organisation = creditor;
			chargeCode.GetGSTRate(parameters, out _);
			var date = new ZDateTime(2009, 11, 20);

			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol);
			TestObjectCreator.CreateJob(shipment1, false);

			var consolCost = testObjectCreator.CreateConsolCost(consol, chargeCode, 10m);
			consolCost.E6_SupplyType = "LOC";
			consolCost.E6_OH_Creditor = creditor.PK;
			consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			consolCost.E6_InvoiceNum = "Test001";
			consolCost.E6_InvoiceDate = date;
			consolCost.E6_PaymentDate = date;
			consolCost.E6_PaymentType = ReceiptTypes.Cheque;
			consolCost.E6_AB_BankAccount = testObjectCreator.AUDBankAccount.PK;
			consolCost.E6_AK_ChequeBook = testObjectCreator.AUDChequeBook.PK;
			consolCost.E6_ChequeOrReference = testObjectCreator.AUDChequeBook.AK_StartNo.ToString();

			Factory.Save();

			AssertEquals("Pre-condition", 1, consolCost.ApportionmentCharges.Count);

			string expectedInfo = FormattableString.Invariant($@"Job Consol Cost:
	PK = {consolCost.PK}
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {Factory._Instance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = ZZCC3");
			string expectedInfo1 = @"Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.ConsolCosting.JobConsolCostCollection";
			string expectedInfo2 = @"Apportionment Charges (1):
Charge: PK = ";
			string expectedInfo3 = @"Apportionment charges in Consol Cost collection but was not in chargeCollection:
Charge: PK = ";

			var actualMessage = consolCost.GetJobConsolCostInfo(Array.Empty<ApportionSplitCharge>());
			AssertContains("Info should be as expected.", expectedInfo, actualMessage);
			AssertContains(expectedInfo1, actualMessage);
			AssertContains(expectedInfo2, actualMessage);
			AssertNotContains(expectedInfo3, actualMessage);

			actualMessage = consolCost.GetJobConsolCostInfo();
			AssertContains("Info should be as expected.", expectedInfo, actualMessage);
			AssertContains(expectedInfo1, actualMessage);
			AssertContains(expectedInfo2, actualMessage);
			AssertNotContains(expectedInfo3, actualMessage);

			actualMessage = consolCost.GetJobConsolCostInfo(chargeCollection: [Factory.NewWithValidTestData<ApportionSplitCharge>()]);
			AssertContains("Info should be as expected.", expectedInfo, actualMessage);
			AssertContains(expectedInfo1, actualMessage);
			AssertContains(expectedInfo2, actualMessage);
			AssertContains(expectedInfo3, actualMessage);
		}

		[TestDate(2017, 09, 28)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		public void TestGetTransactionHeaderWithLinesInfo()
		{
			ZDateTime invoiceDate = new ZDateTime(2009, 11, 20);
			ZDecimal sumOfLinesAmount = 0;
			ARInvoice invoice = (ARInvoice)Factory.New(typeof(ARInvoice), new Guid("E66A7FED-B46B-4322-BDF3-616D341CF2ED"));
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_PostDate = invoiceDate;
			invoice.AH_TransactionNum = "123";
			invoice.Lines.Add(Factory.NewWithValidTestData<APInvoiceLine>());
			invoice.Lines.Add(Factory.NewWithValidTestData<UAInvoiceLine>());
			invoice.Lines.Add(Factory.NewWithValidTestData<ARInvoiceLine>());
			foreach (InvoicingLineBase line in invoice.Lines)
			{
				line.AL_PostDate = invoiceDate;
				line.AL_ReverseDate = invoiceDate;
				line.AL_OverseasTotal = 100M;
				line.AL_LineAmount = line.AL_OSAmount;
				sumOfLinesAmount += line.AL_OSAmount;
			}

			invoice.AH_OSTotal = sumOfLinesAmount;
			invoice.AH_OSExTaxAmount = invoice.AH_OSTotalAmount;
			string expectedInfo = string.Format(@"Header: PK = {0}, Ledger = AR, Transaction Type = INV, Invoice Date = , Post Date = 20-Nov-09 00:00:00, Invoice Amount = -100, GST Amount = 0, OS Total = -100, Exchange Rate = 1, Currency = , Outstanding Amount = -100, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 123, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Line Details:
Line: PK = {1}, Charge Code = , GL Account = , Type = CST, OS Amount = -100, Local Amount = -100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 20-Nov-09 00:00:00, Reverse Date = 20-Nov-09 00:00:00, Post To GL = N, Reverse To GL = N, Header PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
Line: PK = {2}, Charge Code = , GL Account = , Type = UCT, OS Amount = -100, Local Amount = -100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 20-Nov-09 00:00:00, Reverse Date = 20-Nov-09 00:00:00, Post To GL = N, Reverse To GL = N, Header PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
Line: PK = {3}, Charge Code = , GL Account = , Type = REV, OS Amount = 100, Local Amount = 100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 20-Nov-09 00:00:00, Reverse Date = 20-Nov-09 00:00:00, Post To GL = N, Reverse To GL = N, Header PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
", invoice.PK, invoice.Lines[0].PK, invoice.Lines[1].PK, invoice.Lines[2].PK);
			AssertEquals("Info should be as expected.", expectedInfo, invoice.GetTransactionHeaderWithLinesInfo());

			ARInvoice originalInvoice = (ARInvoice)Factory.New(typeof(ARInvoice), new Guid("6B47F711-6F6C-4E04-8FB3-37082942F227"));
			originalInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			originalInvoice.AH_TransactionType = TransactionTypes.Invoice;
			originalInvoice.AH_PostDate = invoiceDate;
			originalInvoice.AH_TransactionNum = "123";
			originalInvoice.Lines.Add(Factory.NewWithValidTestData<APInvoiceLine>());
			originalInvoice.Lines.Add(Factory.NewWithValidTestData<UAInvoiceLine>());
			originalInvoice.Lines.Add(Factory.NewWithValidTestData<ARInvoiceLine>());
			ZDecimal sumOfOriginalLinesAmount = 0;
			foreach (InvoicingLineBase line in originalInvoice.Lines)
			{
				line.AL_PostDate = invoiceDate;
				line.AL_ReverseDate = invoiceDate;
				line.AL_OverseasTotal = 100M;
				line.AL_LineAmount = line.AL_OSAmount;
				sumOfOriginalLinesAmount += line.AL_OSAmount;
			}

			originalInvoice.AH_OSTotal = sumOfOriginalLinesAmount;
			originalInvoice.AH_OSExTaxAmount = originalInvoice.AH_OSTotalAmount;
			invoice.OriginalTransaction = originalInvoice;

			expectedInfo = string.Format(@"Header: PK = {0}, Ledger = AR, Transaction Type = INV, Invoice Date = , Post Date = 20-Nov-09 00:00:00, Invoice Amount = -100, GST Amount = 0, OS Total = -100, Exchange Rate = 1, Currency = , Outstanding Amount = -100, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 123, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Line Details:
Line: PK = {1}, Charge Code = , GL Account = , Type = CST, OS Amount = -100, Local Amount = -100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 20-Nov-09 00:00:00, Reverse Date = 20-Nov-09 00:00:00, Post To GL = N, Reverse To GL = N, Header PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
Line: PK = {2}, Charge Code = , GL Account = , Type = UCT, OS Amount = -100, Local Amount = -100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 20-Nov-09 00:00:00, Reverse Date = 20-Nov-09 00:00:00, Post To GL = N, Reverse To GL = N, Header PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
Line: PK = {3}, Charge Code = , GL Account = , Type = REV, OS Amount = 100, Local Amount = 100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 20-Nov-09 00:00:00, Reverse Date = 20-Nov-09 00:00:00, Post To GL = N, Reverse To GL = N, Header PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
Original Transaction:
Header: PK = {4}, Ledger = AR, Transaction Type = INV, Invoice Date = , Post Date = 20-Nov-09 00:00:00, Invoice Amount = -100, GST Amount = 0, OS Total = -100, Exchange Rate = 1, Currency = , Outstanding Amount = -100, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 123, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Line Details:
Line: PK = {5}, Charge Code = , GL Account = , Type = CST, OS Amount = -100, Local Amount = -100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 20-Nov-09 00:00:00, Reverse Date = 20-Nov-09 00:00:00, Post To GL = N, Reverse To GL = N, Header PK = 6b47f711-6f6c-4e04-8fb3-37082942f227, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
Line: PK = {6}, Charge Code = , GL Account = , Type = UCT, OS Amount = -100, Local Amount = -100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 20-Nov-09 00:00:00, Reverse Date = 20-Nov-09 00:00:00, Post To GL = N, Reverse To GL = N, Header PK = 6b47f711-6f6c-4e04-8fb3-37082942f227, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
Line: PK = {7}, Charge Code = , GL Account = , Type = REV, OS Amount = 100, Local Amount = 100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 20-Nov-09 00:00:00, Reverse Date = 20-Nov-09 00:00:00, Post To GL = N, Reverse To GL = N, Header PK = 6b47f711-6f6c-4e04-8fb3-37082942f227, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
", invoice.PK, invoice.Lines[0].PK, invoice.Lines[1].PK, invoice.Lines[2].PK, originalInvoice.PK, originalInvoice.Lines[0].PK, originalInvoice.Lines[1].PK, originalInvoice.Lines[2].PK);
			AssertEquals("Info should be as expected.", expectedInfo, invoice.GetTransactionHeaderWithLinesInfo());
		}

		public void TestGetTransactionHeaderWithLinesInfo_WithLinesNotLinkedWithHeader()
		{
			var date = new ZDateTime(2023, 9, 20, 0, 0, 0);
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_TransactionNum = "000001";
			invoice.AH_InvoiceDate = date;
			invoice.AH_PostDate = date;

			var line = Factory.NewWithValidTestData<APInvoiceLine>();

			string expectedInfo = $@"Header: PK = {invoice.PK}, Ledger = AP, Transaction Type = INV, Invoice Date = 20-Sep-23 00:00:00, Post Date = 20-Sep-23 00:00:00, Invoice Amount = 0, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 000001, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
The header has no lines. Top 5 suspicious lines in the factory:
Line: PK = {line.PK}, Charge Code = , GL Account = , Type = CST, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = 00000000-0000-0000-0000-000000000000, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = No.";
			AssertEquals(expectedInfo, invoice.GetTransactionHeaderWithLinesInfo().Trim());

			Factory.NewWithValidTestData<APInvoiceLine>();
			Factory.NewWithValidTestData<APInvoiceLine>();
			Factory.NewWithValidTestData<APInvoiceLine>();
			Factory.NewWithValidTestData<APInvoiceLine>();
			Factory.NewWithValidTestData<APInvoiceLine>();

			AssertEquals("Record top 5 lines", 5, Regex.Matches(invoice.GetTransactionHeaderWithLinesInfo(), "Line: PK = ").Count);
		}

		public void TestGetTransactionHeaderTaxAmountWithLinesTaxAmountInfo()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.Lines.RemoveAndDeleteAll();
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			invoice.AH_ExchangeRate = 0.5;
			var line = invoice.Lines.AddNew() as InvoicingLineBase;
			line.AL_OSExTaxAmount = 100m;
			line.AL_AT = TestObjectCreator.GST1.PK;

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AL_ARLine = line.PK;
			charge.SetAmountsFromLinkedLinesForTests();

			var expectInfo = $@"Header: OSTaxAmount = 10, LocalTaxAmount = 20
Lines:
OSExTaxAmount = 100, LocalExTaxAmount = 200, OSTaxAmount = 10, LocalTaxAmount = 20, Charge JR_OSSellGSTAmt_Calc = 10, Charge JR_Sell_LocalGSTAmount = 20";
			AssertMultilineASCIIEquals(expectInfo, invoice.GetTransactionHeaderTaxAmountWithLinesTaxAmountInfo());
		}

		[TestDate(2000, 10, 10)]
		[SuspendCriticalValidation]
		public void TestGetCashBasisVATInfo()
		{
			var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
			var line = invoice.Lines[0];
			var lineGST = line.AL_GSTVAT;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_GSTVAT = lineGST;
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			var matchLink = TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice);
			matchLink.SkipCashBasisVATCreationForTestOnly = true;
			var cashVATRecord = TestObjectCreator.CreateCashBasisVAT(line, -90, -9, new ZDateTime(2000, 11, 11), matchLink);
			AssertEquals(string.Format(@"Cash VAT Recognition Record: PK = {0}, Post Date = 11-Nov-00 00:00:00, Tax Base Amount = -90, Tax Amount = -9, Transaction Line = {1}, Company Code = EDI, Match Group Number = M001, Is In DB = No, Has Changes = Yes.
Line: PK = {1}, Charge Code = , GL Account = {3}, Type = CST, OS Amount = -110, Local Amount = -100, GST = -10, Tax Rate = ZZGST1, Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {2}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
", cashVATRecord.PK, line.PK, invoice.PK, line.GLHeader.AccountNum), cashVATRecord.GetCashBasisVATInfo());
			Factory.Save();
			cashVATRecord.YC_MatchGroupNum = "";
			AssertEquals(string.Format(@"Cash VAT Recognition Record: PK = {0}, Post Date = 11-Nov-00 00:00:00, Tax Base Amount = -90, Tax Amount = -9, Transaction Line = {1}, Company Code = EDI, Match Group Number = , Is In DB = Yes, Has Changes = Yes.
	Fields with changes: YC_MatchGroupNum (M001, ).
Line: PK = {1}, Charge Code = , GL Account = {3}, Type = CST, OS Amount = -110, Local Amount = -100, GST = -10, Tax Rate = ZZGST1, Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 10-Oct-00 00:00:00, Reverse Date = 10-Oct-00 00:00:00, Post To GL = N, Reverse To GL = N, Header PK = {2}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = Yes, Is Final = No, Sub Accounts = , Has Changes = No.
", cashVATRecord.PK, line.PK, invoice.PK, line.GLHeader.AccountNum), cashVATRecord.GetCashBasisVATInfo());
			cashVATRecord.YC_AL_TransactionLine = ZGuid.Empty;
			AssertEquals(string.Format(@"Cash VAT Recognition Record: PK = {0}, Post Date = 11-Nov-00 00:00:00, Tax Base Amount = -90, Tax Amount = -9, Transaction Line = 00000000-0000-0000-0000-000000000000, Company Code = EDI, Match Group Number = , Is In DB = Yes, Has Changes = Yes.
	Fields with changes: YC_AL_TransactionLine ({1}, 00000000-0000-0000-0000-000000000000), YC_MatchGroupNum (M001, ).
", cashVATRecord.PK, line.PK), cashVATRecord.GetCashBasisVATInfo());
		}

		public void TestGetExchangeRateOriginalInfo()
		{
			var exchangeRate = Factory.NewWithValidTestData<ExchangeRate>();
			exchangeRate.JF_RX_NKRateCurrency = "USD";
			exchangeRate.JF_JH = TestObjectCreator.Job1.PK;
			exchangeRate.JF_BaseRate = 1.1m;
			exchangeRate.JF_CFXMinimum = 2.2m;
			exchangeRate.JF_CFXPercent = 0.5m;
			exchangeRate.JF_IsTransformed = false;
			exchangeRate.JF_OH_Org = TestObjectCreator.AALSHI.PK;
			exchangeRate.JF_OrgType = "DEB";

			var expectMessage = $"ExchangeRate original values: PK = {exchangeRate.PK}, Currency Code = USD, Job Header PK = {TestObjectCreator.Job1.PK}, Base Rate = 1.1, CFX Minimum = 2.2, CFX Percent = 0.5, Is Transformed = N, Org Header PK = {TestObjectCreator.AALSHI.PK}, Org Type = DEB.";
			AssertEquals(expectMessage, exchangeRate.GetExchangeRateOriginalInfo());
			Factory.Save();

			exchangeRate.JF_CFXMinimum = 2m;
			AssertEquals(expectMessage, exchangeRate.GetExchangeRateOriginalInfo());

			exchangeRate.Delete();
			AssertEquals(expectMessage, exchangeRate.GetExchangeRateOriginalInfo());

			exchangeRate = Factory.NewWithValidTestData<ExchangeRate>();
			exchangeRate.JF_RX_NKRateCurrency = "USD";
			exchangeRate.JF_JH = TestObjectCreator.Job1.PK;

			exchangeRate.Delete();
			expectMessage = "Original property values for deleted and not previously saved BusinessObject are not accessible";
			AssertEquals(expectMessage, exchangeRate.GetExchangeRateOriginalInfo());
		}

		public void TestGetAccComplianceDocumentHeaderInfo()
		{
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.AALSHI);
			address.OA_Code = "aaa";

			var header = Factory.NewWithValidTestData<APComplianceDocumentHeader>();
			header.ADH_Ledger = LedgerTypes.AccountsPayable;
			header.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			header.ADH_OH_Organisation = TestObjectCreator.ABIGAS.PK;
			header.ADH_OA_AddressOverride = address.PK;

			var customsCode = header.Organisation.CustomsCodes.AddNew();
			var addressOfParentTransaction = TestObjectCreator.ABIGAS.Addresses.AddNew();
			addressOfParentTransaction.OA_Code = "bbb";
			customsCode.OK_CodeType = "VAT";
			customsCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			customsCode.OK_OA_PremisesAddress = addressOfParentTransaction.PK;

			var complianceline = header.ComplianceDocumentLines.AddNew();
			var pivot = complianceline.ComplianceDocumentPivots.AddNew();
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			pivot.ADP_AL = transactionLine.PK;
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.TransactionHeader.AH_OA_InvoiceAddressOverride = address.PK;

			AssertEquals($"Compliance Document Header: PK = {header.PK}, Organization = ABIGAS, Organization for Address = AALSHI, Address = aaa, Address Of Organization CustomeCode = bbb, Address Of Parent Transactiton = aaa, Sending Documents Address = {header.Organisation.AddressForSendingAPDocuments.OA_Code}, Is In DB = No, Has Changes = Yes.\r\n" +
				$"{transactionHeader.GetTransactionHeaderInfo()}\r\n", header.GetAccComplianceDocumentHeaderInfo());
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
			}
		}

		TestObjectCreator testObjectCreator;
	}
}
