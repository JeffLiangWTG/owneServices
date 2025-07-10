using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.GB.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Customs.GB.CNS.Testing
{
	class CnsCredentialsCheckerTests : TestCaseWithFactory
	{
		public void TestCheckCredentials()
		{
			var mock = new Mock<ICspPrintsMailBoxProvider>();
			var objectSubstitution = ObjectFactory.Substitute(mock.Object);

			var ediMessageBatch = new TestCspDownloadResult();
			ediMessageBatch.SetMessagesArray(new string[] { @"UNB", "UNB" });

			mock.Setup(m => m.checkCdsCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(ediMessageBatch);

			var badge = new BadgeCodeSetting();
			badge.BadgeCode = "JAN";
			badge.CSPCode = "CNS";
			badge.ApplicationCode = "CDS";
			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var credential = new CredentialsSetting();
			credential.BadgeCode = badge.BadgeCode;

			var cdsCredentialCheckerResponse = new CDSCredentialCheckerResponse();
			cdsCredentialCheckerResponse.MessagesArray = new string[] { "http://www.topicregisteredto.url" };
			badge.ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			mock.Setup(m => m.checkCdsCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(cdsCredentialCheckerResponse);
			var notifier = UnitTestUserNotification.Instance;
			credential.CheckCredentialsAgainstCspWebService(notifier);
			AssertContains("CSP confirmed that the credentials are known, and that the topic is registered to: \r\nhttp://www.topicregisteredto.url", notifier.LastMessage.Text);

			cdsCredentialCheckerResponse.MessagesArray = Array.Empty<string>();
			mock.Setup(m => m.checkCdsCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(cdsCredentialCheckerResponse);
			credential.CheckCredentialsAgainstCspWebService(notifier);
			AssertContains("CSP confirmed that the credentials are known, but the topic is not curently registered. \r\neHub will register the topic when first used. Please refer to eLearning unit 1BGB047", notifier.LastMessage.Text);

			cdsCredentialCheckerResponse.errorText = "401 UNAUTHORIZED";
			mock.Setup(m => m.checkCdsCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(cdsCredentialCheckerResponse);
			credential.CheckCredentialsAgainstCspWebService(notifier);
			AssertContains("CSP rejected the request.  Please liaise directly with the CSP about their rejection of your details. \r\nThe error was: 401 UNAUTHORIZED", notifier.LastMessage.Text);
			mock.VerifyAll();
		}
	}

	class TestCspDownloadResult : ICspDownloadResult
	{
		string[] messagesArray;
		string ICspDownloadResult.errorText { get { return string.Empty; } }
		public int batchId { get; set; }
		string[] ICspDownloadResult.MessagesArray
		{
			get
			{
				return this.messagesArray;
			}
		}
		public void SetMessagesArray(string[] messages)
		{
			messagesArray = messages;
		}
	}
}
