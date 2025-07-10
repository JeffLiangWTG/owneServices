using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ECSMessageBuilderManagerTest : TestCaseWithFactory
	{
		[TestDate(2021, 3, 16, 15, 13, 23, 456)]
		public void TestNewMessageBuilder()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_GS_NKCustomsAgent = staff.GS_Code;
			var exitDetail = Factory.NewWithValidTestData<CusExitDetail>();
			exitDetail.CED_Status = "XXX";
			exitDetail.CED_CEH = exitHeader.PK;
			exitDetail.CED_MovementReferenceNumber = "11ES00113112683757";
			var messageObject = new ECSExitHeaderMessageSendingObject(exitDetail);
			var messageBuilderManager = new ECSMessageBuilderManager(messageObject, certificate);

			var messageBuilder = messageBuilderManager.NewMessageBuilder();
			AssertType<ArrivalAtExitExportMessageBuilder>("NewMessageBuilder is ArrivalAtExit type", messageBuilder);
		}

		[TestDate(2021, 3, 16, 15, 13, 23, 456)]
		public void TestGetCompleteMessage()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_GS_NKCustomsAgent = staff.GS_Code;
			var exitDetail = Factory.NewWithValidTestData<CusExitDetail>();
			exitDetail.CED_Status = "XXX";
			exitDetail.CED_CEH = exitHeader.PK;
			exitDetail.CED_MovementReferenceNumber = "11ES00113112683757";
			var messageObject = new ECSExitHeaderMessageSendingObject(exitDetail);
			var messageBuilderManager = new ECSMessageBuilderManager(messageObject, certificate);

			var messageBuilder = messageBuilderManager.NewMessageBuilder();

			CombineAssertions(() =>
			{
				AssertEquals("messageBuilder.MessageType", DeclarationMessageTypeList.Codes.ArrivalAtExit, messageBuilder.MessageType);
				AssertEquals("messageBuilder.MessageSubType", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, messageBuilder.MessageSubType);
				AssertEquals("messageBuilder.Provider.IsTest", false, messageBuilder.Provider.IsTest);
				AssertEquals("messageBuilder.Provider.CertificateName", certificate.CertificateName, messageBuilder.Provider.CertificateName);
				AssertEquals("messageBuilder.Provider.CertificatePK", certificate.CertificatePK, messageBuilder.Provider.CertificatePK);
				AssertEquals("messageBuilder.Provider.BusinessObjectReference", exitDetail.CED_MovementReferenceNumber, messageBuilder.Provider.BusinessObjectReference);
				AssertMultilineASCIIEquals("messageBuilder.CompleteMessageText", ExpectedMessageText, messageBuilder.GetSignedMessageText().Replace("'", "'\n"));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			staff = staffWithCertificateHelperTest.Staff;
			certificate = staffWithCertificateHelperTest.Certificate;
		}

		CertificateProviderTestClass certificate;
		GlbStaff staff;

		const string ExpectedMessageText = @"UNB+UNOA:1+:ZZ+AEATADUE:ZZ+210316:1513+<<MSGNO PLACEHOLDER>>++&EE'
UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:1:921:UN:ECSR02'
BGM+EAL+11113112683757'
CST++++++11ES00113112683757'
DTM+128:20210316:102'
UNT+5+<<MSGNO PLACEHOLDER>>'
UNZ+1+<<MSGNO PLACEHOLDER>>'";
	}
}
