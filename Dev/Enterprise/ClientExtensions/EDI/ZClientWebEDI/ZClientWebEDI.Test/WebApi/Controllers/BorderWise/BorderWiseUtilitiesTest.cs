using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise.Testing
{
	public class BorderWiseUtilitiesTest : TestCaseWithFactory
	{
		public void TestCreateTokenCoreReturnTokenSuccess()
		{
			// Arrange
			const string tokenName = "TokenName";
			const string tokenValue = "TokenValue-1";
			// Act
			var token = BorderWiseUtilities.CreateTokenCore(tokenName, tokenValue, Factory);
			// Assert
			var tokenQuery = new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Equal, tokenName + token);
			var tokenRecord = Factory.LoadTop1<StmData>(tokenQuery);
			AssertNotNull(tokenRecord);
			var tokenDetails = tokenRecord.SD_BinaryValue.ToAscii();
			AssertEquals(tokenValue, tokenDetails);
		}

		public void TestSendEmailWithRecipientsSaveSuccess()
		{
			// Arrange and Act
			ArrangeAndActSendEmail(false, true);
			// Assert
			AssertEmailCreated(false, true);
		}

		public void TestSendEmailWithGroupsSaveSuccess()
		{
			// Arrange and Act
			ArrangeAndActSendEmail(true, true);
			// Assert
			AssertEmailCreated(true, true);
		}

		public void TestSendEmailWithRecipientWithoutSaveSuccess()
		{
			// Arrange and Act
			ArrangeAndActSendEmail(false, false);
			// Assert
			AssertEmailCreated(false, false);
		}

		public void TestSendEmailWithGroupsWithoutSaveSuccess()
		{
			// Arrange and Act
			ArrangeAndActSendEmail(true, false);
			// Assert
			AssertEmailCreated(true, false);
		}

		void ArrangeAndActSendEmail(bool isGroupTest, bool isSave)
		{
			//Arrange
			if (isGroupTest)
			{
				var group = Factory.NewWithValidTestData<GlbGroup>();
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_EmailAddress = "lola@appoo.net";
				group.Staff.Add(staff);
				Factory.Save();
				EDIDataRegistry.Instance.BorderWiseNewOrganisationEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			}

			const string fromAddress = "support@borderwise.com";
			const string body = "email body";
			const string subject = "subject";
			var groupRegistryItem = isGroupTest ? EDIDataRegistry.Instance.BorderWiseNewOrganisationEmailNotificationGroup : null;
			var toAddress = isGroupTest ? "" : "test@email.com";
			// Act
			BorderWiseUtilities.SendEmail(fromAddress, toAddress, subject, body, groupRegistryItem, isSave, Factory);
		}

		void AssertEmailCreated(bool isGroupTest, bool isSave)
		{
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("BorderWise Support", email.FromDisplayName);
			AssertEquals("support@borderwise.com", email.FromAddress);
			AssertEquals("support@borderwise.com", email.ReplyTo);
			AssertEquals("subject", email.Subject);
			AssertContains("email body", email.Body);
			var expectedRecipient = isGroupTest ? "lola@appoo.net" : "test@email.com";
			AssertEquals(expectedRecipient, email.Recipients[0].Email);
			var query = new ZQuery();
			var emailInDb = Factory.Load<MailItem>(query);
			AssertEquals(1, emailInDb.Length);
			AssertEquals(isSave, emailInDb[0].IsInDatabase);
		}
	}
}
