using System;
using System.Collections.Generic;

namespace Enterprise.BufferManagement.Business
{
	public class AcceptabilityBandMatchingWorkflowResultCollection
	{
		public AcceptabilityBandMatchingWorkflowResultCollection(Guid bandPK) {
			BandPK = bandPK;
		}

		public Guid BandPK { get; }

		public List<Guid> MatchingWorkflows { get; set; } = [];
	}
}
