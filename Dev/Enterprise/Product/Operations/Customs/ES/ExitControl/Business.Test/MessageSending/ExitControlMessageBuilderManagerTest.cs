using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class ExitControlMessageBuilderManagerTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => new ExitControlMessageBuilderManager(null, certificate));
				AssertExceptionThrown<ArgumentNullException>(() => new ExitControlMessageBuilderManager(new ExitControlMessageSendingObject(exitReport), null));
			});
		}

		[TestDate(2021, 3, 16, 15, 13, 23, 456)]
		public void TestNewMessageBuilder()
		{
			var messageObject = new ExitControlMessageSendingObject(exitReport);
			var messageBuilderManager = new ExitControlMessageBuilderManager(messageObject, certificate);

			var messageBuilder = messageBuilderManager.NewMessageBuilder();
			AssertType<EALAESMessageBuilder>("NewMessageBuilder is EALAESMessageBuilder type", messageBuilder);
		}

		[TestDate(2021, 3, 16, 15, 13, 23, 456)]
		public void TestGetCompleteMessage()
		{
			var messageObject = new ExitControlMessageSendingObject(exitReport);
			var messageBuilderManager = new ExitControlMessageBuilderManager(messageObject, certificate);

			var messageBuilder = messageBuilderManager.NewMessageBuilder();

			CombineAssertions(() =>
			{
				AssertEquals("messageBuilder.MessageType", DeclarationMessageTypeList.Codes.ArrivalAtExitUcc6, messageBuilder.MessageType);
				AssertEquals("messageBuilder.MessageSubType", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, messageBuilder.MessageSubType);
				AssertEquals("messageBuilder.Provider.IsTest", true, messageBuilder.Provider.IsTest);
				AssertEquals("messageBuilder.Provider.CertificateName", certificate.CertificateName, messageBuilder.Provider.CertificateName);
				AssertEquals("messageBuilder.Provider.CertificatePK", certificate.CertificatePK, messageBuilder.Provider.CertificatePK);
				AssertEquals("messageBuilder.Provider.BusinessObjectReference", exitReport.Consignment?.CXC_MovementReference, messageBuilder.Provider.BusinessObjectReference);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			var staff = staffWithCertificateHelperTest.Staff;
			certificate = staffWithCertificateHelperTest.Certificate;
			CusExitHeader exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_GS_NKCustomsAgent = staff.GS_Code;
			exitHeader.CXH_CustomsProfile = "TestCert1";

			exitReport = exitHeader.CusExitReports.AddNew();
			exitReport.CER_MessageStatus = "XXX";
			var consignment = exitHeader.CusExitConsignments.AddNew();
			consignment.CXC_LocalReference = "23ES00999912345678";
			exitReport.CER_CXC_Consignment = consignment.PK;
		}

		CertificateProviderTestClass certificate;
		CusExitReport exitReport;
	}
}
