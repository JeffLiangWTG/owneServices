using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ProjectProcessTaskCollection : ProcessManagement.Business.ProjectProcessTaskCollection
	{
		public ProjectProcessTaskCollection(EDIProject project)
			: base(project)
		{
		}

		public new ProjectProcessTask this[int index]
		{
			get { return (ProjectProcessTask)Elements[index]; }
		}

		public virtual new ProjectProcessTask AddNew()
		{
			return (ProjectProcessTask)base.AddNew();
		}

		#region Country

		public override ZString OriginCountry
		{
			get { return Country; }
		}

		public override ZString DestinationCountry
		{
			get { return Country; }
		}

		ZString Country
		{
			get { return Parent.ClientOrganisation != null ? Parent.ClientOrganisation.CountryCode : ZString.Empty; }
		}

		#endregion

		#region Defaults

		protected override void SetDefaultsForNewChildCore(IWorkflowProviderCollection collection, ProcessTask processTask, bool defaultAssignedStaff)
		{
			base.SetDefaultsForNewChildCore(collection, processTask, defaultAssignedStaff);

			processTask.OrganisationPK = Parent.ClientOrganisationPK;
			processTask.P9_OA = Parent.WKP_OA_ClientAddress;
			processTask.P9_OC = Parent.WKP_OC_Contact;

			GlbStaff projectManager = Parent.ProjectManager;
			if (processTask.P9_GS_NKAssignedStaffMember.IsEmpty && projectManager != null && processTask.P9_G4_RequiredCapability.IsEmpty)
			{
				processTask.P9_GS_NKAssignedStaffMember = projectManager.GS_Code;
			}
		}

		#endregion

		public new EDIProject Parent
		{
			get { return (EDIProject)base.Parent; }
		}

		internal void SetDefaultsForNewChildForTest(ProcessTask processTask, bool defaultAssignedStaff)
		{
			SetDefaultsForNewChildCore(this, processTask, defaultAssignedStaff);
		}
	}
}
