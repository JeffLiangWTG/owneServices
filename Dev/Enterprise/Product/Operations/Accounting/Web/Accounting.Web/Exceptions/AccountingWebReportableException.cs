using System;
using CargoWise.Common.ErrorManagement;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Web.Exceptions;

[Serializable]
public class AccountingWebReportableException : Exception, IHasErrorReportID, IErrorReporterExtender
{
	public AccountingWebReportableException()
	{
	}

	public AccountingWebReportableException(string message)
		: base(message)
	{
	}

	public AccountingWebReportableException(string message, Exception inner)
		: base(message, inner)
	{
		Source = inner.Source;
	}

#if NETFRAMEWORK
	protected AccountingWebReportableException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{ }
#endif

	public string ErrorReportID { get; set; }

	public bool ShouldReportAlwaysInReportOnce { get; set; }
}
