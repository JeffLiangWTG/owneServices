using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocBankAccount))]
	sealed class DocBankAccountTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
					DocBankAccount.New(BankAccount, Factory)
			};
		}

		AccBankAccount BankAccount;
		protected override void SetUp()
		{
			BankAccount = Factory.New<AccBankAccount>();
			base.SetUp();
		}

		#region IBAN
		public void TestIBANPropertyWrapsAB_AccountNumber()
		{
			var docBankAccount = (DocBankAccount)GetDocumentWrappers()[0];

			BankAccount.AB_AccountNumber = "";
			AssertEquals("When BankAccount.AB_AccountNumber is blank, DocBankAccount.IBAN should be blank.", "", docBankAccount.IBAN);
			BankAccount.IBAN = "";
			AssertEquals("When BankAccount.IBAN is blank, DocBankAccount.IBAN should be blank.", "", docBankAccount.IBAN);

			ZString validIBAN = "ES2637011181545485279943";
			BankAccount.AB_AccountNumber = validIBAN;
			AssertEquals("When BankAccount.AB_AccountNumber is set, the same value should appear at DocBankAccount.IBAN.", validIBAN, docBankAccount.IBAN);
			BankAccount.AB_AccountNumber = "";
			BankAccount.IBAN = validIBAN;
			AssertEquals("When BankAccount.IBAN is set, the same value should appear at DocBankAccount.IBAN.", validIBAN, docBankAccount.IBAN);
		}
		#endregion

		public void TestUniqueAccountNumPropertyWrapsAB_FullAccountNumber()
		{
			var docBankAccount = (DocBankAccount)GetDocumentWrappers()[0];

			BankAccount.AB_FullAccountNumber = "";
			AssertEquals("Empty", "", docBankAccount.UniqueAccountNum);

			BankAccount.AB_FullAccountNumber = "CH44319999123000889012";
			AssertEquals("Not empty", "CH44319999123000889012", docBankAccount.UniqueAccountNum);
		}
	}
}
