using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.Testing
{
	public class PayableTransactionPendingAllocationDataTest : AssemblyDataTest
	{
		public void TestBusinessObjectType() => AssertEquals(typeof(TransactionPendingAllocation), PTPAData.BusinessObjectType);
		public void TestReferenceType() => AssertEquals(Core.Constants.ReferenceTypes.Accounting, PTPAData.ReferenceType);
		public void TestHumanReadableName() => AssertEquals("AP Transaction Pending Allocation", PTPAData.HumanReadableName.ToString());
		public void TestModuleID() => AssertEquals(ModuleIDs.TransactionsPendingAllocation, PTPAData.ModuleID);
		public void TestIsAllowedForUnallocatedeDocs() => Assert(!PTPAData.IsAllowedForUnallocatedeDocs);
		PayableTransactionPendingAllocationData PTPAData => ptpaData ?? (ptpaData = new PayableTransactionPendingAllocationData());
		PayableTransactionPendingAllocationData ptpaData;

		public void TestGetEDocViaUniversalXmlSupport()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "MYTESTORG";
			var uxmlSupport = new PayableTransactionPendingAllocationData().GetEDocsViaUniversalXmlSupport();
			AssertEquals(null, uxmlSupport.LoadBusinessObjectFromCode(Factory, "invalid-code"));

			var transactionPendingAllocationValidCPA = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			transactionPendingAllocationValidCPA.AH_OH = orgHeader.PK;
			transactionPendingAllocationValidCPA.AH_TransactionType = TransactionTypes.CreditNotePendingAllocation;
			transactionPendingAllocationValidCPA.AH_TransactionNum = "00001000";

			var transactionPendingAllocationValidIPA = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			transactionPendingAllocationValidIPA.AH_OH = orgHeader.PK;
			transactionPendingAllocationValidIPA.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			transactionPendingAllocationValidIPA.AH_TransactionNum = "00001001";

			var transactionPendingAllocationBadTransactionType = Factory.NewWithValidTestData<DepositBatch>();
			transactionPendingAllocationBadTransactionType.AH_OH = orgHeader.PK;
			transactionPendingAllocationBadTransactionType.AH_TransactionType = TransactionTypes.ReceiptBatch;
			transactionPendingAllocationBadTransactionType.AH_Ledger = LedgerTypes.AccountsPayable;
			transactionPendingAllocationBadTransactionType.AH_TransactionNum = "00001002";
			Factory.Save();

			AssertNull("Expecting a key in the format 'OrgCode|TransactionType|TransactionNumber', transaction type and transaction number are required", uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG"));
			AssertNull("Expecting a key in the format 'OrgCode|TransactionType|TransactionNumber', org code and transaction type are required", uxmlSupport.LoadBusinessObjectFromCode(Factory, "00001000"));
			AssertNull("Expecting a key in the format 'OrgCode|TransactionType|TransactionNumber', org code and transaction number are required", uxmlSupport.LoadBusinessObjectFromCode(Factory, "CPA"));

			AssertNull("Expecting a key in the format 'OrgCode|TransactionType|TransactionNumber', org code is required", uxmlSupport.LoadBusinessObjectFromCode(Factory, "CPA|00001000"));
			AssertNull("Expecting a key in the format 'OrgCode|TransactionType|TransactionNumber', transaction number is required", uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG|CPA"));
			AssertNull("Expecting a key in the format 'OrgCode|TransactionType|TransactionNumber', transaction type is required", uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG|00001000"));

			AssertNull("Expecting a key in the format 'OrgCode|TransactionType|TransactionNumber', too many '|' seperators", uxmlSupport.LoadBusinessObjectFromCode(Factory, "|MYTESTORG|CPA|00001000"));
			AssertNull("Expecting a key in the format 'OrgCode|TransactionType|TransactionNumber', too many '|' seperators", uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG|CPA|00001000|"));
			AssertNull("Expecting a key in the format 'OrgCode|TransactionType|TransactionNumber', too many '|' seperators", uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG||CPA|00001000"));

			AssertNull("There is no business object matching the criteria, transaction number is incorrect", uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG|CPA|00000003"));
			AssertNull("There is no business object matching the criteria, org code is incorrect", uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG0|CPA|00001000"));
			AssertNull("There is no business object matching the criteria, transaction type is incorrect", uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG|IPA|00001000"));
			AssertNull("There is no business object matching the criteria, transaction type is incorrect", uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG|CPA|00001001"));

			AssertEquals(transactionPendingAllocationValidCPA.PK, uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG|CPA|00001000")?.PK);
			AssertEquals(transactionPendingAllocationValidIPA.PK, uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG|IPA|00001001")?.PK);

			AssertNull("Filter should remove invalid transaction/ledger types", uxmlSupport.LoadBusinessObjectFromCode(Factory, "MYTESTORG|RCB|00001002"));
		}

		public void TestGetEDocViaUniversalXmlSupport_NullOrWhiteSpaceArgumentThrows()
		{
			var uxmlSupport = new PayableTransactionPendingAllocationData().GetEDocsViaUniversalXmlSupport();
			AssertExceptionThrown<ArgumentException>("Factory Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(null, "MYTESTORG|CPA|00001000"));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be string empty", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, ""));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, null));
		}

		public void TestGetEDocViaUniversalXmlSupport_ExampleCodeFormat()
		{
			AssertEquals("ABIGAS|CPA|100001", new PayableTransactionPendingAllocationData().GetEDocsViaUniversalXmlSupport().ExampleCodeFormat);
		}
	}
}
