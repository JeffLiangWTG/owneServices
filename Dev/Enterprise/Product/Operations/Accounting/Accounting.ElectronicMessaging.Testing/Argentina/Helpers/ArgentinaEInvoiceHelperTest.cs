using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing.Helpers
{
	public class ArgentinaEInvoiceHelperTest : TestCaseWithFactory
	{
		public void TestGetReferenceNumberInfo()
		{
			CombineAssertions(() =>
			{
				var transactionReference = "";
				AssertResult("", null, transactionReference);

				transactionReference = "FCA#-00023-000002526";
				AssertResult("000002526", null, transactionReference);

				transactionReference = "FCA#-00023-000002526";

				var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				complianceSequence.XD_Prefix = "FCA#-00023-";
				AssertResult("000002526", complianceSequence, transactionReference);

				void AssertResult(ZString expectedResult, AccComplianceSequence sequence, ZString reference)
				{
					AssertEquals(expectedResult, ArgentinaEInvoiceHelper.GetReferenceNumberInfo(sequence, reference));
				}
			});
		}

		public void TestGetReferencePrefixInfo()
		{
			CombineAssertions(() =>
			{
				var transactionReference = "";
				AssertResult("", null, transactionReference);

				transactionReference = "FCA#-00023-000002526";
				AssertResult("00023", null, transactionReference);

				transactionReference = "FCA#-00023-000002526";
				var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				complianceSequence.XD_Prefix = "FCA#-00023-";
				AssertResult("00023", complianceSequence, transactionReference);

				void AssertResult(ZString expectedResult, AccComplianceSequence sequence, ZString reference)
				{
					AssertEquals(expectedResult, ArgentinaEInvoiceHelper.GetReferencePrefixInfo(sequence, reference));
				}
			});
		}

		public void TestGetComplianceNumberPrefixFromTransaction()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertResult(ZString.Empty);

			transaction.OriginalReference = new OriginalReference();
			AssertResult(ZString.Empty);

			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = null;
			AssertResult(ZString.Empty);

			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionNumber = null;
			AssertResult(ZString.Empty);

			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			transaction.OriginalReference.OriginalTransactionReference = "0002200000004";
			AssertResult("00022");

			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionNumber = "TXA0000100000056";

			AssertResult("00001");

			void AssertResult(ZString? expectedResult)
			{
				AssertEquals(expectedResult, ArgentinaEInvoiceHelper.GetComplianceNumberPrefixFromTransaction(transaction.OriginalReference));
			}
		}

		public void TestGetComplianceNumberFromTransaction()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertResult(ZString.Empty);

			transaction.OriginalReference = new OriginalReference();
			AssertResult(ZString.Empty);

			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = null;
			AssertResult(ZString.Empty);

			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionNumber = null;
			AssertResult(ZString.Empty);

			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			transaction.OriginalReference.OriginalTransactionReference = "0002200000004";

			AssertResult("00000004");

			transaction.OriginalReference = new OriginalReference();
			transaction.OriginalReference.OriginalTransactionNumber = "TXA0000100000056";

			AssertResult("00000056");

			void AssertResult(ZString? expectedResult)
			{
				AssertEquals(expectedResult, ArgentinaEInvoiceHelper.GetComplianceNumberFromTransaction(transaction.OriginalReference));
			}
		}

		public void TestGetComplianceSequencePrefixInfo()
		{
			CombineAssertions(() =>
			{
				var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				Factory.Save();
				complianceSequence.XD_Prefix = "";
				AssertResult("", complianceSequence);

				complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				Factory.Save();
				complianceSequence.XD_Prefix = "00023";
				AssertResult("00023", complianceSequence);

				void AssertResult(ZString expectedResult, AccComplianceSequence sequence)
				{
					AssertEquals(expectedResult, ArgentinaEInvoiceHelper.GetComplianceSequencePrefixInfo(sequence));
				}
			});
		}

		public void TestGetOriginalTransactionReference_NullOrEmptyChecks()
		{
			CombineAssertions(() =>
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				AssertResult(ZString.Empty);

				transaction.OriginalReference = new OriginalReference();
				AssertResult(ZString.Empty);

				transaction.OriginalReference = new OriginalReference();
				transaction.OriginalReference.OriginalTransactionReference = null;
				AssertResult(ZString.Empty);

				transaction.OriginalReference = new OriginalReference();
				transaction.OriginalReference.OriginalTransactionNumber = null;
				AssertResult(ZString.Empty);

				transaction.OriginalReference = new OriginalReference();
				transaction.OriginalReference.OriginalTransactionReference = ZString.Empty;
				AssertResult(ZString.Empty);

				transaction.OriginalReference = new OriginalReference();
				transaction.OriginalReference.OriginalTransactionNumber = ZString.Empty;
				AssertResult(ZString.Empty);

				void AssertResult(ZString? expectedResult)
				{
					AssertEquals(expectedResult, ArgentinaEInvoiceHelper.GetOriginalTransactionReference(transaction.OriginalReference));
				}
			});
		}

		public void TestGetOriginalTransactionComplianceSubType_NullOrEmptyChecks()
		{
			CombineAssertions(() =>
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				AssertResult(ZString.Empty);

				transaction.OriginalReference = new OriginalReference();
				AssertResult(ZString.Empty);

				transaction.OriginalReference = new OriginalReference();
				transaction.OriginalReference.OriginalTransactionComplianceSubType = null;
				AssertResult(ZString.Empty);

				transaction.OriginalReference = new OriginalReference();
				transaction.OriginalReference.OriginalTransactionNumber = null;
				AssertResult(ZString.Empty);

				transaction.OriginalReference = new OriginalReference();
				transaction.OriginalReference.OriginalTransactionComplianceSubType = ZString.Empty;
				AssertResult(ZString.Empty);

				transaction.OriginalReference = new OriginalReference();
				transaction.OriginalReference.OriginalTransactionNumber = ZString.Empty;
				AssertResult(ZString.Empty);

				void AssertResult(ZString? expectedResult)
				{
					AssertEquals(expectedResult, ArgentinaEInvoiceHelper.GetOriginalTransactionComplianceSubType(transaction.OriginalReference));
				}
			});
		}

		public void TestGetOriginalTransactionReference()
		{
			CombineAssertions(() =>
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				transaction.OriginalReference = new OriginalReference();

				transaction.OriginalReference.OriginalTransactionReference = "1234567890";
				AssertResult("1234567890");

				transaction.OriginalReference.OriginalTransactionNumber = "TXA1234567890";
				AssertResult("1234567890");

				transaction.ComplianceSubType = "TDA";
				transaction.OriginalReference = new OriginalReference();
				transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXA";
				transaction.OriginalReference.OriginalTransactionJobInvoiceNumber = "S000001";
				transaction.OriginalReference.OriginalTransactionNumber = "TXA00001010";
				transaction.OriginalReference.OriginalTransactionDate = ZDateTime.Today;
				AssertResult("");

				void AssertResult(ZString expectedResult)
				{
					AssertEquals(expectedResult, ArgentinaEInvoiceHelper.GetOriginalTransactionReference(transaction.OriginalReference));
				}
			});
		}

		public void TestGetOriginalTransactionComplianceSubType()
		{
			CombineAssertions(() =>
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				transaction.OriginalReference = new OriginalReference();

				transaction.OriginalReference.OriginalTransactionComplianceSubType = "TXA";
				AssertResult("TXA");

				transaction.OriginalReference.OriginalTransactionNumber = "TXA1234567890";
				AssertResult("TXA");

				void AssertResult(ZString expectedResult)
				{
					AssertEquals(expectedResult, ArgentinaEInvoiceHelper.GetOriginalTransactionComplianceSubType(transaction.OriginalReference));
				}
			});
		}

		public void TestIsOriginalReferenceCreditOrDebitNoteTransaction()
		{
			CombineAssertions(() =>
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				transaction.OriginalReference = new OriginalReference();

				transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDA;
				AssertResult(true);

				transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
				AssertResult(false);

				void AssertResult(bool expectedResult)
				{
					AssertEquals(expectedResult, ArgentinaEInvoiceHelper.IsOriginalReferenceCreditOrDebitNoteTransaction(transaction));
				}
			});
		}

		#region MiPyme Opcionales and DatosAdicionales Node

		public void TestComplianceSubTypeIsMiPyme()
		{
			var testCasesComplianceSubTypes = new (ZString ComplianceSubType, bool ExpectedResult)[]
			{
				(null, false),
				(ZString.Empty, false),
				("XXX", false ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDC, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCC, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXB, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXC, true ),
			};

			foreach (var (complianceSubtype, expectedResult) in testCasesComplianceSubTypes)
			{
				AssertEquals(expectedResult, ArgentinaEInvoiceHelper.IsMiPymeComplianceSubType(complianceSubtype));
			}
		}

		public void TestComplianceSubTypeIsMiPymeDebitOrCreditNote()
		{
			var testCasesComplianceSubTypes = new (ZString ComplianceSubType, bool ExpectedResult)[]
			{
				(null, false),
				(ZString.Empty, false),
				("XXX", false ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDC, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCC, true ),
			};

			foreach (var (complianceSubtype, expectedResult) in testCasesComplianceSubTypes)
			{
				AssertEquals(expectedResult, ArgentinaEInvoiceHelper.IsMiPymeDebitOrCreditNoteComplianceSubType(complianceSubtype));
			}
		}

		public void TestComplianceSubTypeIsMiPymeInvoice()
		{
			var testCasesComplianceSubTypes = new (ZString ComplianceSubType, bool ExpectedResult)[]
			{
				(null, false),
				(ZString.Empty, false),
				("XXX", false ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXB, true ),
				(ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXC, true ),
			};

			foreach (var (complianceSubtype, expectedResult) in testCasesComplianceSubTypes)
			{
				AssertEquals(expectedResult, ArgentinaEInvoiceHelper.IsMiPymeComplianceSubType(complianceSubtype) && !ArgentinaEInvoiceHelper.IsMiPymeDebitOrCreditNoteComplianceSubType(complianceSubtype));
			}
		}

		public void TestUniqueAccountNumberForLoginCompany_CheckNullsAndEmptys()
		{
			var (branch, bank, organization) = TestData();

			var expectedResult = ZString.Empty;

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertCBUNumber(transaction, expectedResult);

			transaction.OrganizationAddress = null;
			AssertCBUNumber(transaction, expectedResult);

			transaction.OrganizationAddress = new OrganizationAddress();
			AssertCBUNumber(transaction, expectedResult);

			transaction.OrganizationAddress.OrganizationCode = null;
			AssertCBUNumber(transaction, expectedResult);

			transaction.OrganizationAddress.OrganizationCode = ZString.Empty;
			AssertCBUNumber(transaction, expectedResult);

			transaction.OrganizationAddress.OrganizationCode = organization.OH_Code;
			transaction.Branch = null;
			AssertCBUNumber(transaction, expectedResult);

			transaction.Branch = new Branch();
			AssertCBUNumber(transaction, expectedResult);

			transaction.Branch.Code = null;
			AssertCBUNumber(transaction, expectedResult);

			transaction.Branch.Code = ZString.Empty;
			AssertCBUNumber(transaction, expectedResult);

			transaction.OSCurrency = null;
			AssertCBUNumber(transaction, expectedResult);

			transaction.OSCurrency = new Currency();
			AssertCBUNumber(transaction, expectedResult);

			transaction.OSCurrency.Code = null;
			AssertCBUNumber(transaction, expectedResult);

			transaction.OSCurrency.Code = ZString.Empty;
			AssertCBUNumber(transaction, expectedResult);

			transaction.OSCurrency.Code = bank.AB_RX_NKAccountCurrency;
			transaction.Branch = null;
			AssertCBUNumber(transaction, expectedResult);

			transaction.Branch = new Branch();
			AssertCBUNumber(transaction, expectedResult);

			transaction.Branch.Code = null;
			AssertCBUNumber(transaction, expectedResult);

			transaction.Branch.Code = ZString.Empty;
			AssertCBUNumber(transaction, expectedResult);

			expectedResult = "1111111199999999988888";

			transaction.Branch.Code = branch.GB_Code;
			transaction.OSCurrency = null;
			AssertCBUNumber(transaction, expectedResult);

			transaction.OSCurrency = new Currency();
			AssertCBUNumber(transaction, expectedResult);

			transaction.OSCurrency.Code = null;
			AssertCBUNumber(transaction, expectedResult);

			transaction.OSCurrency.Code = ZString.Empty;
			AssertCBUNumber(transaction, expectedResult);
		}

		public void TestUniqueAccountNumberForLoginCompany_HasBankAccount()
		{
			var (branch, bank, organization) = TestData();

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress();
			transaction.OSCurrency = new Currency();
			transaction.Branch = new Branch();

			AssertUniqueAccountNumber(false);

			bank.AB_FullAccountNumber = ZString.Empty;
			Factory.Save();

			AssertUniqueAccountNumber(true);

			void AssertUniqueAccountNumber(bool isEmptyUniqueAccountNumber)
			{
				var expectedResult = ZString.Empty;

				if (!isEmptyUniqueAccountNumber)
				{
					expectedResult = "1111111199999999988888";
				}

				transaction.OrganizationAddress.OrganizationCode = organization.OH_Code;
				transaction.OSCurrency.Code = bank.AB_RX_NKAccountCurrency;
				transaction.Branch.Code = branch.GB_Code;
				AssertCBUNumber(transaction, expectedResult);

				transaction.OSCurrency.Code = "---";
				AssertCBUNumber(transaction, expectedResult);

				transaction.OSCurrency.Code = "EUR";
				AssertCBUNumber(transaction, expectedResult);

				expectedResult = string.Empty;

				transaction.Branch.Code = "ZXY";
				transaction.OSCurrency.Code = bank.AB_RX_NKAccountCurrency;
				AssertCBUNumber(transaction, expectedResult);

				transaction.OSCurrency.Code = "---";
				AssertCBUNumber(transaction, expectedResult);

				transaction.OSCurrency.Code = "EUR";
				AssertCBUNumber(transaction, expectedResult);

				transaction.OrganizationAddress.OrganizationCode = "XXXXX";
				AssertCBUNumber(transaction, expectedResult);
			}
		}

		public void TestUniqueAccountNumberForLoginCompany_HasNoBankAccount()
		{
			var (branch, bank, organization) = TestData();
			bank.Delete();
			Factory.Save();

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			transaction.OrganizationAddress = new OrganizationAddress() { OrganizationCode = organization.OH_Code };
			transaction.OSCurrency = new Currency() { Code = "AUD" };
			transaction.Branch = new Branch() { Code = branch.GB_Code };

			AssertCBUNumber(transaction, ZString.Empty);
		}

		void AssertCBUNumber(TransactionInfo transaction, ZString expectedResult)
		{
			var actualResult = ArgentinaEInvoiceHelper.GetBankAccountCBUNumber(transaction, Factory);
			AssertEquals(expectedResult, actualResult);
		}

		(GlbBranch branch, AccBankAccount bank, OrgHeader organization) TestData()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR1";

			var bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_GB = branch.PK;
			bank.AB_GC = branch.Company.PK;
			bank.AB_RX_NKAccountCurrency = branch.Company.GC_RX_NKLocalCurrency;
			bank.AB_IsDefaultReceiptBankAccount = true;
			bank.AB_IsActive = true;
			bank.AB_FullAccountNumber = "1111111199999999988888";

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "Debtor";

			Factory.Save();

			return (branch, bank, organization);
		}

		public void TestGetMiPymeOriginalTransactionIsRejectedByBuyer()
		{
			var transaction = new TransactionInfo();
			transaction.OriginalReference = null;
			AssertEquals("N", ArgentinaEInvoiceHelper.GetMiPymeOriginalTransactionIsRejectedByBuyer(transaction));

			transaction.OriginalReference = new OriginalReference();
			AssertEquals("N", ArgentinaEInvoiceHelper.GetMiPymeOriginalTransactionIsRejectedByBuyer(transaction));

			transaction.OriginalReference.OriginalTransactionAmendingReversingReason = new CodeDescriptionPair();
			AssertEquals("N", ArgentinaEInvoiceHelper.GetMiPymeOriginalTransactionIsRejectedByBuyer(transaction));

			transaction.OriginalReference.OriginalTransactionAmendingReversingReason.Code = null;
			AssertEquals("N", ArgentinaEInvoiceHelper.GetMiPymeOriginalTransactionIsRejectedByBuyer(transaction));

			transaction.OriginalReference.OriginalTransactionAmendingReversingReason.Code = ZString.Empty;
			AssertEquals("N", ArgentinaEInvoiceHelper.GetMiPymeOriginalTransactionIsRejectedByBuyer(transaction));

			transaction.OriginalReference.OriginalTransactionAmendingReversingReason.Code = "TXT";
			AssertEquals("N", ArgentinaEInvoiceHelper.GetMiPymeOriginalTransactionIsRejectedByBuyer(transaction));

			transaction.OriginalReference.OriginalTransactionAmendingReversingReason.Code = "MIR";
			AssertEquals("S", ArgentinaEInvoiceHelper.GetMiPymeOriginalTransactionIsRejectedByBuyer(transaction));
		}

		#endregion

		#region GetTotalsTaxAmountFromTransactionInfo

		public void TestGetTotalsTaxAmountFromTransactionInfo_PostingJournalCollectionIsMissing()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionType = TransactionType.INV;
			AssertTotalesTaxAmountFromTransactionInfo(transaction, 0, 0, 0);
		}

		public void TestGetTotalsTaxAmountFromTransactionInfo_PostingJournalCollectionIsEmpty()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionType = TransactionType.INV;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			AssertTotalesTaxAmountFromTransactionInfo(transaction, 0, 0, 0);
		}

		public void TestGetTotalsTaxAmountFromTransactionInfo_HasChargeLines()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionType = TransactionType.INV;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			transaction.PostingJournalCollection.Add(AddTransactionLine(1200m, 126m, 1326m, 1200m, 126m, 1326m));
			transaction.PostingJournalCollection.Add(AddTransactionLine(853.64m, 179.26m, 1032.9m, 853.64m, 179.26m, 1032.9m, AddTaxID("IVA", 21, "RAT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(1000.2m, 0m, 1000.2m, 1000.2m, 0m, 1000.2m, AddTaxID("EXEMPT", 0, "EXT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(1321.27m, 277.47m, 1598.74m, 1321.27m, 277.47m, 1598.74m, AddTaxID("CAPIVA", 21, "CAP")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(560m, 0m, 560m, 560m, 0m, 560m, AddTaxID("NOTREPORT", 0, "NOT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(300m, 0m, 300m, 300m, 0m, 300m, AddTaxID("EXCLUDE", 0, "EXL")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(982.36m, 265.24m, 1247.6m, 982.36m, 265.24m, 1247.6m, AddTaxID("IVA27", 27, "RAT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(500m, 52.5m, 552.5m, 500m, 52.5m, 552.5m, AddTaxID("IVA10.5", 10.5, "RAT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(1032.9m, 0m, 1032.9m, 1032.9m, 0m, 1032.9m, AddTaxID("FREEIVA", 0, "RAT")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(333m, 0m, 333m, 333, 0m, 333m, AddTaxID("EXEMPT", 0, "")));
			transaction.PostingJournalCollection.Add(AddTransactionLine(10m, 0m, 10m, 10m, 0m, 10m, AddTaxID("IVA10.5", 10.5, null)));
			AssertTotalesTaxAmountFromTransactionInfo(transaction, 4690.17m, 860m, 1000.20m);

			transaction.TransactionType = TransactionType.CRD;
			transaction.PostingJournalCollection.ForEach(x => ChangeSignTransactionLine(x));
			AssertTotalesTaxAmountFromTransactionInfo(transaction, 4690.17m, 860m, 1000.20m, -1);
		}

		void AssertTotalesTaxAmountFromTransactionInfo(TransactionInfo transaction, ZDecimal expectedTotalSubjectToVAT, ZDecimal expectedTotalNotSubjectToVAT, ZDecimal expectedTotalExemptVAT, int multiplier = 1)
		{
			(ZDecimal actualTotalSubjectToVAT, ZDecimal actualTotalNotSubjectToVAT, ZDecimal actualTotalExemptVAT) = ArgentinaEInvoiceHelper.GetTotalsTaxAmountFromTransactionInfo(transaction, multiplier);
			AssertEquals("Total Subject To VAT must be equal to the expected value", expectedTotalSubjectToVAT, actualTotalSubjectToVAT);
			AssertEquals("Total Not Subject To VAT must be equal to the expected value", expectedTotalNotSubjectToVAT, actualTotalNotSubjectToVAT);
			AssertEquals("Total Exempt To VAT must be equal to the expected value", expectedTotalExemptVAT, actualTotalExemptVAT);
		}

		PostingJournal AddTransactionLine(ZDecimal osAmount, ZDecimal osGSTVATAmount, ZDecimal osTotalAmount, ZDecimal localAmount, ZDecimal localGSTVATAmount, ZDecimal localTotalAmount, TaxID taxID = null, TaxMessageID taxMessageID = null)
		{
			var transactionLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OSAmount = osAmount,
				OSGSTVATAmount = osGSTVATAmount,
				OSTotalAmount = osTotalAmount,
				LocalAmount = localAmount,
				LocalGSTVATAmount = localGSTVATAmount,
				LocalTotalAmount = localTotalAmount
			};
			transactionLine.VATTaxID = taxID;
			transactionLine.TaxMessageID = taxMessageID;

			return transactionLine;
		}

		TaxID AddTaxID(ZString? taxCode, ZDecimal? taxRate, ZString? taxTypecode)
		{
			var taxID = new TaxID();
			taxID.TaxCode = taxCode;
			taxID.TaxRate = taxRate;
			taxID.TaxType = new CodeDescriptionPair() { Code = taxTypecode };
			return taxID;
		}

		void ChangeSignTransactionLine(PostingJournal postingJournal)
		{
			postingJournal.OSAmount = (-1) * postingJournal.OSAmount;
			postingJournal.OSGSTVATAmount = (-1) * postingJournal.OSGSTVATAmount;
			postingJournal.OSTotalAmount = (-1) * postingJournal.OSTotalAmount;
			postingJournal.LocalAmount = (-1) * postingJournal.LocalAmount;
			postingJournal.LocalGSTVATAmount = (-1) * postingJournal.LocalGSTVATAmount;
			postingJournal.LocalTotalAmount = (-1) * postingJournal.LocalTotalAmount;
		}

		#endregion

		public void TestGetBranchCompanyAndDepartamentPKFromTransactionInfo()
		{
			CombineAssertions(() =>
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				AssertResult(transaction, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);

				transaction.Branch = null;
				transaction.Department = null;
				AssertResult(transaction, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);

				var company = Factory.NewWithValidTestData<GlbCompany>();
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = company.PK;
				var department = Factory.NewWithValidTestData<GlbDepartment>();

				transaction.Branch = new Branch() { Code = "BUE" };
				transaction.Branch.Code = branch.GB_Code;
				transaction.Department = new Department() { Code = "BUE" };
				transaction.Department.Code = department.GE_Code;

				AssertResult(transaction, branch.PK, company.PK, department.PK);

				void AssertResult(TransactionInfo transactionInfo, ZGuid expectedbranchPK, ZGuid expectedcompanyPK, ZGuid expecteddepartamentPK)
				{
					var result = ArgentinaEInvoiceHelper.GetBranchCompanyAndDepartamentPKFromTransactionInfo(transaction, Factory);
					AssertEquals(result.branchPK, expectedbranchPK);
					AssertEquals(result.companyPK, expectedcompanyPK);
					AssertEquals(result.deparmentPK, expecteddepartamentPK);
				}
			});
		}

		public void TestGetCondicionIvaReceptor()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertResult(null);

			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			AssertResult(null);

			transactionInfo.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			AssertResult(null);

			var invalidRegistrationNumber = new RegistrationNumber();
			transactionInfo.OrganizationAddress.RegistrationNumberCollection.Add(invalidRegistrationNumber);
			AssertResult(null);

			invalidRegistrationNumber.CountryOfIssue = new Country();
			AssertResult(null);

			invalidRegistrationNumber.CountryOfIssue.Code = string.Empty;
			AssertResult(null);

			invalidRegistrationNumber.CountryOfIssue.Code = Constants.CountryCodes.Argentina;
			AssertResult(null);

			invalidRegistrationNumber.Type = new RegistrationNumberType();
			AssertResult(null);

			invalidRegistrationNumber.Type.Code = string.Empty;
			AssertResult(null);

			invalidRegistrationNumber.Type.Code = "XXX";
			AssertResult(null);

			invalidRegistrationNumber.Type.Code = "IVX";
			invalidRegistrationNumber.CountryOfIssue.Code = "XX";
			AssertResult(null);

			var validRegistrationNumber = new RegistrationNumber
			{
				CountryOfIssue = new Country { Code = Constants.CountryCodes.Argentina },
				Type = new RegistrationNumberType { Code = "IVI" }
			};
			transactionInfo.OrganizationAddress.RegistrationNumberCollection.Add(validRegistrationNumber);
			AssertResult(1);

			validRegistrationNumber.Type.Code = "IVE";
			AssertResult(4);

			validRegistrationNumber.Type.Code = "IVF";
			AssertResult(5);

			validRegistrationNumber.Type.Code = "IVM";
			AssertResult(6);

			validRegistrationNumber.Type.Code = "IVS";
			AssertResult(7);

			validRegistrationNumber.Type.Code = "IVP";
			AssertResult(8);

			validRegistrationNumber.Type.Code = "IVX";
			AssertResult(9);

			void AssertResult(short? expectedResult)
			{
				var result = ArgentinaEInvoiceHelper.GetCondicionIvaReceptor(transactionInfo);
				AssertEquals(expectedResult, result);
			}
		}

		IArgentinaEInvoiceHelper ArgentinaEInvoiceHelper => new ArgentinaEInvoiceHelper();
	}
}
