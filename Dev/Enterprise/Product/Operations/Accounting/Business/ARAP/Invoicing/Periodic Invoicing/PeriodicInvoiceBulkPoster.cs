using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PeriodicInvoiceBulkPoster
	{
		public PeriodicInvoiceBulkPoster()
		{
			IsCancelled = false;
		}

		public void AddInvoiceInfo(InvoiceInfo invoiceInfo)
		{
			invoiceInfos.Add(invoiceInfo);
		}

		readonly List<InvoiceInfo> invoiceInfos = new List<InvoiceInfo>();

		public void Post()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				LastStackTraceInPost_ForTestOnly = new StackTrace();
			}
#endif
			List<ZGuid> postedInvoicePKs = new List<ZGuid>();
			string errorMessage = null;
			bool isPostingDone = false;
			string lastInvoiceID = string.Empty;
			Exception raisedException = null;
			int numberOfInvoicesProcessed = 0;
			try
			{
				RaisePostingStarted(invoiceInfos.Count);
				foreach (var invoiceInfo in invoiceInfos)
				{
					if (IsCancelled)
					{
						break;
					}

					var invoice = new PeriodicInvoiceLightForBulkPosting(new BusinessObjectFactory());
					numberOfInvoicesProcessed++;
					try
					{
						invoice.Factory.RefreshEnabled = false;     //removing this can cause performance issue
						var errors = invoice.InitalizeAndValidate(invoiceInfo);
						bool isFailed = errors.Length > 0;
						lastInvoiceID = invoice.InvoiceID;
						((IBusiness)invoice).ValidateIfQuickAndImprovesPreSaveValidationPerformance(); //to avoid children validation
						string message = string.Empty;
						if (isFailed)
						{
							string delimeter = System.Environment.NewLine + "\t";
							message = delimeter + new ZStringBuilder(errors).ToStringWithDelimiterBetweenAppends(delimeter);
						}
						else
						{
							invoice.CreateTransactions();
							RaiseNegativeComplianceFailedToCreate(invoice.PostManager.Poster.PostedInvoices);
							var stringBuilder = new ZStringBuilder();
							foreach (var transaction in invoice.ARTransactionsCreatedForPosting)
							{
								if (transaction.HasRowErrors)
								{
									isFailed = true;
									stringBuilder.Append(Res.GetString("D3430DEC-E0A8-450C-A8C6-D305BF0AB2D8", "Transaction was not posted."));
									foreach (var rowError in transaction.RowErrors)
									{
										stringBuilder.Append($"{transaction.Header.OH_Code}: {rowError.Message}");
									}
								}
							}

							if (!isFailed)
							{
								invoice.Factory.Save();

								foreach (var transaction in invoice.ARTransactionsCreatedForPosting)
								{
									postedInvoicePKs.Add(transaction.PK);
									stringBuilder.Append(Res.GetString("78b81793-e159-4340-bb5d-b5047f292fde", "{0} {1} {2} was posted.",
										transaction.AH_Ledger, transaction.AH_TransactionType, transaction.AH_TransactionNum));
									foreach (var rowWarning in transaction.RowWarnings)
									{
										stringBuilder.Append(Res.GetString("D76F5757-2034-48E3-8327-1C01D97C8D41", "{0}: Warning! {1}",
											transaction.Header.OH_Code, rowWarning.Message));
									}
								}
								if (stringBuilder.Length == 0)
								{
									stringBuilder.Append(Res.GetString("def74ee0-3517-4231-a9fb-a01a3a1f590e", "No AR transaction was posted."));
									isFailed = true;
								}
								if (!invoiceInfo.ExcludedJobs.IsNullOrEmpty())
								{
									stringBuilder.Append(Res.GetString("17CC187E-0DE4-45AA-AD54-2ABF5320095B", "Warning! The following charges were excluded:") + string.Join(string.Empty, invoiceInfo.ExcludedJobs));
								}
							}
							message = stringBuilder.ToStringWithNewLineBetweenAppends();
						}
						RaisePostingProgress(numberOfInvoicesProcessed, GetFullMessage(lastInvoiceID, message), isFailed, null);
					}
					catch (ComplianceSequenceRelatedException ex)
					{
						RaisePostingProgress(numberOfInvoicesProcessed, ex.UserFriendlyMessage, true, null);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						RaisePostingProgress(numberOfInvoicesProcessed, GetFullMessage(lastInvoiceID, ""), true, ex);
					}

					GCWrapper.ReclaimMemory(ref invoice);
				}
				isPostingDone = !IsCancelled;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				raisedException = ex;
				errorMessage = GetFullMessage(lastInvoiceID, "");
			}
			finally
			{
				RaisePostingFinished(!isPostingDone, numberOfInvoicesProcessed, postedInvoicePKs.ToArray(), errorMessage, raisedException);
			}
		}

		string GetFullMessage(string invoiceID, string message)
		{
			return Res.GetString("03A423E1-13BE-41B9-8286-E58094EFFEAB", "Invoice {0}: {1}", invoiceID, message);
		}

		public event Action<int> PostingStarted;
		public event Action<int, string, bool, Exception> PostingProgress;
		public event Action<bool, int, ZGuid[], string, Exception> PostingFinished;
		public event EventHandler NegativeComplianceFailedToCreate;

		void RaiseNegativeComplianceFailedToCreate(InvoicingBaseCollection transactions)
		{
			NegativeComplianceFailedToCreate?.Invoke(transactions, null);
		}

		void RaisePostingStarted(int numberOfInvoicesToPost)
		{
			PostingStarted?.Invoke(numberOfInvoicesToPost);
		}

		void RaisePostingProgress(int numberOfInvoicesProcessed, string message, bool isFailed, Exception exception)
		{
			PostingProgress?.Invoke(numberOfInvoicesProcessed, message, isFailed, exception);
		}

		void RaisePostingFinished(bool isCancelled, int numberOfInvoicesProcessed, ZGuid[] postedInvoicePKs, string errorMessage, Exception exception)
		{
			PostingFinished?.Invoke(isCancelled, numberOfInvoicesProcessed, postedInvoicePKs, errorMessage, exception);
		}

		public void CancelPosting()
		{
			IsCancelled = true;
		}

		bool IsCancelled
		{
			get;
			set;
		}

		public class InvoiceInfo
		{
			public InvoiceInfo() { }

			public InvoiceInfo(PeriodicInvoice invoice)
			{
				DebtorPK = invoice.DebtorPK;
				InvoiceType = invoice.InvoiceType;
				CurrencyNK = invoice.CurrencyNK;
				InvoiceDate = invoice.InvoiceDate;
				PostDate = invoice.PostDate;
				SelectedJobs = invoice.SelectedJobs.Select(x => x.PK).ToArray();
				Charges = invoice.Charges.Select(x => x.PK).ToArray();
				MiscInvoices = invoice.SelectedMiscInvoices.Select(x => x.PK).ToArray();
				LocalExTaxAmount = invoice.LocalExTaxAmount;
				LocalTaxAmount = invoice.LocalTaxAmount;
				ExcludedJobs = invoice.Jobs.Cast<PeriodicInvoiceSelectableJob>()
					.Where(t => !t.IncludeInThePeriodicInvoice)
					.Select(t => ExcludedJobInfo(t))
					.ToArray();
				SellReference = invoice.SellReference;
				TaxBranch = invoice.TaxBranch;
				InvoiceTerm = invoice.InvoiceTerm;
				InvoiceTermDays = invoice.InvoiceTermDays;
				DueDate = invoice.DueDate;
			}
			string ExcludedJobInfo(PeriodicInvoiceSelectableJob job)
			{
				return FormattableString.Invariant($"{System.Environment.NewLine}\t - {job.JobType} {job.JH_JobNum} {job.JH_Status}");
			}

			public ZGuid DebtorPK;
			public ZString InvoiceType;
			public ZString CurrencyNK;
			public ZDateTime InvoiceDate;
			public ZDateTime PostDate;
			public ZGuid[] SelectedJobs;
			public ZGuid[] Charges;
			public ZGuid[] MiscInvoices;
			public ZDecimal LocalExTaxAmount;
			public ZDecimal LocalTaxAmount;
			public string[] ExcludedJobs;
			public ZString SellReference;
			public ZGuid TaxBranch;
			public ZString InvoiceTerm;
			public ZByte InvoiceTermDays;
			public ZDateTime DueDate;
		}

#if DEBUG
		public StackTrace LastStackTraceInPost_ForTestOnly;
#endif
	}
}
