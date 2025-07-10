using System.Collections.Generic;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing.Tests
{
	[TestedType(typeof(AccTransactionHeaderSubscriber))]
	class AccTransactionHeaderSubscriberTest : DataScienceAuditSubscriberTestBase<AccTransactionHeaderSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new AccTransactionHeaderSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			//var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(3, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table AccTransactionHeader required by AccTransactionHeaderSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(14, subscriber.ColumnInfos.Count);

					AssertEquals("AH_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("AH_InvoiceDate", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("AH_OH", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("AH_GC", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("AH_SystemCreateTimeUtc", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("AH_SystemCreateUser", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("AH_SystemCreateBranch", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("AH_SystemCreateDepartment", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("AH_SystemLastEditTimeUtc", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("AH_SystemLastEditUser", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("AH_OA_InvoiceAddressOverride", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("AH_OC_InvoiceContactOverride", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("AH_IsCancelled", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("AH_JobNumber", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("varchar(35)", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames
		{
			get
			{
				yield return "AH_AB";
				yield return "AH_AG";
				yield return "AH_AgePeriod";
				yield return "AH_AgreedPaymentMethodOverride";
				yield return "AH_AH_InvoiceStatement";
				yield return "AH_CAH_CashAdvanceRequestHeader";
				yield return "AH_CashBasisGSTIndicator";
				yield return "AH_CashBasisGSTRealisedToGL";
				yield return "AH_ChequeDrawer";
				yield return "AH_ChequeOrReference";
				yield return "AH_ComplianceDocumentDate";
				yield return "AH_ComplianceSubType";
				yield return "AH_ConsolidatedInvoiceRef";
				yield return "AH_DateClearedInCashbook";
				yield return "AH_Desc";
				yield return "AH_DocumentReceivedDate";
				yield return "AH_DrawerBank";
				yield return "AH_DrawerBranch";
				yield return "AH_DueDate";
				yield return "AH_ExchangeRate";
				yield return "AH_ExportBatchNumber";
				yield return "AH_FullyPaidDate";
				yield return "AH_GB";
				yield return "AH_GB_TaxBranch";
				yield return "AH_GE";
				yield return "AH_GovernmentAllocatedID";
				yield return "AH_GS_NKAuditedBy";
				yield return "AH_GS_NKCashier";
				yield return "AH_GSTAmount";
				yield return "AH_InvoiceAmount";
				yield return "AH_InvoiceApproved";
				yield return "AH_InvoicePaymentReferenceCode";
				yield return "AH_InvoicePrinted";
				yield return "AH_InvoiceTerm";
				yield return "AH_InvoiceTermDays";
				yield return "AH_IsOSOutstandingAmountApplicable";
				yield return "AH_JH";
				yield return "AH_Ledger";
				yield return "AH_LocalTaxAmountOtherTaxes";
				yield return "AH_MatchStatus";
				yield return "AH_MatchStatusReasonCode";
				yield return "AH_NotAllocated";
				yield return "AH_NumberOfSupportingDocuments";
				yield return "AH_OriginalInvoiceDate";
				yield return "AH_OriginalReferenceEndDate";
				yield return "AH_OriginalReferenceStartDate";
				yield return "AH_OriginalTransactionNum";
				yield return "AH_OSOutstandingAmount";
				yield return "AH_OSTaxAmountOtherTaxes";
				yield return "AH_OSTotal";
				yield return "AH_OutstandingAmount";
				yield return "AH_OverrideExchangeRate";
				yield return "AH_PlaceOfSupply";
				yield return "AH_PlaceOfSupplyType";
				yield return "AH_POST1";
				yield return "AH_POST2";
				yield return "AH_POST3";
				yield return "AH_POST4";
				yield return "AH_PostDate";
				yield return "AH_PostedInternal";
				yield return "AH_PostedToEFT";
				yield return "AH_PostPeriod";
				yield return "AH_PostToGL";
				yield return "AH_ReceiptBatchNo";
				yield return "AH_ReceiptType";
				yield return "AH_RequisitionDate";
				yield return "AH_RequisitionStatus";
				yield return "AH_RX_NKTransactionCurrency";
				yield return "AH_TransactionBelongsToGroup";
				yield return "AH_TransactionCategory";
				yield return "AH_TransactionCount";
				yield return "AH_TransactionCreatedByMatching";
				yield return "AH_TransactionNum";
				yield return "AH_TransactionReference";
				yield return "AH_TransactionType";
				yield return "AH_WithholdingTax";
				yield return "AH_XD_ComplianceBook";
			}
		}
	}
}
