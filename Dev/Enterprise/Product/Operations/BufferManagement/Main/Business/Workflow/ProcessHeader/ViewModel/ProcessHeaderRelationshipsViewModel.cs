using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderRelationshipsViewModel : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ProcessHeaderRelationshipsViewModel(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[ChildEditable]
#if DEBUG
		public
#endif
 ActiveBusinessObjectCollection<ProcessHeader> TouchedProcessHeaders
		{
			get
			{
				if (touchedProcessHeaders == null)
				{
					touchedProcessHeaders = new ActiveBusinessObjectCollection<ProcessHeader>(Factory, new AdhocCollectionRelationship(typeof(ProcessHeader)));
					RegisterEditableChildObject(touchedProcessHeaders);
				}

				return touchedProcessHeaders;
			}
		}
		ActiveBusinessObjectCollection<ProcessHeader> touchedProcessHeaders;
		readonly HashSet<ZGuid> touchedProcessHeaderPKs = new HashSet<ZGuid>();

		[ChildEditable]
		public ActiveBusinessObjectCollection<ProcessHeader> ActiveProcessHeaders
		{
			get
			{
				if (activeProcessHeaders == null)
				{
					activeProcessHeaders = new ActiveBusinessObjectCollection<ProcessHeader>(Factory, new AdhocCollectionRelationship(typeof(ProcessHeader)));
					RegisterEditableChildObject(activeProcessHeaders);
				}

				return activeProcessHeaders;
			}
		}
		ActiveBusinessObjectCollection<ProcessHeader> activeProcessHeaders;

		public void NavigateTo(ProcessHeader processHeader)
		{
			// This is so we can track changes to ProcessHeader records outside the context of a single top-level parent job.
			if (!touchedProcessHeaderPKs.Contains(processHeader.PK))
			{
				AddToTouchedProcessHeaders(processHeader);
			}

			if (processHeader.JobHeader != null && !touchedProcessHeaderPKs.Contains(processHeader.JobHeader.PK))
			{
				AddToTouchedProcessHeaders(processHeader.JobHeader);
			}

			// We need to bind to a collection of just one item so that when the data source changes all the binding magic happens. Brett told me to do it - don't yell at me.
			if (activeProcessHeader != null)
			{
				ActiveProcessHeaders.RemoveFromRelationship(activeProcessHeader);
			}
			activeProcessHeader = processHeader;
			ActiveProcessHeaders.Add(processHeader);

			if (ActiveProcessHeaders.Count > 1)
			{
				ErrorReporter.ReportOnce("Somehow there are two ProcessHeaders in ActiveProcessHeaders - there should only be one.");
			}
		}

		void AddToTouchedProcessHeaders(ProcessHeader processHeader)
		{
			touchedProcessHeaderPKs.Add(processHeader.PK);
			TouchedProcessHeaders.Add(processHeader);
		}

		ProcessHeader activeProcessHeader;

		public void AddLinks(IReadOnlyCollection<ProcessHeader> selectedProcessHeaders, ProcessHeaderLinkCollection collection, RelationshipDirection target)
		{
			foreach (var processHeader in selectedProcessHeaders)
			{
				var link = collection.AddNew();
				switch (target)
				{
					case RelationshipDirection.From:
						link.FP_FH_HeaderFrom = processHeader.PK;
						break;

					case RelationshipDirection.To:
						link.FP_FH_HeaderTo = processHeader.PK;
						break;
				}

				link.ProcessNewLink();
			}
		}

		public void RemoveLinks(IReadOnlyCollection<ProcessHeaderLink> selectedLinks, ProcessHeaderLinkCollection collection)
		{
			ProcessHeaderLinkViewModelHelper.RemoveLinks(selectedLinks, collection);
		}
	}
}
