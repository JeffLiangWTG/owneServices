using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class ARReversalDateHelper
	{
		public static void DefaultReversalDates(InvoicingBase reversedInvoice)
		{
			bool isConsumerConsol;
			IJobInvoicingPlugIn consumer = GetConsumer(reversedInvoice, out isConsumerConsol);

			if (consumer != null)
			{
				DefaultReversalDatesJobRelated(reversedInvoice, consumer, isConsumerConsol);
			}
			else
			{
				DefaultReversalDatesIfNonJobRelatedARTransaction(reversedInvoice);
			}
		}

		static IJobInvoicingPlugIn GetConsumer(InvoicingBase reversedInvoice, out bool isConsumerConsol)
		{
			IJobInvoicingPlugIn consumer = null;

			isConsumerConsol = false;

			if (reversedInvoice != null && reversedInvoice.IsARInvoiceOrCreditNote
				&& !reversedInvoice.IsPeriodicInvoice)
			{
				if (reversedInvoice.AH_JH.IsValid)
				{
					Job job = reversedInvoice.Factory.Load<Job>(reversedInvoice.AH_JH);

					if (job != null)
					{
						job.InitializeParentFromGenericJobWithoutSettingDefaults();
						consumer = job.PlugInData;
					}
				}
				else if (reversedInvoice.IsConsolInvoice)
				{
					consumer = reversedInvoice.SingleConsolConsumer;
					isConsumerConsol = true;
				}
			}

			return consumer;
		}

#if DEBUG
		public static IJobInvoicingPlugIn GetConsumerForTest(InvoicingBase reversedInvoice, out bool isConsumerConsol)
		{
			return GetConsumer(reversedInvoice, out isConsumerConsol);
		}
#endif

		public static bool? DefaultReversalDatesJobRelated(TransactionHeaderCollection reversedInvoices, IJobInvoicingPlugIn consumer, Func<TransactionHeaderCollection, ChangeTransactionDatesBusinessObject, bool?> showReversalDatesFormDelegate, bool isConsumerConsol)
		{
			SecurityCheckpoint pluginSecurity = consumer.InvoicingSupporter.JobInvoicingSecurity;
			OperationsJobConfigurationCodes codes = new OperationsJobConfigurationCodes(consumer);

			var invoiceDateConfigurationHelper = new InvoiceDateConfigurationHelper(codes, consumer, true);
			InvoiceDateConfiguration invoiceDateConfiguration = invoiceDateConfigurationHelper.FindInvoiceDateConfiguration();

			ChangeTransactionDatesBusinessObject changeTransactionDatesBusinessObject;
			if (isConsumerConsol)
			{
				changeTransactionDatesBusinessObject = new ChangeTransactionDatesForConsolBusinessObject(pluginSecurity, reversedInvoices.Factory, codes);
			}
			else
			{
				changeTransactionDatesBusinessObject = new ChangeTransactionDatesBusinessObject(pluginSecurity, reversedInvoices.Factory, codes);
			}

			bool canBackPostForReversal = invoiceDateConfigurationHelper.CanBackPost || (invoiceDateConfiguration != null && invoiceDateConfiguration.ReversalRule != InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules);

			bool canPerformBackDating = canBackPostForReversal ||
										!changeTransactionDatesBusinessObject.InvoiceDateInfo.ReadOnly ||
										!changeTransactionDatesBusinessObject.PostDateInfo.ReadOnly;

			bool canDefaultPostDateFromInvoiceDate = ((invoiceDateConfiguration != null && invoiceDateConfiguration.Today)
										|| AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value.OverridePostDate) &&
										AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value.DefaultPostDateFromInvoiceDate;

			if (canPerformBackDating)
			{
				foreach (InvoicingBase invoice in reversedInvoices)
				{
					if (canBackPostForReversal)
					{
						invoice.AH_InvoiceDate = invoiceDateConfigurationHelper.GetInvoiceDate(invoice.OriginalTransaction.AH_InvoiceDate);

						if (canDefaultPostDateFromInvoiceDate)
						{
							ZDateTime postDate = invoice.AH_InvoiceDate;

							if (postDate < invoice.OriginalTransaction.AH_PostDate)
							{
								postDate = invoice.OriginalTransaction.AH_PostDate;
							}

							AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(reversedInvoices.Factory);
							ZInt period = periodCalculator.GetPeriodFromDate(postDate);

							if (period == 0 || periodCalculator.IsPeriodSubLedgerClosed(period))
							{
								postDate = ZDateTime.Today;
							}

							invoice.AH_PostDate = postDate;
						}
					}
				}

				if (showReversalDatesFormDelegate != null)
				{
					return showReversalDatesFormDelegate(reversedInvoices, changeTransactionDatesBusinessObject);
				}
				else
				{
					return true;
				}
			}

			return false;
		}

		static bool? DefaultReversalDatesJobRelated(BusinessObject reversedInvoice, IJobInvoicingPlugIn consumer, bool isConsumerConsol)
		{
			TransactionHeaderCollection collection = new TransactionHeaderCollection(reversedInvoice.Factory);
			collection.Add(reversedInvoice);
			return DefaultReversalDatesJobRelated(collection, consumer, null, isConsumerConsol);
		}

#if DEBUG
		public static bool? DefaultReversalDatesJobRelatedForTest(BusinessObject reversedInvoice, IJobInvoicingPlugIn consumer, bool isConsumerConsol)
		{
			return DefaultReversalDatesJobRelated(reversedInvoice, consumer, isConsumerConsol);
		}

#endif

		static ZDateTime GetDefaultDate(InvoicingBase reversedInvoice, string registryValue)
		{
			ZDateTime date = ZDateTime.Today;

			if (registryValue == AccountingConstants.ReversalDefaultFromOriginalTransactionDate.TodaysDate)
			{
				date = ZDateTime.Today;	//explicit to show business rule
			}
			else if (registryValue == AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate)
			{
				AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(reversedInvoice.Factory);
				ZInt period = periodCalculator.GetPeriodFromDate(reversedInvoice.OriginalTransaction.AH_InvoiceDate);

				if (period == 0 || periodCalculator.IsPeriodSubLedgerClosed(period))
				{
					date = ZDateTime.Today;	//explicit to show business rule
				}
				else
				{
					date = reversedInvoice.OriginalTransaction.AH_InvoiceDate;
				}
			}
			else if (registryValue == AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod)
			{
				AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(reversedInvoice.Factory);
				ZInt period = periodCalculator.GetPeriodFromDate(reversedInvoice.OriginalTransaction.AH_InvoiceDate);

				if (period == 0 || periodCalculator.IsPeriodSubLedgerClosed(period))
				{
					AccPeriodManagement periodManagement = periodCalculator.GetFirstOpenPeriod(GlbCompany.CurrentCompany.PK);
					if (periodManagement != null)
					{
						date = periodManagement.AM_StartDate;
					}
					else
					{
						date = ZDateTime.Today;
					}
				}
				else
				{
					date = reversedInvoice.OriginalTransaction.AH_InvoiceDate;
				}
			}
			else if (registryValue == AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionPostDateOrCurrentDate)
			{
				AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(reversedInvoice.Factory);
				ZInt period = periodCalculator.GetPeriodFromDate(reversedInvoice.OriginalTransaction.AH_PostDate);

				if (period == 0 || periodCalculator.IsPeriodSubLedgerClosed(period))
				{
					date = ZDateTime.Today;	//explicit to show business rule
				}
				else
				{
					date = reversedInvoice.OriginalTransaction.AH_PostDate;
				}
			}
			else if (registryValue == AccountingConstants.ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod)
			{
				AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(reversedInvoice.Factory);
				ZInt period = periodCalculator.GetPeriodFromDate(reversedInvoice.OriginalTransaction.AH_PostDate);

				if (period == 0 || periodCalculator.IsPeriodSubLedgerClosed(period))
				{
					AccPeriodManagement periodManagement = periodCalculator.GetFirstOpenPeriod(GlbCompany.CurrentCompany.PK);
					if (periodManagement != null)
					{
						date = periodManagement.AM_StartDate;
					}
					else
					{
						date = ZDateTime.Today;
					}
				}
				else
				{
					date = reversedInvoice.OriginalTransaction.AH_PostDate;
				}
			}

			return date;
		}

		static void DefaultReversalDatesIfNonJobRelatedARTransaction(InvoicingBase reversedInvoice)
		{
			if (reversedInvoice != null && reversedInvoice.IsARInvoiceOrCreditNote
				&& ((!reversedInvoice.AH_JH.IsValid && !reversedInvoice.IsConsolInvoice)
					|| reversedInvoice.IsPeriodicInvoice)
				&& reversedInvoice.OriginalTransaction != null)
			{
				reversedInvoice.AH_InvoiceDate = GetDefaultDate(reversedInvoice, AccountingConfigurationRegistry.Instance.AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Value);

				reversedInvoice.AH_PostDate = GetDefaultDate(reversedInvoice, AccountingConfigurationRegistry.Instance.AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate.Value);

				if (reversedInvoice.AH_PostDate < reversedInvoice.OriginalTransaction.AH_PostDate)
				{
					reversedInvoice.AH_PostDate = reversedInvoice.OriginalTransaction.AH_PostDate;
				}
			}
		}

#if DEBUG
		public static void DefaultReversalDatesIfNonJobRelatedARTransactionForTest(InvoicingBase reversedInvoice)
		{
			DefaultReversalDatesIfNonJobRelatedARTransaction(reversedInvoice);
		}
#endif

	}
}