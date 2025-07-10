using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.AccountingCountryFactory.Testing
{
	public class TaiwanComplianceDocumentNumberProviderTest : TestCaseWithFactory
	{
		public void TestTaiwanComplianceDocumentNumberProvider_AllocateComplianceDocumentNumberErrorMessage()
		{
			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine.AL_AT = TestObjectCreator.GST1.PK;
			var complianceDocumentHeader1 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine) as ARComplianceDocumentHeader;
			var complianceDocumentHeaderList = new List<ARComplianceDocumentHeader>() { complianceDocumentHeader1 };

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			complianceDocumentHeader1.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;

			AssertEquals("Precondition: EnableEInvoicingFunctionality is true", true, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);
			AssertEquals("Precondition: EnableComplianceDocumentModule is true", true, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);
			Assert("Precondition: All compliance document header is TXE", complianceDocumentHeaderList.All(x => x.ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE));
			Assert("Precondition: All compliance document header total amount is 0", complianceDocumentHeaderList.All(x => x.TotalAmount == 0));
			AssertEquals("Compliance document number cannot be allocated to TXE compliance document as the total amount of the compliance document is zero.", ComplianceDocumentNumberProvider.AllocateComplianceDocumentNumberErrorMessage(complianceDocumentHeaderList));

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition: EnableEInvoicingFunctionality is false", false, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);
			AssertEquals(string.Empty, ComplianceDocumentNumberProvider.AllocateComplianceDocumentNumberErrorMessage(complianceDocumentHeaderList));

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition: EnableEInvoicingFunctionality is true", true, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);
			AssertEquals("Precondition: EnableComplianceDocumentModule is false", false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);
			AssertEquals(string.Empty, ComplianceDocumentNumberProvider.AllocateComplianceDocumentNumberErrorMessage(complianceDocumentHeaderList));

			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			complianceDocumentHeader1.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC;
			AssertEquals("Precondition: EnableComplianceDocumentModule is true", true, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);
			Assert("Precondition: Some compliance document header is not TXE", complianceDocumentHeaderList.Any(x => x.ComplianceSubType != TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE));
			AssertEquals(string.Empty, ComplianceDocumentNumberProvider.AllocateComplianceDocumentNumberErrorMessage(complianceDocumentHeaderList));

			complianceDocumentHeader1.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
			var arInv2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var arInvLine2 = (ARInvoiceLine)arInv2.Lines.AddNew();
			arInvLine2.AL_AT = TestObjectCreator.GST1.PK;
			arInvLine2.AL_LineAmount = 10;
			var complianceDocumentHeader2 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine2) as ARComplianceDocumentHeader;
			complianceDocumentHeaderList.Add(complianceDocumentHeader2);
			Assert("Precondition: All compliance document header is TXE", complianceDocumentHeaderList.All(x => x.ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE));
			Assert("Precondition: Some of compliance document header total amount is not 0", complianceDocumentHeaderList.Any(x => x.TotalAmount != 0));
			AssertEquals(string.Empty, ComplianceDocumentNumberProvider.AllocateComplianceDocumentNumberErrorMessage(complianceDocumentHeaderList));
		}

		public void TestTaiwanComplianceDocumentNumberProvider_CheckCanAllocateComplianceDocumentNumberForSomeComplianceDocument()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine.AL_AT = TestObjectCreator.GST1.PK;
			var complianceDocumentHeader1 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine) as ARComplianceDocumentHeader;

			var arInv2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var arInvLine2 = (ARInvoiceLine)arInv2.Lines.AddNew();
			arInvLine2.AL_AT = TestObjectCreator.GST1.PK;
			arInvLine2.AL_LineAmount = 10;
			var complianceDocumentHeader2 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine2) as ARComplianceDocumentHeader;
			var complianceDocumentHeaderList = new List<ARComplianceDocumentHeader>() { complianceDocumentHeader1, complianceDocumentHeader2 };
			var errorMessageList = new List<string>();
			ComplianceDocumentNumberProvider.CheckCanAllocateComplianceDocumentNumberForSomeComplianceDocument(errorMessageList, complianceDocumentHeaderList);
			AssertCollectionContains("Compliance document number cannot be allocated to TXE compliance document as the total amount of the compliance document is zero.", errorMessageList);
			AssertCollectionContains(complianceDocumentHeader2, complianceDocumentHeaderList);
			AssertCollectionNotContains(complianceDocumentHeader1, complianceDocumentHeaderList);
		}

		public void TestTaiwanComplianceDocumentNumberProvider_GetComplianceDocumentNumbers()
		{
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var complianceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceBook.XD_IsActive = ZBool.True;
			complianceBook.XD_EndNumber = 100;
			complianceBook.XD_NextNumber = 1;
			complianceBook.XD_StartDate = ZDate.Today.AddYears(-1);
			complianceBook.XD_ExpiryDate = ZDateTime.Today.AddYears(1);
			Factory.Save();

			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			var arInvLine =  TestObjectCreator.CreateRevenueLine(Factory.New<Charge>(), arInv.PK);
			var arInvLine2 = TestObjectCreator.CreateRevenueLine(Factory.New<Charge>(), arInv.PK);

			arInv.Lines.Add(arInvLine);
			arInv.Lines.Add(arInvLine2);

			arInvLine2.AL_AT = TestObjectCreator.GST1.PK;
			arInvLine.AL_AT = TestObjectCreator.GST1.PK;
			var complianceDocumentHeader1 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine) as ARComplianceDocumentHeader;
			var complianceDocumentHeader2 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine2) as ARComplianceDocumentHeader;
			var complianceDocumentHeaderList = new ARComplianceDocumentHeader[] { complianceDocumentHeader1, complianceDocumentHeader2 };

			complianceDocumentHeader1.ADH_XD_ComplianceBook = complianceBook.PK;
			complianceDocumentHeader2.ADH_XD_ComplianceBook = complianceBook.PK;

			Factory.Save();

			ComplianceDocumentHelper.AllocateComplianceDocuments(Factory, complianceDocumentHeaderList);

			Factory.Save();

			var lines = ComplianceDocumentNumberProvider.GetComplianceDocumentNumbers(arInv.Lines);
			AssertEquals("Should have 2 lines with the transaction header", 2, lines.Count);
			AssertCollectionContains("000000001", lines);
			AssertCollectionContains("000000002", lines);
		}

		IComplianceDocumentNumberProvider ComplianceDocumentNumberProvider;

		TestObjectCreator testObjectCreator;
		public TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ComplianceDocumentNumberProvider = new TaiwanComplianceDocumentNumberProvider();
		}

		#endregion

	}
}
