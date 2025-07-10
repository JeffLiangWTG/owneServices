using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.MasterFiles.Organisations.Module;
using Enterprise.MasterFiles.Module;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIOrganisationControllerOverride : OrganisationController
	{
		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			return new EDIOrganisationForm((EDIOrgHeader)businessEntity, new EdiOrgViewController());
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EDIOrgHeader); }
		}
	}
}
