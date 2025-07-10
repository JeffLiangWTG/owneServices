using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	class EUOrganisationConsigneePlugInControllerTests
	{
		[TestedType(typeof(EUOrganisationConsigneePlugInController))]
		class EUOrganisationConsigneePlugInControllerBasherTest : ZControllerBasherTest
		{
			public override Type ControllerToBashType
			{
				get { return typeof(EUOrganisationConsigneePlugInController); }
			}

			protected override ControllerID GetControllerID()
			{
				return ControllerIDs.Customs.EU.OrganisationConsigneePlugIn;
			}
		}
	}
}
