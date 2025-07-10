using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Module.Testing
{
	[TestedType(typeof(NctsOrganisationDetailsPlugInController))]
	class NctsOrganisationDetailsTests : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType() => typeof(OrgHeader);

		protected override string CountryCode => Core.Constants.CountryCodes.Latvia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.NctsOrganisationDetailsPlugIn;
	}
}
