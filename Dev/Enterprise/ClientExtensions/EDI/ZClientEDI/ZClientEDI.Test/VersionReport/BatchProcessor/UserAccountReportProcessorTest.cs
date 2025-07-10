using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.UserAccountReporting.BatchProcessor;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor.Testing
{
	class UserAccountReportProcessorTest : TestCaseWithFactory
	{
		[TestDate(2023, 9, 20)]
		public void TestProcess()
		{
			SetupTestData();
			var org = Database.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();
			Database.LD_OH_WebAccessOrg = org.PK;
			Factory.Save();
			var logger = new TestServiceLogger();
			var processor = new UserAccountReportProcessor(logger);
			const string xmlTemplate = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UserAccountReport>
  <DatabaseNumber>98765</DatabaseNumber>
  <BranchList>
    <Branch>
      <IsActive>1</IsActive>
      <PK>{PK}</PK>
      <Code>7VQ</Code>
      <BranchName>Test Branch</BranchName>
      <CompanyName>Eagle Datamation International</CompanyName>
      <CompanyCode>EDI</CompanyCode>
      <Address1>Address 1</Address1>
      <Address2>Address 2</Address2>
      <City>City 1</City>
      <State>State 1</State>
      <PostCode>002068</PostCode>
      <CountryCode>AU</CountryCode>
      <Unloco />
      <ValidationStatus>NYV</ValidationStatus>
    </Branch>
  </BranchList>
  <StaffList>
    <IsFullStaffList>1</IsFullStaffList>
    <Staff>
      <IsActive>1</IsActive>
      <Code>GS1</Code>
      <Name>staff1</Name>
      <Email>staff1@test.com</Email>
      <Language>EN</Language>
      <Branch>DEM</Branch>
      <WorkPhone>111111</WorkPhone>
      <Extension>xxx</Extension>
      <JobTitle>boss</JobTitle>
    </Staff>
    <Staff>
      <IsActive>1</IsActive>
      <Code>GS2</Code>
      <Name>staff2</Name>
      <Email>staff2@test.com</Email>
      <Language>EN</Language>
      <Branch>DEM</Branch>
      <WorkPhone>222222</WorkPhone>
      <Extension>yyy</Extension>
      <JobTitle>master</JobTitle>
    </Staff>
  </StaffList>
</UserAccountReport>";
			var reportXml = xmlTemplate.Replace("{PK}", ZGuid.NewZGuid().ToString());
			processor.Process(reportXml);
			var clientStaffList = Factory.Load<ClientStaff>(new ZQuery());
			var clientBranchList = Factory.Load<ClientBranch>(new ZQuery());
			var testStaff1 = clientStaffList.Where(x => x.LS_Code == "GS1").First();
			var testStaff2 = clientStaffList.Where(x => x.LS_Code == "GS2").First();
			var testBranch = clientBranchList.Where(x => x.LCB_Code == "7VQ").First();
			AssertEquals("The count of clientStaffList should be 2", 2, clientStaffList.Length);
			AssertEquals("clientStaffList should contain GS1", "GS1", testStaff1.LS_Code);
			AssertEquals("clientStaffList should contain GS2", "GS2", testStaff2.LS_Code);
			AssertEquals("The count of clientBranchList should be 1", 1, clientBranchList.Length);
			AssertEquals("clientBranchList should contain 7VQ", "7VQ", testBranch.LCB_Code);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals(1, logger.Count);
			AssertEquals("Information|Processing user account report from database 98765 complete", logger[0]);

			const string xmlTemplate2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UserAccountReport>
  <DatabaseNumber>98765</DatabaseNumber>
  <StaffList>
    <IsFullStaffList>1</IsFullStaffList>
    <Staff>
      <IsActive>1</IsActive>
      <Code>gs2</Code>
      <Name>staff2</Name>
      <Email>staff2new@test.com</Email>
      <Language>EN</Language>
      <Branch>DEM</Branch>
      <WorkPhone>222222</WorkPhone>
      <Extension>yyy</Extension>
      <JobTitle>master</JobTitle>
    </Staff>
    <Staff>
      <IsActive>1</IsActive>
      <Code>GS3</Code>
      <Name>staff3</Name>
      <Email>staff3@test.com</Email>
      <Language>EN</Language>
      <Branch>DEM</Branch>
      <WorkPhone>333333</WorkPhone>
      <Extension>yyy</Extension>
      <JobTitle>ceo</JobTitle>
    </Staff>
  </StaffList>
</UserAccountReport>";
			processor.Process(xmlTemplate2);
			var reloadFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			clientStaffList = reloadFactory.Load<ClientStaff>(new ZQuery());
			testStaff2 = clientStaffList.Where(x => x.LS_Code == "gs2").First();
			var testStaff3 = clientStaffList.Where(x => x.LS_Code == "GS3").First();
			AssertEquals("The count of clientStaffList should be 3", 3, clientStaffList.Length);
			AssertEquals("staff2's email should be updated", "staff2new@test.com", testStaff2.LS_Email);
			AssertEquals("clientStaffList should contain GS3", "GS3", testStaff3.LS_Code);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		void SetupTestData()
		{
			TestCaseHelper.ClearTable(LicenceConnectionSchema.Constants.TableName);
			TestCaseHelper.ClearTable(LicenceModulesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(LicenceHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(LicenceDatabaseSchema.Constants.TableName);
			Organisation = Factory.New<EDIOrgHeader>();
			Organisation.CreateAndLoadLicenceForOrg();
			Enterprise = Organisation.LicEnterprise;
			Company = Organisation.LicCompany;
			Database = Company.LicDatabases.AddNew();
			Database.LD_DatabaseNumber = 98765;
			Header = Company.GetHeader(Database);
			PreviousBuild = Factory.NewWithValidTestData<ReleaseBuild>();
			NewBuild = Factory.NewWithValidTestData<ReleaseBuild>();
			NextBuild = Factory.NewWithValidTestData<ReleaseBuild>();
			Organisation.OH_RL_NKClosestPort = "AUSYD";
			Organisation.OH_FullName = "ABCXYZ";
			Organisation.OH_Code = "ABCXYZ";
			Organisation.MainAddress.OA_Address1 = "Address1";
			Enterprise.LE_EnterpriseCode = "ABC";
			Company.LC_CompanyCode = "XYZ";
			Company.LC_RX_NKCurrency = "AUD";
			Database.LD_ServerCode = "123";
			Database.LD_Product = ProductTypes.Codes.Enterprise;
			Factory.Save();
		}

		protected EDIOrgHeader Organisation;
		protected LicenceEnterprise Enterprise;
		protected LicenceCompany Company;
		protected LicenceDatabase Database;
		protected LicenceHeader Header;
		protected ReleaseBuild PreviousBuild;
		protected ReleaseBuild NewBuild;
		protected ReleaseBuild NextBuild;
	}
}
