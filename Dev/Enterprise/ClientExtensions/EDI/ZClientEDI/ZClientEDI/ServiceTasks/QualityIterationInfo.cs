using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.Client.EDI.ServiceTasks
{
	public class QualityIterationInfo
	{
		public QualityIterationInfo(WorkItemProcessTask qcbTask, string qualityIterationType, string qualityIterationReasonCode, string resourceUnderReviewNk)
		{
			QcbTask = qcbTask;
			QualityIterationType = qualityIterationType;
			QualityIterationReasonCode = qualityIterationReasonCode;
			ResourceUnderReviewNk = resourceUnderReviewNk;
		}

		public WorkItemProcessTask QcbTask { get; }
		public string QualityIterationType { get; }
		public string QualityIterationReasonCode { get; }
		public string ResourceUnderReviewNk { get; set; }
	}
}
