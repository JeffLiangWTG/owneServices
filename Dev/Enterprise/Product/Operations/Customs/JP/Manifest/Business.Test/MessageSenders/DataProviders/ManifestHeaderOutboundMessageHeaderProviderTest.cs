using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestHeaderOutboundMessageHeaderProvider))]
	sealed class ManifestHeaderOutboundMessageHeaderProviderTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ManifestHeaderOutboundMessageHeaderProvider(null));

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "AMA0000024";
			header.AMA_InputReference = "0020240720";

			var bill = header.Bills.AddNew();
			var sendingObject = new ManifestMessageSendingObject(bill);
			sendingObject.MessageType = "ABC01";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";

			var bthPassword = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
			bthPassword.GP_GS = staff.PK;
			bthPassword.GP_MailBoxID = "TEST1";
			bthPassword.GP_UserID = "TEST2";
			bthPassword.CurrentDecryptedPassword = "TESTPASS";
			header.AMA_GS_NKCustomsAgent = staff.GS_Code;
			header.AMA_CustomsAgentCredentialPK = bthPassword.PK;

			Factory.Save();

			var provider = new ManifestHeaderOutboundMessageHeaderProvider(sendingObject);

			CombineAssertions(() =>
			{
				AssertEquals(nameof(ManifestHeaderOutboundMessageHeaderProvider.MessageTag), EDIMessage.MessageNumberPlaceHolder, provider.MessageTag);
				AssertEquals(nameof(ManifestHeaderOutboundMessageHeaderProvider.InputReference), "0020240720", provider.InputReference);
				AssertEquals(nameof(ManifestHeaderOutboundMessageHeaderProvider.ProcedureCode), "ABC01", provider.ProcedureCode);
				AssertEquals(nameof(ManifestHeaderOutboundMessageHeaderProvider.UserCode), bthPassword.GP_MailBoxID, provider.UserCode);
				AssertEquals(nameof(ManifestHeaderOutboundMessageHeaderProvider.UserId), bthPassword.GP_UserID, provider.UserId);
				AssertEquals(nameof(ManifestHeaderOutboundMessageHeaderProvider.UserPassword), EDIMessage.PasswordPlaceHolder, provider.UserPassword);
				AssertEquals(nameof(ManifestHeaderOutboundMessageHeaderProvider.SystemType), "1", provider.SystemType);
			});

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			provider = new ManifestHeaderOutboundMessageHeaderProvider(sendingObject);
			AssertEquals(nameof(ManifestHeaderOutboundMessageHeaderProvider.SystemType), "2", provider.SystemType);
		}
	}
}
