using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(OrganisationDetailsPlugIn))]
sealed class OrganisationDetailsPlugInTest : BaseOrganisationPlugInAbstractTest
{
	protected override string ExpectedPlugInName => "India";

	protected override Type ExpectPlugInUserControl => typeof(OrganisationDetailsPlugInUserControl);

	protected override BaseOrganisationPlugIn CreateNewPlugIn(OrgHeader organisation) => new OrganisationDetailsPlugIn(organisation);
}
