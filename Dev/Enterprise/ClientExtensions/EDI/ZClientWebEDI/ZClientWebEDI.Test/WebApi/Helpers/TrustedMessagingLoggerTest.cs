using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http.Controllers;
using NLog;
using NUnit.Framework;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	internal class TrustedMessagingLoggerTest : TestCase
	{
		public void TestLogTrustedRequest()
		{
			var logger = new NLogWrapper(typeof(HandshakeController));
			var sessionId = Guid.NewGuid().ToString();
			var request = new TrustedRequest()
			{
				Product = "DDD",
				SystemId = "1234",
			};
			TrustedMessagingLogger.LogTrustedRequest(logger, "ab/cd", sessionId, request);

			var expectedLogMessages = $"Info | HandshakeController: Request received | 200 | {sessionId} |  | ab/cd | DDD | 1234";
			AssertLogMessages(expectedLogMessages);
		}

		public void TestLogBadRequestWithErrorMessage()
		{
			var logger = new NLogWrapper(typeof(HandshakeController));
			var sessionId = Guid.NewGuid().ToString();
			var request = new TrustedRequest()
			{
				Product = "DDD",
				SystemId = "1234",
			};

			var certInfo = new CertificateInfo()
			{
				Product = "DDD",
				SystemId = "1234",
				TenantId = "ABC"
			};

			TrustedMessagingLogger.LogBadRequestWithErrorMessage(logger, "ab/cd", sessionId, request, certInfo, new ErrorMessages("5000", "Validation error"));
			var expectedLogMessages = $"Warn | HandshakeController: 5000 Validation error | 400 | {sessionId} |  | ab/cd | DDD | 1234";
			AssertLogMessages(expectedLogMessages);
			memoryTarget.Logs.Clear();

			var userInfo = new TrustedUserInfo()
			{
				Product = "DDD",
				SystemId = "1234",
				TenantId = "ABC",
				UserId = "UR1"
			};

			TrustedMessagingLogger.LogBadRequestWithErrorMessage(logger, "ab/cd", sessionId, request, userInfo, new ErrorMessages("5000", "Validation error"));
			expectedLogMessages = $"Warn | HandshakeController: 5000 Validation error | 400 | {sessionId} | UR1 | ab/cd | DDD | 1234";
			AssertLogMessages(expectedLogMessages);
			memoryTarget.Logs.Clear();

			TrustedMessagingLogger.LogBadRequestWithErrorMessage(logger, "ab/cd", sessionId, "DDD_Production", new ErrorMessages("5000", "Validation error"));
			expectedLogMessages = $"Warn | HandshakeController: DDD_Production | 5000 Validation error | 400 | {sessionId} |  | ab/cd |  | ";
			AssertLogMessages(expectedLogMessages);
		}

		public void TestLogTrustedMessageDecryptionSuccess()
		{
			var logger = new NLogWrapper(typeof(HandshakeController));
			var sessionId = Guid.NewGuid().ToString();
			var request = new TrustedRequest()
			{
				Product = "DDD",
				SystemId = "1234",
			};

			var certInfo = new CertificateInfo()
			{
				Product = "DDD",
				SystemId = "1234",
				TenantId = "ABC"
			};

			TrustedMessagingLogger.LogTrustedMessageDecryptionSuccess(logger, "ab/cd", sessionId, request, certInfo);
			var expectedLogMessages = $"Info | HandshakeController: Trusted message decrypted | 200 | {sessionId} |  | ab/cd | DDD | 1234";
			AssertLogMessages(expectedLogMessages);
			memoryTarget.Logs.Clear();

			var userInfo = new TrustedUserInfo()
			{
				Product = "DDD",
				SystemId = "1234",
				TenantId = "ABC",
				UserId = "UR1"
			};

			TrustedMessagingLogger.LogTrustedMessageDecryptionSuccess(logger, "ab/cd", sessionId, request, userInfo);
			expectedLogMessages = $"Info | HandshakeController: Trusted message decrypted | 200 | {sessionId} | UR1 | ab/cd | DDD | 1234";
			AssertLogMessages(expectedLogMessages);
		}

		public void TestLogTrustedMessageDecryptionFalure()
		{
			var logger = new NLogWrapper(typeof(HandshakeController));
			var sessionId = Guid.NewGuid().ToString();
			var request = new TrustedRequest()
			{
				Product = "DDD",
				SystemId = "1234",
			};

			TrustedMessagingLogger.LogTrustedMessageDecryptionFalure(logger, "ab/cd", sessionId, request, new ErrorMessages("7000", "System not found"));
			var expectedLogMessages =
@$"Warn | HandshakeController: Trusted message failed to decrypt | 400 | {sessionId} |  | ab/cd | DDD | 1234
Warn | HandshakeController: 7000 System not found | 400 | {sessionId} |  | ab/cd | DDD | 1234";
			AssertLogMessages(expectedLogMessages);
		}

		public void TestLogOkResponse()
		{
			var logger = new NLogWrapper(typeof(HandshakeController));
			var sessionId = Guid.NewGuid().ToString();
			var request = new TrustedRequest()
			{
				Product = "DDD",
				SystemId = "1234",
			};

			var certInfo = new CertificateInfo()
			{
				Product = "DDD",
				SystemId = "1234",
				TenantId = "ABC"
			};

			TrustedMessagingLogger.LogOkResponse(logger, "ab/cd", sessionId, request, certInfo);
			var expectedLogMessages = $"Info | HandshakeController: success | 200 | {sessionId} |  | ab/cd | DDD | 1234";
			AssertLogMessages(expectedLogMessages);
			memoryTarget.Logs.Clear();

			var userInfo = new TrustedUserInfo()
			{
				Product = "DDD",
				SystemId = "1234",
				TenantId = "ABC",
				UserId = "UR1"
			};

			TrustedMessagingLogger.LogOkResponse(logger, "ab/cd", sessionId, request, userInfo, "Data");
			expectedLogMessages = $"Info | HandshakeController: Data | 200 | {sessionId} | UR1 | ab/cd | DDD | 1234";
			AssertLogMessages(expectedLogMessages);
		}

		public void TestControllerWithILogger()
		{
			var controllerNames = new List<string>();
			foreach (var type in typeof(TrustedMessagingLogger).Assembly.GetTypes())
			{
				if (type.GetInterfaces().Contains(typeof(IHttpController)) && !type.IsAbstract && !type.Name.EndsWith("ForTest"))
				{
					var propertyLogger = type.GetProperty("Logger");
					if (null != propertyLogger)
					{
						var controller = Activator.CreateInstance(type);
						var logger = (NLogWrapper)propertyLogger.GetValue(controller);
						AssertNotNull(logger);
						logger.AddLog(LogLevel.Info, "TestControllerWithILogger", ((int)HttpStatusCode.OK));
						controllerNames.Add(type.Name);
					}
				}
			}

			Assert(controllerNames.Contains("TrustedMessagingV1Controller"));
			Assert(controllerNames.Contains("ERequestV1Controller"));
		}

		void AssertLogMessages(string expectedLogMessages)
		{
			var builder = new StringBuilder();
			foreach (string log in memoryTarget.Logs)
			{
				builder.AppendLine(log);
			}
			var actualMessages = builder.ToString().TrimEnd('\r', '\n');
			AssertEquals(expectedLogMessages, actualMessages);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var config = new NLog.Config.LoggingConfiguration();
			memoryTarget = new NLog.Targets.MemoryTarget();
			memoryTarget.Layout = "${level} | ${logger}: ${message} | ${event-properties:status_code} | ${event-properties:session_id} | ${event-properties:user_id} | ${event-properties:routing_path} | ${event-properties:product} | ${event-properties:system_id}";
			config.AddRuleForAllLevels(memoryTarget);
			LogManager.Configuration = config;
		}

		NLog.Targets.MemoryTarget memoryTarget;
	}
}
