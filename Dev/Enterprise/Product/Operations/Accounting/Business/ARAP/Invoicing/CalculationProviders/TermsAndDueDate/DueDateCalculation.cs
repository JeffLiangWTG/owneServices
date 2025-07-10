using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class DueDateCalculation
	{
		public static ZDateTime GetDueDate(BusinessObjectFactory factory, InvoiceTerm invoiceTerm, ZDateTime invoiceDate, string overridenTerm = "", int overridenTermDays = -1, JobHeader jobHeader = null)
		{
			ZDateTime result = ZDateTime.Now;

			string term = invoiceTerm.Term;
			int termDays = invoiceTerm.Days;
			if (!string.IsNullOrEmpty(overridenTerm))
			{
				term = overridenTerm;
			}
			if (overridenTermDays != -1)
			{
				termDays = overridenTermDays;
			}

			if (invoiceDate.IsValid)
			{
				switch (term)
				{
					case Constants.InvoiceTerms.FromInvoiceDate:
						result = invoiceDate.AddDays(termDays);
						break;
					case Constants.InvoiceTerms.FromMonthEnd:
						result = new ZDateTime(invoiceDate.AddMonths(1).Year, invoiceDate.AddMonths(1).Month, 1).AddDays(termDays - 1);
						break;
					case Constants.InvoiceTerms.FromWeekEnd:
						var daysUntilEWK = (new DayOfWeekCodeList().GetDayOfWeek(OrganisationRegistry.Instance.InvoiceTermsEndOfWeek.Value) - invoiceDate.DayOfWeek + 7) % 7;
						result = invoiceDate.AddDays(daysUntilEWK).AddDays(termDays);
						break;
					case Constants.InvoiceTerms.FromPeriodEnd:
						result = invoiceDate;
						if (invoiceDate.IsValid)
						{
							ZDateTime periodLastDay = new AccountingPeriodCalculator(factory).GetLastDayForPeriod(invoiceDate);
							result = periodLastDay.IsValid && !periodLastDay.IsEmpty ? periodLastDay.AddDays(termDays) : invoiceDate;
						}
						break;
					case Constants.InvoiceTerms.CashOnDelivery:
					case Constants.InvoiceTerms.FromCustomsClearanceDate:
					case Constants.InvoiceTerms.FromShipmentDate:
					case Constants.InvoiceTerms.LaterOfShipmentOrInvoiceDate:
					case Constants.InvoiceTerms.PaymentInAdvance:
						result = invoiceDate;
						break;
					case Constants.InvoiceTerms.MonthsFromInvoiceCycleDate:
						ZDateTime arTermsCycleDueDate = invoiceTerm.GetARTermsCycleDueDate(invoiceDate, termDays);
						if (!arTermsCycleDueDate.IsEmpty)
						{
							result = arTermsCycleDueDate;
						}
						break;
					case Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle:
						ZDateTime arPaymentCycleDueDate = invoiceTerm.GetARPaymentCycleDueDate(invoiceDate, termDays);
						if (!arPaymentCycleDueDate.IsEmpty)
						{
							result = arPaymentCycleDueDate;
						}
						break;
					case Constants.InvoiceTerms.FromDeliveryOrPickupDate:
						var job = jobHeader != null ? factory.Load<Job>(jobHeader.PK) : null;
						if (job != null && job.PlugInData != null && !job.IsPluginDataDeleted)
						{
							var supporter = job.PlugInData.InvoicingSupporter;
							result = supporter.GetOperationsSignificantDateByDirection(supporter.IsExport ? InvoiceDateConfigurationLookups.SignificantDateCodes.PickupDate : InvoiceDateConfigurationLookups.SignificantDateCodes.DeliveryDate, job.Direction);
							if (result.IsValid)
							{
								result = result.AddDays(termDays);
								result = result < invoiceDate ? invoiceDate : result;
							}
							else
							{
								result = invoiceDate.AddDays(termDays);
							}
						}
						break;
				}
			}

			return result;
		}

		public static ZDateTime GetCalculateDate(InvoiceTerm invoiceTerm, ZDateTime invoiceDate, ZDateTime documentReceivedDate)
		{
			var rule = AccountingMasterFilesRegistry.Instance.APInvoiceDueDateCalculationRule.Value;
			if (rule)
			{
				if (documentReceivedDate.IsValid && invoiceTerm.Term != InvoiceTermsList.FromCustomsClearanceDate.Code && invoiceTerm.Term != InvoiceTermsList.FromShipmentDate.Code && invoiceTerm.Term != InvoiceTermsList.LaterOfShipmentOrInvoiceDate.Code)
				{
					return documentReceivedDate;
				}
				else
				{
					return invoiceDate;
				}
			}
			else
			{
				return invoiceDate;
			}
		}
	}
}
