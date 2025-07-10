using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(LPCODeclarationMessageSendingObject))]
	class LPCODeclarationMessageSendingObjectTest : DeclarationMessageSendingObjectTest
	{
		protected override ZString ExpectedDefaultMessageType => LPCOEntryActionCodeList.Codes.ORI;

		protected override IReadOnlyList<string> ExpectedMessageTypeCodes => new string[] { LPCOEntryActionCodeList.Codes.ORI, LPCOEntryActionCodeList.Codes.REQ, LPCOEntryActionCodeList.Codes.RCA, LPCOEntryActionCodeList.Codes.ALE, LPCOEntryActionCodeList.Codes.COM, LPCOEntryActionCodeList.Codes.MSG };

		protected override ZString ExpectedMessageTypeForEDIMessage => MessageTypeList.Codes.LPC;

		protected override ZString ExpectedFriendlyNameForMessageManager => MessageTypeList.Descriptions.LPC;

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.LPCO;

		protected override DeclarationMessageSendingObject CreateNewMessageSendingObject(CusEntryHeader entryHeader) => new LPCODeclarationMessageSendingObject(entryHeader);

		public void TestProperties()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting() as LPCODeclarationMessageSendingObject;
			var entryHeader = sendingObject.Header;
			entryHeader.CH_EntrySubmittedDate = new ZDateTime(2020, 12, 12);
			entryHeader.CH_Status = "ABC";
			AssertEquals(new ZDateTime(2020, 12, 12), sendingObject.SubmittedDate);
			AssertEquals("ABC", sendingObject.CustomsStatus);
		}

		public override void TestDefaultMessageType()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertEquals("Default Message Type", ExpectedDefaultMessageType, sendingObject.MessageType);

			var entry = sendingObject.Header;
			entry.EntryNumber = "EN1234";
			entry.CH_EntryStatus = ZString.Empty;
			sendingObject = new LPCODeclarationMessageSendingObject(entry);
			AssertEquals(LPCOEntryActionCodeList.Codes.RCA, sendingObject.MessageType);
			entry.EntryNumber = ZString.Empty;
			entry.CH_EntryStatus = "EST";
			sendingObject = new LPCODeclarationMessageSendingObject(entry);
			AssertEquals(LPCOEntryActionCodeList.Codes.ORI, sendingObject.MessageType);
		}

		public void TestGetGlbExternalPasswordPK()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BR1";
			var password = BRGlbStaffWrapper.Get(broker).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var sendingObject = GetNewMessageSendingObjectForTesting();
			sendingObject.Header.Declaration.JE_GS_NKCusAgent = broker.GS_Code;
			AssertEquals("GetGlbExternalPasswordPK should be", password.PK, sendingObject.GetGlbExternalPasswordPK());
		}

		public void TestShouldSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var sendingObject = CreateNewMessageSendingObject(entryHeader);

			AssertEquals("ShouldSend", true, sendingObject.ShouldSend);

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;

			sendingObject = CreateNewMessageSendingObject(entryHeader);
			AssertEquals("ShouldSend", false, sendingObject.ShouldSend);
		}

		public void TestGetApplicationReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "EntryInstructionDescription";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.MovementReferenceNumberSetter("Ref123", ZDateTime.Now);

			var sendingObject = CreateNewMessageSendingObject(entryHeader) as LPCODeclarationMessageSendingObject;
			sendingObject.MessageType = LPCOEntryActionCodeList.Codes.REQ;
			sendingObject.Requirement = 444;
			AssertEquals("Ref123|444", sendingObject.GetApplicationReference());

			foreach (var messageType in new string[] { LPCOEntryActionCodeList.Codes.ORI, LPCOEntryActionCodeList.Codes.RCA, LPCOEntryActionCodeList.Codes.ALE })
			{
				sendingObject.MessageType = messageType;
				sendingObject.Requirement = 444;
				AssertEquals("Ref123", sendingObject.GetApplicationReference());
			}
		}

		public void TestNewEffectiveDate()
		{
			var date = ZDate.Today;
			var messageSending = GetNewMessageSendingObjectForTesting() as LPCODeclarationMessageSendingObject;

			CombineAssertions(() =>
			{
				messageSending.MessageType = LPCOEntryActionCodeList.Codes.ALE;
				messageSending.NewEffectiveDate = date;
				AssertEquals("MessageType = ALE", date, messageSending.NewEffectiveDate);

				messageSending.NewEffectiveDate = date;
				messageSending.MessageType = LPCOEntryActionCodeList.Codes.RCA;
				AssertEquals("MessageType = RCA", ZDate.Empty, messageSending.NewEffectiveDate);

				messageSending.NewEffectiveDate = date;
				messageSending.MessageType = LPCOEntryActionCodeList.Codes.REQ;
				AssertEquals("MessageType = REQ", ZDate.Empty, messageSending.NewEffectiveDate);

				messageSending.NewEffectiveDate = date;
				messageSending.MessageType = LPCOEntryActionCodeList.Codes.MSG;
				AssertEquals("MessageType = MSG", ZDate.Empty, messageSending.NewEffectiveDate);

				messageSending.NewEffectiveDate = date;
				messageSending.MessageType = LPCOEntryActionCodeList.Codes.ORI;
				AssertEquals("MessageType = ORI", ZDate.Empty, messageSending.NewEffectiveDate);
			});
		}

		public void TestLPCORequestObject()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting() as LPCODeclarationMessageSendingObject;

			AssertPropertyReadOnly(sendingObject.ReasonInfo, (ZString)"reason", new[] { LPCOEntryActionCodeList.Codes.RCA, LPCOEntryActionCodeList.Codes.REQ, LPCOEntryActionCodeList.Codes.ALE, LPCOEntryActionCodeList.Codes.COM });
			AssertPropertyReadOnly(sendingObject.RequirementInfo, (ZInt)444, new[] { LPCOEntryActionCodeList.Codes.REQ });
			AssertPropertyReadOnly(sendingObject.NewEffectiveDateInfo, ZDate.Today, new[] { LPCOEntryActionCodeList.Codes.ALE });
			AssertPropertyReadOnly(sendingObject.EntryNumberInfo, (ZString)"123456", new[] { LPCOEntryActionCodeList.Codes.COM });
			AssertPropertyReadOnly(sendingObject.EntryLineNumberInfo, (ZInt)111, new[] { LPCOEntryActionCodeList.Codes.COM });
			AssertPropertyReadOnly(sendingObject.VersionInfo, (ZString)"1", new[] { LPCOEntryActionCodeList.Codes.COM });
			AssertPropertyReadOnly(sendingObject.MessageInfo, (ZString)"TEST MESSAGE", new[] { LPCOEntryActionCodeList.Codes.MSG });

			void AssertPropertyReadOnly(ZPropertyInfo propertyInfo, IZType nonEmptyValue, IEnumerable<string> availableMessageTypes)
			{
				LPCORequestObjectTest.AssertPropertyReadOnly(sendingObject.MessageTypeInfo as ZPropertyInfoString, propertyInfo, nonEmptyValue, sendingObject.MessageTypesList.GetAllCodes(), availableMessageTypes);
			}
		}

		public void TestGetMessageText()
		{
			CombineAssertions(() =>
			{
				var sendingObject = GetNewMessageSendingObjectForTesting() as LPCODeclarationMessageSendingObject;

				sendingObject.MessageType = LPCOMessageTypesList.Codes.ORI;
				AssertEquals(sendingObject.MessageType, ZString.Empty, sendingObject.GetMessageText());
				sendingObject.MessageType = LPCOMessageTypesList.Codes.RET;
				AssertEquals(sendingObject.MessageType, ZString.Empty, sendingObject.GetMessageText());
				sendingObject.MessageType = LPCOMessageTypesList.Codes.REQ;
				AssertEquals(sendingObject.MessageType, "{\r\n  \"justificativa\": \"\"\r\n}", sendingObject.GetMessageText());
				sendingObject.MessageType = LPCOMessageTypesList.Codes.RCA;
				AssertEquals(sendingObject.MessageType, "{\r\n  \"justificativa\": \"\"\r\n}", sendingObject.GetMessageText());
				sendingObject.MessageType = LPCOMessageTypesList.Codes.COM;
				AssertEquals(sendingObject.MessageType, "{\r\n  \"numeroDocumento\": \"\",\r\n  \"versaoCompatibilizacao\": \"\",\r\n  \"justificativa\": \"\"\r\n}", sendingObject.GetMessageText());
				sendingObject.MessageType = LPCOMessageTypesList.Codes.ALE;
				AssertEquals(sendingObject.MessageType, "{\r\n  \"novaDataFimVigencia\": \"\",\r\n  \"justificativa\": \"\"\r\n}", sendingObject.GetMessageText());
				sendingObject.MessageType = LPCOMessageTypesList.Codes.MSG;
				AssertEquals(sendingObject.MessageType, "{\r\n  \"mensagem\": \"\"\r\n}", sendingObject.GetMessageText());
			});
		}
	}
}
