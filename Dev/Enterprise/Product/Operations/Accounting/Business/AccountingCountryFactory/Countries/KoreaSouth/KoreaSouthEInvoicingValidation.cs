using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class KoreaSouthEInvoicingValidation : IReverseDateValidation, IInvoiceDateValidation, ITransactionLinesValidation
	{
		#region ITransactionLinesValidation

		public ResourceString ValidateInvoiceLines(IEnumerable<AccTransactionLines> accTransactionLines)
		{
			TransactionHeader transactionHeader = null;
			if (!AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value
				|| accTransactionLines.Count() < 100
				|| !accTransactionLines.AllSame(x => x.TransactionHeader)
				|| (transactionHeader = GetTransactionHeader(accTransactionLines.First())) == null
				|| transactionHeader.IsInDatabase
				|| !ElectronicInvoicingEligibilityDecider.IsEligible(transactionHeader))
			{
				return null;
			}

			return ExceedMaximumLineNumberMessage;
		}

		string[] ITransactionLinesValidation.GetErrorMessages()
		{
			return new string[] { ExceedMaximumLineNumberMessage };
		}

		ResourceString ExceedMaximumLineNumberMessage => ResString.GetMultilingualString("B385EB8E-EF7A-463D-9A3F-65D81027B6E1", "The Korea National Tax Service only accepts up to 99 transaction lines per invoice. Please split the charges into multiple invoices.");

		#endregion

		public ResourceString ValidateReverseDate(AccTransactionLines line)
		{
			TransactionHeader transactionHeader = null;
			if (line == null
				|| !AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value
				|| (transactionHeader = GetTransactionHeader(line)) == null
				|| (line.IsInDatabase && !line.AL_ReverseDateInfo.HasChanges && !transactionHeader.AH_InvoiceDateInfo.HasChanges)
				|| !ElectronicInvoicingEligibilityDecider.IsEligible(transactionHeader)
				|| IsAmendingTransactionWithOriginalReference(transactionHeader))
			{
				return null;
			}

			if (!AccountingUtils.AreDatesInTheSameCalendarMonth(transactionHeader.AH_InvoiceDate, line.AL_ReverseDate))
			{
				return ResString.GetMultilingualString("B69EE270-A29C-4C01-9B5D-C6FC09525371", "The invoice date and revenue recognition date must be in the same calendar month. Current Invoice Date: {0}. Current Revenue Recognition Date: {1}", transactionHeader.AH_InvoiceDate.ToShortDateString(), line.AL_ReverseDate.ToShortDateString());
			}

			return null;
		}

		public ResourceString ValidateInvoiceDate(AccTransactionHeader accTransactionHeader)
		{
			TransactionHeader transactionHeader = null;
			if (accTransactionHeader == null
				|| (accTransactionHeader.IsInDatabase && !accTransactionHeader.AH_InvoiceDateInfo.HasChanges)
				|| !AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value
				|| (transactionHeader = GetTransactionHeader(accTransactionHeader)) == null
				|| !ElectronicInvoicingEligibilityDecider.IsEligible(transactionHeader))
			{
				return null;
			}

			return ValidateInvoiceDate(transactionHeader.AH_InvoiceDate);
		}

		bool IsAmendingTransactionWithOriginalReference(AccTransactionHeader accTransactionHeader)
		{
			return accTransactionHeader is IAmending amending && amending.OriginalTransaction != null;
		}

		public ResourceString ValidateInvoiceDate(ZDateTime invoiceDate)
		{
			if (!invoiceDate.IsValid
				|| !AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value
				|| AccountingConfigurationRegistry.Instance.BackDateInvoiceDateDeadline.Value == 0)
			{
				return null;
			}

			var dateTimeToday = ZDateTime.Today;
			var firstDayInLastMonth = new ZDateTime(dateTimeToday.Year, dateTimeToday.Month, 1).AddMonths(-1);

			var deadLineInCurrentMonth = new ZDateTime(dateTimeToday.Year, dateTimeToday.Month, AccountingConfigurationRegistry.Instance.BackDateInvoiceDateDeadline.Value);
			if (deadLineInCurrentMonth.DayOfWeek == DayOfWeek.Saturday)
			{
				deadLineInCurrentMonth = deadLineInCurrentMonth.AddDays(2);
			}
			else if (deadLineInCurrentMonth.DayOfWeek == DayOfWeek.Sunday)
			{
				deadLineInCurrentMonth = deadLineInCurrentMonth.AddDays(1);
			}

			if (invoiceDate < dateTimeToday && !AccountingUtils.AreDatesInTheSameCalendarMonth(invoiceDate, dateTimeToday))
			{
				if (invoiceDate < firstDayInLastMonth)
				{
					return ResString.GetMultilingualString("BC6EF0CC-85A9-4AA4-8407-CDD7E5199CC5", "The invoice date can only be back dated to the previous month only, not earlier than previous month.");
				}

				if (dateTimeToday > deadLineInCurrentMonth)
				{
					return ResString.GetMultilingualString("5C875E43-7932-4587-BC20-23873183B7D2", @"The invoice date can not be back dated to the previous month after day {0} of the current month. 
Should day {0} falls on a Saturday or Sunday, back dating to previous month is allowed on the immediate Monday only.
However, back dating to current month is allowed.", AccountingConfigurationRegistry.Instance.BackDateInvoiceDateDeadline.Value);
				}
			}

			return null;
		}

		TransactionHeader GetTransactionHeader(AccTransactionLines line)
		{
			return line.TransactionHeader as TransactionHeader ??
						(line.AL_AH.IsValid ? line.Factory.Load<TransactionHeader>(line.AL_AH) : null);
		}

		TransactionHeader GetTransactionHeader(AccTransactionHeader header)
		{
			return header as TransactionHeader ??
						(header.PK.IsValid ? header?.Factory.Load<TransactionHeader>(header.PK) : null);
		}
	}
}
