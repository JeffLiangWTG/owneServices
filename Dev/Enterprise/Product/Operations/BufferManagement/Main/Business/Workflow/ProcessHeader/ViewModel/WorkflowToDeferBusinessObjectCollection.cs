using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowToDeferBusinessObjectCollection : NonPersistentBusinessObjectCollection<WorkflowToDeferBusinessObject>
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public WorkflowToDeferBusinessObjectCollection(ProcessHeader workflow)
		{
			AddPostRequisites(workflow);
		}

		public WorkflowToDeferBusinessObjectCollection(ProcessJobHeader jobHeader, IEnumerable<ZGuid> fromComponentPKs)
		{
			AddWorkflowsForJob(jobHeader, fromComponentPKs);
		}

		void AddWorkflowsForJob(ProcessJobHeader jobHeader, IEnumerable<ZGuid> fromComponentPKs)
		{
			var workflows = jobHeader.ProcessHeaders.Where(x => x.IsOpen && x.WorkflowParentLink == null);
			var toAdd = workflows.Select(x => new WorkflowToDeferBusinessObject(x, null, true)
			{
				ActionToBeTaken = fromComponentPKs.Contains(x.FH_FC_CurrentComponent)
				? WorkflowDeferalActionList.Codes.Defer
				: WorkflowDeferalActionList.Codes.None
			});
			AddRange(toAdd);
		}

		void AddPostRequisites(ProcessHeader processHeader, HashSet<ProcessHeader> addedPostRequisites = null)
		{
			if (addedPostRequisites == null)
			{
				addedPostRequisites = new HashSet<ProcessHeader>();
			}

			var postreqs = from x in processHeader.GetPostRequisitesAndTheirLinksDownTheTree()
						   let postreq = x.ProcessHeader
						   where (postreq.IsWorkflow && postreq.IsReleased) || (!postreq.IsWorkflow && ((ProcessJobHeader)postreq).ProcessHeaders.Any(y => y.IsReleased))
						   select x;

			foreach (var pair in postreqs.Where(x => !addedPostRequisites.Contains(x.ProcessHeader)))
			{
				var postreq = pair.ProcessHeader;
				addedPostRequisites.Add(postreq);

				Add(new WorkflowToDeferBusinessObject(postreq, pair.LinkToProcessHeader, false));
				AddPostRequisites(postreq, addedPostRequisites);
			}
		}

		#region BusinessObjectCollection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion
	}
}
