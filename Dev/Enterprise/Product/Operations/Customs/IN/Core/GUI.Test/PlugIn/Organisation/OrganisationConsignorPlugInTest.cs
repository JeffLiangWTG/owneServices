using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(OrganisationConsigneePlugIn))]
sealed class OrganisationConsignorPlugInTest : BaseOrganisationPlugInAbstractTest
{
	protected override Type ExpectPlugInUserControl => typeof(OrganisationConsignorPlugInUserControl);

	protected override BaseOrganisationPlugIn CreateNewPlugIn(OrgHeader organisation) => new OrganisationConsignorPlugIn(organisation);
}
