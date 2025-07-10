#if DEBUG

using System;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class PeriodicInvoiceBulkPosterController
	{
		public ProgressWithDetailesForm ProgressForm_ForTestOnly => ProgressForm;

		public void ResetProgressForm_ForTestOnly()
		{
			ResetProgressForm();
		}

		public void Poster_PostingStarted_ForTestOnly(int numberOfInvoicesToPost)
		{
			Poster_PostingStarted(numberOfInvoicesToPost);
		}

		public void Poster_PostingFinished_ForTestOnly(bool isCancelled, int numberOfInvoicesProcessed, ZGuid[] postedInvoicePKs, string errorMessage, Exception exception)
		{
			Poster_PostingFinished(isCancelled, numberOfInvoicesProcessed, postedInvoicePKs, errorMessage, exception);
		}

		public PrintTask EnterpriseInvoicesPrintTaskForTesting_ForTestOnly
		{
			get { return enterpriseInvoicesPrintTaskForTesting; }
			set { enterpriseInvoicesPrintTaskForTesting = value; }
		}

		public PrintTask GovernmentInvoicesPrintTaskForTesting_ForTestOnly
		{
			get { return governmentInvoicesPrintTaskForTesting; }
			set { governmentInvoicesPrintTaskForTesting = value; }
		}

		public void HookEvents_ForTestOnly()
		{
			HookEvents();
		}

		public void UnHookEvents_ForTestOnly()
		{
			UnHookEvents();
		}
	}
}

#endif
