using System;
using System.IO;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.SystemMerge.GUI.Testing
{
	sealed class SysMergeOrganisationXmlDataTransferDirectorTest : TestCaseWithFactory
	{
		public void TestAdapter()
		{
			var director = new SysMergeOrganisationXmlDataTransferDirectorForTest();
			AssertEquals("Adapter", typeof(SysMergeOrganisationValueObjectDataAdapter), director.Adapter.GetType());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportMultipleOrgsSavesSuccessfulOrgEvenWhenOthersFail()
		{
			var orgPk1 = new ZGuid("1b334475-15ee-4890-9c8d-db8ee8ea36ee");
			var orgPk2 = new ZGuid("17e67617-1e39-469a-bd12-d3a602bfcfc5");
			var orgName3 = "TESTORGDUP";

			var org1BeforeImport = Factory.Load<OrgHeader>(orgPk1);
			var org2BeforeImport = Factory.Load<OrgHeader>(orgPk2);
			var org3BeforeImportCount = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, orgName3)).Length;
			AssertNull("[PRE-CONDITION] TESTORG should not exist", org1BeforeImport);
			AssertNull("[PRE-CONDITION] TESTORGFAIL should not exist", org2BeforeImport);
			AssertEquals("[PRE-CONDITION] TESTORGDUP should not exist", 0, org3BeforeImportCount);

			// Add Dummy Test Check Constraint on OH_Language to force import of TESTORGFAIL to fail
			((IDbConnected)Factory).Connection.ExecuteNonQuery("ALTER TABLE dbo.OrgHeader ADD CONSTRAINT OrgHeader_DummyCheck CHECK (OH_Language != '~~~')");

			//Import XML File To Database
			var director = new SysMergeOrganisationXmlDataTransferDirectorForTest();
			var fileName1 = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Core", "DataTransfer", "DataTransfer.SystemMerge", "GUI", "Testing", "3Org.xml");
			director.Import(fileName1, new NotificationBuffer(), SourceInfo.EmptySourceInfo);

			//Load OrgHeader from Database using a new factory
			var loadFactory = new BusinessObjectFactory();
			var orgHeader1 = loadFactory.Load<OrgHeader>(orgPk1);
			var orgHeader2 = loadFactory.Load<OrgHeader>(orgPk2);
			var orgHeader3Count = loadFactory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, orgName3)).Length;
			AssertNotNull("TESTORG should have been imported", orgHeader1);
			AssertNull("TESTORGFAIL should have error, invalid OH_Language", orgHeader2);
			AssertEquals("TESTORGDUP should have skipped, same PK as TESTORG", 0, orgHeader3Count);

			//Check OrgAddress is import correctly	
			var orgAddress1 = loadFactory.Load<OrgAddress>(new ZGuid("5f9e648d-532a-4f15-af20-4c75191fb385"));
			var orgAddress2 = loadFactory.Load<OrgAddress>(new ZGuid("AB699C02-5600-46E7-8E86-44235F3BD84A"));
			var orgAddress3 = loadFactory.Load<OrgAddress>(new ZGuid("fc141111-f27b-4dac-9102-6313e5016c0b"));
			AssertNotNull("OrgAddress1 should have been imported", orgAddress1);
			AssertNull("OrgAddress2 should NOT have been imported", orgAddress2);
			AssertNull("OrgAddress3 should NOT have been imported", orgAddress3);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportMultipleOrgs_WithInvalidGlbCompanyCode()
		{
			var orgPk1 = new ZGuid("a4865c3f-4fe8-405a-8f80-48a2b222995d");
			var orgPk2 = new ZGuid("14be8949-4b70-40ac-99e8-b20ea36917fb");

			var org1BeforeImport = Factory.Load<OrgHeader>(orgPk1);
			var org2BeforeImport = Factory.Load<OrgHeader>(orgPk2);

			AssertNull("[PRE-CONDITION] CODE_INVALID ORG should not exist", org1BeforeImport);
			AssertNull("[PRE-CONDITION] CODE_VALID ORG should not exist", org2BeforeImport);

			//Import XML File To Database
			var director = new SysMergeOrganisationXmlDataTransferDirectorForTest();
			var fileName1 = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Core", "DataTransfer", "DataTransfer.SystemMerge", "GUI", "Testing", "2ORG_Valid_Invalid_Glb_Code.xml");
			director.Import(fileName1, new NotificationBuffer(), SourceInfo.EmptySourceInfo);

			//Load OrgHeader from Database using a new factory
			var loadFactory = new BusinessObjectFactory();
			var orgHeader1 = loadFactory.Load<OrgHeader>(orgPk1);
			var orgHeader2 = loadFactory.Load<OrgHeader>(orgPk2);

			AssertNull("CODE_INVALID ORG should have error, invalid GLB_Code", orgHeader1);
			AssertNotNull("CODE_VALID ORG should have been imported", orgHeader2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportMultipleOrgs_WithInvalidGlbCompanyCodeInTheEndOfTheXmlFile()
		{
			var orgPk1 = new ZGuid("a4865c3f-4fe8-405a-8f80-48a2b222995d");
			var orgPk2 = new ZGuid("14be8949-4b70-40ac-99e8-b20ea36917fb");

			var org1BeforeImport = Factory.Load<OrgHeader>(orgPk1);
			var org2BeforeImport = Factory.Load<OrgHeader>(orgPk2);

			AssertNull("[PRE-CONDITION] CODE_INVALID ORG should not exist", org1BeforeImport);
			AssertNull("[PRE-CONDITION] CODE_VALID ORG should not exist", org2BeforeImport);

			//Import XML File To Database
			var director = new SysMergeOrganisationXmlDataTransferDirectorForTest();
			var fileName1 = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Core", "DataTransfer", "DataTransfer.SystemMerge", "GUI", "Testing", "2ORG_Valid_Invalid_Glb_Code2.xml");
			var notification = new NotificationBuffer();
			director.Import(fileName1, notification, SourceInfo.EmptySourceInfo);

			//Load OrgHeader from Database using a new factory
			var loadFactory = new BusinessObjectFactory();
			var orgHeader1 = loadFactory.Load<OrgHeader>(orgPk1);
			var orgHeader2 = loadFactory.Load<OrgHeader>(orgPk2);

			AssertNull("CODE_INVALID ORG should have error, invalid GLB_Code", orgHeader1);
			AssertNotNull("CODE_VALID ORG should have been imported", orgHeader2);

			bool hasCompanyCodeNotExistError = false;
			foreach (var item in notification.GetEventsByType(ErrorType.Error))
			{
				if (item.Message.Contains(@"Error: Error when importing organization [(a4865c3f-4fe8-405a-8f80-48a2b222995d) - CODE_ISYD - CODE_INVALID]. Details: Could not find Company code = [XXX].
Please create this company or adjust your company code mapping in the registry (System -> Data Import Settings -> System Merge -> Import Company Code Mapping).
Then retry the import operation."))
				{
					hasCompanyCodeNotExistError = true;
					break;
				}
			}
			Assert("Shoud have shown the right error message", hasCompanyCodeNotExistError);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportMultipleOrgs_WithCompanyCodeMapping()
		{
			var orgPk1 = new ZGuid("a4865c3f-4fe8-405a-8f80-48a2b222995d");
			var orgPk2 = new ZGuid("14be8949-4b70-40ac-99e8-b20ea36917fb");

			var org1 = Factory.Load<OrgHeader>(orgPk1);
			var org2 = Factory.Load<OrgHeader>(orgPk2);

			AssertNull("[PRE-CONDITION] Org with company data code XXX - should not exist initially", org1);
			AssertNull("[PRE-CONDITION] Org with company data code EDI - should not exist initially", org2);

			// Create mappings XXX -> DEM and SIN -> (blank)
			var mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("XXX", "DEM");
			mappingList.AddPair("SIN", "");
			SystemDataRegistry.Instance.SystemMergeCompanyCodeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingList);

			//Import XML File To Database
			var director = new SysMergeOrganisationXmlDataTransferDirectorForTest();
			var fileName = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Core", "DataTransfer", "DataTransfer.SystemMerge", "GUI", "Testing", "2ORG_Valid_Invalid_Glb_Code.xml");
			director.Import(fileName, new NotificationBuffer(), SourceInfo.EmptySourceInfo);

			//Load OrgHeader from Database using a new factory
			var loadFactory = new BusinessObjectFactory();
			org1 = loadFactory.Load<OrgHeader>(orgPk1);
			org2 = loadFactory.Load<OrgHeader>(orgPk2);

			// Org1 - with XXX (DEM) company data code
			AssertNotNull("Org with company data code XXX - should be imported", org1);
			var companyDataQuery1 = new ZQuery(OrgCompanyDataSchema.OB_OH, org1.PK);
			var companyDataArray1 = loadFactory.Load<OrgCompanyData>(companyDataQuery1);
			AssertEquals("Org1 Company Data count", 2, companyDataArray1.Length);
			var org1company1 = loadFactory.Load<GlbCompany>(companyDataArray1[0].OB_GC);
			var org1company2 = loadFactory.Load<GlbCompany>(companyDataArray1[1].OB_GC);

			switch (org1company1.GC_Code)
			{
				case "EDI":
					AssertEquals("Org1 second Company Data code", "DEM", org1company2.GC_Code);
					break;
				case "DEM":
					AssertEquals("Org1 second Company Data code", "EDI", org1company2.GC_Code);
					break;
				default:
					Fail("Unexpected Org1 first Company Data code" + org1company1.GC_Code);
					break;
			}

			// Org2 - with SIN (blank) company data code
			AssertNotNull("Org with company data code EDI - should be imported", org2);
			var companyDataQuery2 = new ZQuery(OrgCompanyDataSchema.OB_OH, org2.PK);
			var companyDataArray2 = loadFactory.Load<OrgCompanyData>(companyDataQuery2);
			AssertEquals("Org2 Company Data count - no company data as mapped to (blank) code", 0, companyDataArray2.Length);
		}

		sealed class SysMergeOrganisationXmlDataTransferDirectorForTest : SysMergeOrganisationXmlDataTransferDirector
		{
			internal new IValueObjectDataAdapter Adapter => base.Adapter;
		}
	}
}
