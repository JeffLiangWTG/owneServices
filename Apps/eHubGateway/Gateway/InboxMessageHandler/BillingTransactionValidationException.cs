using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CargoWise.eHub.Gateway
{
	public class BillingTransactionValidationException : Exception
	{
		public const string MessagePrefix = "Billing transaction validation failed:";

		public BillingTransactionValidationException()
		{
		}

		public BillingTransactionValidationException(string message)
			: base(message)
		{
		}

		public BillingTransactionValidationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public BillingTransactionValidationException(IEnumerable<string> errors)
			: this(GetDetailedValidationErrorMessage(errors))
		{
		}

		public BillingTransactionValidationException(IEnumerable<string> errors, Exception innerException)
			: this(GetDetailedValidationErrorMessage(errors), innerException)
		{
		}

		protected BillingTransactionValidationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		static string GetDetailedValidationErrorMessage(IEnumerable<string> errors)
		{
			return MessagePrefix + Environment.NewLine + string.Join(Environment.NewLine, errors);
		}
	}
}