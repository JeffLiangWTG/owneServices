using System;
using CargoWise.Common;

namespace Enterprise.RemotePrinting.Engine
{
	public class PrintResult
	{
		PrintResult(Guid jobPK, string printerName, string failureReason, Exception unhandledException)
			: this(jobPK, printerName, failureReason)
		{
			Argument.NotNull(unhandledException, nameof(unhandledException));

			GetUnhandledException = () => unhandledException;
		}

		PrintResult(Guid jobPK, string printerName, string failureReason)
			: this(jobPK, isSuccess: false)
		{
			PrinterName = Argument.NotNullOrEmpty(printerName, nameof(printerName));
			FailureReason = Argument.NotNullOrEmpty(failureReason, nameof(failureReason));
		}

		PrintResult(Guid jobPK, bool isSuccess)
		{
			JobPK = jobPK;
			IsSuccess = isSuccess;
		}

		public bool HadUnhandledException => GetUnhandledException != null;

		public static PrintResult Fail(Guid jobPK) => new PrintResult(jobPK, isSuccess: false);
		public static PrintResult Success(Guid jobPK) => new PrintResult(jobPK, isSuccess: true);

		public static PrintResult WithFailureReason(Guid jobPK, string printerName, string failureReason) => new PrintResult(jobPK, printerName, failureReason);

		public static PrintResult WithUnhandledException(Guid jobPK, string printerName, string failureReason, Exception unhandledException)
		{
			return new PrintResult(jobPK, printerName, failureReason, unhandledException);
		}

		public bool IsSuccess { get; }
		public Guid JobPK { get; }

		// Used for Errors
		public string PrinterName { get; }
		public string FailureReason { get; }
		public Func<Exception> GetUnhandledException { get; }
	}
}
