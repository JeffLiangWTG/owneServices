using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public static class EUICS2MessageTestHelper
	{
		public static TestEdiMessage CreateMessage(BusinessObjectFactory factory, string messageType)
		{
			var message = factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IC2;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = "MESSAGETEXT";
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageType = MessageTypes.Codes.F24;

			return message;
		}

		public static void AssertEmail(string expectingSubject, string[] expectingBodyTexts, ZString[] expectingRecipients)
		{
			var outgoingEmails = Env.OutgoingCustomsMailManager.EmailsCreated;
			Assertion.Assert("New Emails Created", outgoingEmails.Count > 0);
			var outgoingEmail = outgoingEmails.Single(x => x.Subject.StartsWith(expectingSubject));
			var body = outgoingEmail.Body;

			foreach (var expectedBodyText in expectingBodyTexts)
			{
				Assertion.AssertContains("Email body should contain", expectedBodyText, body);
			}

			Assertion.AssertContainsExactElementsInAnyOrder("Recipients", expectingRecipients, outgoingEmail.Recipients.Cast<RecipientDef>().Select(x => x.Email));
		}

		public static GlbStaff CreateStaff(BusinessObjectFactory factory, string code, string name, string email)
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_FullName = name;
			staff.GS_EmailAddress = email;
			return staff;
		}

		public static void SetManifestHeaderEntryNumberForTesting(AsycudaManifestHeader header, string entryNumberType, string number)
		{
			var entryNum = CusEntryNumber.New(header, entryNumberType, header.AMA_RN_NKCountry);
			entryNum.CE_EntryNum = number;
		}

		public static byte[] ICS2EmptyQueryResponseData
			=> new Customs.Business.Testing.TestFileReader(typeof(EUICS2MessageTestHelper)).GetEmbeddedFileData("Enterprise.Customs.EU.Manifest.ICS2.Business.Test.Message.InterchangeProcessors.TestFiles", "ICS2EmptyQueueResponse.xml");

		public static string ICS2EmptyQueryResponseDataAsString
			=> new Customs.Business.Testing.TestFileReader(typeof(EUICS2MessageTestHelper)).GetEmbeddedFileText("Enterprise.Customs.EU.Manifest.ICS2.Business.Test.Message.InterchangeProcessors.TestFiles", "ICS2EmptyQueueResponse.xml");
	}
}
