using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(EDIInterchange))]
sealed class EDIInterchangeTest : EnterpriseBusinessObjectTestCase
{
	public void TestSetDefaultValues()
	{
		var interchange = Factory.New<EDIInterchange>();
		AssertEquals(EDIInterchange.ApplicationCodes.JPCustoms, interchange.EI_ApplicationCode);
	}

	public void TestCreateFromMessage()
	{
		var password = Factory.NewWithValidTestData<GlbExternalPasswordNMC>();

		var message = Factory.New<EDIMessage>();
		message.EM_MessageData = new byte[] { 10 };
		message.EM_MessageNum = "JPTST000001";
		message.EM_GP = password.PK;

		var interchange = EDIInterchange.CreateFromMessage(message);
		CombineAssertions(() =>
		{
			var regKey = ObjectFactory.Get<IProductRegistration>().Key;
			AssertEquals("EI_To", $"{regKey.EnterpriseCode}{regKey.ServerCode}_JPC", interchange.EI_To);

			AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
			AssertEquals("EI_ApplicationCode", message.EM_ApplicationCode, interchange.EI_ApplicationCode);
			AssertEquals("EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
			AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
			AssertEquals("Message data is copied to interchange", message.EM_MessageData.Length, interchange.EI_BodyData.Length);
		});
	}

	public void TestGetMessageAttrDictionary()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_To = "WTLTST_JPC";
		var attrDic = ((IxTMessageAttributeProvider)interchange).GetMessageAttrDictionary();

		AssertContainsExactElementsInAnyOrder(new[] { $"{Constants.DirectxT.ReceiverAttribute}:WTLTST_JPC" }, attrDic.Select(c => $"{c.Key}:{c.Value}"));
	}

	public void TestGetMessageData()
	{
		var (branchPK, brokerCredential) = SetupCredential();
		var messageContent = @"   IDA                       TEST3003<******>" + new string(' ', 174) + "IDA__345678901200000000001        3456789012".PadRight(398);

		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_GB = branchPK;
		interchange.EI_GP = brokerCredential.PK;
		interchange.EI_BodyData = JPMessageUtils.ConvertStringToMessage(messageContent);

		var dataProvider = (IMessageDataProvider)interchange;

		using (var reader = dataProvider.GetMessageData())
		{
			var finalContent = JPMessageUtils.ConvertMessageToString(reader.ReadBytes((int)reader.BaseStream.Length));

			CombineAssertions(() =>
			{
				AssertContains("Should contain the MIME Header.", "MIME-Version: 1.0", finalContent);
				AssertContains("Should contain the correct charset.", @"Content-Type: text/plain; charset=EUC-JP", finalContent);
				AssertContains("Should contain the correct encoding.", "Content-Transfer-Encoding: 8bit", finalContent);
				AssertContains("Should contain the Company Credential Mail Address.", "From: TEST@Mail.Test.NACCS6", finalContent);
				AssertContains("Should contain the NACCS Mail Box Address.", "To: NACCS@Mail.Test.NACCS6", finalContent);
				AssertContains("Should contain the message content with the decrypted password.", messageContent.Replace("<******>", brokerCredential.CurrentDecryptedPassword), finalContent);
			});
		}
		var reg = ObjectFactory.Get<IProductRegistration>();
		reg.ResetKeyToDefault();
		reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

		using (var reader = dataProvider.GetMessageData())
		{
			var finalContent = JPMessageUtils.ConvertMessageToString(reader.ReadBytes((int)reader.BaseStream.Length));

			CombineAssertions(() =>
			{
				AssertContains("Should contain the Company Credential Mail Address.", "From: TEST@Mail.Prod.NACCS6", finalContent);
				AssertContains("Should contain the NACCS Mail Box Address.", "To: NACCS@Mail.Prod.NACCS6", finalContent);
			});
		}
	}

	public void TestGetMessageDataWithAttachment()
	{
		var orginalFilename = "テスト.pdf";
		var encodedFilenameInJP = JPMessageUtils.AttachmentFilenameJapaneseEncoding.GetBytes(orginalFilename);
		var filenameInBase64 = Convert.ToBase64String(encodedFilenameInJP);
		var declaration = Factory.New(ObjectFactory.GetType(typeof(Integration.Customs.JP.IJobDeclaration))) as BaseJobDeclaration;
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], orginalFilename, "CIV");

		var (branchPK, brokerCredential) = SetupCredential();
		var messageContent = @"   MSX                       TEST3003<******>" + new string(' ', 174) + "MSX__345678901200000000001        3456789012".PadRight(398);

		var outgoingMessage = Factory.New<EDIMessage>();
		outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		outgoingMessage.EM_MessageData = JPMessageUtils.ConvertStringToMessage(messageContent);
		outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
		outgoingMessage.EM_LinkedObject = entryHeader;
		var messageAttach = outgoingMessage.MessageAttachments.AddNew();
		messageAttach.EG_FileName = eDoc.FileName;
		messageAttach.EG_StorageDocsGuid = eDoc.UniqueKey;

		Factory.Save();

		var interchange = EDIInterchange.CreateFromMessage(outgoingMessage);
		interchange.EI_GB = branchPK;
		interchange.EI_GP = brokerCredential.PK;
		var dataProvider = (IMessageDataProvider)interchange;

		using (var reader = dataProvider.GetMessageData())
		{
			var finalContent = JPMessageUtils.ConvertMessageToString(reader.ReadBytes((int)reader.BaseStream.Length));
			CombineAssertions(() =>
			{
				AssertContains("Should contain the MIME Header.", "MIME-Version: 1.0", finalContent);
				AssertContains("Should contain the mixed content-type", "Content-Type: multipart/mix; boundary=", finalContent);
				AssertContains("Should contain the correct charset for message part.", @"Content-Type: text/plain; charset=EUC-JP", finalContent);
				AssertContains("Should contain the correct encoding for message part.", "Content-Transfer-Encoding: 8bit", finalContent);
				AssertContains("Should contain the Company Credential Mail Address.", "From: TEST@Mail.Test.NACCS6", finalContent);
				AssertContains("Should contain the NACCS Mail Box Address.", "To: NACCS@Mail.Test.NACCS6", finalContent);
				AssertContains("Should contain the message content with the decrypted password.", messageContent.Replace("<******>", brokerCredential.CurrentDecryptedPassword), finalContent);
				AssertContains("Should contain the correct content-type setting for the attachment", "Content-Type: application/pdf;", finalContent);
				AssertContains("Should contain the correct name parameter from content-type", $"name=\"=?ISO-2022-JP?B?{filenameInBase64}?=\"", finalContent);
				AssertContains("Should contain the correct content-transfer-encoding setting for the attachment", "Content-Transfer-Encoding: base64", finalContent);
				AssertContains("Should contain the correct content-disposition setting for the attachment", "Content-Disposition: attachment;", finalContent);
				AssertContains("Should contain the correct filename parameter from content-disposition", $"filename=\"=?ISO-2022-JP?B?{filenameInBase64}?=\"", finalContent);
			});
		}

		var reg = ObjectFactory.Get<IProductRegistration>();
		reg.ResetKeyToDefault();
		reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

		using (var reader = dataProvider.GetMessageData())
		{
			var finalContent = JPMessageUtils.ConvertMessageToString(reader.ReadBytes((int)reader.BaseStream.Length));

			CombineAssertions(() =>
			{
				AssertContains("Should contain the Company Credential Mail Address.", "From: TEST@Mail.Prod.NACCS6", finalContent);
				AssertContains("Should contain the NACCS Mail Box Address.", "To: NACCS@Mail.Prod.NACCS6", finalContent);
			});
		}
	}

	(ZGuid, GlbExternalPasswordCUS) SetupCredential()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

		var branch = company.Branches.AddNew();
		branch.GB_Code = "TST";

		var broker = Factory.NewWithValidTestData<GlbStaff>();
		broker.GS_GB_HomeBranch = branch.PK;
		broker.GS_LoginName = "Ayachi";
		broker.GS_FullName = "Ayachi Ne";
		broker.GS_Code = "AN";

		var brokerCredential = Factory.New<GlbExternalPasswordCUS>();
		brokerCredential.GP_GC = company.PK;
		brokerCredential.GP_GS = broker.PK;
		brokerCredential.GP_PasswordType = JPPasswordType.Codes.CUS;
		brokerCredential.GP_Transport = UserCodeSpecificTransportModeList.Codes.SEA;
		brokerCredential.GP_MailBoxID = "TEST3";
		brokerCredential.GP_UserID = "003";
		brokerCredential.CurrentDecryptedPassword = "12345678";

		var companyCredential = Factory.New<GlbExternalPasswordNMC>();
		companyCredential.GP_GC = company.PK;
		companyCredential.GP_MailBoxID = "TEST";

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var configType1 = helper.CreateOrGetExistingRefSysConfigType("NACCSMailP", "NACCS Mail Prod", "NACCS Mail Prod");
		var configType2 = helper.CreateOrGetExistingRefSysConfigType("NACCSMailT", "NACCS Mail Test", "NACCS Mail Test");
		helper.CreateOrUpdateExistingRefSysConfig(configType1.ZRT_ConfigCode, "NACCS@Mail.Prod.NACCS6", ZDateTime.Now.AddMonths(-1), ZDateTime.Now.AddMonths(1));
		helper.CreateOrUpdateExistingRefSysConfig(configType2.ZRT_ConfigCode, "NACCS@Mail.Test.NACCS6", ZDateTime.Now.AddMonths(-1), ZDateTime.Now.AddMonths(1));

		Factory.Save();
		return (branch.PK, brokerCredential);
	}
}
