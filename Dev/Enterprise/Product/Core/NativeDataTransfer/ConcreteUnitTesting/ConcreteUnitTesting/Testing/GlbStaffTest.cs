using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	class GlbStaffTest : TestCaseWithFactory
	{
		public void TestCanImportGlbStaffWithGroups()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "JOHNNY ROCKET";
			staff.GS_LoginName = "JOHNNY";
			staff.GS_Code = "JR";

			Factory.Save();

			AssertEquals("Precondition: Groups Linked (by code)", string.Join(", ", staff.Groups.Cast<GlbGroup>().Select(o => o.GG_Code).OrderBy(o => o)), "ALL");

			var encoding = new System.Text.UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(GlbStaffXmlWithGroups)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Staff
--- Import Process Finished -----------------------------------------------------------
GlbStaff - 0 inserts, 1 updates, 0 deletes
GlbGroupLink - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}

			staff = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
			AssertEquals("Groups Linked (by code)", string.Join(", ", staff.Groups.Cast<GlbGroup>().Select(o => o.GG_Code).OrderBy(o => o)), "ALL, PMG");
		}

		public void TestExportFileNotContainsGeoLocation()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_GeoLocation = new CargoWise.Types.ZGeography("0,0");

			Factory.Save();

			string actualMessage = "";
			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new Adapter.Utils.BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(staff))
			using (var reader = new StreamReader(dataStream))
			{
				actualMessage = reader.ReadToEnd();
			}

			var element = XElement.Load(new StringReader(actualMessage));

			var geoLocation = element.Descendants().Where(e => e.Name.LocalName == "GeoLocation").ToArray();

			AssertEquals(0, geoLocation.Length);
		}

		#region GlbStaffXmlWithGroups

		const string GlbStaffXmlWithGroups = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>BENGOVSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Staff version=""2.0"">
      <GlbStaff Action=""MERGE"">
        <Code>JR</Code>
        <IsActive>true</IsActive>
        <LoginName>JOHNNY</LoginName>
        <FullName>JOHNNY ROCKET</FullName>
        <Title>The Incomparably Talented</Title>
        <EmailAddress>johnathon.rocket@cargowise.com</EmailAddress>
        <GlbGroupLinkCollection>
          <GlbGroupLink Action=""MERGE"">
            <MembershipType>TOP</MembershipType>
            <SkillLevel>10</SkillLevel>
            <GlbGroup>
              <Code>PMG</Code>
            </GlbGroup>
          </GlbGroupLink>
        </GlbGroupLinkCollection>
      </GlbStaff>
    </Staff>
  </Body>
</Native>";

		#endregion
	}
}
