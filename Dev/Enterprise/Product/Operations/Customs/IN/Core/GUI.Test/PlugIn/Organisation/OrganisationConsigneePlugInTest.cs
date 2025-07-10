using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(OrganisationConsigneePlugIn))]
sealed class OrganisationConsigneePlugInTest : BaseOrganisationPlugInAbstractTest
{
	protected override Type ExpectPlugInUserControl => typeof(OrganisationConsigneePlugInUserControl);

	protected override BaseOrganisationPlugIn CreateNewPlugIn(OrgHeader organisation) => new OrganisationConsigneePlugIn(organisation);
}
