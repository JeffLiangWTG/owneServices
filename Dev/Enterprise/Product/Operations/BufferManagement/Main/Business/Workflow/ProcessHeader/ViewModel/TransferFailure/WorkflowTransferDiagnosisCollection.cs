using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowTransferDiagnosisCollection : NonPersistentBusinessObjectCollection<WorkflowTransferDiagnosis>
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public WorkflowTransferDiagnosisCollection(ProcessHeader workflow)
		{
			SetReadOnlyIncludingChildren(true);

			var currentComponent = workflow.CurrentComponent;
			if (currentComponent != null)
			{
				foreach (var link in currentComponent.FromMeToOthersLinks.OrderBy(l => Math.Abs(l.ComponentTo.FC_DisplaySequence - currentComponent.FC_DisplaySequence)))
				{
					var transferFailure = new WorkflowTransferDiagnosis(link, workflow);
					transferFailure.TryTransfer();
					Add(transferFailure);
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WorkflowTransferDiagnosis();
		}
	}
}
