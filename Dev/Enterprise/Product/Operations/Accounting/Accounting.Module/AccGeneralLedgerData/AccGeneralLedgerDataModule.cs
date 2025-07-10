using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AccGeneralLedgerDataModule : ZFilterGridModule
	{
		public AccGeneralLedgerDataModule()
		{
		}

		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccGeneralLedgerData; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			ZController result = null;

			if (selectedBusinessObject is TransactionHeader transactionHeader)
			{
				if (selectedBusinessObject is GLJournal)
				{
					result = new GLJournalController();
				}
				else if (selectedBusinessObject is JCJournalHeader)
				{
					result = new JCJournalController();
				}
				else
				{
					result = AccountingControllerCreator.GetNewController(transactionHeader);
				}
			}
			else if (selectedBusinessObject is BaseWIPAccrual baseWIPAccrual)
			{
				if (baseWIPAccrual.AL_LineType == TransactionLineTypes.WIP)
				{
					result = new WIPController();
				}
				else
				{
					result = new AccrualController();
				}
			}

			return result;
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccGeneralLedgerDataFilterControl(GridCollection, (AccGeneralLedgerDataFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccGeneralLedgerDataCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccGeneralLedgerDataFilterBusinessObject();
		}

		protected override IZForm ShowViewForm(BusinessObject bo)
		{
			IZForm result = null;

			if (bo is AccGeneralLedgerData gld)
			{
				var transactionPK = ZGuid.Empty;
				var isWIPOrAccrual = gld.GLD_AL_TransactionLine.IsValid && (gld.TransactionLine.AL_LineType == TransactionLineTypes.WIP || gld.TransactionLine.AL_LineType == TransactionLineTypes.Accrual);

				if (isWIPOrAccrual)
				{
					transactionPK = gld.GLD_AL_TransactionLine;
				}
				else
				{
					if (gld.GLD_ATM_TaxGLMovement.IsValid)
					{
						var taxGLMovement = Factory.Load<AccTaxGLMovement>(gld.GLD_ATM_TaxGLMovement);
						var taxTransaction = Factory.Load<AccTaxTransaction>(taxGLMovement.ATM_ATT_TaxTransaction);
						transactionPK = taxTransaction.ATT_AH;
					}
					else if (gld.GLD_YC_CashBasisVAT.IsValid)
					{
						var cashBasisVat = Factory.Load<AccCashBasisVAT>(gld.GLD_YC_CashBasisVAT);
						var line = Factory.Load<AccTransactionLines>(cashBasisVat.YC_AL_TransactionLine);
						transactionPK = line.AL_AH;
					}
					else if (gld.GLD_AH_TransactionHeader.IsValid)
					{
						transactionPK = gld.GLD_AH_TransactionHeader;
					}
				}

				var bizObj = Factory.Load(isWIPOrAccrual ? typeof(BaseWIPAccrual) : typeof(TransactionHeader), transactionPK);
				result = base.ShowViewForm(bizObj);
			}

			return result;
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AccountingJournals; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#region Menu Item

		#region Print

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			var printMenuItem = new ZMenuItem(PrintMenuText);
			printMenuItem.MenuItems.Add(new ZMenuItem(AccountingJournalPrintHelper.PrintAccountingJournalText, new EventHandler(HandlePrintAccountingJournal)));

			var reportingBookAccountingJournalPrintOptions = AccountingMasterFilesRegistry.Instance.ReportingBookAccountingJournalPrintOption.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).Cast<ReportingBookAccountingJournalPrintOption>();
			if (reportingBookAccountingJournalPrintOptions?.Any() ?? false)
			{
				var printReportingBookAccountingJournalMenuItem = new ZMenuItem(ReportingBookAccountingJournalPrintHelper.PrintReportingBookAccountingJournalText, new EventHandler((sender, e) => HandlePrintReportingBookAccountingJournal(sender, e, reportingBookAccountingJournalPrintOptions.First(x => x.Default).ReportingBook)));
				printMenuItem.MenuItems.Add(printReportingBookAccountingJournalMenuItem);

				foreach (var reportingBookAccountingJournalPrintOption in reportingBookAccountingJournalPrintOptions)
				{
					printReportingBookAccountingJournalMenuItem.MenuItems.Add(new ZMenuItem(reportingBookAccountingJournalPrintOption.ReportingBookCode, new EventHandler((sender, e) => HandlePrintReportingBookAccountingJournal(sender, e, reportingBookAccountingJournalPrintOption.ReportingBook))));
				}
			}

			result.Add(printMenuItem);
			return result.ToArray();
		}

		static MultilingualString PrintMenuText => ResString.GetMultilingualString("EA8334AD-DCE3-4DF9-BF07-B5ED0CEECDDE", "&Print");

		protected void HandlePrintAccountingJournal(object sender, EventArgs e)
		{
			HandlePrintAccountingJournalCore(sender, e, null);
		}

		void HandlePrintReportingBookAccountingJournal(object sender, EventArgs e, ZGuid? reportingBookPK = null)
		{
			HandlePrintAccountingJournalCore(sender, e, reportingBookPK);
		}

		void HandlePrintAccountingJournalCore(object sender, EventArgs e, ZGuid? reportingBookPK)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(AccountingJournalPrintHelper.PrintAccountingJournalPromptText);
			}
			else
			{
				var gridSelections = Grid.GetSelectedElements<AccGeneralLedgerData>();

				var transactionsHeaderToPrint = new List<TransactionHeader>();
				var transactionsLinesToPrint = new List<BaseWIPAccrual>();
				var additionalHeaderPKs = new List<ZGuid>();
				var startDate = gridSelections.Min(gld => gld.GLD_PostDate).Date.ToZDateTime();
				var endDate = gridSelections.Max(gld => gld.GLD_PostDate).EndOfDay();

				foreach (var glData in gridSelections)
				{
					if (glData.TransactionLine != null && (glData.TransactionLine.AL_LineType == TransactionLineTypes.Accrual || glData.TransactionLine.AL_LineType == TransactionLineTypes.WIP))
					{
						var transactionline = Factory.Load<BaseWIPAccrual>(glData.TransactionLine.PK);
						transactionsLinesToPrint.Add(transactionline);
					}
					else if (glData.TransactionHeader != null)
					{
						var transactionHeader = Factory.Load<TransactionHeader>(glData.TransactionHeader.PK);
						transactionsHeaderToPrint.Add(transactionHeader);

						if (reportingBookPK != null)
						{
							if (transactionHeader.AH_TransactionType == TransactionTypes.Transfer)
							{
								var oppositeTransactionQuery = new ZQuery(AccTransactionHeaderSchema.AH_GC, transactionHeader.AH_GC);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, transactionHeader.AH_Ledger);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionHeader.AH_TransactionType);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transactionHeader.AH_TransactionNum);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transactionHeader.PK);
								var oppositeTransaction = Factory.LoadTop1<TransactionHeader>(oppositeTransactionQuery);
								additionalHeaderPKs.Add(oppositeTransaction?.PK ?? ZGuid.Empty);
							}
							else if (transactionHeader.AH_TransactionType == TransactionTypes.Contra)
							{
								var oppositeTransactionQuery = new ZQuery(AccTransactionHeaderSchema.AH_GC, transactionHeader.AH_GC);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionHeader.AH_TransactionType);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transactionHeader.AH_TransactionNum);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, transactionHeader.AH_TransactionBelongsToGroup);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transactionHeader.PK);
								var oppositeTransaction = Factory.LoadTop1<TransactionHeader>(oppositeTransactionQuery);
								additionalHeaderPKs.Add(oppositeTransaction?.PK ?? ZGuid.Empty);
							}
							else if (transactionHeader.AH_TransactionType == TransactionTypes.GLReversingJournal || transactionHeader.AH_TransactionType == TransactionTypes.GLAutoJournal)
							{
								var query = new ZQuery(AccGeneralLedgerDataSchema.GLD_GC_Company, transactionHeader.AH_GC);
								query.AddToFilter(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, transactionHeader.PK);
								var relatedGldDatas = Factory.Load<AccGeneralLedgerData>(query);
								var minStart = relatedGldDatas.Min(x => x.GLD_PostDate);
								var maxEnd = relatedGldDatas.Max(x => x.GLD_PostDate);
								startDate = minStart < startDate ? minStart : startDate;
								endDate = maxEnd > endDate ? maxEnd : endDate;
							}
						}
					}
				}

				if (transactionsHeaderToPrint.Count == 0 && transactionsLinesToPrint.Count == 0)
				{
					Globals.Message.Show(AccountingJournalPrintHelper.PrintAccountingJournalPromptText);
				}
				else
				{
					if (reportingBookPK != null)
					{
						AccountingJournalPrintHelper.PrintAccountingJournalForReportingBook(transactionsHeaderToPrint, transactionsLinesToPrint, additionalHeaderPKs, reportingBookPK.Value, startDate, endDate);
					}
					else
					{
						AccountingJournalPrintHelper.PrintAccountingJournal(transactionsHeaderToPrint, transactionsLinesToPrint);
					}
				}
			}
		}

		#endregion

		#endregion

		#region ZModule Allowed Actions

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		public override bool CouldAllowUniversalCopy
		{
			get { return false; }
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard
		{
			get { return false; }
		}

		#endregion

		#endregion
	}
}
