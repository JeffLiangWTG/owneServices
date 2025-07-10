using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicPayment.EPaymentStaffToken.Testing
{
	public class EPaymentStaffTokenConfigurationHandlerTest : TestCaseWithFactory
	{
		public void TestProcessInterchangeSucceed_UpdateExistingTokenExpiryAndStatusInDB()
		{
			var ofxLoginUserId = "FFFCF8E8-2C12-4FB3-8D86-ABDD580616E4";

			var tokenExpiry = ZDateTime.Now.ToSmallDateTime();
			var configMsg = string.Format(configurationMessageTemplate,
				GlbCompany.CurrentCompany.GC_Code,
				TestObjectCreator.GS1.GS_Code,
				"Payments",
				tokenExpiry,
				TestObjectCreator.AUDBankAccount.AB_Code,
				ofxLoginUserId,
				string.Empty);
			SetupInterchange(configMsg, 1);

			var originalToken = Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, TestObjectCreator.AUDBankAccount.PK));
			AssertEquals("Precondition", ZDateTime.Empty, originalToken.TK_ExpiryUtc);
			AssertEquals("Precondition", ZString.Empty, originalToken.TK_AccountName);
			AssertEquals("Precondition", AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised, originalToken.TK_Status);

			var logger = new LoggingInformation();
			var processor = new EPaymentStaffTokenProcessor(logger);
			processor.ExecuteBatch();

			var updatedToken = Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, TestObjectCreator.AUDBankAccount.PK));
			AssertEquals("Postcondition", tokenExpiry, updatedToken.TK_ExpiryUtc);
			AssertEquals("Postcondition", ofxLoginUserId, updatedToken.TK_AccountName);
			AssertEquals("Postcondition", AccEPaymentStaffTokenLookups.StatusCodes.Authorised, updatedToken.TK_Status);
		}

		public void TestProcessInterchangeWithError_UpdateTokenStatusAndErrorInDB()
		{
			var ofxLoginUserId = "FFFCF8E8-2C12-4FB3-8D86-ABDD580616E4";

			var tokenExpiry = ZDateTime.Now.ToSmallDateTime();
			var configMsg = string.Format(configurationMessageTemplate,
				GlbCompany.CurrentCompany.GC_Code,
				TestObjectCreator.GS1.GS_Code,
				"Payments",
				tokenExpiry,
				TestObjectCreator.AUDBankAccount.AB_Code,
				ofxLoginUserId,
				"Some errors occurred on xHub");
			SetupInterchange(configMsg, 1);

			var originalToken = Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, TestObjectCreator.AUDBankAccount.PK));
			AssertEquals("Precondition", ZDateTime.Empty, originalToken.TK_ExpiryUtc);
			AssertEquals("Precondition", ZString.Empty, originalToken.TK_AccountName);
			AssertEquals("Precondition", AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised, originalToken.TK_Status);

			var logger = new LoggingInformation();
			var processor = new EPaymentStaffTokenProcessor(logger);
			processor.ExecuteBatch();

			var updatedToken = Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, TestObjectCreator.AUDBankAccount.PK));
			AssertEquals("Postcondition", ZDateTime.Empty, updatedToken.TK_ExpiryUtc);
			AssertEquals("Postcondition", ZString.Empty, updatedToken.TK_AccountName);
			AssertEquals("Postcondition", "Some errors occurred on xHub", updatedToken.TK_ErrorDescription);
			AssertEquals("Postcondition", AccEPaymentStaffTokenLookups.StatusCodes.Error, updatedToken.TK_Status);
		}

		public void TestProcessInterchangeError_TokenExpiryEarlierThanRequestedUtc()
		{
			var ofxLoginUserId = "FFFCF8E8-2C12-4FB3-8D86-ABDD580616E4";
			var tokenExpiry = ZDateTime.Now.ToSmallDateTime().AddHours(-1);
			var configMsg = string.Format(configurationMessageTemplate,
				GlbCompany.CurrentCompany.GC_Code,
				TestObjectCreator.GS1.GS_Code,
				"Payments",
				tokenExpiry,
				TestObjectCreator.AUDBankAccount.AB_Code,
				ofxLoginUserId,
				string.Empty);
			SetupInterchange(configMsg, 1);

			var originalToken = Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, TestObjectCreator.AUDBankAccount.PK));
			AssertEquals("Precondition", ZDateTime.Empty, originalToken.TK_ExpiryUtc);
			AssertEquals("Precondition", ZString.Empty, originalToken.TK_AccountName);
			AssertEquals("Precondition", AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised, originalToken.TK_Status);
			Assert("expirty is earlier than requested UTC", tokenExpiry < originalToken.TK_RequestedUtc);

			var logger = new LoggingInformation();
			var processor = new EPaymentStaffTokenProcessor(logger);
			processor.ExecuteBatch();

			var updatedToken = Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, TestObjectCreator.AUDBankAccount.PK));
			AssertEquals("Postcondition", ZDateTime.Empty, updatedToken.TK_ExpiryUtc);
			AssertEquals("Postcondition", ZString.Empty, updatedToken.TK_AccountName);
			AssertEquals("Postcondition", AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised, updatedToken.TK_Status);

			AssertEquals("Token's expiry is earlier than the requested date/time.", logger.Logs.First().Message);
		}

		public void TestProcessInterchangeError_WasAuthorisedWithDifferentProviderAccount()
		{
			var ofxLoginUserId = "FFFCF8E8-2C12-4FB3-8D86-ABDD580616E4";
			var tokenExpiry = ZDateTime.Now.ToSmallDateTime();
			var configMsg = string.Format(configurationMessageTemplate,
				GlbCompany.CurrentCompany.GC_Code,
				TestObjectCreator.GS1.GS_Code,
				"Payments",
				tokenExpiry,
				TestObjectCreator.AUDBankAccount.AB_Code,
				ofxLoginUserId,
				string.Empty);
			SetupInterchange(configMsg, 1);
			//simulate the case that a different user was logged in before
			var oldUserId = "3387D429-2096-4B82-BF0D-ED5AA7ECEBFE";
			staffToken.TK_AccountName = oldUserId;
			staffToken.TK_Status = AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised;
			Factory.Save();

			var originalToken = Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, TestObjectCreator.AUDBankAccount.PK));
			AssertEquals("Precondition", ZDateTime.Empty, originalToken.TK_ExpiryUtc);
			AssertEquals("Precondition", oldUserId, originalToken.TK_AccountName);
			AssertEquals("Precondition", AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised, originalToken.TK_Status);

			var logger = new LoggingInformation();
			var processor = new EPaymentStaffTokenProcessor(logger);
			processor.ExecuteBatch();

			var updatedToken = Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, TestObjectCreator.AUDBankAccount.PK));
			AssertEquals("Postcondition", ZDateTime.Empty, originalToken.TK_ExpiryUtc);
			AssertEquals("Postcondition", oldUserId, originalToken.TK_AccountName);
			AssertEquals("Postcondition", AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised, originalToken.TK_Status);

			AssertEquals("Provider authorization was done with a different user account before.", logger.Logs.First().Message);
		}

		public void TestProcessInterchangeError_NotMatchedRecordsFound()
		{
			var ofxLoginUserId = "FFFCF8E8-2C12-4FB3-8D86-ABDD580616E4";
			var tokenExpiry = ZDateTime.Now.ToSmallDateTime();
			var configMsg = string.Format(configurationMessageTemplate,
				GlbCompany.CurrentCompany.GC_Code,
				TestObjectCreator.GS1.GS_Code,
				"Rates",
				tokenExpiry,
				TestObjectCreator.AUDBankAccount.AB_Code,
				ofxLoginUserId,
				string.Empty);
			SetupInterchange(configMsg, 1);
			Factory.Save();

			var originalToken = Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, TestObjectCreator.AUDBankAccount.PK));
			AssertEquals("Precondition", ZDateTime.Empty, originalToken.TK_ExpiryUtc);
			AssertEquals("Precondition", ZString.Empty, originalToken.TK_AccountName);
			AssertEquals("Precondition", AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised, originalToken.TK_Status);

			var logger = new LoggingInformation();
			var processor = new EPaymentStaffTokenProcessor(logger);
			processor.ExecuteBatch();

			var updatedToken = Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, TestObjectCreator.AUDBankAccount.PK));
			AssertEquals("Postcondition", ZDateTime.Empty, originalToken.TK_ExpiryUtc);
			AssertEquals("Postcondition", ZString.Empty, originalToken.TK_AccountName);
			AssertEquals("Postcondition", AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised, originalToken.TK_Status);

			AssertEquals("Found 0 matched staff token record(s), however it must only be 1.", logger.Logs.First().Message);
		}

		public void TestProcessInterchangeError_CompanyCodeMissingInMessage()
		{
			AssertProcessInterchangeError_FieldsMissingInMessage(isCompanyCodeMissing: true);
		}

		public void TestProcessInterchangeError_StaffCodeMissingInMessage()
		{
			AssertProcessInterchangeError_FieldsMissingInMessage(isStaffCodeMissing: true);
		}

		public void TestProcessInterchangeError_ScopeMissingInMessage()
		{
			AssertProcessInterchangeError_FieldsMissingInMessage(isScopeMissing: true);
		}

		public void TestProcessInterchangeError_OfxAccountNameMissingInMessage()
		{
			AssertProcessInterchangeError_FieldsMissingInMessage(isOfxAccountNameMissing: true);
		}

		public void TestProcessInterchangeError_TokenExpiryMissingInMessage()
		{
			AssertProcessInterchangeError_FieldsMissingInMessage(isTokenExpiryMissing: true);
		}

		public void TestProcessInterchangeError_BankAccountMissingInMessage()
		{
			AssertProcessInterchangeError_FieldsMissingInMessage(isBankAccountMissing: true);
		}

		public void TestProcessInterchangeError_FailedToFindCompanyCode()
		{
			AssertProcessInterchangeError_FieldsMissingInMessage(isCompanyCodeInvaid: true);
		}

		public void TestProcessInterchangeError_FailedToFindBankAccount()
		{
			AssertProcessInterchangeError_FieldsMissingInMessage(isBankAccountInvalid: true);
		}

		public void AssertProcessInterchangeError_FieldsMissingInMessage(bool isCompanyCodeMissing = false, bool isStaffCodeMissing = false,
																		bool isScopeMissing = false, bool isOfxAccountNameMissing = false,
																		bool isTokenExpiryMissing = false, bool isBankAccountMissing = false,
																		bool isCompanyCodeInvaid = false, bool isBankAccountInvalid = false)
		{
			var ofxLoginUserId = "FFFCF8E8-2C12-4FB3-8D86-ABDD580616E4";
			var tokenExpiry = ZDateTime.Now.ToSmallDateTime();
			var configMsg = string.Format(configurationMessageTemplate,
				isCompanyCodeMissing ? ZString.Empty : (isCompanyCodeInvaid ? new ZString("XXX") : GlbCompany.CurrentCompany.GC_Code),
				isStaffCodeMissing ? ZString.Empty : TestObjectCreator.GS1.GS_Code,
				isScopeMissing ? string.Empty : "Payments",
				isTokenExpiryMissing ? string.Empty : tokenExpiry.ToString(),
				isBankAccountMissing ? ZString.Empty : (isBankAccountInvalid ? new ZString("ZZZ") : TestObjectCreator.AUDBankAccount.AB_Code),
				isOfxAccountNameMissing ? string.Empty : ofxLoginUserId,
				string.Empty);
			SetupInterchange(configMsg, 1);

			var originalToken = Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, TestObjectCreator.AUDBankAccount.PK));
			AssertEquals("Precondition", ZDateTime.Empty, originalToken.TK_ExpiryUtc);
			AssertEquals("Precondition", ZString.Empty, originalToken.TK_AccountName);
			AssertEquals("Precondition", AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised, originalToken.TK_Status);

			var logger = new LoggingInformation();
			var processor = new EPaymentStaffTokenProcessor(logger);
			processor.ExecuteBatch();

			var updatedToken = Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_AB, TestObjectCreator.AUDBankAccount.PK));
			AssertEquals("Postcondition", ZDateTime.Empty, updatedToken.TK_ExpiryUtc);
			AssertEquals("Postcondition", ZString.Empty, updatedToken.TK_AccountName);
			AssertEquals("Postcondition", AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised, updatedToken.TK_Status);

			if (isCompanyCodeMissing)
			{
				AssertEquals("Company code is missing.", logger.Logs.First().Message);
			}
			else if (isStaffCodeMissing)
			{
				AssertEquals("Staff code is missing.", logger.Logs.First().Message);
			}
			else if (isScopeMissing)
			{
				AssertEquals("Token scope is missing.", logger.Logs.First().Message);
			}
			else if (isTokenExpiryMissing)
			{
				AssertEquals("Refresh token expiry information is missing.", logger.Logs.First().Message);
			}
			else if (isBankAccountMissing)
			{
				AssertEquals("Bank account information is missing.", logger.Logs.First().Message);
			}
			else if (isOfxAccountNameMissing)
			{
				AssertEquals("Payment provider account name is missing.", logger.Logs.First().Message);
			}
			else if (isCompanyCodeInvaid)
			{
				AssertEquals("Failed to find company 'XXX'.", logger.Logs.First().Message);
			}
			else if (isBankAccountInvalid)
			{
				AssertEquals("Failed to find bank account 'ZZZ'.", logger.Logs.First().Message);
			}
			else
			{
				Fail("Invaid test case");
			}
		}

		void SetupInterchange(string configurationMessage, int interchangeNo)
		{
			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.OFX;
			incomingInterchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.Configuration;
			incomingInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			incomingInterchange.EI_From = "From";
			incomingInterchange.EI_To = "To";
			incomingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			incomingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			incomingInterchange.EI_BodyText = configurationMessage;
			incomingInterchange.EI_InterchangeNum = interchangeNo.ToString();
			Factory.Save();
		}

		readonly string configurationMessageTemplate = @"<Configuration Name=""EPaymentStaffTokenConfiguration"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
	<Group Type=""System"" Reference=""WTLTST"">
		<Group Type=""Company"" Reference=""{0}"">
			<Group Type=""Staff"" Reference=""{1}"">
				<Item Name=""Provider"">OFX</Item>
				<Item Name=""Scope"">{2}</Item>
				<Item Name=""BankAccount"">{4}</Item>
				<Item Name=""FirstName"">John</Item>
				<Item Name=""MiddleName""></Item>
				<Item Name=""LastName"">Smith</Item>
				<Item Name=""RefreshToken""></Item>
				<Item Name=""RefreshExpiry"">{3}</Item>
				<Item Name=""ErrorDescription"">{6}</Item>
				<Credential>
					<UserName>{5}</UserName>
				</Credential>
			</Group>
		</Group>
	</Group>
</Configuration>";

		TestObjectCreator TestObjectCreator;
		AccEPaymentStaffToken staffToken;

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
			InitializeStaffTokenInDB();
		}

		void InitializeStaffTokenInDB()
		{
			staffToken = Factory.New<AccEPaymentStaffToken>();
			staffToken.TK_GS_NKStaffCode = TestObjectCreator.GS1.GS_Code;
			staffToken.TK_AB = TestObjectCreator.AUDBankAccount.PK;
			staffToken.TK_Scope = "Payments";
			staffToken.TK_ExpiryUtc = ZDate.Empty;
			staffToken.TK_Status = AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised;
			staffToken.TK_RequestedUtc = ZDateTime.Now.AddMinutes(-10);
			staffToken.TK_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
		}
	}
}
