using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ClientFeatureRequestValueAndContributionUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		#region IUniqueIndexFailureHandler Members

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			notifier.ReportError("An EBV / Contribution data has previously been created for this form. Please close and reopen the form again.", "Duplication Error");
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get { yield return ClientFeatureRequestValueAndContributionSchema.Constants.Indexes.NR_UC__T9_ParentID; }
		}

		#endregion
	}
}

