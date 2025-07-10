using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class ARAPTransactionLinesSPTest : ScriptTest
	{
		[ExpectNoExceptions]
		public void TestOrgReferenceListDoesNotCauseException()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00001", TestObjectCreator.AUD, 1m, TestObjectCreator.LocalClient);
			var line = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "Desc", 100m);
			TestObjectCreator.CreateCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			for (var i = 0; i < 50; i++)
			{
				var order = Factory.NewWithValidTestData<Order>();
				order.JD_JS = shipment.PK;
				order.JD_OrderNumber = new string((char)i, 35);
			}
			Factory.Save();

			RunScript(LedgerTypes.AccountsReceivable, true, System.Array.Empty<string>(), System.Array.Empty<string>());
		}

		public void TestTransactionLinesInvoiceTotalWithOtherTaxes()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.AUD, 1, TestObjectCreator.Debtor);
			TestObjectCreator.CreateARInvoiceLineWithJobCharge(arInvoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc1", 1000m, TestObjectCreator.GST1.PK);
			AssertEquals(100m, arInvoice.AH_GSTAmount);
			arInvoice.AH_LocalTaxAmountOtherTaxes = arInvoice.AH_OSTaxAmountOtherTaxes = 200M;

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP01", TestObjectCreator.AUD, 1.0m, 1000m, 100m, 0m, 1000m, 100m, 0m, TestObjectCreator.Creditor1);
			TestObjectCreator.CreateJobCharge(apInvoice.Lines[0], TestObjectCreator.Job1, TestObjectCreator.CC1);
			apInvoice.AH_LocalTaxAmountOtherTaxes = apInvoice.AH_OSTaxAmountOtherTaxes = -200M;

			Factory.Save();

			var result = RunScript(LedgerTypes.AccountsReceivable, false, System.Array.Empty<string>(), System.Array.Empty<string>());
			AssertEquals(1300m, result.Rows[0]["InvoiceTotal"]);

			result = RunScript(LedgerTypes.AccountsPayable, false, System.Array.Empty<string>(), System.Array.Empty<string>());
			AssertEquals(1300m, result.Rows[0]["InvoiceTotal"]);
		}

		public void TestAccountNameBeShownCorrectlyWithChineseCharacters()
		{
			var expectedAccountName = "测试中文";

			TestObjectCreator.ABIGAS.OH_FullName = expectedAccountName;
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			Factory.Save();

			var result = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				new string[] { TestObjectCreator.ABIGAS.OH_Code },
				System.Array.Empty<string>());

			AssertEquals("Result for Organisation list", 1, result.Rows.Count);
			AssertEquals("AccountName", expectedAccountName, result.Rows[0]["AccountName"]);

			result = RunScript(
				LedgerTypes.AccountsReceivable,
				true,
				new string[] { TestObjectCreator.ABIGAS.OH_Code },
				System.Array.Empty<string>());

			AssertEquals("Result for Organisation list", 1, result.Rows.Count);
			AssertEquals("AccountName", expectedAccountName, result.Rows[0]["AccountName"]);
		}

		public void TestARSettlementGroupList()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.LocalClient, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.LocalClient2, glAccount.PK);
			Factory.Save();

			DataTable resultForOrg = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				new string[] { TestObjectCreator.ABIGAS.OH_Code, TestObjectCreator.LocalClient.OH_Code, TestObjectCreator.LocalClient2.OH_Code },
				System.Array.Empty<string>());

			AssertEquals("Result for Organisation list", 3, resultForOrg.Rows.Count);

			DataTable resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				System.Array.Empty<string>(),
				new string[] { TestObjectCreator.ABIGAS.OH_Code });

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);

			AssertEquals("Results must contain the same amount of rows.", resultForSettlementGroup.Rows.Count, resultForOrg.Rows.Count);

			resultForOrg = RunScript(
				LedgerTypes.AccountsReceivable,
				true,
				new string[] { TestObjectCreator.ABIGAS.OH_Code, TestObjectCreator.LocalClient.OH_Code, TestObjectCreator.LocalClient2.OH_Code },
				System.Array.Empty<string>());

			AssertEquals("Result for Organisation list", 3, resultForOrg.Rows.Count);

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsReceivable,
				true,
				System.Array.Empty<string>(),
				new string[] { TestObjectCreator.ABIGAS.OH_Code });

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);

			AssertEquals("Results must contain the same amount of rows.", resultForSettlementGroup.Rows.Count, resultForOrg.Rows.Count);

			TestObjectCreator.ABIGAS.ARSettlementGroupPK = TestObjectCreator.Agent.PK;
			Factory.Save();

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				System.Array.Empty<string>(),
				new string[] { TestObjectCreator.ABIGAS.OH_Code });

			AssertEquals("Result for SettlementGroup list", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result don't contain ABIGAS", 0, resultForSettlementGroup.Select("AccountCode = 'ABIGAS'").Length);

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsReceivable,
				true,
				System.Array.Empty<string>(),
				new string[] { TestObjectCreator.ABIGAS.OH_Code });

			AssertEquals("Result for SettlementGroup list", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result don't contain ABIGAS", 0, resultForSettlementGroup.Select("AccountCode = 'ABIGAS'").Length);
		}

		public void TestAPSettlementGroupList()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.AALSHI.APSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.Creditor1.APSettlementGroupPK = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.Creditor2.APSettlementGroupPK = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.AALSHI, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.Creditor1, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.Creditor2, glAccount.PK);
			Factory.Save();

			DataTable resultForOrg = RunScript(
				LedgerTypes.AccountsPayable,
				false,
				new string[] { TestObjectCreator.AALSHI.OH_Code, TestObjectCreator.Creditor1.OH_Code, TestObjectCreator.Creditor2.OH_Code },
				System.Array.Empty<string>());

			AssertEquals("Result for Organisation list", 3, resultForOrg.Rows.Count);

			DataTable resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsPayable,
				false,
				System.Array.Empty<string>(),
				new string[] { TestObjectCreator.AALSHI.OH_Code });

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);

			AssertEquals("Results must contain the same amount of rows.", resultForSettlementGroup.Rows.Count, resultForOrg.Rows.Count);

			resultForOrg = RunScript(
				LedgerTypes.AccountsPayable,
				true,
				new string[] { TestObjectCreator.AALSHI.OH_Code, TestObjectCreator.Creditor1.OH_Code, TestObjectCreator.Creditor2.OH_Code },
				System.Array.Empty<string>());

			AssertEquals("Result for Organisation list", 3, resultForOrg.Rows.Count);

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsPayable,
				true,
				System.Array.Empty<string>(),
				new string[] { TestObjectCreator.AALSHI.OH_Code });

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);

			AssertEquals("Results must contain the same amount of rows.", resultForSettlementGroup.Rows.Count, resultForOrg.Rows.Count);

			TestObjectCreator.AALSHI.APSettlementGroupPK = TestObjectCreator.Agent.PK;
			Factory.Save();

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsPayable,
				false,
				System.Array.Empty<string>(),
				new string[] { TestObjectCreator.AALSHI.OH_Code });

			AssertEquals("Result for SettlementGroup list", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result don't contain AALSHI", 0, resultForSettlementGroup.Select("AccountCode = 'AALSHI'").Length);

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsPayable,
				true,
				System.Array.Empty<string>(),
				new string[] { TestObjectCreator.AALSHI.OH_Code });

			AssertEquals("Result for SettlementGroup list", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result don't contain AALSHI", 0, resultForSettlementGroup.Select("AccountCode = 'AALSHI'").Length);
		}

		public void TestARCountryList()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			var orgHeader1 = TestObjectCreator.CreateOrgHeader("TSTORG1", false, true, "USCHI");
			var invoice1 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Inv001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, orgHeader1, glAccount.PK);

			var orgHeader2 = TestObjectCreator.CreateOrgHeader("TSTORG2", false, true, "AUSYD");
			var invoice2 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Inv002", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, orgHeader2, glAccount.PK);

			Factory.Save();

			DataTable resultForUS = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				System.Array.Empty<string>(),
				System.Array.Empty<string>(),
				new string[1] { orgHeader1.CountryCode });

			AssertEquals("Result for Country list", 1, resultForUS.Rows.Count);
			AssertEquals("Country should be ", orgHeader1.CountryCode, resultForUS.Rows[0]["CountryCode"]);

			DataTable resultForAU = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				System.Array.Empty<string>(),
				System.Array.Empty<string>(),
				System.Array.Empty<string>(),
				new string[1] { orgHeader1.CountryCode });

			AssertEquals("Result for Country list", 1, resultForAU.Rows.Count);
			AssertEquals("Country should be ", orgHeader2.CountryCode, resultForAU.Rows[0]["CountryCode"]);
		}

		public void TestAPCountryList()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			var orgHeader1 = TestObjectCreator.CreateOrgHeader("TSTORG1", false, true, "USCHI");
			var invoice1 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, orgHeader1, glAccount.PK);

			var orgHeader2 = TestObjectCreator.CreateOrgHeader("TSTORG2", false, true, "AUSYD");
			var invoice2 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv002", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, orgHeader2, glAccount.PK);

			Factory.Save();

			DataTable resultForUS = RunScript(
				LedgerTypes.AccountsPayable,
				false,
				System.Array.Empty<string>(),
				System.Array.Empty<string>(),
				new string[1] { orgHeader1.CountryCode });

			AssertEquals("Result for Country list", 1, resultForUS.Rows.Count);
			AssertEquals("Country should be ", orgHeader1.CountryCode, resultForUS.Rows[0]["CountryCode"]);

			DataTable resultForAU = RunScript(
				LedgerTypes.AccountsPayable,
				false,
				System.Array.Empty<string>(),
				System.Array.Empty<string>(),
				System.Array.Empty<string>(),
				new string[1] { orgHeader1.CountryCode });

			AssertEquals("Result for Country list", 1, resultForAU.Rows.Count);
			AssertEquals("Country should be ", orgHeader2.CountryCode, resultForAU.Rows[0]["CountryCode"]);
		}

		public void TestLineCurrencyLineExchangeColumnExist()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			Factory.Save();

			DataTable resultForOrg = RunScript(
				LedgerTypes.AccountsReceivable,
				false,
				new string[] { TestObjectCreator.ABIGAS.OH_Code, TestObjectCreator.LocalClient.OH_Code, TestObjectCreator.LocalClient2.OH_Code },
				System.Array.Empty<string>());

			AssertEquals("Result", 1, resultForOrg.Rows.Count);
			AssertEquals("Result Should Have Header Currency Code", "AUD", resultForOrg.Rows[0]["CurrencyCode"]);
			AssertEquals("Result Should Have Line Currency Code", "AUD", resultForOrg.Rows[0]["LineCurrencyCode"]);
			AssertEquals("Result Should Have Header Exchange Rate", "1", resultForOrg.Rows[0]["ExchangeRate"].ToString());
			AssertEquals("Result Should Have Line Exchange Rate", "1", resultForOrg.Rows[0]["LineExchangeRate"].ToString());
		}

		public void TestJobNumbersShowWithMultipleJobARInvoice()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00001", TestObjectCreator.AUD, 1m, TestObjectCreator.LocalClient);
			var line1 = TestObjectCreator.CreateARInvoiceLine(arInvoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "Desc", 100m);
			TestObjectCreator.CreateCharge(line1, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			var line2 = TestObjectCreator.CreateARInvoiceLine(arInvoice, TestObjectCreator.Job2, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "Desc", 200m);
			TestObjectCreator.CreateCharge(line2, TestObjectCreator.Job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Factory.Save();

			DataTable result = RunScript(LedgerTypes.AccountsReceivable, false, System.Array.Empty<string>(), System.Array.Empty<string>());

			AssertEquals("Result", 2, result.Rows.Count);
			var jobNumbers = new string[] { TestObjectCreator.Job1.JH_JobNum, TestObjectCreator.Job2.JH_JobNum };
			Assert("Job on line 1", jobNumbers.Contains(result.Rows[0]["JobNumber"]));
			Assert("Job on line 2", jobNumbers.Contains(result.Rows[1]["JobNumber"]));
			AssertNotEquals("Expect different Job Numbers on the lines", result.Rows[0]["JobNumber"], result.Rows[1]["JobNumber"]);
		}

		public void TestGSFullNameTruncatedException()
		{
			ZStringBuilder name = new ZStringBuilder();
			for (int i = 1; i <= 256; i++)
			{
				name.Append("a");
			}

			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = name.ToString();
			staff.GS_Code = "ABC";

			var assignment = Factory.New<OrgStaffAssignments>();
			assignment.O8_GC = GlbCompany.CurrentCompany.PK;
			assignment.O8_OH = TestObjectCreator.ABIGAS.PK;
			assignment.O8_GS_NKPersonResponsible = staff.GS_Code;
			assignment.O8_Department = "ALL";

			assignment.O8_Role = "SAL";
			Factory.Save();
			AssertNoExceptionThrown(() => RunScript(LedgerTypes.AccountsPayable, true, System.Array.Empty<string>(), System.Array.Empty<string>()));
			AssertNoExceptionThrown(() => RunScript(LedgerTypes.AccountsPayable, false, System.Array.Empty<string>(), System.Array.Empty<string>()));

			assignment.O8_Role = "CRE";
			Factory.Save();
			AssertNoExceptionThrown(() => RunScript(LedgerTypes.AccountsPayable, true, System.Array.Empty<string>(), System.Array.Empty<string>()));
			AssertNoExceptionThrown(() => RunScript(LedgerTypes.AccountsPayable, false, System.Array.Empty<string>(), System.Array.Empty<string>()));

			assignment.O8_Role = "CUS";
			Factory.Save();
			AssertNoExceptionThrown(() => RunScript(LedgerTypes.AccountsPayable, true, System.Array.Empty<string>(), System.Array.Empty<string>()));
			AssertNoExceptionThrown(() => RunScript(LedgerTypes.AccountsPayable, false, System.Array.Empty<string>(), System.Array.Empty<string>()));

			assignment.O8_Role = "ACT";
			Factory.Save();
			AssertNoExceptionThrown(() => RunScript(LedgerTypes.AccountsPayable, true, System.Array.Empty<string>(), System.Array.Empty<string>()));
		}

		public void TestTaxBranchValueIsReturnedForAP_WhenValueIsNull()
		{
			SetupTaxBranchForInvoices(typeof(APInvoice), new ZGuid[] { GlbBranch.CurrentBranch.PK, TestObjectCreator.NonCurrentBranch.PK } );

			var result = RunScript(LedgerTypes.AccountsPayable, false, System.Array.Empty<string>(), System.Array.Empty<string>());

			var taxBranchCodes = new string[] { GlbBranch.CurrentBranch.GB_Code, TestObjectCreator.NonCurrentBranch.GB_Code };
			AssertEquals("Result", 2, result.Rows.Count);
			Assert("TaxBranch on line 1", taxBranchCodes.Contains(result.Rows[0]["TaxBranch"]));
			Assert("TaxBranch on line 2", taxBranchCodes.Contains(result.Rows[1]["TaxBranch"]));
			AssertNotEquals("Expect different Tax Branches on the lines", result.Rows[0]["TaxBranch"], result.Rows[1]["TaxBranch"]);
		}

		public void TestTaxBranchValueIsReturnedForAP_WhenValueIsSpecified()
		{
			SetupTaxBranchForInvoices(typeof(APInvoice), new ZGuid[] { GlbBranch.CurrentBranch.PK, TestObjectCreator.NonCurrentBranch.PK });

			var result = RunScript(LedgerTypes.AccountsPayable, false, System.Array.Empty<string>(), System.Array.Empty<string>(), null, null, GlbBranch.CurrentBranch.PK);

			AssertEquals("Result", 1, result.Rows.Count);
			AssertEquals("TaxBranch on line 1", GlbBranch.CurrentBranch.GB_Code, result.Rows[0]["TaxBranch"]);
		}

		public void TestTaxBranchValueIsReturnedForAR_WhenValueIsNull()
		{
			SetupTaxBranchForInvoices(typeof(ARInvoice), new ZGuid[] { GlbBranch.CurrentBranch.PK, TestObjectCreator.NonCurrentBranch.PK });

			var result = RunScript(LedgerTypes.AccountsReceivable, false, System.Array.Empty<string>(), System.Array.Empty<string>());

			var taxBranchCodes = new string[] { GlbBranch.CurrentBranch.GB_Code, TestObjectCreator.NonCurrentBranch.GB_Code };
			AssertEquals("Result", 2, result.Rows.Count);
			Assert("TaxBranch on line 1", taxBranchCodes.Contains(result.Rows[0]["TaxBranch"]));
			Assert("TaxBranch on line 2", taxBranchCodes.Contains(result.Rows[1]["TaxBranch"]));
			AssertNotEquals("Expect different Tax Branches on the lines", result.Rows[0]["TaxBranch"], result.Rows[1]["TaxBranch"]);
		}

		public void TestTaxBranchValueIsReturnedForAR_WhenValueIsSpecified()
		{
			SetupTaxBranchForInvoices(typeof(ARInvoice), new ZGuid[] { GlbBranch.CurrentBranch.PK, TestObjectCreator.NonCurrentBranch.PK });

			var result = RunScript(LedgerTypes.AccountsReceivable, false, System.Array.Empty<string>(), System.Array.Empty<string>(), null, null, GlbBranch.CurrentBranch.PK);

			AssertEquals("Result", 1, result.Rows.Count);
			AssertEquals("TaxBranch on line 1", GlbBranch.CurrentBranch.GB_Code, result.Rows[0]["TaxBranch"]);
		}

		public void TestSupplyTypeValueIsReturnedForAP_WhenValueIsNull()
		{
			var supplyTypes = new string[] { AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOX };
			SetupSupplyTypeForInvoiceLines(typeof(APInvoice), supplyTypes);

			var result = RunScript(LedgerTypes.AccountsPayable, false, System.Array.Empty<string>(), System.Array.Empty<string>());

			AssertEquals("Result", 2, result.Rows.Count);
			Assert("SupplyType on line 1", supplyTypes.Contains(result.Rows[0]["SupplyType"]));
			Assert("SupplyType on line 2", supplyTypes.Contains(result.Rows[1]["SupplyType"]));
			AssertNotEquals("Expect different Supply Type on the lines", result.Rows[0]["SupplyType"], result.Rows[1]["SupplyType"]);
		}

		public void TestSupplyTypeValueIsReturnedForAP_WhenValueIsSpecified()
		{
			var supplyTypes = new string[] { AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOX };
			SetupSupplyTypeForInvoiceLines(typeof(APInvoice), supplyTypes);

			var result = RunScript(LedgerTypes.AccountsPayable, false, System.Array.Empty<string>(), System.Array.Empty<string>(), null, null, null, supplyTypes[0]);

			AssertEquals("Result", 1, result.Rows.Count);
			AssertEquals("SupplyType on line 1", supplyTypes[0], result.Rows[0]["SupplyType"]);
		}

		public void TestSupplyTypeValueIsReturnedForAR_WhenValueIsNull()
		{
			var supplyTypes = new string[] { AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOX };
			SetupSupplyTypeForInvoiceLines(typeof(ARInvoice), supplyTypes);

			var result = RunScript(LedgerTypes.AccountsReceivable, false, System.Array.Empty<string>(), System.Array.Empty<string>());

			AssertEquals("Result", 2, result.Rows.Count);
			Assert("SupplyType on line 1", supplyTypes.Contains(result.Rows[0]["SupplyType"]));
			Assert("SupplyType on line 2", supplyTypes.Contains(result.Rows[1]["SupplyType"]));
			AssertNotEquals("Expect different Supply Type on the lines", result.Rows[0]["SupplyType"], result.Rows[1]["SupplyType"]);
		}

		public void TestSupplyTypeValueIsReturnedForAR_WhenValueIsSpecified()
		{
			var supplyTypes = new string[] { AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOX };
			SetupSupplyTypeForInvoiceLines(typeof(ARInvoice), supplyTypes);

			var result = RunScript(LedgerTypes.AccountsReceivable, false, System.Array.Empty<string>(), System.Array.Empty<string>(), null, null, null, supplyTypes[0]);

			AssertEquals("Result", 1, result.Rows.Count);
			AssertEquals("SupplyType on line 1", supplyTypes[0], result.Rows[0]["SupplyType"]);
		}

		public void SetupTaxBranchForInvoices(System.Type invoiceType, ZGuid[] taxBranchPKs)
		{
			for (var i = 0; i < taxBranchPKs.Length; i++)
			{
				TestObjectCreator.CreateInvoiceWithLine(invoiceType, $"INV00{i}", TestObjectCreator.AUD, 1M, 1000M, 100M, 1000M, 100M, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader1.PK, "FIN").AH_GB_TaxBranch = taxBranchPKs[i];
			}

			Factory.Save();
		}

		public void SetupSupplyTypeForInvoiceLines(System.Type invoiceType, string[] supplyTypes)
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(invoiceType, "INV001", TestObjectCreator.AUD, 1M, 1000M, 100M, 1000M, 100M, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader1.PK, "FIN");
			invoice.Lines[0].AL_SupplyType = supplyTypes[0];

			for (var i = 1; i < supplyTypes.Length; i++)
			{
				TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 100M).AL_SupplyType = supplyTypes[i];
			}

			Factory.Save();
		}

		DataTable RunScript(string ledger, ZBool summaryOnly, string[] orgList, string[] settlementGroupList, string[] includingCountryList = null, string[] excludedCountryList = null, ZGuid? taxBranchPK = null, string supplyType = null)
		{
			var includingCountryListParam = includingCountryList == null ? "" : new ZStringBuilder(includingCountryList).ToStringWithDelimiterBetweenAppends(",");
			var excludedCountryListParam = excludedCountryList == null ? "" : new ZStringBuilder(excludedCountryList).ToStringWithDelimiterBetweenAppends(",");
			var taxBranchParam = taxBranchPK == null ? "NULL" : ($"'{taxBranchPK}'");
			var supplyTypeParam = supplyType == null ? "NULL" : ($"'{supplyType}'");

			return DataUtils.GetDataTableFromQuery(Db.Connection,
				($@"
EXEC ARAPTransactionLinesSP 
@Period						 = {PeriodCalculator.GetPeriodFromDate(ZDateTime.Now)}, 
@Company					 = '{GlbCompany.CurrentCompany.PK}', 
@OrgList					 = '{new ZStringBuilder(orgList).ToStringWithDelimiterBetweenAppends(",")}', 
@OrgGroupList				 = '',
@AccHeaderBranchList		 = '', 
@AccHeaderDepartmentList	 = '',
@AccLineBranchList			 = '',
@AccLineDepartmentList		 = '', 
@SalesRep					 = NULL, 
@AccountsRelationShip		 = NULL, 
@ConsolidatedCategory		 = NULL, 
@SalesRepRoll				 = '', 
@SummaryOnly				 = '{summaryOnly}', 
@LedgerType					 = '{ledger}', 
@SettlementGroupList		 = '{new ZStringBuilder(settlementGroupList).ToStringWithDelimiterBetweenAppends(",")}', 
@CreditRating				 = NULL, 
@TransactionTypeList		 = 'INV', 
@PostDateFrom				 = '', 
@PostDateTo					 = '', 
@ChargeGroupList			 = '', 
@ChargeCodeList				 = '', 
@IncludeOrgCountryList		 = '{includingCountryListParam}', 
@ExcludeOrgCountryList		 = '{excludedCountryListParam}',
@OrderBy					 = 'T',
@TaxBranch					 = {taxBranchParam},
@SupplyType					 = {supplyTypeParam}
"));
		}
	}
}


