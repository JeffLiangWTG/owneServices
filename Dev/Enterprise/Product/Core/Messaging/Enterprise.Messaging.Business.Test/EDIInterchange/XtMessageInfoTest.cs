using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.Messaging.Business.Testing
{
	sealed class XtMessageInfoTest : TestCaseWithFactory
	{
		public void TestGetMessageAttrDictionaryGlbExternalPasswordHandlesCrytographicExceptionsAndReportsErrorOnNonInternalSystems()
		{
			var glbExternalPassword = Factory.New<GlbExternalPasswordThrowsCryptographicException_ForTest>();
			var interchange = Factory.New<EDIInterchange>();

			interchange.EI_GP = glbExternalPassword.PK;
			interchange.EI_SessionGUID = Guid.Empty;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_BodyText = "BODY";
			interchange.EI_Status = "QUE";
			Factory.Save();

			SharedAssertions(interchange);
		}

		public void TestGetMessageAttrDictionaryEDIInterchangeHandlesCrytographicExceptionsAndReportsErrorOnNonInternalSystems()
		{
			var interchange = Factory.New<EDIInterchangeThrowsCryptographicException_ForTest>();

			interchange.EI_SessionGUID = Guid.Empty;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_BodyText = "BODY";
			interchange.EI_Status = "QUE";
			Factory.Save();

			SharedAssertions(interchange);
		}

		public void SharedAssertions(EDIInterchange interchange)
		{
			var logger = new TestLogger();
			var xTMessageInfo = new XtMessageInfo(interchange, logger);

			AssertExceptionThrown<Exception>("XtMessageInfo class should consume CryptographicException for ErrorReporting, re-throw Exception to be handled during message processing.", () => { var t = xTMessageInfo.XTMessageAttributes; });

			Assert(logger.loggedContents.Count > 0);
			var log = logger.loggedContents[0];
			var expectedString = $"Interchange.MessageTrackingID={Guid.Empty}) failed to attach certificate information to outgoing xT message: Something went wrong!.";
			AssertEquals(expectedString, log.Message);

			using (MockProductRegistration())
			{
				AssertExceptionThrown<Exception>("XtMessageInfo class should consume CryptographicException for ErrorReporting, re-throw Exception to be handled during message processing.", () => { var t = xTMessageInfo.XTMessageAttributes; });
				AssertContains("Assert ErrorReporter is fired for external Systems", expectedString, ErrorReporter.LastMessageReported);
			}
			ErrorReporter.Clear();
		}

		static IDisposable MockProductRegistration()
		{
			var prodKeyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(m => m.IsWiseTechGlobalInternalSystem()).Returns(false);
			productRegistrationMock.Setup(m => m.Key).Returns(prodKeyMock.Object);

			return ObjectFactory.Substitute(productRegistrationMock.Object);
		}
	}

	sealed class GlbExternalPasswordThrowsCryptographicException_ForTest : GlbExternalPassword, IxTMessageAttributeProvider
	{
		public GlbExternalPasswordThrowsCryptographicException_ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public Dictionary<string, string> GetMessageAttrDictionary()
		{
			throw new CryptographicException("Something went wrong!");
		}
	}

	sealed class EDIInterchangeThrowsCryptographicException_ForTest : EDIInterchange, IxTMessageAttributeProvider
	{
		public EDIInterchangeThrowsCryptographicException_ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public Dictionary<string, string> GetMessageAttrDictionary()
		{
			throw new CryptographicException("Something went wrong!");
		}
	}

	sealed class TestLogger : ILogger
	{
		public readonly List<(LogType Type, string Message)> loggedContents = [];

		public void Log(LogType type, string message)
		{
			loggedContents.Add((type, message));
		}

		public void Log(LogType type, string message, Exception ex)
		{
			Log(type, $"{message}:{ex.Message}");
		}
	}
}
