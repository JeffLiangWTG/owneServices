using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	/// <summary>
	/// Calculates and updates invoice terms and due date for IInvoiceTerms objects. Supports manual invoice term changing. 
	/// </summary>
	public abstract class TermsAndDueDateCalculationProvider
	{
		public TermsAndDueDateCalculationProvider(IInvoiceTerms invoice)
		{
			Factory = invoice.Factory;
			this.Invoice = invoice;
		}

		public void CalculateDueDate()
		{
			if (!CalculateDueDateSuspender.IsSuspended
				&& !(Invoice.AH_InvoiceDateInfo.HasErrors() && Invoice.AH_InvoiceTermInfo.HasErrors() && Invoice.AH_InvoiceTermDaysInfo.HasErrors()))
			{
				Invoice.AH_DueDate = DueDateCalculation.GetDueDate(Invoice.Factory, GetInvoiceTerm(true), GetInvoiceDate(), Invoice.AH_InvoiceTerm, Invoice.AH_InvoiceTermDays, Invoice.Job);
			}
		}

		public void SetInvoiceTermsAndDays()
		{
			if (Invoice.Header != null)
			{
				using (CalculateDueDateSuspender.GetSuspender())
				{
					InvoiceTerm invoiceTerm = GetInvoiceTerm(false);
					Invoice.AH_InvoiceTerm = invoiceTerm.Term;
					Invoice.AH_InvoiceTermDays = invoiceTerm.Days;
				}
			}
		}

		FunctionalitySuspender CalculateDueDateSuspender
		{
			get { return calculateDueDateSuspender ?? (calculateDueDateSuspender = new FunctionalitySuspender(() => CalculateDueDate() )); }
		}
		FunctionalitySuspender calculateDueDateSuspender;

		public string CanInvoiceTermBeSelectedError
		{
			get
			{
				string errorMessage = string.Empty;
				if (Invoice.AH_InvoiceTerm == Constants.InvoiceTerms.MonthsFromInvoiceCycleDate && GetInvoiceTerm(true).IsEmpty)
				{
					errorMessage = MonthsFromInvoiceCycleDateError;
				}

				return errorMessage;
			}
		}

		public static string MonthsFromInvoiceCycleDateError
		{
			get { return Res.GetString("CA03A748-B69F-4B31-8EBD-089C4DA1C100", "Invoice cycle data is not set up for this invoice term. Please go to Organization screen > AR Tab to set it up."); }
		}

		protected abstract InvoiceTerm GetInvoiceTerm(bool useFallbackByInvoiceTerm);

		protected virtual ZDateTime GetInvoiceDate()
		{
			return Invoice.AH_InvoiceDate;
		}

		#region Implementation

		protected IInvoiceTerms Invoice;
		protected BusinessObjectFactory Factory;

		#endregion
	}
}
