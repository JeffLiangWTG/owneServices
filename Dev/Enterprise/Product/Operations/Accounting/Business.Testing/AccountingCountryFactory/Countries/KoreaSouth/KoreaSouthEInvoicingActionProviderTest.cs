using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class KoreaSouthEInvoicingActionProviderTest : TestCaseWithFactory
	{
		public void TestOnEvaluateEligibilityAndQueue_UAT()
		{
			AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.KoreaSouth;
			GlbCompany.CurrentCompany.Factory.Save();

			var transactionHeader1 = Factory.New<AccTransactionHeader>();
			transactionHeader1.AH_InvoiceDate = new ZDateTime(2022, 03, 28);
			transactionHeader1.AH_GC = GlbCompany.CurrentCompany.PK;

			var transactionHeader2 = Factory.New<AccTransactionHeader>();
			transactionHeader2.AH_InvoiceDate = new ZDateTime(2022, 03, 27);
			transactionHeader2.AH_GC = GlbCompany.CurrentCompany.PK;

			AssertEquals("PreCondition", null, GetInvoiceIssueId(transactionHeader1.PK));
			AssertEquals("PreCondition", null, GetInvoiceIssueId(transactionHeader2.PK));

			AssertGenerateIssueId("Formatted IssueId length must be 24(8+8+8).", transactionHeader1, "202203281234567800000001");
			AssertGenerateIssueId("Should be same value in second run.", transactionHeader1, "202203281234567800000001");

			AssertGenerateIssueId("Formatted IssueId length must be 24(8+8+8).", transactionHeader2, "202203271234567800000002");
			AssertGenerateIssueId("Should be same value in second run.", transactionHeader2, "202203271234567800000002");
		}

		public void TestOnEvaluateEligibilityAndQueue_Prod()
		{
			AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.KoreaSouth;
			GlbCompany.CurrentCompany.Factory.Save();

			var transactionHeader1 = Factory.New<AccTransactionHeader>();
			transactionHeader1.AH_InvoiceDate = new ZDateTime(2022, 03, 28);
			transactionHeader1.AH_GC = GlbCompany.CurrentCompany.PK;

			var transactionHeader2 = Factory.New<AccTransactionHeader>();
			transactionHeader2.AH_InvoiceDate = new ZDateTime(2022, 03, 27);
			transactionHeader2.AH_GC = GlbCompany.CurrentCompany.PK;

			AssertEquals("PreCondition", null, GetInvoiceIssueId(transactionHeader1.PK));
			AssertEquals("PreCondition", null, GetInvoiceIssueId(transactionHeader2.PK));

			AssertGenerateIssueId("Formatted IssueId length must be 24(8+8+8).", transactionHeader1, "202203284100019300000001");
			AssertGenerateIssueId("Should be same value in second run.", transactionHeader1, "202203284100019300000001");

			AssertGenerateIssueId("Formatted IssueId length must be 24(8+8+8).", transactionHeader2, "202203274100019300000002");
			AssertGenerateIssueId("Should be same value in second run.", transactionHeader2, "202203274100019300000002");
		}

		AccTransactionHeaderReference GetInvoiceIssueId(ZGuid transactionHeaderPK)
		{
			var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_AH, transactionHeaderPK)
				.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.KRI);
			return Factory.LoadTop1<AccTransactionHeaderReference>(query);
		}

		void AssertGenerateIssueId(string comment, AccTransactionHeader transactionHeader, string expectedIssuedId)
		{
			var koreaSouthIssueIdGenerator = new KoreaSouthEInvoicingActionProvider();
			koreaSouthIssueIdGenerator.OnEvaluateEligibilityAndQueue(transactionHeader);

			var reference = GetInvoiceIssueId(transactionHeader.PK);
			var issueId = reference?.AH1_Reference ?? ZString.Empty;
			AssertEquals(comment, expectedIssuedId, issueId);
		}
	}
}
