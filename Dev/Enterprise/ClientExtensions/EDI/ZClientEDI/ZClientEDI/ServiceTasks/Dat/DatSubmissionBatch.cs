using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.Client.EDI
{
	public readonly struct DatSubmissionBatch
	{
		public DatSubmissionBatch(IReadOnlyCollection<EDIShelvesetInfo> submissions, BusinessObjectFactory batchFactory, bool isLastBatch)
		{
			Submissions = submissions;
			BatchFactory = batchFactory;
			IsLastBatch = isLastBatch;
		}

		public IReadOnlyCollection<EDIShelvesetInfo> Submissions { get; }
		public BusinessObjectFactory BatchFactory { get; }
		public bool IsLastBatch { get; }
	}
}
