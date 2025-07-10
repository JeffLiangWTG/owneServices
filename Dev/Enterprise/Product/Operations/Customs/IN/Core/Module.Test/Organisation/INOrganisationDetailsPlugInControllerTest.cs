using System;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Module.Testing;

[TestedType(typeof(INOrganisationDetailsPlugInController))]
sealed class INOrganisationDetailsPlugInControllerTest : BaseOrganisationPlugInControllerAbstractTest
{
	protected override Type ExpectPlugInType => typeof(GUI.OrganisationDetailsPlugIn);

	protected override string ExpectedPlugInTabCaption => "India";

	protected override ControllerID GetControllerID() => ControllerIDs.Customs.IN.OrganisationDetailsPlugIn;
}
