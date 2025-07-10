using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccountingJournalDataLoaderTest : TestCaseWithFactory
	{
		public void TestLoadTransactionsWithHeaderByTransactionPKs()
		{
			SetupData();
			var loader = new AccountingJournalDataLoader();
			var ajs = loader.LoadTransactionsWithHeader(new ZGuid[] { apInvoice.PK, directReceipt.PK, autoJournal.PK });

			AssertEquals("Loaded Accounting Journal", true, (ajs != null && ajs.Count() == 3));

			var aj = ajs.Where(x => x.Ledger == apInvoice.AH_Ledger).ToArray();
			AssertEquals("ARAPAccountingJournal", true, aj.Any());
			AssertEquals("ARAPAccountingJournal", typeof(ARAPAccountingJournal), aj[0].GetType());

			var aj2 = ajs.Where(x => x.Ledger == directReceipt.AH_Ledger).ToArray();
			AssertEquals("CashBookAccountingJournal", true, aj2.Any());
			AssertEquals("CashBookAccountingJournal", typeof(CashBookAccountingJournal), aj2[0].GetType());

			var aj3 = ajs.Where(x => x.Ledger == autoJournal.AH_Ledger).ToArray();
			AssertEquals("GLAccountingJournal", true, aj3.Any());
			AssertEquals("GLAccountingJournal", typeof(GLAccountingJournal), aj3[0].GetType());
		}

		void SetupData()
		{
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader2.PK.ToGuid());

			var job = Creator.CreateJob(Creator.LocalClient, 1.0m, Creator.Agent, 1.0m);

			//AP Invoice
			apInvoice = Creator.CreateAPInvoice<APInvoice>("AP100001", Creator.AUD, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, Creator.Creditor1);
			apInvoice.AH_AB = Creator.AUDBankAccount.PK;
			apInvoice.Lines.RemoveAndDeleteAll();
			var line = Creator.CreateAPInvoiceLine(apInvoice, job, Creator.CC1, Creator.AUD, 1.0m, "AP Line 001", 250m);
			line.AL_AG = Creator.GLHeader1.PK;
			Creator.CreateJobCharge(line, job, Creator.CC1);

			//AR Invoice
			arInvoice = Creator.CreateARInvoice<ARInvoice>("AR100001", Creator.AUD, 1.0m, Creator.Debtor);
			arInvoice.AH_AB = Creator.AUDBankAccount2.PK;
			arInvoice.Lines.RemoveAndDeleteAll();
			var line2 = Creator.CreateARInvoiceLine(arInvoice, job, Creator.CC1, Creator.AUD, 1.0m, "AR Line 001", 250m);
			line2.AL_AG = Creator.GLHeader1.PK;
			Creator.CreateJobCharge(line2, job, Creator.CC1);

			//Direct Receipt
			directReceipt = Creator.CreateDirectReceipt(ZDateTime.Today, 150m, 50m, 250m, 50m);
			directReceipt.AH_AB = Creator.AUDBankAccount2.PK;

			//Direct Payment
			directPayment = Creator.CreateDirectPayment(ZDateTime.Today, 150m, 50m, 250m, 50m);
			directPayment.AH_AB = Creator.AUDBankAccount.PK;

			//ExchangeDifference
			exchangeDifference = Creator.CreateExchangeDifference<ARExchangeDifference>(0m, ZDateTime.Today, Creator.ABIGAS.PK);
			exchangeDifference.AH_AB = Creator.AUDBankAccount.PK;

			// WIP and ACR
			var job2 = Creator.CreateJob("S00001001", Creator.ABIGAS, 0m, null, 0m);
			job2.JH_GB = GlbBranch.CurrentBranch.PK;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var charge2 = job.Charges.AddNew();
			charge2.JR_AC = Creator.CC1.PK;

			Creator.CreateAccrual(charge2);
			Creator.CreateWIP(charge2);

			//GLJournal
			autoJournal = Creator.CreateGLJournal(TransactionTypes.GLAutoJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
			Creator.CreateGLJournal(TransactionTypes.GLReversingJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);

			Creator.CreateJob(Creator.LocalClient, 1.0m, Creator.Agent, 1.0m);
			Creator.CreateJobRevenueJournal(Creator.CC1, job, 250m);

			Factory.Save();
		}

		public virtual void TestPopulateJournalLines_ShowErrorMessage_WhenNoGLDDataAndGenerateAndStoreJournalEntriesForPostedAccountingTransactions()
		{
			SetupData();
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			InsertGeneralLedgerDataRegistry(currentCompanyPK, "GenerateAndStoreJournalEntriesForPostedAccountingTransactions");
			var exceptionMessage = "Journal entries have not been generated yet for the selected transaction.";

			var loader = new AccountingJournalDataLoader();
			var exp = AssertExceptionThrown<InvalidOperationException>(() => loader.LoadTransactionsWithHeader(new ZGuid[] { apInvoice.PK }));
			AssertEquals(exceptionMessage, exp.Message);
		}

		void InsertGeneralLedgerDataRegistry(Guid currentCompanyPK, String sdName)
		{
			var sql =
				@"
				DELETE FROM dbo.StmData WHERE SD_Name = @sdName
				INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_PreserveTestValue, SD_BinaryValue, SD_SystemCreateTimeUtc)
				VALUES (newid(), @SdName, @CompanyPK, 0, @BinaryVal, GETUTCDATE())";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, currentCompanyPK);
				command.AddParameter("@BinaryVal", SqlDbType.Binary, System.Text.Encoding.Unicode.GetBytes("true"));
				command.AddParameter("@SdName", SqlDbType.VarChar, sdName);
				command.ExecuteNonQuery();
			}
		}

		protected TestObjectCreator Creator
		{
			get
			{
				if (creator == null)
				{
					creator = new TestObjectCreator(Factory);
				}
				return creator;
			}
		}
		TestObjectCreator creator;

		ARInvoice arInvoice;
		APInvoice apInvoice;
		GLJournal autoJournal;
		DirectReceipt directReceipt;
		DirectPayment directPayment;
		ARExchangeDifference exchangeDifference;
	}
}
