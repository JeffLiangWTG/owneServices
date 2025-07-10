using System;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.MasterFiles.Organisations.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	class EDIOrgContactsController : OrgContactsController
	{
		protected override ZOrganisationsForm GetFormCore(OrgHeader org)
		{
			return new EDIOrganisationForm((EDIOrgHeader)org, new EdiOrgViewController());
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EDIOrgContact); }
		}
	}
}
