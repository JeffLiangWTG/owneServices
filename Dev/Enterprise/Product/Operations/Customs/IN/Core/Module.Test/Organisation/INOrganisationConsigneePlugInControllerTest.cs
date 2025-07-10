using System;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Module.Testing;

[TestedType(typeof(INOrganisationConsigneePlugInController))]
sealed class INOrganisationConsigneePlugInControllerTest : BaseOrganisationPlugInControllerAbstractTest
{
	protected override Type ExpectPlugInType => typeof(GUI.OrganisationConsigneePlugIn);

	protected override ControllerID GetControllerID() => ControllerIDs.Customs.IN.OrganisationConsigneePlugIn;
}
