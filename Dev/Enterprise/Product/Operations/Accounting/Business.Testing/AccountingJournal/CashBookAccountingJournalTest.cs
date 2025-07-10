using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CashBookAccountingJournal))]
	public class CashBookAccountingJournalTest : AccountingJournalTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			DirectReceipt directReceipt = Factory.NewWithValidTestData<DirectReceipt>();
			return new CashBookAccountingJournal(directReceipt, ReadonlyFactory);
		}

		protected override AccountingJournal CreateTestAccountingJournal()
		{
			var receipt = Creator.CreateDirectReceipt(ZDateTime.Today, 2500m, 250m, 3500m, 350m);
			Creator.Factory.Save();
			transaction = receipt;
			return new CashBookAccountingJournal(receipt, ReadonlyFactory);
		}

		public override void TestAJOptionalFields()
		{
			var directReceipt = Creator.CreateDirectReceipt(ZDateTime.Today, 150m, 50m, 250m, 50m);
			directReceipt.AH_AB = Creator.AUDBankAccount2.PK;
			directReceipt.AH_GB_TaxBranch = Creator.NonCurrentBranch.PK;
			Factory.Save();

			var aj = new CashBookAccountingJournal(directReceipt, ReadonlyFactory);
			AssertEquals("Optional Field Count", 2, aj.ApplicableOptionalFields.Count);
			AssertEquals("BANK_CODE", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.BankCodeText));
			AssertEquals("BANK_CODE Value", Creator.AUDBankAccount2.AB_BankAbbreviation, aj.ApplicableOptionalFields[AccountingJournal.BankCodeText]);
			AssertEquals("TAXBRANCH", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.TaxBranchText));
			AssertEquals("TAXBRANCH Value", Creator.NonCurrentBranch.GB_Code, aj.ApplicableOptionalFields[AccountingJournal.TaxBranchText]);

			var directPayment = Creator.CreateDirectPayment(ZDateTime.Today, 150m, 50m, 250m, 50m);
			directPayment.AH_AB = Creator.AUDBankAccount.PK;
			directPayment.AH_GB_TaxBranch = Creator.NonCurrentBranch.PK;
			Factory.Save();

			aj = new CashBookAccountingJournal(directPayment, ReadonlyFactory);
			AssertEquals("Optional Field Count", 2, aj.ApplicableOptionalFields.Count);
			AssertEquals("BANK_CODE", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.BankCodeText));
			AssertEquals("BANK_CODE Value", Creator.AUDBankAccount2.AB_BankAbbreviation, aj.ApplicableOptionalFields[AccountingJournal.BankCodeText]);
			AssertEquals("TAXBRANCH", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.TaxBranchText));
			AssertEquals("TAXBRANCH Value", Creator.NonCurrentBranch.GB_Code, aj.ApplicableOptionalFields[AccountingJournal.TaxBranchText]);

			var exchangeDifference = Creator.CreateExchangeDifference<ARExchangeDifference>(0m, ZDateTime.Today, Creator.ABIGAS.PK);
			exchangeDifference.AH_AB = Creator.AUDBankAccount.PK;
			Factory.Save();

			aj = new CashBookAccountingJournal(directPayment, ReadonlyFactory);
			AssertEquals("Optional Field Count", 2, aj.ApplicableOptionalFields.Count);
			AssertEquals("BANK_CODE", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.BankCodeText));
			AssertEquals("BANK_CODE Value", Creator.AUDBankAccount2.AB_BankAbbreviation, aj.ApplicableOptionalFields[AccountingJournal.BankCodeText]);
		}

		public override void TestAccountingJournalLines()
		{
			Assert(true);
		}

		public void TestALDescForReportingBook()
		{
			var factory = new ReadOnlyBusinessObjectFactory();
			var reportingBook = factory.New<AccReportingBook>();
			var creator = new TestObjectCreator(factory);
			var directReceipt = creator.CreateDirectReceipt(ZDateTime.Today, 150m, 50m, 250m, 50m);
			directReceipt.AH_AB = creator.AUDBankAccount.PK;
			directReceipt.AH_GB_TaxBranch = creator.NonCurrentBranch.PK;
			directReceipt.AH_Desc = "desc";
			Factory.Save();

			var line = directReceipt.Lines.AddNew();
			line.AL_Desc = "line desc";

			var dataTable = new System.Data.DataTable("GeneralLedgerTransactionData");
			dataTable.Columns.Add("TransactionHeaderID", typeof(Guid));
			dataTable.Columns.Add("TransactionLineID", typeof(Guid));
			dataTable.Columns.Add("TaxGLMovementKey", typeof(Guid));
			dataTable.Columns.Add("GLType", typeof(string));
			var row = dataTable.Rows.Add();
			row["TransactionHeaderID"] = directReceipt.PK.ToGuid();
			row["TransactionLineID"] = line.PK.ToGuid();
			var accountingJournal = new CashBookAccountingJournal(directReceipt, factory, reportingBook, dataTable);
			Assert(accountingJournal.Lines.Cast<AccountingJournalLine>().All(x => x.AL_Desc == ZString.Empty));
		}

		public void TestAJLine_Transfer_HighPrecisionExchangeRate()
		{
			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_IsReciprocal = true;
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_Name = "Your US Company";
			usCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var chicagoBranch = Factory.NewWithValidTestData<GlbBranch>();
			chicagoBranch.GB_GC = usCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, chicagoBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var transfer = Creator.CreateBankTransfer(ZDateTime.Today, Creator.AUDBankAccount.PK, Creator.USDBankAccount.PK, 0m, 1m);
				transfer.SellExchangeRate = 0.1274m;
				transfer.SellAmount = 31757.72m;
				Factory.Save();

				var bankTransferRow = transfer.TransferRowFrom;
				var journal = new CashBookAccountingJournal(bankTransferRow, ReadonlyFactory);
				var lines = journal.Lines.OfType<AccountingJournalLine>().ToArray();

				AssertEquals("Lines Count", 2, lines.Length);

				AssertEquals(-31757.72m, lines[0].AL_OSExTaxAmount);
				AssertEquals(4045.93m, lines[1].AL_OSExTaxAmount);
			}
		}

		public void TestAJLine_ExchangeRateDifference_HighPrecisionExchangeRate()
		{
			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_IsReciprocal = true;
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_Name = "Your US Company";
			usCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var chicagoBranch = Factory.NewWithValidTestData<GlbBranch>();
			chicagoBranch.GB_GC = usCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, chicagoBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			using (AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid()))
			{
				Creator.AUDBankAccount.Factory.Save();
				AssertEquals("Bank account will be reloaded when save EXX with valid post date & bank account. Hence it must be in DB already.", true, Creator.AUDBankAccount.IsInDatabase);

				var exchangeDifference = Creator.CreateCashbookExchangeDifference(ZDateTime.Today, 0m, Creator.AUDBankAccount);
				exchangeDifference.AH_ExchangeRate = 0.1274m;
				exchangeDifference.AH_LocalExTaxAmount = 4045.93m;
				exchangeDifference.AH_OSExTaxAmount = 31757.72m;

				var cbJournal = new CashBookAccountingJournal(exchangeDifference, ReadonlyFactory);
				var lines = cbJournal.Lines.OfType<AccountingJournalLine>().ToArray();

				AssertEquals("Lines Count", 2, lines.Length);

				AssertEquals(-31757.72m, lines[0].AL_OSExTaxAmount);
				AssertEquals(31757.72m, lines[1].AL_OSExTaxAmount);
			}
		}

		public void TestAJLines_DRC()
		{
			Creator.CreateGLHeaderSubAccount(Creator.GLHeader1, OrgHeaderSchema.Constants.Prefix, false);
			Creator.CreateGLHeaderSubAccount(Creator.GLHeader2, OrgHeaderSchema.Constants.Prefix, false);
			var receipt = Creator.CreateDirectReceipt(ZDateTime.Today, 2500m, 250m, 3500m, 0m);
			SetupSubAccount(receipt.Lines[0].SubAccounts, OrgHeaderSchema.Constants.Prefix, Creator.ABIGAS.PK);
			SetupSubAccount(receipt.Lines[1].SubAccounts, OrgHeaderSchema.Constants.Prefix, Creator.AALSHI.PK);
			Factory.Save();

			var cbJournal = new CashBookAccountingJournal(receipt, ReadonlyFactory);
			var lines = cbJournal.Lines.Cast<AccountingJournalLine>().ToArray();

			AssertEquals("Lines Count", 6, lines.Length);

			AssertAccountingJournalLine(lines[0], "Line 1 glLine",
										receipt.Lines[0].AL_AG, receipt.Lines[0].GLHeader.AG_Description,
										"ORG: ABIGAS",
										receipt.Lines[0].AL_GC, receipt.Lines[0].AL_GB, receipt.Lines[0].AL_GE,
										-2500m, -2500m, "AUD",
										ZDateTime.Today, receipt.Lines[0].AL_PostPeriod);

			AssertAccountingJournalLine(lines[1], "Line 1 bankAccountLine",
										receipt.BankAccount.AB_AG, receipt.BankAccount.GLHeader.AG_Description,
										ZString.Empty,
										receipt.Lines[0].AL_GC, receipt.Lines[0].AL_GB, receipt.Lines[0].AL_GE,
										2500m, 2500m, "AUD",
										ZDateTime.Today, receipt.Lines[0].AL_PostPeriod);

			var vatControlAccount = Factory.Load<AccGLHeader>(new ZGuid(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value));
			AssertAccountingJournalLine(lines[2], "Line 1 vatLine",
										AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value, vatControlAccount.AG_Description,
										ZString.Empty,
										receipt.Lines[0].AL_GC, receipt.Lines[0].AL_GB, receipt.Lines[0].AL_GE,
										-250m, -250m, "AUD",
										ZDateTime.Today, receipt.Lines[0].AL_PostPeriod);

			AssertAccountingJournalLine(lines[3], "Line 1 bankVATLine",
										receipt.BankAccount.AB_AG, receipt.BankAccount.GLHeader.AG_Description,
										ZString.Empty,
										receipt.Lines[0].AL_GC, receipt.Lines[0].AL_GB, receipt.Lines[0].AL_GE,
										250m, 250m, "AUD",
										ZDateTime.Today, receipt.Lines[0].AL_PostPeriod);

			AssertAccountingJournalLine(lines[4], "Line 2 glLine",
										receipt.Lines[1].AL_AG, receipt.Lines[1].GLHeader.AG_Description,
										"ORG: AALSHI",
										receipt.Lines[1].AL_GC, receipt.Lines[1].AL_GB, receipt.Lines[1].AL_GE,
										-3500m, -3500m, "AUD",
										ZDateTime.Today, receipt.Lines[0].AL_PostPeriod);

			AssertAccountingJournalLine(lines[5], "Line 2 bankAccountLine",
										receipt.BankAccount.AB_AG, receipt.BankAccount.GLHeader.AG_Description,
										ZString.Empty,
										receipt.Lines[1].AL_GC, receipt.Lines[1].AL_GB, receipt.Lines[1].AL_GE,
										3500m, 3500m, "AUD",
										ZDateTime.Today, receipt.Lines[0].AL_PostPeriod);
		}

		public void TestAJLines_DPY()
		{
			Creator.CreateGLHeaderSubAccount(Creator.GLHeader1, OrgHeaderSchema.Constants.Prefix, false);
			Creator.CreateGLHeaderSubAccount(Creator.GLHeader2, OrgHeaderSchema.Constants.Prefix, false);
			var payment = Creator.CreateDirectPayment(ZDateTime.Today, 2500m, 250m, 3500m, 350m);
			SetupSubAccount(payment.Lines[0].SubAccounts, OrgHeaderSchema.Constants.Prefix, Creator.ABIGAS.PK);
			SetupSubAccount(payment.Lines[1].SubAccounts, OrgHeaderSchema.Constants.Prefix, Creator.AALSHI.PK);
			Factory.Save();

			var cbJournal = new CashBookAccountingJournal(payment, ReadonlyFactory);
			var lines = cbJournal.Lines.Cast<AccountingJournalLine>().ToArray();

			AssertEquals("Lines Count", 8, lines.Length);

			AssertAccountingJournalLine(lines[0], "Line 1 glLine",
										payment.Lines[0].AL_AG, payment.Lines[0].GLHeader.AG_Description,
										"ORG: ABIGAS",
										payment.Lines[0].AL_GC, payment.Lines[0].AL_GB, payment.Lines[0].AL_GE,
										2500m, 2500m, "AUD",
										ZDateTime.Today, payment.Lines[0].AL_PostPeriod);

			AssertAccountingJournalLine(lines[1], "Line 1 bankAccountLine",
										payment.BankAccount.AB_AG, payment.BankAccount.GLHeader.AG_Description,
										ZString.Empty,
										payment.Lines[0].AL_GC, payment.Lines[0].AL_GB, payment.Lines[0].AL_GE,
										-2500m, -2500m, "AUD",
										ZDateTime.Today, payment.Lines[0].AL_PostPeriod);

			var vatControlAccount = Factory.Load<AccGLHeader>(new ZGuid(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value));
			AssertAccountingJournalLine(lines[2], "Line 1 vatLine",
										AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value, vatControlAccount.AG_Description,
										ZString.Empty,
										payment.Lines[0].AL_GC, payment.Lines[0].AL_GB, payment.Lines[0].AL_GE,
										250m, 250m, "AUD",
										ZDateTime.Today, payment.Lines[0].AL_PostPeriod);

			AssertAccountingJournalLine(lines[3], "Line 1 bankVATLine",
										payment.BankAccount.AB_AG, payment.BankAccount.GLHeader.AG_Description,
										ZString.Empty,
										payment.Lines[0].AL_GC, payment.Lines[0].AL_GB, payment.Lines[0].AL_GE,
										-250m, -250m, "AUD",
										ZDateTime.Today, payment.Lines[0].AL_PostPeriod);

			AssertAccountingJournalLine(lines[4], "Line 2 glLine",
										payment.Lines[1].AL_AG, payment.Lines[1].GLHeader.AG_Description,
										"ORG: AALSHI",
										payment.Lines[1].AL_GC, payment.Lines[1].AL_GB, payment.Lines[1].AL_GE,
										3500m, 3500m, "AUD",
										ZDateTime.Today, payment.Lines[0].AL_PostPeriod);

			AssertAccountingJournalLine(lines[5], "Line 2 bankAccountLine",
										payment.BankAccount.AB_AG, payment.BankAccount.GLHeader.AG_Description,
										ZString.Empty,
										payment.Lines[1].AL_GC, payment.Lines[1].AL_GB, payment.Lines[1].AL_GE,
										-3500m, -3500m, "AUD",
										ZDateTime.Today, payment.Lines[0].AL_PostPeriod);

			AssertAccountingJournalLine(lines[6], "Line 2 vatLine",
										AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value, vatControlAccount.AG_Description,
										ZString.Empty,
										payment.Lines[1].AL_GC, payment.Lines[1].AL_GB, payment.Lines[1].AL_GE,
										350m, 350m, "AUD",
										ZDateTime.Today, payment.Lines[0].AL_PostPeriod);

			AssertAccountingJournalLine(lines[7], "Line 2 bankVATLine",
										payment.BankAccount.AB_AG, payment.BankAccount.GLHeader.AG_Description,
										ZString.Empty,
										payment.Lines[1].AL_GC, payment.Lines[1].AL_GB, payment.Lines[1].AL_GE,
										-350m, -350m, "AUD",
										ZDateTime.Today, payment.Lines[0].AL_PostPeriod);
		}

		public void TestAL_OSAmountOfAJLines_DRCDPY()
		{
			var payment = Creator.CreateDirectPayment(ZDateTime.Today, 51592M, 0M, 51592M, 0M, Creator.USDBankAccount.PK, 0.0005M);
			Factory.Save();
			payment.Lines[0].AL_OSAmount = 103184200M;

			var cbJournalDPY = new CashBookAccountingJournal(payment, ReadonlyFactory);
			AssertEquals(-103184200M, cbJournalDPY.Lines.OfType<AccountingJournalLine>().ToArray()[0].AL_OSAmount);

			var receipt = Creator.CreateDirectReceipt(ZDateTime.Today, 51592M, 0M, 51592M, 0m, Creator.USDBankAccount.PK, 0.0005M);
			Factory.Save();
			receipt.Lines[0].AL_OSAmount = 103184200M;

			var cbJournalDRC = new CashBookAccountingJournal(receipt, ReadonlyFactory);
			AssertEquals(-103184200M, cbJournalDRC.Lines.OfType<AccountingJournalLine>().ToArray()[0].AL_OSAmount);
		}

		public void TestBranchPKAndDepartmentPKOfAJLines_DRCDPY()
		{
			var payment = Creator.CreateDirectPayment(ZDateTime.Today, 51592M, 0M, 51592M, 0M, Creator.USDBankAccount.PK, 0.0005M);
			payment.AH_GSTAmount = 2m;
			payment.AH_GB = Creator.NonCurrentBranch.PK;
			payment.AH_GE = Creator.NonCurrentDepartment.PK;
			payment.Lines[0].AL_GSTVAT = 1m;
			payment.Lines[1].AL_GSTVAT = 1m;
			Factory.Save();

			var cbJournalDPY = new CashBookAccountingJournal(payment, ReadonlyFactory);
			AssertEquals(4, cbJournalDPY.Lines.Count(x => x.AL_GB == payment.AH_GB && x.AL_GE == payment.AH_GE && x.AL_AG == payment.BankAccount.AB_AG));
			AssertEquals(4, cbJournalDPY.Lines.Count(x => x.AL_GB != payment.AH_GB && x.AL_GE != payment.AH_GE && x.AL_AG != payment.BankAccount.AB_AG));

			var receipt = Creator.CreateDirectReceipt(ZDateTime.Today, 51592M, 0M, 51592M, 0m, Creator.USDBankAccount.PK, 0.0005M);
			receipt.AH_GSTAmount = 2m;
			receipt.Lines[0].AL_GSTVAT = 1m;
			receipt.Lines[1].AL_GSTVAT = 1m;
			receipt.AH_GB = Creator.NonCurrentBranch.PK;
			receipt.AH_GE = Creator.NonCurrentDepartment.PK;
			Factory.Save();

			var cbJournalDRC = new CashBookAccountingJournal(receipt, ReadonlyFactory);
			AssertEquals(4, cbJournalDRC.Lines.Count(x => x.AL_GB == receipt.AH_GB && x.AL_GE == receipt.AH_GE && x.AL_AG == receipt.BankAccount.AB_AG));
			AssertEquals(4, cbJournalDRC.Lines.Count(x => x.AL_GB != receipt.AH_GB && x.AL_GE != receipt.AH_GE && x.AL_AG != receipt.BankAccount.AB_AG));
		}

		public void TestAJLines_DPYForTaxRecNot100Percent()
		{
			Creator.CreateGLHeaderSubAccount(Creator.GLHeader1, OrgHeaderSchema.Constants.Prefix, false);
			Creator.CreateGLHeaderSubAccount(Creator.GLHeader2, OrgHeaderSchema.Constants.Prefix, false);

			AssertAJLines_DPY(0.6m);
			AssertAJLines_DPY(0.7m);
			AssertAJLines_DPY(0.8m);
			AssertAJLines_DPY(0m);
		}

		void AssertAJLines_DPY(ZDecimal inputGSTVATRecoverable)
		{
			var payment = Creator.CreateDirectPayment(ZDateTime.Today, 2500m, 250m, 3500m, 350m);
			payment.Lines[0].AL_InputGSTVATRecoverable = inputGSTVATRecoverable;
			payment.Lines[1].Delete();
			SetupSubAccount(payment.Lines[0].SubAccounts, OrgHeaderSchema.Constants.Prefix, Creator.ABIGAS.PK);
			Factory.Save();

			AssertEquals("Precondition", 1, payment.Lines.Count);
			AssertEquals("Precondition", inputGSTVATRecoverable, payment.Lines[0].AL_InputGSTVATRecoverable);

			var cbJournal = new CashBookAccountingJournal(payment, ReadonlyFactory);
			var lines = cbJournal.Lines.Cast<AccountingJournalLine>().ToArray();

			AssertEquals("Lines Count", 5, lines.Length);

			AssertAccountingJournalLine(lines[0], "Line 1 glLine",
										payment.Lines[0].AL_AG, payment.Lines[0].GLHeader.AG_Description,
										"ORG: ABIGAS",
										payment.Lines[0].AL_GC, payment.Lines[0].AL_GB, payment.Lines[0].AL_GE,
										2500m, 2500m, "AUD",
										ZDateTime.Today, payment.Lines[0].AL_PostPeriod);

			AssertAccountingJournalLine(lines[1], "Line 1 bankAccountLine",
										payment.BankAccount.AB_AG, payment.BankAccount.GLHeader.AG_Description,
										ZString.Empty,
										payment.Lines[0].AL_GC, payment.Lines[0].AL_GB, payment.Lines[0].AL_GE,
										-2500m, -2500m, "AUD",
										ZDateTime.Today, payment.Lines[0].AL_PostPeriod);

			AssertAccountingJournalLine(lines[2], "Line 1 notRecVatLine",
										payment.Lines[0].AL_AG, payment.Lines[0].GLHeader.AG_Description,
										ZString.Empty,
										payment.Lines[0].AL_GC, payment.Lines[0].AL_GB, payment.Lines[0].AL_GE,
										250m * (1m - inputGSTVATRecoverable), 250m * (1m - inputGSTVATRecoverable), "AUD",
										ZDateTime.Today, payment.Lines[0].AL_PostPeriod);

			var vatControlAccount = Factory.Load<AccGLHeader>(new ZGuid(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value));
			AssertAccountingJournalLine(lines[3], "Line 1 vatLine",
										AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value, vatControlAccount.AG_Description,
										ZString.Empty,
										payment.Lines[0].AL_GC, payment.Lines[0].AL_GB, payment.Lines[0].AL_GE,
										250m * inputGSTVATRecoverable, 250m * inputGSTVATRecoverable, "AUD",
										ZDateTime.Today, payment.Lines[0].AL_PostPeriod);

			AssertAccountingJournalLine(lines[4], "Line 1 bankVATLine",
										payment.BankAccount.AB_AG, payment.BankAccount.GLHeader.AG_Description,
										ZString.Empty,
										payment.Lines[0].AL_GC, payment.Lines[0].AL_GB, payment.Lines[0].AL_GE,
										-250m, -250m, "AUD",
										ZDateTime.Today, payment.Lines[0].AL_PostPeriod);
		}

		public void TestAJLines_TRF()
		{
			var transfer = Creator.CreateBankTransfer(ZDateTime.Today, Creator.AUDBankAccount.PK, Creator.AUDBankAccount2.PK, 2500m, 1.0m);
			Factory.Save();

			var bankTransferRow = transfer.TransferRowFrom;
			var cbJournal = new CashBookAccountingJournal(bankTransferRow, ReadonlyFactory);
			var lines = cbJournal.Lines.Cast<AccountingJournalLine>().ToArray();

			AssertEquals("Lines Count", 2, lines.Length);

			AssertAccountingJournalLine(lines[0], "Line 1 FromRow",
										Creator.AUDBankAccount.AB_AG, Creator.AUDBankAccount.GLHeader.AG_Description,
										ZString.Empty,
										bankTransferRow.AH_GC, bankTransferRow.AH_GB, bankTransferRow.AH_GE,
										-2500m, -2500m, "AUD",
										ZDateTime.Today, bankTransferRow.AH_PostPeriod);

			AssertAccountingJournalLine(lines[1], "Line 1 ToRow",
										Creator.AUDBankAccount2.AB_AG, Creator.AUDBankAccount2.GLHeader.AG_Description,
										ZString.Empty,
										bankTransferRow.AH_GC, bankTransferRow.AH_GB, bankTransferRow.AH_GE,
										2500m, 2500m, "AUD",
										ZDateTime.Today, bankTransferRow.AH_PostPeriod);
		}

		public void TestAJLines_EXX_AR()
		{
			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			Creator.USDBankAccount.Factory.Save();
			AssertEquals("Bank account will be reloaded when save EXX with valid post date & bank account. Hence it must be in DB already.", true, Creator.USDBankAccount.IsInDatabase);
			var exchangeDifference = Creator.CreateCashbookExchangeDifference(ZDateTime.Today, 100m, Creator.USDBankAccount);

			var cbJournal = new CashBookAccountingJournal(exchangeDifference, ReadonlyFactory);
			var lines = cbJournal.Lines.Cast<AccountingJournalLine>().ToArray();

			AssertEquals("Lines Count", 2, lines.Length);

			AssertAccountingJournalLine(lines[0], "Line 1 GLAccount",
										AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value, Creator.GLHeader1.AG_Description,
										ZString.Empty,
										exchangeDifference.AH_GC, exchangeDifference.AH_GB, exchangeDifference.AH_GE,
										-100m, 0m, "USD",
										ZDateTime.Today, exchangeDifference.AH_PostPeriod);

			AssertAccountingJournalLine(lines[1], "Line 2 BankAccount",
										exchangeDifference.BankAccount.AB_AG, Creator.USDBankAccount.GLHeader.AG_Description,
										ZString.Empty,
										exchangeDifference.AH_GC, exchangeDifference.AH_GB, exchangeDifference.AH_GE,
										100m, 0m, "USD",
										ZDateTime.Today, exchangeDifference.AH_PostPeriod);
		}

		public void TestMultiSubAccountTypeCode()
		{
			var receipt = Creator.CreateDirectReceipt(ZDateTime.Today, 2500m, 250m, 3500m, 350m);
			var accountingJournalForDRC = GetJournalForMultiSubAccountTypeCode(receipt, LedgerTypes.CashBook, TransactionTypes.DirectReceipt);
			AssertEquals(8, accountingJournalForDRC.Lines.Count());
			AssertEquals(1, accountingJournalForDRC.Lines.Count(x => x.MultiSubAccountTypeCode == "ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1"));

			var payment = Creator.CreateDirectPayment(ZDateTime.Today, 2500m, 250m, 3500m, 350m);
			var accountingJournalForDPY = GetJournalForMultiSubAccountTypeCode(payment, LedgerTypes.CashBook, TransactionTypes.DirectPayment);
			AssertEquals(8, accountingJournalForDPY.Lines.Count());
			AssertEquals(1, accountingJournalForDPY.Lines.Count(x => x.MultiSubAccountTypeCode == "ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1"));

			var transfer = Creator.CreateBankTransfer(ZDateTime.Today, Creator.AUDBankAccount.PK, Creator.AUDBankAccount2.PK, 2500m, 1.0m);
			var accountingJournalForTRF = GetJournalForMultiSubAccountTypeCode(transfer.TransferRowFrom, LedgerTypes.CashBook, TransactionTypes.Transfer, false);

			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			Creator.USDBankAccount.Factory.Save();
			var exchangeDifference = Creator.CreateCashbookExchangeDifference(ZDateTime.Today, 100m, Creator.USDBankAccount);
			var accountingJournalForEXX = GetJournalForMultiSubAccountTypeCode(exchangeDifference, LedgerTypes.CashBook, TransactionTypes.ExchangeDifference, false);
		}

		void AssertAccountingJournalLine(AccountingJournalLine ajLine, ZString lineName,
										ZGuid expectedAL_AG, ZString expectedAL_Desc,
										ZString expectedMultiSubAccountTypeCode,
										ZGuid expectedAL_GC, ZGuid expectedAL_GB, ZGuid expectedAL_GE,
										ZDecimal expectedAL_LineAmount, ZDecimal expectedAL_OSAmount, ZString expectedAL_RX_NKTransactionCurrency,
										ZDateTime expectedAL_PostDate, ZInt expectedAL_PostPeriod)
		{
			AssertEquals(lineName + " AL_AG", expectedAL_AG, ajLine.AL_AG);
			AssertEquals(lineName + " AL_Desc", expectedAL_Desc, ajLine.AL_Desc);
			AssertEquals(lineName + " MultiSubAccountTypeCode", expectedMultiSubAccountTypeCode, ajLine.MultiSubAccountTypeCode);
			AssertEquals(lineName + " AL_GC", expectedAL_GC, ajLine.AL_GC);
			AssertEquals(lineName + " AL_GB", expectedAL_GB, ajLine.AL_GB);
			AssertEquals(lineName + " AL_GE", expectedAL_GE, ajLine.AL_GE);
			AssertEquals(lineName + " AL_LineAmount", expectedAL_LineAmount, ajLine.AL_LineAmount);
			AssertEquals(lineName + " AL_OSAmount", expectedAL_OSAmount, ajLine.AL_OSAmount);
			AssertEquals(lineName + " AL_RX_NKTransactionCurrency", expectedAL_RX_NKTransactionCurrency, ajLine.AL_RX_NKTransactionCurrency);
			AssertEquals(lineName + " AL_PostDate", expectedAL_PostDate, ajLine.AL_PostDate);
			AssertEquals(lineName + " AL_PostPeriod", expectedAL_PostPeriod, ajLine.AL_PostPeriod);
		}
	}
}
