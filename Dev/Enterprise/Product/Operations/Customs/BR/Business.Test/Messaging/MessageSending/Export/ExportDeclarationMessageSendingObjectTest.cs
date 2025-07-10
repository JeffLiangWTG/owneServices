using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ExportDeclarationMessageSendingObject))]
	public class ExportDeclarationMessageSendingObjectTest : DeclarationMessageSendingObjectTest
	{
		protected override ZString ExpectedDefaultMessageType => ExportEntryActionCodeList.Codes.ORI;

		protected override IReadOnlyList<string> ExpectedMessageTypeCodes => new string[] { "ORI", "RET" };

		protected override ZString ExpectedMessageTypeForEDIMessage => MessageTypeList.Codes.CDE;

		protected override ZString ExpectedFriendlyNameForMessageManager => MessageTypeList.Descriptions.CDE;

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.Export;

		protected override DeclarationMessageSendingObject CreateNewMessageSendingObject(CusEntryHeader entryHeader) => new ExportDeclarationMessageSendingObject(entryHeader);

		public void TestProperties()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			var entryHeader = sendingObject.Header;
			entryHeader.MovementReferenceNumberSetter("ABC", ZDateTime.BrettsBirthday);
			entryHeader.CH_EntrySubmittedDate = new ZDateTime(2020, 12, 12);
			entryHeader.CH_Status = "XXX";
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_SubStyle = "Bye";
			AssertEquals(true, sendingObject.ShouldSend);
			AssertEquals(new ZDateTime(2020, 12, 12), sendingObject.SubmittedDate);
			AssertEquals("XXX", sendingObject.CustomsStatus);
			AssertEquals("ABC", sendingObject.GetApplicationReference());
		}

		public void TestVOCReasonReadOnly()
		{
			var messageSending = GetNewMessageSendingObjectForTesting();
			messageSending.MessageType = ExportEntryActionCodeList.Codes.ORI;
			AssertEquals(true, messageSending.VOCReasonInfo.ReadOnly);
			messageSending.MessageType = ExportEntryActionCodeList.Codes.RET;
			AssertEquals(false, messageSending.VOCReasonInfo.ReadOnly);
		}

		public override void TestDefaultMessageType()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertEquals("Default Message Type", ExpectedDefaultMessageType, sendingObject.MessageType);

			var entry = sendingObject.Header;
			entry.EntryNumber = "EN1234";
			entry.CH_EntryStatus = ZString.Empty;
			sendingObject = new ExportDeclarationMessageSendingObject(entry);
			AssertEquals(ExportEntryActionCodeList.Codes.RET, sendingObject.MessageType);
			entry.EntryNumber = ZString.Empty;
			entry.CH_EntryStatus = "EST";
			sendingObject = new ExportDeclarationMessageSendingObject(entry);
			AssertEquals(ExportEntryActionCodeList.Codes.ORI, sendingObject.MessageType);
		}

		public void TestGetGlbExternalPasswordPK()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BR1";
			var password = BRGlbStaffWrapper.Get(broker).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var sendingObject = GetNewMessageSendingObjectForTesting();
			sendingObject.Header.Declaration.JE_GS_NKCusAgent = broker.GS_Code;
			AssertEquals("GetGlbExternalPasswordPK should be", password.PK, sendingObject.GetGlbExternalPasswordPK());
		}

		public void TestDefaultShouldSend()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertEquals("ShouldSend", true, sendingObject.ShouldSend);

			var entryHeader = sendingObject.Header;
			entryHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			sendingObject = CreateNewMessageSendingObject(entryHeader);
			AssertEquals("ShouldSend", false, sendingObject.ShouldSend);

			var entryHeader2 = entryHeader.Declaration.CustomsEntryHeaders.AddNew();
			sendingObject = CreateNewMessageSendingObject(entryHeader2);
			AssertEquals("ShouldSend", false, sendingObject.ShouldSend);
		}

		public void TestGetMessageText()
		{
			CombineAssertions(() =>
			{
				var sendingObject = GetNewMessageSendingObjectForTesting();
				sendingObject.Header.EntryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;
				AssertContains("CEI_LegalDocument = ElectronicLogisticInvoice", "<DeclarationNFe>", sendingObject.GetMessageText());
				sendingObject.Header.EntryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;
				AssertContains("CEI_LegalDocument = NoInvoice", "<DeclarationNoNF>", sendingObject.GetMessageText());
				sendingObject.MessageType = EDIMessageSubTypeList.Codes.CompleteConsult;
				AssertEquals("CompleteConsult", ZString.Empty, sendingObject.GetMessageText());
			});
		}
	}
}
