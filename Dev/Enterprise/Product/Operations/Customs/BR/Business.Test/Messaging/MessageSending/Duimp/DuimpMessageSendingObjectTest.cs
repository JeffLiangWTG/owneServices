using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DuimpMessageSendingObject))]
	public class DuimpMessageSendingObjectTest : DeclarationMessageSendingObjectTest
	{
		protected override ZString ExpectedDefaultMessageType => ImportEntryActionCodeList.Codes.ORI;

		protected override IReadOnlyList<string> ExpectedMessageTypeCodes => new string[] { "DIA", "ORI", "REG" };

		protected override ZString ExpectedMessageTypeForEDIMessage => MessageTypeList.Codes.CIH;

		protected override ZString ExpectedFriendlyNameForMessageManager => MessageTypeList.Descriptions.CDD;

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.Import;

		protected override DeclarationMessageSendingObject CreateNewMessageSendingObject(CusEntryHeader entryHeader) => new DuimpMessageSendingObject(entryHeader);

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

		public void TestExpectedMessageTypeForEDIMessage()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			sendingObject.MessageType = EDIMessageSubTypeList.Codes.CompleteConsult;
			AssertEquals("GetMessageTypeForEDIMessage", MessageTypeList.Codes.CIH, sendingObject.GetMessageTypeForEDIMessage());

			sendingObject.MessageType = ImportEntryActionCodeList.Codes.ORI;
			AssertEquals("GetMessageTypeForEDIMessage", MessageTypeList.Codes.CIH, sendingObject.GetMessageTypeForEDIMessage());

			sendingObject.MessageType = EDIMessageSubTypeList.Codes.Update;
			AssertEquals("GetMessageTypeForEDIMessage", MessageTypeList.Codes.CIH, sendingObject.GetMessageTypeForEDIMessage());

			sendingObject.MessageType = ImportEntryActionCodeList.Codes.DEL;
			AssertEquals("GetMessageTypeForEDIMessage", MessageTypeList.Codes.DOR, sendingObject.GetMessageTypeForEDIMessage());

			sendingObject.MessageType = ImportEntryActionCodeList.Codes.CVH;
			AssertEquals("GetMessageTypeForEDIMessage", MessageTypeList.Codes.CDD, sendingObject.GetMessageTypeForEDIMessage());

			sendingObject.MessageType = ImportEntryActionCodeList.Codes.DIA;
			AssertEquals("GetMessageTypeForEDIMessage", MessageTypeList.Codes.CIH, sendingObject.GetMessageTypeForEDIMessage());

			sendingObject.MessageType = ImportEntryActionCodeList.Codes.REG;
			AssertEquals("GetMessageTypeForEDIMessage", MessageTypeList.Codes.CIH, sendingObject.GetMessageTypeForEDIMessage());

			sendingObject.MessageType = ImportEntryActionCodeList.Codes.RET;
			AssertEquals("GetMessageTypeForEDIMessage", MessageTypeList.Codes.CDD, sendingObject.GetMessageTypeForEDIMessage());
		}

		public void TestGetApplicationReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobDeclarationMessageType;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "EntryInstructionDescription";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.MovementReferenceNumberSetter("23269020");
			entryHeader.CH_AuthorityVersion = "1";
			var sendingObject = CreateNewMessageSendingObject(entryHeader);

			sendingObject.MessageType = ImportEntryActionCodeList.Codes.ORI;
			AssertEquals("23269020", sendingObject.GetApplicationReference());
			sendingObject.MessageType = ImportEntryActionCodeList.Codes.CVH;
			AssertEquals("23269020|1", sendingObject.GetApplicationReference());
			sendingObject.MessageType = ImportEntryActionCodeList.Codes.DEL;
			AssertEquals("23269020|1", sendingObject.GetApplicationReference());
			sendingObject.MessageType = ImportEntryActionCodeList.Codes.RET;
			AssertEquals("23269020|1", sendingObject.GetApplicationReference());
			sendingObject.MessageType = ImportEntryActionCodeList.Codes.DIA;
			AssertEquals("23269020|1", sendingObject.GetApplicationReference());
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

		public void TestGetMessageText()
		{
			CombineAssertions(() =>
			{
				var sendingObject = GetNewMessageSendingObjectForTesting();
				sendingObject.MessageType = EDIMessageSubTypeList.Codes.CompleteConsult;
				AssertEquals(sendingObject.MessageType, ZString.Empty, sendingObject.GetMessageText());

				sendingObject.MessageType = ImportEntryActionCodeList.Codes.ORI;
				AssertEquals(sendingObject.MessageType, expectedDuimpMessageText, sendingObject.GetMessageText());

				sendingObject.MessageType = EDIMessageSubTypeList.Codes.Update;
				AssertEquals(sendingObject.MessageType, expectedDuimpMessageText, sendingObject.GetMessageText());

				sendingObject.MessageType = ImportEntryActionCodeList.Codes.DEL;
				AssertEquals(sendingObject.MessageType, ZString.Empty, sendingObject.GetMessageText());

				sendingObject.MessageType = ImportEntryActionCodeList.Codes.CVH;
				AssertEquals(sendingObject.MessageType, ZString.Empty, sendingObject.GetMessageText());

				sendingObject.MessageType = ImportEntryActionCodeList.Codes.DIA;
				AssertEquals(sendingObject.MessageType, "{\r\n  \"totalItem\": 1\r\n}", sendingObject.GetMessageText());

				sendingObject.MessageType = ImportEntryActionCodeList.Codes.REG;
				AssertEquals(sendingObject.MessageType, "{\r\n  \"totalItem\": 1\r\n}", sendingObject.GetMessageText());

				sendingObject.MessageType = ImportEntryActionCodeList.Codes.RET;
				AssertEquals(sendingObject.MessageType, expectedDuimpMessageText, sendingObject.GetMessageText());
			});
		}

		readonly string expectedDuimpMessageText = @"{
  ""identificacao"": {
    ""importador"": {},
    ""informacaoComplementar"": """"
  },
  ""carga"": {
    ""tipoIdentificacaoCarga"": """",
    ""identificacao"": """",
    ""unidadeDeclarada"": {
      ""codigo"": """"
    },
    ""motivoSituacaoEspecial"": """"
  },
  ""documentos"": {
    ""documentosInstrucao"": [],
    ""processos"": [],
    ""declaracoesExportacaoEstrangeira"": []
  }
}";
	}
}
