using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

#if DEBUG
using Enterprise.ZArchitecture.Core.Testing;
#endif

namespace Enterprise.Accounting.Netting
{
	public class NettingPeriodManager
	{
		public NettingPeriodManager(NettingSystemPeriod period)
		{
			Argument.NotNull(period, "period");
			this.period = period;
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		readonly NettingSystemPeriod period;
		public void FinaliseNettingCycle(NotificationBuffer notification)
		{
			using (Factory.AddDisposableService())
			using (var manager = Connection.BeginTransactionWithManager())
			{
				NettingHelper.MoveUnmatchedTransactionsToTheNextPeriod(period, period.Factory, Connection);
				UpdateIndicativeExchangeRatesFromExecutionRates();
				CopyExecutionRatesToNextPeriodIndicativeRate();

				GenerateCalculationRecords();
				var netMovements = NettingHelper.GetNettingMovements(Factory, period, isFinal: true);

				if (netMovements.Any())
				{
					CreateJournalsFromNetMovement(netMovements);
					GenerateAndSendJournalImportFileToParticipants(netMovements, notification);
				}

				MarkPeriodAsComplete();
				Factory.Save();

				manager.CommitTransaction();
			}
		}

		DbConnection Connection => connection ?? (connection = Db.Connection);
		protected DbConnection connection;

		void MarkPeriodAsComplete()
		{
			var periodInCurrentFactory = Factory.Load<NettingSystemPeriod>(period.PK);
			periodInCurrentFactory.NSP_IsComplete = true;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void UpdateIndicativeExchangeRatesFromExecutionRates()
		{
			using (var cmd = Connection.Command("NettingUpdateIndicativeRateFromExecutionRate"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@NettingPeriod", SqlDbType.UniqueIdentifier, period.PK.ToGuid());

				cmd.ExecuteNonQuery();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
#if DEBUG
		public
#endif
		void CopyExecutionRatesToNextPeriodIndicativeRate()
		{
			var nextPeriod = NettingPeriodHelper.GetNextOpenPeriod(period, period.Factory);

			if (nextPeriod != null)
			{
				using (var cmd = Connection.Command("NettingCopyExecutionRatesToNextPeriodIndicativeRate"))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@NettingPeriod", SqlDbType.UniqueIdentifier, period.PK.ToGuid());
					cmd.AddParameter("@NextPeriod", SqlDbType.UniqueIdentifier, nextPeriod.PK.ToGuid());

					cmd.ExecuteProcedureWithReturnValue();
				}
			}
			else
			{
				throw new IncorrectDataSetupException(Res.GetString("eda60565-d0ed-4854-b37f-1a9e2319143d", "No Netting Cycle found after current open Cycle: {0}", period.NSP_Period));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void GenerateCalculationRecords()
		{
			using (var cmd = Connection.Command("CreateNettingCalculationRecords"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@NettingPeriod", SqlDbType.UniqueIdentifier, period.PK.ToGuid());
				cmd.AddParameter("@CurrentDate", SqlDbType.SmallDateTime, ZDateTime.UtcNow.ToDateTime());
				cmd.ExecuteProcedureWithReturnValue();
			}
		}

		void CreateJournalsFromNetMovement(IEnumerable<NettingMovement> movements)
		{
			foreach (var movement in movements)
			{
				var createReceivablesJournal = Equals(movement.Direction.ToUpper(), "IN");
				var journal = (Journal)Factory.New(createReceivablesJournal ? typeof(ARJournal) : typeof(APJournal));
				journal.AH_InvoiceDate = ZDateTime.Now;
				journal.AH_PostDate = ZDateTime.Now;
				journal.AH_DueDate = ZDateTime.Now;
				journal.AH_OH = movement.OrgPK;
				journal.AH_Desc = movement.MovementType == MovementType.Transaction
											? Res.GetString("966fbe28-d4ce-4d55-99fa-dc439a29ae19", "Netting Transactions for {0}", period.NSP_Period)
												: createReceivablesJournal
													? Res.GetString("ab33a16d-868b-42d7-acd9-fe5aa4f39008", "FX Offer")
														: Res.GetString("90dcecd0-e21a-44db-bd1b-7d3a7a7d5ca8", "FX Request");
				journal.DebitCreditSign = createReceivablesJournal ? DebitCreditDataEntry.CR : DebitCreditDataEntry.DR;
				journal.AH_RX_NKTransactionCurrency = movement.Currency;
				journal.AH_OSExTaxAmount = Math.Abs(movement.MovementAmount);
				journal.AH_LocalExTaxAmount = Math.Abs(movement.NettingCurrencyReportingAmount);

				journal.AH_AG = new ZGuid(AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.Value);

				//journal.SubAccounts[0].AHS_SubClassParentTableCode = org		// TODO
				//journal.SubAccounts[0].AHS_SubClassParentId = org.PK			// TODO
				//journal.AH_GB =							// TODO
				//journal.AH_GE =							// TODO

				CreateTransactionHeaderAndNettingLinks(journal);
			}
		}

		void CreateTransactionHeaderAndNettingLinks(Journal journal)
		{
			var nettingLink = Factory.New<AccTransactionHeaderNettingLink>();
			nettingLink.AH2_AH = journal.PK;
			nettingLink.AH2_NSP_Period = period.PK;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		void GenerateAndSendJournalImportFileToParticipants(IEnumerable<NettingMovement> movements, NotificationBuffer notification)
		{
			var nettingSystemOrgHeader = OrgHeader.DefaultOrg;

			var allTransactionMovement = movements.Where(x => x.MovementType == MovementType.Transaction);
			foreach (var movement in allTransactionMovement)
			{
				var csvContentBuilder = new ZStringBuilder();

				AddOpenClearingJournalForNettingSystemOrg(nettingSystemOrgHeader, movement, csvContentBuilder);

				var allTransactions = NettingHelper.GetAllTransactionsForParticipantJournalCreation(period.PK, Connection);
				var transactions = from n in allTransactions
								   where n.CompanyCode == movement.CompanyCode
								   orderby n.Ledger, n.TransactionReference
								   select n;

				var groupedByOrgCode = from n in transactions
									   orderby n.OrgCode
									   group n by new { n.OrgPK } into g
									   select g;

				foreach (var item in groupedByOrgCode)
				{
					var secondaryGrouping = from n in item
											where !n.OfferOrRequest
											orderby n.TransactionCurrency, n.TransactionAmount, n.TransactionReference
											group n by new { n.Ledger, n.OrgPK, n.ParticipantOrgPK, n.TransactionCurrency } into g
											select new
											{
												g.Key.Ledger,
												g.Key.OrgPK,
												g.Key.ParticipantOrgPK,
												g.Key.TransactionCurrency,
												Amount = g.Sum(p => p.TransactionAmount),
												Transactions = g
											};
					var ptrDescriptionPrefix = Res.GetString("D1493B72-FCE0-41A0-9203-7079D612AFC2", "Journal Related to ");
					foreach (var grp in secondaryGrouping)
					{
						var refCurrency = GetRefCurrency(grp.TransactionCurrency);

						var participantOrgHeader = Factory.Load<OrgHeader>(grp.ParticipantOrgPK);
						if (participantOrgHeader != null)
						{
							ZDecimal invertedOsAmount = 0M;
							if (string.Equals(grp.Ledger, LedgerTypes.AccountsReceivable))
							{
								invertedOsAmount = grp.Amount * -1; //Header amount is set as negative for both AR and AP journal
							}
							else
							{
								//because netting is receivable based, the AP journal amount should be the same amount as the AR journal.
								//we are calculating the AP journal amount from the opposite AR journal amount. We cannot rely on the sum of AP invoice amount as AP invoice amount can be different (but within threshold of AR amount)
								var journalAmountFromAR = from n in allTransactions
														  group n by new { n.Ledger, n.OrgPK, n.ParticipantOrgPK, n.TransactionCurrency } into g
														  where g.Key.Ledger == LedgerTypes.AccountsReceivable && g.Key.OrgPK == grp.ParticipantOrgPK && g.Key.ParticipantOrgPK == item.Key.OrgPK && g.Key.TransactionCurrency == grp.TransactionCurrency
														  select g.Sum(p => p.TransactionAmount);

								invertedOsAmount = journalAmountFromAR.FirstOrDefault() * -1; //Header amount is set as negative for both AR and AP journal
							}

							csvContentBuilder.AppendLine($"{AccountingConstants.RemittanceFileRowTypes.NettingClearingLine}, {grp.Ledger}, JNL, {ZDateTime.Today.ToString(dateFormat)}, {ZDateTime.Today.ToString(dateFormat)}, {participantOrgHeader.OH_Code}, {JournalDescription}, {refCurrency.RX_Code}, {invertedOsAmount.ToString(refCurrency.Decimals)}");

							foreach (var transaction in grp.Transactions)
							{
								csvContentBuilder.AppendLine($@"{AccountingConstants.RemittanceFileRowTypes.PaidTransaction}, {grp.Ledger}, {transaction.TransactionType}, {transaction.TransactionReference}, {transaction.TransactionAmount.ToString(refCurrency.Decimals)},,,{ptrDescriptionPrefix + transaction.TransactionReference},,,,{refCurrency.RX_Code}");
							}
						}
					}

					AddJournalsForFXOffersAndRequests(item, csvContentBuilder, nettingSystemOrgHeader.OH_Code);
				}

				SendRemittanceFile(notification, nettingSystemOrgHeader, transactions, csvContentBuilder);
			}
		}

		void AddJournalsForFXOffersAndRequests(IGrouping<object, NettingClearingJournal> item, ZStringBuilder csvContentBuilder, ZString nettingSystemOrg)
		{
			var offersAndRequests = from n in item
									where n.OfferOrRequest
									orderby n.CompanyCode, n.Description
									select n;

			foreach (var offerAndRequest in offersAndRequests)
			{
				var refCurrency = GetRefCurrency(offerAndRequest.TransactionCurrency);

				csvContentBuilder.AppendLine($@"{AccountingConstants.RemittanceFileRowTypes.NettingClearingLine}, {offerAndRequest.Ledger}, JNL, {ZDateTime.Today.ToString(dateFormat)}, {ZDateTime.Today.ToString(dateFormat)}, {nettingSystemOrg}, {offerAndRequest.Description}, {refCurrency.RX_Code}, {offerAndRequest.TransactionAmount.ToString(refCurrency.Decimals)}");
			}
		}

		void AddOpenClearingJournalForNettingSystemOrg(OrgHeader nettingSystemOrgHeader, NettingMovement movement, ZStringBuilder csvContentBuilder)
		{
			var ledger = movement.Direction == "IN" ? LedgerTypes.AccountsPayable : LedgerTypes.AccountsReceivable;
			var journalCurrency = GetRefCurrency(movement.Currency);
			csvContentBuilder.AppendLine($"{AccountingConstants.RemittanceFileRowTypes.NettingClearingLine}, {ledger}, JNL, {ZDateTime.Today.ToString(dateFormat)}, {ZDateTime.Today.ToString(dateFormat)}, {nettingSystemOrgHeader.OH_Code}, {JournalDescription}, {journalCurrency.RX_Code}, {movement.MovementAmount.ToString(journalCurrency.Decimals)}");
		}

		void SendRemittanceFile(NotificationBuffer notification, OrgHeader nettingSystemOrgHeader, IOrderedEnumerable<NettingClearingJournal> transactions, ZStringBuilder csvContentBuilder)
		{
			if (transactions.Any())
			{
				var orgHeader = OrgHeader.GetCompanyOrgProxyFromCompanyCode(Factory, transactions.First().CompanyCode);

				if (orgHeader != null)
				{
					var orgCusCode = orgHeader.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.EHubOrganisationID);
					if (orgCusCode.Length > 0)
					{
#if DEBUG
						var di = Directory.CreateDirectory(Path.Combine(Env.TempPath, period.NSP_Period));
						var fileName = Path.Combine(di.FullName, string.Format(CultureInfo.InvariantCulture, "{0}.csv", orgHeader.OH_Code));

						using (var stream = File.CreateText(fileName))
						{
							stream.Write(csvContentBuilder.ToString());
							stream.Flush();
						}

						if (!TempFilesTestListener.Instance.FilesWithDelayedDelete.Contains(di.FullName))
						{
							TempFilesTestListener.Instance.FilesWithDelayedDelete.Add(di.FullName);
						}
						TempFilesTestListener.Instance.FilesWithDelayedDelete.Add(fileName);
#endif
						var exporter = (INettingClearingJournalExporter)Activator.CreateInstance(ObjectFactory.GetType("INettingClearingJournalExporter")
						, Factory, notification, new NettingCentreStatement(Factory, period.PK));
						exporter.ExportClearingJournal(nettingSystemOrgHeader, orgCusCode[0].OK_CustomsRegNo, csvContentBuilder.ToString());
					}
				}
			}
		}

		RefCurrency GetRefCurrency(ZString currency)
		{
			return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency);
		}

		const string dateFormat = "yyyyMMdd";
		static string JournalDescription => Res.GetString("95bc6045-1c97-42a5-bb27-b4519b17bb16", "Netting Clearing Journal");
	}

	[Serializable]
	public class IncorrectDataSetupException : Exception
	{
		public IncorrectDataSetupException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected IncorrectDataSetupException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class MalformedUniversalXmlException : Exception
	{
		public MalformedUniversalXmlException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected MalformedUniversalXmlException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
