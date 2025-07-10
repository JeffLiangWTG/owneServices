using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class PSQuoteProcessTaskCollection : ProcessTaskCollection
	{
		public PSQuoteProcessTaskCollection(ProfessionalServicesQuote parent)
			: base(parent)
		{
		}

		public new PSQuoteProcessTask this[int index]
		{
			get { return (PSQuoteProcessTask)Elements[index]; }
		}

		public virtual new PSQuoteProcessTask AddNew()
		{
			return (PSQuoteProcessTask)base.AddNew();
		}

		#region Defaults

		protected override void SetDefaultsForNewChildCore(IWorkflowProviderCollection collection, ProcessTask processTask, bool defaultAssignedStaff)
		{
			base.SetDefaultsForNewChildCore(collection, processTask, defaultAssignedStaff);
			processTask.P9_OA = Parent.IM_OA_BranchAddress;
			processTask.P9_OC = Parent.IM_OC_Contact;
		}

		public new ProfessionalServicesQuote Parent
		{
			get { return (ProfessionalServicesQuote)base.Parent; }
		}

		#endregion
	}
}

