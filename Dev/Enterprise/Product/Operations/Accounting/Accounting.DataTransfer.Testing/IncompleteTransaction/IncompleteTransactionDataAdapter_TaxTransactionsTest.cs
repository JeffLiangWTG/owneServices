using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	sealed class IncompleteTransactionDataAdapter_TaxTransactionsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSaveAsIncomplete_Invokes_GetTaxRecordDataForDataTransfer()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();
			var line = (APInvoiceLine)invoice.Lines.AddNew();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			invoice.SaveAsIncomplete();

			taxProcessorMock.Verify(x => x.GetTaxRecordDataForDataTransfer(It.IsAny<ITaxRecordParent>()));
		}

		[ExpectNoExceptions]
		public void TestSaveAsIncomplete_Invokes_GetTaxRecordDataForDataTransfer_WithCorrectTaxParentArgument()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();
			var line = (APInvoiceLine)invoice.Lines.AddNew();

			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			invoice.SaveAsIncomplete();

			taxProcessorMock.Verify(x => x.GetTaxRecordDataForDataTransfer(taxRecordParent));
		}

		[ExpectNoExceptions]
		public void TestRestore_Invokes_RestoreFromTaxRecordData()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			SaveAndRestoreInvoice(invoice);

			taxProcessorMock.Verify(x => x.RestoreFromTaxRecordData(It.IsAny<ITaxRecordParent>(), It.IsAny<IReadOnlyCollection<IReadOnlyTaxRecordData>>()));
		}

		[ExpectNoExceptions]
		public void TestRestore_Invokes_RestoreFromTaxRecordData_WithCorrectTaxParentArgument()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			var restoredIncompleteInvoice = SaveAndRestoreInvoice(invoice);

			var restoredTaxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(restoredIncompleteInvoice);
			taxProcessorMock.Verify(x => x.RestoreFromTaxRecordData(restoredTaxRecordParent, It.IsAny<IReadOnlyCollection<IReadOnlyTaxRecordData>>()));
		}

		[ExpectNoExceptions]
		public void TestRestoreEmptyTaxRecordDataObjectList_When_ReceivedTaxRecordDataObjectListEmpty()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			taxProcessorMock.Setup(x => x.GetTaxRecordDataForDataTransfer(It.IsAny<ITaxRecordParent>())).Returns(Array.Empty<IReadOnlyTaxRecordData>());

			SaveAndRestoreInvoice(invoice);

			taxProcessorMock.Verify(x => x.RestoreFromTaxRecordData(It.IsAny<ITaxRecordParent>(), Array.Empty<IReadOnlyTaxRecordData>()));
		}

		[ExpectNoExceptions]
		public void TestRestoreNullTaxRecordDataObjectList_When_ReceivedTaxRecordDataObjectListNull()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			taxProcessorMock.Setup(x => x.GetTaxRecordDataForDataTransfer(It.IsAny<ITaxRecordParent>())).Returns((IReadOnlyCollection<IReadOnlyTaxRecordData>)null);

			SaveAndRestoreInvoice(invoice);

			taxProcessorMock.Verify(x => x.RestoreFromTaxRecordData(It.IsAny<ITaxRecordParent>(), null));
		}

		public void TestRestore_ThrowsException_When_TaxFrameworkThrowsException()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			taxProcessorMock.Setup(x => x.GetTaxRecordDataForDataTransfer(It.IsAny<ITaxRecordParent>())).Returns(Array.Empty<IReadOnlyTaxRecordData>());
			taxProcessorMock.Setup(x => x.RestoreFromTaxRecordData(It.IsAny<ITaxRecordParent>(), It.IsAny<IReadOnlyCollection<IReadOnlyTaxRecordData>>())).Throws(() => new InvalidOperationException("Invalid operation"));
			invoice.SaveAsIncomplete();

			var incompleteInvoice = new BusinessObjectFactory().Load<APInvoice>(invoice.PK);
			AssertExceptionThrown<InvalidOperationException>("Invalid operation", () => incompleteInvoice.RestoreSavedData());
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxMessagePK_ValidValue()
		{
			var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			taxMessage.A9_Code = "TESTCODE";
			Factory.Save();

			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxMessagePK = taxMessage.PK, restoredDataObject => AssertEquals(taxMessage.PK, restoredDataObject.TaxMessagePK));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxMessagePK_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxMessagePK = ZGuid.Empty, restoredDataObject => AssertEquals(ZGuid.Empty, restoredDataObject.TaxMessagePK));
		}

		public void TestSaveRestoreTaxTransaction_TaxMessageCodeIsEmpty_When_TaxMessageBizoNotInDB()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxMessagePK = ZGuid.NewZGuid(), restoredDataObject => AssertEquals(ZGuid.Empty, restoredDataObject.TaxMessagePK));
		}

		public void TestRestoreTaxTransaction_ErrorInContext_When_TaxMessageCodeNotInDB()
		{
			var taxMessage = TestObjectCreator.CreateTaxMsg("TESTCODE", "Test Description", "", "");
			Factory.Save();

			AssertRestoreTaxTransaction_ErrorInContext(
				dataObject => dataObject.TaxMessagePK = taxMessage.PK,
				errorMsg => { AssertEquals("Could not find Tax Message TESTCODE", errorMsg); },
				suppliedFactory =>
				{
					var taxMsgInNewFactory = suppliedFactory.Load<AccInvMsg>(taxMessage.PK);
					taxMsgInNewFactory.A9_Code = "NEWCODE";
					suppliedFactory.Save();
				});
		}

		public void TestRestoreSameTaxRecordDataForProperty_AffectsSourceTransactionTotal_false()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.AffectsSourceTransactionTotal = false, restoredDataObject => AssertEquals(false, restoredDataObject.AffectsSourceTransactionTotal));
		}

		public void TestRestoreSameTaxRecordDataForProperty_AffectsSourceTransactionTotal_true()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.AffectsSourceTransactionTotal = true, restoredDataObject => AssertEquals(true, restoredDataObject.AffectsSourceTransactionTotal));
		}

		public void TestRestoreSameTaxRecordDataForProperty_LedgerControlGLAccountPK_ValidValue()
		{
			var ledgerControlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();

			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.LedgerControlGLAccountPK = ledgerControlAccount.PK, restoredDataObject => AssertEquals(ledgerControlAccount.PK, restoredDataObject.LedgerControlGLAccountPK));
		}

		public void TestRestoreSameTaxRecordDataForProperty_LedgerControlGLAccountPK_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.LedgerControlGLAccountPK = ZGuid.Empty, restoredDataObject => AssertEquals(ZGuid.Empty, restoredDataObject.LedgerControlGLAccountPK));
		}

		public void TestRestoreTaxTransaction_ErrorInContext_When_LedgerControlGLAccountBizoNotInDB()
		{
			var (ledgerControlAccountPK, taxConfigPK, branchPK) = Create_BizOs_For_ErrorInContext_Tests();

			AssertRestoreTaxTransaction_ErrorInContext(
				dataObject =>
				{
					Setup_TaxRecordData_For_ErrorInContextTests(dataObject, branchPK, taxConfigPK, ledgerControlAccountPK, "LedgerControl");
				},
				errorMsg => { AssertEquals($"Could not find GL Account. Account Type: Ledger Control Account, Tax Transaction info: Branch = BBB, Ledger = AP, Tax system = ZZZ, Tax configuration Code = TAXCNFG", errorMsg); },
				suppliedFactory =>
				{
					DeleteObjectFromDB<AccGLHeader>(suppliedFactory, ledgerControlAccountPK);
				});
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxControlGLAccountPK_ValidValue()
		{
			var taxControlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();

			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxControlGLAccountPK = taxControlAccount.PK, restoredDataObject => AssertEquals(taxControlAccount.PK, restoredDataObject.TaxControlGLAccountPK));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxControlGLAccountPK_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxControlGLAccountPK = ZGuid.Empty, restoredDataObject => AssertEquals(ZGuid.Empty, restoredDataObject.TaxControlGLAccountPK));
		}

		public void TestRestoreTaxTransaction_ErrorInContext_When_TaxControlGLAccountBizoNotInDB()
		{
			var (taxControlAccountPK, taxConfigPK, branchPK) = Create_BizOs_For_ErrorInContext_Tests();

			AssertRestoreTaxTransaction_ErrorInContext(
				dataObject =>
				{
					Setup_TaxRecordData_For_ErrorInContextTests(dataObject, branchPK, taxConfigPK, taxControlAccountPK, "TaxControl");
				},
				errorMsg => { AssertEquals($"Could not find GL Account. Account Type: Tax Control Account, Tax Transaction info: Branch = BBB, Ledger = AP, Tax system = ZZZ, Tax configuration Code = TAXCNFG", errorMsg); },
				suppliedFactory =>
				{
					DeleteObjectFromDB<AccGLHeader>(suppliedFactory, taxControlAccountPK);
				});
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxExpenseGLAccountPK_ValidValue()
		{
			var taxExpenseAccount = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();

			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxExpenseGLAccountPK = taxExpenseAccount.PK, restoredDataObject => AssertEquals(taxExpenseAccount.PK, restoredDataObject.TaxExpenseGLAccountPK));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxExpenseGLAccountPK_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxExpenseGLAccountPK = ZGuid.Empty, restoredDataObject => AssertEquals(ZGuid.Empty, restoredDataObject.TaxExpenseGLAccountPK));
		}

		public void TestRestoreTaxTransaction_ErrorInContext_When_TaxExpenseGLAccountBizoNotInDB()
		{
			var (taxExpenseAccountPK, taxConfigPK, branchPK) = Create_BizOs_For_ErrorInContext_Tests();

			AssertRestoreTaxTransaction_ErrorInContext(
				dataObject =>
				{
					Setup_TaxRecordData_For_ErrorInContextTests(dataObject, branchPK, taxConfigPK, taxExpenseAccountPK, "TaxExpense");
				},
				errorMsg => { AssertEquals($"Could not find GL Account. Account Type: Tax Expense Account, Tax Transaction info: Branch = BBB, Ledger = AP, Tax system = ZZZ, Tax configuration Code = TAXCNFG", errorMsg); },
				suppliedFactory =>
				{
					DeleteObjectFromDB<AccGLHeader>(suppliedFactory, taxExpenseAccountPK);
				});
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxPendingControlGLAccountPK_ValidValue()
		{
			var taxPendingControlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			Factory.Save();

			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxPendingControlGLAccountPK = taxPendingControlAccount.PK, restoredDataObject => AssertEquals(taxPendingControlAccount.PK, restoredDataObject.TaxPendingControlGLAccountPK));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxPendingControlGLAccountPK_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxPendingControlGLAccountPK = ZGuid.Empty, restoredDataObject => AssertEquals(ZGuid.Empty, restoredDataObject.TaxPendingControlGLAccountPK));
		}

		public void TestRestoreTaxTransaction_ErrorInContext_When_TaxPendingControlGLAccountBizoNotInDB()
		{
			var (taxPendingControlPK, taxConfigPK, branchPK) = Create_BizOs_For_ErrorInContext_Tests();

			AssertRestoreTaxTransaction_ErrorInContext(
				dataObject =>
				{
					Setup_TaxRecordData_For_ErrorInContextTests(dataObject, branchPK, taxConfigPK, taxPendingControlPK, "TaxPendingControl");
				},
				errorMsg => { AssertEquals($"Could not find GL Account. Account Type: Tax Pending Control Account, Tax Transaction info: Branch = BBB, Ledger = AP, Tax system = ZZZ, Tax configuration Code = TAXCNFG", errorMsg); },
				suppliedFactory =>
				{
					DeleteObjectFromDB<AccGLHeader>(suppliedFactory, taxPendingControlPK);
				});
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxIDCode_ValidValue()
		{
			var taxID = Factory.NewWithValidTestData<AccTaxRate>();
			taxID.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();

			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxIDPK = taxID.PK, restoredDataObject => AssertEquals(taxID.PK, restoredDataObject.TaxIDPK));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxIDCode_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxIDPK = ZGuid.Empty, restoredDataObject => AssertEquals(ZGuid.Empty, restoredDataObject.TaxIDPK));
		}

		public void TestSaveRestoreTaxTransaction_TaxRateCodeIsEmpty_When_TaxRateBizoNotInDB()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxIDPK = ZGuid.NewZGuid(), restoredDataObject => AssertEquals(ZGuid.Empty, restoredDataObject.TaxIDPK));
		}

		public void TestRestoreTaxTransaction_ErrorInContext_When_TaxRateCodeNotInDB()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("TESTCODE", "Test Tax rate", 3);
			Factory.Save();

			AssertRestoreTaxTransaction_ErrorInContext(
				dataObject => dataObject.TaxIDPK = taxRate.PK,
				errorMsg => { AssertEquals("Could not find tax rate TESTCODE", errorMsg); },
				suppliedFactory =>
				{
					var taxRateInNewFactory = suppliedFactory.Load<AccTaxRate>(taxRate.PK);
					taxRateInNewFactory.AT_Code = "NEWCODE";
					suppliedFactory.Save();
				});
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxBasis_ValidValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxBasis = TaxBasisList.PostingOnMatching.Code, restoredDataObject => AssertEquals(TaxBasisList.PostingOnMatching.Code, restoredDataObject.TaxBasis));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxBasis_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxBasis = ZString.Empty, restoredDataObject => AssertEquals(ZString.Empty, restoredDataObject.TaxBasis));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxConfigurationPK_ValidValue()
		{
			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			Factory.Save();
			Assert("Precondition: taxConfig ETC_IsActive", taxConfig.ETC_IsActive);
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxConfigurationPK = taxConfig.PK, restoredDataObject => AssertEquals(taxConfig.PK, restoredDataObject.TaxConfigurationPK));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxConfigurationPK_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxConfigurationPK = ZGuid.Empty, restoredDataObject => AssertEquals(ZGuid.Empty, restoredDataObject.TaxConfigurationPK));
		}

		public void TestRestoreTaxTransaction_ErrorInContext_When_TaxConfigurationBizoNotInDB()
		{
			var (_, taxConfigPK, branchPK) = Create_BizOs_For_ErrorInContext_Tests();

			AssertRestoreTaxTransaction_ErrorInContext(
				dataObject =>
				{
					Setup_TaxRecordData_For_ErrorInContextTests(dataObject, branchPK, taxConfigPK, ZGuid.Empty);
				},
				errorMsg => { AssertEquals($"Could not find tax configuration. Tax Transaction info: Branch = BBB, Ledger = AP, Tax system = ZZZ", errorMsg); },
				suppliedFactory =>
				{
					DeleteObjectFromDB<AccTaxConfiguration>(suppliedFactory, taxConfigPK);
				});
		}

		public void TestRestoreTaxTransaction_ErrorInContext_When_TaxConfigurationIsInactive()
		{
			var (_, taxConfigPK, branchPK) = Create_BizOs_For_ErrorInContext_Tests();

			AssertRestoreTaxTransaction_ErrorInContext(
				dataObject =>
				{
					Setup_TaxRecordData_For_ErrorInContextTests(dataObject, branchPK, taxConfigPK, ZGuid.Empty);
				},
				errorMsg => { AssertEquals($"Could not find active tax configuration. Tax Transaction info: Branch = BBB, Ledger = AP, Tax system = ZZZ, Tax configuration code = TAXCNFG", errorMsg); },
				suppliedFactory =>
				{
					var taxConfigInNewFactory = suppliedFactory.Load<AccTaxConfiguration>(taxConfigPK);
					taxConfigInNewFactory.ETC_IsActive = false;
					suppliedFactory.Save();
				});
		}

		public void TestRestoreSameTaxRecordDataForProperty_LocalTaxAmount()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.LocalTaxAmount = 111, restoredDataObject => AssertEquals(111m, restoredDataObject.LocalTaxAmount));
		}

		public void TestRestoreSameTaxRecordDataForProperty_LocalTaxBaseAmount()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.LocalTaxBaseAmount = 222, restoredDataObject => AssertEquals(222m, restoredDataObject.LocalTaxBaseAmount));
		}

		public void TestRestoreSameTaxRecordDataForProperty_OSTaxAmount()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.OSTaxAmount = 333, restoredDataObject => AssertEquals(333m, restoredDataObject.OSTaxAmount));
		}

		public void TestRestoreSameTaxRecordDataForProperty_OSTaxBaseAmount()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.OSTaxBaseAmount = 444, restoredDataObject => AssertEquals(444m, restoredDataObject.OSTaxBaseAmount));
		}

		public void TestRestoreSameTaxRecordDataForProperty_BranchCode_ValidValue()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.BranchPK = branch.PK, restoredDataObject => AssertEquals(branch.PK, restoredDataObject.BranchPK));
		}

		public void TestRestoreSameTaxRecordDataForProperty_BranchCode_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.BranchPK = ZGuid.Empty, restoredDataObject => AssertEquals(ZGuid.Empty, restoredDataObject.BranchPK));
		}

		public void TestSaveRestoreTaxTransaction_BranchCodeIsEmpty_When_BranchBizoNotInDB()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.BranchPK = ZGuid.NewZGuid(), restoredDataObject => AssertEquals(ZGuid.Empty, restoredDataObject.BranchPK));
		}

		public void TestRestoreTaxTransaction_ErrorInContext_When_BranchCodeNotInDB()
		{
			var branch = TestObjectCreator.CreateBranch("BCH", GlbCompany.CurrentCompany);
			Factory.Save();

			AssertRestoreTaxTransaction_ErrorInContext(
				dataObject => dataObject.BranchPK = branch.PK,
				errorMsg => { AssertEquals("Could not find branch BCH", errorMsg); },
				suppliedFactory =>
				{
					var branchInNewFactory = suppliedFactory.Load<GlbBranch>(branch.PK);
					branchInNewFactory.GB_Code = "BBB";
					suppliedFactory.Save();
				});
		}

		public void TestRestoreSameTaxRecordDataForProperty_DepartmentCode_ValidValue()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.DepartmentPK = department.PK, restoredDataObject => AssertEquals(department.PK, restoredDataObject.DepartmentPK));
		}

		public void TestRestoreSameTaxRecordDataForProperty_DepartmentCode_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.DepartmentPK = ZGuid.Empty, restoredDataObject => AssertEquals(ZGuid.Empty, restoredDataObject.DepartmentPK));
		}

		public void TestSaveRestoreTaxTransaction_DepartmentCodeIsEmpty_When_DepartmentBizoNotInDB()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.DepartmentPK = ZGuid.NewZGuid(), restoredDataObject => AssertEquals(ZGuid.Empty, restoredDataObject.DepartmentPK));
		}

		public void TestRestoreTaxTransaction_ErrorInContext_When_DepartmentCodeNotInDB()
		{
			var department = TestObjectCreator.CreateDepartment("DPT");
			Factory.Save();

			AssertRestoreTaxTransaction_ErrorInContext(
				dataObject => dataObject.DepartmentPK = department.PK,
				errorMsg => { AssertEquals("Could not find department DPT", errorMsg); },
				suppliedFactory =>
				{
					var departmentInNewFactory = suppliedFactory.Load<GlbDepartment>(department.PK);
					departmentInNewFactory.GE_Code = "DDD";
					suppliedFactory.Save();
				});
		}

		public void TestRestoreSameTaxRecordDataForProperty_Ledger_ValidValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.Ledger = LedgerTypesList.Codes.AccountsReceivable, restoredDataObject => AssertEquals(LedgerTypesList.Codes.AccountsReceivable, restoredDataObject.Ledger));
		}

		public void TestRestoreSameTaxRecordDataForProperty_Ledger_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.Ledger = ZString.Empty, restoredDataObject => AssertEquals(ZString.Empty, restoredDataObject.Ledger));
		}

		public void TestRestoreSameTaxRecordDataForProperty_PostDate_ValidValue()
		{
			var postDate = new ZDateTime(2020, 04, 10).Date;
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.PostDate = postDate, restoredDataObject => AssertEquals(postDate, restoredDataObject.PostDate));
		}

		public void TestRestoreSameTaxRecordDataForProperty_PostDate_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.PostDate = ZDate.Empty, restoredDataObject => AssertEquals(ZDate.Empty, restoredDataObject.PostDate));
		}

		public void TestRestoreSameTaxRecordDataForProperty_RateDenominator()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.RateDenominator = 9999, restoredDataObject => AssertEquals(9999, restoredDataObject.RateDenominator));
		}

		public void TestRestoreSameTaxRecordDataForProperty_RateNumerator()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.RateNumerator = 125, restoredDataObject => AssertEquals(125, restoredDataObject.RateNumerator));
		}

		public void TestRestoreSameTaxRecordDataForProperty_RealisationDate_ValidValue()
		{
			var realisationDate = new ZDateTime(2020, 04, 10).Date;
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.RealisationDate = realisationDate, restoredDataObject => AssertEquals(realisationDate, restoredDataObject.RealisationDate));
		}

		public void TestRestoreSameTaxRecordDataForProperty_RealisationDate_EmptyValue()
		{
			var realisationDate = new ZDateTime(2020, 04, 10).Date;
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.RealisationDate = ZDate.Empty, restoredDataObject => AssertEquals(ZDate.Empty, restoredDataObject.RealisationDate));
		}

		public void TestRestoreSameTaxRecordDataForProperty_OSTaxCurrency_ValidValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.OSTaxCurrency = Core.Constants.CurrencyCodes.Australia, restoredDataObject => AssertEquals(Core.Constants.CurrencyCodes.Australia, restoredDataObject.OSTaxCurrency));
		}

		public void TestRestoreSameTaxRecordDataForProperty_OSTaxCurrency_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.OSTaxCurrency = ZString.Empty, restoredDataObject => AssertEquals(ZString.Empty, restoredDataObject.OSTaxCurrency));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxAuthorityServiceCode_ValidValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxAuthorityServiceCode = "TESTCODE1", restoredDataObject => AssertEquals("TESTCODE1", restoredDataObject.TaxAuthorityServiceCode));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxAuthorityServiceCode_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxAuthorityServiceCode = ZString.Empty, restoredDataObject => AssertEquals(ZString.Empty, restoredDataObject.TaxAuthorityServiceCode));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxAuthorityServiceCodeDescription_ValidValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxAuthorityServiceCodeDescription = "TEST DESCRIPTION 1", restoredDataObject => AssertEquals("TEST DESCRIPTION 1", restoredDataObject.TaxAuthorityServiceCodeDescription));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxAuthorityServiceCodeDescription_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxAuthorityServiceCodeDescription = ZString.Empty, restoredDataObject => AssertEquals(ZString.Empty, restoredDataObject.TaxAuthorityServiceCodeDescription));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxDate_ValidValue()
		{
			var taxDate = new ZDateTime(2020, 04, 10).Date;
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxDate = taxDate, restoredDataObject => AssertEquals(taxDate, restoredDataObject.TaxDate));
		}
		public void TestRestoreSameTaxRecordDataForProperty_TaxDate_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxDate = ZDate.Empty, restoredDataObject => AssertEquals(ZDate.Empty, restoredDataObject.TaxDate));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxSuperType_ValidValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxSuperType = TaxSuperTypeList.TurnoverTax.Code, restoredDataObject => AssertEquals(TaxSuperTypeList.TurnoverTax.Code, restoredDataObject.TaxSuperType));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxSuperType_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxSuperType = ZString.Empty, restoredDataObject => AssertEquals(ZString.Empty, restoredDataObject.TaxSuperType));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxSystemCode_ValidValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxSystemCode = "TAXSYSTEMCODE1", restoredDataObject => AssertEquals("TAXSYSTEMCODE1", restoredDataObject.TaxSystemCode));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TaxSystemCode_EmptyValue()
		{
			AssertPropertyValuesAfterSaveAndRestore(dataObject => dataObject.TaxSystemCode = ZString.Empty, restoredDataObject => AssertEquals(ZString.Empty, restoredDataObject.TaxSystemCode));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TransactionLinePKs_WithTransactionLinesOnly()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();

			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, 100m);
			line1.AL_Desc = "TransactionLinePK test line1";
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, 200m);
			line2.AL_Desc = "TransactionLinePK test line2";
			var line3 = TestObjectCreator.CreateInvoiceLine(invoice, 300m);
			line3.AL_Desc = "TransactionLinePK test line3";
			var line4 = TestObjectCreator.CreateInvoiceLine(invoice, 400m);
			line4.AL_Desc = "TransactionLinePK test line4";

			var taxRecordDataObject1 = new TaxRecordData();
			taxRecordDataObject1.TransactionLinePKs = new[] { line1.PK, line3.PK };

			var taxRecordDataObject2 = new TaxRecordData();
			taxRecordDataObject2.TransactionLinePKs = new[] { line2.PK, line3.PK, line4.PK };

			IReadOnlyTaxRecordData[] restoredTaxRecordDataObjects = null;
			CreateAndSetupTaxProcessorMock(new[] { taxRecordDataObject1, taxRecordDataObject2 }, dataObjects => restoredTaxRecordDataObjects = dataObjects);

			var restoredIncompleteInvoice = SaveAndRestoreInvoice(invoice);

			AssertLinesAreSame(line1, restoredIncompleteInvoice, restoredTaxRecordDataObjects[0].TransactionLinePKs.ElementAt(0));
			AssertLinesAreSame(line3, restoredIncompleteInvoice, restoredTaxRecordDataObjects[0].TransactionLinePKs.ElementAt(1));
			AssertLinesAreSame(line2, restoredIncompleteInvoice, restoredTaxRecordDataObjects[1].TransactionLinePKs.ElementAt(0));
			AssertLinesAreSame(line3, restoredIncompleteInvoice, restoredTaxRecordDataObjects[1].TransactionLinePKs.ElementAt(1));
			AssertLinesAreSame(line4, restoredIncompleteInvoice, restoredTaxRecordDataObjects[1].TransactionLinePKs.ElementAt(2));
		}

		public void TestRestoreSameTaxRecordDataForProperty_TransactionLinePKs_WithConsolCostChargesOnly()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var shipment2 = TestObjectCreator.CreateShipment("S00001005", "AUSYD", "NZAKL", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "00001", TestObjectCreator.USD, 1.3321m, TestObjectCreator.Creditor1);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, null, 1000, true, "MAN");
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, null, 2000, true, "MAN");
			invoice.ConsolCosting.ConsolCosts.Add(consolCost1);
			invoice.ConsolCosting.ConsolCosts.Add(consolCost2);

			consolCost1.ApportionmentCharges[0].JR_OSCostAmt = 700m;
			consolCost1.ApportionmentCharges[1].JR_OSCostAmt = 300m;
			consolCost2.ApportionmentCharges[0].JR_OSCostAmt = 800m;
			consolCost2.ApportionmentCharges[1].JR_OSCostAmt = 1200m;

			invoice.ImportAllApportionmentsFromCosting();
			AssertEquals("Precondition: Line Count", 4, invoice.Lines.Count);

			var lines = invoice.Lines;

			var taxRecordDataObject1 = new TaxRecordData();
			taxRecordDataObject1.TransactionLinePKs = new[] { lines[0].PK, lines[1].PK, lines[2].PK };

			var taxRecordDataObject2 = new TaxRecordData();
			taxRecordDataObject2.TransactionLinePKs = new[] { lines[2].PK, lines[3].PK };

			IReadOnlyTaxRecordData[] restoredTaxRecordDataObjects = null;
			CreateAndSetupTaxProcessorMock(new[] { taxRecordDataObject1, taxRecordDataObject2 }, dataObjects => restoredTaxRecordDataObjects = dataObjects);

			var restoredIncompleteInvoice = SaveAndRestoreInvoice(invoice);

			AssertLinesAreSame(lines[0], restoredIncompleteInvoice, restoredTaxRecordDataObjects[0].TransactionLinePKs.ElementAt(0), isLineImportedFromApportionSplitCharge: true);
			AssertLinesAreSame(lines[1], restoredIncompleteInvoice, restoredTaxRecordDataObjects[0].TransactionLinePKs.ElementAt(1), isLineImportedFromApportionSplitCharge: true);
			AssertLinesAreSame(lines[2], restoredIncompleteInvoice, restoredTaxRecordDataObjects[0].TransactionLinePKs.ElementAt(2), isLineImportedFromApportionSplitCharge: true);
			AssertLinesAreSame(lines[2], restoredIncompleteInvoice, restoredTaxRecordDataObjects[1].TransactionLinePKs.ElementAt(0), isLineImportedFromApportionSplitCharge: true);
			AssertLinesAreSame(lines[3], restoredIncompleteInvoice, restoredTaxRecordDataObjects[1].TransactionLinePKs.ElementAt(1), isLineImportedFromApportionSplitCharge: true);
		}

		public void TestRestoreSameTaxRecordDataForProperty_TransactionLinePKs_WithTransactionLineAndConsolCostCharge()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "00001", TestObjectCreator.USD, 1.3321m, TestObjectCreator.Creditor1);
			var consolCost = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 1000);

			var line = TestObjectCreator.CreateInvoiceLine(invoice, 100m);
			line.AL_Desc = "TransactionLinePK test line1";

			invoice.ImportAllApportionmentsFromCosting();
			var lines = invoice.Lines;

			AssertEquals("Precondition: Line Count", 2, lines.Count);

			var taxRecordDataObject1 = new TaxRecordData();
			taxRecordDataObject1.TransactionLinePKs = new[] { lines[1].PK, lines[0].PK };

			var taxRecordDataObject2 = new TaxRecordData();
			taxRecordDataObject2.TransactionLinePKs = new[] { lines[1].PK };

			IReadOnlyTaxRecordData[] restoredTaxRecordDataObjects = null;
			CreateAndSetupTaxProcessorMock(new[] { taxRecordDataObject1, taxRecordDataObject2 }, dataObjects => restoredTaxRecordDataObjects = dataObjects);

			var restoredIncompleteInvoice = SaveAndRestoreInvoice(invoice);

			AssertLinesAreSame(lines[1], restoredIncompleteInvoice, restoredTaxRecordDataObjects[0].TransactionLinePKs.ElementAt(0), isLineImportedFromApportionSplitCharge: true);
			AssertLinesAreSame(lines[0], restoredIncompleteInvoice, restoredTaxRecordDataObjects[0].TransactionLinePKs.ElementAt(1), isLineImportedFromApportionSplitCharge: false);
			AssertLinesAreSame(lines[1], restoredIncompleteInvoice, restoredTaxRecordDataObjects[1].TransactionLinePKs.ElementAt(0), isLineImportedFromApportionSplitCharge: true);
		}

		public void TestSaveIncompleteInvoice_ThrowsNoException_With_NotAllApportionSplitChargeImportedAsLines_When_SaveTaxtransactionRegistryEnabled()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var shipment2 = TestObjectCreator.CreateShipment("S00001005", "AUSYD", "NZAKL", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "00001", TestObjectCreator.USD, 1.3321m, TestObjectCreator.Creditor1);
			var consolCost = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 1000);

			consolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = false;

			invoice.ImportAllApportionmentsFromCosting();
			AssertEquals("Precondition: Line Count", 1, invoice.Lines.Count);

			AssertNoExceptionThrown(() => invoice.SaveAsIncomplete());
		}

		void AssertLinesAreSame(InvoicingLineBase savedLine, InvoicingBase restoredInvoice, ZGuid restoredLinePK, bool isLineImportedFromApportionSplitCharge = false)
		{
			var restoredLine = restoredInvoice.Lines.Where(x => x.PK == restoredLinePK).FirstOrDefault();

			AssertEquals("Line parent", savedLine.AL_AH, restoredLine.AL_AH);
			AssertEquals("Line Description", savedLine.AL_Desc, restoredLine.AL_Desc);

			if (isLineImportedFromApportionSplitCharge)
			{
				AssertEquals("Charge Job header", savedLine.ApportionmentChargeImportedFrom.JR_JH, ((InvoicingLineBase)restoredLine).ApportionmentChargeImportedFrom.JR_JH);
				AssertEquals("Charge OS Cost Amount", savedLine.ApportionmentChargeImportedFrom.JR_OSCostAmt, ((InvoicingLineBase)restoredLine).ApportionmentChargeImportedFrom.JR_OSCostAmt);
			}
		}

		public void TestRestoreSameTaxRecordDataForProperty_TransactionLinePKs_Length()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, 100m);
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, 200m);
			var line3 = TestObjectCreator.CreateInvoiceLine(invoice, 300m);

			AssertPropertyValuesAfterSaveAndRestore(
				dataObject => dataObject.TransactionLinePKs = new[] { line1.PK, line2.PK, line3.PK },
				restoredDataObject =>
				{
					AssertEquals("Number of Transaction line PKs in restored taxRecordDataObject", 3, restoredDataObject.TransactionLinePKs.Count);
				}, invoice);
		}

		public void TestRestoreSameTaxRecordDataForProperty_SystemCalculatedValues_ValidValue()
		{
			var expectedSystemCalculatedValues = new TaxRecordDataSystemCalculatedValues(1000, 50, 5, 100, ZDate.Today, "CODE1", "DESCRIPTION 1");
			AssertPropertyValuesAfterSaveAndRestore(
				dataObject => dataObject.SystemCalculatedValues = expectedSystemCalculatedValues,
				restoredDataObject =>
				{
					AssertSystemCaculatedValues(expectedSystemCalculatedValues, restoredDataObject.SystemCalculatedValues);
				});

			void AssertSystemCaculatedValues(TaxRecordDataSystemCalculatedValues savedSystemCalculatedValues, TaxRecordDataSystemCalculatedValues restoredSystemCalculatedValues)
			{
				AssertEquals(savedSystemCalculatedValues.OSTaxBaseAmount, restoredSystemCalculatedValues.OSTaxBaseAmount);
				AssertEquals(savedSystemCalculatedValues.OSTaxAmount, restoredSystemCalculatedValues.OSTaxAmount);
				AssertEquals(savedSystemCalculatedValues.RateNumerator, restoredSystemCalculatedValues.RateNumerator);
				AssertEquals(savedSystemCalculatedValues.RateDenominator, restoredSystemCalculatedValues.RateDenominator);
				AssertEquals(savedSystemCalculatedValues.TaxDate, restoredSystemCalculatedValues.TaxDate);
				AssertEquals(savedSystemCalculatedValues.TaxAuthorityServiceCode, restoredSystemCalculatedValues.TaxAuthorityServiceCode);
				AssertEquals(savedSystemCalculatedValues.TaxAuthorityServiceCodeDescription, restoredSystemCalculatedValues.TaxAuthorityServiceCodeDescription);
			}
		}

		public void TestRestoreSameTaxRecordDataForProperty_SystemCalculatedValues_IsNull()
		{
			AssertPropertyValuesAfterSaveAndRestore(
				dataObject => dataObject.SystemCalculatedValues = null,
				restoredDataObject =>
				{
					AssertNull(restoredDataObject.SystemCalculatedValues);
				});
		}

		[ExpectNoExceptions]
		public void TestSaveRestoreTaxTransaction_NumberOfRestoredTaxRecords_SameAs_NumberOfSavedTaxRecords()
		{
			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, exchangeRate: 1, aL_OSExTaxAmount: 100m);

			var taxRecordDataObjectsList = new List<IReadOnlyTaxRecordData>();

			var taxRecordDataObject1 = new TaxRecordData();
			taxRecordDataObject1.TransactionLinePKs = new[] { line.PK };

			taxRecordDataObjectsList.Add(taxRecordDataObject1);

			AssertTaxRecordDataCountBeforeAndAfter("Invoice with 1 tax record", expectedNumberOfTaxRecords: 1);

			var taxRecordDataObject2 = new TaxRecordData();
			taxRecordDataObject2.TransactionLinePKs = new[] { line.PK };
			taxRecordDataObjectsList.Add(taxRecordDataObject2);

			AssertTaxRecordDataCountBeforeAndAfter("Invoice with 2 tax record", expectedNumberOfTaxRecords: 2);

			void AssertTaxRecordDataCountBeforeAndAfter(string message, int expectedNumberOfTaxRecords)
			{
				taxProcessorMock.Setup(x => x.GetTaxRecordDataForDataTransfer(It.IsAny<ITaxRecordParent>())).Returns(taxRecordDataObjectsList.ToArray());
				SaveAndRestoreInvoice(invoice);

				taxProcessorMock.Verify(x => x.RestoreFromTaxRecordData(It.IsAny<ITaxRecordParent>(), It.Is<IReadOnlyCollection<IReadOnlyTaxRecordData>>((restoredTaxRecordDataObjects) => expectedNumberOfTaxRecords == restoredTaxRecordDataObjects.Count)), message);
			}
		}

		void AssertPropertyValuesAfterSaveAndRestore(Action<TaxRecordData> setup, Action<IReadOnlyTaxRecordData> assert, InvoicingBase invoice = null)
		{
			if (invoice == null)
			{
				invoice = Factory.NewWithValidTestData<APInvoice>();
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, exchangeRate: 3, aL_OSExTaxAmount: 333m);
			}

			var taxRecordDataObject = new TaxRecordData();
			taxRecordDataObject.TransactionLinePKs = new[] { invoice.Lines[0].PK };

			setup(taxRecordDataObject);

			IReadOnlyTaxRecordData[] restoredTaxRecordDataObjects = null;
			CreateAndSetupTaxProcessorMock(new[] { taxRecordDataObject }, dataObjects => restoredTaxRecordDataObjects = dataObjects);

			var restoredIncompleteInvoice = SaveAndRestoreInvoice(invoice);

			assert(restoredTaxRecordDataObjects[0]);
		}

		public void TestSaveRestoreTaxTransaction_ThrowsInvalidOperationException_When_TaxRecordDataForInvoiceAlreadyPresentInFactoryCache()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var line = TestObjectCreator.CreateInvoiceLine(invoice, 100m);

			var taxRecordDataObject = new TaxRecordData();
			taxRecordDataObject.TransactionLinePKs = new[] { line.PK };

			IReadOnlyTaxRecordData[] restoredTaxRecordDataObjects = null;
			CreateAndSetupTaxProcessorMock(new[] { taxRecordDataObject }, dataObjects => restoredTaxRecordDataObjects = dataObjects);

			var incompleteInvoiceDataAdapter = new IncompleteTransactionDataAdapter<APInvoice>(invoice);
			var incompleteTransactionXML = ExportToXML(invoice, incompleteInvoiceDataAdapter); // Creates transactionHeader XML data

			var factoryUsedForImport = new BusinessObjectFactory();
			var importedIncompleteInvoice = factoryUsedForImport.New<APInvoice>();
			importedIncompleteInvoice.AH_TransactionNum = "TEST001";

			ImportFromXML(factoryUsedForImport, incompleteInvoiceDataAdapter, importedIncompleteInvoice, incompleteTransactionXML); // Imports back from the transactionHeader XML data and saves taxRecordDataList in factory cache in the process.
																																	// Import is called instead of RestoreSavedData to avoid the clearing of Factory cache where taxRecordDataList is saved.

			AssertEquals("Precondition: TaxRecordData list in factory cache ", expected: true, importedIncompleteInvoice.Factory.TryGetValueFromCacheOnly("RestoredIncompleteInvoiceTaxRecordsData" + importedIncompleteInvoice.PK, out List<TaxRecordData> taxRecordDataList));

			AssertExceptionThrown(typeof(InvalidOperationException), "Import incomplete invoice operation may have been attempted twice, which is not valid. TaxRecordData for the same key already exists in the invoice factory cache.",
				() => ImportFromXML(factoryUsedForImport, incompleteInvoiceDataAdapter, importedIncompleteInvoice, incompleteTransactionXML));
		}

		void AssertRestoreTaxTransaction_ErrorInContext(Action<TaxRecordData> setup, Action<ZString> assert, Action<BusinessObjectFactory> alterDBBetweenSaveAndRestore)
		{
			InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var line = TestObjectCreator.CreateInvoiceLine(invoice, 100m);
			var taxRecordDataObject = new TaxRecordData();
			taxRecordDataObject.TransactionLinePKs = new[] { line.PK };

			setup(taxRecordDataObject);

			CreateAndSetupTaxProcessorMock(new[] { taxRecordDataObject }, dataObjects => { });

			invoice.SaveAsIncomplete();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			alterDBBetweenSaveAndRestore(newFactory);

			var incompleteInvoice = newFactory.Load<APInvoice>(invoice.PK);

			var result = incompleteInvoice.RestoreSavedData();

			assert(result.Error);
		}

		void DeleteObjectFromDB<BizoTypeName>(BusinessObjectFactory suppliedFactory, ZGuid pkOfObjectToDelete) where BizoTypeName : BusinessObject
		{
			var bizO = suppliedFactory.Load<BizoTypeName>(pkOfObjectToDelete);
			bizO.Delete();
			suppliedFactory.Save();
		}

		Xsd.IncompleteTransactionHeader ExportToXML(APInvoice invoice, IncompleteTransactionDataAdapter<APInvoice> transactionDataAdapter)
		{
			var incompleteTransactionXML = new Xsd.IncompleteTransactionHeader();

			transactionDataAdapter.ExportToValueObject(invoice, incompleteTransactionXML, null);

			return incompleteTransactionXML;
		}

		void ImportFromXML(BusinessObjectFactory factoryToUseForImport, IncompleteTransactionDataAdapter<APInvoice> transactionDataAdapter, APInvoice importedIncompleteInvoice, Xsd.IncompleteTransactionHeader incompleteTransactionXMLData)
		{
			IValueObjectImportContext context = new ValueObjectImportContext(factoryToUseForImport, new NotificationBuffer());
			transactionDataAdapter.ImportFromValueObject(importedIncompleteInvoice, incompleteTransactionXMLData, context);
		}

		InvoicingBase SaveAndRestoreInvoice(InvoicingBase invoice)
		{
			invoice.SaveAsIncomplete();

			var incompleteInvoice = new BusinessObjectFactory().Load<APInvoice>(invoice.PK);
			incompleteInvoice.RestoreSavedData();

			return incompleteInvoice;
		}

		void CreateAndSetupTaxProcessorMock(TaxRecordData[] taxRecordsDataToReturn, Action<IReadOnlyTaxRecordData[]> callbackOnRestore)
		{
			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			taxProcessorMock.Setup(x => x.GetTaxRecordDataForDataTransfer(It.IsAny<ITaxRecordParent>())).Returns(taxRecordsDataToReturn);

			taxProcessorMock.Setup(x => x.RestoreFromTaxRecordData(It.IsAny<ITaxRecordParent>(), It.IsAny<IReadOnlyCollection<IReadOnlyTaxRecordData>>()))
				.Callback<ITaxRecordParent, IReadOnlyCollection<IReadOnlyTaxRecordData>>((taxRecordParent, dataObjects) => callbackOnRestore(dataObjects.ToArray()));
		}

		(ZGuid GLAccountPK, ZGuid TaxConfigPK, ZGuid BranckPK) Create_BizOs_For_ErrorInContext_Tests()
		{
			var glAccount = TestObjectCreator.CreateAPControlAccount();
			var taxConfig = AccountingTestObjectCreator.CreateTaxConfiguration("AP");
			taxConfig.ETC_Code = "TAXCNFG";
			var branch = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);
			Factory.Save();

			return (glAccount.PK, taxConfig.PK, branch.PK);
		}

		void Setup_TaxRecordData_For_ErrorInContextTests(TaxRecordData dataObject, ZGuid branchPK, ZGuid taxConfigPK, ZGuid glAccountPK, string glAccountType = "")
		{
			dataObject.Ledger = "AP";
			dataObject.BranchPK = branchPK;
			dataObject.TaxSystemCode = "ZZZ";
			dataObject.TaxConfigurationPK = taxConfigPK;

			switch (glAccountType)
			{
				case "LedgerControl":
					dataObject.LedgerControlGLAccountPK = glAccountPK;
					break;
				case "TaxControl":
					dataObject.TaxControlGLAccountPK = glAccountPK;
					break;
				case "TaxExpense":
					dataObject.TaxExpenseGLAccountPK = glAccountPK;
					break;
				case "TaxPendingControl":
					dataObject.TaxPendingControlGLAccountPK = glAccountPK;
					break;
				default:
					break;
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accountingTestObjectCreator;
	}
}
