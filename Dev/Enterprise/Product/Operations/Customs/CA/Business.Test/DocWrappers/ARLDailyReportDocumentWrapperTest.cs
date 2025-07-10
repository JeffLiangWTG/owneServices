using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ARLDailyReportDocumentWrapperTest : TestCaseWithFactory
	{
		#region TestSourceIdentifierProvider
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestISourceIdentifierProvider()
		{
			var messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNUniversalTransactionBatch.xml");
			var message = CreateTransactionBatchMessage("8EBA241A-2047-4D00-9F8C-D74904C28842", Factory, messageText);
			Factory.Save();

			var wrapper = new ARLDailyNoticeDocumentWrapper(message);

			var supporter = wrapper as DocumentEngineIntegration.ISourceIdentifierProvider;
			AssertNotNull("ARLDailyNoticeDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", message.PK, supporter?.SourceIdentifier);
		}

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMatchExactTransactionNumber()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTERX";
			importer.OH_FullName = "IMPORTERX NAME";
			var importerAddInfo = OrgImpAddInfo.Get(importer);
			importerAddInfo.ZO_IsGSTDirectPayment = true;
			importerAddInfo.ZO_AccountSecurityNumber = "11222";
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0002", "CA");
			var dec1 = CreatedDeclaration("A2883FFB-3E1D-4994-9B1D-BB56D8A34191", Factory, importer, "B0000111X", "11222783837955", ZDateTime.UtcNow);
			Factory.Save();

			var messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNUniversalTransactionBatch.xml");
			messageText = messageText.Replace("11222783837955", "783837955");
			var message = CreateTransactionBatchMessage("8EBA241A-2047-4D00-9F8C-D74904C28842", Factory, messageText);

			var wrapper = new ARLDailyNoticeDocumentWrapper(message);
			var accounting = wrapper.PreviousDaysAccountings.First();
			var transactions = accounting.Transactions.ToList();
			var transaction = transactions[0];

			AssertNull("No declaration matched transaction number = '783837955'", transaction.Declaration);

			messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNUniversalTransactionBatch.xml");
			message = CreateTransactionBatchMessage("97D38429-B916-42DE-AE7B-B6C128BB15EF", Factory, messageText);
			wrapper = new ARLDailyNoticeDocumentWrapper(message);
			accounting = wrapper.PreviousDaysAccountings.First();
			transactions = accounting.Transactions.ToList();
			transaction = transactions[0];
			AssertEquals(dec1, transaction.Declaration);
		}

		[ExpectExceptionMessage(typeof(ArgumentNullException), "Batch type is null in DN message.\r\nParameter name: transactionBatch")]
		public void TestLastMessageIsDNButBatchTypeIsNull()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "8804P04001";

			var message = statement.Messages.AddNew();
			message.EM_MessageText = EDIMessageTextWithoutBatchType;
			message.EM_MessageSubType = ARLMessageTypes.Codes.DailyNotice;
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
		public void TestLoadLatestDeclaration()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTERX";
			importer.OH_FullName = "IMPORTERX NAME";
			var importerAddInfo = OrgImpAddInfo.Get(importer);
			importerAddInfo.ZO_IsGSTDirectPayment = true;
			importerAddInfo.ZO_AccountSecurityNumber = "11222";
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0002", "CA");

			var dec1 = CreatedDeclaration("A2883FFB-3E1D-4994-9B1D-BB56D8A34191", Factory, importer, "B0000111X", "11222783837955", ZDateTime.UtcNow.AddDays(-1));
			var dec2 = CreatedDeclaration("F8CBA338-3B7D-4A42-A907-63872CE6B779", Factory, importer, "B0000111Y", "11222783837955", ZDateTime.UtcNow);
			Factory.Save();

			var messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNUniversalTransactionBatch.xml");
			var message = CreateTransactionBatchMessage("8EBA241A-2047-4D00-9F8C-D74904C28842", Factory, messageText);

			var wrapper = new ARLDailyNoticeDocumentWrapper(message);
			var accounting = wrapper.PreviousDaysAccountings.First();
			var transactions = accounting.Transactions.ToList();
			var transaction = transactions[0];

			AssertEquals(dec2, transaction.Declaration);
		}

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

			var message = CreateDailyNoticeMessageAndSetUpTestData(Factory);

			var org1 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER1");
			var org2 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "IMPORTER2");

			var dec = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001111"));
			dec.JE_OH_Importer = ZGuid.Empty;

			dec = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001112"));
			dec.JE_OH_Importer = ZGuid.Empty;

			dec = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001116"));
			dec.JE_OH_Importer = org1.PK;
			dec.ImporterOfRecordAddress.OrganisationPK = org2.PK;

			var wrapper = new ARLDailyNoticeDocumentWrapper(message);
			CombineAssertions(() =>
			{
				AssertEquals("ImporterBusinessNumber", "123546789", wrapper.ImporterBusinessNumber);
				AssertEquals("OrganizationLegalName", "GUY & MIKE'S IMPORTS INC.12", wrapper.OrganizationLegalName);

				var previousDaysAccountings = wrapper.PreviousDaysAccountings.ToArray();
				AssertEquals("PreviousDaysAccountings.Length", 2, previousDaysAccountings.Length);

				var accounting = previousDaysAccountings.FirstOrDefault(c => c.ImporterBusinessNumber == "123546789RM0001");

				AssertNotNull("1 ImporterBusinessNumber", accounting);
				AssertEquals("1 ImporterCode", "MULTIPLE", accounting.ImporterCode);
				AssertEquals("1 ImporterLegalName", "GUY & MIKE'S IMPORTS INC.", accounting.ImporterLegalName);
				AssertArrayEqualsByElements("1 Importers", new[] { org3, org4 }, accounting.Importers);
				Assert("1 IsImporterDirectPayment", !accounting.IsImporterDirectPayment);
				Assert("1 IsGSTDirectPayment", !accounting.IsGSTDirectPayment);

				accounting = previousDaysAccountings.FirstOrDefault(c => c.ImporterBusinessNumber == "123546789RM0002");

				AssertNotNull("2 ImporterBusinessNumber", accounting);
				AssertEquals("2 ImporterCode", "IMPORTER2", accounting.ImporterCode);
				AssertEquals("2 ImporterLegalName", "GUY & MIKE'S IMPORTS INC.\r\n(IMPORTER2 NAME)", accounting.ImporterLegalName);
				AssertArrayEqualsByElements("2 Importers", new[] { org2 }, accounting.Importers);
				Assert("2 IsImporterDirectPayment", !accounting.IsImporterDirectPayment);
				Assert("2 IsGSTDirectPayment", accounting.IsGSTDirectPayment);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProperties()
		{
			var message = CreateDailyNoticeMessageAndSetUpTestData(Factory);

			var wrapper = new ARLDailyNoticeDocumentWrapper(message);
			CombineAssertions(() =>
			{
				AssertEquals("StatementDate", new ZDate(2014, 11, 18), wrapper.StatementDate);
				AssertEquals("AccountingDate", new ZDate(2014, 11, 17), wrapper.AccountingDate);
				AssertEquals("ImporterBusinessNumber", "123546789", wrapper.ImporterBusinessNumber);
				AssertEquals("OrganizationLegalName", "GUY & MIKE'S IMPORTS INC.12", wrapper.OrganizationLegalName);
				AssertEquals("AccountSecurityCode", "11222", wrapper.AccountSecurityCode);
				AssertEquals("NumberOfSupportingDocuments", 1, wrapper.NumberOfSupportingDocuments);
				AssertEquals("TotalCustomsDuties", 135847.00m, wrapper.TotalCustomsDuties);
				AssertEquals("TotalSIMA", -1150.2m, wrapper.TotalSIMA);
				AssertEquals("TotalExciseTax", 0m, wrapper.TotalExciseTax);
				AssertEquals("TotalGST", 25848.45m, wrapper.TotalGST);
				AssertEquals("TotalOthers", 3425.26m, wrapper.TotalOthers);
				AssertEquals("TotalTotal", 163970.51m, wrapper.TotalTotal);
				AssertEquals("StatementType", "I", wrapper.StatementType);
				AssertEquals("StatementTypeDescription", "Importer", wrapper.StatementTypeDescription);
				AssertEquals("RMAccountNumber", "0001", wrapper.RMAccountNumber);
			});

			CombineAssertions(() =>
			{
				AssertEquals("PreviousDaysAccountings.Count", 2, wrapper.PreviousDaysAccountings.Count());
				var accounting = wrapper.PreviousDaysAccountings.First();
				AssertEquals("ImporterBusinessNumber", "123546789RM0001", accounting.ImporterBusinessNumber);
				AssertEquals("ImporterLegalName", "GUY & MIKE'S IMPORTS INC.\r\n(IMPORTER1 NAME)", accounting.ImporterLegalName);
				AssertEquals("PaymentsReceived", -33300m, accounting.PaymentsReceived);
				AssertEquals("Refund", 0m, accounting.Refund);
				AssertEquals("CustomsDuties", 135987.23m, accounting.CustomsDuties);
				AssertEquals("SIMA", 0m, accounting.SIMA);
				AssertEquals("ExciseTax", 0m, accounting.ExciseTax);
				AssertEquals("GST", 25698.25m, accounting.GST);
				AssertEquals("Others", 100m, accounting.Others);
				AssertEquals("Total", 161785.48m, accounting.Total);
				Assert("IsImporterDirectPayment", accounting.IsImporterDirectPayment);
				Assert("IsGSTDirectPayment", !accounting.IsGSTDirectPayment);

				var transactions = accounting.Transactions.ToList();
				AssertEquals("Transactions.Count", 3, transactions.Count);

				var transaction = transactions[0];
				AssertEquals("DocumentNumber 1", "11222783837955", transaction.DocumentNumber);
				AssertEquals("B3Field6Identifier", "I", transaction.B3Field6Identifier);
				AssertEquals("ReleaseDate", new ZDate(2014, 11, 19), transaction.ReleaseDate);
				AssertEquals("AccountingDate", new ZDate(2014, 12, 1), transaction.AccountingDate);
				AssertEquals("Port", "B3", transaction.DocumentType);
				AssertEquals("Port", "0395", transaction.Port);
				AssertAmounts(transaction, 135987.23m, 0, 0, 0, 0, 135987.23m);
				AssertEquals("B00001111", transaction.Declaration.JE_DeclarationReference);
				Assert("JobNumber", transaction.JobNumber.Contains("B00001111"));
				AssertEquals("Warning", "IMP", transaction.Warning);

				var detailsWrapper = new ARLDailyNoticeDocumentWrapper.TransactionsDetailsWrapper(transaction, accounting, "Transactions");
				AssertEquals("B3Field6Identifier", transaction.B3Field6Identifier, detailsWrapper.B3Field6Identifier);
				AssertEquals("ReleaseDate", transaction.ReleaseDate, detailsWrapper.ReleaseDate);
				AssertEquals("AccountingDate", transaction.AccountingDate, detailsWrapper.AccountingDate);
				AssertEquals("Port", transaction.Port, detailsWrapper.Port);
				AssertEquals("CustomsDuties", transaction.CustomsDuties, detailsWrapper.CustomsDuties);
				AssertEquals("SIMA", transaction.SIMA, detailsWrapper.SIMA);
				AssertEquals("ExciseTax", transaction.ExciseTax, detailsWrapper.ExciseTax);
				AssertEquals("GST", transaction.GST, detailsWrapper.GST);
				AssertEquals("Others", transaction.Others, detailsWrapper.Others);
				AssertEquals("AmountBilled", transaction.AmountBilled, detailsWrapper.AmountBilled);
				AssertEquals("Warning", transaction.Warning, detailsWrapper.Warning);
				AssertEquals("ImporterCode", accounting.ImporterCode, detailsWrapper.ImporterCode);
				AssertEquals("ImporterLegalName", accounting.ImporterLegalName, detailsWrapper.ImporterLegalName);
				AssertEquals("ImporterBusinessNumber", accounting.ImporterBusinessNumber, detailsWrapper.ImporterBusinessNumber);
				AssertEquals("IsImporterDirectPayment", accounting.IsImporterDirectPayment ? "Yes" : "No", detailsWrapper.IsImporterDirectPayment);
				AssertEquals("IsGSTDirectPayment", accounting.IsGSTDirectPayment ? "Yes" : "No", detailsWrapper.IsGSTDirectPayment);
				AssertEquals("PaymentsReceived", accounting.PaymentsReceived, detailsWrapper.PaymentsReceived);
				AssertEquals("Refund", accounting.Refund, detailsWrapper.Refund);
				AssertEquals("TransactionDetailsType", "Transactions", detailsWrapper.TransactionDetailsType);
				AssertEquals("NormalTransaction.TotalPayableByBroker", 100m, detailsWrapper.TotalPayableByBroker);
				AssertEquals("NormalTransaction.TotalPayableByImporter", 161685.48m, detailsWrapper.TotalPayableByImporter);

				transaction = transactions[1];
				AssertEquals("DocumentNumber 2", "11222783837988", transaction.DocumentNumber);
				AssertEquals("B3Field6Identifier", "G", transaction.B3Field6Identifier);
				AssertAmounts(transaction, 0, 0, 0, 25698.25m, 0, 25698.25m);
				AssertEquals("Port", "B3", transaction.DocumentType);
				AssertEquals("Port", "0395", transaction.Port);
				AssertEquals("B00001112", transaction.Declaration.JE_DeclarationReference);
				Assert("JobNumber", transaction.JobNumber.Contains("B00001112"));
				AssertEquals("Warning", "BILL", transaction.Warning);

				transaction = transactions[2];
				AssertEquals("DocumentNumber 3", "11222783837957", transaction.DocumentNumber);
				AssertEquals("Port", "LA", transaction.DocumentType);
				AssertEquals("Port", "0395", transaction.Port);
				AssertAmounts(transaction, 0, 0, 0, 0, 100m, 100m);
				AssertEquals("B00001113", transaction.Declaration.JE_DeclarationReference);
				Assert("JobNumber", transaction.JobNumber.Contains("B00001113"));
				AssertEquals("Warning", "BILL", transaction.Warning);

				AssertEquals("OtherTransactions.Count", 1, accounting.OtherTransactions.Count());
				var otherTransaction = accounting.OtherTransactions.First();
				AssertEquals("DocumentType", "B2", otherTransaction.DocumentType);
				AssertEquals("DocumentNumber", "11222783837973", otherTransaction.DocumentNumber);
				AssertEquals("B3RelatedDocumentNumber", "11222783837974", otherTransaction.B3RelatedDocumentNumber);
				AssertEquals("DocumentDate", new ZDate(2014, 11, 17), otherTransaction.DocumentDate);
				AssertEquals("DocumentType", new ZDate(2014, 11, 17), otherTransaction.PaymentDueDate);
				AssertEquals("DocumentType", 300m, otherTransaction.Total);
				AssertEquals("B00001119", otherTransaction.Declaration.JE_DeclarationReference);
				AssertEquals("B00001120", otherTransaction.RelatedB3Declaration.JE_DeclarationReference);
				Assert("JobNumber", otherTransaction.JobNumber.Contains("B00001119"));
				Assert("JobNumber", otherTransaction.JobNumber.Contains("B00001120"));

				detailsWrapper = new ARLDailyNoticeDocumentWrapper.TransactionsDetailsWrapper(otherTransaction, accounting, "Other Transactions");
				AssertEquals("DocumentDate", otherTransaction.DocumentDate, detailsWrapper.DocumentDate);
				AssertEquals("DocumentTypeDescription", otherTransaction.DocumentTypeDescription, detailsWrapper.DocumentTypeDescription);
				AssertEquals("DocumentNumber", otherTransaction.DocumentNumber, detailsWrapper.DocumentNumber);
				AssertEquals("Total", otherTransaction.Total, detailsWrapper.Total);
				AssertEquals("B3RelatedDocumentNumber", otherTransaction.B3RelatedDocumentNumber, detailsWrapper.B3RelatedDocumentNumber);
				Assert("JobNumber", detailsWrapper.JobNumber.Contains("B00001119"));
				Assert("JobNumber", detailsWrapper.JobNumber.Contains("B00001120"));
				AssertEquals("PaymentDueDate", otherTransaction.PaymentDueDate, detailsWrapper.PaymentDueDate);
				AssertEquals("TransactionDetailsType", "Other Transactions", detailsWrapper.TransactionDetailsType);
				AssertEquals("OtherTransaction.TotalPayableByBroker", 100m, detailsWrapper.TotalPayableByBroker);
				AssertEquals("OtherTransaction.TotalPayableByImporter", 161685.48m, detailsWrapper.TotalPayableByImporter);

				AssertEquals("UnderReviewTransactions.Count", 1, accounting.UnderReviewTransactions.Count());
				var urTransaction = accounting.UnderReviewTransactions.First();
				AssertEquals("DocumentType", "B2", urTransaction.DocumentType);
				AssertEquals("DocumentNumber", "11222783837971", urTransaction.DocumentNumber);
				AssertEquals("B3RelatedDocumentNumber", "11222783837972", urTransaction.B3RelatedDocumentNumber);
				AssertEquals("DocumentDate", new ZDate(2014, 11, 17), urTransaction.DocumentDate);
				AssertEquals("DocumentType", new ZDate(2014, 11, 17), urTransaction.PaymentDueDate);
				AssertEquals("DocumentType", 321.36m, urTransaction.Total);
				AssertEquals("B00001117", urTransaction.Declaration.JE_DeclarationReference);
				AssertEquals("B00001118", urTransaction.RelatedB3Declaration.JE_DeclarationReference);
				Assert("JobNumber", urTransaction.JobNumber.Contains("B00001117"));
				Assert("JobNumber", urTransaction.JobNumber.Contains("B00001118"));

				detailsWrapper = new ARLDailyNoticeDocumentWrapper.TransactionsDetailsWrapper(urTransaction, accounting, "Under Review");
				AssertEquals("TotalCustomsDuties", accounting.CustomsDuties, detailsWrapper.TotalCustomsDuties);
				AssertEquals("TotalSIMA", accounting.SIMA, detailsWrapper.TotalSIMA);
				AssertEquals("TotalExciseTax", accounting.ExciseTax, detailsWrapper.TotalExciseTax);
				AssertEquals("TotalGST", accounting.GST, detailsWrapper.TotalGST);
				AssertEquals("TotalOthers", accounting.Others, detailsWrapper.TotalOthers);
				AssertEquals("TotalTransactionsTotal", accounting.Total, detailsWrapper.TotalTransactionsTotal);
				AssertEquals("TransactionDetailsType", "Under Review", detailsWrapper.TransactionDetailsType);
				AssertEquals("UnderReviewTransaction.TotalPayableByBroker", 100m, detailsWrapper.TotalPayableByBroker);
				AssertEquals("UnderReviewTransaction.TotalPayableByImporter", 161685.48m, detailsWrapper.TotalPayableByImporter);

				accounting = wrapper.PreviousDaysAccountings.ElementAt(1);
				AssertEquals("ImporterBusinessNumber", "123546789RM0002", accounting.ImporterBusinessNumber);
				AssertEquals("ImporterLegalName", "GUY & MIKE'S IMPORTS INC.\r\n(IMPORTER2 NAME)", accounting.ImporterLegalName);
				AssertEquals("PaymentsReceived", -11100m, accounting.PaymentsReceived);
				AssertEquals("Refund", 0m, accounting.Refund);
				AssertEquals("CustomsDuties", -140.23m, accounting.CustomsDuties);
				AssertEquals("SIMA", -1150.2m, accounting.SIMA);
				AssertEquals("ExciseTax", 0m, accounting.ExciseTax);
				AssertEquals("GST", 150.2m, accounting.GST);
				AssertEquals("Others", 3325.26m, accounting.Others);
				AssertEquals("Total", 2185.03m, accounting.Total);
				Assert("IsImporterDirectPayment", !accounting.IsImporterDirectPayment);
				Assert("IsGSTDirectPayment", accounting.IsGSTDirectPayment);

				transactions = accounting.Transactions.ToList();
				AssertEquals("Transactions.Count", 5, transactions.Count);

				transaction = transactions[0];
				AssertEquals("DocumentNumber 4", "11222783837958", transaction.DocumentNumber);
				AssertEquals("B3RelatedDocumentNumber", "11222356875434m", transaction.B3RelatedDocumentNumber);
				AssertEquals("Port", "B2", transaction.DocumentType);
				AssertEquals("Port", "0395", transaction.Port);
				AssertAmounts(transaction, -140.23m, 0, 0, 0, 0, -140.23m);
				AssertEquals("B00001114", transaction.Declaration.JE_DeclarationReference);
				Assert("JobNumber", transaction.JobNumber.Contains("B00001114"));
				AssertEquals("Warning", "CHECK", transaction.Warning);

				transaction = transactions[1];
				AssertEquals("DocumentNumber 5", "11222783837958", transaction.DocumentNumber);
				AssertEquals("Port", "K3", transaction.DocumentType);
				AssertEquals("Port", "0395", transaction.Port);
				AssertAmounts(transaction, 0, 0, 0, 150.2m, 0, 150.2m);
				AssertEquals("B00001114", transaction.Declaration.JE_DeclarationReference);
				Assert("JobNumber", transaction.JobNumber.Contains("B00001114"));
				AssertEquals("Warning", "", transaction.Warning);

				transaction = transactions[2];
				AssertEquals("DocumentNumber 6", "11222783837960", transaction.DocumentNumber);
				AssertEquals("Port", "K3", transaction.DocumentType);
				AssertEquals("Port", "0395", transaction.Port);
				AssertAmounts(transaction, 0, -1150.2m, 0, 0, 0, -1150.2m);
				AssertEquals("B00001116", transaction.Declaration.JE_DeclarationReference);
				Assert("JobNumber", transaction.JobNumber.Contains("B00001116"));
				AssertEquals("Warning", "CHECK", transaction.Warning);

				transaction = transactions[3];
				AssertEquals("DocumentNumber 7", "11222783837961", transaction.DocumentNumber);
				AssertEquals("Port", "P1", transaction.DocumentType);
				AssertEquals("Port", "0395", transaction.Port);
				AssertAmounts(transaction, 0, 0, 0, 0, 3300.26m, 3300.26m);
				AssertNull(transaction.Declaration);
				AssertEquals("", transaction.JobNumber);
				AssertEquals("Warning", "NODEC", transaction.Warning);

				transaction = transactions[4];
				AssertEquals("DocumentNumber 8", "", transaction.DocumentNumber);
				AssertEquals("Port", "NS", transaction.DocumentType);
				AssertEquals("Port", "", transaction.Port);
				AssertAmounts(transaction, 0, 0, 0, 0, 25m, 25m);
				AssertNull(transaction.Declaration);
				AssertEquals("", transaction.JobNumber);
				AssertEquals("Warning", "NODEC", transaction.Warning);
			});

			var collection = wrapper.AllTransactionsForDocument;
			CombineAssertions(() =>
			{
				AssertEquals("collection.Count", 10, collection.Count);
				Assert("IsFirstItemOfCurrentTransactions", collection[0].IsFirstItemOfCurrentTransactions);
				Assert("IsFirstItemOfCurrentGroup", collection[0].IsFirstItemOfCurrentGroup);
				Assert("IsLastItemOfCurrentTransactions", collection[2].IsLastItemOfCurrentTransactions);
				Assert("IsFirstItemOfCurrentTransactions", collection[3].IsFirstItemOfCurrentTransactions);
				Assert("IsLastItemOfCurrentTransactions", collection[3].IsLastItemOfCurrentTransactions);
				Assert("IsFirstItemOfCurrentGroup", !collection[3].IsFirstItemOfCurrentGroup);
				AssertEquals("TotalOtherTransactionsTotal", 300m, collection[3].TotalOtherTransactionsTotal);
				Assert("IsFirstItemOfCurrentTransactions", collection[4].IsFirstItemOfCurrentTransactions);
				Assert("IsLastItemOfCurrentTransactions", collection[4].IsLastItemOfCurrentTransactions);
				AssertEquals("TotalOtherTransactionsTotal", 321.36m, collection[4].TotalOtherTransactionsTotal);
				Assert("IsFirstItemOfCurrentGroup", !collection[4].IsFirstItemOfCurrentGroup);
				Assert("IsFirstItemOfCurrentTransactions", collection[5].IsFirstItemOfCurrentTransactions);
				Assert("IsFirstItemOfCurrentGroup", collection[5].IsFirstItemOfCurrentGroup);
				Assert("IsLastItemOfCurrentTransactions", collection[9].IsLastItemOfCurrentTransactions);
				Assert("IsFirstItemOfCurrentGroup", !collection[9].IsFirstItemOfCurrentGroup);
			});

			message.EM_MessageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNUniversalTransactionBatch3.xml");
			wrapper = new ARLDailyNoticeDocumentWrapper(message);
			collection = wrapper.AllTransactionsForDocument;
			CombineAssertions(() =>
			{
				AssertEquals("collection.Count", 6, collection.Count);
				AssertEquals("TransactionDetailsType", ZString.Empty, collection[5].TransactionDetailsType);
				Assert("IsFirstItemOfCurrentTransactions", !collection[5].IsFirstItemOfCurrentTransactions);
				Assert("IsLastItemOfCurrentTransactions", !collection[5].IsLastItemOfCurrentTransactions);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAccountSecurityCode()
		{
			var message = CreateDailyNoticeMessageAndSetUpTestData(Factory, "12345");
			var wrapper = new ARLDailyNoticeDocumentWrapper(message);
			AssertEquals("AccountSecurityCode", "12345", wrapper.AccountSecurityCode);
		}

		public static Enterprise.Messaging.Business.EDIMessage CreateDailyNoticeMessageAndSetUpTestData(BusinessObjectFactory factory, string accountSecurityCode = null, string messageText = null, GlbBranch branch = null)
		{
			var importer = factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTER1";
			importer.OH_FullName = "IMPORTER1 NAME";
			var importerAddInfo = OrgImpAddInfo.Get(importer);
			importerAddInfo.ZO_IsImporterDirectPayment = true;
			importerAddInfo.ZO_AccountSecurityNumber = "11222";
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0003", "CA");
			CreatedDeclaration("f60e93f9-1332-42df-b6aa-7f632e308be1", factory, importer, "B00001111", "11222783837955", "", "AB", branch: branch);
			CreatedDeclaration("acb03422-0a2c-4c41-abda-c87283fa1517", factory, importer, "B00001112", "11222783837988", branch: branch);
			CreatedDeclaration("e9ff5970-8f0d-47cc-983f-599eeca50f0f", factory, null, "B00001113", "11222783837957", JobMessageTypeList.Codes.LowValueShipments, branch: branch);

			importer = factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTER2";
			importer.OH_FullName = "IMPORTER2 NAME";
			importerAddInfo = OrgImpAddInfo.Get(importer);
			importerAddInfo.ZO_IsGSTDirectPayment = true;
			importerAddInfo.ZO_AccountSecurityNumber = "11222";
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0002", "CA");
			CreatedDeclaration("cac506cc-7efe-495b-899c-01ca33725486", factory, importer, "B00001114", "11222783837958", branch: branch);
			var tran8Dec = CreatedDeclaration("4d1e627a-c2c5-4084-af5c-84c56d28b30b", factory, importer, "B00001116", "11222783837960", branch: branch);
			CreatedDeclaration("a96f5534-f7d1-4640-aec2-b4d4ceffeb7b", factory, importer, "B00001117", "11222783837971", branch: branch);
			CreatedDeclaration("6eebafac-cff1-4c4f-a2d8-ad26c54ab876", factory, importer, "B00001118", "11222783837972", branch: branch);
			CreatedDeclaration("c51f151f-7e5f-47c2-99a2-ad47c120aefb", factory, importer, "B00001119", "11222783837973", branch: branch);
			CreatedDeclaration("78b02631-93ed-4815-b48c-90f4a12788bc", factory, importer, "B00001120", "11222783837974", branch: branch);

			var entryHeader = tran8Dec.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var sentMessage = factory.New<B3Message>();
			sentMessage.EM_MessageText = @"UNH+" + Enterprise.Messaging.Business.EDIMessage.MessageNumberPlaceHolder + "+CUSDEC:S:99B:UN'BGM+:::AB+930+9'CST++I'LOC+41+497'LOC+11+423'LOC+18+12345'RFF+TN:1227'";
			sentMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			sentMessage.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			sentMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-1);
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CAIMP;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_To = "TO";
			interchange.EI_From = "FROM";
			interchange.EI_InterchangeNum = "156";
			sentMessage.EM_EI = interchange.PK;
			entryHeader.Messages.Add(sentMessage);
			var responseMessage = factory.New<B3Message>();
			responseMessage.EM_MessageText = @"UNH+1+CUSRES:S:99B:UN+12345'BGM++930+9'DTM+137:20100212:102'ERP+:I99'RFF+ABO:1227'ERC+942992'DOC+961'CST++1+1+0'UNT+9+1'";
			responseMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			responseMessage.EM_MessageSubType = EntryStatusList.Codes.Clear;
			responseMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			entryHeader.Messages.Add(responseMessage);

			if (messageText == null)
			{
				messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\DNUniversalTransactionBatch.xml");
			}
			if (accountSecurityCode != null)
			{
				messageText = messageText.Replace("<Value>00000</Value>", string.Format("<Value>{0}</Value>", accountSecurityCode));
			}
			var message = CreateTransactionBatchMessage("d6ad4b25-7c49-43e9-bb9b-da8f7249c769", factory, messageText);
			factory.Save();

			return message;
		}

		internal static JobDeclaration CreatedDeclaration(string pk, BusinessObjectFactory factory, OrgHeader importer, ZString jobNumber, ZString transactionNo, ZDateTime createTime, string messageType = "", string messageSubType = "", GlbBranch branch = null)
		{
			var declaration = CreatedDeclaration(pk, factory, importer, jobNumber, transactionNo, messageType, messageSubType, branch);
			declaration.JE_SystemCreateTimeUtc = createTime;
			return declaration;
		}

		internal static JobDeclaration CreatedDeclaration(string pk, BusinessObjectFactory factory, OrgHeader importer, ZString jobNumber, ZString transactionNo, string messageType = "", string messageSubType = "", GlbBranch branch = null)
		{
			var declaration = factory.NewWithPrimaryKey<JobDeclaration>(new Guid(pk));

			declaration.JE_GB = branch?.PK ?? GlbBranch.CurrentBranch.PK;
			if (!string.IsNullOrEmpty(messageType))
			{
				declaration.JE_MessageType = messageType;
			}

			if (!string.IsNullOrEmpty(messageSubType))
			{
				declaration.JE_MessageSubType = messageSubType;
			}

			if (importer != null)
			{
				declaration.JE_OH_Importer = importer.PK;
			}

			declaration.JE_DeclarationReference = jobNumber;
			var documentNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumber.EntryType.CATransactionNumber, Constants.CountryCodes.Canada);
			documentNumber.CE_EntryNum = transactionNo;
			declaration.JE_PaymentMethod = Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;

			var topGroupInvoice = declaration.JobComInvoiceGroupHeaders.AddNew();
			topGroupInvoice.JZ_InvoiceNumber = JobComInvoiceGroupHeader.AllInvoices;

			return declaration;
		}

		void AssertAmounts(ARLDailyNoticeDocumentWrapper.DailyAccounting.TransactionDetails transaction, decimal duty, decimal sima, decimal excise, decimal gst, decimal others, decimal total)
		{
			AssertEquals("CustomsDuty", duty, transaction.CustomsDuties);
			AssertEquals("SIMA", sima, transaction.SIMA);
			AssertEquals("ExciseTax", excise, transaction.ExciseTax);
			AssertEquals("GST", gst, transaction.GST);
			AssertEquals("Others", others, transaction.Others);
			AssertEquals("Total", total, transaction.Total);
		}

		internal static Enterprise.Messaging.Business.EDIMessage CreateTransactionBatchMessage(string pk, BusinessObjectFactory factory, string messageText)
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CAIMP;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_To = "JANETEST";
			interchange.EI_From = "FROM";

			var message = factory.NewWithPrimaryKey<Enterprise.Messaging.Business.EDIMessage>(new Guid(pk));
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message.EM_MessageText = messageText;
			message.EM_EI = interchange.PK;

			return message;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		public static Enterprise.Messaging.Business.EDIMessage CreateTransactionBatchMessage(BusinessObjectFactory factory, string fileName)
		{
			var messageText = File.ReadAllText(BaseSourcePath + $@"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\{fileName}");
			return CreateTransactionBatchMessage(ZGuid.NewZGuid().ToString(), factory, messageText);
		}
	}
}
