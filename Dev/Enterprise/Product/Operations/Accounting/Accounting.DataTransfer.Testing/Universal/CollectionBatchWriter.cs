using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalBankAccount = Enterprise.UniversalDataBuss.DataObjects.Accounting.BankAccount;
using UniversalOrgAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal
{
	public class CollectionBatchWriterTest : TestCaseWithFactory
	{
		public void TestWrite()
		{
			var registry = AccountingConfigurationRegistry.Instance.TransactionNumberOptionForCollectionBatchExportFile;
			AssertEquals(AccountingMasterFilesConstants.TransactionNumberCodes.ComplianceOrInvoiceNr, registry.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			var writer = new CollectionBatchWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, batch)));
			var batchDataObject = writer.GetDataObject(batch);
			AssertNotNull(batchDataObject);
			AssertBatchWasWrittenCorrectly(batch, batchDataObject);

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.TransactionNumberCodes.InvoiceNr))
			{
				batchDataObject = writer.GetDataObject(batch);
				AssertNotNull(batchDataObject);
				AssertBatchWasWrittenCorrectly(batch, batchDataObject);
			}
		}

		public void TestWrite_Unknown()
		{
			batch.ACB_CollectionFileFormat = CollectionFileFormatList.Codes.unknown;
			TestWrite();
		}

		public void TestWrite_Sepa()
		{
			batch.ACB_CollectionFileFormat = CollectionFileFormatList.Codes.sepaFormat;
			TestWrite();
		}

		public void TestWrite_WithEmptyTransactions()
		{
			var writer = new CollectionBatchWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, BatchEmpty)));
			var errMessage = "Batch number 00001002 without valid transactions, {0} cannot be created.";

			AssertEquals("File Format must be RIBA", CollectionFileFormatList.Codes.ribaFormat, BatchEmpty.ACB_CollectionFileFormat);
			AssertExceptionThrown<DataObjectValidationException>(string.Format(errMessage, "RIBA"), () => writer.GetDataObject(BatchEmpty));
			AssertNoRowErrors("No Row Errors must be present", BatchEmpty);
			var eventLogs = BatchEmpty.Logs.GetAllLogs();
			AssertEquals("There should be 1 event log.", 1, eventLogs.Count);
			AssertEquals("ADD", eventLogs[0].SL_SE_NKEvent);

			BatchEmpty.ClearRowNotifications();
			BatchEmpty.ACB_CollectionFileFormat = CollectionFileFormatList.Codes.sepaFormat;
			Factory.Save();

			AssertExceptionThrown<DataObjectValidationException>(string.Format(errMessage, "SEPA"), () => writer.GetDataObject(BatchEmpty));
			AssertNoRowErrors("No Row Errors must be present", BatchEmpty);
			eventLogs = BatchEmpty.Logs.GetAllLogs();
			AssertEquals("There should be 2 (one more only) event logs.", 2, eventLogs.Count);
			AssertEquals("EDT", eventLogs[1].SL_SE_NKEvent);

			BatchEmpty.ClearRowNotifications();
			BatchEmpty.ACB_CollectionFileFormat = CollectionFileFormatList.Codes.unknown;
			Factory.Save();

			var batchDataObject = writer.GetDataObject(BatchEmpty);
			AssertNotNull(batchDataObject);
			AssertNotNull("DataContext is not empty", batchDataObject.DataContext);
			var dsCollection = batchDataObject.DataContext.DataSourceCollection;
			AssertEquals(1, dsCollection.Count());
			var dataSource = dsCollection.First();
			AssertEquals("00001002", dataSource.Key);
			AssertEquals("CollectionBatch", dataSource.Type);
			var collOrders = BatchEmpty.CollectionOrders;
			AssertEquals("Only one CollectionOrder", 1, collOrders.Count);
			AssertEquals("Only one CollectionOrderLine", 1, collOrders[0].CollectionOrderLines.Count);
			AssertEquals("TransactionCollection is empty", 0, batchDataObject.TransactionCollection.Count);

			AssertNoRowErrors("No Row Errors must be present", BatchEmpty);
			eventLogs = BatchEmpty.Logs.GetAllLogs();
			AssertEquals("There should be 3 (one more only) event logs.", 3, eventLogs.Count);
			AssertEquals("EDT", eventLogs[2].SL_SE_NKEvent);
		}

		void AssertBatchWasWrittenCorrectly(AccCollectionBatch batch, TransactionBatch batchDataObject)
		{
			var isRibaOrSepa = batch.ACB_CollectionFileFormat == CollectionFileFormatList.Codes.ribaFormat || batch.ACB_CollectionFileFormat == CollectionFileFormatList.Codes.sepaFormat;
			var isCinTransactionNumberOption = AccountingConfigurationRegistry.Instance.TransactionNumberOptionForCollectionBatchExportFile.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty) == AccountingMasterFilesConstants.TransactionNumberCodes.ComplianceOrInvoiceNr;
			AssertEquals(1, batchDataObject.DataContext.DataSourceCollection.Count(x => true));
			IDataSourceDataObject dataSource = batchDataObject.DataContext.DataSourceCollection.First();
			AssertEquals("00001001", dataSource.Key);
			AssertEquals("CollectionBatch", dataSource.Type);

			Assert("No Row Errors must be present", !batch.HasRowErrors);
			AssertEquals("TransactionCollection.Count", 4, batchDataObject.TransactionCollection.Count);

			var pairsForOrderLineAndTransactionInfo = new List<Tuple<AccCollectionOrderLine, TransactionInfo>>();
			List<AccTransactionHeader> transactionsInBatch = new List<AccTransactionHeader>();
			foreach (var order in batch.CollectionOrders)
			{
				foreach (var orderLine in order.CollectionOrderLines)
				{
					pairsForOrderLineAndTransactionInfo.Add(
						new Tuple<AccCollectionOrderLine, TransactionInfo>(orderLine,
						batchDataObject.TransactionCollection.Single(x => (x.Number.Value == orderLine.TransactionNumber || x.Number.Value == orderLine.Transaction.AH_TransactionReference)))
						);
				}
			}
			AssertEquals("Pairs.Count", 4, pairsForOrderLineAndTransactionInfo.Count);

			foreach (var pair in pairsForOrderLineAndTransactionInfo)
			{
				var order = pair.Item1.CollectionOrder;
				var transaction = pair.Item1.Transaction;
				var transactionInfo = pair.Item2;

				AssertEquals("OrderNumber", order.ACO_OrderNumber.IsEmpty ? null : order.ACO_OrderNumber, transactionInfo.TransactionReference);
				AssertEquals("OrderCollectionDate", order.ACO_CollectionDate.IsEmpty ? (ZDateTime?)null : order.ACO_CollectionDate, transactionInfo.OrderCollectionDate);

				if (!order.ACO_OH_Debtor.IsEmpty)
				{
					var orgHeader = Factory.Load<OrgHeader>(order.ACO_OH_Debtor);
					AssertNotNull("Must be a valid Org", orgHeader);
					var officeAddress = orgHeader.Addresses.DefaultAddressOfType(OrgAddressType.Office);
					AssertNotNull("Must be a valid Office Address", officeAddress);
					AssertNotNull("OrganizationAddress", transactionInfo.OrganizationAddress);
					AssertOrgAddress(officeAddress, transactionInfo.OrganizationAddress);

					AssertEquals(batch.ACB_CollectionFileFormat, transactionInfo.SettlementMethod.Method.Code);
					if (orgHeader.OH_Code == "ABIGAS")
					{
						AssertEquals("Debtor bank account UMRReference", "123456", transactionInfo.SettlementMethod.AuthorizationReference);
					}
					else if (orgHeader.OH_Code == "AALSHI")
					{
						AssertEquals("Debtor bank account UMRReference", "654321", transactionInfo.SettlementMethod.AuthorizationReference);
					}
				}

				AssertEquals("TransactionType", transaction.AH_TransactionType.IsEmpty ? null : new TransactionTypeConverter().ToEnumValue(transaction.AH_TransactionType), transactionInfo.TransactionType);
				AssertEquals("PostDate", transaction.AH_PostDate.IsEmpty ? (ZDateTime?)null : transaction.AH_PostDate, transactionInfo.PostDate);
				AssertEquals("DueDate", transaction.AH_DueDate.IsEmpty ? (ZDateTime?)null : transaction.AH_DueDate, transactionInfo.DueDate);
				AssertEquals("InvoiceDate", transaction.AH_InvoiceDate.IsEmpty ? (ZDateTime?)null : transaction.AH_InvoiceDate, transactionInfo.TransactionDate);
				if (isRibaOrSepa && isCinTransactionNumberOption && !transaction.AH_ComplianceSubType.IsEmpty && !transaction.AH_TransactionReference.IsEmpty)
				{
					AssertEquals("Number", transaction.AH_TransactionReference, transactionInfo.Number);
					AssertEquals("Compliance SubType", transaction.AH_ComplianceSubType, transactionInfo.ComplianceSubType);
				}
				else
				{
					AssertEquals("Number", transaction.AH_TransactionNum.IsEmpty ? null : transaction.AH_TransactionNum, transactionInfo.Number);
					Assert("Compliance SubType must be empty", string.IsNullOrEmpty(transactionInfo.ComplianceSubType));
				}
				AssertEquals("JobInvoiceNumber", transaction.JobNumber.IsEmpty ? null : transaction.JobNumber, transactionInfo.JobInvoiceNumber);
				AssertEquals("OutstandingAmount", transaction.AH_OutstandingAmount, transactionInfo.OutstandingAmount);
				AssertNotNull("LocalCurrency", transactionInfo.LocalCurrency);
				AssertEquals("LocalCurrency.Code", transaction.Company.LocalCurrency.RX_Code, transactionInfo.LocalCurrency.Code);
				AssertEquals("LocalTotal", ((TransactionHeader)transaction).AH_LocalTotalAmount, transactionInfo.LocalTotal);
				AssertNotNull("OSCurrency", transactionInfo.OSCurrency);
				AssertEquals("OSCurrency.Code", transaction.AH_RX_NKTransactionCurrency, transactionInfo.OSCurrency.Code);
				AssertEquals("LocalTotal", ((TransactionHeader)transaction).AH_OSTotalAmount, transactionInfo.OSTotal);

				AssertNotNull(transactionInfo.BankAccountCollection);
				AssertEquals(2, transactionInfo.BankAccountCollection.Count);
				AssertBankAccountDetail(order.CollectionRequestBankAccountDetail, transactionInfo.BankAccountCollection[0]);
				AssertBankAccountDetail(batch.BankAccount, transactionInfo.BankAccountCollection[1]);
			}
		}

		void AssertBankAccountDetail(AccBankAccount accountDetail, UniversalBankAccount bankAccount)
		{
			AssertEquals("BankName", accountDetail.AB_BankName, bankAccount.BankName);
			AssertEquals("AccountName", accountDetail.AB_BankAccountName, bankAccount.AccountName);
			AssertEquals("AccountNumber", accountDetail.AB_AccountNum, bankAccount.AccountNumber);
			AssertEquals("BankBranch", accountDetail.AB_BSB, bankAccount.BankBranch);
			AssertEquals("BankSwift", accountDetail.AB_SWIFT, bankAccount.BankSwift);
			AssertEquals("Currency", accountDetail.AB_RX_NKAccountCurrency, bankAccount.Currency);
			AssertEquals("IsDefaultAccount", accountDetail.AB_IsDefaultReceiptBankAccount, bankAccount.IsDefaultAccount);
			AssertEquals("IBANNumber", ZString.Empty, bankAccount.IBANNumber);
			AssertEquals("Country code", accountDetail.BankAccountCountry.Code, bankAccount.Country.Code);
			AssertEquals("Country description", accountDetail.BankAccountCountry.RN_Desc, bankAccount.Country.Name);
			AssertEquals("Account type", BankAccountType.Credit, bankAccount.AccountType);
			if (!accountDetail.AB_AutoDDRFormat.IsEmpty)
			{
				AssertNotNull(bankAccount.AutoDDRFormat);
				AssertEquals("AutoDDRFormat", accountDetail.AB_AutoDDRFormat, bankAccount.AutoDDRFormat.Code);
				AssertEquals("AutoDDRFormat.Description", new BankDDRFormatList().GetDescriptionFromCode(bankAccount.AutoDDRFormat.Code), bankAccount.AutoDDRFormat.Description);
			}
		}

		void AssertBankAccountDetail(AccARAccountDetails accountDetail, UniversalBankAccount bankAccount)
		{
			AssertEquals("BankName", accountDetail.A1_BankName, bankAccount.BankName);
			AssertEquals("AccountName", accountDetail.A1_AccountName, bankAccount.AccountName);
			AssertEquals("AccountNumber", accountDetail.A1_BankAccount, bankAccount.AccountNumber);
			AssertEquals("BankBranch", accountDetail.A1_BankBsb, bankAccount.BankBranch);
			AssertEquals("BankSwift", accountDetail.A1_BankSwift, bankAccount.BankSwift);
			AssertEquals("Currency", accountDetail.A1_RX_NKAccountCurrency, bankAccount.Currency);
			AssertEquals("IsDefaultAccount", accountDetail.A1_IsDefaultAccount, bankAccount.IsDefaultAccount);
			AssertEquals("IBANNumber", accountDetail.A1_IBANNumber, bankAccount.IBANNumber);
			AssertEquals("Country code", accountDetail.Country.Code, bankAccount.Country.Code);
			AssertEquals("Country description", accountDetail.Country.RN_Desc, bankAccount.Country.Name);
			AssertEquals("Account type", BankAccountType.Debit, bankAccount.AccountType);
		}

		void AssertOrgAddress(OrgAddress officeAddress, UniversalOrgAddress orgAddress)
		{
			AssertEquals("AddressType", OrgAddressType.Office.ToString(), orgAddress.AddressType);
			AssertEquals("AddressOverride", false, orgAddress.AddressOverride);
			AssertEquals("OrganizationCode", officeAddress.Header.OH_Code, orgAddress.OrganizationCode);
			AssertEquals("AddressShortCode", officeAddress.OA_Code, orgAddress.AddressShortCode);

			if (!officeAddress.OA_RL_NKRelatedPortCode.IsEmpty)
			{
				AssertNotNull("Port", orgAddress.Port);
				AssertEquals("Port.Code", officeAddress.OA_RL_NKRelatedPortCode, orgAddress.Port.Code);
				AssertEquals("Port.Name", officeAddress.PortName, orgAddress.Port.Name);
			}
			else
			{
				AssertNull("Port", orgAddress.Port);
			}

			ZString? companyNameOverride = officeAddress.OA_CompanyNameOverride.IsEmpty ? null : officeAddress.OA_CompanyNameOverride;
			AssertEquals("orgAddress.CompanyName", companyNameOverride.HasValue && !companyNameOverride.Value.IsEmpty ? companyNameOverride : officeAddress.Header.OH_FullName, orgAddress.CompanyName);

			if (!officeAddress.OA_RN_NKCountryCode.IsEmpty)
			{
				AssertNotNull("Country", orgAddress.Country);
				AssertEquals("Country.Code", officeAddress.OA_RN_NKCountryCode, orgAddress.Country.Code);
				AssertEquals("Country.Name", officeAddress.Country != null ? officeAddress.Country.RN_Desc : null, orgAddress.Country.Name);
			}
			else
			{
				AssertNull("Country", orgAddress.Country);
			}

			if (!officeAddress.Header.OH_ScreeningStatus.IsEmpty)
			{
				AssertNotNull("ScreeningStatus", orgAddress.ScreeningStatus);
				AssertEquals("ScreeningStatus.Code", officeAddress.Header.OH_ScreeningStatus, orgAddress.ScreeningStatus.Code);
				AssertEquals("ScreeningStatus.Description", new ScreeningStatusesList().GetDescriptionFromCode(orgAddress.ScreeningStatus.Code), orgAddress.ScreeningStatus.Description);
			}
			else
			{
				AssertNull("ScreeningStatus", orgAddress.ScreeningStatus);
			}

			AssertEquals("AddressShortCode", officeAddress.OA_Code, orgAddress.AddressShortCode);
			AssertEquals("Address1", officeAddress.OA_Address1.IsEmpty ? null : officeAddress.OA_Address1, orgAddress.Address1);
			AssertEquals("Address2", officeAddress.OA_Address2.IsEmpty ? null : officeAddress.OA_Address2, orgAddress.Address2);
			AssertEquals("City", officeAddress.OA_City.IsEmpty ? null : officeAddress.OA_City, orgAddress.City);
			AssertEquals("Postcode", officeAddress.OA_PostCode.IsEmpty ? null : officeAddress.OA_PostCode, orgAddress.Postcode);
			AssertEquals("State", officeAddress.OA_State.IsEmpty ? null : officeAddress.OA_State, orgAddress.State);

			AssertEquals("Email", officeAddress.OA_Email.IsEmpty ? null : officeAddress.OA_Email, orgAddress.Email);
			AssertEquals("Fax", officeAddress.OA_Fax.IsEmpty ? null : officeAddress.OA_Fax, orgAddress.Fax);
			AssertEquals("Phone", officeAddress.OA_Phone.IsEmpty ? null : officeAddress.OA_Phone, orgAddress.Phone);

			if (officeAddress.Header.CustomsCodes.Any())
			{
				AssertNotNull("RegistrationNumberCollection", orgAddress.RegistrationNumberCollection);
				AssertEquals("TransactionCollection.Count", officeAddress.Header.CustomsCodes.Count, orgAddress.RegistrationNumberCollection.Count);

				for (int i = 0; i < officeAddress.Header.CustomsCodes.Count; i++)
				{
					var regNo = officeAddress.Header.CustomsCodes[i];
					var regNoInfo = orgAddress.RegistrationNumberCollection[i];

					if (regNo.OK_RN_NKCodeCountry != ZString.Empty)
					{
						AssertNotNull("CountryOfIssue", regNoInfo.CountryOfIssue);
						AssertEquals("CountryOfIssue.Code", regNo.OK_RN_NKCodeCountry.IsEmpty ? null : regNo.OK_RN_NKCodeCountry, regNoInfo.CountryOfIssue.Code);
						AssertEquals("CountryOfIssue.Name", regNo.CodeCountry.RN_Desc.IsEmpty ? null : regNo.CodeCountry.RN_Desc, regNoInfo.CountryOfIssue.Name);
					}
					else
					{
						AssertNull("CountryOfIssue", regNoInfo.CountryOfIssue);
					}

					AssertNotNull("Type", regNoInfo.Type);
					AssertEquals("Type.Code", regNo.OK_CodeType.IsEmpty ? null : regNo.OK_CodeType, regNoInfo.Type.Code);
					AssertEquals("Type.Description", regNo.CustomsRegNoFieldType.IsEmpty ? null : regNo.CustomsRegNoFieldType, regNoInfo.Type.Description);

					AssertEquals("Value", regNo.OK_CustomsRegNo.IsEmpty ? null : regNo.OK_CustomsRegNo, regNoInfo.Value);
				}
			}
		}

		#region Implementation

		AccCollectionBatch batch;
		AccCollectionBatch BatchEmpty;

		protected override void SetUp()
		{
			base.SetUp();

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			batch = TestObjectCreator.CreateCollectionBatch(bankAccount, GlbCompany.CurrentCompany, "00001001", 100m, false);

			var invoice1 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 20, 0M, 20, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);
			var invoice2 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 30, 0M, 30, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);
			var invoice3 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M, 20, 0M, 20, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);
			var invoice4 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV4", TestObjectCreator.AUD, 1M, 30, 0M, 30, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);

			invoice1.AH_ComplianceSubType = "ARI";
			invoice1.AH_TransactionReference = "ARI000001015";
			invoice2.AH_ComplianceSubType = "ARI";
			invoice2.AH_TransactionReference = "ARI000001018";
			invoice4.AH_ComplianceSubType = "ARI";
			invoice4.AH_TransactionReference = "ARI000001022";

			var document1 = Factory.NewWithValidTestData<JobRequiredDocument>();
			document1.EQ_DocType = "UMR";
			document1.EQ_DocUsage = "DBT";
			document1.EQ_DocPeriod = "PER";
			document1.EQ_ValidToDate = ZDateTime.Now.AddMonths(2);
			document1.EQ_DocNumber = "123456";
			TestObjectCreator.ABIGAS.RequiredDocuments.Add(document1);

			var document2 = Factory.NewWithValidTestData<JobRequiredDocument>();
			document2.EQ_DocType = "UMR";
			document2.EQ_DocUsage = "DBT";
			document2.EQ_DocPeriod = "PER";
			document2.EQ_ValidToDate = ZDateTime.Now.AddMonths(2);
			document2.EQ_DocNumber = "654321";
			TestObjectCreator.AALSHI.RequiredDocuments.Add(document2);

			var order1 = TestObjectCreator.CreateCollectionOrder(batch, ZDateTime.Today.Date, TestObjectCreator.ABIGAS, "0000001", 50m, false);
			TestObjectCreator.CreateCollectionOrderLine(order1, invoice1, false);
			TestObjectCreator.CreateCollectionOrderLine(order1, invoice2, false);
			TestObjectCreator.ABIGAS.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.CUU, "11111", Core.Constants.CountryCodes.Italy);
			TestObjectCreator.ABIGAS.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, "99999", ZString.Empty);

			var order2 = TestObjectCreator.CreateCollectionOrder(batch, ZDateTime.Today.Date.AddDays(1), TestObjectCreator.AALSHI, "0000002", 50m, false);
			TestObjectCreator.CreateCollectionOrderLine(order2, invoice3, false);
			TestObjectCreator.CreateCollectionOrderLine(order2, invoice4, false);

			var accountDetail1 = Factory.NewWithValidTestData<AccARAccountDetails>();
			accountDetail1.A1_OB = TestObjectCreator.ABIGAS.CompanyData.PK;
			accountDetail1.A1_PaymentMethod = AccARAccountDetails.ARCollectionRequest;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_RX_NKAccountCurrency = order1.ACO_RX_NKCurrency;
			accountDetail1.A1_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			TestObjectCreator.ABIGAS.CompanyData.ARAccountDetailsCollection.Add(accountDetail1);

			var accountDetail2 = Factory.NewWithValidTestData<AccARAccountDetails>();
			accountDetail2.A1_OB = TestObjectCreator.AALSHI.CompanyData.PK;
			accountDetail2.A1_PaymentMethod = AccARAccountDetails.ARCollectionRequest;
			accountDetail2.A1_IsDefaultAccount = true;
			accountDetail2.A1_RX_NKAccountCurrency = order2.ACO_RX_NKCurrency;
			accountDetail2.A1_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			TestObjectCreator.AALSHI.CompanyData.ARAccountDetailsCollection.Add(accountDetail2);

			BatchEmpty = TestObjectCreator.CreateCollectionBatch(bankAccount, GlbCompany.CurrentCompany, "00001002", 100m, false);

			//BatchEmpty not really empty, but with an invoice that has Amounts = 0
			var invoice5 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV5", TestObjectCreator.AUD, 1M, 0, 0M, 0, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK,
				ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);

			invoice5.AH_ComplianceSubType = "ARI";
			invoice5.AH_TransactionReference = "ARI000001145";

			var order3 = TestObjectCreator.CreateCollectionOrder(BatchEmpty, ZDateTime.Today.Date.AddDays(1), TestObjectCreator.AALSHI, "0000003", 50m, false);
			TestObjectCreator.CreateCollectionOrderLine(order3, invoice5, false);

			var accountDetail3 = Factory.NewWithValidTestData<AccARAccountDetails>();
			accountDetail3.A1_OB = TestObjectCreator.AALSHI.CompanyData.PK;
			accountDetail3.A1_PaymentMethod = AccARAccountDetails.ARCollectionRequest;
			accountDetail3.A1_IsDefaultAccount = true;
			accountDetail3.A1_RX_NKAccountCurrency = order3.ACO_RX_NKCurrency;
			accountDetail3.A1_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			TestObjectCreator.AALSHI.CompanyData.ARAccountDetailsCollection.Add(accountDetail3);

			Factory.Save();
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
