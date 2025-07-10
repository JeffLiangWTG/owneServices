using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class EdiHelpErrorLogLookups : HelpErrorLogLookups
	{
		public EdiHelpErrorLogLookups(AutoHelpErrorLog parent)
			: base(parent)
		{
		}

		public NewWorkItemCollection WorkItems
		{
			get
			{
				if (workItems == null)
				{
					workItems = new NewWorkItemCollection(Factory);
				}
				return workItems;
			}
		}

		NewWorkItemCollection workItems;
	}
}

