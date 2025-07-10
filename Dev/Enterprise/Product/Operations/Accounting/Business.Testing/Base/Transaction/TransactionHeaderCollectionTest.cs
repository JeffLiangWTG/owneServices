using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(TransactionHeaderCollection))]
	public class TransactionHeaderCollectionTest : AccTransactionHeaderCollectionTest
	{
		public void TestTransactionHeaderCollection_LoadInAnyCompany()
		{
			var transactionInCurrentCompany = Factory.NewWithValidTestData<ARInvoice>();
			var transactionNotInCurrentCompany = Factory.NewWithValidTestData<ARInvoice>();
			transactionNotInCurrentCompany.AH_GC = new TestObjectCreator(Factory).NonCurrentCompany.PK;
			transactionNotInCurrentCompany.AH_GB = new TestObjectCreator(Factory).NonCurrentCompany.Branches[0].PK;
			Factory.Save();

			var collection = new TransactionHeaderCollection(Factory);
			collection.Load();
			AssertEquals(1, collection.Count);

			collection = new TransactionHeaderCollection(Factory, new ZQuery(), true);
			collection.Load();
			AssertEquals(2, collection.Count);
		}

		public virtual void TestFilterCorrect()
		{
			GlbBranch anotherCompanyBranch = Factory.New(typeof(GlbBranch)) as GlbBranch;

			GlbDepartment testDept1 = Factory.New(typeof(GlbDepartment)) as GlbDepartment;
			testDept1.GE_Code = "%%%";
			testDept1.GE_Desc = "$$$";

			GlbDepartment testDept2 = Factory.New(typeof(GlbDepartment)) as GlbDepartment;
			testDept2.GE_Code = "!!!";
			testDept2.GE_Desc = "%%%";

			GlbCompany anotherCompany = Factory.New(typeof(GlbCompany)) as GlbCompany;
			anotherCompany.GC_StartDate = ZDateTime.Now;
			anotherCompany.GC_RX_NKLocalCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			anotherCompany.GC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery()).Code;
			anotherCompany.GC_Code = "!!!";

			anotherCompanyBranch.GB_GC = anotherCompany.PK;

			TestAPInvoice = Factory.New(typeof(APInvoice)) as APInvoice;
			TestAPInvoice.AH_GB = anotherCompanyBranch.PK;
			TestAPInvoice.AH_PostDate = ZDateTime.Now;
			TestAPInvoice.AH_GE = testDept1.PK;
			TestAPInvoice.AH_InvoiceDate = ZDateTime.Now;
			TestAPInvoice.AH_TransactionNum = "Test001";

			GlbBranch currentBranch = Factory.LoadTop1(typeof(GlbBranch), new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK)) as GlbBranch;
			currentBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			TestARInvoice = Factory.New(typeof(ARInvoice)) as ARInvoice;
			TestARInvoice.AH_GB = currentBranch.PK;
			TestARInvoice.AH_PostDate = ZDateTime.Now;
			TestARInvoice.AH_GE = testDept2.PK;
			TestARInvoice.AH_InvoiceDate = ZDateTime.Now;

			ARReceipt testARReceipt = Factory.New(typeof(ARReceipt)) as ARReceipt;
			testARReceipt.AH_GB = currentBranch.PK;
			testARReceipt.AH_PostDate = ZDateTime.Now;
			testARReceipt.AH_GE = testDept2.PK;
			testARReceipt.AH_InvoiceDate = ZDateTime.Now;

			Factory.Save();

			TransactionHeaderCollection testCollection = new TransactionHeaderCollection(Factory);
			testCollection.Load();

			Assert("TestCollection should contain the AR Invoice because it is created by the current company", testCollection.FindByPK(TestARInvoice.PK) != null);
			Assert("TestCollection should contain AR Receipt because it is created by the current company", testCollection.FindByPK(testARReceipt.PK) != null);
			Assert("TestCollection should not contain AP Invoice because it is not created by the current company", testCollection.FindByPK(TestAPInvoice.PK) == null);
		}

		public void TestLoadTransactionsFromMatchLinks()
		{
			ARInvoice aRINV1 = Factory.NewWithValidTestData<ARInvoice>();
			APInvoice aPINV1 = Factory.NewWithValidTestData<APInvoice>();

			TransactionMatchLink matchLink1 = Factory.New<TransactionMatchLink>();
			matchLink1.AP_AH = aRINV1.PK;
			TransactionMatchLink matchLink2 = Factory.New<TransactionMatchLink>();
			matchLink2.AP_AH = aPINV1.PK;

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Add(matchLink1);
			matchLinks.Add(matchLink2);

			TransactionHeaderCollection headers = new TransactionHeaderCollection(Factory);
			headers.LoadTransactionsFromMatchLinks(matchLinks);
			Assert("Headers should contain ARINV1", headers.Contains(aRINV1));
			Assert("Headers should contain APINV1", headers.Contains(aPINV1));
			AssertEquals("Should be 2 elements in headers", 2, headers.Count);
		}

		public void TestContainsAllTransactions()
		{
			APInvoice aPINV = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			ARInvoice aRINV = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			APCreditNote aPCRD = Factory.NewWithValidTestData(typeof(APCreditNote)) as APCreditNote;

			TransactionHeaderCollection subsetTransactions = new TransactionHeaderCollection(Factory);
			subsetTransactions.Add(aPINV);
			subsetTransactions.Add(aRINV);

			TransactionHeaderCollection supersetTransactions = new TransactionHeaderCollection(Factory);
			supersetTransactions.Add(aPINV);
			supersetTransactions.Add(aRINV);
			supersetTransactions.Add(aPCRD);

			Assert("Superset should contain subset", supersetTransactions.ContainsAll(subsetTransactions));
			Assert("Subset should not contain superset", !subsetTransactions.ContainsAll(supersetTransactions));

			subsetTransactions.RemoveAll();
			Assert("Superset should not contain empty subset", !supersetTransactions.ContainsAll(subsetTransactions));
			Assert("Empty subset should not contain superset", !subsetTransactions.ContainsAll(supersetTransactions));
		}

		public void TestRelatedClaimStatusAndQueryNumber()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();

			var claim1 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim1.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus1Open;
			claim1.AY_AH = invoice.PK;
			var claim2 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim2.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus5RejectedClosed;
			claim2.AY_AH = invoice.PK;

			var transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(invoice);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(["OPN", "CLS"], transactions.RelatedClaimStatus(invoice.PK).Replace(" ", "").Split(","));
			AssertContainsExactElementsInAnyOrder([claim1.AY_QueryClaimReference, claim2.AY_QueryClaimReference], transactions.RelatedClaimQueryNumber(invoice.PK).Replace(" ", "").Split(","));
		}

		public void TestRelatedClaimStatusAndQueryNumberMultiInvoice()
		{
			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			var invoice2 = Factory.NewWithValidTestData<APInvoice>();

			var claim1 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim1.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus1Open;
			claim1.AY_AH = invoice1.PK;
			var claim2 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim2.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus5RejectedClosed;
			claim2.AY_AH = invoice1.PK;

			var claim3 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim3.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus1Open;
			claim3.AY_AH = invoice2.PK;
			var claim4 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim4.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus5RejectedClosed;
			claim4.AY_AH = invoice2.PK;

			var transactions1 = new TransactionHeaderCollection(Factory);
			transactions1.Add(invoice1);

			var transactions2 = new TransactionHeaderCollection(Factory);
			transactions2.Add(invoice1);

			Factory.Save();

			// Clear the SQL Query Plan Cache -- DBCC FREEPROCCACHE

			AssertContainsExactElementsInAnyOrder(["OPN", "CLS"], transactions1.RelatedClaimStatus(invoice1.PK).Replace(" ", "").Split(","));
			AssertContainsExactElementsInAnyOrder([claim1.AY_QueryClaimReference, claim2.AY_QueryClaimReference], transactions1.RelatedClaimQueryNumber(invoice1.PK).Replace(" ", "").Split(","));

			AssertContainsExactElementsInAnyOrder(["OPN", "CLS"], transactions2.RelatedClaimStatus(invoice2.PK).Replace(" ", "").Split(","));
			AssertContainsExactElementsInAnyOrder([claim3.AY_QueryClaimReference, claim4.AY_QueryClaimReference], transactions2.RelatedClaimQueryNumber(invoice2.PK).Replace(" ", "").Split(","));

			// Assert that there is a single query plan in the Cache and that it has been called twice
			//   SELECT cplan.usecounts, cplan.objtype, qtext.text, qplan.query_plan
			//   FROM sys.dm_exec_cached_plans AS cplan
			//   CROSS APPLY sys.dm_exec_sql_text(plan_handle) AS qtext
			//   CROSS APPLY sys.dm_exec_query_plan(plan_handle) AS qplan
			//   ORDER BY cplan.usecounts DESC
		}

		public void TestRelatedTransactionDebtorsAsString()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = testObjectCreator.AALSHI.PK;
			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.AL_AC = testObjectCreator.CC1.PK;
			invoiceLine.AL_JH = testObjectCreator.Job1.PK;
			invoiceLine.AL_AT = ZGuid.Empty;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AC = testObjectCreator.CC1.PK;
			charge.JR_JH = testObjectCreator.Job1.PK;
			charge.JR_AL_APLine = invoiceLine.PK;

			ARInvoice relatedInvoice = Factory.NewWithValidTestData<ARInvoice>();
			relatedInvoice.AH_OH = testObjectCreator.AALSHI.PK;
			InvoicingLineBase relatedInvoiceLine = (InvoicingLineBase)relatedInvoice.Lines.AddNew();
			relatedInvoiceLine.AL_AC = testObjectCreator.CC1.PK;
			relatedInvoiceLine.AL_JH = testObjectCreator.Job1.PK;
			relatedInvoiceLine.AL_AT = ZGuid.Empty;
			JobCharge relatedCharge = Factory.NewWithValidTestData<JobCharge>();
			relatedCharge.JR_AC = testObjectCreator.CC1.PK;
			relatedCharge.JR_JH = testObjectCreator.Job1.PK;
			relatedCharge.JR_AL_ARLine = relatedInvoiceLine.PK;

			Factory.Save();

			using (TestConnection.TrackExecutedCommands())
			{
				ZString result = invoice.RelatedTransactionDebtorsAsString;
				AssertEquals("Debtor", "AALSHI:A.A.L. SHIPPING AGENCIES P/L", result);

				ARInvoice relatedInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
				relatedInvoice2.AH_OH = testObjectCreator.ABIGAS.PK;
				InvoicingLineBase relatedInvoice2Line = (InvoicingLineBase)relatedInvoice2.Lines.AddNew();
				relatedInvoice2Line.AL_AC = testObjectCreator.CC1.PK;
				relatedInvoice2Line.AL_JH = testObjectCreator.Job1.PK;
				JobCharge relatedCharge2 = Factory.NewWithValidTestData<JobCharge>();
				relatedCharge2.JR_AC = testObjectCreator.CC1.PK;
				relatedCharge2.JR_JH = testObjectCreator.Job1.PK;
				relatedCharge2.JR_AL_ARLine = relatedInvoice2Line.PK;
				Factory.Save();

				result = invoice.RelatedTransactionDebtorsAsString;
				AssertEquals("Debtor should be updated", "AALSHI:A.A.L. SHIPPING AGENCIES P/L, ABIGAS:ABI GAS & TOOLS", result);

				var expectedQuery = @"IF OBJECT_ID('tempdb..#GroupedPrimaryLines') IS NOT NULL DROP TABLE #GroupedPrimaryLines;

SELECT AH_PK, AH_Ledger, AL_JH, AL_AC, AL_GB, AL_GE
INTO #GroupedPrimaryLines
FROM
	dbo.AccTransactionHeader AS PrimaryHeader
	INNER JOIN dbo.AccTransactionLines AS PrimaryLines ON PrimaryLines.AL_AH = PrimaryHeader.AH_PK
WHERE PrimaryHeader.AH_PK IN (SELECT value from @TransactionHeaderPKs)
GROUP BY AH_PK, AH_Ledger, AL_JH, AL_AC, AL_GB, AL_GE
OPTION (RECOMPILE);

SELECT DISTINCT #GroupedPrimaryLines.AH_PK,
	SecondaryOrgHeader.OH_Code,
	SecondaryOrgHeader.OH_FullName
FROM #GroupedPrimaryLines
INNER JOIN dbo.AccTransactionLines AS SecondaryLines ON
	SecondaryLines.AL_AC = #GroupedPrimaryLines.AL_AC
	AND SecondaryLines.AL_JH = #GroupedPrimaryLines.AL_JH
	AND SecondaryLines.AL_GB = #GroupedPrimaryLines.AL_GB
	AND SecondaryLines.AL_GE = #GroupedPrimaryLines.AL_GE
INNER JOIN dbo.AccTransactionHeader AS SecondaryHeader ON 
	SecondaryHeader.AH_PK = SecondaryLines.AL_AH
	AND SecondaryHeader.AH_Ledger != #GroupedPrimaryLines.AH_Ledger
	AND (SecondaryHeader.AH_Ledger = 'AR' OR SecondaryHeader.AH_Ledger = 'AP') 
	AND (SecondaryHeader.AH_TransactionType = 'INV' OR SecondaryHeader.AH_TransactionType = 'CRD')
INNER JOIN dbo.OrgHeader AS SecondaryOrgHeader ON
	SecondaryOrgHeader.OH_PK = SecondaryHeader.AH_OH
OPTION (RECOMPILE)

DROP TABLE #GroupedPrimaryLines";

				Assert(TestConnection.ExecutedCommands.Any(s => s.StartsWith(expectedQuery)));
			}
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TransactionHeaderCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(APInvoice));
		}

		protected APInvoice TestAPInvoice;
		protected ARInvoice TestARInvoice;
		protected ARReceipt TestARReceipt;

		#endregion
	}
}
