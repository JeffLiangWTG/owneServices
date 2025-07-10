using System;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Module;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	class EdiOrganisationActionSupporter : OrganisationActionSupporter
	{
		public override Type RootType => typeof(EDIOrgHeader);
	}
}
