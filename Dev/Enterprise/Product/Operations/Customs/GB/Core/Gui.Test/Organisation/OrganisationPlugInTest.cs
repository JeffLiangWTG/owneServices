using Enterprise.Customs.GB.CDS.Organisation;
using Enterprise.Customs.GB.GUI.Organisation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	[TestedType(typeof(OrganisationPlugIn))]
	sealed class OrganisationPlugInTest : ZPlugInGenericTest
	{
		public void TestPlugInProperties()
		{
			using var plugIn = GetPlugInToTest();
			AssertType<OrgHeaderWrapper>(plugIn.BusinessEntity);
			AssertType<OrganisationPlugInMenu>(plugIn.TopLevelMenu);
			AssertType<Customs.GUI.MessageUserControl>(plugIn.UserControl);
			AssertEquals("Customs Messaging", plugIn.Name);
			AssertEquals(true, plugIn.ShouldBeReadOnly);
		}

		protected override ZPlugIn GetPlugInToTest() => new OrganisationPlugIn(Factory.New<OrgHeader>());
	}
}
