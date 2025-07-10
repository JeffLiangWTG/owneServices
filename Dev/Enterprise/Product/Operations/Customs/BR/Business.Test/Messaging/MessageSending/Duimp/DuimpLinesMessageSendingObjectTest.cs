using System.Text.Json;
using CargoWise.Customs.BR.MessageDefinitions.Duimp;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class DuimpLinesMessageSendingObjectTest : TestCaseWithFactory
	{
		public void TestIMessageSendingObject()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BR1";
			var password = BRGlbStaffWrapper.Get(broker).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_CEI = entryInstruction.PK;
			declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("21BR0000022649");
			entryHeader.CH_AuthorityVersion = "111";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			declaration.InvoiceLines[0].JI_CL = entryLine1.PK;
			declaration.InvoiceLines[1].JI_CL = entryLine2.PK;

			CombineAssertions("Addition", () =>
			{
				var sendingObject = new DuimpLinesMessageSendingObject(entryHeader.AllEntryLines, EDIMessageSubTypeList.Codes.Addition);
				sendingObject.Header.Declaration.JE_GS_NKCusAgent = broker.GS_Code;
				AssertEquals("MessageAttachee", entryHeader, sendingObject.MessageAttachee);
				AssertEquals("GetMessageTypeForEDIMessage", MessageTypeList.Codes.CIL, sendingObject.GetMessageTypeForEDIMessage());
				AssertEquals("MessageType", EDIMessageSubTypeList.Codes.Addition, sendingObject.MessageType);
				AssertEquals("GetMessageOwner", ZString.Empty, sendingObject.GetMessageOwner());
				AssertEquals("GetMessageText", 2, JsonSerializer.Deserialize<ItemCover[]>(sendingObject.GetMessageText()).Length);
				AssertEquals("GetGlbExternalPasswordPK", password.PK, sendingObject.GetGlbExternalPasswordPK());
				AssertEquals("GetApplicationReference", "21BR0000022649|111", sendingObject.GetApplicationReference());
			});

			CombineAssertions("Update", () =>
			{
				var downloadObject = new DuimpLinesMessageSendingObject(entryHeader.AllEntryLines, EDIMessageSubTypeList.Codes.Update);
				downloadObject.Header.Declaration.JE_GS_NKCusAgent = broker.GS_Code;
				AssertEquals("GetMessageTypeForEDIMessage", MessageTypeList.Codes.CIL, downloadObject.GetMessageTypeForEDIMessage());
				AssertEquals("MessageType", EDIMessageSubTypeList.Codes.Update, downloadObject.MessageType);
				AssertEquals("GetMessageOwner", ZString.Empty, downloadObject.GetMessageOwner());
				AssertEquals("GetMessageText", 2, JsonSerializer.Deserialize<ItemCover[]>(downloadObject.GetMessageText()).Length);
				AssertEquals("GetGlbExternalPasswordPK", password.PK, downloadObject.GetGlbExternalPasswordPK());
				AssertEquals("GetApplicationReference", "21BR0000022649|111", downloadObject.GetApplicationReference());
			});

			CombineAssertions("Deletion", () =>
			{
				var downloadObject = new DuimpLinesMessageSendingObject(new[] { entryLine2 }, EDIMessageSubTypeList.Codes.Deletion);
				downloadObject.Header.Declaration.JE_GS_NKCusAgent = broker.GS_Code;
				AssertEquals("GetMessageTypeForEDIMessage", MessageTypeList.Codes.CIL, downloadObject.GetMessageTypeForEDIMessage());
				AssertEquals("MessageType", EDIMessageSubTypeList.Codes.Deletion, downloadObject.MessageType);
				AssertEquals("GetMessageOwner", ZString.Empty, downloadObject.GetMessageOwner());
				AssertEquals("GetMessageText", ZString.Empty, downloadObject.GetMessageText());
				AssertEquals("GetGlbExternalPasswordPK", password.PK, downloadObject.GetGlbExternalPasswordPK());
				AssertEquals("GetApplicationReference", "21BR0000022649|111|2", downloadObject.GetApplicationReference());
			});
		}
	}
}
