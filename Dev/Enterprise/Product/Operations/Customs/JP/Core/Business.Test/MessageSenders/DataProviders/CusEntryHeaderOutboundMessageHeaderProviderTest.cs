using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Business.Testing
{
	sealed class CusEntryHeaderOutboundMessageHeaderProviderTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var sendingObject = new MessageSendingObject(entryHeader);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			sendingObject.ProcedureCode = "ABC01";
			entryHeader.CH_BGMReference = "InputRef";

			var message = Factory.New<EDIMessage>();
			message.EM_LinkedObject = entryHeader;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";

			var bthPassword = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
			bthPassword.GP_GS = staff.PK;
			bthPassword.GP_MailBoxID = "TEST1";
			bthPassword.GP_UserID = "TEST2";
			bthPassword.CurrentDecryptedPassword = "TESTPASS";
			entryHeader.Declaration.JE_GS_NKCusAgent = staff.GS_Code;
			entryHeader.Declaration.JE_NACCSCredential = bthPassword.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				var provider = new CusEntryHeaderOutboundMessageHeaderProvider(sendingObject);
				AssertEndsWith(nameof(CusEntryHeaderOutboundMessageHeaderProvider.MessageTag), "<<MSGNO PLACEHOLDER>>", provider.MessageTag);
				AssertEquals(nameof(CusEntryHeaderOutboundMessageHeaderProvider.InputReference), "InputRef", provider.InputReference);
				AssertEquals(nameof(CusEntryHeaderOutboundMessageHeaderProvider.ProcedureCode), "ABC01", provider.ProcedureCode);
				AssertEquals(nameof(CusEntryHeaderOutboundMessageHeaderProvider.SystemType), "1", provider.SystemType);
				AssertEquals("GP_MailBoxID", bthPassword.GP_MailBoxID, provider.UserCode);
				AssertEquals("GP_UserID", bthPassword.GP_UserID, provider.UserId);
				AssertEquals("PasswordPlaceHolder", EDIMessage.PasswordPlaceHolder, provider.UserPassword);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				provider = new CusEntryHeaderOutboundMessageHeaderProvider(sendingObject);
				AssertEquals(nameof(CusEntryHeaderOutboundMessageHeaderProvider.SystemType), "2", provider.SystemType);
			});
		}
	}
}
