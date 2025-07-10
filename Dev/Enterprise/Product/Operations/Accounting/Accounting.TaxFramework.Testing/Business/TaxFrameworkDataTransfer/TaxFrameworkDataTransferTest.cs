using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	public class TaxFrameworkDataTransferTest : TestCaseWithFactory
	{
		public void TestTaxFrameworkDataTransfer_CtorThrowsException_When_TaxRecordPivotProcessorArgumentIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaxFrameworkDataTransfer(null));
		}

		#region CreateTaxRecordDataFromTaxTransactions

		public void TestCreateTaxRecordDataFromTaxTransactions_EmptyTaxRecordsAndPivotsList_OtherTaxesCalculated()
		{
			var taxParentMock = SetupTaxParentMock();

			var taxRecordData = TestTaxFrameworkDataTransfer.CreateTaxRecordDataFromTaxTransactions(taxParentMock.Object, Array.Empty<AccTaxTransaction>(), Array.Empty<AccTaxRecordTransactionLinePivot>());
			AssertEquals(0, taxRecordData.Count);
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_EmptyTaxRecordsAndPivotsList_OtherTaxesNotCalculated()
		{
			var taxParentMock = SetupTaxParentMock(false);

			var taxRecordData = TestTaxFrameworkDataTransfer.CreateTaxRecordDataFromTaxTransactions(taxParentMock.Object, Array.Empty<AccTaxTransaction>(), Array.Empty<AccTaxRecordTransactionLinePivot>());
			AssertNull(taxRecordData);
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_LengthOfReturnedTaxRecordDataList()
		{
			var taxParentMock = SetupTaxParentMock();

			int numberOfTaxTransactions = 2;
			AssertLengthOfTaxRecordDataList();

			numberOfTaxTransactions = 3;
			AssertLengthOfTaxRecordDataList();

			void AssertLengthOfTaxRecordDataList()
			{
				var taxRecordsAndPivots = CreateTestTaxTransactionsAndPivots(numberOfTaxTransactions);
				var taxRecordData = TestTaxFrameworkDataTransfer.CreateTaxRecordDataFromTaxTransactions(taxParentMock.Object, taxRecordsAndPivots.TaxRecords, taxRecordsAndPivots.Pivots);
				AssertEquals(numberOfTaxTransactions, taxRecordData.Count);
			}
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxMessagePK_ValidValue()
		{
			var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			taxMessage.A9_Code = "TESTCODE";
			Factory.Save();

			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_A9_TaxMessage = taxMessage.PK, (taxRecordDataObject) => AssertEquals(taxMessage.PK, taxRecordDataObject.TaxMessagePK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxMessagePK_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_A9_TaxMessage = ZGuid.Empty, (taxRecordDataObject) => AssertEquals(ZGuid.Empty, taxRecordDataObject.TaxMessagePK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_AffectsSourceTransactionTotal_false()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_AffectsSourceTransactionTotal = false, (taxRecordDataObject) => AssertEquals(false, taxRecordDataObject.AffectsSourceTransactionTotal));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_AffectsSourceTransactionTotal_true()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_AffectsSourceTransactionTotal = true, (taxRecordDataObject) => AssertEquals(true, taxRecordDataObject.AffectsSourceTransactionTotal));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_LedgerControlGLAccountPK_ValidValue()
		{
			var ledgerCountrolAccount = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();

			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_AG_LedgerControlAccount = ledgerCountrolAccount.PK, (taxRecordDataObject) => AssertEquals(ledgerCountrolAccount.PK, taxRecordDataObject.LedgerControlGLAccountPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_LedgerControlGLAccountPK_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_AG_LedgerControlAccount = ZGuid.Empty, (taxRecordDataObject) => AssertEquals(ZGuid.Empty, taxRecordDataObject.LedgerControlGLAccountPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxControlGLAccountPK_ValidValue()
		{
			var taxControlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();

			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_AG_TaxControlAccount = taxControlAccount.PK, (taxRecordDataObject) => AssertEquals(taxControlAccount.PK, taxRecordDataObject.TaxControlGLAccountPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxControlGLAccountPK_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_AG_TaxControlAccount = ZGuid.Empty, (taxRecordDataObject) => AssertEquals(ZGuid.Empty, taxRecordDataObject.TaxControlGLAccountPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxExpenseGLAccountPK_ValidValue()
		{
			var taxExpenseAccount = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();

			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_AG_TaxExpenseAccount = taxExpenseAccount.PK, (taxRecordDataObject) => AssertEquals(taxExpenseAccount.PK, taxRecordDataObject.TaxExpenseGLAccountPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxExpenseGLAccountPK_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_AG_TaxExpenseAccount = ZGuid.Empty, (taxRecordDataObject) => AssertEquals(ZGuid.Empty, taxRecordDataObject.TaxExpenseGLAccountPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxPendingControlGLAccountPK_ValidValue()
		{
			var taxPendingControlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();

			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_AG_TaxPendingControlAccount = taxPendingControlAccount.PK, (taxRecordDataObject) => AssertEquals(taxPendingControlAccount.PK, taxRecordDataObject.TaxPendingControlGLAccountPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxPendingControlGLAccountPK_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_AG_TaxPendingControlAccount = ZGuid.Empty, (taxRecordDataObject) => AssertEquals(ZGuid.Empty, taxRecordDataObject.TaxPendingControlGLAccountPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxIDPK_ValidValue()
		{
			var taxID = Factory.NewWithValidTestData<AccTaxRate>();
			Factory.Save();

			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_AT_TaxID = taxID.PK, (taxRecordDataObject) => AssertEquals(taxID.PK, taxRecordDataObject.TaxIDPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxIDPK_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_AT_TaxID = ZGuid.Empty, (taxRecordDataObject) => AssertEquals(ZGuid.Empty, taxRecordDataObject.TaxIDPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxBasis_ValidValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_Basis = TaxBasisList.PostingOnMatching.Code, (taxRecordDataObject) => AssertEquals(TaxBasisList.PostingOnMatching.Code, taxRecordDataObject.TaxBasis));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxBasis_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_Basis = ZString.Empty, (taxRecordDataObject) => AssertEquals(ZString.Empty, taxRecordDataObject.TaxBasis));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxConfigurationPK_ValidValue()
		{
			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			Factory.Save();

			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_ETC = taxConfig.PK, (taxRecordDataObject) => AssertEquals(taxConfig.PK, taxRecordDataObject.TaxConfigurationPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxConfigurationPK_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_ETC = ZGuid.Empty, (taxRecordDataObject) => AssertEquals(ZGuid.Empty, taxRecordDataObject.TaxConfigurationPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_LocalTaxAmount()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_LocalTaxAmount = 111m, (taxRecordDataObject) => AssertEquals(111m, taxRecordDataObject.LocalTaxAmount));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_LocalTaxBaseAmount()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_LocalTaxBaseAmount = 222m, (taxRecordDataObject) => AssertEquals(222m, taxRecordDataObject.LocalTaxBaseAmount));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_OSTaxAmount()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_OSTaxAmount = 333m, (taxRecordDataObject) => AssertEquals(333m, taxRecordDataObject.OSTaxAmount));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_OSTaxBaseAmount()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_OSTaxBaseAmount = 444m, (taxRecordDataObject) => AssertEquals(444m, taxRecordDataObject.OSTaxBaseAmount));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_BranchPK_ValidValue()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_GB = branch.PK, (taxRecordDataObject) => AssertEquals(branch.PK, taxRecordDataObject.BranchPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_BranchPK_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_GB = ZGuid.Empty, (taxRecordDataObject) => AssertEquals(ZGuid.Empty, taxRecordDataObject.BranchPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_DepartmentPK_ValidValue()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_GE_Department = department.PK, (taxRecordDataObject) => AssertEquals(department.PK, taxRecordDataObject.DepartmentPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_DepartmentPK_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_GE_Department = ZGuid.Empty, (taxRecordDataObject) => AssertEquals(ZGuid.Empty, taxRecordDataObject.DepartmentPK));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_Ledger_ValidValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_Ledger = LedgerTypesList.Codes.AccountsReceivable, (taxRecordDataObject) => AssertEquals(LedgerTypesList.Codes.AccountsReceivable, taxRecordDataObject.Ledger));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_Ledger_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_Ledger = ZString.Empty, (taxRecordDataObject) => AssertEquals(ZString.Empty, taxRecordDataObject.Ledger));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_PostDate_ValidValue()
		{
			var postDate = new ZDateTime(2020, 04, 10).Date;
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_PostDate = postDate, (taxRecordDataObject) => AssertEquals(postDate, taxRecordDataObject.PostDate));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_PostDate_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_PostDate = ZDate.Empty, (taxRecordDataObject) => AssertEquals(ZDate.Empty, taxRecordDataObject.PostDate));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_RateDenominator()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_RateDenominator = 9999, (taxRecordDataObject) => AssertEquals(9999, taxRecordDataObject.RateDenominator));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_RateNumerator()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_RateNumerator = 125, (taxRecordDataObject) => AssertEquals(125, taxRecordDataObject.RateNumerator));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_RealisationDate_ValidValue()
		{
			var realisationDate = new ZDateTime(2020, 04, 10).Date;
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_RealisationDate = realisationDate, (taxRecordDataObject) => AssertEquals(realisationDate, taxRecordDataObject.RealisationDate));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_RealisationDate_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_RealisationDate = ZDate.Empty, (taxRecordDataObject) => AssertEquals(ZDate.Empty, taxRecordDataObject.RealisationDate));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_OSTaxCurrency_ValidValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_RX_NKOSTaxCurrency = Core.Constants.CurrencyCodes.Australia, (taxRecordDataObject) => AssertEquals(Core.Constants.CurrencyCodes.Australia, taxRecordDataObject.OSTaxCurrency));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_OSTaxCurrency_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_RX_NKOSTaxCurrency = ZString.Empty, (taxRecordDataObject) => AssertEquals(ZString.Empty, taxRecordDataObject.OSTaxCurrency));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxAuthorityServiceCode_ValidValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_TaxAuthorityServiceCode = "TESTCODE1", (taxRecordDataObject) => AssertEquals("TESTCODE1", taxRecordDataObject.TaxAuthorityServiceCode));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxAuthorityServiceCode_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_TaxAuthorityServiceCode = ZString.Empty, (taxRecordDataObject) => AssertEquals(ZString.Empty, taxRecordDataObject.TaxAuthorityServiceCode));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxAuthorityServiceCodeDescription_ValidValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_TaxAuthorityServiceCodeDescription = "TEST DESCRIPTION 1", (taxRecordDataObject) => AssertEquals("TEST DESCRIPTION 1", taxRecordDataObject.TaxAuthorityServiceCodeDescription));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxAuthorityServiceCodeDescription_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_TaxAuthorityServiceCodeDescription = ZString.Empty, (taxRecordDataObject) => AssertEquals(ZString.Empty, taxRecordDataObject.TaxAuthorityServiceCodeDescription));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxDate_ValidValue()
		{
			var taxDate = new ZDateTime(2020, 04, 10).Date;
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_TaxDate = taxDate, (taxRecordDataObject) => AssertEquals(taxDate, taxRecordDataObject.TaxDate));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxDate_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_TaxDate = ZDate.Empty, (taxRecordDataObject) => AssertEquals(ZDate.Empty, taxRecordDataObject.TaxDate));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxSuperType_ValidValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_TaxSuperType = TaxSuperTypeList.TurnoverTax.Code, (taxRecordDataObject) => AssertEquals(TaxSuperTypeList.TurnoverTax.Code, taxRecordDataObject.TaxSuperType));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxSuperType_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_TaxSuperType = ZString.Empty, (taxRecordDataObject) => AssertEquals(ZString.Empty, taxRecordDataObject.TaxSuperType));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxSystemCode_ValidValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_TaxSystemCode = "TAXSYSTEMCODE1", (taxRecordDataObject) => AssertEquals("TAXSYSTEMCODE1", taxRecordDataObject.TaxSystemCode));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TaxSystemCode_EmptyValue()
		{
			AssertTaxRecordDataPropertyValue((taxTransaction) => taxTransaction.ATT_TaxSystemCode = ZString.Empty, (taxRecordDataObject) => AssertEquals(ZString.Empty, taxRecordDataObject.TaxSystemCode));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TransactionLinePKs()
		{
			var taxParentMock = SetupTaxParentMock();

			var taxRecordsAndPivots = CreateTestTaxTransactionsAndPivots(2);

			LinkMockLineToPivot(taxRecordsAndPivots.Pivots[0]);
			LinkMockLineToPivot(taxRecordsAndPivots.Pivots[1]);

			var taxRecordData = TestTaxFrameworkDataTransfer.CreateTaxRecordDataFromTaxTransactions(taxParentMock.Object, taxRecordsAndPivots.TaxRecords, taxRecordsAndPivots.Pivots).ToArray();

			AssertEquals(taxRecordsAndPivots.Pivots[0].ATP_AL_TransactionLine, taxRecordData[0].TransactionLinePKs.ElementAt(0));
			AssertEquals(taxRecordsAndPivots.Pivots[1].ATP_AL_TransactionLine, taxRecordData[1].TransactionLinePKs.ElementAt(0));
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_TransactionLinePKs_Length()
		{
			var taxParentMock = SetupTaxParentMock();

			var taxRecord1 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters());
			var pivot11 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot11.ATP_ATT = taxRecord1.PK;
			LinkMockLineToPivot(pivot11);

			var pivot12 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot12.ATP_ATT = taxRecord1.PK;
			LinkMockLineToPivot(pivot12);

			var pivot13 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot13.ATP_ATT = taxRecord1.PK;
			LinkMockLineToPivot(pivot13);

			var taxRecord2 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters());
			var pivot21 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot21.ATP_ATT = taxRecord2.PK;
			LinkMockLineToPivot(pivot21);

			var taxRecordData = TestTaxFrameworkDataTransfer.CreateTaxRecordDataFromTaxTransactions(taxParentMock.Object, new[] { taxRecord1, taxRecord2 }, new[] { pivot11, pivot12, pivot13, pivot21 }).ToArray();

			AssertEquals(3, taxRecordData[0].TransactionLinePKs.Count);
			AssertEquals(1, taxRecordData[1].TransactionLinePKs.Count);
		}

		void AssertTaxRecordDataPropertyValue(Action<AccTaxTransaction> setup, Action<IReadOnlyTaxRecordData> assert)
		{
			var taxParentMock = SetupTaxParentMock();

			var taxRecordsAndPivots = CreateTestTaxTransactionsAndPivots(1);

			setup(taxRecordsAndPivots.TaxRecords[0]);

			var taxRecordData = TestTaxFrameworkDataTransfer.CreateTaxRecordDataFromTaxTransactions(taxParentMock.Object, taxRecordsAndPivots.TaxRecords, taxRecordsAndPivots.Pivots).ToArray();

			assert(taxRecordData[0]);
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_SystemCalculatedValues()
		{
			var taxParentMock = SetupTaxParentMock();

			var taxRecordsAndPivots = CreateTestTaxTransactionsAndPivots(3);

			Factory.SetContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);
			taxRecordsAndPivots.TaxRecords[0].SetTaxTransactionsSystemCalculatedValues(1000, 50, 5, 100, ZDate.Today, "CODE1", "DESCRIPTION 1");
			taxRecordsAndPivots.TaxRecords[1].SetTaxTransactionsSystemCalculatedValues(500, 100, 10, 50, ZDate.Today.AddDays(-5), "CODE2", null);
			taxRecordsAndPivots.TaxRecords[2].SetTaxTransactionsSystemCalculatedValues(0, 0, 0, 0, ZDate.Empty, null, "DESCRIPTION 3");
			Factory.RemoveContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);

			var taxRecordData = TestTaxFrameworkDataTransfer.CreateTaxRecordDataFromTaxTransactions(taxParentMock.Object, taxRecordsAndPivots.TaxRecords, taxRecordsAndPivots.Pivots).ToArray();

			AssertSystemCalculatedValuesPropertyValues((1000, 50, 5, 100, ZDate.Today, "CODE1", "DESCRIPTION 1"), taxRecordData[0]);
			AssertSystemCalculatedValuesPropertyValues((500, 100, 10, 50, ZDate.Today.AddDays(-5), "CODE2", null), taxRecordData[1]);
			AssertSystemCalculatedValuesPropertyValues((0, 0, 0, 0, ZDate.Empty, null, "DESCRIPTION 3"), taxRecordData[2]);
		}

		void AssertSystemCalculatedValuesPropertyValues((ZDecimal osTaxBaseAmount, ZDecimal osTaxAmount, ZInt rateNumerator, ZInt rateDenominator, ZDate taxDate, ZString taxAuthorityServiceCode, ZString taxAuthorityServiceCodeDescription) expectedValue, IReadOnlyTaxRecordData taxRecordData)
		{
			AssertEquals(expectedValue.osTaxBaseAmount, taxRecordData.SystemCalculatedValues.OSTaxBaseAmount);
			AssertEquals(expectedValue.osTaxAmount, taxRecordData.SystemCalculatedValues.OSTaxAmount);
			AssertEquals(expectedValue.rateNumerator, taxRecordData.SystemCalculatedValues.RateNumerator);
			AssertEquals(expectedValue.rateDenominator, taxRecordData.SystemCalculatedValues.RateDenominator);
			AssertEquals(expectedValue.taxDate, taxRecordData.SystemCalculatedValues.TaxDate);
			AssertEquals(expectedValue.taxAuthorityServiceCode, taxRecordData.SystemCalculatedValues.TaxAuthorityServiceCode);
			AssertEquals(expectedValue.taxAuthorityServiceCodeDescription, taxRecordData.SystemCalculatedValues.TaxAuthorityServiceCodeDescription);
		}

		public void TestCreateTaxRecordDataFromTaxTransactions_DataObjectPropertyHasSameValueAsTaxTransaction_SystemCalculatedValues_WhenNull()
		{
			var taxParentMock = SetupTaxParentMock();

			var taxRecordsAndPivots = CreateTestTaxTransactionsAndPivots(1);

			AssertNull("Pre-condition", taxRecordsAndPivots.TaxRecords[0].GetSystemCalculatedValuesIfAvailable());

			var taxRecordData = TestTaxFrameworkDataTransfer.CreateTaxRecordDataFromTaxTransactions(taxParentMock.Object, taxRecordsAndPivots.TaxRecords, taxRecordsAndPivots.Pivots).ToArray();

			AssertNull(taxRecordData[0].SystemCalculatedValues);
		}

		#endregion

		#region CreateTaxTransactionFromTaxRecordData

		public void TestCreateTaxTransactionFromTaxRecordData_DoesNotCreateTaxTransactions_When_TaxRecordDataObjectsIsNull()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var (taxRecordParent, _) = SetupMinimumValidTestData_For_TestCreateDataObjectsFromTaxTransaction(invoice: invoice);

			TestTaxFrameworkDataTransfer.CreateTaxTransactionFromTaxRecordData(taxRecordParent, null);
			AssertEquals(0, LoadTaxTransactionsBySchemaGuidColumn(AccTaxTransactionSchema.ATT_AH, taxRecordParent.PK).Length);
		}

		public void TestCreateTaxTransactionFromTaxRecordData_IsTaxTransactionsCalculatedBeforePosting_False_When_TaxRecordDataObjectsIsNull()
		{
			var (taxRecordParent, _) = SetupMinimumValidTestData_For_TestCreateDataObjectsFromTaxTransaction(invoice: null);

			TestTaxFrameworkDataTransfer.CreateTaxTransactionFromTaxRecordData(taxRecordParent, null);

			AssertEquals(expected: false, taxRecordParent.IsTaxTransactionsCalculatedBeforePosting);
		}

		public void TestCreateTaxTransactionFromTaxRecordData_DoesNotCreatePivots_When_TaxRecordDataObjectsIsNull()
		{
			var (taxRecordParent, line) = SetupMinimumValidTestData_For_TestCreateDataObjectsFromTaxTransaction(invoice: null);

			TestTaxFrameworkDataTransfer.CreateTaxTransactionFromTaxRecordData(taxRecordParent, null);

			AssertEquals("Number of pivots for the invoice line", 0, LoadPivotsBySchemaGuidColumn(AccTaxRecordTransactionLinePivotSchema.ATP_AL_TransactionLine, line.PK).Length);
		}

		public void TestCreateTaxTransactionFromTaxRecordData_DoesNotCreateTaxTransactions_When_TaxRecordDataObjectsIsEmpty()
		{
			var (taxRecordParent, _) = SetupMinimumValidTestData_For_TestCreateDataObjectsFromTaxTransaction(invoice: null);

			TestTaxFrameworkDataTransfer.CreateTaxTransactionFromTaxRecordData(taxRecordParent, Array.Empty<TaxRecordData>());

			AssertEquals("Number of tax transactions returned", 0, LoadTaxTransactionsBySchemaGuidColumn(AccTaxTransactionSchema.ATT_AH, taxRecordParent.PK).Length);
		}

		public void TestCreateTaxTransactionFromTaxRecordData_IsTaxTransactionsCalculatedBeforePosting_True_When_TaxRecordDataObjectsIsEmpty()
		{
			var (taxRecordParent, _) = SetupMinimumValidTestData_For_TestCreateDataObjectsFromTaxTransaction(invoice: null);

			TestTaxFrameworkDataTransfer.CreateTaxTransactionFromTaxRecordData(taxRecordParent, Array.Empty<TaxRecordData>());

			AssertEquals(expected: true, taxRecordParent.IsTaxTransactionsCalculatedBeforePosting);
		}

		public void TestCreateTaxTransactionFromTaxRecordData_DoesNotCreatePivots_When_TaxRecordDataObjectsIsEmpty()
		{
			var (taxRecordParent, line) = SetupMinimumValidTestData_For_TestCreateDataObjectsFromTaxTransaction(invoice: null);

			TestTaxFrameworkDataTransfer.CreateTaxTransactionFromTaxRecordData(taxRecordParent, Array.Empty<TaxRecordData>());

			AssertEquals("Number of pivots for the invoice line", 0, LoadPivotsBySchemaGuidColumn(AccTaxRecordTransactionLinePivotSchema.ATP_AL_TransactionLine, line.PK).Length);
		}

		public void TestCreateTaxTransactionFromTaxRecordData_IsTaxTransactionsCalculatedBeforePosting_True_When_TaxRecordDataObjectsIsNotEmpty()
		{
			var (taxRecordParent, _) = SetupMinimumValidTestData_For_TestCreateDataObjectsFromTaxTransaction(invoice: null);

			var taxRecordDataMocks = CreateTaxRecordDataMocks();

			SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataMocks);

			TestTaxFrameworkDataTransfer.CreateTaxTransactionFromTaxRecordData(taxRecordParent, GetTaxRecordDataObjectsFromArrayOfMocks(taxRecordDataMocks));

			AssertEquals(expected: true, taxRecordParent.IsTaxTransactionsCalculatedBeforePosting);
		}

		public void TestCreateTaxTransactionFromTaxRecordData_CreatesSameNumberOfTaxTransactions_AsTaxRecordDataObjects()
		{
			var taxRecordsDataMocks = CreateTaxRecordDataMocks(3);

			AssertTaxTransactionPropertyValue(
				taxRecordDataObject => { },
				taxTransactions => AssertEquals("Number of tax transactions for the invoice", 3, taxTransactions.Length),
				taxRecordDataObjectMocks: taxRecordsDataMocks);
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionHasCorrectTransactionHeaderPK()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));

			AssertTaxTransactionPropertyValue(taxRecordDataMock => { }, taxTransactions => AssertEquals(invoice.PK, taxTransactions[0].ATT_AH), invoice);
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_OSTaxCurrency_ValidValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.OSTaxCurrency).Returns(Core.Constants.CurrencyCodes.Fiji), taxTransactions => AssertEquals(Core.Constants.CurrencyCodes.Fiji, taxTransactions[0].ATT_RX_NKOSTaxCurrency));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxConfigurationPK_ValidValue()
		{
			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			Factory.Save();

			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxConfigurationPK).Returns(taxConfig.PK), taxTransactions => AssertEquals(taxConfig.PK, taxTransactions[0].ATT_ETC));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_BranchPK_ValidValue()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.BranchPK).Returns(branch.PK), taxTransactions => AssertEquals(branch.PK, taxTransactions[0].ATT_GB));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionHasCorrectCompany_When_BranchPKIsValid()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			Factory.Save();

			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.BranchPK).Returns(branch.PK), taxTransactions => AssertEquals(company.PK, taxTransactions[0].ATT_GC));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionHasCorrectCompany_When_BranchPKIsEmpty()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.BranchPK).Returns(ZGuid.Empty), taxTransactions => AssertEquals(ZGuid.Empty, taxTransactions[0].ATT_GC));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_DepartmentPK_ValidValue()
		{
			var departmentPK = ZGuid.NewZGuid();

			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.DepartmentPK).Returns(departmentPK), taxTransactions => AssertEquals(departmentPK, taxTransactions[0].ATT_GE_Department));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_DepartmentPK_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.DepartmentPK).Returns(ZGuid.Empty), taxTransactions => AssertEquals(ZGuid.Empty, taxTransactions[0].ATT_GE_Department));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_OSTaxBaseAmount()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.OSTaxBaseAmount).Returns(444m), taxTransactions => AssertEquals(444m, taxTransactions[0].ATT_OSTaxBaseAmount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_SettingOSTaxBaseAmount_SetsExactValue()
		{
			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			Factory.Save();
			AssertTaxTransactionPropertyValue(
				taxRecordDataMock =>
				{
					taxRecordDataMock.Setup(x => x.TaxConfigurationPK).Returns(taxConfig.PK);
					taxRecordDataMock.Setup(x => x.OSTaxBaseAmount).Returns(444.1236m);
				},
				taxTransactions => AssertEquals(444.1236m, taxTransactions[0].ATT_OSTaxBaseAmount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_LocalTaxBaseAmount()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.LocalTaxBaseAmount).Returns(222m), taxTransactions => AssertEquals(222m, taxTransactions[0].ATT_LocalTaxBaseAmount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_SettingLocalTaxBaseAmount_SetsExactValue()
		{
			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			Factory.Save();
			AssertTaxTransactionPropertyValue(
				taxRecordDataMock =>
				{
					taxRecordDataMock.Setup(x => x.TaxConfigurationPK).Returns(taxConfig.PK);
					taxRecordDataMock.Setup(x => x.LocalTaxBaseAmount).Returns(222.9874m);
				},
				taxTransactions => AssertEquals(222.9874m, taxTransactions[0].ATT_LocalTaxBaseAmount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_OSTaxAmount()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.OSTaxAmount).Returns(333m), taxTransactions => AssertEquals(333m, taxTransactions[0].ATT_OSTaxAmount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_SettingOSTaxAmount_SetsExactValue()
		{
			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			Factory.Save();
			AssertTaxTransactionPropertyValue(
				taxRecordDataMock =>
				{
					taxRecordDataMock.Setup(x => x.TaxConfigurationPK).Returns(taxConfig.PK);
					taxRecordDataMock.Setup(x => x.OSTaxAmount).Returns(333.6548m);
				},
				taxTransactions => AssertEquals(333.6548m, taxTransactions[0].ATT_OSTaxAmount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_LocalTaxAmount()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.LocalTaxAmount).Returns(111m), taxTransactions => AssertEquals(111m, taxTransactions[0].ATT_LocalTaxAmount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_SettingAmounts_SetsExactValueWithoutRecalculationOfLocalAmounts()
		{
			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			Factory.Save();
			AssertTaxTransactionPropertyValue(
				taxRecordDataMock =>
				{
					taxRecordDataMock.Setup(x => x.TaxConfigurationPK).Returns(taxConfig.PK);
					taxRecordDataMock.Setup(x => x.LocalTaxAmount).Returns(111.6543m);
				},
				taxTransactions => AssertEquals(111.6543m, taxTransactions[0].ATT_LocalTaxAmount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_SettingLocalTaxAmount_SetsExactValue()
		{
			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			Factory.Save();
			AssertTaxTransactionPropertyValue(
				taxRecordDataMock =>
				{
					taxRecordDataMock.Setup(x => x.TaxConfigurationPK).Returns(taxConfig.PK);

					taxRecordDataMock.Setup(x => x.OSTaxBaseAmount).Returns(101m);
					taxRecordDataMock.Setup(x => x.LocalTaxBaseAmount).Returns(211m);
					taxRecordDataMock.Setup(x => x.OSTaxAmount).Returns(20m);
					taxRecordDataMock.Setup(x => x.LocalTaxAmount).Returns(35m);
				},
				taxTransactions =>
				{
					CombineAssertions(() =>
					{
						AssertEquals(101m, taxTransactions[0].ATT_OSTaxBaseAmount);
						AssertEquals(211m, taxTransactions[0].ATT_LocalTaxBaseAmount);
						AssertEquals(20m, taxTransactions[0].ATT_OSTaxAmount);
						AssertEquals(35m, taxTransactions[0].ATT_LocalTaxAmount);
					});
				});
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_Ledger_ValidValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.Ledger).Returns(LedgerTypes.AccountsPayable), taxTransactions => AssertEquals(LedgerTypes.AccountsPayable, taxTransactions[0].ATT_Ledger));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_Ledger_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.Ledger).Returns(ZString.Empty), taxTransactions => AssertEquals(ZString.Empty, taxTransactions[0].ATT_Ledger));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_PostDate_ValidValue()
		{
			var postDate = new ZDateTime(2020, 04, 10).Date;
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.PostDate).Returns(postDate), taxTransactions => AssertEquals(postDate, taxTransactions[0].ATT_PostDate));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_PostDate_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.PostDate).Returns(ZDate.Empty), taxTransactions => AssertEquals(ZDate.Empty, taxTransactions[0].ATT_PostDate));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_RateDenominator()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.RateDenominator).Returns(9999), taxTransactions => AssertEquals(9999, taxTransactions[0].ATT_RateDenominator));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_RateNumerator()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.RateNumerator).Returns(125), taxTransactions => AssertEquals(125, taxTransactions[0].ATT_RateNumerator));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxMessagePK_ValidValue()
		{
			var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			taxMessage.A9_Code = "TESTCODE";
			Factory.Save();

			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxMessagePK).Returns(taxMessage.PK), taxTransactions => AssertEquals(taxMessage.PK, taxTransactions[0].ATT_A9_TaxMessage));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxMessagePK_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxMessagePK).Returns(ZGuid.Empty), taxTransactions => AssertEquals(ZGuid.Empty, taxTransactions[0].ATT_A9_TaxMessage));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_AffectsSourceTransactionTotal_false()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.AffectsSourceTransactionTotal).Returns(false), taxTransactions => AssertEquals(false, taxTransactions[0].ATT_AffectsSourceTransactionTotal));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_AffectsSourceTransactionTotal_true()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.AffectsSourceTransactionTotal).Returns(true), taxTransactions => AssertEquals(true, taxTransactions[0].ATT_AffectsSourceTransactionTotal));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_LedgerControlGLAccountPK_ValidValue()
		{
			var ledgerCountrolAccount = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();

			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.LedgerControlGLAccountPK).Returns(ledgerCountrolAccount.PK), taxTransactions => AssertEquals(ledgerCountrolAccount.PK, taxTransactions[0].ATT_AG_LedgerControlAccount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_LedgerControlGLAccountPK_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.LedgerControlGLAccountPK).Returns(ZGuid.Empty), taxTransactions => AssertEquals(ZGuid.Empty, taxTransactions[0].ATT_AG_LedgerControlAccount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxControlGLAccountPK_ValidValue()
		{
			var taxCountrolAccount = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();

			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxControlGLAccountPK).Returns(taxCountrolAccount.PK), taxTransactions => AssertEquals(taxCountrolAccount.PK, taxTransactions[0].ATT_AG_TaxControlAccount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxControlGLAccountPK_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxControlGLAccountPK).Returns(ZGuid.Empty), taxTransactions => AssertEquals(ZGuid.Empty, taxTransactions[0].ATT_AG_TaxControlAccount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxExpenseGLAccountPK_ValidValue()
		{
			var taxExpenseAccount = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();

			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxExpenseGLAccountPK).Returns(taxExpenseAccount.PK), taxTransactions => AssertEquals(taxExpenseAccount.PK, taxTransactions[0].ATT_AG_TaxExpenseAccount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxExpenseGLAccountPK_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxExpenseGLAccountPK).Returns(ZGuid.Empty), taxTransactions => AssertEquals(ZGuid.Empty, taxTransactions[0].ATT_AG_TaxExpenseAccount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxPendingControlGLAccountPK_ValidValue()
		{
			var taxPendingControlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();

			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxPendingControlGLAccountPK).Returns(taxPendingControlAccount.PK), taxTransactions => AssertEquals(taxPendingControlAccount.PK, taxTransactions[0].ATT_AG_TaxPendingControlAccount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxPendingControlGLAccountPK_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxPendingControlGLAccountPK).Returns(ZGuid.Empty), taxTransactions => AssertEquals(ZGuid.Empty, taxTransactions[0].ATT_AG_TaxPendingControlAccount));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxBasis_ValidValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxBasis).Returns(TaxBasisList.PostingOnMatching.Code), taxTransactions => AssertEquals(TaxBasisList.PostingOnMatching.Code, taxTransactions[0].ATT_Basis));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxBasis_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxBasis).Returns(ZString.Empty), taxTransactions => AssertEquals(ZString.Empty, taxTransactions[0].ATT_Basis));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_RealisationDate_ValidValue()
		{
			var postDate = new ZDateTime(2020, 04, 10).Date;
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.RealisationDate).Returns(postDate), taxTransactions => AssertEquals(postDate, taxTransactions[0].ATT_RealisationDate));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_RealisationDate_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.RealisationDate).Returns(ZDate.Empty), taxTransactions => AssertEquals(ZDate.Empty, taxTransactions[0].ATT_RealisationDate));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxIDPK_ValidValue()
		{
			var taxIDPK = ZGuid.NewZGuid();

			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxIDPK).Returns(taxIDPK), taxTransactions => AssertEquals(taxIDPK, taxTransactions[0].ATT_AT_TaxID));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TAXIDPK_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxIDPK).Returns(ZGuid.Empty), taxTransactions => AssertEquals(ZGuid.Empty, taxTransactions[0].ATT_AT_TaxID));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxDate_ValidValue()
		{
			var taxDate = new ZDateTime(2020, 04, 10).Date;

			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxDate).Returns(taxDate), taxTransactions => AssertEquals(taxDate, taxTransactions[0].ATT_TaxDate));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxDate_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxDate).Returns(ZDate.Empty), taxTransactions => AssertEquals(ZDate.Empty, taxTransactions[0].ATT_TaxDate));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxSuperType_ValidValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxSuperType).Returns(TaxSuperTypeList.TurnoverTax.Code), taxTransactions => AssertEquals(TaxSuperTypeList.TurnoverTax.Code, taxTransactions[0].ATT_TaxSuperType));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxSuperType_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxSuperType).Returns(ZString.Empty), taxTransactions => AssertEquals(ZString.Empty, taxTransactions[0].ATT_TaxSuperType));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxSystemCode_ValidValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxSystemCode).Returns("TAXSYSTEMCODE1"), taxTransactions => AssertEquals("TAXSYSTEMCODE1", taxTransactions[0].ATT_TaxSystemCode));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxSystemCode_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxSystemCode).Returns(ZString.Empty), taxTransactions => AssertEquals(ZString.Empty, taxTransactions[0].ATT_TaxSystemCode));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxAuthorityServiceCode_ValidValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxAuthorityServiceCode).Returns("TESTCODE1"), taxTransactions => AssertEquals("TESTCODE1", taxTransactions[0].ATT_TaxAuthorityServiceCode));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxAuthorityServiceCode_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxAuthorityServiceCode).Returns(ZString.Empty), taxTransactions => AssertEquals(ZString.Empty, taxTransactions[0].ATT_TaxAuthorityServiceCode));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxAuthorityServiceCodeDescription_ValidValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxAuthorityServiceCodeDescription).Returns("TEST DESCRIPTION 1"), taxTransactions => AssertEquals("TEST DESCRIPTION 1", taxTransactions[0].ATT_TaxAuthorityServiceCodeDescription));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_TaxAuthorityServiceCodeDescription_EmptyValue()
		{
			AssertTaxTransactionPropertyValue(taxRecordDataMock => taxRecordDataMock.Setup(x => x.TaxAuthorityServiceCodeDescription).Returns(ZString.Empty), taxTransactions => AssertEquals(ZString.Empty, taxTransactions[0].ATT_TaxAuthorityServiceCodeDescription));
		}

		void AssertTaxTransactionPropertyValue(Action<Mock<IReadOnlyTaxRecordData>> testCaseSpecificSetup, Action<AccTaxTransaction[]> testCaseSpecificAssert, InvoicingBase invoice = null, Mock<IReadOnlyTaxRecordData>[] taxRecordDataObjectMocks = null)
		{
			var (taxRecordParent, _) = SetupMinimumValidTestData_For_TestCreateDataObjectsFromTaxTransaction(invoice);

			taxRecordDataObjectMocks = taxRecordDataObjectMocks ?? CreateTaxRecordDataMocks();
			SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataObjectMocks);

			testCaseSpecificSetup(taxRecordDataObjectMocks[0]);

			TestTaxFrameworkDataTransfer.CreateTaxTransactionFromTaxRecordData(taxRecordParent, GetTaxRecordDataObjectsFromArrayOfMocks(taxRecordDataObjectMocks));

			testCaseSpecificAssert(LoadTaxTransactionsBySchemaGuidColumn(AccTaxTransactionSchema.ATT_AH, taxRecordParent.PK));
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_SystemCalculatedValues()
		{
			var (taxRecordParent, _) = SetupMinimumValidTestData_For_TestCreateDataObjectsFromTaxTransaction();

			var expectedSystemCalculatedValues1 = new TaxRecordDataSystemCalculatedValues(1000, 50, 5, 100, ZDate.Today, "CODE1", "DESCRIPTION 1");
			var expectedSystemCalculatedValues2 = new TaxRecordDataSystemCalculatedValues(500, 100, 10, 50, ZDate.Today.AddDays(-5), "CODE2", null);
			var expectedSystemCalculatedValues3 = new TaxRecordDataSystemCalculatedValues(0, 0, 0, 0, ZDate.Empty, null, "DESCRIPTION 3");

			var taxRecordDataObjectMocks = CreateTaxRecordDataMocks(3);
			SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataObjectMocks);
			taxRecordDataObjectMocks[0].Setup(x => x.SystemCalculatedValues).Returns(expectedSystemCalculatedValues1);
			taxRecordDataObjectMocks[1].Setup(x => x.SystemCalculatedValues).Returns(expectedSystemCalculatedValues2);
			taxRecordDataObjectMocks[2].Setup(x => x.SystemCalculatedValues).Returns(expectedSystemCalculatedValues3);

			TestTaxFrameworkDataTransfer.CreateTaxTransactionFromTaxRecordData(taxRecordParent, GetTaxRecordDataObjectsFromArrayOfMocks(taxRecordDataObjectMocks));

			var taxTransactions = LoadTaxTransactionsBySchemaGuidColumn(AccTaxTransactionSchema.ATT_AH, taxRecordParent.PK);
			AssertTaxTransactionSystemCalculatedValuesPropertyValues((1000, 50, 5, 100, ZDate.Today, "CODE1", "DESCRIPTION 1"), taxTransactions[0].GetSystemCalculatedValuesIfAvailable());
			AssertTaxTransactionSystemCalculatedValuesPropertyValues((500, 100, 10, 50, ZDate.Today.AddDays(-5), "CODE2", null), taxTransactions[1].GetSystemCalculatedValuesIfAvailable());
			AssertTaxTransactionSystemCalculatedValuesPropertyValues((0, 0, 0, 0, ZDate.Empty, null, "DESCRIPTION 3"), taxTransactions[2].GetSystemCalculatedValuesIfAvailable());
		}

		void AssertTaxTransactionSystemCalculatedValuesPropertyValues((ZDecimal osTaxBaseAmount, ZDecimal osTaxAmount, ZInt rateNumerator, ZInt rateDenominator, ZDate taxDate, ZString taxAuthorityServiceCode, ZString taxAuthorityServiceCodeDescription) expectedValue, IReadOnlyTaxTransactionSystemCalculatedValues taxTransactionSystemCalculatedValues)
		{
			AssertEquals(expectedValue.osTaxBaseAmount, taxTransactionSystemCalculatedValues.OSTaxBaseAmount);
			AssertEquals(expectedValue.osTaxAmount, taxTransactionSystemCalculatedValues.OSTaxAmount);
			AssertEquals(expectedValue.rateNumerator, taxTransactionSystemCalculatedValues.RateNumerator);
			AssertEquals(expectedValue.rateDenominator, taxTransactionSystemCalculatedValues.RateDenominator);
			AssertEquals(expectedValue.taxDate, taxTransactionSystemCalculatedValues.TaxDate);
			AssertEquals(expectedValue.taxAuthorityServiceCode, taxTransactionSystemCalculatedValues.TaxAuthorityServiceCode);
			AssertEquals(expectedValue.taxAuthorityServiceCodeDescription, taxTransactionSystemCalculatedValues.TaxAuthorityServiceCodeDescription);
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionPropertyHasSameValueAsDataObject_SystemCalculatedValues_WhenNull()
		{
			var (taxRecordParent, _) = SetupMinimumValidTestData_For_TestCreateDataObjectsFromTaxTransaction();

			var taxRecordDataObjectMocks = CreateTaxRecordDataMocks();
			SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataObjectMocks);

			var taxRecordDataObjects = GetTaxRecordDataObjectsFromArrayOfMocks(taxRecordDataObjectMocks);
			AssertNull("Pre-condition", taxRecordDataObjects[0].SystemCalculatedValues);

			TestTaxFrameworkDataTransfer.CreateTaxTransactionFromTaxRecordData(taxRecordParent, taxRecordDataObjects);

			var taxTransactions = LoadTaxTransactionsBySchemaGuidColumn(AccTaxTransactionSchema.ATT_AH, taxRecordParent.PK);
			AssertNull(taxTransactions[0].GetSystemCalculatedValuesIfAvailable());
		}

		public void TestCreateTaxTransactionFromTaxRecordData_CreatesCorrectNumberOfPivotsForEachLine()
		{
			AssertPivots_TestCreateDataObjectsFromTaxTransaction(
				(taxRecordDataMocks, lines) =>
				{
					SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataMocks);
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[0], new[] { lines[0].PK, lines[1].PK });
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[1], new[] { lines[1].PK });
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[2], new[] { lines[1].PK });
				},
				(taxTransactions, lines) =>
				{
					CombineAssertions(() =>
					{
						AssertEquals("Number of pivots linked to line1", 1, LoadPivotsBySchemaGuidColumn(AccTaxRecordTransactionLinePivotSchema.ATP_AL_TransactionLine, lines[0].PK).Length);
						AssertEquals("Number of pivots linked to line2", 3, LoadPivotsBySchemaGuidColumn(AccTaxRecordTransactionLinePivotSchema.ATP_AL_TransactionLine, lines[1].PK).Length);
					});
				});
		}

		public void TestCreateTaxTransactionFromTaxRecordData_CreatesCorrectNumberOfPivotsForEachTaxTransaction()
		{
			AssertPivots_TestCreateDataObjectsFromTaxTransaction(
				(taxRecordDataMocks, lines) =>
				{
					SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataMocks);
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[0], new[] { lines[0].PK, lines[1].PK });
				},
				(taxTransactions, _) =>
				{
					AssertEquals("Number of pivots linked to tax transaction 1", 2, LoadPivotsBySchemaGuidColumn(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransactions[0].PK).Length);
				});

			AssertPivots_TestCreateDataObjectsFromTaxTransaction(
				(taxRecordDataMocks, lines) =>
				{
					SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataMocks);
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[2], new[] { lines[1].PK });
				},
				(taxTransactions, _) =>
				{
					AssertEquals("Number of pivots linked to tax transaction 2", 1, LoadPivotsBySchemaGuidColumn(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransactions[2].PK).Length);
				});
		}

		[ExpectNoExceptions]
		public void TestCreateTaxTransactionFromTaxRecordData_Invokes_TaxRecordPivotProcessor_CreateMethod()
		{
			var taxRecordPivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();

			AssertPivots_TestCreateDataObjectsFromTaxTransaction(
				(taxRecordDataMocks, lines) =>
				{
					SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataMocks);
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[0], new[] { lines[0].PK, lines[1].PK });
				},
				(taxTransactions, _) =>
				{
					taxRecordPivotProcessorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLineBase>()));
				},
				taxRecordPivotProcessorMock.Object);
		}

		[ExpectNoExceptions]
		public void TestCreateTaxTransactionFromTaxRecordData_Invokes_TaxRecordPivotProcessor_CreateMethod_WithTaxTransactionArgument()
		{
			var taxRecordPivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();

			AssertPivots_TestCreateDataObjectsFromTaxTransaction(
				(taxRecordDataMocks, lines) =>
				{
					SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataMocks);
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[0], new[] { lines[0].PK, lines[1].PK });
				},
				(taxTransactions, _) =>
				{
					taxRecordPivotProcessorMock.Verify(x => x.Create(taxTransactions[0], It.IsAny<ITaxableTransactionLineBase>()));
				},
				taxRecordPivotProcessorMock.Object);
		}

		[ExpectNoExceptions]
		public void TestCreateTaxTransactionFromTaxRecordData_Invokes_TaxRecordPivotProcessor_CreateMethod_WithTaxableTransactionLineArgument()
		{
			var taxRecordPivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();

			AssertPivots_TestCreateDataObjectsFromTaxTransaction(
				(taxRecordDataMocks, lines) =>
				{
					SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataMocks);
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[0], new[] { lines[0].PK });
				},
				(_, lines) =>
				{
					taxRecordPivotProcessorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(lines[0])));
				},
				taxRecordPivotProcessorMock.Object);
		}

		[ExpectNoExceptions]
		public void TestCreateTaxTransactionFromTaxRecordData_Invokes_TaxRecordPivotProcessor_CreateMethod_CorrectNumberOfTimes()
		{
			var taxRecordPivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();

			AssertPivots_TestCreateDataObjectsFromTaxTransaction(
				(taxRecordDataMocks, lines) =>
				{
					SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataMocks);
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[0], new[] { lines[0].PK, lines[1].PK });
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[1], new[] { lines[1].PK });
				},
				(taxTransactions, _) =>
				{
					taxRecordPivotProcessorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), It.IsAny<ITaxableTransactionLineBase>()), Times.Exactly(3));
				},
				taxRecordPivotProcessorMock.Object);
		}

		[ExpectNoExceptions]
		public void TestCreateTaxTransactionFromTaxRecordData_Invokes_TaxRecordPivotProcessor_CreateMethod_CorrectNumberOfTimesPerTaxTransaction()
		{
			var taxRecordPivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();

			AssertPivots_TestCreateDataObjectsFromTaxTransaction(
				(taxRecordDataMocks, lines) =>
				{
					SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataMocks);
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[0], new[] { lines[0].PK, lines[1].PK });
				},
				(taxTransactions, _) =>
				{
					taxRecordPivotProcessorMock.Verify(x => x.Create(taxTransactions[0], It.IsAny<ITaxableTransactionLineBase>()), Times.Exactly(2));
				},
				taxRecordPivotProcessorMock.Object);

			AssertPivots_TestCreateDataObjectsFromTaxTransaction(
				(taxRecordDataMocks, lines) =>
				{
					SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataMocks);
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[1], new[] { lines[1].PK });
				},
				(taxTransactions, _) =>
				{
					taxRecordPivotProcessorMock.Verify(x => x.Create(taxTransactions[1], It.IsAny<ITaxableTransactionLineBase>()), Times.Exactly(1));
				},
				taxRecordPivotProcessorMock.Object);
		}

		[ExpectNoExceptions]
		public void TestCreateTaxTransactionFromTaxRecordData_Invokes_TaxRecordPivotProcessor_CreateMethod_CorrectNumberOfTimesPerLine()
		{
			var taxRecordPivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();

			AssertPivots_TestCreateDataObjectsFromTaxTransaction(
				(taxRecordDataMocks, lines) =>
				{
					SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataMocks);
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[0], new[] { lines[0].PK, lines[1].PK });
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[1], new[] { lines[1].PK });
				},
				(_, lines) =>
				{
					CombineAssertions(() =>
					{
						taxRecordPivotProcessorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(lines[0])), Times.Exactly(1));
						taxRecordPivotProcessorMock.Verify(x => x.Create(It.IsAny<AccTaxTransaction>(), TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(lines[1])), Times.Exactly(2));
					});
				},
				taxRecordPivotProcessorMock.Object);
		}

		void SetupTransactionLinePKsPropertyOnTaxRecordDataMock(Mock<IReadOnlyTaxRecordData> taxRecordDataMock, IReadOnlyCollection<ZGuid> linePKs)
		{
			taxRecordDataMock.Setup(x => x.TransactionLinePKs).Returns(linePKs);
		}

		void AssertPivots_TestCreateDataObjectsFromTaxTransaction(Action<Mock<IReadOnlyTaxRecordData>[], InvoicingLineBase[]> testCaseSpecificSetup, Action<AccTaxTransaction[], InvoicingLineBase[]> testCaseSpecificAssert, ITaxRecordPivotProcessor taxRecordPivotProcessor = null)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var lines = new InvoicingLineBase[2];
			lines[0] = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.CC1.PK, 100);
			lines[1] = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.CC2.PK, 200);
			ITaxRecordParent taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);

			var taxRecordDataMocks = CreateTaxRecordDataMocks(3);

			testCaseSpecificSetup(taxRecordDataMocks, lines);

			var taxFrameworkDataTransfer = (taxRecordPivotProcessor != null) ? new TaxFrameworkDataTransfer(taxRecordPivotProcessor) : new TaxFrameworkDataTransfer(new TaxRecordPivotProcessor());

			taxFrameworkDataTransfer.CreateTaxTransactionFromTaxRecordData(taxRecordParent, GetTaxRecordDataObjectsFromArrayOfMocks(taxRecordDataMocks));

			testCaseSpecificAssert(LoadTaxTransactionsBySchemaGuidColumn(AccTaxTransactionSchema.ATT_AH, taxRecordParent.PK), lines);
		}

		public void TestCreateTaxTransactionFromTaxRecordData_TaxTransactionHasRowError_WhenLineToLinkWithNotFound_WhileCreatingPivot()
		{
			AssertPivots_TestCreateDataObjectsFromTaxTransaction(
				(taxRecordDataMocks, lines) =>
				{
					lines[0].AL_AC = ZGuid.Empty;
					SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(taxRecordDataMocks);
					SetupTransactionLinePKsPropertyOnTaxRecordDataMock(taxRecordDataMocks[0], new[] { lines[0].PK });
				},
				(taxTransactions, _) =>
				{
					AssertEquals(expected: true, taxTransactions[0].HasRowErrors);
					AssertEquals("Row Error Message", "Error when restoring tax transaction because the associated line was not found. Please try recalculating tax transactions.", taxTransactions[0].RowErrors.First().Message);
				});
		}

		Mock<IReadOnlyTaxRecordData>[] CreateTaxRecordDataMocks(int numberOfTaxRecordsData = 1)
		{
			var mocks = new List<Mock<IReadOnlyTaxRecordData>>();

			for (int i = 0; i < numberOfTaxRecordsData; i++)
			{
				mocks.Add(new Mock<IReadOnlyTaxRecordData>());
			}

			return mocks.ToArray();
		}

		void SetupPropertiesOnTaxRecordataMocksNecessaryForPivotCreation(Mock<IReadOnlyTaxRecordData>[] taxRecordDataObjectMocks)
		{
			if (taxRecordDataObjectMocks == null)
			{
				return;
			}

			foreach (var taxRecordDataObjectMock in taxRecordDataObjectMocks)
			{
				taxRecordDataObjectMock.Setup(x => x.BranchPK).Returns(GlbBranch.CurrentBranch.PK);
				taxRecordDataObjectMock.Setup(x => x.OSTaxCurrency).Returns(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			}
		}

		(ITaxRecordParent taxRecordParent, InvoicingLineBase line) SetupMinimumValidTestData_For_TestCreateDataObjectsFromTaxTransaction(InvoicingBase invoice = null)
		{
			if (invoice == null)
			{
				invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			}

			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.CC1.PK, 100);
			ITaxRecordParent taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);

			return (taxRecordParent, line);
		}

		IReadOnlyTaxRecordData[] GetTaxRecordDataObjectsFromArrayOfMocks(Mock<IReadOnlyTaxRecordData>[] mocks)
		{
			return mocks.Select(x => x.Object).ToArray();
		}

		AccTaxTransaction[] LoadTaxTransactionsBySchemaGuidColumn(SchemaColumn column, object value)
		{
			return Factory.Load<AccTaxTransaction>(new ZQuery(column, value));
		}

		AccTaxRecordTransactionLinePivot[] LoadPivotsBySchemaGuidColumn(SchemaColumn column, object value)
		{
			return Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(column, value));
		}

		#endregion

		Mock<ITaxRecordParent> SetupTaxParentMock(bool isTaxTransctionsCalculatedBeforePosting = true)
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupProperty(x => x.IsTaxTransactionsCalculatedBeforePosting, isTaxTransctionsCalculatedBeforePosting);

			return taxParentMock;
		}

		(AccTaxTransaction[] TaxRecords, AccTaxRecordTransactionLinePivot[] Pivots) CreateTestTaxTransactionsAndPivots(int numberOfTaxRecords)
		{
			var taxRecords = new AccTaxTransaction[numberOfTaxRecords];
			var pivots = new AccTaxRecordTransactionLinePivot[numberOfTaxRecords];

			for (int i = 0; i < numberOfTaxRecords; i++)
			{
				taxRecords[i] = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters());

				pivots[i] = Factory.New<AccTaxRecordTransactionLinePivot>();
				pivots[i].ATP_ATT = taxRecords[i].PK;
			}

			return (taxRecords, pivots);
		}

		void LinkMockLineToPivot(AccTaxRecordTransactionLinePivot pivot)
		{
			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(ZGuid.NewZGuid());
			pivot.LinkLine(lineMock.Object);
		}

		TaxFrameworkDataTransfer TestTaxFrameworkDataTransfer => new TaxFrameworkDataTransfer(new Mock<ITaxRecordPivotProcessor>().Object);

		TaxFrameworkTestObjectCreator TaxFrameworkObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
