using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business
{
	public abstract class AccountingEventArgs : EventArgs
	{
		public AccountingEventArgs()
		{
		}
	}

	public class AuditAndCashEventArgs : AccountingEventArgs
	{
		public SecurityCheckpoint SecurityCheckpoint;
		public AccTransactionHeader[] SelectedTransactions;

		public AuditAndCashEventArgs(SecurityCheckpoint securityCheckpoint, AccTransactionHeader[] selectedTransactions)
		{
			SecurityCheckpoint = securityCheckpoint;
			SelectedTransactions = selectedTransactions;
		}
	}

	public class AggrgeateEventArgs : AccountingEventArgs
	{
		public int NumberOfPeriodToExclude;
		public int PeriodToReverseInto;

		public AggrgeateEventArgs(int numberOfPeriodToExclude, int periodToReverse)
			: base()
		{
			this.NumberOfPeriodToExclude = numberOfPeriodToExclude;
			PeriodToReverseInto = periodToReverse;
		}
	}

	public class UserMessageEventArgs : AccountingEventArgs
	{
		public UserMessageEventArgs(string message)
		{
			Message = message;
		}

		public readonly string Message;
	}

	public class UserQueryEventArgs : AccountingEventArgs
	{
		public UserQueryEventArgs(string message, bool defaultResponse = true)
		{
			QueryMessage = message;
			Response = defaultResponse;
		}

		public readonly string QueryMessage;

		public bool Response
		{
			get;
			set;
		}
	}

	public class ChangedBizoEventArgs : AccountingEventArgs
	{
		public ChangedBizoEventArgs(BusinessObject newBizo)
		{
			NewBusinessObject = newBizo;
		}

		public readonly BusinessObject NewBusinessObject;
	}

	public class ProfitShareChargeCreationEventArgs : AccountingEventArgs
	{
		public ProfitShareChargeCreationEventArgs(ZString message)
		{
			Message = message;
		}

		public ZString Message { get; set; }
	}

	public class ComplianceSequenceRelatedExceptionEventArgs : AccountingEventArgs
	{
		public ComplianceSequenceRelatedExceptionEventArgs(ComplianceSequenceRelatedException exception)
		{
			complianceSequenceRelatedException = exception;
		}

		public readonly ComplianceSequenceRelatedException complianceSequenceRelatedException;
	}

	public class NoComplianceInvoicesToPrintHBDExceptionEventArgs : AccountingEventArgs
	{
		public NoComplianceInvoicesToPrintHBDExceptionEventArgs(NoComplianceInvoicesToPrintHBDException exception)
		{
			noComplianceInvoicesToPrintHBDException = exception;
		}

		public readonly NoComplianceInvoicesToPrintHBDException noComplianceInvoicesToPrintHBDException;
	}

	public class InvoiceBatchEventArgs : AccountingEventArgs
	{
		public InvoiceBatchEventArgs(InvoicingBase invoice)
		{
			Invoice = invoice;
		}

		public readonly InvoicingBase Invoice;
	}
}
