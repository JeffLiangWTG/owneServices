using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromWorkProject : FreightWrapper
	{
		public FreightWrapperFromWorkProject(Project project, BusinessObjectFactory factory)
			: base(project, factory)
		{
			this.project = project;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return project.PK;
		}

		protected override Project GetWorkProject()
		{
			return project;
		}

		protected override ZString GetJobNumber()
		{
			return project.Number;
		}

		readonly Project project;
	}
}
