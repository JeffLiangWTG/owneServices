using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Export.Business.TaxFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Export.Business
{
	public abstract class AccountingTransactionExporter
	{
		protected AccountingTransactionExporter(BatchExportDataAccess dataAccess)
		{
			this.DataAccess = dataAccess;
		}

		protected readonly BatchExportDataAccess DataAccess;

		public IDataContextDataObject GetContextDataObject(string companyCode, long batchNumber, string nameSpace)
		{
			if (!nameSpace.IsValidUniversalXmlNamespace())
			{
				throw new XmlProcessingException(string.Format(CultureInfo.InvariantCulture, "Invalid namespace [{0}] - Please use a valid Universal Namespace.", nameSpace ?? "(null)"));
			}
			IDataContextDataObject context = DataContextFactory.New(nameSpace);
			CompanyRow companyRow = DataAccess.LoadCompany(companyCode);
			context.SetCompanyAndDataProviderDetails(companyRow);
			context.AddDataSource(DataContextType.BatchNumber, batchNumber.ToString(CultureInfo.InvariantCulture));
			return context;
		}

		protected TransactionBatch CreateAndInitialiseAccountingBatchDataObject(AccountingTransactionDataObjectWriterStrategy writerStrategy, string companyCode, long batchNumber, string nameSpace)
		{
			return CreateAndInitialiseAccountingBatchDataObject(writerStrategy, companyCode, batchNumber, nameSpace, null);
		}

		protected TransactionBatch CreateAndInitialiseAccountingBatchDataObject(AccountingTransactionDataObjectWriterStrategy writerStrategy, string companyCode, long batchNumber, string nameSpace, ICodeDescription type)
		{
			var accountingBatch = new TransactionBatch(writerStrategy);
			accountingBatch.DataContext = GetContextDataObject(companyCode, batchNumber, nameSpace);
			if (type != null)
			{
				accountingBatch.BatchType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = type.Code, Description = type.Description };
			}

			return accountingBatch;
		}

		protected void PopulateAccountingBatch(AccountingTransactionDataObjectWriterStrategy writerStrategy, string companyCode, long batchNumber, TransactionBatch accountingBatch, HashSet<BatchRow> batchRows)
		{
			var headers = DataAccess.GetTransactionHeaders(writerStrategy, companyCode, batchNumber);
			var lines = DataAccess.GetTransactionLines(companyCode, batchNumber);
			var relatedJournals = DataAccess.GetJournalLines(companyCode, batchNumber);
			var taxTranasctionRows = DataAccess.GetTaxTransactions(writerStrategy, companyCode, batchNumber);
			var (taxGLMovementRows, taxTransactionGLMovementLink) = DataAccess.GetTaxGLMovements(companyCode, batchNumber);
			var (taxLinksAgainstLinePK, linePksAgainstTaxPK) = DataAccess.GetTaxTransactionLinks(companyCode, batchNumber, taxTranasctionRows);

			PopulateAccountingBatch(writerStrategy, new AccountingBatchParameters
			{
				CompanyCode = companyCode,
				AccountingBatch = accountingBatch,
				BatchRows = batchRows,
				HeaderRowDictionary = headers,
				LineRowDictionary = lines,
				RelatedJournalsDictionary = relatedJournals,
				TaxTransactionRowDictionary = taxTranasctionRows,
				TaxGLMovementRowDictionary = taxGLMovementRows,
				TaxLinksAgainstLineDictionary = taxLinksAgainstLinePK,
				TaxTransactionGLMovementLinkDictionary = taxTransactionGLMovementLink,
				LinePKsAgainstTaxPKDictionary = linePksAgainstTaxPK
			});
		}

		protected void PopulateAccountingBatch(AccountingTransactionDataObjectWriterStrategy writerStrategy, AccountingBatchParameters parameters)
		{
			var controlAccounts = new ControlAccounts(DataAccess, parameters.CompanyCode);
			var transfers = new Dictionary<string, TransactionHeaderRow>();
			var contras = new Dictionary<string, TransactionHeaderRow>();

			var i = 0;
			foreach (var batchRow in parameters.BatchRows)
			{
				if (batchRow.ParentTableCode == AccTransactionLinesSchema.Constants.Prefix)
				{
					if (!parameters.LineRowDictionary.TryGetValue(batchRow.ParentID, out var row))
					{
						throw new Exception(string.Format("Unable to load TransactionLine: {0}", batchRow.ParentID.ToString()));
					}

					parameters.HeaderRowDictionary.TryGetValue(row.TransactionHeader, out var headerRow);
					ProcessParentTableCodeAL(writerStrategy, row, headerRow, parameters.AccountingBatch, batchRow, controlAccounts, DataAccess, parameters.TaxLinksAgainstLineDictionary, parameters.RelatedJournalsDictionary);
				}
				else if (batchRow.ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix)
				{
					if (!parameters.HeaderRowDictionary.TryGetValue(batchRow.ParentID, out var row))
					{
						throw new Exception(string.Format("Unable to load TransactionHeader: {0}", batchRow.ParentID.ToString()));
					}

					ProcessParentTableCodeAH(writerStrategy, row, parameters.AccountingBatch, controlAccounts, transfers, contras);
				}
				else if (batchRow.ParentTableCode == AccTaxGLMovementSchema.Constants.Prefix)
				{
					if (!parameters.TaxTransactionGLMovementLinkDictionary.TryGetValue(batchRow.ParentID, out var taxTranactionPK))
					{
						throw new ArgumentException($"Unable to find TaxTranasctionPK for GLMovementPK: {batchRow.ParentID}");
					}

					if (!parameters.TaxTransactionRowDictionary.TryGetValue(taxTranactionPK, out var taxTransactionRow))
					{
						throw new ArgumentException($"Unable to load TaxTransaction: {taxTranactionPK}");
					}

					parameters.HeaderRowDictionary.TryGetValue(taxTransactionRow.TransactionHeaderPK, out var headerRow);

					if (headerRow == null)
					{
						throw new ArgumentException($"Unable to load Transaction Header: {taxTransactionRow.TransactionHeaderPK}");
					}

					if (!parameters.TaxGLMovementRowDictionary.TryGetValue(batchRow.ParentID, out var taxGLMovementRow))
					{
						throw new ArgumentException($"Unable to load GL Movement: {batchRow.ParentID}");
					}

					ProcessParentTableCodeATM(writerStrategy, parameters, taxTranactionPK, taxTransactionRow, headerRow, taxGLMovementRow);
				}
				else if (batchRow.ParentTableCode == AccTaxTransactionSchema.Constants.Prefix)
				{
					if (!parameters.TaxTransactionRowDictionary.TryGetValue(batchRow.ParentID, out var taxTransactionRow))
					{
						throw new ArgumentException($"Unable to load TaxTransaction: {batchRow.ParentID}");
					}

					parameters.HeaderRowDictionary.TryGetValue(taxTransactionRow.TransactionHeaderPK, out var headerRow);

					if (headerRow == null)
					{
						throw new ArgumentException($"Unable to load Transaction Header: {taxTransactionRow.TransactionHeaderPK}");
					}

					ProcessParentTableCodeATT(writerStrategy, parameters, batchRow.ParentID, taxTransactionRow, headerRow);
				}
				else
				{
					throw new Exception(string.Format("Unknown ParentTableCode: {0}", batchRow.ParentTableCode));
				}
				i++;
			}

			var companyPK = parameters.LineRowDictionary.Values.FirstOrDefault(x => x.CompanyPK != Guid.Empty)?.CompanyPK;
			if (!companyPK.HasValue)
			{
				companyPK = DataAccess.LoadCompany(parameters.CompanyCode)?.PK.ToGuid();
			}
			if (companyPK.HasValue)
			{
				PopulatePostingPeriods(parameters.AccountingBatch, companyPK.Value);
				PopulateLocalGLAccounts(parameters.AccountingBatch, companyPK.Value);
				PopulateTotalCashAdvanceReceived(parameters.AccountingBatch.TransactionCollection);
			}
		}

		void ProcessParentTableCodeATM(IDataObjectWriterStrategy writerStrategy, AccountingBatchParameters parameters, ZGuid taxTranactionPK, TaxTransactionRow taxTransactionRow, TransactionHeaderRow headerRow, TaxGLMovementRow taxGLMovementRow)
		{
			ProcessParentTableCodeATT(writerStrategy, parameters, taxTranactionPK, taxTransactionRow, headerRow);

			if (taxTransactionRow.TaxTransaction.PostingJournalDetailCollection == null)
			{
				taxTransactionRow.TaxTransaction.SetPostingJournalDetailCollection(() => new List<PostingJournalDetail> { PopulatePostingJournalDetail(taxGLMovementRow) });
			}
			else
			{
				var postingJournalDetail = PopulatePostingJournalDetail(taxGLMovementRow);
				taxTransactionRow.TaxTransaction.PostingJournalDetailCollection.Add(postingJournalDetail);

#if DEBUG
				taxTransactionRow.TaxTransaction.PostingJournalDetailCollection.Sort((x, y) => x.CreditGLAccount.AccountCode.Value.CompareTo(y.CreditGLAccount.AccountCode.Value));
#endif
			}
		}

		PostingJournalDetail PopulatePostingJournalDetail(TaxGLMovementRow taxGLMovementRow)
		{
			var postingJournalDetail = new PostingJournalDetail();
			postingJournalDetail.DebitGLAccount = new GLAccount();
			postingJournalDetail.DebitGLAccount.AccountCode = taxGLMovementRow.DebitAccountNumber;
			postingJournalDetail.DebitGLAccount.Description = taxGLMovementRow.DebitAccountDescription;

			postingJournalDetail.CreditGLAccount = new GLAccount();
			postingJournalDetail.CreditGLAccount.AccountCode = taxGLMovementRow.CreditAccountNumber;
			postingJournalDetail.CreditGLAccount.Description = taxGLMovementRow.CreditAccountDescription;

			postingJournalDetail.PostingAmount = taxGLMovementRow.PostingAmount;
			postingJournalDetail.PostingDate = taxGLMovementRow.PostingDate;
			postingJournalDetail.PostingPeriod = taxGLMovementRow.PostingPeriod;
			postingJournalDetail.PostingCurrency = new Currency();
			postingJournalDetail.PostingCurrency.Code = taxGLMovementRow.PostingCurrencyCode;
			postingJournalDetail.PostingCurrency.Description = taxGLMovementRow.PostingCurrencyName;

			return postingJournalDetail;
		}

		TransactionLineRow lastLineRow;

		public class AccountingBatchParameters
		{
			public ZString CompanyCode { get; set; }
			public TransactionBatch AccountingBatch { get; set; }
			public HashSet<BatchRow> BatchRows { get; set; }
			public Dictionary<Guid, TransactionHeaderRow> HeaderRowDictionary { get; set; }
			public Dictionary<Guid, TransactionLineRow> LineRowDictionary { get; set; }
			public Dictionary<Guid, List<RelatedJournalLineRow>> RelatedJournalsDictionary { get; set; }
			public Dictionary<ZGuid, TaxTransactionRow> TaxTransactionRowDictionary { get; set; }
			public Dictionary<ZGuid, TaxGLMovementRow> TaxGLMovementRowDictionary { get; set; }
			public Dictionary<ZGuid, List<TaxLink>> TaxLinksAgainstLineDictionary { get; set; }
			public Dictionary<ZGuid, List<ZGuid>> LinePKsAgainstTaxPKDictionary { get; set; }
			public Dictionary<ZGuid, ZGuid> TaxTransactionGLMovementLinkDictionary { get; set; }
		}

		void ProcessParentTableCodeATT(IDataObjectWriterStrategy writerStrategy, AccountingBatchParameters parameters, ZGuid taxTransactionPK, TaxTransactionRow taxTransactionRow, TransactionHeaderRow headerRow)
		{
			AddTransactionIfNotContained(parameters.AccountingBatch, headerRow.Info);
			var transactionInfo = headerRow.Info;

			if (transactionInfo.TaxTransactionCollection == null)
			{
				transactionInfo.SetTaxTransactionCollection(() => new List<TaxTransaction> { taxTransactionRow.TaxTransaction });
			}
			else if (!transactionInfo.TaxTransactionCollection.Contains(taxTransactionRow.TaxTransaction))
			{
				transactionInfo.TaxTransactionCollection.Add(taxTransactionRow.TaxTransaction);
			}

			if (parameters.LinePKsAgainstTaxPKDictionary.TryGetValue(taxTransactionPK, out var linePKs))
			{
				foreach (var linePK in linePKs)
				{
					if (!parameters.BatchRows.Any(x => x.ParentID == linePK))
					{
						var postingJournal = new PostingJournal(writerStrategy);
						AddPostingJournalBatchedAtLineLevelTwice(parameters.LineRowDictionary[linePK.ToGuid()], headerRow, parameters.AccountingBatch, parameters.TaxLinksAgainstLineDictionary, ref postingJournal);
					}
				}
			}
		}

		void PopulatePostingPeriods(TransactionBatch accountingBatch, ZGuid companyPK)
		{
			var postingJournalDetails = accountingBatch.TransactionCollection.SelectMany(x => x.PostingJournalCollection.SelectMany(y => y.PostingJournalDetailCollection).Where(z => z.PostingDate.HasValue && !z.PostingDate.Value.IsEmpty)).ToList();
			if (postingJournalDetails.Any())
			{
				var minPostingDate = postingJournalDetails.Min(x => x.PostingDate).Value.Date;
				var maxPostingDate = postingJournalDetails.Max(x => x.PostingDate).Value.Date;

				var periods = DataAccess.GetPeriodsContaining(minPostingDate, maxPostingDate, companyPK);

				if (periods.Any())
				{
					postingJournalDetails.ForEach(x =>
						{
							var period = periods.FirstOrDefault(y => y.StartDate <= x.PostingDate && y.EndDate >= x.PostingDate);
							if (period != null)
							{
								x.PostingPeriod = period.Period;
							}
						});
				}
			}
		}

		void PopulateTotalCashAdvanceReceived(List<TransactionInfo> transactions)
		{
			foreach (var transaction in transactions)
			{
				if (transaction.PostingJournalCollection != null)
				{
					var cashAdvanceReceivedJournals = transaction.PostingJournalCollection.Where(p => p.CashAdvanceAmount != null);
					if (cashAdvanceReceivedJournals.Any())
					{
						transaction.TotalCashAdvanceAmount = cashAdvanceReceivedJournals.Sum(p => p.CashAdvanceAmount);
					}
				}
			}
		}

		void PopulateLocalGLAccounts(TransactionBatch accountingBatch, ZGuid companyPK)
		{
			var postingJournals = accountingBatch.TransactionCollection.SelectMany(x => x.PostingJournalCollection.Where(y => y.GLAccount != null && y.GLAccount.AccountCode.HasValue)).ToList();
			var postingJournalDetails = accountingBatch.TransactionCollection.SelectMany(x => x.PostingJournalCollection.SelectMany(y => y.PostingJournalDetailCollection));

			var postingJournalDetailGLAccounts = new List<GLAccount>();
			foreach (var postingJournalDetail in postingJournalDetails)
			{
				if (postingJournalDetail.CreditGLAccount != null && postingJournalDetail.CreditGLAccount.AccountCode.HasValue)
				{
					postingJournalDetailGLAccounts.Add(postingJournalDetail.CreditGLAccount);
				}

				if (postingJournalDetail.DebitGLAccount != null && postingJournalDetail.DebitGLAccount.AccountCode.HasValue)
				{
					postingJournalDetailGLAccounts.Add(postingJournalDetail.DebitGLAccount);
				}
			}

			var postingJournalsGLAccounts = postingJournals.Select(x => x.GLAccount.AccountCode.Value);
			var glAccouts = postingJournalsGLAccounts.Concat(postingJournalDetailGLAccounts.Select(x => x.AccountCode.Value));

			if (glAccouts.Any())
			{
				PopulateLocalGLAccountsCore(companyPK, postingJournals, postingJournalDetailGLAccounts, glAccouts.Distinct().ToList());
			}
		}

		void PopulateLocalGLAccountsCore(ZGuid companyPK, List<PostingJournal> postingJournals, List<GLAccount> postingJournalDetailGLAccounts, List<ZString> glAccouts)
		{
			var localAccountMapping = DataAccess.GetLocalGLAccountsMapping(glAccouts, companyPK);
			if (localAccountMapping.Any())
			{
				postingJournals.ForEach(x =>
				{
					if (localAccountMapping.TryGetValue(x.GLAccount.AccountCode.Value, out Tuple<ZString, ZString> localAccount))
					{
						x.LocalGLAccount = localAccount.Item1;
						x.GLAccount.LocalComplianceAccountCode = localAccount.Item1;
						x.GLAccount.LocalComplianceAccountDescription = localAccount.Item2;
					}
				});

				postingJournalDetailGLAccounts.Distinct().ToList().ForEach(x =>
				{
					if (localAccountMapping.TryGetValue(x.AccountCode.Value, out Tuple<ZString, ZString> localAccount))
					{
						x.LocalComplianceAccountCode = localAccount.Item1;
						x.LocalComplianceAccountDescription = localAccount.Item2;
					}
				});
			}
		}

		void PopulatePostingRelatedJournals(BatchRow batchRow, TransactionHeaderRow headerRow, IDataObjectWriterStrategy writerStrategy, Dictionary<Guid, List<RelatedJournalLineRow>> journals)
		{
			if (journals.Any())
			{
				if (batchRow.RowType == Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost)
				{
					var transaction = headerRow.Info;
					List<RelatedJournalLineRow> relatedJournals;
					if (journals.TryGetValue(headerRow.PK, out relatedJournals))
					{
						var apportionments = relatedJournals.Where(x => x.Ledger == LedgerTypes.General
							&& x.TransactionType == TransactionTypes.GLStandardJournal).ToList();

						if (apportionments.Any())
						{
							transaction.SetPostingJournalApportionmentCollection(() =>
							{
								var relatedApportionments = new List<PostingRelatedJournal>();
								foreach (var apportionment in apportionments)
								{
									var postingJournalApportionment = PopulatePostingRelatedJournal(apportionment, isForApportionments: true);
									relatedApportionments.Add(postingJournalApportionment);
								}
								return relatedApportionments;
							});
						}

						var multipleInstallments = relatedJournals.Where(x => x.Ledger == LedgerTypes.AccountsReceivable
							&& (x.TransactionCategory == Constants.TransactionCategory.Codes.ClearingJournal
								|| x.TransactionCategory == Constants.TransactionCategory.Codes.InstalmentJournal)
							&& x.TransactionType == TransactionTypes.Journal).ToList();

						if (multipleInstallments.Any())
						{
							transaction.SetPostingJournalMultipleInstallmentCollection(() =>
							{
								var relatedInstallments = new List<PostingRelatedJournal>();
								foreach (var multipleInstallment in multipleInstallments)
								{
									var postingJournalMultipleInstallment = PopulatePostingRelatedJournal(multipleInstallment, isForApportionments: false);
									relatedInstallments.Add(postingJournalMultipleInstallment);
								}
								return relatedInstallments;
							});
						}
					}
				}
			}

			PostingRelatedJournal PopulatePostingRelatedJournal(RelatedJournalLineRow relatedJournal, bool isForApportionments)
			{
				var postingJournal = new PostingRelatedJournal(writerStrategy);
				postingJournal.Ledger = relatedJournal.Ledger;
				postingJournal.TransactionCategory = relatedJournal.TransactionCategory;
				postingJournal.TransactionType = relatedJournal.TransactionType;
				postingJournal.DueDate = relatedJournal.DueDate;
				postingJournal.ExchangeRate = relatedJournal.ExchangeRate;
				postingJournal.FullyPaidDate = relatedJournal.FullyPaidDate;
				postingJournal.GLAccount = relatedJournal.GLAccount;
				postingJournal.LocalTotal = relatedJournal.LocalTotal;
				postingJournal.LocalCurrency = relatedJournal.LocalCurrency;
				postingJournal.Organization = relatedJournal.Organization;
				postingJournal.OSCurrency = relatedJournal.OSCurrency;
				postingJournal.IsCancelled = relatedJournal.IsCancelled;
				postingJournal.OSTotal = relatedJournal.OSTotal;
				postingJournal.OutstandingAmount = relatedJournal.OutstandingAmount;
				postingJournal.PostDate = relatedJournal.PostDate;
				postingJournal.InvoiceDate = relatedJournal.InvoiceDate;
				postingJournal.Description = relatedJournal.Description;
				if (isForApportionments)
				{
					postingJournal.SetPostingJournalDetailCollection(() => { return PopulateRelatedJournalDetailsForApportionments(relatedJournal); });
				}
				else
				{
					postingJournal.SetPostingJournalDetailCollection(() => { return PopulateRelatedJournalDetailsForMultipleInstallments(relatedJournal); });
				}
				return postingJournal;
			}

			List<PostingJournalDetail> PopulateRelatedJournalDetailsForMultipleInstallments(RelatedJournalLineRow relatedJournal)
			{
				var postingJournalDetail = new PostingJournalDetail();

				if (relatedJournal.OSTotal.Value >= 0m)
				{
					postingJournalDetail.DebitGLAccount = relatedJournal.GLAccount;
				}
				else
				{
					postingJournalDetail.CreditGLAccount = relatedJournal.GLAccount;
				}

				postingJournalDetail.PostingAmount = Math.Abs(relatedJournal.OSTotal.Value);
				postingJournalDetail.PostingCurrency = relatedJournal.LocalCurrency;
				postingJournalDetail.PostingDate = relatedJournal.PostDate.Value;

				var postingJournalDetails = new List<PostingJournalDetail>();
				postingJournalDetails.Add(postingJournalDetail);
				return postingJournalDetails;
			}

			List<PostingJournalDetail> PopulateRelatedJournalDetailsForApportionments(RelatedJournalLineRow relatedJournal)
			{
				var transactionLines = DataAccess.ExportLineRowsOfRelatedJournal(relatedJournal.PK);
				var postingJournalDetails = new List<PostingJournalDetail>();

				foreach (var line in transactionLines)
				{
					var postingJournalDetail = new PostingJournalDetail();
					if (line.LineAmount.Value >= 0m)
					{
						postingJournalDetail.DebitGLAccount = line.GLAccount;
					}
					else
					{
						postingJournalDetail.CreditGLAccount = line.GLAccount;
					}

					postingJournalDetail.PostingAmount = Math.Abs(line.LineAmount.Value);
					postingJournalDetail.PostingCurrency = line.LocalCurrency;
					postingJournalDetail.PostingDate = line.PostDate.Value;
					postingJournalDetails.Add(postingJournalDetail);
				}

				return postingJournalDetails;
			}
		}

		void ProcessParentTableCodeAL(IDataObjectWriterStrategy writerStrategy, TransactionLineRow lineRow, TransactionHeaderRow headerRow, TransactionBatch accountingBatch, BatchRow batchRow, ControlAccounts controlAccounts, BatchExportDataAccess dataAccess, Dictionary<ZGuid, List<TaxLink>> taxTransactionLinkRows, Dictionary<Guid, List<RelatedJournalLineRow>> relatedJournals)
		{
			if (lineRow == null)
			{
				throw new ArgumentNullException(nameof(lineRow), "Null Transaction Line Row.");
			}

			TransactionInfo transactionInfo = null;
			PostingJournal postingJournal = new PostingJournal(writerStrategy);

			if (IsWipOrAccrual(lineRow))
			{
				transactionInfo = new TransactionInfo(writerStrategy);
				PopulateTransactionInfoWipOrAccrual(transactionInfo, lineRow, batchRow);
				accountingBatch.TransactionCollection.Add(transactionInfo);
				PopulatePostingJournalWipOrAccrual(postingJournal, lineRow, batchRow);
				AddPostingJournalSafe(transactionInfo, postingJournal);
				AddPostingJournalDetailWipOrAccrual(postingJournal, lineRow, batchRow, controlAccounts);
			}
			else if (headerRow.Info.Ledger.Value == LedgerTypes.General && (lineRow.LineType.Value == TransactionTypes.GLStandardJournal || lineRow.LineType.Value == TransactionTypes.GLNoteJournal))
			{
				AddTransactionIfNotContained(accountingBatch, headerRow.Info);

				transactionInfo = headerRow.Info;
				PopulatePostingJournalBatchedAtLineLevelOnce(batchRow.RowType, postingJournal, lineRow, headerRow);
				AddPostingJournalSafe(transactionInfo, postingJournal);
				AddPostingJournalDetailGeneralJournal(postingJournal, lineRow);
			}
			else if (headerRow.Info.Ledger.Value == LedgerTypes.General && lineRow.LineType.Value == TransactionTypes.GLAutoJournal)
			{
				AddTransactionIfNotContained(accountingBatch, headerRow.Info);
				AddPostingJournalAutoJournal(writerStrategy, lineRow, headerRow, dataAccess);
			}
			else if (headerRow.Info.Ledger.Value == LedgerTypes.General && lineRow.LineType.Value == TransactionTypes.GLReversingJournal)
			{
				AddTransactionIfNotContained(accountingBatch, headerRow.Info);
				AddPostingJournalReversingJournal(writerStrategy, postingJournal, lineRow, headerRow);
			}
			else if (IsBatchedAtLineLevelOnce(lineRow, headerRow))
			{
				AddTransactionIfNotContained(accountingBatch, headerRow.Info);

				transactionInfo = headerRow.Info;
				PopulatePostingJournalBatchedAtLineLevelOnce(batchRow.RowType, postingJournal, lineRow, headerRow);
				AddPostingJournalSafe(transactionInfo, postingJournal);
				AddPostingJournalDetailBatchedAtLineLevelOnce(postingJournal, lineRow, headerRow, controlAccounts);
			}
			else if (IsBatchedAtLineLevelTwice(lineRow, headerRow))
			{
				AddPostingJournalBatchedAtLineLevelTwice(lineRow, headerRow, accountingBatch, taxTransactionLinkRows, ref postingJournal);
				AddPostingJournalDetailBatchedAtLineLevelTwice(postingJournal, lineRow, headerRow, batchRow, controlAccounts);
				PopulatePostingRelatedJournals(batchRow, headerRow, writerStrategy, relatedJournals);
			}
			else
			{
				throw new ArgumentException("Unexpected Transaction Line Row. " + lineRow.ToString(), nameof(lineRow));
			}

			lastLineRow = lineRow;
		}

		void AddPostingJournalBatchedAtLineLevelTwice(TransactionLineRow lineRow, TransactionHeaderRow headerRow, TransactionBatch accountingBatch, Dictionary<ZGuid, List<TaxLink>> taxTransactionLinkRows, ref PostingJournal postingJournal)
		{
			TransactionInfo transactionInfo = headerRow.Info;
			AddTransactionIfNotContained(accountingBatch, transactionInfo);

			if (lastLineRow != null && lastLineRow.PK == lineRow.PK)
			{
				postingJournal = transactionInfo.PostingJournalCollection.Last();
			}
			else
			{
				PopulatePostingJournalBatchedAtLineLevelTwice(postingJournal, lineRow, headerRow);
				AddPostingJournalSafe(transactionInfo, postingJournal);
			}
			if (taxTransactionLinkRows != null && taxTransactionLinkRows.Any())
			{
				if (taxTransactionLinkRows.TryGetValue(lineRow.PK, out var taxTransactionLinks))
				{
					postingJournal.SetTaxTransactionLinkCollection(() => new List<TaxLink>(taxTransactionLinks));
				}
			}
		}

		void AddTransactionIfNotContained(TransactionBatch accountingBatch, TransactionInfo transactionInfo)
		{
			if (!accountingBatch.TransactionCollection.Contains(transactionInfo))
			{
				accountingBatch.TransactionCollection.Add(transactionInfo);
			}
		}

		void AddPostingJournalSafe(TransactionInfo info, PostingJournal journal)
		{
			if (info.PostingJournalCollection != null || info.SetPostingJournalCollection(() => new List<PostingJournal>()))
			{
				info.PostingJournalCollection.Add(journal);
			}
		}

		void AddPostingJournalDetailReversingJournal(PostingJournal postingJournal, PostingJournal postingJournalReverse, TransactionLineRow lineRow)
		{
			AddPostingJournalDetailGeneralJournal(postingJournal, lineRow);

			PostingJournalDetail detailReverse = new PostingJournalDetail();

			if (lineRow.LineAmount < 0m)
			{
				detailReverse.DebitGLAccount = lineRow.GLAccount;
			}
			else
			{
				detailReverse.CreditGLAccount = lineRow.GLAccount;
			}

			detailReverse.PostingAmount = Math.Abs(lineRow.LineAmount.Value);
			detailReverse.PostingCurrency = lineRow.LocalCurrency;
			detailReverse.PostingDate = lineRow.ReverseDate.Value;
			postingJournalReverse.PostingJournalDetailCollection.Add(detailReverse);
		}

		void AddPostingJournalReversingJournal(IDataObjectWriterStrategy writerStrategy, PostingJournal postingJournal, TransactionLineRow lineRow, TransactionHeaderRow headerRow)
		{
			TransactionInfo transactionInfo = headerRow.Info;

			PopulatePostingJournalBatchedAtLineLevelOnce(Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost, postingJournal, lineRow, headerRow);
			AddPostingJournalSafe(transactionInfo, postingJournal);

			PostingJournal postingJournalReverse = new PostingJournal(writerStrategy);
			PopulatePostingJournalBatchedAtLineLevelOnce(Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost, postingJournalReverse, lineRow, headerRow);
			postingJournalReverse.LocalTotalAmount *= -1;
			postingJournalReverse.LocalGSTVATAmount *= -1;
			postingJournalReverse.LocalExtraVATAmount *= -1;
			AddPostingJournalSafe(transactionInfo, postingJournalReverse);

			AddPostingJournalDetailReversingJournal(postingJournal, postingJournalReverse, lineRow);
		}

		void AddPostingJournalDetailAutoJournal(PostingJournal postingJournal, TransactionLineRow lineRow, ZDateTime postDate)
		{
			PostingJournalDetail detail = AddPostingJournalDetailGeneralJournal(postingJournal, lineRow);
			detail.PostingDate = postDate;
		}

		void AddPostingJournalAutoJournal(IDataObjectWriterStrategy writerStrategy, TransactionLineRow lineRow, TransactionHeaderRow headerRow, BatchExportDataAccess dataAccess)
		{
			TransactionInfo transactionInfo = headerRow.Info;

			ZDateTime startDate = lineRow.PostDate.Value;
			ZDateTime endDate = lineRow.ReverseDate.Value;
			ZDateTime[] dates = dataAccess.GetPeriodEndDatesBetween(startDate, endDate, lineRow.CompanyPK);

			foreach (ZDateTime date in dates)
			{
				PostingJournal postingJournal = new PostingJournal(writerStrategy);
				PopulatePostingJournalBatchedAtLineLevelOnce(Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost, postingJournal, lineRow, headerRow);
				postingJournal.GLPostDate = date;
				AddPostingJournalSafe(transactionInfo, postingJournal);

				AddPostingJournalDetailAutoJournal(postingJournal, lineRow, date);
			}
		}

		PostingJournalDetail AddPostingJournalDetailGeneralJournal(PostingJournal postingJournal, TransactionLineRow lineRow)
		{
			PostingJournalDetail detail = new PostingJournalDetail();

			if (lineRow.LineAmount >= 0m)
			{
				detail.DebitGLAccount = lineRow.GLAccount;
			}
			else
			{
				detail.CreditGLAccount = lineRow.GLAccount;
			}

			detail.PostingAmount = Math.Abs(lineRow.LineAmount.Value);
			detail.PostingCurrency = lineRow.LocalCurrency;
			detail.PostingDate = lineRow.PostDate.Value;
			postingJournal.PostingJournalDetailCollection.Add(detail);
			return detail;
		}

		void ProcessParentTableCodeAH(IDataObjectWriterStrategy writerStrategy, TransactionHeaderRow headerRow, TransactionBatch accountingBatch, ControlAccounts controlAccounts, Dictionary<string, TransactionHeaderRow> transfers, Dictionary<string, TransactionHeaderRow> contras)
		{
			if (headerRow == null)
			{
				throw new ArgumentNullException(nameof(headerRow), "Null Transaction Header Row.");
			}
			if (!IsHeaderExport(headerRow))
			{
				throw new ArgumentException("Unexpected Transaction Header. " + headerRow.ToString(), nameof(headerRow));
			}

			if ((headerRow.Info.Ledger.Value == LedgerTypes.AccountsReceivable || headerRow.Info.Ledger.Value == LedgerTypes.AccountsPayable || headerRow.Info.Ledger.Value == LedgerTypes.CashBook) && headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Transfer)
			{
				ProcessTransfer(writerStrategy, headerRow, accountingBatch, controlAccounts, transfers);
			}
			else if ((headerRow.Info.Ledger.Value == LedgerTypes.AccountsReceivable || headerRow.Info.Ledger.Value == LedgerTypes.AccountsPayable) && headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Contra)
			{
				ProcessContra(writerStrategy, headerRow, accountingBatch, controlAccounts, contras);
			}
			else
			{
				var transactionInfo = headerRow.Info;
				accountingBatch.TransactionCollection.Add(transactionInfo);

				var postingJournal = new PostingJournal(writerStrategy);
				PopulatePostingJournalHeaderExport(postingJournal, headerRow);
				AddPostingJournalSafe(transactionInfo, postingJournal);
				AddPostingJournalDetailHeaderExport(postingJournal, headerRow, controlAccounts);
			}
		}

		void ProcessTransfer(IDataObjectWriterStrategy writerStrategy, TransactionHeaderRow headerRow, TransactionBatch accountingBatch, ControlAccounts controlAccounts, Dictionary<string, TransactionHeaderRow> transfers)
		{
			if (!IsHeaderExport(headerRow)
				|| !(headerRow.Info.Ledger.Value == LedgerTypes.AccountsReceivable || headerRow.Info.Ledger.Value == LedgerTypes.AccountsPayable || headerRow.Info.Ledger.Value == LedgerTypes.CashBook) || headerRow.Info.TransactionType.Value.ToString() != TransactionTypes.Transfer)
			{
				throw new ArgumentException("Batch Row must be AR/AP/CB Transfer", nameof(headerRow));
			}

			if (transfers.ContainsKey(headerRow.Info.Ledger + headerRow.Info.Number))
			{
				TransactionInfo transactionInfo = headerRow.Info;
				accountingBatch.TransactionCollection.Add(transactionInfo);

				PostingJournal postingJournal = new PostingJournal(writerStrategy);
				PopulatePostingJournalHeaderExport(postingJournal, headerRow);

				TransactionHeaderRow headerRow2 = transfers[headerRow.Info.Ledger + headerRow.Info.Number];

				PostingJournal postingJournal2 = new PostingJournal(writerStrategy);
				PopulatePostingJournalHeaderExport(postingJournal2, headerRow2);

				if (headerRow.TransactionCount < headerRow2.TransactionCount)
				{
					AddPostingJournalSafe(transactionInfo, postingJournal);
					AddPostingJournalSafe(transactionInfo, postingJournal2);
				}
				else
				{
					AddPostingJournalSafe(transactionInfo, postingJournal2);
					AddPostingJournalSafe(transactionInfo, postingJournal);
				}

				if (headerRow.Info.Ledger.Value == LedgerTypes.CashBook)
				{
					AddPostingJournalDetailHeaderExportCashBookTransfer(postingJournal, headerRow);
					AddPostingJournalDetailHeaderExportCashBookTransfer(postingJournal2, headerRow2);
				}
				else if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsReceivable || headerRow.Info.Ledger.Value == LedgerTypes.AccountsPayable)
				{
					AddPostingJournalDetailHeaderExportARAPTransfer(postingJournal, headerRow, controlAccounts);
					AddPostingJournalDetailHeaderExportARAPTransfer(postingJournal2, headerRow2, controlAccounts);
				}

				var batchNumberDataSource = accountingBatch.DataContext.DataSourceCollection.FirstOrDefault(x => (x.Type ?? string.Empty) == nameof(DataContextType.BatchNumber));
				if (batchNumberDataSource != null && (batchNumberDataSource.Key ?? string.Empty) == "-1")
				{
					headerRow2.Info.PostingJournalCollection.AddRange(transactionInfo.PostingJournalCollection);
				}

				transfers.Remove(headerRow.Info.Ledger + headerRow.Info.Number);
			}
			else
			{
				transfers.Add(headerRow.Info.Ledger + headerRow.Info.Number, headerRow);
			}
		}

		void ProcessContra(IDataObjectWriterStrategy writerStrategy, TransactionHeaderRow headerRow, TransactionBatch accountingBatch, ControlAccounts controlAccounts, Dictionary<string, TransactionHeaderRow> contras)
		{
			if (!IsHeaderExport(headerRow)
				|| !(headerRow.Info.Ledger.Value == LedgerTypes.AccountsReceivable || headerRow.Info.Ledger.Value == LedgerTypes.AccountsPayable) || headerRow.Info.TransactionType.Value.ToString() != TransactionTypes.Contra)
			{
				throw new ArgumentException("Batch Row must be AR/AP Contra", nameof(headerRow));
			}

			if (contras.ContainsKey(headerRow.Info.Number))
			{
				TransactionInfo transactionInfo = headerRow.Info;
				accountingBatch.TransactionCollection.Add(transactionInfo);

				PostingJournal postingJournal = new PostingJournal(writerStrategy);
				PopulatePostingJournalHeaderExport(postingJournal, headerRow);

				TransactionHeaderRow headerRow2 = contras[headerRow.Info.Number];

				PostingJournal postingJournal2 = new PostingJournal(writerStrategy);
				PopulatePostingJournalHeaderExport(postingJournal2, headerRow2);

				if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsReceivable)
				{
					AddPostingJournalSafe(transactionInfo, postingJournal);
					AddPostingJournalSafe(transactionInfo, postingJournal2);
				}
				else
				{
					AddPostingJournalSafe(transactionInfo, postingJournal2);
					AddPostingJournalSafe(transactionInfo, postingJournal);
				}

				AddPostingJournalDetailHeaderExportContra(postingJournal, headerRow, controlAccounts);
				AddPostingJournalDetailHeaderExportContra(postingJournal2, headerRow2, controlAccounts);

				var batchNumberDataSource = accountingBatch.DataContext.DataSourceCollection.FirstOrDefault(x => (x.Type ?? string.Empty) == nameof(DataContextType.BatchNumber));
				if (batchNumberDataSource != null && (batchNumberDataSource.Key ?? string.Empty) == "-1")
				{
					headerRow2.Info.PostingJournalCollection.AddRange(transactionInfo.PostingJournalCollection);
				}

				contras.Remove(headerRow.Info.Number);
			}
			else
			{
				contras.Add(headerRow.Info.Number, headerRow);
			}
		}

		void AddPostingJournalDetailHeaderExportContra(PostingJournal postingJournal, TransactionHeaderRow headerRow, ControlAccounts controlAccounts)
		{
			GLAccount debit = null;
			GLAccount credit = null;

			//if AP is +ve or AR is -ve = positive transaction

			if (headerRow.Info.LocalExVATAmount >= 0m)
			{
				if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsReceivable)
				{
					debit = controlAccounts.ARControlAccount;
				}
				else if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsPayable)
				{
					debit = controlAccounts.APControlAccount;
				}
			}
			else
			{
				if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsReceivable)
				{
					credit = controlAccounts.ARControlAccount;
				}
				else if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsPayable)
				{
					credit = controlAccounts.APControlAccount;
				}
			}

			PostingJournalDetail detail = new PostingJournalDetail();
			detail.DebitGLAccount = debit;
			detail.CreditGLAccount = credit;
			detail.PostingDate = headerRow.Info.PostDate.Value;
			detail.PostingAmount = Math.Abs(headerRow.Info.LocalExVATAmount.Value);
			detail.PostingCurrency = headerRow.Info.LocalCurrency;
			postingJournal.PostingJournalDetailCollection.Add(detail);
		}

		void AddPostingJournalDetailHeaderExportARAPTransfer(PostingJournal postingJournal, TransactionHeaderRow headerRow, ControlAccounts controlAccounts)
		{
			GLAccount debit = null;
			GLAccount credit = null;

			decimal multiplier = (headerRow.TransactionCount == 1 && headerRow.Info.Ledger.Value == LedgerTypes.AccountsReceivable)
				|| (headerRow.TransactionCount == 2 && headerRow.Info.Ledger.Value == LedgerTypes.AccountsPayable) ? -1 : 1;

			decimal amount = headerRow.Info.LocalExVATAmount.Value * multiplier;

			if (headerRow.TransactionCount == 1)    //from
			{
				if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsReceivable)
				{
					if (amount >= 0m)
					{
						credit = controlAccounts.ARControlAccount;
					}
					else
					{
						debit = controlAccounts.ARControlAccount;
					}
				}
				else if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsPayable)
				{
					if (amount >= 0m)
					{
						debit = controlAccounts.APControlAccount;
					}
					else
					{
						credit = controlAccounts.APControlAccount;
					}
				}
			}
			else  //to
			{
				if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsReceivable)
				{
					if (amount >= 0m)
					{
						debit = controlAccounts.ARControlAccount;
					}
					else
					{
						credit = controlAccounts.ARControlAccount;
					}
				}
				else if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsPayable)
				{
					if (amount >= 0m)
					{
						credit = controlAccounts.APControlAccount;
					}
					else
					{
						debit = controlAccounts.APControlAccount;
					}
				}
			}

			PostingJournalDetail detail = new PostingJournalDetail();
			detail.DebitGLAccount = debit;
			detail.CreditGLAccount = credit;
			detail.PostingDate = headerRow.Info.PostDate.Value;
			detail.PostingAmount = Math.Abs(amount);
			detail.PostingCurrency = headerRow.Info.LocalCurrency;
			postingJournal.PostingJournalDetailCollection.Add(detail);
		}

		void AddPostingJournalDetailHeaderExportCashBookTransfer(PostingJournal postingJournal, TransactionHeaderRow headerRow)
		{
			GLAccount debit = null;
			GLAccount credit = null;

			if (headerRow.Info.LocalExVATAmount >= 0m)
			{
				debit = headerRow.BankGLAccount;
			}
			else
			{
				credit = headerRow.BankGLAccount;
			}

			PostingJournalDetail detail = new PostingJournalDetail();
			detail.DebitGLAccount = debit;
			detail.CreditGLAccount = credit;
			detail.PostingDate = headerRow.Info.PostDate;
			detail.PostingAmount = Math.Abs(headerRow.Info.LocalExVATAmount.Value);
			detail.PostingCurrency = headerRow.Info.LocalCurrency;
			postingJournal.PostingJournalDetailCollection.Add(detail);
		}

		void AddPostingJournalDetailHeaderExportAR(PostingJournal postingJournal, TransactionHeaderRow headerRow, ControlAccounts controlAccounts)
		{
			PostingJournalDetail detail = new PostingJournalDetail();

			decimal multiplier = 1m;

			if (headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Payment)
			{
				detail.DebitGLAccount = controlAccounts.ARControlAccount;
				detail.CreditGLAccount = headerRow.BankGLAccount;
			}
			else if (headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Receipt)
			{
				detail.DebitGLAccount = headerRow.BankGLAccount;
				detail.CreditGLAccount = controlAccounts.ARControlAccount;
				multiplier = -1;
			}
			else if (headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Discount
				|| headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Overpayment
				|| headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.ExchangeDifference)
			{
				detail.DebitGLAccount = controlAccounts.ARControlAccount;
				detail.CreditGLAccount = headerRow.GLAccount;
			}
			else if (headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Journal)
			{
				detail.DebitGLAccount = controlAccounts.ARControlAccount;
				detail.CreditGLAccount = headerRow.GLAccount;
			}
			else
			{
				detail.DebitGLAccount = controlAccounts.ARControlAccount;
				detail.CreditGLAccount = headerRow.GLAccount;
			}

			decimal amount = headerRow.Info.LocalExVATAmount.Value * multiplier;

			if (amount < 0)
			{
				GLAccount temp = detail.DebitGLAccount;
				detail.DebitGLAccount = detail.CreditGLAccount;
				detail.CreditGLAccount = temp;
			}

			detail.PostingDate = headerRow.Info.PostDate.Value;
			detail.PostingAmount = Math.Abs(headerRow.Info.LocalExVATAmount.Value);
			detail.PostingCurrency = headerRow.Info.LocalCurrency;
			postingJournal.PostingJournalDetailCollection.Add(detail);
		}

		void AddPostingJournalDetailHeaderExportAP(PostingJournal postingJournal, TransactionHeaderRow headerRow, ControlAccounts controlAccounts)
		{
			PostingJournalDetail detail = new PostingJournalDetail();
			decimal multiplier = 1m;

			if (headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Payment)
			{
				detail.DebitGLAccount = controlAccounts.APControlAccount;
				detail.CreditGLAccount = headerRow.BankGLAccount;
			}
			else if (headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Receipt)
			{
				detail.DebitGLAccount = headerRow.BankGLAccount;
				detail.CreditGLAccount = controlAccounts.APControlAccount;
				multiplier = -1m;
			}
			else if (headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Discount
				|| headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Overpayment
				|| headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.ExchangeDifference)
			{
				detail.DebitGLAccount = controlAccounts.APControlAccount;
				detail.CreditGLAccount = headerRow.GLAccount;
			}
			else if (headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Journal)
			{
				detail.DebitGLAccount = controlAccounts.APControlAccount;
				detail.CreditGLAccount = headerRow.GLAccount;
			}
			else
			{
				detail.DebitGLAccount = controlAccounts.APControlAccount;
				detail.CreditGLAccount = headerRow.GLAccount;
			}

			decimal amount = headerRow.Info.LocalExVATAmount.Value * multiplier;

			if (amount < 0)
			{
				GLAccount temp = detail.DebitGLAccount;
				detail.DebitGLAccount = detail.CreditGLAccount;
				detail.CreditGLAccount = temp;
			}

			detail.PostingDate = headerRow.Info.PostDate.Value;
			detail.PostingAmount = Math.Abs(headerRow.Info.LocalExVATAmount.Value);
			detail.PostingCurrency = headerRow.Info.LocalCurrency;
			postingJournal.PostingJournalDetailCollection.Add(detail);
		}

		void AddPostingJournalDetailHeaderExport(PostingJournal postingJournal, TransactionHeaderRow headerRow, ControlAccounts controlAccounts)
		{
			if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsReceivable)
			{
				AddPostingJournalDetailHeaderExportAR(postingJournal, headerRow, controlAccounts);
			}
			else if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsPayable)
			{
				var isPBWJournal = headerRow.Info.TransactionType.Value == TransactionType.JNL && headerRow.Info.Category.Value == Constants.TransactionCategory.Codes.PaymentBasisWithholding;
				if (!isPBWJournal)
				{
					AddPostingJournalDetailHeaderExportAP(postingJournal, headerRow, controlAccounts);
				}
			}
			else if (headerRow.Info.Ledger.Value == LedgerTypes.CashBook && headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.ExchangeDifference)
			{
				postingJournal.PostingJournalDetailCollection.Add(CreatePostingJournalDetail(headerRow.BankGLAccount, headerRow.GLAccount, headerRow.Info.PostDate.Value, headerRow.Info.LocalExVATAmount.Value, headerRow.Info.LocalCurrency));
			}
		}

		void AddPostingJournalDetailWipOrAccrual(PostingJournal postingJournal, TransactionLineRow lineRow, BatchRow batchRow, ControlAccounts controlAccounts)
		{
			if (batchRow.RowType == Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost)
			{
				var detail = new PostingJournalDetail();
				if (lineRow.LineType.Value == TransactionLineTypes.WIP)
				{
					if (lineRow.LineAmount < 0)
					{
						detail.DebitGLAccount = controlAccounts.WIPControlAccount;
						detail.CreditGLAccount = lineRow.GLAccount;
					}
					else
					{
						detail.DebitGLAccount = lineRow.GLAccount;
						detail.CreditGLAccount = controlAccounts.WIPControlAccount;
					}
				}
				else if (lineRow.LineType.Value == TransactionLineTypes.Accrual)
				{
					if (lineRow.LineAmount >= 0)
					{
						detail.DebitGLAccount = lineRow.GLAccount;
						detail.CreditGLAccount = controlAccounts.AccrualControlAccount;
					}
					else
					{
						detail.DebitGLAccount = controlAccounts.AccrualControlAccount;
						detail.CreditGLAccount = lineRow.GLAccount;
					}
				}
				detail.PostingDate = lineRow.PostDate.Value;
				detail.PostingCurrency = lineRow.LocalCurrency;
				detail.PostingAmount = Math.Abs(lineRow.LineAmount.Value);
				postingJournal.PostingJournalDetailCollection.Add(detail);
			}
			else if (batchRow.RowType == Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServiceReverse)
			{
				var detail = new PostingJournalDetail();
				if (lineRow.LineType.Value == TransactionLineTypes.WIP)
				{
					if (lineRow.LineAmount < 0)
					{
						detail.DebitGLAccount = lineRow.GLAccount;
						detail.CreditGLAccount = controlAccounts.WIPControlAccount;
					}
					else
					{
						detail.DebitGLAccount = controlAccounts.WIPControlAccount;
						detail.CreditGLAccount = lineRow.GLAccount;
					}
				}
				else if (lineRow.LineType.Value == TransactionLineTypes.Accrual)
				{
					if (lineRow.LineAmount >= 0)
					{
						detail.DebitGLAccount = controlAccounts.AccrualControlAccount;
						detail.CreditGLAccount = lineRow.GLAccount;
					}
					else
					{
						detail.DebitGLAccount = lineRow.GLAccount;
						detail.CreditGLAccount = controlAccounts.AccrualControlAccount;
					}
				}
				detail.PostingDate = lineRow.ReverseDate.Value;
				detail.PostingCurrency = lineRow.LocalCurrency;
				detail.PostingAmount = Math.Abs(lineRow.LineAmount.Value);
				postingJournal.PostingJournalDetailCollection.Add(detail);
			}
		}

		void AddPostingJournalDetailBatchedAtLineLevelOnce(PostingJournal postingJournal, TransactionLineRow lineRow, TransactionHeaderRow headerRow, ControlAccounts controlAccounts)
		{
			if (headerRow.Info.Ledger.Value == LedgerTypes.CashBook)
			{
				bool isInput = headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.DirectPayment && lineRow.LineType.Value == TransactionTypes.DirectPayment;
				bool isOutput = headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.DirectReceipt && lineRow.LineType.Value == TransactionTypes.DirectReceipt;

				if (isInput || isOutput)
				{
					postingJournal.PostingJournalDetailCollection.Add(CreatePostingJournalDetail(headerRow.BankGLAccount, lineRow.GLAccount, lineRow.PostDate.Value, lineRow.LineAmount.Value, lineRow.LocalCurrency));
					if (lineRow.GSTVAT != 0m)
					{
						var gstAccount = isInput ? controlAccounts.GSTInputAccount : controlAccounts.GSTOutputAccount;
						if (lineRow.InputGSTVATRecoverable != 1 && lineRow.GSTVATRecoverable.HasValue && lineRow.GSTVATNotRecoverable.HasValue)
						{
							postingJournal.PostingJournalDetailCollection.Add(CreatePostingJournalDetail(headerRow.BankGLAccount, gstAccount, lineRow.PostDate.Value, lineRow.GSTVATRecoverable.Value, lineRow.LocalCurrency));
							postingJournal.PostingJournalDetailCollection.Add(CreatePostingJournalDetail(headerRow.BankGLAccount, lineRow.GLAccount, lineRow.PostDate.Value, lineRow.GSTVATNotRecoverable.Value, lineRow.LocalCurrency));
						}
						else
						{
							postingJournal.PostingJournalDetailCollection.Add(CreatePostingJournalDetail(headerRow.BankGLAccount, gstAccount, lineRow.PostDate.Value, lineRow.GSTVAT.Value, lineRow.LocalCurrency));
						}
					}
				}
			}
		}

		void AddPostingJournalDetailBatchedAtLineLevelTwice(PostingJournal postingJournal, TransactionLineRow lineRow, TransactionHeaderRow headerRow, BatchRow batchRow, ControlAccounts controlAccounts)
		{
			var ledger = headerRow.Info.Ledger.Value.ToString();
			var transactionType = headerRow.Info.TransactionType.Value.ToString();
			var lineType = lineRow.LineType.Value.ToString();

			if (batchRow.RowType == Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost)
			{
				if (lineType == TransactionLineTypes.Revenue)
				{
					AddPostingJournalDetail(controlAccounts.ARControlAccount, controlAccounts.ARControlSuspenseAccount);
				}
				else if (lineType == TransactionLineTypes.Cost)
				{
					AddPostingJournalDetail(controlAccounts.APControlAccount, controlAccounts.APControlSuspenseAccount);
				}
			}
			else if (batchRow.RowType == Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServiceReverse)
			{
				var controlSuspenseAccount = lineType == TransactionLineTypes.Revenue ? controlAccounts.ARControlSuspenseAccount : controlAccounts.APControlSuspenseAccount;
				if (transactionType == TransactionTypes.JobRevenueJournal)
				{
					controlSuspenseAccount = controlAccounts.ARControlSuspenseAccount;
				}

				postingJournal.PostingJournalDetailCollection.Add(CreatePostingJournalDetail(controlSuspenseAccount, lineRow.GLAccount, lineRow.ReverseDate.Value, lineRow.LineAmount.Value, lineRow.LocalCurrency));
			}

			void AddPostingJournalDetail(GLAccount controlAccount, GLAccount suspenseAccount)
			{
				if (!lineRow.IsCashBasisVATOnly)
				{
					if (ledger == LedgerTypes.JobCosting)
					{
						if (transactionType == TransactionTypes.JobRevenueJournal)
						{
							controlAccount = controlAccounts.JobRevenueJournalAccount;
							suspenseAccount = controlAccounts.ARControlSuspenseAccount;
						}
						else if (transactionType == TransactionTypes.Journal)
						{
							controlAccount = controlAccounts.GetCFXAccount(lineRow.CompanyPK, lineRow.BranchPK, lineRow.DepartmentPK);
						}
					}

					postingJournal.PostingJournalDetailCollection.Add(CreatePostingJournalDetail(controlAccount, suspenseAccount, lineRow.PostDate.Value, lineRow.LineAmount.Value, lineRow.LocalCurrency));
				}

				if (lineRow.GSTVAT != 0m && ledger != LedgerTypes.JobCosting && (transactionType != TransactionTypes.Journal && transactionType != TransactionTypes.JobRevenueJournal))
				{
					postingJournal.PostingJournalDetailCollection.AddRange(CreatePostingJournalDetailsForInvoiceTaxAmounts(lineRow, controlAccounts));
				}
			}
		}

		List<PostingJournalDetail> CreatePostingJournalDetailsForInvoiceTaxAmounts(TransactionLineRow lineRow, ControlAccounts controlAccounts)
		{
			var result = new List<PostingJournalDetail>();

			var isCost = lineRow.LineType.Value == TransactionLineTypes.Cost;
			var gstAccount = isCost ? controlAccounts.GSTInputAccount : controlAccounts.GSTOutputAccount;
			var gstAccountPending = isCost ? controlAccounts.PendingGSTInputAccount : controlAccounts.PendingGSTOutputAccount;
			var isVATRecoverable = isCost && lineRow.InputGSTVATRecoverable != 1 && lineRow.GSTVATRecoverable.HasValue && lineRow.GSTVATNotRecoverable.HasValue;

			if (!lineRow.IsCashBasisVATOnly)
			{
				var controlAccount = isCost ? controlAccounts.APControlAccount : controlAccounts.ARControlAccount;
				var isCashBasisVAT = lineRow.GSTVATBasis.Value == Constants.TransactionLineTaxBasisTypes.Cash;

				if (!isCashBasisVAT && isVATRecoverable)
				{
					result.Add(CreatePostingJournalDetail(controlAccount, gstAccount, lineRow.PostDate.Value, lineRow.GSTVATRecoverable.Value, lineRow.LocalCurrency));
					result.Add(CreatePostingJournalDetail(controlAccount, lineRow.GLAccount, lineRow.PostDate.Value, lineRow.GSTVATNotRecoverable.Value, lineRow.LocalCurrency));
				}
				else
				{
					result.Add(CreatePostingJournalDetail(controlAccount, isCashBasisVAT ? gstAccountPending : gstAccount, lineRow.PostDate.Value, lineRow.GSTVAT.Value, lineRow.LocalCurrency));
				}
			}

			foreach (var cashVAT in lineRow.CashBasisVATLines)
			{
				if (isVATRecoverable)
				{
					var vatRecoverable = Utilities.Round(cashVAT.TaxAmount * lineRow.InputGSTVATRecoverable, lineRow.LocalCurrencyDecimals);
					var vatNotRecoverable = cashVAT.TaxAmount - vatRecoverable;
					result.Add(CreatePostingJournalDetail(gstAccountPending, gstAccount, cashVAT.PostDate, vatRecoverable, lineRow.LocalCurrency));
					result.Add(CreatePostingJournalDetail(gstAccountPending, lineRow.GLAccount, cashVAT.PostDate, vatNotRecoverable, lineRow.LocalCurrency));
				}
				else
				{
					result.Add(CreatePostingJournalDetail(gstAccountPending, gstAccount, cashVAT.PostDate, cashVAT.TaxAmount, lineRow.LocalCurrency));
				}
			}

			return result;
		}

		PostingJournalDetail CreatePostingJournalDetail(GLAccount debitGLAccountForPositiveAmount, GLAccount creditGLAccountForPositiveAmount, ZDateTime postingDate, ZDecimal postingAmount, Currency postingCurrency)
		{
			var detail = new PostingJournalDetail();

			if (postingAmount >= 0)
			{
				detail.DebitGLAccount = debitGLAccountForPositiveAmount;
				detail.CreditGLAccount = creditGLAccountForPositiveAmount;
			}
			else
			{
				detail.DebitGLAccount = creditGLAccountForPositiveAmount;
				detail.CreditGLAccount = debitGLAccountForPositiveAmount;
			}

			detail.PostingDate = postingDate;
			detail.PostingAmount = Math.Abs(postingAmount);
			detail.PostingCurrency = postingCurrency;

			return detail;
		}

		void PopulateTransactionInfoWipOrAccrual(TransactionInfo transactionInfo, TransactionLineRow lineRow, BatchRow batchRow)
		{
			transactionInfo.Ledger = LedgerTypes.JobCosting;
			transactionInfo.TransactionType = GetTransactionTypeEnum(lineRow.LineType);
			transactionInfo.Number = null;
			transactionInfo.Description = lineRow.Description;
			transactionInfo.TransactionDate = lineRow.PostDate;
			transactionInfo.Category = null;
			transactionInfo.DueDate = null;
			ZDecimal lineAmount = Math.Abs(lineRow.LineAmount.Value);
			ZDecimal osAmount = Math.Abs(lineRow.OSAmount.Value);
			if (WipOrAccrualAmountsShouldBeNegated(batchRow.RowType, lineRow.LineAmount.Value))
			{
				lineAmount *= -1;
				osAmount *= -1;
			}

			transactionInfo.LocalExVATAmount = lineAmount;
			transactionInfo.LocalVATAmount = null;
			transactionInfo.LocalTotal = lineAmount;
			transactionInfo.LocalWHTAmount = null;
			transactionInfo.OSCurrency = lineRow.LocalCurrency;
			transactionInfo.OSExGSTVATAmount = osAmount;
			transactionInfo.OSGSTVATAmount = null;
			transactionInfo.OSTotal = osAmount;
			transactionInfo.OSWHTAmount = null;
			transactionInfo.PostDate = lineRow.PostDate;
			transactionInfo.CheckNumberOrPaymentRef = null;
			transactionInfo.PaymentOrReceiptType = null;
			transactionInfo.CheckDrawer = null;
			transactionInfo.DrawerBank = null;
			transactionInfo.DrawerBranch = null;
			transactionInfo.JobInvoiceNumber = null;
			transactionInfo.FullyPaidDate = null;
			transactionInfo.IsPrinted = null;
			transactionInfo.IsCancelled = null;
			transactionInfo.DateClearedInCashBook = null;
			transactionInfo.OutstandingAmount = null;
			transactionInfo.ReceiptOrDirectDebitNumber = null;
			transactionInfo.IsCreatedByMatchingProcess = null;
			transactionInfo.InvoiceTerm = null;
			transactionInfo.InvoiceTermDays = null;
			transactionInfo.BankAccount = null;
			transactionInfo.Job = lineRow.Job;
			transactionInfo.Branch = lineRow.Branch;
			transactionInfo.Department = lineRow.Department;
			transactionInfo.RequisitionDate = null;
			transactionInfo.RequisitionStatus = null;
			transactionInfo.CreateTime = null;
			transactionInfo.CreateUser = null;
			transactionInfo.NumberOfSupportingDocuments = null;
			transactionInfo.ExternalCreditorCode = lineRow.ExternalCreditorCode;
			transactionInfo.ExternalDebtorCode = lineRow.ExternalDebtorCode;
		}

		void PopulatePostingJournalHeaderExport(PostingJournal postingJournal, TransactionHeaderRow headerRow)
		{
			postingJournal.TransactionType = headerRow.Info.TransactionType;
			postingJournal.Description = headerRow.Info.Description;
			postingJournal.LocalCurrency = headerRow.Info.LocalCurrency;
			postingJournal.LocalAmount = headerRow.Info.LocalExVATAmount;
			postingJournal.LocalGSTVATAmount = headerRow.Info.LocalVATAmount;

			postingJournal.TransactionType = headerRow.Info.TransactionType;
			postingJournal.Description = headerRow.Info.Description;
			postingJournal.LocalCurrency = headerRow.Info.LocalCurrency;
			postingJournal.LocalAmount = headerRow.Info.LocalExVATAmount;
			postingJournal.LocalGSTVATAmount = headerRow.Info.LocalVATAmount;
			postingJournal.LocalWHTAmount = headerRow.Info.LocalWHTAmount;
			postingJournal.LocalTotalAmount = headerRow.Info.LocalTotal;
			postingJournal.OSCurrency = headerRow.Info.OSCurrency;
			postingJournal.OSAmount = headerRow.Info.OSExGSTVATAmount;
			postingJournal.OSGSTVATAmount = headerRow.Info.OSGSTVATAmount;
			postingJournal.OSTotalAmount = headerRow.Info.OSTotal;
			postingJournal.OSWHTAmount = headerRow.Info.OSWHTAmount;
			postingJournal.ChargeExchangeRate = null;
			postingJournal.ChargeCurrency = null;
			postingJournal.ChargeTotalAmount = null;
			postingJournal.ChargeTotalExVATAmount = null;
			postingJournal.VATTaxID = null;
			postingJournal.WithholdingTaxID = null;
			postingJournal.GLPostDate = headerRow.Info.PostDate;
			postingJournal.ChargeCode = null;
			postingJournal.GLAccount = headerRow.GLAccount;
			postingJournal.Job = headerRow.Info.Job;
			postingJournal.Branch = headerRow.Info.Branch;
			postingJournal.Department = headerRow.Info.Department;
			if (headerRow.Organization.Key.HasValue && !((ZString)headerRow.Organization.Key.Value).IsEmpty)
			{
				postingJournal.Organization = headerRow.Organization;
			}
			postingJournal.IsFinalCharge = null;
			postingJournal.RevenueRecognitionType = null;
			postingJournal.TransactionCategory = headerRow.Info.Category;
			postingJournal.TaxBranch = headerRow.Info.TaxBranch;
		}

		void PopulatePostingJournalBatchedAtLineLevelOnce(string batchRowType, PostingJournal postingJournal, TransactionLineRow lineRow, TransactionHeaderRow headerRow)
		{
			PopulatePostingJournalGenericDataFromDataRow(batchRowType, postingJournal, lineRow);

			postingJournal.LocalAmount = lineRow.LineAmount;
			postingJournal.LocalGSTVATAmount = lineRow.GSTVAT;
			postingJournal.LocalExtraVATAmount = lineRow.LocalExtraVATAmount;
			postingJournal.LocalTotalAmount = lineRow.LineAmount + lineRow.GSTVAT;
			postingJournal.OSCurrency = lineRow.OSCurrency;
			postingJournal.OSAmount = lineRow.OSExTaxAmount;
			postingJournal.OSGSTVATAmount = lineRow.OSTaxAmount;
			postingJournal.OSExtraVATAmount = lineRow.OSExtraVATAmount;
			postingJournal.OSTotalAmount = lineRow.OSAmount;
			postingJournal.ChargeExchangeRate = lineRow.ChargeExchangeRate;
			postingJournal.ChargeCurrency = lineRow.ChargeCurrency;
			postingJournal.ChargeTotalAmount = lineRow.ChargeAmount + lineRow.ChargeGST;
			postingJournal.ChargeTotalExVATAmount = lineRow.ChargeAmount;
			postingJournal.VATTaxID = lineRow.TaxID;
			postingJournal.TaxMessageID = lineRow.TaxMessageID;
			postingJournal.SupplyType = lineRow.SupplyType;
			postingJournal.TaxBranch = lineRow.TaxBranch;
			postingJournal.CashAdvanceAmount = lineRow.CashAdvanceAmount;
			if (headerRow.Info.OSWHTAmount.HasValue)
			{
				postingJournal.LocalWHTAmount = lineRow.WHTTax;
				postingJournal.OSWHTAmount = lineRow.OSWHTTax;
				postingJournal.WithholdingTaxID = lineRow.WithholdingTaxID;
			}
			postingJournal.JobRecognitionDate = lineRow.JobRecognitionDate;
			postingJournal.TaxDate = lineRow.TaxDate;
			if (lineRow.Organization.Key.HasValue && !((ZString)lineRow.Organization.Key.Value).IsEmpty)
			{
				postingJournal.Organization = lineRow.Organization;
			}
			else if (headerRow.Organization.Key.HasValue && !((ZString)headerRow.Organization.Key.Value).IsEmpty)
			{
				postingJournal.Organization = headerRow.Organization;
			}
			postingJournal.IsFinalCharge = lineRow.IsFinalCharge;
			postingJournal.RevenueRecognitionType = lineRow.RevRecognitionType;
			postingJournal.TransactionCategory = headerRow.Info.Category;

			if (lineRow.GovtChargeCode.HasValue)
			{
				postingJournal.GovernmentReportingChargeCode = lineRow.GovtChargeCode;
			}
			if (lineRow.InputGSTVATRecoverable != 1)
			{
				postingJournal.RecoverableGSTVATPercentage = lineRow.InputGSTVATRecoverable * 100m;
			}

			if (lineRow.SubAccountCollection != null && lineRow.SubAccountCollection.Count > 0)
			{
				postingJournal.SetSubAccountCollection(() => lineRow.SubAccountCollection);
			}

			if (lineRow.PlaceOfSupply != null)
			{
				postingJournal.PlaceOfSupply = lineRow.PlaceOfSupply;
			}
		}

		TransactionType? GetTransactionTypeEnum(ZString? lineType)
		{
			return lineType.HasValue ? new TransactionTypeConverter().ToEnumValue(lineType.Value) : null;
		}

		void PopulatePostingJournalBatchedAtLineLevelTwice(PostingJournal postingJournal, TransactionLineRow lineRow, TransactionHeaderRow headerRow)
		{
			PopulatePostingJournalBatchedAtLineLevelOnce(Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost, postingJournal, lineRow, headerRow);
		}

		void PopulatePostingJournalWipOrAccrual(PostingJournal postingJournal, TransactionLineRow lineRow, BatchRow batchRow)
		{
			PopulatePostingJournalGenericDataFromDataRow(batchRow.RowType, postingJournal, lineRow);

			ZDecimal? lineAmount = Math.Abs(lineRow.LineAmount.Value);
			ZDecimal? osAmount = Math.Abs(lineRow.OSAmount.Value);
			if (WipOrAccrualAmountsShouldBeNegated(batchRow.RowType, lineRow.LineAmount.Value))
			{
				lineAmount *= -1;
				osAmount *= -1;
			}

			postingJournal.LocalAmount = lineAmount;
			postingJournal.LocalGSTVATAmount = null;
			postingJournal.LocalWHTAmount = null;
			postingJournal.LocalExtraVATAmount = null;
			postingJournal.LocalTotalAmount = lineAmount;
			postingJournal.OSCurrency = lineRow.LocalCurrency;
			postingJournal.OSAmount = osAmount;
			postingJournal.OSGSTVATAmount = null;
			postingJournal.OSExtraVATAmount = null;
			postingJournal.OSTotalAmount = osAmount;
			postingJournal.OSWHTAmount = null;
			postingJournal.ChargeExchangeRate = lineRow.ChargeExchangeRate;
			postingJournal.ChargeCurrency = lineRow.ChargeCurrency;
			postingJournal.ChargeTotalAmount = lineRow.ChargeAmount + lineRow.ChargeGST;
			postingJournal.ChargeTotalExVATAmount = lineRow.ChargeAmount;
			postingJournal.ChargeTotalVATAmount = lineRow.ChargeGST;
			postingJournal.VATTaxID = lineRow.ChargeTaxID;
			postingJournal.TaxMessageID = lineRow.ChargeTaxMessageID;
			postingJournal.WithholdingTaxID = lineRow.ChargeWithholdingTaxID;
			if (lineRow.Organization.Key.HasValue && !((ZString)lineRow.Organization.Key.Value).IsEmpty)
			{
				postingJournal.Organization = lineRow.Organization;
			}
			postingJournal.IsFinalCharge = null;
			postingJournal.RevenueRecognitionType = lineRow.RevRecognitionType;
			postingJournal.TransactionCategory = null;
			if (lineRow.GovtChargeCode.HasValue)
			{
				postingJournal.GovernmentReportingChargeCode = lineRow.GovtChargeCode;
			}
			postingJournal.SupplyType = lineRow.SupplyType;
		}

		bool WipOrAccrualAmountsShouldBeNegated(string batchRowType, ZDecimal amount)
		{
			var result = false;
			if (batchRowType == Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost && amount > 0)
			{
				result = true;
			}
			else if (batchRowType == Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServiceReverse && amount < 0)
			{
				result = true;
			}
			return result;
		}

		void PopulatePostingJournalGenericDataFromDataRow(string batchRowType, PostingJournal postingJournal, TransactionLineRow lineRow)
		{
			if (lineRow.BatchSequence.HasValue)
			{
				postingJournal.BatchSequence = lineRow.BatchSequence;
			}

			if (batchRowType == Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost && lineRow.OriginalBatchSequence.HasValue)
			{
				// Should use OriginalBatchSequence as it is an original transaction
				postingJournal.BatchSequence = lineRow.OriginalBatchSequence;
			}
			else if (batchRowType == Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServiceReverse)
			{
				postingJournal.OriginalBatchNumber = lineRow.OriginalBatchNumber;
				postingJournal.OriginalBatchSequence = lineRow.OriginalBatchSequence;
			}

			postingJournal.TransactionType = GetTransactionTypeEnum(lineRow.LineType);
			postingJournal.Sequence = lineRow.Sequence;
			postingJournal.Description = lineRow.Description;
			postingJournal.LocalCurrency = lineRow.LocalCurrency;

			postingJournal.GLPostDate = lineRow.PostDate;
			postingJournal.ChargeCode = lineRow.ChargeCode;
			postingJournal.GLAccount = lineRow.GLAccount;
			postingJournal.Job = lineRow.Job;
			postingJournal.CostSource = lineRow.CostSource;
			postingJournal.Branch = lineRow.Branch;
			postingJournal.Department = lineRow.Department;
			postingJournal.SetRatingBasisCollection(() => lineRow.RatingBasisCollection);
		}

		bool IsBatchedAtLineLevelTwice(TransactionLineRow lineRow, TransactionHeaderRow headerRow)
		{
			/*
			1.	AH_Ledger = ‘AR’ and AL_LineType = ‘REV’
			a.	AH_TransactionType = ‘INV’
			b.	AH_TransactionType = ‘CRD’
			c.	AH_TransactionType = ‘ADJ’
			2.	AH_Ledger = ‘AP’ and AL_LineType = ‘CST’
			a.	AH_TransactionType = ‘INV’
			b.	AH_TransactionType = ‘CRD’
			c.	AH_TransactionType = ‘ADJ’
			3.  AH_Ledger = 'JC' and AL_LineType = 'REV'
			a.  AH_TransactionType = 'JNL'
			b.  AH_TransactionType = 'JRJ'
			 */
			bool result = false;
			if (lineRow.TransactionHeader != Guid.Empty)
			{
				if (headerRow != null)
				{
					if (headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Invoice || headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.CreditNote || headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.AdjustmentNote)
					{
						if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsReceivable && lineRow.LineType.Value == TransactionLineTypes.Revenue)
						{
							result = true;
						}
						else if (headerRow.Info.Ledger.Value == LedgerTypes.AccountsPayable && lineRow.LineType.Value == TransactionLineTypes.Cost)
						{
							result = true;
						}
					}
					else if (headerRow.Info.Ledger.Value == LedgerTypes.JobCosting && (lineRow.LineType.Value == TransactionLineTypes.Revenue || lineRow.LineType.Value == TransactionLineTypes.Cost)
						&& (headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.Journal || headerRow.Info.TransactionType.Value.ToString() == TransactionTypes.JobRevenueJournal))
					{
						result = true;
					}
				}
			}
			return result;
		}

		bool IsBatchedAtLineLevelOnce(TransactionLineRow row, TransactionHeaderRow header)
		{
			/*
			a.	AH_Ledger = ‘CB’ and AH_TransactionType = ‘DPY’ and AL_LineType = ‘DPY’
			b.	AH_Ledger = ‘CB’ and AH_TransactionType = ‘DRC’ and AL_LineType = ‘DRC’
			c.	AH_Ledger = ‘GL’
			i.	AH_TransactionType = ‘GJL’
			ii.	AH_TransactionType = ‘RJL’
			iii.	AH_TransactionType = ‘AJL’
			iiii.	AH_TransactionType = ‘NJL’
			*/
			bool result = false;
			if (row.TransactionHeader != Guid.Empty)
			{
				if (header != null)
				{
					if ((string)header.Info.Ledger == LedgerTypes.CashBook)
					{
						if (header.Info.TransactionType.Value.ToString() == TransactionTypes.DirectPayment && (string)row.LineType == TransactionTypes.DirectPayment)
						{
							result = true;
						}
						else if (header.Info.TransactionType.Value.ToString() == TransactionTypes.DirectReceipt && (string)row.LineType == TransactionTypes.DirectReceipt)
						{
							result = true;
						}
					}
					else if ((string)header.Info.Ledger == LedgerTypes.General && (header.Info.TransactionType.Value.ToString() == TransactionTypes.GLStandardJournal ||
							header.Info.TransactionType.Value.ToString() == TransactionTypes.GLReversingJournal || header.Info.TransactionType.Value.ToString() == TransactionTypes.GLAutoJournal ||
							header.Info.TransactionType.Value.ToString() == TransactionTypes.GLNoteJournal))
					{
						result = true;
					}
				}
			}
			return result;
		}

		bool IsWipOrAccrual(TransactionLineRow row)
		{
			return (string)row.LineType == TransactionLineTypes.WIP || (string)row.LineType == TransactionLineTypes.Accrual;
		}

		bool IsHeaderExport(TransactionHeaderRow row)
		{
			/*
			a.	AH_Ledger = ‘AR’
			i.	AH_TransactionType = ‘JNL’
			ii.	AH_TransactionType = ‘TRF’
			iii.	AH_TransactionType = ‘CTR’
			iv.	AH_TransactionType = ‘PAY’
			v.	AH_TransactionType = ‘REC’
			vi.	AH_TransactionType = ‘EXX’
			vii.	AH_TransactionType = ‘OVP’
			viii.	AH_TransactionType = ‘DSC’

			b.	AH_Ledger = ‘AP’
			i.	AH_TransactionType = ‘JNL’
			ii.	AH_TransactionType = ‘TRF’
			iii.	AH_TransactionType = ‘CTR’
			iv.	AH_TransactionType = ‘PAY’
			v.	AH_TransactionType = ‘REC’
			vi.	AH_TransactionType = ‘EXX’
			vii.	AH_TransactionType = ‘OVP’
			viii.	AH_TransactionType = ‘DSC’

			c.	AH_Ledger = ‘CB’
			i.	AH_TransactionType = ‘EXX’
			ii.	AH_TransactionType = ‘TRF’
			iii.	AH_TransactionType = ‘OPY’
			iv.	AH_TransactionType = ‘ORC’
			 */
			bool result = false;
			if ((string)row.Info.Ledger == LedgerTypes.AccountsReceivable || (string)row.Info.Ledger == LedgerTypes.AccountsPayable)
			{
				if (row.Info.TransactionType.Value.ToString() == TransactionTypes.Journal || row.Info.TransactionType.Value.ToString() == TransactionTypes.Transfer ||
					row.Info.TransactionType.Value.ToString() == TransactionTypes.Contra || row.Info.TransactionType.Value.ToString() == TransactionTypes.Payment ||
					row.Info.TransactionType.Value.ToString() == TransactionTypes.Receipt || row.Info.TransactionType.Value.ToString() == TransactionTypes.ExchangeDifference ||
					row.Info.TransactionType.Value.ToString() == TransactionTypes.Overpayment || row.Info.TransactionType.Value.ToString() == TransactionTypes.Discount)
				{
					result = true;
				}
			}
			else if ((string)row.Info.Ledger == LedgerTypes.CashBook &&
				(row.Info.TransactionType.Value.ToString() == TransactionTypes.ExchangeDifference || row.Info.TransactionType.Value.ToString() == TransactionTypes.Transfer ||
				row.Info.TransactionType.Value.ToString() == TransactionTypes.OpeningPayment || row.Info.TransactionType.Value.ToString() == TransactionTypes.OpeningReceipt))
			{
				result = true;
			}
			return result;
		}
	}
}

