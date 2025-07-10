using System;
using CargoWise.Common;

namespace Enterprise.Dash.Business
{
	public class DashErrorReporter : IDashErrorReporter
	{
		public IDisposable GatherAdditionalInformation(DashDocumentDataMessage message)
		{
			var disposable = ErrorReporter.GatherAdditionalInformation();

			var docPk = message?.LinkedDashDocument?.DDD_DocID;
			if (docPk != null)
			{
				ErrorReporter.SetAdditionalInfo("DocPK", docPk.ToString());
			}

			return disposable;
		}
		public IDisposable GatherAdditionalInformation(DashDocument dashDocument)
		{
			var disposable = ErrorReporter.GatherAdditionalInformation();

			ErrorReporter.SetAdditionalInfo("DocPK", dashDocument?.DDD_DocID.ToString());

			return disposable;
		}
		public IDisposable GatherAdditionalInformation(string apiPath)
		{
			var disposable = ErrorReporter.GatherAdditionalInformation();
			ErrorReporter.SetAdditionalInfo("RelativeUrl", apiPath);
			return disposable;
		}
	}
}
