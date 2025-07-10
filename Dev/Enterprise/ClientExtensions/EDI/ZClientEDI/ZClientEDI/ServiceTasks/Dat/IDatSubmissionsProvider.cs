using System;
using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.Client.EDI
{
	public interface IDatSubmissionsProvider : IDisposable
	{
		bool CanProvideSubmissions { get; }
		DatSubmissionBatch GetNextBatch();
		void UpdateStatusToNotified(EDIShelvesetInfo submission);
		void NotifySubmissionCouldNotBeProcessed(EDIShelvesetInfo submission);
	}
}
