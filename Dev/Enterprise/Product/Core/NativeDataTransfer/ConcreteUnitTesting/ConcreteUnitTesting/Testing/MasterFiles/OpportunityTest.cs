using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	public class OpportunityTest : TestCaseWithFactory
	{
		public void TestExportOpportunity()
		{
			using (var stream = NativeDataTransferTestHelper.ExportToStream(Opportunity))
			{
				var reader = new StreamReader(stream, Encoding.UTF8);
				string text = reader.ReadToEnd();
				CombineAssertions("Opportunity XML from import is incorrect", () =>
				{
					AssertContains(@"<OrgOpportunity Action=""MERGE"">", text);
					AssertContains($"<PK>{Opportunity.PK}</PK>", text);
					AssertContains("<OpportunityID>TestOpp</OpportunityID>", text);
					AssertContains("<OpportunityDescription>Test Current Description</OpportunityDescription>", text);
					AssertContains("<Stage>DIS</Stage>", text);
					AssertContains("<Status>CRT</Status>", text);
					AssertContains("<Outcome>OPN</Outcome>", text);
					AssertContains("<Source>WEB</Source>", text);
					AssertContains("<PackageType>FWD</PackageType>", text);
					AssertContains(@"<AssignedOffice TableName=""OrgAddress"">", text);
					AssertContains("<Code>100 CHAPEL STREET</Code>", text);
				});
			}
		}

		public void TestImportOpportunity_Add()
		{
			var opportunityQuery = new ZQuery(OrgOpportunitySchema.P8_OpportunityDescription, "Test Add Description");
			opportunityQuery.AddToFilter(OrgOpportunitySchema.P8_OpportunityType, "NEW");
			opportunityQuery.AddToFilter(OrgOpportunitySchema.P8_Stage, "DIS");
			opportunityQuery.AddToFilter(OrgOpportunitySchema.P8_Status, "CRT");
			opportunityQuery.AddToFilter(OrgOpportunitySchema.P8_Outcome, "OPN");
			opportunityQuery.AddToFilter(OrgOpportunitySchema.P8_Source, "WEB");
			opportunityQuery.AddToFilter(OrgOpportunitySchema.P8_PackageType, "FWD");
			opportunityQuery.AddToFilter(OrgOpportunitySchema.P8_DiscountAmount, 100);
			opportunityQuery.AddToFilter(OrgOpportunitySchema.P8_RentalMultiplier, 150);
			opportunityQuery.AddToFilter(OrgOpportunitySchema.P8_EstimatedCloseDate, new ZDateTime(2022, 01, 01));
			opportunityQuery.AddToFilter(OrgOpportunitySchema.P8_CloseCertainty, new ZByte(40));
			opportunityQuery.AddToFilter(OrgOpportunitySchema.P8_SourceDetails, "Test Source");
			opportunityQuery.AddToFilter(OrgOpportunitySchema.P8_DateForExchangeRate, new ZDateTime(2021, 12, 01));
			Assert("Precondition - Opportunity should not be present", !Factory.Exists(typeof(OrgOpportunity), opportunityQuery));

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(OpportunityXml_Add)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Opportunity
--- Import Process Finished -----------------------------------------------------------
OrgOpportunity - 1 inserts, 0 updates, 0 deletes
				".Trim();
				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}

			var opportunities = Factory.Load<OrgOpportunity>(opportunityQuery);
			AssertEquals("Opportunity should have been inserted by import", 1, opportunities.Length);

			var insertedOpportunity = opportunities.First();
			CombineAssertions(() =>
			{
				AssertEquals("AUD", insertedOpportunity.EstimatedValueCurrency.Code);
				AssertEquals(TestStaff.PK, insertedOpportunity.PrimarySalesPerson.PK);
				AssertEquals(AssignedOffice.PK, insertedOpportunity.AssignedOffice.PK);
				AssertEquals(TestOrg.PK, insertedOpportunity.Header.PK);
				AssertEquals(TestAddress.PK, insertedOpportunity.Address.PK);
				AssertEquals(TestContact.PK, insertedOpportunity.Contact.PK);
				AssertEquals(TestCompany.PK, insertedOpportunity.Company.PK);
			});
		}

		public void TestImportOpportunity_Update()
		{
			var opportunityXml = string.Format(OpportunityXml_Update, Opportunity.PK);
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(opportunityXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Opportunity
--- Import Process Finished -----------------------------------------------------------
OrgOpportunity - 0 inserts, 1 updates, 0 deletes
OrgOpportunityValue - 1 inserts, 0 updates, 0 deletes
				".Trim();
				AssertMultilineASCIIEquals("Log Text on Update", expectedLog, manager.GetLogs());
			}

			var importedOpportunity = new BusinessObjectFactory().Load<OrgOpportunity>(Opportunity.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Test New Description", importedOpportunity.P8_OpportunityDescription);
				AssertEquals("RES", importedOpportunity.P8_Stage);
				AssertEquals("SUS", importedOpportunity.P8_Status);
				AssertEquals("Test Source", importedOpportunity.P8_SourceDetails);
				AssertEquals(TestStaff.PK, importedOpportunity.PrimarySalesPerson.PK);
				AssertEquals(TestCompany.PK, importedOpportunity.Company.PK);
				AssertNull(importedOpportunity.AssignedOffice);
			});

			AssertEquals("Opportunity value should have been inserted by import", 1, importedOpportunity.ValueItems.Count);
			var opportunityValue = importedOpportunity.ValueItems[0];
			CombineAssertions(() =>
			{
				AssertEquals("CFS", opportunityValue.PV_RevenueType);
				AssertEquals(new ZDecimal(10), opportunityValue.PV_Value);
				AssertEquals("FLT", opportunityValue.PV_DiscountBasis);
				AssertEquals(new ZDecimal(1), opportunityValue.PV_Discount);
				AssertEquals(new ZDecimal(0), opportunityValue.PV_DiscountPercent);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestCompany = Factory.NewWithValidTestData<GlbCompany>();
			TestCompany.GC_Code = "TGC";
			TestStaff = Factory.NewWithValidTestData<GlbStaff>();
			TestStaff.GS_Code = "TGS";

			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg.OH_Code = "TESTORG1";
			TestAddress = TestOrg.Addresses.AddNew();
			TestAddress.OA_Code = "105 O'RIORDAN STREET";
			TestAddress.OA_Address1 = "105 O'RIORDAN STREET";
			TestContact = TestOrg.Contacts.AddNew();
			TestContact.OC_ContactName = "Test Contact";

			var assignedOfficeOrg = Factory.NewWithValidTestData<OrgHeader>();
			assignedOfficeOrg.OH_Code = "TESTORG2";
			AssignedOffice = assignedOfficeOrg.Addresses.AddNew();
			AssignedOffice.OA_Code = "100 CHAPEL STREET";
			AssignedOffice.OA_Address1 = "100 CHAPEL STREET";

			Opportunity = Factory.New<OrgOpportunity>();
			Opportunity.P8_OpportunityID = "TestOpp";
			Opportunity.P8_OpportunityDescription = "Test Current Description";
			Opportunity.P8_OpportunityType = "NEW";
			Opportunity.P8_Stage = "DIS";
			Opportunity.P8_Status = "CRT";
			Opportunity.P8_Outcome = "OPN";
			Opportunity.P8_Source = "WEB";
			Opportunity.P8_PackageType = "FWD";
			Opportunity.P8_OA_AssignedOffice = AssignedOffice.PK;
			Opportunity.P8_OH = TestOrg.PK;
			Opportunity.P8_GC = TestCompany.PK;

			Factory.Save();
		}

		OrgAddress AssignedOffice { get; set; }
		OrgOpportunity Opportunity { get; set; }
		OrgAddress TestAddress { get; set; }
		GlbCompany TestCompany { get; set; }
		OrgContact TestContact { get; set; }
		OrgHeader TestOrg { get; set; }
		GlbStaff TestStaff { get; set; }

		const string OpportunityXml_Add = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Opportunity version=""2.0"">
      <OrgOpportunity Action=""MERGE"">
        <OpportunityType>NEW</OpportunityType>
        <Stage>DIS</Stage>
        <Status>CRT</Status>
        <Outcome>OPN</Outcome>
        <Source>WEB</Source>
        <PackageType>FWD</PackageType>
        <EstimatedValue>0.0000</EstimatedValue>
        <DiscountAmount>100.0000</DiscountAmount>
        <RentalMultiplier>150.0000</RentalMultiplier>
        <EstimatedCloseDate>2022-01-01T00:00:00</EstimatedCloseDate>
        <ClosedDate></ClosedDate>
        <RecallDate></RecallDate>
        <OpportunityNotes></OpportunityNotes>
        <CloseCertainty>40</CloseCertainty>
        <LostReason></LostReason>
        <SourceDetails>Test Source</SourceDetails>
        <OpportunityDescription>Test Add Description</OpportunityDescription>
        <DateForExchangeRate>2021-12-01T00:00:00</DateForExchangeRate>
        <GlbCompanyCampaign />
        <EstimatedValueCurrency TableName=""RefCurrency"">
          <Code>AUD</Code>
          <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
        </EstimatedValueCurrency>
        <PrimarySalesPerson TableName=""GlbStaff"">
          <Code>TGS</Code>
        </PrimarySalesPerson>
        <AssignedOfficeContact TableName=""OrgContact"" />
        <AssignedOffice TableName=""OrgAddress"">
          <Code>100 CHAPEL STREET</Code>
          <OrgHeader>
            <Code>TESTORG2</Code>
          </OrgHeader>
        </AssignedOffice>
        <OrgContact>
          <ContactName>Test Contact</ContactName>
          <OrgHeader>
            <Code>TESTORG1</Code>
          </OrgHeader>
        </OrgContact>
        <OrgAddress>
          <Code>105 O'RIORDAN STREET</Code>
          <PK>f0229eb0-0809-4741-907f-83da320bfecc</PK>
          <OrgHeader>
            <Code>TESTORG1</Code>
          </OrgHeader>
        </OrgAddress>
        <OrgHeader>
          <Code>TESTORG1</Code>
          <PK>7fcf9b01-6116-46dc-baf0-7dbf684f4b19</PK>
        </OrgHeader>
        <Enquiry TableName=""OrgColdCallRegister"" />
        <ReferringOrganisation TableName=""OrgHeader"" />
        <ReferringContact TableName=""OrgContact"" />
        <GlbCompany>
          <Code>TGC</Code>
        </GlbCompany>
      </OrgOpportunity>
    </Opportunity>
  </Body>
</Native>";

		const string OpportunityXml_Update = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Opportunity version=""2.0"">
      <OrgOpportunity Action=""MERGE"">
        <PK>{0}</PK>
		    <OpportunityID>TestOpp</OpportunityID>
        <Stage>RES</Stage>
        <Status>SUS</Status>
        <SourceDetails>Test Source</SourceDetails>
        <OpportunityDescription>Test New Description</OpportunityDescription>
        <PrimarySalesPerson TableName=""GlbStaff"">
          <Code>TGS</Code>
        </PrimarySalesPerson>
        <OrgOpportunityValueCollection>
          <OrgOpportunityValue Action=""MERGE"">
            <RevenueType>CFS</RevenueType>
            <Value>10.0000</Value>
            <DiscountBasis>FLT</DiscountBasis>
            <Discount>1.0000</Discount>
            <DiscountPercent>0.000</DiscountPercent>
          </OrgOpportunityValue>
        </OrgOpportunityValueCollection>
        <AssignedOffice TableName=""OrgAddress"" />
      </OrgOpportunity>
    </Opportunity>
  </Body>
</Native>";
	}
}
