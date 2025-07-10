using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.Accounting.Business
{
	public class ReportErrorHandler : IReportErrorHandler
	{
		public (bool ShouldSkip, string DisplayMessage) ShouldSkipReportError(Exception exception)
		{
			Argument.NotNull(exception, nameof(exception));

			if (exception is SQLExecutionException)
			{
				var message = exception.InnerException?.Message ?? exception.Message;
				if (SkipReportErrorMsgList.Any(messageToSkip => message.Contains(messageToSkip)))
				{
					return (true, message);
				}
			}

			return (false, string.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		readonly string[] SkipReportErrorMsgList = new[] {
			"China local tier 2 and tier 3 GL Accounts with the same tier 1 account number must have the same account type. Please check your local GL accounts and its sub GL accounts, to ensure that they have the same account type."
		};
	}
}
