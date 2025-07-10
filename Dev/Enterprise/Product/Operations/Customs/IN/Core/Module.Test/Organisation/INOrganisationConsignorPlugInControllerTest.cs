using System;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Module.Testing;

[TestedType(typeof(INOrganisationConsignorPlugInController))]
sealed class INOrganisationConsignorPlugInControllerTest : BaseOrganisationPlugInControllerAbstractTest
{
	protected override Type ExpectPlugInType => typeof(GUI.OrganisationConsignorPlugIn);

	protected override ControllerID GetControllerID() => ControllerIDs.Customs.IN.OrganisationConsignorPlugIn;
}
