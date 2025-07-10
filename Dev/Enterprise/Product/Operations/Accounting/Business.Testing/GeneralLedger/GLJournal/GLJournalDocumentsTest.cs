using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(GLJournalDocumentSupporter))]
	public class GLJournalDocumentsTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<GLJournal>();
		}

		public void TestGetDocBusinessObject()
		{
			GLJournal journal = Factory.New<GLJournal>();
			AssertNotNull(journal.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GLJournal, null)[0]);
		}

		public void TestSupportedDataContexts()
		{
			GLJournal journal = Factory.New<GLJournal>();
			AssertEquals("DataContext.GenericFreightJob is Supported", true, journal.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJob)));
			AssertEquals("DataContext.ChinaJournalListing is Supported", true, journal.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ChinaJournalListing)));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				AssertEquals("DataContext.AccountingVoucher is Supported", true, journal.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.AccountingVoucher)));
			}
		}

		public void TestAccountingVoucherPrintFormDocument()
		{
			AccountingVoucherPrintFormDocument(TransactionTypes.GLStandardJournal);

			AccountingVoucherPrintFormDocument(TransactionTypes.GLReversingJournal);
			AccountingVoucherPrintFormDocument(TransactionTypes.WIPAccrualJournal);
			AccountingVoucherPrintFormDocument(TransactionTypes.GLNoteJournal);

			void AccountingVoucherPrintFormDocument(string transactionType)
			{
				var query = new ZQuery(StmMenuItemSchema.SU_BusinessContext, CargoWise.Definitions.BusinessContext.ARTransaction).AddToFilter(StmMenuItemSchema.SU_MenuName, "Accounting Voucher");
				var command = Factory.LoadTop1<DocumentCommand>(query);
				var glJournal = Factory.NewWithValidTestData<GLJournal>();
				glJournal.AH_TransactionType = transactionType;
				command.Parent = glJournal;

				using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
				{
					var runner = new DocumentRunner();
					var documentPrintSet = runner.GetDocumentPrintSetForTesting(command);

					if (transactionType == TransactionTypes.GLStandardJournal)
					{
						AssertEquals(1, documentPrintSet.GetDocumentPacks().Count());
						AssertEquals(0, documentPrintSet.ReasonsForEmptyPacks.Count);
					}
					else
					{
						AssertEquals(0, documentPrintSet.GetDocumentPacks().Count());
						AssertEquals(1, documentPrintSet.ReasonsForEmptyPacks.Count);
						AssertEquals("Cannot produce this Document because the data required to do so is not present", documentPrintSet.ReasonsForEmptyPacks[0]);
					}
				}
			}
		}
	}
}
