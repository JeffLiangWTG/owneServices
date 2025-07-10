using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PaymentDetailRetriverTest : TestCaseWithFactory
	{
		public void TestPartyToPayString()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			PaymentDetailRetriever paymentDetailRetriever = new PaymentDetailRetriever(declaration);
			AssertEquals("Importer will pay", JobDeclaration.PaymentMethods.Importer, paymentDetailRetriever.PartyToPayString());

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			AssertEquals("Broker will pay", JobDeclaration.PaymentMethods.Broker, paymentDetailRetriever.PartyToPayString());

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.SecondBroker;
			AssertEquals("Second Broker will pay", JobDeclaration.PaymentMethods.SecondBroker, paymentDetailRetriever.PartyToPayString());

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Cash;
			AssertEquals("Cash will pay", JobDeclaration.PaymentMethods.Cash, paymentDetailRetriever.PartyToPayString());

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.DrawbackClaimant;
			AssertEquals("Drawback claimant will pay", JobDeclaration.PaymentMethods.DrawbackClaimant, paymentDetailRetriever.PartyToPayString());
		}

		public void TestPartyToPayEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			PaymentDetailRetriever paymentDetailRetriever = new PaymentDetailRetriever(declaration);
			AssertEquals("Importer will pay", PaymentParty.Importer, paymentDetailRetriever.PartyToPayEntry);

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.MiscServ.OM_IMEFTBankBSB = "I1001";
			importer.MiscServ.OM_IMEFTBankAccount = "I00010000";
			importer.MiscServ.OM_IMMinEFTAmount = 0;
			importer.MiscServ.OM_IMMaxEFTAmount = 1500;
			importer.MiscServ.OM_IMEftCustomsFromImport = true;

			declaration.JE_OH_Importer = importer.PK;
			declaration.CustomsEntryHeaders.AddNew().CH_TotalPaid = 1000m;
			AssertEquals("Importer will pay", PaymentParty.Importer, paymentDetailRetriever.PartyToPayEntry);

			declaration.CustomsEntryHeaders.AddNew().CH_TotalPaid = 5000m;
			AssertEquals("Broker will pay", PaymentParty.Broker, paymentDetailRetriever.PartyToPayEntry);

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			AssertEquals("Broker will pay", PaymentParty.Broker, paymentDetailRetriever.PartyToPayEntry);
		}

		public void TestGetBankDetails()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			PaymentDetailRetriever paymentDetailRetriever = new PaymentDetailRetriever(declaration);

			AccBankAccount account = Factory.New<AccBankAccount>();
			account.AB_BSB = "B1001";
			account.AB_AccountNum = "B00010000";
			Env.Registry.SetCustomsPaymentBankAccountForCurrentCompany(account.PK.ToGuid());

			AccBankAccount secondAccount = Factory.New<AccBankAccount>();
			secondAccount.AB_BSB = "B1002";
			secondAccount.AB_AccountNum = "B00010001";
			Env.Registry.SetCustomsSecondPaymentBankAccountForCurrentCompany(secondAccount.PK.ToGuid());

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "Full Name";
			importer.MiscServ.OM_IMEFTBankBSB = "I1002";
			importer.MiscServ.OM_IMEFTBankAccount = "I00010001";
			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			AssertEquals("Party to pay is broker", PaymentParty.Broker, paymentDetailRetriever.PartyToPayEntry);

			BankDetails details = paymentDetailRetriever.GetBankDetails();
			AssertEquals("Broker's details", "1001", details.BSBNumber);
			AssertEquals("Broker's details", "00010000", details.AccountNumber);
			AssertEquals("Broker's details", ZString.Empty, details.AccountName);

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.SecondBroker;
			AssertEquals("Party to pay is second broker", PaymentParty.SecondBroker, paymentDetailRetriever.PartyToPayEntry);

			details = paymentDetailRetriever.GetBankDetails();
			AssertEquals("Broker's details", "1002", details.BSBNumber);
			AssertEquals("Broker's details", "00010001", details.AccountNumber);
			AssertEquals("Broker's details", ZString.Empty, details.AccountName);

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Cash;
			AssertEquals("Party to pay is second broker", PaymentParty.Cash, paymentDetailRetriever.PartyToPayEntry);

			details = paymentDetailRetriever.GetBankDetails();
			AssertEquals("Impoter's details", "1002", details.BSBNumber);
			AssertEquals("Impoter's details", "00010001", details.AccountNumber);
			AssertEquals("Impoter's details", "Full Name", details.AccountName);

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			AssertEquals("Party to pay is broker", PaymentParty.Importer, paymentDetailRetriever.PartyToPayEntry);

			details = paymentDetailRetriever.GetBankDetails();
			AssertEquals("Impoter's details", "1002", details.BSBNumber);
			AssertEquals("Impoter's details", "00010001", details.AccountNumber);
			AssertEquals("Impoter's details", "Full Name", details.AccountName);

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.DrawbackClaimant;
			AssertEquals("Party to pay is broker", PaymentParty.DrawbackClaimant, paymentDetailRetriever.PartyToPayEntry);

			details = paymentDetailRetriever.GetBankDetails();
			AssertEquals("Impoter's details", "1002", details.BSBNumber);
			AssertEquals("Impoter's details", "00010001", details.AccountNumber);
			AssertEquals("Impoter's details", "Full Name", details.AccountName);
		}

		public void TestImporterWillPayDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			OrgHeader importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			TestRetriever details = new TestRetriever(declaration);
			details.TotalAmountPayableExposed = 0;
			Assert(!details.ImporterWillPayDeclaration());

			importer.MiscServ.OM_IMEFTBankBSB = "123";
			Assert(!details.ImporterWillPayDeclaration());

			importer.MiscServ.OM_IMEFTBankAccount = "123";
			Assert(!details.ImporterWillPayDeclaration());

			importer.MiscServ.OM_IMEftCustomsFromImport = true;
			Assert(details.ImporterWillPayDeclaration());

			details.TotalAmountPayableExposed = 1000;
			Assert(details.ImporterWillPayDeclaration());

			importer.MiscServ.OM_IMMinEFTAmount = 1500;
			Assert(!details.ImporterWillPayDeclaration());
			importer.MiscServ.OM_IMMaxEFTAmount = 2000;
			Assert(!details.ImporterWillPayDeclaration());

			details.TotalAmountPayableExposed = 1500;
			Assert(details.ImporterWillPayDeclaration());

			details.TotalAmountPayableExposed = 2000;
			Assert(details.ImporterWillPayDeclaration());

			details.TotalAmountPayableExposed = 2001;
			Assert(!details.ImporterWillPayDeclaration());
		}

		class TestRetriever : PaymentDetailRetriever
		{
			public TestRetriever(JobDeclaration declaration)
				: base(declaration)
			{
			}

			public ZDecimal TotalAmountPayableExposed;
			protected override ZDecimal TotalAmountPayableCore
			{
				get { return TotalAmountPayableExposed; }
			}
		}

		public void TestTotalAmountPayable()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			PaymentDetailRetriever paymentDetailRetriever = new PaymentDetailRetriever(declaration);
			AssertEquals("Total Amount payable", 0m, paymentDetailRetriever.TotalAmountPayable);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Total Amount payable", 0m, paymentDetailRetriever.TotalAmountPayable);

			entryHeader.CH_TotalPaid = 1000m;
			AssertEquals("Total Amount payable", 1000m, paymentDetailRetriever.TotalAmountPayable);

			declaration.CustomsEntryHeaders.AddNew().CH_TotalPaid = 50m;
			AssertEquals("Total Amount payable", 1050m, paymentDetailRetriever.TotalAmountPayable);
		}

		public void TestPartyToPayEntryWithIsImporterFlag()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			PaymentDetailRetriever paymentDetailRetriever = new PaymentDetailRetriever(declaration);
			AssertEquals("Broker will pay", PaymentParty.Broker, paymentDetailRetriever.PartyToPayEntry);

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.LocalBusinessRegNo = GlbCompany.CurrentCompany.GC_BusinessRegNo;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Importer will pay", PaymentParty.Importer, paymentDetailRetriever.PartyToPayEntry);

			importer.LocalBusinessRegNo = "12345678901";
			AssertEquals("Broker will pay", PaymentParty.Broker, paymentDetailRetriever.PartyToPayEntry);

			ZString previousABN = GlbCompany.CurrentCompany.GC_BusinessRegNo;
			GlbCompany.CurrentCompany.GC_BusinessRegNo = ZString.Empty;
			importer.LocalBusinessRegNo = ZString.Empty;
			AssertEquals("Broker will pay", PaymentParty.Broker, paymentDetailRetriever.PartyToPayEntry);
		}
	}
}
