using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class EDIProjectController : ProcessManagement.Module.ProjectController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EDIProjectForm((EDIProject)businessEntity);
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EDIProject); }
		}
	}
}
