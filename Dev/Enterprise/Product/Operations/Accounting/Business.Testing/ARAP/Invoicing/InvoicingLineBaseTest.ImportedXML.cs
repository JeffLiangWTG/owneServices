using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	partial class InvoicingLineBaseTest
	{
		public void TestShouldCreateChargeCodeMatchingRule()
		{
			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				if (MasterHeaderType != typeof(APInvoice) && MasterHeaderType != typeof(APCreditNote))
				{
					Assert(true);
					return;
				}
				var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
				orgAddress.AddressType = nameof(DocAddressType.None);
				orgAddress.Address1 = "Street";
				orgAddress.OrganizationCode = "ORG";

				var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				universalTransaction.Ledger = LedgerTypes.AccountsPayable;
				universalTransaction.OrganizationAddress = orgAddress;
				universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
				var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code } };
				universalTransaction.PostingJournalCollection.Add(universalLine1);

				var invoice = TestObjectCreator.CreateAndAllocateInvoiceWithUniversalTransactionInAllocationApprovalRequest(MasterHeaderType, universalTransaction);
				var line = invoice.Lines[0];
				line.AL_OSExTaxAmount = 20;
				line.GenericCharge = TestObjectCreator.CC2.PK;

				AssertNotEquals("Precondition", line.ChargeCode.AC_Code, line.ImportedChargeCodeXmlCode);
				AssertEquals("Autometic charge code mapping is enabled by default", true, line.ShouldCreateChargeCodeMatchingRule);

				AccountingConfigurationRegistry.Instance.EnableAutomaticChargeCodeMappingForUnallocatedInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals("Autometic charge code mapping is disabled", false, line.ShouldCreateChargeCodeMatchingRule);

				AccountingConfigurationRegistry.Instance.EnableAutomaticChargeCodeMappingForUnallocatedInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.EnableAutomaticChargeCodeMappingForUnallocatedInvoices.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertEquals("Autometic charge code mapping is disabled", false, line.ShouldCreateChargeCodeMatchingRule);
			}
		}

		public void TestImportedXMLCodeMappingForCodesWithoutMatchingRules()
		{
			using (AccountingConfigurationRegistry.Instance.EnableAutoAccrualMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				if (MasterHeaderType != typeof(APInvoice) && MasterHeaderType != typeof(APCreditNote))
				{
					Assert(true);
					return;
				}
				var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
				orgAddress.AddressType = nameof(DocAddressType.None);
				orgAddress.Address1 = "Street";

				var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				universalTransaction.Ledger = LedgerTypes.AccountsPayable;
				universalTransaction.OrganizationAddress = orgAddress;
				universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());

				var chargeCode = new ChargeCode();
				var expectedChargeCode = TestObjectCreator.CC1.AC_Code;
				chargeCode.Code = expectedChargeCode;
				var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
				universalLine1.ChargeCode = chargeCode;
				universalTransaction.PostingJournalCollection.Add(universalLine1);

				InvoicingBase invoice = TestObjectCreator.CreateInvoiceWithUniversalTransactionInAllocationApprovalRequest(MasterHeaderType, universalTransaction);
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.IndexOfImportedUniversalTransactionLine = 0;

				var universalLineWrapper = invoice.AllocationApprovalRequest.PostingDetails.UniversalTransaction.Lines[0];
				AssertEquals("universalLineWrapper.ChargeCodeSource", expectedChargeCode, universalLineWrapper.ChargeCodeSource);
				AssertEquals("universalLineWrapper.ChargeCode. Mapping of codes without matching rules should not be triggered before accessing ImportedChargeCode on invoice line to not do a job if nobody need it.", ZString.Empty, universalLineWrapper.ChargeCode);

				AssertEquals("ImportedChargeCodeXmlCode", expectedChargeCode, line.ImportedChargeCodeXmlCode);
				AssertEquals("ImportedChargeCode", expectedChargeCode, line.ImportedChargeCode);
			}
		}

		public void TestDefaultValuesForImportedXMLValues()
		{
			var invoiceLine = (InvoicingLineBase)GetNewBusinessObject();
			AssertEquals("IndexOfImportedUniversalTransactionLine: value 0 is reserved for the first line number.", -1, invoiceLine.IndexOfImportedUniversalTransactionLine);
		}

		public void TestJobConsolXMLDataForJobDefaultValue()
		{
			var invoice = TestObjectCreator.CreateInvoice(MasterHeaderType, "INV1");
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);
			AssertEquals("", line.JobConsolXMLData);
		}

		public void TestJobConsolXMLDataForConsolDefaultValue()
		{
			var invoice = TestObjectCreator.CreateInvoice(MasterHeaderType, "INV1");
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);
			AssertEquals("", line.JobConsolXMLData);
		}
	}
}
