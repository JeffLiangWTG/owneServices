using CargoWise.EntityFramework;

namespace Enterprise.CustomerService.Business
{
	public class IncidentApprovalAttachmentCollection : NonPersistentBusinessObjectCollection<IncidentApprovalAttachment>
	{
		public IncidentApprovalAttachmentCollection(IncidentApproval incident)
		{
			this.incident = incident;
		}
		readonly IncidentApproval incident;

		public override void Load()
		{
			RemoveAll();
			var allEDocs = incident.DocManagerInfo.AllEDocs;
			for (int i = 0; i < allEDocs.Count; i++)
			{
				if (!allEDocs[i].IsDeleted)
				{
					var doc = new IncidentApprovalAttachment(allEDocs[i]);
					Add(doc);
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IncidentApprovalAttachment();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
