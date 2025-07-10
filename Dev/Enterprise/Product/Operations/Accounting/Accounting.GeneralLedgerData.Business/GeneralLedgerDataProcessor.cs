using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedgerData;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class GeneralLedgerDataProcessor : IGeneralLedgerDataProcessor
	{
		readonly ReadOnlyBusinessObjectFactory ReadOnlyFactory = new ReadOnlyBusinessObjectFactory();
		readonly string[] GLJournalLineTypes = new[] { TransactionTypes.GLStandardJournal, TransactionTypes.GLNoteJournal, TransactionTypes.GLAutoJournal, TransactionTypes.GLReversingJournal };

		void IGeneralLedgerDataProcessor.ProcessData(DataRow[] gLDDataSources)
		{
			var lookup = gLDDataSources.ToLookup(row => row.Table.TableName == AccTransactionLinesSchema.Constants.TableName && GLJournalLineTypes.Contains((string)row[AccTransactionLinesSchema.Constants.AL_LineType]));

			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				ProcessGLJournalData(lookup[true]);
				ProcessNonGLJournalData(lookup[false]);
				manager.CommitTransaction();
			}

			AccountingMasterFilesUtils.NudgeServiceTask("GLN");
		}

		DataTable GetTVPGeneralLedgerData(IEnumerable<DataRow> dataRows)
		{
			var tvpGeneralLedgerData = GeneralLedgerDataScriptHelper.GetEmptyTVPGeneralLedgerData();
			foreach (var gLDDataSourceRow in dataRows)
			{
				var (lineCreator, ledger, transactionType) = GetGeneralLedgerDataLineCreator(gLDDataSourceRow);
				var drcrEntry = lineCreator?.CreateDRCREntries(gLDDataSourceRow);

				if (drcrEntry != null)
				{
					GeneralLedgerDataScriptHelper.PopulateTVPGeneralLedgerData(tvpGeneralLedgerData, drcrEntry, ledger, transactionType);
				}
			}

			return tvpGeneralLedgerData;
		}

		(GeneralLedgerDataLineCreatorBase LineCreator, ZString Ledger, ZString TransactionType) GetGeneralLedgerDataLineCreator(DataRow gLDDataSource)
		{
			var tableName = gLDDataSource.Table.TableName;
			ZString ledger = ZString.Empty;
			ZString transactionType = ZString.Empty;
			ZString transactionLineType = ZString.Empty;
			ZString resultLedger = ZString.Empty;
			ZString resultTransactionType = ZString.Empty;

			if (tableName == AccTransactionHeaderSchema.Constants.TableName)
			{
				ledger = (string)gLDDataSource[AccTransactionHeaderSchema.Constants.AH_Ledger];
				transactionType = (string)gLDDataSource[AccTransactionHeaderSchema.Constants.AH_TransactionType];
				resultLedger = ledger;
				resultTransactionType = transactionType;
			}
			else if (tableName == AccTransactionLinesSchema.Constants.TableName)
			{
				if (gLDDataSource[AccTransactionLinesSchema.Constants.AL_AH] is Guid headerPK)
				{
					(ledger, transactionType) = GetLedgerAndTransactionType(headerPK);
				}

				transactionLineType = (string)gLDDataSource[AccTransactionLinesSchema.Constants.AL_LineType];
				resultLedger = ledger;
				resultTransactionType = transactionType.IsEmpty ? transactionLineType : transactionType;
			}
			else if (tableName == AccCashBasisVATSchema.Constants.TableName)
			{
				var cashBasisTransactionLine = ReadOnlyFactory.Load<TransactionLine>((Guid)gLDDataSource[AccCashBasisVATSchema.Constants.YC_AL_TransactionLine]);
				resultTransactionType = cashBasisTransactionLine.AL_LineType;
				if (!cashBasisTransactionLine.AL_AH.IsEmpty)
				{
					(resultLedger, resultTransactionType) = GetLedgerAndTransactionType(cashBasisTransactionLine.AL_AH.ToGuid());
				}
			}
			else if (tableName == AccTaxGLMovementSchema.Constants.TableName)
			{
				var taxTransaction = ReadOnlyFactory.Load<AccTaxTransaction>((Guid)gLDDataSource[AccTaxGLMovementSchema.Constants.ATM_ATT_TaxTransaction]);
				(resultLedger, resultTransactionType) = GetLedgerAndTransactionType(taxTransaction.ATT_AH.ToGuid());
			}

			return (GetGeneralLedgerDataLineCreatorCore(gLDDataSource, ledger, transactionType, transactionLineType), resultLedger, resultTransactionType);
		}

		(ZString Ledger, ZString TransactionType) GetLedgerAndTransactionType(Guid headerPK)
		{
			ZString ledger = ZString.Empty;
			ZString transactionType = ZString.Empty;
			var headerInfo = GeneralLedgerDataRetriever.GetTransactionHeaderInfo(ReadOnlyFactory, headerPK);
			if (headerInfo != null)
			{
				ledger = headerInfo.Ledger;
				transactionType = headerInfo.TransactionType;
			}

			return (ledger, transactionType);
		}

		GeneralLedgerDataLineCreatorBase GetGeneralLedgerDataLineCreatorCore(DataRow gLDDataSource, ZString ledger, ZString transactionType, ZString transactionLineType)
		{
			GeneralLedgerDataLineCreatorBase creator = null;
			var creatorKey = gLDDataSource.Table.TableName + ledger + transactionType + transactionLineType;
			if (GeneralLedgerDataLineCreatorDict.TryGetValue(creatorKey, out creator))
			{
				return creator;
			}
			else
			{
				if (ledger == LedgerTypes.AccountsPayable || ledger == LedgerTypes.AccountsReceivable)
				{
					if (transactionType == TransactionTypes.Invoice || transactionType == TransactionTypes.CreditNote || transactionType == TransactionTypes.AdjustmentNote)
					{
						creator = new INVCRDADJGeneralLedgerDataLineCreator();
					}
					else if (transactionType == TransactionTypes.Transfer)
					{
						creator = new ARAPTRFGeneralLedgerDataLineCreator();
					}
					else if (transactionType == TransactionTypes.Journal)
					{
						creator = new ARAPJNLGeneralLedgerDataLineCreator();
					}
					else if (transactionType == TransactionTypes.Contra)
					{
						creator = new ARAPCTRGeneralLedgerDataLineCreator();
					}
					else if (transactionType == TransactionTypes.Payment || transactionType == TransactionTypes.Receipt)
					{
						creator = new ARAPPAYRECGeneralLedgerDataLineCreator();
					}
					else if (transactionType == TransactionTypes.ExchangeDifference || transactionType == TransactionTypes.Overpayment || transactionType == TransactionTypes.Discount)
					{
						creator = new EXXOVPDSCGeneralLedgerDataLineCreator();
					}
				}
				else if (transactionLineType == TransactionLineTypes.Accrual || transactionLineType == TransactionLineTypes.WIP)
				{
					creator = new WIPACRGeneralLedgerDataLineCreator();
				}
				else if (transactionLineType == TransactionTypes.GLStandardJournal || transactionLineType == TransactionTypes.GLNoteJournal)
				{
					creator = new GLGJLNJLGeneralLedgerDataLineCreator();
				}
				else if (transactionLineType == TransactionTypes.GLAutoJournal)
				{
					creator = new GLAJLGeneralLedgerDataLineCreator();
				}
				else if (ledger == LedgerTypes.General && transactionType == TransactionTypes.GLReversingJournal && transactionLineType == TransactionTypes.GLReversingJournal)
				{
					creator = new GLRJLGeneralLedgerDataLineCreator();
				}
				else if (gLDDataSource.Table.TableName == AccCashBasisVATSchema.Constants.TableName)
				{
					creator = new CashBasisVATGeneralLegerDataLineCreator();
				}
				else if (gLDDataSource.Table.TableName == AccTaxGLMovementSchema.Constants.TableName)
				{
					creator = new TaxGLMovementGeneralLedgerDataLineCreator();
				}
				else if (ledger == LedgerTypes.CashBook)
				{
					if (transactionType == TransactionTypes.Transfer)
					{
						creator = new CBTRFGeneralLedgerDataLineCreator();
					}
					else if (transactionType == TransactionTypes.ExchangeDifference)
					{
						creator = new CBEXXGeneralLedgerDataLineCreator();
					}
					else if (transactionType == TransactionTypes.DirectPayment || transactionType == TransactionTypes.DirectReceipt)
					{
						creator = new CBDRCDPYGeneralLedgerDataLineCreator();
					}
				}
				else if (ledger == LedgerTypes.JobCosting && (transactionType == TransactionTypes.Journal || transactionType == TransactionTypes.JobRevenueJournal))
				{
					creator = new JCJRJJNLGeneralLedgerDataLineCreator();
				}

				GeneralLedgerDataLineCreatorDict.Add(creatorKey, creator);
				return creator;
			}
		}

		void ProcessGLJournalData(IEnumerable<DataRow> gLJournalDataRows)
		{
			if (!gLJournalDataRows.Any())
			{
				return;
			}
			var gLJournalDataRowsDistinct = gLJournalDataRows.GroupBy(lineForGroupBy => lineForGroupBy[AccTransactionLinesSchema.Constants.PK]).Select(g => g.Last());
			var toDeleteGLDGLJournalLinePKs = gLJournalDataRowsDistinct.Where(x => x.RowState == DataRowState.Modified).Select(x => (Guid)x[AccTransactionLinesSchema.Constants.PK]).ToList();

			var tvpGeneralLedgerDataForGLJournal = GetTVPGeneralLedgerData(gLJournalDataRowsDistinct);
			GeneralLedgerDataScriptHelper.ExecuteGeneralLedgerDataForGLJournalScript(tvpGeneralLedgerDataForGLJournal, toDeleteGLDGLJournalLinePKs.ToArray());
		}

		void ProcessNonGLJournalData(IEnumerable<DataRow> noneGLJournalDataRows)
		{
			if (noneGLJournalDataRows.Any())
			{
				var tvpGeneralLedgerData = GetTVPGeneralLedgerData(noneGLJournalDataRows);
				GeneralLedgerDataScriptHelper.ExecuteGeneralLedgerDataNonGLJournalScript(tvpGeneralLedgerData);
			}
		}

		Dictionary<ZString, GeneralLedgerDataLineCreatorBase> GeneralLedgerDataLineCreatorDict
		{
			get { return fGeneralLedgerDataLineCreatorDict ?? (fGeneralLedgerDataLineCreatorDict = new Dictionary<ZString, GeneralLedgerDataLineCreatorBase>()); }
		}
		Dictionary<ZString, GeneralLedgerDataLineCreatorBase> fGeneralLedgerDataLineCreatorDict;
	}
}
