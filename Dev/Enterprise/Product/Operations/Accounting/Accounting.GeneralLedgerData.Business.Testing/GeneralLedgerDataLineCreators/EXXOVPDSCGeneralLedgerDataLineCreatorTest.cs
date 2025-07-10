using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class EXXOVPDSCGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		[TestDate(2023, 6, 30, 0, 0, 0)]
		public void TestCreateDRCREntries_EXXOVPDSC()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			CombineAssertions(() =>
			{
				AssertCreateDRCREntries_EXXOVPDSC(LedgerTypes.AccountsPayable, TransactionTypes.ExchangeDifference);
				AssertCreateDRCREntries_EXXOVPDSC(LedgerTypes.AccountsPayable, TransactionTypes.Overpayment);
				AssertCreateDRCREntries_EXXOVPDSC(LedgerTypes.AccountsPayable, TransactionTypes.Discount);
				AssertCreateDRCREntries_EXXOVPDSC(LedgerTypes.AccountsReceivable, TransactionTypes.ExchangeDifference);
				AssertCreateDRCREntries_EXXOVPDSC(LedgerTypes.AccountsReceivable, TransactionTypes.Overpayment);
				AssertCreateDRCREntries_EXXOVPDSC(LedgerTypes.AccountsReceivable, TransactionTypes.Discount);
			});
		}

		void AssertCreateDRCREntries_EXXOVPDSC(ZString ledger, ZString transactionType)
		{
			SetControlAccount(ledger, enable: true);

			var header = GetTransactionHeader(ledger, transactionType);

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();

			header.AH_AG = glHeader.PK;

			var creator = new EXXOVPDSCGeneralLedgerDataLineCreatorForTest();
			var entry = creator.CreateDRCREntries(((INeedRow)header).Row);
			var controlAccount = creator.GetControlAccountForTest(header.AH_Ledger);
			var controlAccountType = creator.GetContorlAccountTypeTest(header.AH_Ledger);

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = controlAccount.PK, LocalAmount = 100m, OSAmount = 110m, DRCRSign = DebitCredit.DR, GLDAccountType =  controlAccountType },
				new DebitCreditEntryItem { AccountPK = glHeader.PK, LocalAmount = -100m, OSAmount = -110m, DRCRSign = DebitCredit.CR, GLDAccountType =  GLDAccountTypes.TransactionHeaderGLAccount },
			};
			foreach (var line in expectedDRCRLines)
			{
				line.JournalDate = header.AH_PostDate;
				line.GLDType = AccountingConstants.GLDTypeCodes.Posting;
				line.Period = new AccountingPeriodCalculator(Factory, GlbCompany.CurrentCompany).GetPeriodFromDate(header.AH_PostDate.ToDateTime());
			}
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		public override void TestHasValidControlAccount()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			CombineAssertions(() =>
			{
				AssertHasValidControlAccount(LedgerTypes.AccountsPayable, TransactionTypes.ExchangeDifference);
				AssertHasValidControlAccount(LedgerTypes.AccountsPayable, TransactionTypes.Overpayment);
				AssertHasValidControlAccount(LedgerTypes.AccountsPayable, TransactionTypes.Discount);
				AssertHasValidControlAccount(LedgerTypes.AccountsReceivable, TransactionTypes.ExchangeDifference);
				AssertHasValidControlAccount(LedgerTypes.AccountsReceivable, TransactionTypes.Overpayment);
				AssertHasValidControlAccount(LedgerTypes.AccountsReceivable, TransactionTypes.Discount);
			});
		}

		void AssertHasValidControlAccount(ZString ledger, ZString transactionType)
		{
			SetControlAccount(ledger, enable: false);

			var header = GetTransactionHeader(ledger, transactionType);

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();

			header.AH_AG = glHeader.PK;

			var creator = new EXXOVPDSCGeneralLedgerDataLineCreatorForTest();
			var controlAccount = creator.GetControlAccountForTest(header.AH_Ledger);

			AssertNull("ControlAccount not set", controlAccount);

			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				creator.CreateDRCREntries(((INeedRow)header).Row);
			});

			SetControlAccount(ledger, enable: true);
			controlAccount = creator.GetControlAccountForTest(header.AH_Ledger);

			AssertNotNull("ControlAccount set", controlAccount);
			AssertNoExceptionThrown(() =>
			{
				creator.CreateDRCREntries(((INeedRow)header).Row);
			});
		}

		void SetControlAccount(ZString ledger, bool enable = true)
		{
			var testAccountPk = enable ? TestObjectCreator.GLHeaderNTE1.PK.ToGuid() : Guid.Empty;

			if (ledger == LedgerTypes.AccountsPayable)
			{
				AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testAccountPk);
			}
			else if (ledger == LedgerTypes.AccountsReceivable)
			{
				AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testAccountPk);
			}
		}

		TransactionHeader GetTransactionHeader(ZString ledger, ZString transactionType)
		{
			TransactionHeader header;

			if (transactionType == TransactionTypes.ExchangeDifference && ledger == LedgerTypes.AccountsPayable)
			{
				header = Factory.NewWithValidTestData<APExchangeDifference>();
			}
			else if (transactionType == TransactionTypes.ExchangeDifference && ledger == LedgerTypes.AccountsReceivable)
			{
				header = Factory.NewWithValidTestData<ARExchangeDifference>();
			}
			else if (transactionType == TransactionTypes.Overpayment && ledger == LedgerTypes.AccountsPayable)
			{
				header = Factory.NewWithValidTestData<APOverpayment>();
			}
			else if (transactionType == TransactionTypes.Overpayment && ledger == LedgerTypes.AccountsReceivable)
			{
				header = Factory.NewWithValidTestData<AROverpayment>();
			}
			else if (transactionType == TransactionTypes.Discount && ledger == LedgerTypes.AccountsPayable)
			{
				header = Factory.NewWithValidTestData<APDiscount>();
			}
			else if (transactionType == TransactionTypes.Discount && ledger == LedgerTypes.AccountsReceivable)
			{
				header = Factory.NewWithValidTestData<ARDiscount>();
			}
			else
			{
				header = Factory.NewWithValidTestData<TransactionHeader>();
			}

			header.AH_Ledger = ledger;
			header.AH_TransactionType = transactionType;
			header.AH_InvoiceAmount = header.AH_OSTotal = 0;
			header.AH_PostDate = new ZDateTime(2023, 3, 1);
			header.AH_GB = GlbBranch.CurrentBranch.PK.ToGuid();
			header.AH_GE = GlbDepartment.CurrentDepartment.PK.ToGuid();
			header.AH_PostToGL = "Y";
			header.AH_DueDate = new ZDateTime(2004, 10, 1).ToDateTime();
			header.AH_InvoiceAmount = 100m;
			header.AH_OSTotal = 110m;

			return header;
		}

		public void TestGLDAccountTypes()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			CombineAssertions(() =>
			{
				AssertGLDAccountTypes_EXXOVPDSC(LedgerTypes.AccountsPayable, TransactionTypes.ExchangeDifference);
				AssertGLDAccountTypes_EXXOVPDSC(LedgerTypes.AccountsPayable, TransactionTypes.Overpayment);
				AssertGLDAccountTypes_EXXOVPDSC(LedgerTypes.AccountsPayable, TransactionTypes.Discount);
				AssertGLDAccountTypes_EXXOVPDSC(LedgerTypes.AccountsReceivable, TransactionTypes.ExchangeDifference);
				AssertGLDAccountTypes_EXXOVPDSC(LedgerTypes.AccountsReceivable, TransactionTypes.Overpayment);
				AssertGLDAccountTypes_EXXOVPDSC(LedgerTypes.AccountsReceivable, TransactionTypes.Discount);
			});
		}

		void AssertGLDAccountTypes_EXXOVPDSC(ZString ledger, ZString transactionType)
		{
			SetControlAccount(ledger, enable: true);

			var header = GetTransactionHeader(ledger, transactionType);

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();

			header.AH_AG = glHeader.PK;

			var creator = new EXXOVPDSCGeneralLedgerDataLineCreatorForTest();
			var entry = creator.CreateDRCREntries(((INeedRow)header).Row);
			var controlAccountType = creator.GetContorlAccountTypeTest(header.AH_Ledger);

			AssertEquals(controlAccountType, entry.EntryItems[0].GLDAccountType);
			AssertEquals(GLDAccountTypes.TransactionHeaderGLAccount, entry.EntryItems[1].GLDAccountType);
		}

		class EXXOVPDSCGeneralLedgerDataLineCreatorForTest : EXXOVPDSCGeneralLedgerDataLineCreator
		{
			public EXXOVPDSCGeneralLedgerDataLineCreatorForTest() : base()
			{
			}

			public AccGLHeader GetControlAccountForTest(ZString ledger)
			{
				return GetControlAccount(ledger);
			}

			public ZString GetContorlAccountTypeTest(ZString ledger)
			{
				return GetContorlAccountType(ledger);
			}
		}
	}
}
