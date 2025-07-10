using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.OrgPatternMatchRegen;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common
{
	class PatternMatchDataManagerTest : TestCaseWithFactory
	{
		public void TestOrganisation()
		{
			var rowFactory = new RowFactory();
			var factory = new WrapperFactory(rowFactory);
			var organisation = factory.New<MatchingOrganisation>();
			var manager = new PatternMatchDataManager(organisation, factory, Db.Connection);
			AssertEquals("manager.Organisation", organisation, manager.Organisation);
		}

		public void TestGetPatternMatchesAlreadyLoaded()
		{
			var rowFactory = new RowFactory();
			var factory = new WrapperFactory(rowFactory);
			var organisation = factory.New<MatchingOrganisation>();
			factory.New<MatchingPattern>().OS_OH = organisation.PK;
			factory.New<MatchingPattern>().OS_OH = organisation.PK;

			var manager = new PatternMatchDataManager(organisation, factory, Db.Connection);
			var alreadyLoaded = manager.GetPatternMatchesAlreadyLoaded();
			AssertNotNull(alreadyLoaded);
			AssertEquals("GetPatternMatchesAlreadyLoaded always returns 0 rows. All rows should be deleted at the outset.", 0, alreadyLoaded.Count());
		}

		public void TestCreateAndLoad()
		{
			var rowFactory = new RowFactory();
			var factory = new WrapperFactory(rowFactory);
			var organisation1 = factory.New<MatchingOrganisation>();
			organisation1.OH_Code = "Z_Z_Z_ONE";

			var manager = new PatternMatchDataManager(organisation1, factory, Db.Connection);
			var organisation1Match1 = manager.CreateNewPatternMatch();

			var organisation2 = factory.New<MatchingOrganisation>();
			organisation2.OH_Code = "Z_Z_Z_TWO";
			var organisation2Match1 = factory.New<MatchingPattern>();
			organisation2Match1.OS_OH = organisation2.PK;
			rowFactory.Save();

			var query = new ZQuery(OrgPatternMatchSchema.OS_OH, organisation1.PK);

			var patternMatches = manager.LoadPatternMatches(query);
			AssertEquals("patternMatches.Count()", 1, patternMatches.Count());
			AssertEquals("patternMatches.First()", organisation1Match1, patternMatches.First());

			var organisation1Match2 = manager.CreateNewPatternMatch();
			rowFactory.Save();

			patternMatches = manager.LoadPatternMatches(query);
			AssertEquals("patternMatches.Count()", 2, patternMatches.Count());
			AssertEquals("patternMatches.Contains(organisation1Match1)", true, patternMatches.Contains(organisation1Match1));
			AssertEquals("patternMatches.Contains(organisation1Match2)", true, patternMatches.Contains(organisation1Match2));
		}

		public void TestDeletePatternMatches()
		{
			var rowFactory = new RowFactory();
			var factory = new WrapperFactory(rowFactory);

			var organisation = factory.New<MatchingOrganisation>();
			organisation.OH_Code = "Z_Z_Z_BATS";

			var manager = new PatternMatchDataManager(organisation, factory, Db.Connection);

			var row1 = manager.CreateNewPatternMatch();
			row1.OS_FullCompanyName = "First Row";

			var row2 = manager.CreateNewPatternMatch();
			row2.OS_FullCompanyName = "Second Row";

			rowFactory.Save();

			var query1 = new ZQuery(OrgPatternMatchSchema.OS_FullCompanyName, "First Row");

			manager.DeletePatternMatches(query1);

			var reloadingRowFactory = new RowFactory();
			var reloadingFactory = new WrapperFactory(reloadingRowFactory);

			var row1Reloaded = reloadingFactory.Load<MatchingPattern>(row1.PK);
			var row2Reloaded = reloadingFactory.Load<MatchingPattern>(row2.PK);

			CombineAssertions(delegate
			{
				AssertNull("row1Reloaded", row1Reloaded);
				AssertEquals("row2Reloaded.IsDeleted", false, row2Reloaded.IsDeleted);
			});
		}

		public void TestSetUpdatedOrgAddressValidationStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORGNCN999";
			var address = org.MainAddress;
			address.OA_Code = "123 SMALL ST";
			address.OA_ValidationStatus = AddressValidationStatus.Verified;
			Factory.Save();

			var reLoadAddress = Factory.Load<OrgAddress>(new ZQuery(OrgAddressSchema.OA_Code, "123 SMALL ST")).Single();
			AssertEquals("Precondition: Validation status of target OrgAddress", AddressValidationStatus.Verified, reLoadAddress.OA_ValidationStatus);

			var manager = new ImportHandler(new AncillaryImportServices());
			using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Format(sourceXML, address.PK))))
			{
				manager.Import(inputStream);
			}

			address.Reload();
			AssertEquals(AddressValidationStatus.ToBeVerified, address.OA_ValidationStatus);
			var statusChangeLogs = address.Logs.Find(o => o.SL_SE_NKEvent.Equals(Events.StatusChange.Code));
			AssertEquals(1, statusChangeLogs?.Count());
			var statusChangeLog = statusChangeLogs.Single();
			AssertEquals("|NEW=NYV|OLD=VAD", statusChangeLog.SL_Reference);
		}

		const string sourceXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Body>
	<Organization version=""2.0"">
      <OrgHeader Action=""MERGE"">
        <Code>ORGNCN999</Code>
        <IsActive>true</IsActive>
        <FullName>Organization NCNM</FullName>
        <IsConsignor>true</IsConsignor>
        <Language>EN</Language>
        <ScreeningStatus>UNK</ScreeningStatus>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>123 SMALL ST</Code>
            <Language>EN</Language>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>123 SMALL ST</Address1>
            <City>SMALLVILLE</City>
            <State>SMALL</State>
            <PostCode>00001</PostCode>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>FI999</Code>
            </RelatedPortCode>
          </OrgAddress>
        </OrgAddressCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>FI999</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";
	}
}

