using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class GBCustomsCDSXmlCredentialConfigurationHandlerTest : Customs.Business.XmlCredential.Testing.ConfigurationHandlerTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "GB";
			company.GC_Code = "CUK";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "CUK";
			branch.GB_RL_NKHomePort = "GBLHR";
			branch.GB_BranchName = "Chris";
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Code = "CUK";
			branch.GB_OH_OrgProxy = orgProxy.PK;
			Factory.Save();
		}

		[TestDate(2019, 7, 8)]
		public void TestProcess()
		{
			TestProcessForInterchangeType(EDIInterchangeTypeList.Codes.EHubRegistryUpdate);
			TestProcessForInterchangeType(EDIInterchangeTypeList.Codes.Configuration);
		}

		void TestProcessForInterchangeType(string interchangeType)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_Status = EDIInterchange.Status.eHubQueued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.eHub;
			interchange.EI_IsActive = ZBool.True;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_BodyText = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""GBCustomsCDS"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
<Group Type = ""System"" Reference = ""HYECMT"">
		  <Group Type = ""CDS"" Status = ""INV"">
				 <Annotations>	  
					  <Item Name = ""TokenType"">Authorisation</Item>	   
					  <Item Name = ""Message"">Authorisation token received OK</Item>			
					  <Item Name = ""Issued"">2018-01-09T09:56:00Z</Item>					  
					   <Item Name = ""Expires"">2019-07-09T09:14:23Z</Item>								
				 </Annotations>								
				 <Item Name = ""MailBoxID"">GB123456789000</Item>								 
				 <Credential Name = ""Current"">								  
					<UserName>ABC</UserName>								  
				 </Credential>								  
		  </Group>								  
</Group>
</Configuration>";

			Factory.Save();

			Configuration configuration;
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				configuration = reader.DeserializeToConfiguration();
			}

			var logger = new LoggingInformation();
			var handler = new GBCustomsCDSXmlCredentialConfigurationHandler(logger);
			handler.Process(configuration);

			var query = new ZQuery(GlbExternalPasswordSchema.GP_UserID, SQLComparisonOperator.EndsWith, ".ABC");
			var password = Factory.LoadTop1<GlbExternalPassword_GB>(query);
			AssertNotNull(password);
			AssertEquals("password.GP_MailBoxID", "GB123456789000", password.EORI);
			AssertEquals("password.GP_UserID", "ABC", password.Badge);
			AssertEquals("password.GP_IssueDate", new ZDateTime(2018, 1, 9, 9, 56, 0), password.GP_IssueDate);
			AssertEquals("password.GP_ExpiryDate", new ZDateTime(2019, 7, 9, 9, 14, 23), password.GP_ExpiryDate);
			AssertEquals("password.GP_StatusReason", "Authorisation Token - Authorisation token received OK", password.StatusMessage);
			AssertEquals("password.GP_PasswordStatus", PasswordStatusList.Codes.Invalid, password.Status);
			AssertEquals("log", "", GetUserLogStrings(logger.UserLogStrings));

			interchange.EI_BodyText = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""GBCustomsCDS"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
<Group Type = ""System"" Reference = ""HYECMT"">
		  <Group Type = ""CDS"" Status = ""VAL"">
				 <Annotations>	  
					  <Item Name = ""TokenType"">Access</Item>	   
					  <Item Name = ""Message"">Access token received OK</Item>			
					  <Item Name = ""Issued"">2018-01-09T09:56:03Z</Item>					  
					   <Item Name = ""Expires"">2019-07-09T13:14:23Z</Item>								
				 </Annotations>								
				 <Item Name = ""MailBoxID"">GB123456789000</Item>								 
				 <Credential Name = ""Current"">								  
					<UserName>ABC</UserName>								  
				 </Credential>								  
		  </Group>								  
</Group>
</Configuration>";

			using (var reader = interchange.GetEI_BodyTextReader())
			{
				configuration = reader.DeserializeToConfiguration();
			}

			logger = new LoggingInformation();
			handler = new GBCustomsCDSXmlCredentialConfigurationHandler(logger);
			handler.Process(configuration);

			password = Factory.LoadTop1<GlbExternalPassword_GB>(query);

			AssertNotNull(password);
			AssertEquals("password.GP_IssueDate", new ZDateTime(2018, 1, 9, 9, 56, 3), password.GP_IssueDate);
			AssertEquals("password.GP_ExpiryDate", new ZDateTime(2019, 7, 9, 13, 14, 23), password.GP_ExpiryDate);
			AssertEquals("password.StatusMessage", "Access Token - Access token received OK", password.StatusMessage);
			AssertEquals("password.Status", PasswordStatusList.Codes.Valid, password.Status);
			AssertEquals("log", "", GetUserLogStrings(logger.UserLogStrings));

			interchange.EI_BodyText = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""GBCustomsCDS"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
<Group Type = ""System"" Reference = ""HYECMT"">
		  <Group Type = ""CDS"" Status = ""VAL"">
				 <Annotations>	  
					  <Item Name = ""TokenType"">Access</Item>	   
					  <Item Name = ""Message"">Access token refreshed OK</Item>			
					  <Item Name = ""Issued"">2018-01-09T09:56:03Z</Item>					  
					   <Item Name = ""Expires"">2019-07-09T17:14:23Z</Item>								
				 </Annotations>								
				 <Item Name = ""MailBoxID"">GB123456789000</Item>								 
				 <Credential Name = ""Current"">								  
					<UserName>ABC</UserName>								  
				 </Credential>								  
		  </Group>								  
</Group>
</Configuration>";

			using (var reader = interchange.GetEI_BodyTextReader())
			{
				configuration = reader.DeserializeToConfiguration();
			}

			logger = new LoggingInformation();
			handler = new GBCustomsCDSXmlCredentialConfigurationHandler(logger);
			handler.Process(configuration);

			password = Factory.LoadTop1<GlbExternalPassword_GB>(query);

			AssertNotNull(password);
			AssertEquals("password.GP_IssueDate", new ZDateTime(2018, 1, 9, 9, 56, 3), password.GP_IssueDate);
			AssertEquals("password.GP_ExpiryDate", new ZDateTime(2019, 7, 9, 17, 14, 23), password.GP_ExpiryDate);
			AssertEquals("password.StatusMessage", "Access Token - Access token refreshed OK", password.StatusMessage);
			AssertEquals("password.Status", PasswordStatusList.Codes.Valid, password.Status);
			AssertEquals("log", "", GetUserLogStrings(logger.UserLogStrings));

			interchange.EI_BodyText = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""GBCustomsCDS"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
<Group Type = ""System"" Reference = ""HYECMT"">
		  <Group Type = ""CDS"" Status = ""INA"">
				 <Annotations>	  
					  <Item Name = ""TokenType"">Authorisation</Item>	   
					  <Item Name = ""Message"">New authorisation token received</Item>			
					  <Item Name = ""Issued"">2019-06-09T09:56:03Z</Item>					  
					   <Item Name = ""Expires"">2021-01-09T09:56:00Z</Item>								
				 </Annotations>								
				 <Item Name = ""MailBoxID"">GB123456789000</Item>								 
				 <Credential Name = ""Current"">								  
					<UserName>ABC</UserName>								  
				 </Credential>								  
		  </Group>								  
</Group>
</Configuration>";

			using (var reader = interchange.GetEI_BodyTextReader())
			{
				configuration = reader.DeserializeToConfiguration();
			}

			logger = new LoggingInformation();
			handler = new GBCustomsCDSXmlCredentialConfigurationHandler(logger);
			handler.Process(configuration);

			password = Factory.LoadTop1<GlbExternalPassword_GB>(query);

			AssertNotNull(password);
			AssertEquals("password.GP_IssueDate", new ZDateTime(2019, 6, 9, 9, 56, 3), password.GP_IssueDate);
			AssertEquals("password.GP_ExpiryDate", new ZDateTime(2021, 1, 9, 9, 56, 00), password.GP_ExpiryDate);
			AssertEquals("password.StatusMessage", "Authorisation Token - New authorisation token received", password.StatusMessage);
			AssertEquals("password.Status", PasswordStatusList.Codes.Deactivated, password.Status);
			AssertEquals("log", "", GetUserLogStrings(logger.UserLogStrings));

			interchange.EI_BodyText = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""GBCustomsCDS"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
<Group Type = ""System"" Reference = ""HYECMT"">
		  <Group Type = ""CDS"" Status = ""INV"">
				 <Annotations>	  
					  <Item Name = ""TokenType"">Access</Item>	   
					  <Item Name = ""Message"">Could not obtain access token, HMRC said: Unauthorized (401)</Item>							
				 </Annotations>								
				 <Item Name = ""MailBoxID"">GB123456789000</Item>								 
				 <Credential Name = ""Current"">								  
					<UserName>ABC</UserName>								  
				 </Credential>								  
		  </Group>								  
</Group>
</Configuration>";

			using (var reader = interchange.GetEI_BodyTextReader())
			{
				configuration = reader.DeserializeToConfiguration();
			}

			logger = new LoggingInformation();
			handler = new GBCustomsCDSXmlCredentialConfigurationHandler(logger);
			handler.Process(configuration);

			password = Factory.LoadTop1<GlbExternalPassword_GB>(query);

			AssertNotNull(password);
			AssertEquals("password.GP_IssueDate", new ZDateTime(2019, 6, 9, 9, 56, 3), password.GP_IssueDate);
			AssertEquals("password.GP_ExpiryDate", new ZDateTime(2021, 1, 9, 9, 56, 00), password.GP_ExpiryDate);
			AssertEquals("password.StatusMessage", (ZString)@"Access Token - Could not obtain access token, HMRC said: Unauthorized (401)", password.StatusMessage);
			AssertEquals("password.Status", PasswordStatusList.Codes.Invalid, password.Status);
			AssertEquals("log", "", GetUserLogStrings(logger.UserLogStrings));
		}

		[ExpectNoExceptions]
		public void TestMessageLongerThanGP_StatusReasonMaxLength()
		{
			Configuration configuration;
			var logger = new LoggingInformation();

			var msgField = "This is supposed to be a really long error".PadRight(GlbExternalPassword.Schema.GP_StatusReasonMaxLength, 'X');
			var xmlContent = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""GBCustomsCDS"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
<Group Type = ""System"" Reference = ""HYECMT"">
		  <Group Type = ""CDS"" Status = ""INV"">
				 <Annotations>	  
					  <Item Name = ""TokenType"">Access</Item>	   
					  <Item Name = ""Message"">{0}</Item>							
				 </Annotations>								
				 <Item Name = ""MailBoxID"">GB123456789000</Item>								 
				 <Credential Name = ""Current"">								  
					<UserName>ABC</UserName>								  
				 </Credential>								  
		  </Group>								  
</Group>
</Configuration>";

			var msg = string.Format(xmlContent, msgField);

			using (var reader = new System.IO.StringReader(msg))
			{
				configuration = reader.DeserializeToConfiguration();
			}

			var handler = new GBCustomsCDSXmlCredentialConfigurationHandler(logger);
			handler.Process(configuration);

			var query = new ZQuery(GlbExternalPasswordSchema.GP_UserID, SQLComparisonOperator.EndsWith, ".ABC");
			var password = Factory.LoadTop1<GlbExternalPassword_GB>(query);

			AssertNotNull(password);
			AssertEquals("Length limit", GlbExternalPassword.Schema.GP_StatusReasonMaxLength, password.StatusMessage.Length);
			AssertContains("Check content", @"Access Token - This is supposed to be a really long errorXXX", password.StatusMessage);
		}
	}
}
