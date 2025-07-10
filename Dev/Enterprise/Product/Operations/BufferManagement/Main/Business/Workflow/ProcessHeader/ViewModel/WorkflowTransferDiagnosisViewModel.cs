using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowTransferDiagnosisViewModel : NonPersistentBusinessObject, IObsoleteValidation
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public WorkflowTransferDiagnosisViewModel(ProcessHeader workflow)
			: base(workflow.Factory)
		{
			RegisterEditableChildObject(workflow);
			this.workflow = workflow;
		}

		#region Related Business Objects

		[ChildEditable]
		public ProcessHeader Workflow
		{
			get { return workflow; }
		}

		readonly ProcessHeader workflow;

		public WorkflowTransferDiagnosisCollection TransferDiagnoses => transferDiagnoses ?? (transferDiagnoses = new WorkflowTransferDiagnosisCollection(workflow));

		WorkflowTransferDiagnosisCollection transferDiagnoses;

		#endregion

		#region Properties

		[ReadOnly(true)]
		[ResourceStringData("WorkflowTransferFailure.LastSuccessfulRelease", Caption = "Last Successful Release")]
		public ZString LastSuccessfulRelease
		{
			get { return lastSuccessfulRelease ?? (lastSuccessfulRelease = Workflow.GetSuccessfulReleaseNotes()); }
		}
		string lastSuccessfulRelease;

		#endregion
	}
}
