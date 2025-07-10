using System;

namespace Enterprise.Dash.Business
{
	public interface IDashErrorReporter
		{
			IDisposable GatherAdditionalInformation(DashDocumentDataMessage message);
			IDisposable GatherAdditionalInformation(DashDocument dashDocument);
			IDisposable GatherAdditionalInformation(string apiPath);
	}
}
