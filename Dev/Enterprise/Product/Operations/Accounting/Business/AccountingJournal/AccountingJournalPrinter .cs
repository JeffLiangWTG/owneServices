using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ReportingBookLoader;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public abstract class AccountingJournalPrinter
	{
		#region Constructor
		protected AccountingJournalPrinter(Type throwExceptionOfThisTypeIfDocCommandIsMissing)
		{
			this.menuName = (NoResString)"Accounting Journal";
			this.throwExceptionOfThisTypeIfDocCommandIsMissing = throwExceptionOfThisTypeIfDocCommandIsMissing;
		}

		protected readonly Type throwExceptionOfThisTypeIfDocCommandIsMissing;
		protected string menuName;
		#endregion

		public int NumberOfDocumentToPrint
		{
			get
			{
				return ObjectsToPrint != null ? ObjectsToPrint.Count() : 0;
			}
		}
		public string CanPrint()
		{
			var result = string.Empty;

			if (NumberOfDocumentToPrint == 0)
			{
				result = Res.GetString("f848fe5a-e2bf-40e7-8fc1-1f73339f354e", "There is no accounting journal within the given selection criteria.");
			}

			return result;
		}
		public void PrintDocuments()
		{
			RunPrintTask();
		}

		protected abstract JournalLoader LoadJournal { get; }
		protected void RunPrintTask()
		{
			var stmMenuItem = GetDocumentCommand();
			using (var task = new PrintTask(stmMenuItem))
			{
				if (ObjectsToPrint.Any())
				{
					using (var pack = new DocumentPack(stmMenuItem))
					{
						foreach (IDocumentSupportable transaction in ObjectsToPrint)
						{
							pack.AddReportsToPack(stmMenuItem, null, transaction, null);
							pack.ForceBusinessObjectToLogAgainst((transaction as IDocumentSupportableWithForcedLogging).GetTheBusinessObjectForForceLogging());
						}
						task.Add(pack);

						DeliveryInstructions instructions =
#if DEBUG
							DeliveryInstructions_ForTestOnly != null && Globals.IsTest ? DeliveryInstructions_ForTestOnly :
#endif
							null;

#if DEBUG
						var dockPack = task.GetDocumentPacks();
						dockPackCount_ForTestOnly = dockPack.Count();
						transactionCount_ForTestOnly = dockPack.First().Count;
						documentMenuPK_ForTestOnly = stmMenuItem.PK.ToGuid();
#endif
						task.RunWithPartialInstructions(AllowedDeliveryOptions.All, instructions, Env.Security.None);
					}
				}
			}
		}

		IEnumerable<IAccountingJournal> ObjectsToPrint
		{
			get
			{
				if (objectsToPrint == null && LoadJournal != null)
				{
					objectsToPrint = LoadJournal();
				}
				return objectsToPrint;
			}
		}
		IEnumerable<IAccountingJournal> objectsToPrint;
		DocumentCommand GetDocumentCommand()
		{
			DocumentCommand menuItem = null;
			if (ObjectsToPrint.Any())
			{
				var docSupportedObject = ObjectsToPrint.First() as IDocumentSupportable;
				var commandFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, menuName);
				commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsPublished, SQLComparisonOperator.Equal, Core.Constants.BooleanTrueChar);
				var documentCommands = new DocumentCommandCollection(docSupportedObject);
				documentCommands.Load();

				var commands = documentCommands.Find(commandFilter);
				if (!commands.Any())
				{
					Exception exception = null;

					if (throwExceptionOfThisTypeIfDocCommandIsMissing != null)
					{
						var ctor = throwExceptionOfThisTypeIfDocCommandIsMissing.GetConstructor(null);
						exception = ctor.Invoke(null) as Exception;
					}
					else
					{
						exception = new InvalidAccountingJournalOperationException(Res.GetString("17f0e0d6-dc78-40fd-af5f-c8f4c51a25a8", "{0} Document Menu is not available", menuName));
					}

					throw exception;
				}
				menuItem = (DocumentCommand)commands[0];
			}
			return menuItem;
		}

#if DEBUG
		public int DockPackCount_ForTestOnly => dockPackCount_ForTestOnly;
		int dockPackCount_ForTestOnly;

		public int TransactionCountInDocPack_ForTestOnly => transactionCount_ForTestOnly;
		int transactionCount_ForTestOnly;

		public Guid DocumentMenuPK_ForTestOnly => documentMenuPK_ForTestOnly;
		Guid documentMenuPK_ForTestOnly = Guid.Empty;

		public DeliveryInstructions DeliveryInstructions_ForTestOnly;
#endif

		#region Delegate
		public delegate IEnumerable<IAccountingJournal> JournalLoader();
		#endregion
	}

	public class AccoutningJournalWithHeaderPrinter : AccountingJournalPrinter
	{
		#region Constructor
		public AccoutningJournalWithHeaderPrinter(IEnumerable<ZGuid> transactionPKs, Type throwExceptionOfThisTypeIfDocCommandIsMissing)
			: this(throwExceptionOfThisTypeIfDocCommandIsMissing)
		{
			this.transactionPKs = transactionPKs;
		}

		AccoutningJournalWithHeaderPrinter(Type throwExceptionOfThisTypeIfDocCommandIsMissing)
			: base(throwExceptionOfThisTypeIfDocCommandIsMissing)
		{
			this.dataLoader = new AccountingJournalDataLoader();
		}

		readonly IEnumerable<ZGuid> transactionPKs;
		readonly AccountingJournalDataLoader dataLoader;
		#endregion

		protected override JournalLoader LoadJournal
		{
			get
			{
				if (loadJournal == null)
				{
					if (transactionPKs != null && transactionPKs.Any())
					{
						loadJournal = new JournalLoader(() => dataLoader.LoadTransactionsWithHeader(transactionPKs));
					}
				}
				return loadJournal;
			}
		}

		JournalLoader loadJournal;
	}

	public class AccoutningJournalWithoutHeaderPrinter : AccountingJournalPrinter
	{
		#region Constructor
		public AccoutningJournalWithoutHeaderPrinter(IEnumerable<ZGuid> transactionPKs, Type throwExceptionOfThisTypeIfDocCommandIsMissing)
			: base(throwExceptionOfThisTypeIfDocCommandIsMissing)
		{
			this.transactionPKs = transactionPKs;
			this.dataLoader = new AccountingJournalDataLoader();
		}

		readonly IEnumerable<ZGuid> transactionPKs;
		readonly AccountingJournalDataLoader dataLoader;
		#endregion

		protected override JournalLoader LoadJournal
		{
			get
			{
				if (loadJournal == null)
				{
					if (transactionPKs != null && transactionPKs.Any())
					{
						loadJournal = new JournalLoader(() => dataLoader.LoadTransactionsWithoutHeader(transactionPKs));
					}
				}
				return loadJournal;
			}
		}

		JournalLoader loadJournal;
	}

	public class AccoutningJournalPrinterWithHeaderAndLines : AccountingJournalPrinter
	{
		#region Constructor
		public AccoutningJournalPrinterWithHeaderAndLines(IEnumerable<ZGuid> transactionHeaderPKs, IEnumerable<ZGuid> transactionLinesPKs, Type throwExceptionOfThisTypeIfDocCommandIsMissing)
			: base(throwExceptionOfThisTypeIfDocCommandIsMissing)
		{
			this.transactionHeaderPKs = transactionHeaderPKs;
			this.transactionLinesPKs = transactionLinesPKs;
			this.dataLoader = new AccountingJournalDataLoader();
		}

		public AccoutningJournalPrinterWithHeaderAndLines(IEnumerable<ZGuid> transactionHeaderPKs, IEnumerable<ZGuid> transactionLinesPKs, List<ZGuid> additionalHeaderPKs, Type throwExceptionOfThisTypeIfDocCommandIsMissing, ZGuid reportingBookPK, ZDateTime startPostDate, ZDateTime endPostDate)
			: base(throwExceptionOfThisTypeIfDocCommandIsMissing)
		{
			menuName = (NoResString)"Reporting Book - Accounting Journal";
			this.transactionHeaderPKs = transactionHeaderPKs;
			this.transactionLinesPKs = transactionLinesPKs;
			additionalHeaderPKs.AddRange(transactionHeaderPKs);
			generalLedgerTransactionData = AccountingJournalReportingBookLoader.LoadReportingBook(reportingBookPK.ToGuid(), startPostDate, endPostDate, additionalHeaderPKs, transactionLinesPKs);
			this.dataLoader = new AccountingJournalDataLoader(reportingBookPK, generalLedgerTransactionData);
		}

		readonly IEnumerable<ZGuid> transactionHeaderPKs;
		readonly IEnumerable<ZGuid> transactionLinesPKs;
		readonly AccountingJournalDataLoader dataLoader;
		readonly DataTable generalLedgerTransactionData;
		#endregion

		protected override JournalLoader LoadJournal
		{
			get
			{
				if (loadJournal == null)
				{
					if ((transactionHeaderPKs != null && transactionHeaderPKs.Any()) || (transactionLinesPKs != null && transactionLinesPKs.Any()))
					{
						loadJournal = new JournalLoader(() => LoadTransactions());
					}
				}
				return loadJournal;
			}
		}

		IEnumerable<IAccountingJournal> LoadTransactions()
		{
			var journals = new List<AccountingJournal>();

			if (transactionHeaderPKs != null && transactionHeaderPKs.Any())
			{
				journals.AddRange(dataLoader.LoadTransactionsWithHeader(transactionHeaderPKs));
			}

			if (transactionLinesPKs != null && transactionLinesPKs.Any())
			{
				journals.AddRange(dataLoader.LoadTransactionsWithoutHeader(transactionLinesPKs));
			}

			return journals;
		}

		JournalLoader loadJournal;
	}
}

