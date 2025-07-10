using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing.ScriptTests
{
	class NettingMatchTransactionsTest : TestCaseWithFactory
	{
		#region Header to Header
		[TestDate(2015, 2, 15)]
		public void TestHeaderToHeaderMatch()
		{
			var issuer2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");
			var recipient2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");

			var nettingReceivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 95M, "AUD", "INV001001"); //should be a match
			var nettingPayable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable1.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable1.ApprovalStatus);

			var nettingReceivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001002");
			var nettingPayable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "USD", "INV001002"); //currency does not match
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable2.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable2.ApprovalStatus);

			var nettingReceivable3 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001003");
			var nettingPayable3 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 115M, "AUD", "INV001003"); //threshold does not match
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable3.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable3.ApprovalStatus);

			var nettingReceivable4 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001004");
			var nettingPayable4 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 105M, "AUD", "INV001004"); //should be a match
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable4.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable4.ApprovalStatus);

			var nettingReceivable5 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001005");
			var nettingPayable5 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period2.PK, 105M, "AUD", "INV001005"); //period does not match
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable5.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable5.ApprovalStatus);

			var nettingReceivable6 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001006");
			var nettingPayable6 = CreateNettingTransaction("AP", Issuer1.PK, recipient2.PK, Period1.PK, 105M, "AUD", "INV001006"); //Recipient does not match
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable6.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable6.ApprovalStatus);

			var nettingReceivable7 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001007");
			var nettingPayable7 = CreateNettingTransaction("AP", issuer2.PK, Recipient1.PK, Period1.PK, 105M, "AUD", "INV001007"); //Issuer does not match
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable7.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable7.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("There should be only two record in the matching pivot table", 2, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPT_PayableTransaction = '{0}' and NMP_NRT_ReceivableTransaction = '{1}'"
				, nettingPayable1.PK, nettingReceivable1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Payable1 and Receivable1 is matched", 1, dataTable.Rows.Count);

			var nettingPayable1_InNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable1.PK);
			var nettingReceivable1_InNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, nettingReceivable1_InNewFactory.ApprovalStatus);
			AssertEquals(NettingTransactionApprovalStatus.Matched, nettingPayable1_InNewFactory.ApprovalStatus);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPT_PayableTransaction = '{0}' and NMP_NRT_ReceivableTransaction = '{1}'"
				, nettingPayable4.PK, nettingReceivable4.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Payable4 and Receivable4 is matched", 1, dataTable.Rows.Count);
			var nettingPayable4_InNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable4.PK);
			var nettingReceivable4_InNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable4.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, nettingReceivable4_InNewFactory.ApprovalStatus);
			AssertEquals(NettingTransactionApprovalStatus.Matched, nettingPayable4_InNewFactory.ApprovalStatus);

			//check a random pair to see that their status is not updated
			var nettingPayable3_InNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable3.PK);
			var nettingReceivable3_InNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable3.PK);
			AssertEquals(NettingTransactionApprovalStatus.Approved, nettingReceivable3_InNewFactory.ApprovalStatus);
			AssertEquals(NettingTransactionApprovalStatus.Approved, nettingPayable3_InNewFactory.ApprovalStatus);
		}

		[TestDate(2017, 08, 15)]
		public void Test_HeaderToHeader_DifferentIssuerSameEHubID()
		{
			var issuer2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");

			var nettingReceivable = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			var nettingPayable = CreateNettingTransaction("AP", issuer2.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be no match as the Issuer is different", 0, dataTable.Rows.Count);

			issuer2.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, issuerEHubID);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as both the issuers have same ehub id", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivableInNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivableInNewFactory.ApprovalStatus);

			var payableInNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payableInNewFactory.ApprovalStatus);
		}

		[TestDate(2017, 08, 15)]
		public void Test_HeaderToHeader_DifferentRecipientSameEHubID()
		{
			var recipient2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");

			var nettingReceivable = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			var nettingPayable = CreateNettingTransaction("AP", Issuer1.PK, recipient2.PK, Period1.PK, 100M, "AUD", "INV001001");
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be no match as the Recipient is different", 0, dataTable.Rows.Count);

			recipient2.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, recipientEHubID);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as both the recipients have same ehub id", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivableInNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivableInNewFactory.ApprovalStatus);

			var payableInNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payableInNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderToHeader_OneReceivableHeaderMatchesWithTwoPayableHeaders_TotalAmountDoesNotMatch()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");
			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "INV001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be no match as the payables amount sums to 200 and way over the threshold of 5%", 0, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderToHeader_OneReceivableHeaderMatchesWithTwoPayableHeaders_TotalAmountMatches()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 40M, "AUD", "INV001001");
			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 59M, "AUD", "INV001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as total amount in payables match total amount in receivables", 2, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);

			var payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable2InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderToHeader_OneReceivableHeaderMatchesWithTwoPayableHeaders_BetterMatch_Case1()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");
			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001/A");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be one match with the receivable with payable1 as this is the better match of the two payables", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);

			var payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2.PK);
			AssertEquals(NettingTransactionApprovalStatus.Approved, payable2InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderToHeader_OneReceivableHeaderMatchesWithTwoPayableHeaders_BetterMatch_Case2()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "ERWEW");
			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001/A");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be one match with the receivable with payable2 as references matches with suffix", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Approved, payable1InNewFactory.ApprovalStatus);

			var payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable2InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderToHeader_OneReceivableHeaderMatchesWithThreePayableHeaders_AmountDoesMatchWithOnePayablesWithSuffix()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");
			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");
			var payable3 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001/A");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be one match with the receivable with payable3 as payable1 and payable2 combined exceeds the receivable amount", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Approved, payable1InNewFactory.ApprovalStatus);

			var payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2.PK);
			AssertEquals(NettingTransactionApprovalStatus.Approved, payable2InNewFactory.ApprovalStatus);

			var payable3InNewFactory = newFactory.Load<NettingPayableTransaction>(payable3.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable3InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderToHeader_TwoReceivableHeadersMatchesToOnePayableHeader()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "INV001001");
			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "INV001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as total amount in payables match total amount in receivables", 2, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var receivable2InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable2.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivable2InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderToHeader_TwoReceivableHeadersMatchesToTwoPayableHeaders_SameAmountsOnTwoReceivableInvoices()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "INV001001");
			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "INV001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "INV001001");
			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "INV001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("No match should be found, many to many matching not enabled", 0, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderToHeader_TwoReceivableHeadersMatchesToTwoPayableHeaders_DifferentAmountsOnTwoReceivableInvoices()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "INV001001");
			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 250M, "AUD", "INV001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "INV001001");
			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 250M, "AUD", "INV001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("No match should be found, many to many matching not enabled", 0, dataTable.Rows.Count);
		}

		public void Test_HeaderToHeader_ReceivableHeaderDoesNotMatchToPayableHeader_As_AlreadyPartiallyMatched()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "Line1", 100, "AUD");
			var receivable1Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "Line2", 50, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "Line1", 100, "AUD");
			var payable1Line2 = TestObjectCreator.CreateNettingTransactionLine(payable1, "IntentionallyNotLine2", 50, "AUD");

			Factory.Save();

			InsertIntoNettingMatchPivot(receivable1Line1, payable1Line1, Period1.PK);

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("One record is inserted", 1, dataTable.Rows.Count);

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Number of matching pivot record shold be just one, though the header matches it should not be added", 1, dataTable.Rows.Count);
		}

		#endregion

		#region Header reference to Header reference

		[TestDate(2015, 2, 15)]
		public void TestHeaderReferenceToHeaderReferenceMatch()
		{
			AccountingConfigurationRegistry.Instance.NettingThresholdValue.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5M);

			Factory.Save();

			var nettingReceivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 95M, "AUD", "INV001001"); //should be a match
			TestObjectCreator.AddNettingTransactionReference(nettingReceivable1, "CON", "CON001001");
			TestObjectCreator.AddNettingTransactionReference(nettingReceivable1, "SHN", "SHN001001");
			TestObjectCreator.AddNettingTransactionReference(nettingReceivable1, "VSV", "VSV001001");
			TestObjectCreator.AddNettingTransactionReference(nettingReceivable1, "CCN", "CCN001001");
			TestObjectCreator.AddNettingTransactionReference(nettingReceivable1, "MBL", "MBL001001");

			var nettingPayable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_1");
			TestObjectCreator.AddNettingTransactionReference(nettingPayable1, "MBL", "MBL001001");
			TestObjectCreator.AddNettingTransactionReference(nettingPayable1, "CON", "CON001001");

			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable1.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable1.ApprovalStatus);

			var nettingReceivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 95M, "AUD", "INV001002");
			TestObjectCreator.AddNettingTransactionReference(nettingReceivable2, "CON", "CON001002");

			var nettingPayable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_2");
			TestObjectCreator.AddNettingTransactionReference(nettingPayable2, "CON", "CON001002"); //should be a match

			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable2.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable2.ApprovalStatus);

			var nettingReceivable3 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "INV001003");
			TestObjectCreator.AddNettingTransactionReference(nettingReceivable3, "CON", "CON001003");

			var nettingPayable3 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_3");
			TestObjectCreator.AddNettingTransactionReference(nettingPayable3, "VSV", "CON001003"); //should not find a match, as reference type is different

			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable3.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable3.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("There should be two records in the matching pivot table", 2, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var nettingPayable1_InNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable1.PK);
			var nettingReceivable1_InNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, nettingReceivable1_InNewFactory.ApprovalStatus);
			AssertEquals(NettingTransactionApprovalStatus.Matched, nettingPayable1_InNewFactory.ApprovalStatus);

			var nettingPayable2_InNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable2.PK);
			var nettingReceivable2_InNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable2.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, nettingReceivable2_InNewFactory.ApprovalStatus);
			AssertEquals(NettingTransactionApprovalStatus.Matched, nettingPayable2_InNewFactory.ApprovalStatus);

			var nettingPayable3_InNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable3.PK);
			var nettingReceivable3_InNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable3.PK);
			AssertEquals(NettingTransactionApprovalStatus.Approved, nettingReceivable3_InNewFactory.ApprovalStatus);
			AssertEquals(NettingTransactionApprovalStatus.Approved, nettingPayable3_InNewFactory.ApprovalStatus);
		}

		[TestDate(2017, 08, 15)]
		public void Test_HeaderReferenceToHeaderReference_DifferentIssuerSameEHubID()
		{
			var issuer2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");

			var nettingReceivable = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			TestObjectCreator.AddNettingTransactionReference(nettingReceivable, "CON", "CON001001");

			var nettingPayable = CreateNettingTransaction("AP", issuer2.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "3453425435ser");
			TestObjectCreator.AddNettingTransactionReference(nettingPayable, "CON", "CON001001");

			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be no match as the Issuer is different", 0, dataTable.Rows.Count);

			issuer2.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, issuerEHubID);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as both the issuers have same ehub id", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivableInNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivableInNewFactory.ApprovalStatus);

			var payableInNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payableInNewFactory.ApprovalStatus);
		}

		[TestDate(2017, 08, 15)]
		public void Test_HeaderReferenceToHeaderReference_DifferentRecipientSameEHubID()
		{
			var recipient2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");

			var nettingReceivable = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			TestObjectCreator.AddNettingTransactionReference(nettingReceivable, "CON", "CON001001");

			var nettingPayable = CreateNettingTransaction("AP", Issuer1.PK, recipient2.PK, Period1.PK, 100M, "AUD", "3453425435ser");
			TestObjectCreator.AddNettingTransactionReference(nettingPayable, "CON", "CON001001");

			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be no match as the Issuer is different", 0, dataTable.Rows.Count);

			recipient2.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "234567");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as both the issuers have same ehub id", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivableInNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivableInNewFactory.ApprovalStatus);

			var payableInNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payableInNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderReferenceToHeaderReference_OneReceivableToTwoPayables_TotalAmountDoesNotMatch()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");
			TestObjectCreator.AddNettingTransactionReference(receivable1, "CON", "CON001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_1");
			TestObjectCreator.AddNettingTransactionReference(payable1, "CON", "CON001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "234234_2");
			TestObjectCreator.AddNettingTransactionReference(payable2, "CON", "CON001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("There should be no match", 0, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderReferenceToHeaderReference_OneReceivableToTwoPayables_TotalAmountMatches()
		{
			//case 1: reference type and reference number matches with amount
			var receivable1_case1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			TestObjectCreator.AddNettingTransactionReference(receivable1_case1, "CON", "CON001001");

			var payable1_case1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "234234_1");
			TestObjectCreator.AddNettingTransactionReference(payable1_case1, "CON", "CON001001");

			var payable2_case1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "234234_2");
			TestObjectCreator.AddNettingTransactionReference(payable2_case1, "CON", "CON001001");

			//case 2: reference number and amount matches but reference type does not match
			var receivable1_case2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001002");
			TestObjectCreator.AddNettingTransactionReference(receivable1_case2, "CCN", "CON001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("There should be 2 matches", 2, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPT_PayableTransaction = '{0}' and NMP_NRT_ReceivableTransaction = '{1}'"
				, payable1_case1.PK, receivable1_case1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1_case1 and receivable1_case1 is matched", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPT_PayableTransaction = '{0}' and NMP_NRT_ReceivableTransaction = '{1}'"
				, payable2_case1.PK, receivable1_case1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable2_case1 and receivable1_case1 is matched", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1_case1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1_case1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);

			var payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2_case1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable2InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderReferenceToHeaderReference_OneReceivableToTwoPayables_BetterMatch_Case1()
		{
			//case 1: reference type and reference number matches with amount
			var receivable1_case1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			TestObjectCreator.AddNettingTransactionReference(receivable1_case1, "CON", "CON001001");

			var payable1_case1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_1");
			TestObjectCreator.AddNettingTransactionReference(payable1_case1, "CON", "CON001001");

			var payable2_case1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_2");
			TestObjectCreator.AddNettingTransactionReference(payable2_case1, "CON", "CON001001/A");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the receivable with payable1 as this is the better match of the two payables", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1_case1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1_case1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);

			var payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2_case1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Approved, payable2InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderReferenceToHeaderReference_OneReceivableToTwoPayables_BetterMatch_Case2()
		{
			//case 1: reference type and reference number matches with amount
			var receivable1_case1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			TestObjectCreator.AddNettingTransactionReference(receivable1_case1, "CON", "CON001001");

			var payable1_case1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_1");
			TestObjectCreator.AddNettingTransactionReference(payable1_case1, "CON", "34534");

			var payable2_case1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_2");
			TestObjectCreator.AddNettingTransactionReference(payable2_case1, "CON", "CON001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the receivable with payable2 as references matches with suffix", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1_case1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1_case1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Approved, payable1InNewFactory.ApprovalStatus);

			var payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2_case1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable2InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderReferenceToHeaderReference_OneReceivableToThreePayables()
		{
			//case 1: reference type and reference number matches with amount
			var receivable1_case1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 300M, "AUD", "INV001001"); //should be a match
			TestObjectCreator.AddNettingTransactionReference(receivable1_case1, "CON", "CON001001");

			var payable1_case1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_1");
			TestObjectCreator.AddNettingTransactionReference(payable1_case1, "CON", "CON001001");

			var payable2_case1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_2");
			TestObjectCreator.AddNettingTransactionReference(payable2_case1, "CON", "CON001001");

			var payable3_case1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_2");
			TestObjectCreator.AddNettingTransactionReference(payable3_case1, "CON", "CON001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the receivable with payable2 as references matches with suffix", 3, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1_case1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1_case1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);

			var payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2_case1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable2InNewFactory.ApprovalStatus);

			var payable3InNewFactory = newFactory.Load<NettingPayableTransaction>(payable3_case1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable3InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderReferenceToHeaderReference_TwoReceivablesToOnePayable()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			TestObjectCreator.AddNettingTransactionReference(receivable1, "CON", "CON001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001002"); //should be a match
			TestObjectCreator.AddNettingTransactionReference(receivable2, "CON", "CON001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "234234_1");
			TestObjectCreator.AddNettingTransactionReference(payable1, "CON", "CON001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("There should be 2 matches", 2, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var receivable2InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable2.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivable2InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderReferenceToHeaderReference_TwoReceivablesToTwoPayables_DifferentAmounts()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			TestObjectCreator.AddNettingTransactionReference(receivable1, "CON", "CON001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "INV001002"); //should be a match
			TestObjectCreator.AddNettingTransactionReference(receivable2, "CON", "CON001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_1");
			TestObjectCreator.AddNettingTransactionReference(payable1, "CON", "CON001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "234234_2");
			TestObjectCreator.AddNettingTransactionReference(payable2, "CON", "CON001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("No match should be found, many to many matching not enabled", 0, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void Test_HeaderReferenceToHeaderReference_TwoReceivablesToTwoPayables_SameAmounts()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");
			TestObjectCreator.AddNettingTransactionReference(receivable1, "CON", "CON001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001002");
			TestObjectCreator.AddNettingTransactionReference(receivable2, "CON", "CON001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_1");
			TestObjectCreator.AddNettingTransactionReference(payable1, "CON", "CON001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_2");
			TestObjectCreator.AddNettingTransactionReference(payable2, "CON", "CON001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("No match should be found, many to many matching not enabled", 0, dataTable.Rows.Count);
		}

		public void Test_HeaderReferenceToHeaderReference_ReceivableHeaderRefDoesNotMatchToPayableHeaderRef_As_AlreadyPartiallyMatched()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "R_INV001001");
			TestObjectCreator.AddNettingTransactionReference(receivable1, "CON", "CON001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "Line1", 100, "AUD");
			var receivable1Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "Line2", 50, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "P_INV001001");
			TestObjectCreator.AddNettingTransactionReference(payable1, "CON", "CON001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "Line1", 100, "AUD");
			var payable1Line2 = TestObjectCreator.CreateNettingTransactionLine(payable1, "IntentionallyNotLine2", 50, "AUD");

			Factory.Save();

			InsertIntoNettingMatchPivot(receivable1Line1, payable1Line1, Period1.PK);

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("One record is inserted", 1, dataTable.Rows.Count);

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Number of matching pivot record shold be just one, though the header matches it should not be added", 1, dataTable.Rows.Count);
		}

		public void Test_HeaderReferenceToHeaderReference_ReceivableJOBMatchesPayableINV()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			TestObjectCreator.AddNettingTransactionReference(receivable1, "JOB", "S001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "234234_1");
			TestObjectCreator.AddNettingTransactionReference(payable1, "INV", "S001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be matched even though just AR JOB ref matches AP INV ref", 1, dataTable.Rows.Count);
		}

		#endregion

		#region Line to Line

		[TestDate(2015, 2, 15)]
		public void TestLineToLineMatch()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 95M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 50M, "AUD");
			var receivable1Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001003", 45M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 50M, "AUD");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 195M, "AUD", "R_INV001002");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "S001002", 150M, "AUD");
			var receivable2Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "S001002", 45M, "AUD");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "S001002", 30M, "AUD"); //amount does not fall within threshold
			var payable2Line2 = TestObjectCreator.CreateNettingTransactionLine(payable2, "S001002", 20M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("There should be one record in the matching pivot table", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable1Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertNotEquals("Status is not 'MAT' since second line is not matched yet.", NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'MAT' as the line is matched", NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);
		}

		[TestDate(2017, 08, 15)]
		public void TestLineToLine_DifferentIssuerSameEHubID()
		{
			var issuer2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");

			var nettingReceivable = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			var receivableLine = TestObjectCreator.CreateNettingTransactionLine(nettingReceivable, "S001001", 100M, "AUD");

			var nettingPayable = CreateNettingTransaction("AP", issuer2.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "3453425435ser");
			var payableLine = TestObjectCreator.CreateNettingTransactionLine(nettingPayable, "S001001", 100M, "AUD");

			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be no match as the Issuer is different", 0, dataTable.Rows.Count);

			issuer2.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, issuerEHubID);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as both the issuers have same ehub id", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payableLine.PK, receivableLine.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payableLine and receivableLine is matched", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivableInNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivableInNewFactory.ApprovalStatus);

			var payableInNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payableInNewFactory.ApprovalStatus);
		}

		[TestDate(2017, 08, 15)]
		public void TestLineToLine_DifferentRecipientSameEHubID()
		{
			var recipient2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");

			var nettingReceivable = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");
			var receivableLine = TestObjectCreator.CreateNettingTransactionLine(nettingReceivable, "S001001", 100M, "AUD");

			var nettingPayable = CreateNettingTransaction("AP", Issuer1.PK, recipient2.PK, Period1.PK, 100M, "AUD", "3453425435ser");
			var payableLine = TestObjectCreator.CreateNettingTransactionLine(nettingPayable, "S001001", 100M, "AUD");

			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be no match as the Recipient is different", 0, dataTable.Rows.Count);

			recipient2.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "234567");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as both the issuers have same ehub id", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payableLine.PK, receivableLine.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payableLine and receivableLine is matched", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivableInNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivableInNewFactory.ApprovalStatus);

			var payableInNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payableInNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_LineToLine_OneReceivableLineMatchesToTwoPayableLines_TotalAmountDoesNotMatch()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 50M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 50M, "AUD");
			var payable1Line2 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 100M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be a match as the receivalbe amount matches with one of the payable amounts and their references matches too", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable1Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivableInNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals("Status of Receivable trasnaction is changed to MAT", NettingTransactionApprovalStatus.Matched, receivableInNewFactory.ApprovalStatus);

			var payableInNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status of Payable trasnaction remains as APP since it has one line which is not matched yet", NettingTransactionApprovalStatus.Approved, payableInNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_LineToLine_OneReceivableLineMatchesToTwoPayableLines_TotalAmountMatches()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 100M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 50M, "AUD");
			var payable1Line2 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 50M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("There should be two records in the matching pivot table", 2, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals("Status is 'MAT' as the line is matched.", NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'MAT' as both of the lines are matched", NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_LineToLine_OneReceivableLineMatchesToTwoPayableLines_BetterMatch_Case1()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 100M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 100M, "AUD");
			var payable1Line2 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001/A", 100M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the receivable1line1 with payable1line1 as this is the better match of the two payables", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable1Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void Test_LineToLine_OneReceivableLineMatchesToTwoPayableLines_BetterMatch_Case2()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 100M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "235234", 100M, "AUD");
			var payable1Line2 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001/A", 100M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the receivable1line1 with payable1line2 as this is the better match of the two payables", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable1Line2.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1Line2 and receivable1Line1 is matched", 1, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void Test_LineToLine_OneReceivableLineMatchesToThreePayableLines_OneHasSuffix()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 200M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 300M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 100M, "AUD");
			var payable1Line2 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 100M, "AUD");
			var payable1Line3 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001/A", 100M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should find 2 matches: receivable1Line1 with both payable1Line1 and payable1Line2", 2, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable1Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable1Line2.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1Line2 and receivable1Line1 is matched", 1, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void Test_LineToLine_TwoReceivableLineMatchesToOnePayableLines()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 50M, "AUD");
			var receivable1Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 50M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 100M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("There should be two records in the matching pivot table", 2, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals("Status is 'MAT' as both of the lines are matched.", NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'MAT' as the line is matched.", NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void Test_LineToLine_TwoReceivableLineMatchesToTwoPayableLines_SameAmountForLines()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 50M, "AUD");
			var receivable1Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 50M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 50M, "AUD");
			var payable1Line2 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 50M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("No matching is found, as many to many matching is not enabled", 0, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void Test_LineToLine_TwoReceivableLineMatchesToTwoPayableLines_DifferentAmountForLines()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 100M, "AUD");
			var receivable1Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 50M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 100M, "AUD");
			var payable1Line2 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 50M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("2 Line level matches should be found", 2, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable1Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable1Line2.PK, receivable1Line2.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1Line2 and receivable1Line2 is matched", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals("Status is 'MAT' as both of the lines are matched.", NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'MAT' as the line is matched.", NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);
		}

		public void Test_LineToLine_ReceivableLineDoesNotMatchToPayableLine_As_HeadersAreAlreadyMatched()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "Line1", 100, "AUD");
			var receivable1Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "Line2", 50, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "Line1", 100, "AUD");
			var payable1Line2 = TestObjectCreator.CreateNettingTransactionLine(payable1, "IntentionallyNotLine2", 50, "AUD");

			Factory.Save();

			InsertIntoNettingMatchPivot(receivable1, payable1, Period1.PK);

			receivable1.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			payable1.ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			Factory.Save();

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("One record is inserted", 1, dataTable.Rows.Count);

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Number of matching pivot record shold be just one, though the line matches it should not be added as header is already matched", 1, dataTable.Rows.Count);
		}
		#endregion

		#region Line Reference to Line Reference

		[TestDate(2015, 2, 15)]
		public void TestLineReferenceToLineReferenceMatch()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 95M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001A", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "R_S001001");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "AGR", "R_AGR1001");
			var receivable1Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001B", 45M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "L_S001001C", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "R_S001001");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "AGR", "R_AGR1001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 95M, "AUD", "R_INV001001");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001D", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable2Line1, "CBR", "R_S001002");
			var receivable2Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001E", 45M, "AUD");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001001");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "L_S001001F", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "R_S001002A"); //should not match as reference does not match

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("There should be one record in the matching pivot table", 2, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable1Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable2Line1.PK, receivable2Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable2Line1 and receivable2Line1 is matched, because reference R_S001002 is a part of R_S001002A", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertNotEquals("Status is not 'MAT' since second line is not matched yet.", NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'MAT' as the line is matched", NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);

			var receivable2InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable2.PK);
			AssertNotEquals("Status is not 'MAT' since second line is not matched yet.", NettingTransactionApprovalStatus.Matched, receivable2InNewFactory.ApprovalStatus);

			var payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2.PK);
			AssertEquals("Status is 'MAT' as the line is matched", NettingTransactionApprovalStatus.Matched, payable2InNewFactory.ApprovalStatus);
		}

		[TestDate(2017, 08, 15)]
		public void TestLineReferenceToLineReference_DifferentIssuerSameEHubID()
		{
			var issuer2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");

			var nettingReceivable = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			var receivableLine = TestObjectCreator.CreateNettingTransactionLine(nettingReceivable, "R_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivableLine, "CBR", "CBR123");

			var nettingPayable = CreateNettingTransaction("AP", issuer2.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "3453425435ser");
			var payableLine = TestObjectCreator.CreateNettingTransactionLine(nettingPayable, "P_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payableLine, "CBR", "CBR123");

			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be no match as the Issuer is different", 0, dataTable.Rows.Count);

			issuer2.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, issuerEHubID);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as both the issuers have same ehub id", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payableLine.PK, receivableLine.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payableLine and receivableLine is matched", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivableInNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivableInNewFactory.ApprovalStatus);

			var payableInNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payableInNewFactory.ApprovalStatus);
		}

		[TestDate(2017, 08, 15)]
		public void TestLineReferenceToLineReference_DifferentRecipientSameEHubID()
		{
			var recipient2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");

			var nettingReceivable = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			var receivableLine = TestObjectCreator.CreateNettingTransactionLine(nettingReceivable, "R_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivableLine, "CBR", "CBR123");

			var nettingPayable = CreateNettingTransaction("AP", Issuer1.PK, recipient2.PK, Period1.PK, 100M, "AUD", "3453425435ser");
			var payableLine = TestObjectCreator.CreateNettingTransactionLine(nettingPayable, "P_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payableLine, "CBR", "CBR123");

			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be no match as the Issuer is different", 0, dataTable.Rows.Count);

			Recipient1.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, recipientEHubID);
			recipient2.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, recipientEHubID);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as both the issuers have same ehub id", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payableLine.PK, receivableLine.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payableLine and receivableLine is matched", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivableInNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivableInNewFactory.ApprovalStatus);

			var payableInNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payableInNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineReferenceToLineReferenceMatch_OneReceivableLineRefMatchesToTwoPayableLineRef_AmountDoesNotMatch()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001A", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "CBR001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "L_S001001C", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "CBR001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "L_S001001D", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "CBR001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be no match", 0, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineReferenceToLineReferenceMatch_OneReceivableLineRefMatchesToTowPayableLineRef_BetterMatch_Case1()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001A", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "CBR001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "L_S001001B", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "CBR001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "L_S001001C", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "CBR001001."); //The dot after the reference number makes it different to the other ones

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the receivable with payable1 as this is the better match of the two payables", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable1Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineReferenceToLineReferenceMatch_OneReceivableLineRefMatchesToTowPayableLineRef_BetterMatch_Case2()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001A", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "CBR001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "L_S001001B", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "CBR001007");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "L_S001001C", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "CBR001001."); //The dot after the reference number makes it different to the other ones

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the receivable with payable2 as references matches with suffix", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable2Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable2Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineReferenceToLineReferenceMatch_OneReceivableLineRefMatchesToThreePayableLineRef_AmountDoesMatchWithOnePayablesWithSuffix()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001A", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "CBR001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "L_S001001B", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "CBR001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "L_S001001C", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "CBR001001");

			var payable3 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001003");
			var payable3Line1 = TestObjectCreator.CreateNettingTransactionLine(payable3, "L_S001001D", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable3Line1, "CBR", "CBR001001."); //The dot after the reference number makes it different to the other ones

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the receivable with payable3 as payable1 and payable2 combined exceeds the receivable amount", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable3Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable3Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineReferenceToLineReferenceMatch_OneReceivableLineRefMatchesToTwoPayableLineRef_AmountMatches()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001A", 150M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "CBR001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "L_S001001C", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "CBR001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "L_S001001D", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "CBR001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be 2 entries", 2, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals("Status is 'MAT' as the line is matched.", NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'MAT' as both of the lines are matched", NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineReferenceToLineReferenceMatch_TwoReceivableLinesRefMatchesToOnePayableLineRef()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001A", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "CBR001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001002");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "L_S001001B", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable2Line1, "CBR", "CBR001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "L_S001001C", 150M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "CBR001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be 2 entries", 2, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals("Status is 'MAT' as both of the lines are matched.", NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'MAT' as the line is matched.", NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineReferenceToLineReference_TwoReceivableLineRefMatchesToTwoPayableLineRef_DifferentAmounts()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001A", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "CBR001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001002");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "L_S001001B", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable2Line1, "CBR", "CBR001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "L_S001001C", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "CBR001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "L_S001001D", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "CBR001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("No matching is found, as many to many matching is not enabled", 0, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineReferenceToLineReference_TwoReceivableLineRefMatchesToTwoPayableLineRef_SameAmounts()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001A", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "CBR001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "R_INV001002");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "L_S001001B", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable2Line1, "CBR", "CBR001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "L_S001001C", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "CBR001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "L_S001001D", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "CBR001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("No matching is found, as many to many matching is not enabled", 0, dataTable.Rows.Count);
		}

		public void TestLineReferenceToLineReference_ReceivableLineRefDoesNotMatchToPayableLineRef_As_HeadersAreAlreadyMatched()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "RINV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "RLine1", 100, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "CBR001001");
			var receivable1Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "Line2", 50, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "PINV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "PLine1", 100, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "CBR001001");
			var payable1Line2 = TestObjectCreator.CreateNettingTransactionLine(payable1, "IntentionallyNotLine2", 50, "AUD");

			Factory.Save();

			InsertIntoNettingMatchPivot(receivable1, payable1, Period1.PK);

			receivable1.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			payable1.ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			Factory.Save();

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("One record is inserted", 1, dataTable.Rows.Count);

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Number of matching pivot record shold be just one, though the line matches it should not be added as header is already matched", 1, dataTable.Rows.Count);
		}

		#endregion

		#region Line to Line Reference

		[TestDate(2015, 2, 15)]
		//receivable line to payable ref, also payable line to receivalbe line ref
		public void TestLineToLineReferenceMatch()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 95M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "R_S001001");
			var receivable1Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001001", 45M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "R_S001001", 50M, "AUD");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 95M, "AUD", "R_INV001002");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "R_S001002", 50M, "AUD");
			var receivable2Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "L_S001002", 45M, "AUD");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "L_S001002A", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "R_S001002");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Both sets of transactions should be in the matching pivot table", 2, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable1Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable2Line1.PK, receivable2Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable2Line1 and receivable2Line1 is matched", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertNotEquals("Status is not 'MAT' since second line is not matched yet.", NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'MAT' as the line is matched", NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);

			var receivable2InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable2.PK);
			AssertNotEquals("Status is not 'MAT' since second line is not matched yet.", NettingTransactionApprovalStatus.Matched, receivable2InNewFactory.ApprovalStatus);

			var payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2.PK);
			AssertEquals("Status is 'MAT' as the line is matched", NettingTransactionApprovalStatus.Matched, payable2InNewFactory.ApprovalStatus);
		}

		#region Receivable line to payable line ref

		[TestDate(2017, 08, 15)]
		public void TestPayableLineReferenceToReceivableLine_DifferentIssuerSameEHubID()
		{
			var issuer2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");

			var nettingReceivable = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			var receivableLine = TestObjectCreator.CreateNettingTransactionLine(nettingReceivable, "R_S001001", 100M, "AUD");

			var nettingPayable = CreateNettingTransaction("AP", issuer2.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "3453425435ser");
			var payableLine = TestObjectCreator.CreateNettingTransactionLine(nettingPayable, "P_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payableLine, "INV", "R_S001001");

			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be no match as the Issuer is different", 0, dataTable.Rows.Count);

			issuer2.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, issuerEHubID);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as both the issuers have same ehub id", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payableLine.PK, receivableLine.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payableLine and receivableLine is matched", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivableInNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivableInNewFactory.ApprovalStatus);

			var payableInNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payableInNewFactory.ApprovalStatus);
		}

		[TestDate(2017, 08, 15)]
		public void TestPayableLineReferenceToReceivableLine_DifferentRecipientSameEHubID()
		{
			var recipient2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");

			var nettingReceivable = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001");
			var receivableLine = TestObjectCreator.CreateNettingTransactionLine(nettingReceivable, "R_S001001", 100M, "AUD");

			var nettingPayable = CreateNettingTransaction("AP", Issuer1.PK, recipient2.PK, Period1.PK, 100M, "AUD", "3453425435ser");
			var payableLine = TestObjectCreator.CreateNettingTransactionLine(nettingPayable, "P_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payableLine, "INV", "R_S001001");

			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be no match as the Issuer is different", 0, dataTable.Rows.Count);

			Recipient1.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, recipientEHubID);
			recipient2.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, recipientEHubID);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as both the issuers have same ehub id", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payableLine.PK, receivableLine.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payableLine and receivableLine is matched", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivableInNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivableInNewFactory.ApprovalStatus);

			var payableInNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payableInNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_OneReceivableLineMatchesToTwoPayableLineReferences_AmountsDoesNotMatch()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 200M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "P_S001001", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "S001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 50M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "P_S001001A", 50M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "S001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("No match should be found as amounts does not tally", 0, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_OneReceivableLineMatchesToTwoPayableLineReferences_AmountsMatch()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 200M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "P_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "S001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "P_S001001A", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "S001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Two matches should be found as amounts tally", 2, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals("Status is 'MAT' line is matched.", NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'MAT' as the line is matched", NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);

			var payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2.PK);
			AssertEquals("Status is 'MAT' as the line is matched", NettingTransactionApprovalStatus.Matched, payable2InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_OneReceivableLineMatchesToTwoPayableLineReferences_BetterMatch_Case1()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 100M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "P_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "S001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "P_S001001A", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "S001001/A");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the receivable with payable1 as this is the better match of the two payables", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable1Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_OneReceivableLineMatchesToTwoPayableLineReferences_BetterMatch_Case2()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 100M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "P_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "234234");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "P_S001001A", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "S001001/A");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the receivable with payable2 as references matches with suffix", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable2Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable2Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_OneReceivableLineMatchesToThreePayableLineReferences_AmountDoesMatchWithOnePayablesWithSuffix()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 100M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "P_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "S001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "P_S001001A", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "S001001");

			var payable3 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001002");
			var payable3Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "P_S001001A", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable3Line1, "CBR", "S001001/A");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the receivable with payable3 as payable1 and payable2 combined exceeds the receivable amount", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable3Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable3Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_TwoReceivableLineMatchesToTwoPayableLineReferences_DifferentAmounts()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "R_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "S001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "R_INV001002");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "R_S001001A", 200M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable2Line1, "CBR", "S001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "P_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "S001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "P_S001001A", 200M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "S001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Many to many matching is not enabled", 0, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_TwoReceivableLineMatchesToTwoPayableLineReferences_SameAmounts()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "R_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "S001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001002");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "R_S001001A", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable2Line1, "CBR", "S001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "P_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable1Line1, "CBR", "S001001");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "P_S001001A", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(payable2Line1, "CBR", "S001001");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Many to many matching is not enabled", 0, dataTable.Rows.Count);
		}

		#endregion

		#region Payable line to Receivable line ref

		[TestDate(2017, 08, 15)]
		public void TestPayableLineToReceivableLineReference_DifferentIssuerSameEHubID()
		{
			var issuer2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");

			var nettingReceivable = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			var receivableLine = TestObjectCreator.CreateNettingTransactionLine(nettingReceivable, "R_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivableLine, "INV", "P_S001001");

			var nettingPayable = CreateNettingTransaction("AP", issuer2.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "3453425435ser");
			var payableLine = TestObjectCreator.CreateNettingTransactionLine(nettingPayable, "P_S001001", 100M, "AUD");

			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be no match as the Issuer is different", 0, dataTable.Rows.Count);

			issuer2.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, issuerEHubID);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as both the issuers have same ehub id", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payableLine.PK, receivableLine.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payableLine and receivableLine is matched", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivableInNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivableInNewFactory.ApprovalStatus);

			var payableInNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payableInNewFactory.ApprovalStatus);
		}

		public void TestPayableLineToReceivableLineReference_DifferentRecipientSameEHubID()
		{
			var recipient2 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.LocalClient, "FUL");

			var nettingReceivable = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "INV001001"); //should be a match
			var receivableLine = TestObjectCreator.CreateNettingTransactionLine(nettingReceivable, "R_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivableLine, "INV", "P_S001001");

			var nettingPayable = CreateNettingTransaction("AP", Issuer1.PK, recipient2.PK, Period1.PK, 100M, "AUD", "3453425435ser");
			var payableLine = TestObjectCreator.CreateNettingTransactionLine(nettingPayable, "P_S001001", 100M, "AUD");

			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingReceivable.ApprovalStatus);
			AssertEquals("Prerequisite", NettingTransactionApprovalStatus.Approved, nettingPayable.ApprovalStatus);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be no match as the Issuer is different", 0, dataTable.Rows.Count);

			Recipient1.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, recipientEHubID);
			recipient2.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, recipientEHubID);

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			sql = "Select * from dbo.NettingMatchPivot";
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("Should be a match as both the issuers have same ehub id", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payableLine.PK, receivableLine.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payableLine and receivableLine is matched", 1, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivableInNewFactory = newFactory.Load<NettingReceivableTransaction>(nettingReceivable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, receivableInNewFactory.ApprovalStatus);

			var payableInNewFactory = newFactory.Load<NettingPayableTransaction>(nettingPayable.PK);
			AssertEquals(NettingTransactionApprovalStatus.Matched, payableInNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_OnePayableLineMatchesToTwoReceivableLineReferences_AmountsMatch()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "R_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "S001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001002");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "R_S001002", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable2Line1, "CBR", "S001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 200M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Two matches should be found as amounts tally", 2, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals("Status is 'MAT' line is matched.", NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var receivable2InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable2.PK);
			AssertEquals("Status is 'MAT' line is matched.", NettingTransactionApprovalStatus.Matched, receivable2InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'MAT' as the line is matched", NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_OnePayableLineMatchesToTwoReceivableLineReferences_AmountDoesNotMatch()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "R_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "S001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 300M, "AUD", "R_INV001002");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "R_S001002", 300M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable2Line1, "CBR", "S001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 300M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 300M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("No matches should be found as amounts do not tally", 0, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_OneReceivableLineRefMatchesToTwoPayableLines_BetterMatch_Case1()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "R_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "S001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 100M, "AUD");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "S001001/A", 100M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the receivable1Line1 with payable1Line1 as this is the better match of the two payables", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable1Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable1Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_OneReceivableLineRefMatchesToTwoPayableLines_BetterMatch_Case2()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "R_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "S001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "345345", 100M, "AUD");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "S001001/A", 100M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the payable2Line1 with receivable1Line1 as this is the better match of the two payables", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable2Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable2Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_OneReceivableLineRefMatchesToThreePayableLines_AmountDoesMatchWithOnePayablesWithSuffix()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "R_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "S001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 100M, "AUD");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "S001001", 100M, "AUD");

			var payable3 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001003");
			var payable3Line1 = TestObjectCreator.CreateNettingTransactionLine(payable3, "S001001/A", 100M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Should be one match with the receivable with payable3 as payable1 and payable2 combined exceeds the receivable amount", 1, dataTable.Rows.Count);

			sql = string.Format("SELECT * FROM dbo.NettingMatchPivot WHERE NMP_NPL_PayableLine = '{0}' and NMP_NRL_ReceivableLine = '{1}'"
				, payable3Line1.PK, receivable1Line1.PK);
			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("payable3Line1 and receivable1Line1 is matched", 1, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_TwoPayableLineMatchesToTwoReceivableLineReferences_AmountsDoesNotMatch()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "R_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "S001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 300M, "AUD", "R_INV001002");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "R_S001002", 300M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable2Line1, "CBR", "S001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 300M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 300M, "AUD");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 300M, "AUD", "P_INV001001");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "S001001", 300M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("No matches should be found as amounts do not tally", 0, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_TwoPayableLineMatchesToTwoReceivableLineReferences_AmountsMatch_DifferentInvoiceAmounts()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "R_S001001", 100M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "S001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 300M, "AUD", "R_INV001002");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "R_S001002", 300M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable2Line1, "CBR", "S001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 200M, "AUD");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "P_INV001001");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "S001001", 200M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("No matches should be found as many to many matching is not enabled", 0, dataTable.Rows.Count);
		}

		[TestDate(2015, 2, 15)]
		public void TestLineToLineReference_TwoPayableLineMatchesToTwoReceivableLineReferences_AmountsMatch_SameInvoiceAmounts()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "R_S001001", 200M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable1Line1, "CBR", "S001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "R_INV001002");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "R_S001002", 200M, "AUD");
			TestObjectCreator.AddNettingLineReference(receivable2Line1, "CBR", "S001001");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 200M, "AUD");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "P_INV001001");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "S001001", 200M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);
			RunMatchingScript(Period1.PK); //the script is run twice just to check if we handle duplicate calls to the script

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("No matches should be found as many to many matching is not enabled", 0, dataTable.Rows.Count);
		}

		#endregion

		#endregion

		#region Unmatch transactions

		public void TestUnmatch_HeaderLevelTransaction()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "INV001001");
			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 150M, "AUD", "INV001001");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "INV001002");
			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "INV001002");

			Factory.Save();

			RunMatchingScript(Period1.PK);

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("There should be two records in the matching pivot table", 2, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals("Status is 'MAT' as both of the lines are matched.", NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'MAT' as the line is matched.", NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);

			var receivable2InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable2.PK);
			AssertEquals("Status is 'MAT' as both of the lines are matched.", NettingTransactionApprovalStatus.Matched, receivable2InNewFactory.ApprovalStatus);

			var payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2.PK);
			AssertEquals("Status is 'MAT' as the line is matched.", NettingTransactionApprovalStatus.Matched, payable2InNewFactory.ApprovalStatus);

			RunUnmatchingScript(Period1.PK, receivable1.PK, "AR", NettingTransactionApprovalStatus.Approved);
			RunUnmatchingScript(Period1.PK, receivable1.PK, "AR", NettingTransactionApprovalStatus.Approved); //the script is run twice just to check if we handle duplicate calls to the script

			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Now there should be only one record in the matching pivot table, since one pair is unmatched", 1, dataTable.Rows.Count);

			newFactory = new BusinessObjectFactory();
			receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals("Status is 'APP' as pair is unmatched", NettingTransactionApprovalStatus.Approved, receivable1InNewFactory.ApprovalStatus);

			payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'APP' as pair is unmatched.", NettingTransactionApprovalStatus.Approved, payable1InNewFactory.ApprovalStatus);

			RunUnmatchingScript(Period1.PK, payable2.PK, "AP", NettingTransactionApprovalStatus.Approved);
			RunUnmatchingScript(Period1.PK, payable2.PK, "AP", NettingTransactionApprovalStatus.Approved); //the script is run twice just to check if we handle duplicate calls to the script

			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Now there should be no record in the matching pivot table, since the other pair too is unmatched", 0, dataTable.Rows.Count);

			newFactory = new BusinessObjectFactory();
			receivable2InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable2.PK);
			AssertEquals("Status is 'APP' as pair is unmatched", NettingTransactionApprovalStatus.Approved, receivable2InNewFactory.ApprovalStatus);

			payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2.PK);
			AssertEquals("Status is 'APP' as pair is unmatched.", NettingTransactionApprovalStatus.Approved, payable2InNewFactory.ApprovalStatus);
		}

		public void TestUnmatch_LineLevelTransactions()
		{
			var receivable1 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "R_INV001001");
			var receivable1Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable1, "S001001", 100M, "AUD");

			var payable1 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 100M, "AUD", "P_INV001001");
			var payable1Line1 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 50M, "AUD");
			var payable1Line2 = TestObjectCreator.CreateNettingTransactionLine(payable1, "S001001", 50M, "AUD");

			var receivable2 = CreateNettingTransaction("AR", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "R_INV001002");
			var receivable2Line1 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "S001002", 100M, "AUD");
			var receivable2Line2 = TestObjectCreator.CreateNettingTransactionLine(receivable2, "S001002", 100M, "AUD");

			var payable2 = CreateNettingTransaction("AP", Issuer1.PK, Recipient1.PK, Period1.PK, 200M, "AUD", "P_INV001002");
			var payable2Line1 = TestObjectCreator.CreateNettingTransactionLine(payable2, "S001002", 200M, "AUD");

			Factory.Save();

			RunMatchingScript(Period1.PK);

			var sql = "Select * from dbo.NettingMatchPivot";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("There should be four records from the two matching transactions", 4, dataTable.Rows.Count);

			var newFactory = new BusinessObjectFactory();
			var receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals("Status is 'MAT' as the line is matched.", NettingTransactionApprovalStatus.Matched, receivable1InNewFactory.ApprovalStatus);

			var payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'MAT' as both of the lines are matched", NettingTransactionApprovalStatus.Matched, payable1InNewFactory.ApprovalStatus);

			var receivable2InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable2.PK);
			AssertEquals("Status is 'MAT' as both of the lines are matched.", NettingTransactionApprovalStatus.Matched, receivable2InNewFactory.ApprovalStatus);

			var payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2.PK);
			AssertEquals("Status is 'MAT' as the line is matched.", NettingTransactionApprovalStatus.Matched, payable2InNewFactory.ApprovalStatus);

			RunUnmatchingScript(Period1.PK, payable1.PK, "AP", NettingTransactionApprovalStatus.Approved);
			RunUnmatchingScript(Period1.PK, payable1.PK, "AP", NettingTransactionApprovalStatus.Approved); //the script is run twice just to check if we handle duplicate calls to the script

			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Now there should be only two records, since one pair of transactions are unmatched", 2, dataTable.Rows.Count);

			newFactory = new BusinessObjectFactory();
			receivable1InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable1.PK);
			AssertEquals("Status is 'APP' as pair is unmatched", NettingTransactionApprovalStatus.Approved, receivable1InNewFactory.ApprovalStatus);

			payable1InNewFactory = newFactory.Load<NettingPayableTransaction>(payable1.PK);
			AssertEquals("Status is 'APP' as pair is unmatched.", NettingTransactionApprovalStatus.Approved, payable1InNewFactory.ApprovalStatus);

			RunUnmatchingScript(Period1.PK, receivable2.PK, "AR", NettingTransactionApprovalStatus.Approved);
			RunUnmatchingScript(Period1.PK, receivable2.PK, "AR", NettingTransactionApprovalStatus.Approved); //the script is run twice just to check if we handle duplicate calls to the script

			dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Now there should be no records, since the other pair of transactions are unmatched too", 0, dataTable.Rows.Count);

			newFactory = new BusinessObjectFactory();
			receivable2InNewFactory = newFactory.Load<NettingReceivableTransaction>(receivable2.PK);
			AssertEquals("Status is 'APP' as pair is unmatched", NettingTransactionApprovalStatus.Approved, receivable2InNewFactory.ApprovalStatus);

			payable2InNewFactory = newFactory.Load<NettingPayableTransaction>(payable2.PK);
			AssertEquals("Status is 'APP' as pair is unmatched.", NettingTransactionApprovalStatus.Approved, payable2InNewFactory.ApprovalStatus);
		}

		#endregion

		//ToDo: Remove this method, instead call TestObjectCreator.CreateNettingTransaction
		INettingTransaction CreateNettingTransaction(ZString ledger, ZGuid issuer, ZGuid recipient, ZGuid period, ZDecimal amount, ZString currency, ZString primaryReference)
		{
			INettingTransaction nettingTransaction = null;
			if (ledger == "AR")
			{
				nettingTransaction = Factory.New<NettingReceivableTransaction>();
			}
			else if (ledger == "AP")
			{
				nettingTransaction = Factory.New<NettingPayableTransaction>();
			}

			nettingTransaction.IssuerPK = issuer;
			nettingTransaction.RecipientPK = recipient;
			nettingTransaction.NettingPeriodPK = period;
			nettingTransaction.NettingSystemPK = NettingSystem.PK;
			nettingTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Approved;
			nettingTransaction.TransactionType = TransactionTypes.Invoice;

			nettingTransaction.Amount = amount;
			nettingTransaction.Currency = currency;

			nettingTransaction.Reference = primaryReference;

			nettingTransaction.Date = ZDateTime.Now;
			nettingTransaction.DueDate = ZDateTime.Now;

			return nettingTransaction;
		}

		void InsertIntoNettingMatchPivot(INettingTransactionLine receivableLine, INettingTransactionLine payableLine, ZGuid nettingPeriod)
		{
			var nettingMatchPivot = Factory.New<NettingMatchPivot>();
			nettingMatchPivot.NMP_NRL_ReceivableLine = receivableLine.PK;
			nettingMatchPivot.NMP_NPL_PayableLine = payableLine.PK;
			nettingMatchPivot.NMP_NSP_Period = nettingPeriod;

			Factory.Save();
		}

		void InsertIntoNettingMatchPivot(INettingTransaction receivableHeader, INettingTransaction payableHeader, ZGuid nettingPeriod)
		{
			var nettingMatchPivot = Factory.New<NettingMatchPivot>();
			nettingMatchPivot.NMP_NRT_ReceivableTransaction = receivableHeader.PK;
			nettingMatchPivot.NMP_NPT_PayableTransaction = payableHeader.PK;
			nettingMatchPivot.NMP_NSP_Period = nettingPeriod;

			Factory.Save();
		}

		void RunMatchingScript(ZGuid nettingPeriod)
		{
			NettingHelper.NettingMatchTransactions(Db.Connection, nettingPeriod.ToGuid(), issuerEHubID, recipientEHubID, GlbCompany.CurrentCompany.PK.ToGuid(), "~BP");
		}

		void RunUnmatchingScript(ZGuid nettingPeriod, ZGuid transactionToUnmatch, ZString ledger, ZString newStatus)
		{
			var sql = string.Format(@"
EXEC NettingUnMatchTransaction
@NettingPeriod = '{0}',
@TransactionToUnmatch = '{1}',
@Ledger = '{2}',
@NewStatus = '{3}'
", nettingPeriod, transactionToUnmatch, ledger, newStatus);

			DbCommand cmd = Db.Connection.Command(sql);
			cmd.ExecuteNonQuery();
		}

		NettingSystem NettingSystem;

		NettingSystemPeriod Period1;
		NettingSystemPeriod Period2;

		NettingOrganisation Issuer1;

		NettingOrganisation Recipient1;

		const string issuerEHubID = "123456";
		const string recipientEHubID = "234567";

		protected override void SetUp()
		{
			base.SetUp();
			AccountingConfigurationRegistry.Instance.NettingThresholdValue.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5M);

			NettingSystem = Factory.New<NettingSystem>();
			NettingSystem.NS_Code = "NS1";
			NettingSystem.NS_Description = "Bla Bla";
			NettingSystem.NS_GC = GlbCompany.CurrentCompany.PK;

			Period1 = CreatePeriod(NettingSystem, "201505");

			Period2 = CreatePeriod(NettingSystem, "201506");

			Issuer1 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.AALSHI, "FUL");
			Issuer1.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, issuerEHubID);

			Recipient1 = CreateNettingOrganisation(NettingSystem, TestObjectCreator.Agent, "FUL");
			Recipient1.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, recipientEHubID);

			Factory.Save();
		}

		NettingOrganisation CreateNettingOrganisation(NettingSystem ns, OrgHeader orgHeader, ZString nettingType)
		{
			var org = Factory.New<NettingOrganisation>();
			org.NSO_NS_NettingSystem = ns.PK;
			org.NSO_OH_Organisation = orgHeader.PK;
			org.NSO_NettingType = nettingType;

			return org;
		}

		NettingSystemPeriod CreatePeriod(NettingSystem nettingSystem, ZString nsp_period)
		{
			var period = Factory.New<NettingSystemPeriod>();
			period.NSP_Period = nsp_period;
			period.NSP_EarliestInvoiceDateUtc = ZDateTime.Now.AddDays(-7);
			period.NSP_LatestInvoiceDateUtc = ZDateTime.Now.AddDays(15);
			period.NSP_NettingExecutionDateUtc = ZDateTime.Now.AddDays(17);

			period.NSP_LatestUploadDateUtc = ZDateTime.Now.AddDays(5);
			period.NSP_LatestFXOfferDateUtc = ZDateTime.Now.AddDays(8);
			period.NSP_LatestApprovalDateUtc = ZDateTime.Now.AddDays(10);
			period.NSP_ValueDate = ZDate.Today.AddDays(12);
			period.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(12);

			period.NSP_NS_NettingSystem = nettingSystem.PK;

			return period;
		}

		NettingObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new NettingObjectCreator(Factory));
		NettingObjectCreator testObjectCreator;
	}
}
