using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(LPCOMessageSendingObject))]
	class LPCOMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			AssertEquals("Default Message Type", LPCOMessageTypesList.Codes.ORI, sendingObject.MessageType);
		}

		public void TestMessageTypeList()
		{
			AssertContainsExactElementsInExactOrder(new string[] { "ORI", "RET", "ALE", "COM", "MSG", "RCA", "REQ" }, sendingObject.MessageTypeList.GetAllCodes());
			AssertSame(sendingObject.MessageTypeList, sendingObject.MessageTypeList);
		}

		public void TestProperties()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "USERPERMIT";
			staff.GS_Code = "PRT";

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			parent.BrokerCode = staff.GS_Code;

			AssertEquals("MessageAttachee", parent.LPCOHeader, sendingObject.MessageAttachee);
			AssertEquals("GetMessageText", ExpectedJsonMessage, sendingObject.GetMessageText());
			AssertEquals("GetMessageTypeForEDIMessage", MessageTypeList.Codes.LPC, sendingObject.GetMessageTypeForEDIMessage());
			AssertEquals("GetApplicationReference", parent.LPCOHeader.CPH_Number, sendingObject.GetApplicationReference());
			AssertEquals("GetMessageOwner", ZString.Empty, sendingObject.GetMessageOwner());
			AssertEquals("SubmittedDate", ZDateTime.Empty, sendingObject.SubmittedDate);
			AssertEquals("GetGlbExternalPasswordPK", parent.BrokerCertificate.PK, sendingObject.GetGlbExternalPasswordPK());
			AssertEquals("Requirement", ZInt.Zero, sendingObject.Requirement);
			AssertEquals("PermitNumber", ZString.Empty, sendingObject.PermitNumber);
			AssertEquals("NewEffectiveDate", ZDate.Empty, sendingObject.NewEffectiveDate);
			AssertEquals("Message", ZString.Empty, sendingObject.Message);
		}

		public void TestLPCORequestObject()
		{
			AssertPropertyReadOnly(sendingObject.ReasonInfo, (ZString)"reason", new[] { LPCOEntryActionCodeList.Codes.RCA, LPCOEntryActionCodeList.Codes.REQ, LPCOEntryActionCodeList.Codes.ALE, LPCOEntryActionCodeList.Codes.COM });
			AssertPropertyReadOnly(sendingObject.RequirementInfo, (ZInt)444, new[] { LPCOEntryActionCodeList.Codes.REQ });
			AssertPropertyReadOnly(sendingObject.NewEffectiveDateInfo, ZDate.Today, new[] { LPCOEntryActionCodeList.Codes.ALE });
			AssertPropertyReadOnly(sendingObject.PermitNumberInfo, (ZString)"123456", new[] { LPCOEntryActionCodeList.Codes.COM });
			AssertPropertyReadOnly(sendingObject.MessageInfo, (ZString)"TEST MESSAGE", new[] { LPCOEntryActionCodeList.Codes.MSG });

			void AssertPropertyReadOnly(ZPropertyInfo propertyInfo, IZType nonEmptyValue, IEnumerable<string> availableMessageTypes)
			{
				LPCORequestObjectTest.AssertPropertyReadOnly(sendingObject.MessageTypeInfo as ZPropertyInfoString, propertyInfo, nonEmptyValue, sendingObject.MessageTypeList.GetAllCodes(), availableMessageTypes);
			}
		}

		public void TestGetApplicationReference()
		{
			sendingObject.MessageType = LPCOMessageTypesList.Codes.REQ;
			sendingObject.Requirement = 111;

			AssertEquals("GetApplicationReference should return ", "TEST01|111", sendingObject.GetApplicationReference());

			foreach (var messageType in new LPCOMessageTypesList().GetAllCodes().Except(new[] { LPCOMessageTypesList.Codes.REQ }))
			{
				sendingObject.MessageType = messageType;
				AssertEquals("GetApplicationReference should return ", "TEST01", sendingObject.GetApplicationReference());
			}
		}

		public void TestGetMessageText()
		{
			CombineAssertions(() =>
			{
				sendingObject.MessageType = LPCOMessageTypesList.Codes.ORI;
				AssertEquals(sendingObject.MessageType, "{\r\n  \"dataReferencia\": \"24-12-2024T12:48\"\r\n}", sendingObject.GetMessageText());
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

		protected override BusinessObject GetNewBusinessObject() => sendingObject;

		protected override void SetUp()
		{
			var lpcoHeader = Factory.NewWithValidTestData<CusLPCOHeader>();
			lpcoHeader.CPH_RetroactiveDate = new ZDateTimeOffset(2024, 12, 24, 12, 48, 12);

			parent = new LPCOMessageSendingObjectParent(lpcoHeader);
			parent.LPCOHeader.CPH_Number = "TEST01";
			sendingObject = new LPCOMessageSendingObject(parent);
		}

		LPCOMessageSendingObject sendingObject;
		LPCOMessageSendingObjectParent parent;

		internal const string ExpectedJsonMessage = @"{
  ""dataReferencia"": ""24-12-2024T12:48""
}";
	}
}
