using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	class EDIProjectFormForTest : EDIProjectForm
	{
		public EDIProjectFormForTest(EDIProject project) : base(project)
		{
		}

		public IEnumerable<MenuItem> ProjectAction_Exposed
		{
			get
			{
				return base.ProjectActionsMenuItems;
			}
		}
	}
}
