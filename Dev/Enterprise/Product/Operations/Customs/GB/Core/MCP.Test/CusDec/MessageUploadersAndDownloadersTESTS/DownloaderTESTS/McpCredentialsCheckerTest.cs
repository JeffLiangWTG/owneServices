using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.MCP.CusDec.destin8WebService;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Customs.GB.MCP.Testing
{
	class McpCredentialsCheckerTest : TestCaseWithFactory
	{
		public void TestCheckCredentials()
		{
			var mock = new Mock<IDestin8WebService>();
			var objectSubstitution = ObjectFactory.Substitute(mock.Object);
			var ediMessageBatch = new EDIMessageBatch();
			var batch = new Batch();
			var listOfPrintMessages = new List<PrintMessage>();
			var printMessage = new PrintMessage();
			printMessage.message = "UNH...";
			listOfPrintMessages.Add(printMessage);
			batch.batchID = 123456;
			batch.messageCount = listOfPrintMessages.Count;
			batch.messages = listOfPrintMessages.ToArray();
			ediMessageBatch.batch = batch;
			ediMessageBatch.errorNo = 0;

			mock.Setup(m => m.checkCdsCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(ediMessageBatch);

			var badge = new BadgeCodeSetting();
			badge = new BadgeCodeSetting();
			badge.BadgeCode = "JAN";
			badge.CSPCode = "MCP";
			badge.ApplicationCode = "CDS";
			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var credential = new CredentialsSetting();
			credential.BadgeCode = badge.BadgeCode;

			var cdsCredentialCheckerResponse = new CDSCredentialCheckerResponse();
			cdsCredentialCheckerResponse.MessagesArray = new string[] { "http://www.topicregisteredto.url" };
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

		/// <summary>
		/// show a new invalid error notification instead of raising exception while the McpDestin8Url is not a valid uri
		/// </summary>
		public void TestCheckCredentialsWithInvalidMcpDestin8UrlWithHttps()
		{
			var mock = new Mock<IDestin8WebService>();
			var objectSubstitution = ObjectFactory.Substitute(mock.Object);
			var ediMessageBatch = new EDIMessageBatch();
			var batch = new Batch();
			var listOfPrintMessages = new List<PrintMessage>();
			var printMessage = new PrintMessage();
			printMessage.message = "UNH...";
			listOfPrintMessages.Add(printMessage);
			batch.batchID = 123456;
			batch.messageCount = listOfPrintMessages.Count;
			batch.messages = listOfPrintMessages.ToArray();
			ediMessageBatch.batch = batch;
			ediMessageBatch.errorNo = 0;

			mock.Setup(m => m.checkCdsCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(ediMessageBatch);
			var badge = new BadgeCodeSetting();
			badge.BadgeCode = "DAN";
			badge.CSPCode = "MCP";
			badge.ApplicationCode = "CDS";
			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var credential = new CredentialsSetting();
			credential.BadgeCode = badge.BadgeCode;

			var notifier = UnitTestUserNotification.Instance;

			//change EditorInfo'sTextEditorType to TextEditorType.Unknown to avoid Url validation
			//otherwise there would raise an validation exception while we assign McpDestin8Url a invalid value
			(GBCustomsDataRegistry.Instance.FindByName("GBMcpDestin8Url2023") as StringRegistryItem).EditorInfo = new TextRegistryEditorInfo(TextEditorType.Unknown);
			(GBCustomsDataRegistry.Instance.FindByName("GBMcpDestin8UrlTest2023") as StringRegistryItem).EditorInfo = new TextRegistryEditorInfo(TextEditorType.Unknown);
			GBCustomsDataRegistry.Instance.McpDestin8Url = "https://edi.destin8.co.uk";
			AssertNoExceptionThrown(() => credential.CheckCredentialsAgainstCspWebService(notifier));
			AssertContains("CSP confirmed that the credentials are known, and that the topic is registered to: \r\nUNH...", notifier.LastMessage.Text);

			GBCustomsDataRegistry.Instance.McpDestin8Url = "ftp://edi.destin8.co.uk";
			AssertNoExceptionThrown(() => credential.CheckCredentialsAgainstCspWebService(notifier));
			AssertContains("CSP confirmed that the credentials are known, and that the topic is registered to: \r\nUNH...", notifier.LastMessage.Text);

			GBCustomsDataRegistry.Instance.McpDestin8Url = "file://edi.destin8.co.uk";
			AssertNoExceptionThrown(() => credential.CheckCredentialsAgainstCspWebService(notifier));
			AssertContains("CSP confirmed that the credentials are known, and that the topic is registered to: \r\nUNH...", notifier.LastMessage.Text);

			GBCustomsDataRegistry.Instance.McpDestin8Url = "edi.destin8.co.uk";
			AssertNoExceptionThrown(() => credential.CheckCredentialsAgainstCspWebService(notifier));
			AssertContains("CSP confirmed that the credentials are known, and that the topic is registered to: \r\nUNH...", notifier.LastMessage.Text);

			GBCustomsDataRegistry.Instance.McpDestin8Url = "destin8";
			AssertNoExceptionThrown(() => credential.CheckCredentialsAgainstCspWebService(notifier));
			AssertContains("CSP confirmed that the credentials are known, and that the topic is registered to: \r\nUNH...", notifier.LastMessage.Text);
			mock.VerifyAll();
		}

		public void TestCheckCredentialsWithInvalidMcpDestin8UrlWithHttp()
		{
			var mock = new Mock<IDestin8WebService>();
			var objectSubstitution = ObjectFactory.Substitute(mock.Object);
			var ediMessageBatch = new EDIMessageBatch();
			var batch = new Batch();
			var listOfPrintMessages = new List<PrintMessage>();
			var printMessage = new PrintMessage();
			printMessage.message = "UNH...";
			listOfPrintMessages.Add(printMessage);
			batch.batchID = 123456;
			batch.messageCount = listOfPrintMessages.Count;
			batch.messages = listOfPrintMessages.ToArray();
			ediMessageBatch.batch = batch;
			ediMessageBatch.errorNo = 0;
			mock.Setup(m => m.checkCdsCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(ediMessageBatch);

			var badge = new BadgeCodeSetting();
			badge.BadgeCode = "DAN";
			badge.CSPCode = "MCP";
			badge.ApplicationCode = "CDS";
			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var credential = new CredentialsSetting();
			credential.BadgeCode = badge.BadgeCode;

			var notifier = UnitTestUserNotification.Instance;

			//change EditorInfo'sTextEditorType to TextEditorType.Unknown to avoid Url validation
			//otherwise there would raise an validation exception while we assign McpDestin8Url a invalid value
			(GBCustomsDataRegistry.Instance.FindByName("GBMcpDestin8Url2023") as StringRegistryItem).EditorInfo = new TextRegistryEditorInfo(TextEditorType.Unknown);
			(GBCustomsDataRegistry.Instance.FindByName("GBMcpDestin8UrlTest2023") as StringRegistryItem).EditorInfo = new TextRegistryEditorInfo(TextEditorType.Unknown);
			GBCustomsDataRegistry.Instance.McpDestin8Url = "http://edi.destin8.co.uk";
			AssertNoExceptionThrown(() => credential.CheckCredentialsAgainstCspWebService(notifier));
			AssertContains("CSP confirmed that the credentials are known, and that the topic is registered to: \r\nUNH...", notifier.LastMessage.Text);

			GBCustomsDataRegistry.Instance.McpDestin8Url = "ftp://edi.destin8.co.uk";
			AssertNoExceptionThrown(() => credential.CheckCredentialsAgainstCspWebService(notifier));
			AssertContains("CSP confirmed that the credentials are known, and that the topic is registered to: \r\nUNH...", notifier.LastMessage.Text);

			GBCustomsDataRegistry.Instance.McpDestin8Url = "file://edi.destin8.co.uk";
			AssertNoExceptionThrown(() => credential.CheckCredentialsAgainstCspWebService(notifier));
			AssertContains("CSP confirmed that the credentials are known, and that the topic is registered to: \r\nUNH...", notifier.LastMessage.Text);

			GBCustomsDataRegistry.Instance.McpDestin8Url = "edi.destin8.co.uk";
			AssertNoExceptionThrown(() => credential.CheckCredentialsAgainstCspWebService(notifier));
			AssertContains("CSP confirmed that the credentials are known, and that the topic is registered to: \r\nUNH...", notifier.LastMessage.Text);

			GBCustomsDataRegistry.Instance.McpDestin8Url = "destin8";
			AssertNoExceptionThrown(() => credential.CheckCredentialsAgainstCspWebService(notifier));
			AssertContains("CSP confirmed that the credentials are known, and that the topic is registered to: \r\nUNH...", notifier.LastMessage.Text);
			mock.VerifyAll();
		}
	}
}
