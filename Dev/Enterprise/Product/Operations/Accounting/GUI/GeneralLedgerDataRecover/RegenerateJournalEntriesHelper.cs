using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI
{
	public static class RegenerateJournalEntriesHelper
	{
		public static void AddRegenerateJournalEntriesMenuItemIfAllowed(IList menuItems, EventHandler handleRegenerateJournalEntries, int index = -1)
		{
			if (Env.CurrentUser.IsSupportUser
				&& AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value)
			{
				var regenerateJournalEntriesItem = new ZMenuItem((NoResString)"Regenerate Journal Entries (CWSupport Only)", new EventHandler(handleRegenerateJournalEntries));
				if (index >= 0)
				{
					menuItems.Insert(index, regenerateJournalEntriesItem);
				}
				else
				{
					menuItems.Add(regenerateJournalEntriesItem);
				}
			}
		}

		public static void RegenerateJournalEntries(BusinessObject[] selectedBusinessObjects)
		{
			if (selectedBusinessObjects != null && selectedBusinessObjects.Any())
			{
				if (selectedBusinessObjects.Length > 10)
				{
					Globals.Message.ShowError((NoResString)"Only maximum number of 10 Transactions can be selected for GLD Regeneration");
				}
				else if (ContainNonSupportTypeForRegenerateJournal(selectedBusinessObjects))
				{
					Globals.Message.ShowError((NoResString)"Journal Entries cannot be regenerated for Opening Receipts or Payments.");
				}
				else if (!ConfirmInvalidateFinalisedComplianceReport(selectedBusinessObjects))
				{
					return;
				}
				else if (Globals.Message.Show((NoResString)"Are you sure you want to Delete and Regenerate General Ledger Data for selected Transactions?", (NoResString)"Regenerate GLD", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
				{
					var generalLedgerDataRecover = ObjectFactory.Get<IGeneralLedgerDataRecover>();

					var headerTransactions = selectedBusinessObjects.Where(x => (x is TransactionHeader) && !(x is TransactionHeaderWithLines)).ToArray();

					var lines = new List<AccTransactionLines>();
					var headerWithLinesTransactions = selectedBusinessObjects.Where(x => x is TransactionHeaderWithLines).Cast<TransactionHeaderWithLines>();
					var headerLines = headerWithLinesTransactions.SelectMany(x => x.Lines).Cast<AccTransactionLines>();
					if (headerLines.Any())
					{
						lines.AddRange(headerLines);
					}

					var onlyLines = selectedBusinessObjects.Where(x => x is AccTransactionLines).Cast<AccTransactionLines>();
					if (onlyLines.Any())
					{
						lines.AddRange(onlyLines);
					}

					var factory = new BusinessObjectFactory
					{
						NameForDebugging = "RegenerateJournalEntries Factory"
					};

					var cashBasisVats = factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_AL_TransactionLine, lines.Select(x => x.PK)));
					var taxTransactions = factory.Load<AccTaxTransaction>(new ZQuery(AccTaxTransactionSchema.ATT_AH, selectedBusinessObjects.Select(x => x.PK)));
					var taxGLMovements = factory.Load<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxTransactions.Select(x => x.PK)));

					using (var manager = Db.Connection.BeginTransactionWithManager())
					{
						if (headerTransactions.Any())
						{
							generalLedgerDataRecover.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, headerTransactions);
						}

						if (lines.Any())
						{
							generalLedgerDataRecover.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, lines.ToArray());
						}

						if (cashBasisVats.Any())
						{
							generalLedgerDataRecover.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_YC_CashBasisVAT, cashBasisVats.ToArray());
						}

						if (taxGLMovements.Any())
						{
							generalLedgerDataRecover.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_ATM_TaxGLMovement, taxGLMovements.ToArray());
						}
						manager.CommitTransaction();
					}

					Globals.Message.ShowInformation((NoResString)"General Ledger Data for selected Transactions have been regenerated");
				}
			}
			else
			{
				Globals.Message.ShowError((NoResString)"Please select Transactions first.");
			}
		}

		static bool ConfirmInvalidateFinalisedComplianceReport(BusinessObject[] selectedBusinessObjects)
		{
			var headerTransactions = selectedBusinessObjects.Where(x => (x is TransactionHeader) && !(x is TransactionHeaderWithLines)).ToArray();
			var lines = new List<AccTransactionLines>();
			var headerWithLinesTransactions = selectedBusinessObjects.Where(x => x is TransactionHeaderWithLines).Cast<TransactionHeaderWithLines>();
			var headerLines = headerWithLinesTransactions.SelectMany(x => x.Lines).Cast<AccTransactionLines>();
			if (headerLines.Any())
			{
				lines.AddRange(headerLines);
			}

			var onlyLines = selectedBusinessObjects.Where(x => x is AccTransactionLines).Cast<AccTransactionLines>();
			if (onlyLines.Any())
			{
				lines.AddRange(onlyLines);
			}

			var factory = new BusinessObjectFactory();
			var cashBasisVats = factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_AL_TransactionLine, lines.Select(x => x.PK)));
			var taxTransactions = factory.Load<AccTaxTransaction>(new ZQuery(AccTaxTransactionSchema.ATT_AH, selectedBusinessObjects.Select(x => x.PK)));
			var taxGLMovements = factory.Load<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxTransactions.Select(x => x.PK)));

			var sql = @"SELECT TOP 1 ACL_ACR_Report
							FROM dbo.AccComplianceReportTransactionPivot
								JOIN dbo.AccComplianceReport ON ACL_ACR_Report = ACR_PK
								JOIN dbo.AccGeneralLedgerData ON ACL_ParentID = GLD_PK
							WHERE ACR_Status = 'FIN'
								AND ACR_GC_Company = @CompanyPk
								AND (
									GLD_AH_TransactionHeader IN (SELECT value FROM @HeaderPKs)
									OR GLD_AL_TransactionLine IN (SELECT value FROM @LinePKs)
									OR GLD_YC_CashBasisVAT IN (SELECT value FROM @CashPKs)
									OR GLD_ATM_TaxGLMovement IN (SELECT value FROM @TaxPKs)
								)";
			var result = Db.Connection.ExecuteScalar(sql,
				command =>
				{
					command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
					command.AddTableValuedParameter("@HeaderPKs", TVPHelper.TVP_uniqueidentifier, headerTransactions.Select(x => x.PK.ToGuid()).Distinct());
					command.AddTableValuedParameter("@LinePKs", TVPHelper.TVP_uniqueidentifier, lines.Select(x => x.PK.ToGuid()).Distinct());
					command.AddTableValuedParameter("@CashPKs", TVPHelper.TVP_uniqueidentifier, cashBasisVats.Select(x => x.PK.ToGuid()).Distinct());
					command.AddTableValuedParameter("@TaxPKs", TVPHelper.TVP_uniqueidentifier, taxGLMovements.Select(x => x.PK.ToGuid()).Distinct());
				});
			return result == null || AccountingMessageHelper.ConfirmInvalidateFinalisedComplianceReport((NoResString)"Regenerate GLD");
		}

		static bool ContainNonSupportTypeForRegenerateJournal(BusinessObject[] selectedBusinessObjects) => selectedBusinessObjects.Any(x => x is OpeningReceipt || x is OpeningPayment);
	}
}
