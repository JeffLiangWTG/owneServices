using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeOrgCountryDataValueObjectHelperTest : TestCaseWithFactory
	{
		public void TestExportAndImport()
		{
			SysMergeOrgCountryDataValueObjectHelper testHelper = new SysMergeOrgCountryDataValueObjectHelper();
			INotifications notifications = new NotificationBuffer();

			// Create and populate OrgHeader with a few OrgCountryData dependent objects
			OrgHeaderForDataTransfer org1 = Factory.New<OrgHeaderForDataTransfer>();
			AddNewDependentOrgCountryDataObjectsToOrganisation(org1);
			OrgCountryData[] originalCountryDataObjects = Factory.Load<OrgCountryData>(new ZQuery(OrgCountryDataSchema.OV_OH_OrgHeader, org1.PK));
			AssertEquals("Original Country Data Records", 2, originalCountryDataObjects.Length);

			// Create XsdOrgCountryDataCollection and Export to it
			Xsd.SysMergeOrgCountryDataCollection xsdOrgCountryDataCollection = new Xsd.SysMergeOrgCountryDataCollection();
			testHelper.ExportToValueObjectCollection(org1, xsdOrgCountryDataCollection, notifications);
			AssertEquals("XSD Country Data Records", 2, xsdOrgCountryDataCollection.Count);
			// Assert XsdOrgCountryDataCollection Details
			AssertEquals("XSD Country Data 0 - Org FK", org1.PK.ToString(), xsdOrgCountryDataCollection[0].OH_OrgHeader_PK);
			AssertEquals("XSD Country Data 0 - Country NK", "SG", xsdOrgCountryDataCollection[0].RN_ClientCountryRelationship_NK);
			AssertEquals("XSD Country Data 0 - Location Empty", false, xsdOrgCountryDataCollection[0].OA_ApprovedLocation_PK.IsEmpty);
			AssertEquals("XSD Country Data 0 - ImportEntryPaymentPreference", "ABC", xsdOrgCountryDataCollection[0].ImportEntryPaymentPreference);
			AssertEquals("XSD Country Data 0 - SystemLastEditTime HasValue", true, xsdOrgCountryDataCollection[0].SystemLastEditTime.HasValue);
			AssertEquals("XSD Country Data 1 - Org FK", org1.PK.ToString(), xsdOrgCountryDataCollection[1].OH_OrgHeader_PK);
			AssertEquals("XSD Country Data 1 - Country NK", "NZ", xsdOrgCountryDataCollection[1].RN_ClientCountryRelationship_NK);
			AssertEquals("XSD Country Data 1 - Location Empty", true, xsdOrgCountryDataCollection[1].OA_ApprovedLocation_PK.IsEmpty);
			AssertEquals("XSD Country Data 1 - ImportEntryPaymentPreference", "XYZ", xsdOrgCountryDataCollection[1].ImportEntryPaymentPreference);
			AssertEquals("XSD Country Data 1 - SystemLastEditTime HasValue", false, xsdOrgCountryDataCollection[1].SystemLastEditTime.HasValue);

			// Create New Org in a separate factory
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeaderForDataTransfer newOrg = newFactory.New<OrgHeaderForDataTransfer>();

			OrgCountryData[] newCountryDataArray = newOrg.Factory.Load<OrgCountryData>(new ZQuery(OrgCountryDataSchema.OV_OH_OrgHeader, newOrg.PK));
			AssertEquals("New Country Data Records - Before Import", 0, newCountryDataArray.Length);

			// Import exported OrgCountryDataXsdCollection to newOrg
			IValueObjectImportContext testContext = new ValueObjectImportContext(newOrg.Factory, new DataTransfer.Xml.XsdVersion1.XmlInterchange(), new SysMergeOrganisationMatching(), notifications);
			testHelper.ImportFromValueObjectCollection(xsdOrgCountryDataCollection, newOrg, testContext);
			newCountryDataArray = newOrg.Factory.Load<OrgCountryData>(new ZQuery(OrgCountryDataSchema.OV_OH_OrgHeader, newOrg.PK));
			AssertEquals("New Country Data Records - After Import", 2, newCountryDataArray.Length);
			// Assert OrgCoutryDataCollection Details
			AssertEquals("New Country Data 0 - Org FK", newOrg.PK, newCountryDataArray[0].OV_OH_OrgHeader);
			AssertEquals("New Country Data 0 - Country NK", "SG", newCountryDataArray[0].OV_RN_NKClientCountryRelation);
			AssertEquals("New Country Data 0 - Location Empty", false, newCountryDataArray[0].OV_OA_ApprovedLocation.IsEmpty);
			AssertEquals("New Country Data 0 - ImportEntryPaymentPreference", "ABC", newCountryDataArray[0].OV_ImportEntryPaymentPreference);
			AssertEquals("New Country Data 0 - SystemLastEditTime Valid", true, newCountryDataArray[0].OV_SystemLastEditTimeUtc.IsValid);
			AssertEquals("New Country Data 1 - Org FK", newOrg.PK, newCountryDataArray[1].OV_OH_OrgHeader);
			AssertEquals("New Country Data 1 - Country NK", "NZ", newCountryDataArray[1].OV_RN_NKClientCountryRelation);
			AssertEquals("New Country Data 1 - Location Empty", true, newCountryDataArray[1].OV_OA_ApprovedLocation.IsEmpty);
			AssertEquals("New Country Data 1 - ImportEntryPaymentPreference", "XYZ", newCountryDataArray[1].OV_ImportEntryPaymentPreference);
			AssertEquals("New Country Data 1 - SystemLastEditTime Valid", false, newCountryDataArray[1].OV_SystemLastEditTimeUtc.IsValid);
		}

		void AddNewDependentOrgCountryDataObjectsToOrganisation(OrgHeaderForDataTransfer org)
		{
			ZDateTime now = ZDateTime.Now;

			OrgCountryData countryData1 = org.Factory.New<OrgCountryData>();
			countryData1.OV_OH_OrgHeader = org.PK;
			countryData1.OV_RN_NKClientCountryRelation = "SG";
			countryData1.OV_OA_ApprovedLocation = ZGuid.NewZGuid();
			countryData1.OV_ImportEntryPaymentPreference = "ABC";
			countryData1.OV_ImportQuarantinePaymentPreference = "DEF";
			countryData1.OV_EXApprovedOrMajorExporter = "GHI";
			countryData1.OV_EXApprovalMethod = "JKL";
			countryData1.OV_EXApprovalNumber = "123456789012345";
			countryData1.OV_EXPermitNumber = "12345678901234567890123456789012345678901234567890";
			countryData1.OV_EXExportPermissionDetails = "VC250";
			countryData1.OV_CustomsEconomicGroupAddInfo = "VC1024";
			countryData1.OV_ImportCustomsDefaultAddInfo = "VC1024";
			countryData1.OV_GS_NKReviewedByUser = GlbStaff.CurrentUser.GS_Code;
			countryData1.OV_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			countryData1.OV_SystemLastEditUser = GlbStaff.CurrentUser.GS_Code;
			countryData1.OV_EXE3Signed = ZBool.True;
			countryData1.OV_MakePartsBothImportAndExport = ZBool.True;
			countryData1.OV_EXSiteInspectionDate = now;
			countryData1.OV_LastReviewedOn = now;
			countryData1.OV_SystemCreateTimeUtc = now;
			countryData1.OV_SystemLastEditTimeUtc = now;

			OrgCountryData countryData2 = org.Factory.New<OrgCountryData>();
			countryData2.OV_OH_OrgHeader = org.PK;
			countryData2.OV_RN_NKClientCountryRelation = "NZ";
			countryData2.OV_ImportEntryPaymentPreference = "XYZ";
			countryData2.OV_EXApprovalMethod = "123";
			countryData2.OV_EXApprovalNumber = "VC15";
			countryData2.OV_EXPermitNumber = "VC50";
			countryData2.OV_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			countryData2.OV_EXE3Signed = ZBool.False;
			countryData1.OV_EXSiteInspectionDate = now;
			countryData1.OV_SystemCreateTimeUtc = now;
		}
	}
}
