using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business
{
	[Serializable]
	public class AccountingException : OdysseyException
	{
		public AccountingException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected AccountingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class ReportException : AccountingException
	{
		public ReportException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ReportException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class AllocationSaveException : ZSaveException
	{
		public AllocationSaveException(ZDataException dataLayerException, BusinessObjectFactory factory)
			: base(dataLayerException, factory)
		{
		}

#if NETFRAMEWORK
		protected AllocationSaveException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public ZString UserFriendlyMessage
		{
			get
			{
				return AccountingConstants.ChequeNumberAllocationErrorMessages.ChequeBookIsBusyExceptionMessage;
			}
		}
	}

	[Serializable]
	public class AllocationChequeBookException : ZSaveException
	{
		public AllocationChequeBookException(ZDataException dataLayerException, BusinessObjectFactory factory)
			: base(dataLayerException, factory)
		{
		}

#if NETFRAMEWORK
		protected AllocationChequeBookException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public ZString UserFriendlyMessage
		{
			get
			{
				return AccountingConstants.ChequeNumberAllocationErrorMessages.ChequeBookIsFullExceptionMessage;
			}
		}
	}

	[Serializable]
	public class ENettProcessCreditCardException : AccountingException
	{
		public ENettProcessCreditCardException(string message, string eNettErrorCode, string eNettErrorMessage)
			: base(message)
		{
			this.eNettErrorCode = eNettErrorCode;
			this.eNettErrorMessage = eNettErrorMessage;
		}

#if NETFRAMEWORK
		protected ENettProcessCreditCardException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public readonly ZString eNettErrorCode;
		public readonly ZString eNettErrorMessage;

		public ZString UserFriendlyMessage
		{
			get
			{
				return AccountingConstants.ENettErrorMessages.ProcessCreditCardExceptionMessage;
			}
		}
	}

	[Serializable]
	public class ENettProcessDirectDebitFxException : AccountingException
	{
		public ENettProcessDirectDebitFxException(string message, string eNettErrorCode, string eNettErrorMessage)
			: base(message)
		{
			this.eNettErrorCode = eNettErrorCode;
			this.eNettErrorMessage = eNettErrorMessage;
		}

#if NETFRAMEWORK
		protected ENettProcessDirectDebitFxException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public readonly ZString eNettErrorCode;
		public readonly ZString eNettErrorMessage;
	}

	[Serializable]
	public class TransactionNotFoundException : AccountingException
	{
		public TransactionNotFoundException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected TransactionNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class InvoiceIsDeletedException : AccountingException
	{
		public InvoiceIsDeletedException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected InvoiceIsDeletedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class ReprintingInvoiceException : AccountingException
	{
		public ReprintingInvoiceException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ReprintingInvoiceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class CASSGstRegistryNotSetException : AccountingException
	{
		public CASSGstRegistryNotSetException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected CASSGstRegistryNotSetException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class MatchingCriteriaException : AccountingException
	{
		public MatchingCriteriaException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected MatchingCriteriaException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class JobCreationException : ExceptionHandledWithPopup
	{
		public JobCreationException(string error)
			: base(error)
		{
		}

#if NETFRAMEWORK
		protected JobCreationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class APAReconciliationTooManyMatchesFoundException : AccountingException
	{
		public APAReconciliationTooManyMatchesFoundException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected APAReconciliationTooManyMatchesFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class APAReconciliationTimeoutException : AccountingException
	{
		public APAReconciliationTimeoutException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected APAReconciliationTimeoutException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}	
}
