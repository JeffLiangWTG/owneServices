using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ARLStatementOfAccountDocumentWrapperTest : TestCaseWithFactory
	{
		#region TestSourceIdentifierProvider

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestISourceIdentifierProvider()
		{
			var message = CreateStatementOfAccountMessageAndSetUpTestData(Factory);
			Factory.Save();
			var wrapper = new ARLStatementOfAccountDocumentWrapper(message);

			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("ARLStatementOfAccountDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", message.PK, supporter?.SourceIdentifier);
		}

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataWhenMultipleOrgMatch()
		{
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "IMPORTER3";
			org3.OH_FullName = "IMPORTER3 DESC";
			org3.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0001", Core.Constants.CountryCodes.Canada);

			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "IMPORTER4";
			org4.OH_FullName = "IMPORTER4 DESC";
			org4.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0001", Core.Constants.CountryCodes.Canada);

			var message = CreateStatementOfAccountMessageAndSetUpTestData(Factory);
			var org1 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER1");
			var org2 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER2");

			var wrapper = new ARLStatementOfAccountDocumentWrapper(message);
			CombineAssertions(() =>
			{
				AssertEquals("ImporterBusinessNumber", "123546789", wrapper.ImporterBusinessNumber);
				AssertEquals("ImporterLegalName", "PIERRE IMPORTERS Inc.", wrapper.ImporterLegalName);

				var dailySummaryTotals = wrapper.DailySummaryTotals.ToArray();
				AssertEquals("DailySummaryTotals.Length", 2, dailySummaryTotals.Length);

				var dailySummary = dailySummaryTotals.FirstOrDefault(c => c.ImporterBusinessNumber == "123546789RM0001");

				AssertNotNull("1 ImporterBusinessNumber", dailySummary);
				AssertEquals("1 ImporterCode", "MULTIPLE", dailySummary.ImporterCode);
				AssertEquals("1 ImporterLegalName", "PIERRE IMPORTERS Inc.", dailySummary.ImporterLegalName);
				AssertArrayEqualsByElements("1 Importers", new[] { org1, org3, org4 }, dailySummary.Importers);

				dailySummary = dailySummaryTotals.FirstOrDefault(c => c.ImporterBusinessNumber == "123546789RM0002");

				AssertNotNull("2 ImporterBusinessNumber", dailySummary);
				AssertEquals("2 ImporterCode", "IMPORTER2", dailySummary.ImporterCode);
				AssertEquals("2 ImporterLegalName", "PIERRE IMPORTERS Inc. 2\r\n(IMPORTER2 NAME)", dailySummary.ImporterLegalName);
				AssertArrayEqualsByElements("2 Importers", new[] { org2 }, dailySummary.Importers);
			});
		}

		[ExpectExceptionMessage(typeof(ArgumentNullException), "Batch type is null in DN message.\r\nParameter name: transactionBatch")]
		public void TestLastMessageIsSOAButBatchTypeIsNull()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "8804P04001";
			statement.B2_IsMonthlyStatement = true;

			var message = statement.Messages.AddNew();
			statement.B2_IsMonthlyStatement = false;
			message.EM_MessageText = EDIMessageTextWithoutBatchType;
			message.EM_MessageSubType = ARLMessageTypes.Codes.StatementOfAccount;

			statement.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".CusStatementHeader"), null)[0].ParentBusinessObject.GetType();
		}

		const string EDIMessageTextWithoutBatchType = @"<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CAAccountsReceivableLedger</Type>
					<Key></Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<TransactionCollection>
			<Transaction>
				<Category>SUM</Category>
				<CreateTime>2014-11-18T00:00:00</CreateTime>
				<PostDate>2014-11-17T00:00:00</PostDate>
				<PostingJournalCollection>
					<PostingJournal>
						<Description>TotalPaymentReceived</Description>
						<LocalAmount>0</LocalAmount>
					</PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProperties()
		{
			var message = CreateStatementOfAccountMessageAndSetUpTestData(Factory);

			var wrapper = new ARLStatementOfAccountDocumentWrapper(message);
			CombineAssertions(() =>
			{
				AssertEquals("StatementDate", new ZDate(2014, 1, 25), wrapper.StatementDate);
				AssertEquals("DueDate", new ZDate(2014, 1, 31), wrapper.DueDate);
				AssertEquals("BN", "123546789", wrapper.ImporterBusinessNumber);
				AssertEquals("Billing Period From", new ZDate(2014, 10, 25), wrapper.BillingDateFrom);
				AssertEquals("Billing Period To", new ZDate(2014, 11, 24), wrapper.BillingDateTo);
				AssertEquals("ImporterLegalName", "PIERRE IMPORTERS Inc.", wrapper.ImporterLegalName);
				AssertEquals("AccountSecurityCode", "40077", wrapper.AccountSecurityCode);
				AssertEquals("NumberOfSupportingDocuments", 1, wrapper.NumberOfSupportingDocuments);
				AssertEquals("TotalCustomsDuties", 146295.79m, wrapper.TotalCustomsDuties);
				AssertEquals("TotalSIMA", -1800m, wrapper.TotalSIMA);
				AssertEquals("TotalExciseTax", 0m, wrapper.TotalExciseTax);
				AssertEquals("TotalGST", 41900.74m, wrapper.TotalGST);
				AssertEquals("TotalOthers", 300m, wrapper.TotalOthers);
				AssertEquals("TotalTotal", 186696.53m, wrapper.TotalTotal);
				AssertEquals("GrandTotal", 3134.59m, wrapper.GrandTotal);
				AssertEquals("StatementType", "B", wrapper.StatementType);
				AssertEquals("StatementTypeDescription", "Broker", wrapper.StatementTypeDescription);
				AssertEquals("RMAccountNumber", "0001", wrapper.RMAccountNumber);
			});

			AssertEquals("DailySummaryTotals.Count", 2, wrapper.DailySummaryTotals.Count());
			var dailySummary = wrapper.DailySummaryTotals.First();
			AssertEquals("ImporterCode", "IMPORTER1", dailySummary.ImporterCode);
			AssertEquals("ImporterBusinessNumber", "123546789RM0001", dailySummary.ImporterBusinessNumber);
			AssertEquals("ImporterLegalName", "PIERRE IMPORTERS Inc.\r\n(IMPORTER1 NAME)", dailySummary.ImporterLegalName);
			AssertEquals("CustomsDuties", 56000m, dailySummary.CustomsDuties);
			AssertEquals("SIMA", 0m, dailySummary.SIMA);
			AssertEquals("ExciseTax", 0m, dailySummary.ExciseTax);
			AssertEquals("GST", 41900.74m, dailySummary.GST);
			AssertEquals("Others", 200m, dailySummary.Others);
			AssertEquals("Total", 98100.74m, dailySummary.Total);
			AssertEquals("PreviousMonthlyStatementTotal", 240870.35m, dailySummary.PreviousMonthlyStatementTotal);
			AssertEquals("PaymentReceivedSinceLastMonthlyStatement", 240620.35m, dailySummary.PaymentReceivedSinceLastMonthlyStatement);
			AssertEquals("Refund", 0m, dailySummary.Refund);
			AssertEquals("UnpaidBalanceForward", 250.00m, dailySummary.UnpaidBalanceForward);
			AssertEquals("ArrearsInterest", 21.12m, dailySummary.ArrearsInterest);
			AssertEquals("OtherCharges", 26000m, dailySummary.OtherCharges);
			AssertEquals("TotalPayableForImporter", 2283144.44m, dailySummary.TotalPayableForImporter);
			AssertEquals("TotalPayableForBroker", 55m, dailySummary.TotalPayableForBroker);
			AssertEquals("InstalmentLastAmount", 0m, dailySummary.InstalmentLastAmount);
			AssertEquals("InstalmentCurrentAmount", 2283144.44m, dailySummary.InstalmentCurrentAmount);
			var transactions = dailySummary.Transactions.ToList();
			AssertEquals("Transactions.Count", 2, transactions.Count);

			var transaction = transactions[0];
			AssertEquals("BusinessDay", new ZDate(2014, 11, 19), transaction.BusinessDay);
			AssertEquals("CustomsDuties", 50000m, transaction.CustomsDuties);
			AssertEquals("CustomsDuties", 0m, transaction.SIMA);
			AssertEquals("CustomsDuties", 0m, transaction.ExciseTax);
			AssertEquals("CustomsDuties", 7505.67m, transaction.GST);
			AssertEquals("CustomsDuties", 200m, transaction.Others);
			AssertEquals("CustomsDuties", 57705.67m, transaction.Total);

			var detailsWrapper = new ARLStatementOfAccountDocumentWrapper.TransactionsDetailsWrapper(transaction, dailySummary, "Transactions");
			AssertEquals("BusinessDay", transaction.BusinessDay, detailsWrapper.BusinessDay);
			AssertEquals("CustomsDuties", transaction.CustomsDuties, detailsWrapper.CustomsDuties);
			AssertEquals("SIMA", transaction.SIMA, detailsWrapper.SIMA);
			AssertEquals("ExciseTax", transaction.ExciseTax, detailsWrapper.ExciseTax);
			AssertEquals("GST", transaction.GST, detailsWrapper.GST);
			AssertEquals("Others", transaction.Others, detailsWrapper.Others);
			AssertEquals("AmountBilled", transaction.Total, detailsWrapper.Total);
			AssertEquals("ImporterCode", dailySummary.ImporterCode, detailsWrapper.ImporterCode);
			AssertEquals("ImporterLegalName", dailySummary.ImporterLegalName, detailsWrapper.ImporterLegalName);
			AssertEquals("ImporterBusinessNumber", dailySummary.ImporterBusinessNumber, detailsWrapper.ImporterBusinessNumber);
			AssertEquals("TransactionDetailsType", "Transactions", detailsWrapper.TransactionDetailsType);

			transaction = transactions[1];
			AssertEquals("BusinessDay", new ZDate(2014, 11, 19), transaction.BusinessDay);
			AssertEquals("CustomsDuties", 600m, transaction.CustomsDuties);
			AssertEquals("SIMA", 0m, transaction.SIMA);
			AssertEquals("ExciseTax", 0m, transaction.ExciseTax);
			AssertEquals("GST", 34395.07m, transaction.GST);
			AssertEquals("Others", 0m, transaction.Others);
			AssertEquals("Total", 40395.07m, transaction.Total);

			var otherTransactions = dailySummary.OtherTransactions.ToList();
			AssertEquals("OtherTransactions.Count", 1, otherTransactions.Count);

			var otherTransaction = otherTransactions[0];
			AssertEquals("DocumentNumber", "84100", otherTransaction.DocumentNumber);
			AssertEquals("DocumentType", "NP", otherTransaction.DocumentType);
			AssertEquals("Total", 100m, otherTransaction.Total);
			AssertEquals("StatusDescription", "Credit document has been offset against CBSA invoices", otherTransaction.StatusDescription);
			AssertEquals("PaymentDueDate", new ZDate(2014, 11, 20), otherTransaction.PaymentDueDate);
			AssertEquals("DocumentIssueDate", new ZDate(2014, 11, 19), otherTransaction.DocumentIssueDate);
			AssertEquals("B3RelatedDocumentNumber", "", otherTransaction.B3RelatedDocumentNumber);
			AssertEquals("AccountingDate", ZDate.Empty, otherTransaction.AccountingDate);

			detailsWrapper = new ARLStatementOfAccountDocumentWrapper.TransactionsDetailsWrapper(otherTransaction, dailySummary, "Other Transactions");
			AssertEquals("DocumentNumber", otherTransaction.DocumentNumber, detailsWrapper.DocumentNumber);
			AssertEquals("DocumentTypeDescription", otherTransaction.DocumentTypeDescription, detailsWrapper.DocumentTypeDescription);
			AssertEquals("Total", otherTransaction.Total, detailsWrapper.Total);
			AssertEquals("StatusDescription", otherTransaction.StatusDescription, detailsWrapper.StatusDescription);
			AssertEquals("PaymentDueDate", otherTransaction.PaymentDueDate, detailsWrapper.PaymentDueDate);
			AssertEquals("DocumentIssueDate", otherTransaction.DocumentIssueDate, detailsWrapper.DocumentIssueDate);
			AssertEquals("B3RelatedDocumentNumber", otherTransaction.B3RelatedDocumentNumber, detailsWrapper.B3RelatedDocumentNumber);
			AssertEquals("JobNumber", otherTransaction.JobNumber, detailsWrapper.JobNumber);
			AssertEquals("TransactionDetailsType", "Other Transactions", detailsWrapper.TransactionDetailsType);

			otherTransactions = dailySummary.UnderReviewTransactions.ToList();
			AssertEquals("OtherTransactions.Count", 1, otherTransactions.Count);

			otherTransaction = otherTransactions[0];
			AssertEquals("DocumentNumber", "40077000125300", otherTransaction.DocumentNumber);
			AssertEquals("DocumentType", "B2", otherTransaction.DocumentType);
			AssertEquals("Total", 58711.50m, otherTransaction.Total);
			AssertEquals("StatusDescription", "Secured", otherTransaction.StatusDescription);
			AssertEquals("PaymentDueDate", new ZDate(2014, 11, 20), otherTransaction.PaymentDueDate);
			AssertEquals("DocumentIssueDate", new ZDate(2014, 11, 19), otherTransaction.DocumentIssueDate);
			AssertEquals("B3RelatedDocumentNumber", "40077000000020", otherTransaction.B3RelatedDocumentNumber);
			AssertEquals("AccountingDate", ZDate.Empty, otherTransaction.AccountingDate);

			detailsWrapper = new ARLStatementOfAccountDocumentWrapper.TransactionsDetailsWrapper(otherTransaction, dailySummary, "Under Review");
			AssertEquals("TotalCustomsDuties", dailySummary.CustomsDuties, detailsWrapper.TotalCustomsDuties);
			AssertEquals("TotalSIMA", dailySummary.SIMA, detailsWrapper.TotalSIMA);
			AssertEquals("TotalExciseTax", dailySummary.ExciseTax, detailsWrapper.TotalExciseTax);
			AssertEquals("TotalGST", dailySummary.GST, detailsWrapper.TotalGST);
			AssertEquals("TotalOthers", dailySummary.Others, detailsWrapper.TotalOthers);
			AssertEquals("TotalTransactionsTotal", dailySummary.Total, detailsWrapper.TotalTransactionsTotal);
			AssertEquals("TransactionDetailsType", "Under Review", detailsWrapper.TransactionDetailsType);

			dailySummary = wrapper.DailySummaryTotals.ElementAt(1);
			AssertEquals("ImporterCode", "IMPORTER2", dailySummary.ImporterCode);
			AssertEquals("ImporterBusinessNumber", "123546789RM0002", dailySummary.ImporterBusinessNumber);
			AssertEquals("ImporterLegalName", "PIERRE IMPORTERS Inc. 2\r\n(IMPORTER2 NAME)", dailySummary.ImporterLegalName);
			AssertEquals("CustomsDuties", 90295.79m, dailySummary.CustomsDuties);
			AssertEquals("SIMA", -1800m, dailySummary.SIMA);
			AssertEquals("ExciseTax", 0m, dailySummary.ExciseTax);
			AssertEquals("GST", 0m, dailySummary.GST);
			AssertEquals("Others", 100m, dailySummary.Others);
			AssertEquals("Total", 88595.79m, dailySummary.Total);
			AssertEquals("PreviousMonthlyStatementTotal", 0m, dailySummary.PreviousMonthlyStatementTotal);
			AssertEquals("PaymentReceivedSinceLastMonthlyStatement", 50000m, dailySummary.PaymentReceivedSinceLastMonthlyStatement);
			AssertEquals("Refund", 0m, dailySummary.Refund);
			AssertEquals("UnpaidBalanceForward", -50000m, dailySummary.UnpaidBalanceForward);
			AssertEquals("ArrearsInterest", 0m, dailySummary.ArrearsInterest);
			AssertEquals("OtherCharges", 100m, dailySummary.OtherCharges);
			AssertEquals("TotalPayableForImporter", 264500.08m, dailySummary.TotalPayableForImporter);
			AssertEquals("TotalPayableForBroker", 4845.34m, dailySummary.TotalPayableForBroker);

			transactions = dailySummary.Transactions.ToList();
			AssertEquals("Transactions.Count", 1, transactions.Count);

			transaction = transactions[0];
			AssertEquals("BusinessDay", new ZDate(2014, 11, 19), transaction.BusinessDay);
			AssertEquals("CustomsDuties", 90295.79m, transaction.CustomsDuties);
			AssertEquals("SIMA", -1800.00m, transaction.SIMA);
			AssertEquals("ExciseTax", 0m, transaction.ExciseTax);
			AssertEquals("GST", 0m, transaction.GST);
			AssertEquals("Others", 100m, transaction.Others);
			AssertEquals("Total", 88595.79m, transaction.Total);

			otherTransactions = dailySummary.OtherTransactions.ToList();
			AssertEquals("OtherTransactions.Count", 1, otherTransactions.Count);

			otherTransaction = otherTransactions[0];
			AssertEquals("DocumentNumber", "84100", otherTransaction.DocumentNumber);
			AssertEquals("DocumentType", "AI", otherTransaction.DocumentType);
			AssertEquals("Total", 100m, otherTransaction.Total);
			AssertEquals("StatusDescription", "Disbursed. The refund has been approved", otherTransaction.StatusDescription);
			AssertEquals("PaymentDueDate", new ZDate(2014, 11, 20), otherTransaction.PaymentDueDate);
			AssertEquals("DocumentIssueDate", new ZDate(2014, 11, 19), otherTransaction.DocumentIssueDate);
			AssertEquals("B3RelatedDocumentNumber", "", otherTransaction.B3RelatedDocumentNumber);
			AssertEquals("AccountingDate", ZDate.Empty, otherTransaction.AccountingDate);

			otherTransactions = dailySummary.UnderReviewTransactions.ToList();
			AssertEquals("OtherTransactions.Count", 1, otherTransactions.Count);

			otherTransaction = otherTransactions[0];
			AssertEquals("DocumentNumber", "40078000125301", otherTransaction.DocumentNumber);
			AssertEquals("DocumentType", "B2", otherTransaction.DocumentType);
			AssertEquals("Total", 58711.50m, otherTransaction.Total);
			AssertEquals("StatusDescription", "Secured", otherTransaction.StatusDescription);
			AssertEquals("PaymentDueDate", new ZDate(2014, 11, 20), otherTransaction.PaymentDueDate);
			AssertEquals("DocumentIssueDate", new ZDate(2014, 11, 19), otherTransaction.DocumentIssueDate);
			AssertEquals("B3RelatedDocumentNumber", "40078000000021", otherTransaction.B3RelatedDocumentNumber);
			AssertEquals("AccountingDate", ZDate.Empty, otherTransaction.AccountingDate);

			var collection = wrapper.AllTransactionsForDocument;
			CombineAssertions(() =>
			{
				AssertEquals("collection.Count", 7, collection.Count);
				Assert("IsFirstItemOfCurrentTransactions", collection[0].IsFirstItemOfCurrentTransactions);
				Assert("IsLastItemOfCurrentTransactions", collection[1].IsLastItemOfCurrentTransactions);
				Assert("IsFirstItemOfCurrentTransactions", collection[2].IsFirstItemOfCurrentTransactions);
				Assert("IsLastItemOfCurrentTransactions", collection[2].IsLastItemOfCurrentTransactions);
				AssertEquals("TotalOtherTransactionsTotal", 100m, collection[2].TotalOtherTransactionsTotal);
				Assert("IsFirstItemOfCurrentTransactions", collection[3].IsFirstItemOfCurrentTransactions);
				Assert("IsLastItemOfCurrentTransactions", collection[3].IsLastItemOfCurrentTransactions);
				AssertEquals("TotalOtherTransactionsTotal", 58711.5m, collection[3].TotalOtherTransactionsTotal);
				Assert("IsFirstItemOfCurrentTransactions", collection[4].IsFirstItemOfCurrentTransactions);
				Assert("IsLastItemOfCurrentTransactions", collection[4].IsLastItemOfCurrentTransactions);
				Assert("IsFirstItemOfCurrentTransactions", collection[5].IsFirstItemOfCurrentTransactions);
				Assert("IsLastItemOfCurrentTransactions", collection[5].IsLastItemOfCurrentTransactions);
				AssertEquals("TotalOtherTransactionsTotal", 100m, collection[5].TotalOtherTransactionsTotal);
				Assert("IsFirstItemOfCurrentTransactions", collection[6].IsFirstItemOfCurrentTransactions);
				Assert("IsLastItemOfCurrentTransactions", collection[6].IsLastItemOfCurrentTransactions);
				AssertEquals("TotalOtherTransactionsTotal", 58711.5m, collection[6].TotalOtherTransactionsTotal);
			});

			message.EM_MessageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\SoAUniversalTransactionBatch2.xml");
			wrapper = new ARLStatementOfAccountDocumentWrapper(message);
			collection = wrapper.AllTransactionsForDocument;
			CombineAssertions(() =>
			{
				AssertEquals("collection.Count", 5, collection.Count);
				AssertEquals("TransactionDetailsType", ZString.Empty, collection[4].TransactionDetailsType);
				Assert("IsFirstItemOfCurrentTransactions", !collection[4].IsFirstItemOfCurrentTransactions);
				Assert("IsLastItemOfCurrentTransactions", !collection[4].IsLastItemOfCurrentTransactions);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAccountSecurityCode()
		{
			var message = CreateStatementOfAccountMessageAndSetUpTestData(Factory, "12345");
			var wrapper = new ARLStatementOfAccountDocumentWrapper(message);
			AssertEquals("AccountSecurityCode", "12345", wrapper.AccountSecurityCode);
		}

		internal static Enterprise.Messaging.Business.EDIMessage CreateStatementOfAccountMessageAndSetUpTestData(BusinessObjectFactory factory, string accountSecurityCode = null, string messageText = null)
		{
			var importer = factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTER1";
			importer.OH_FullName = "IMPORTER1 NAME";
			var importerAddInfo = OrgImpAddInfo.Get(importer);
			importerAddInfo.ZO_IsImporterDirectPayment = true;
			importerAddInfo.ZO_AccountSecurityNumber = "40077";
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0001", "CA");
			ARLDailyReportDocumentWrapperTest.CreatedDeclaration("46e145fb-4084-4ccd-b96d-eb7068ebe6d8", factory, importer, "B00002111", "40077000125300");
			ARLDailyReportDocumentWrapperTest.CreatedDeclaration("1e03e919-9e47-464b-b90b-b041d152286f", factory, importer, "B00003111", "40077000000020");

			importer = factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTER2";
			importer.OH_FullName = "IMPORTER2 NAME";
			importerAddInfo = OrgImpAddInfo.Get(importer);
			importerAddInfo.ZO_IsImporterDirectPayment = true;
			importerAddInfo.ZO_AccountSecurityNumber = "40078";
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0002", "CA");
			ARLDailyReportDocumentWrapperTest.CreatedDeclaration("8f9cfa32-40e4-4d8b-9ee0-ef19d1a15821", factory, importer, "B00002112", "40078000125301");
			ARLDailyReportDocumentWrapperTest.CreatedDeclaration("a3b17f88-edab-4be8-9754-aba310b4c749", factory, importer, "B00003112", "40078000000021");

			if (messageText == null)
			{
				messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\SoAUniversalTransactionBatch.xml");
			}
			if (accountSecurityCode != null)
			{
				messageText = messageText.Replace("<Value>00000</Value>", string.Format("<Value>{0}</Value>", accountSecurityCode));
			}
			var message = ARLDailyReportDocumentWrapperTest.CreateTransactionBatchMessage("6f51922a-484a-4b21-93f9-0286d3584852", factory, messageText);
			factory.Save();

			return message;
		}
	}
}
