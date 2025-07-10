using System;
using System.IO;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.TrustedMessaging.Models;
using static WTG.TrustedMessaging.Constants;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BaseTermsAgreementTest : TransactionedTestCase
	{
		public void TestSecretKeyStorageCached()
		{
			var termsAgreement = new BaseTermsAgreementForTest();
			termsAgreement.IsLoadSucceed = false;
			termsAgreement.ParentConditionSatisfied = true;
			AssertEquals(false, termsAgreement.IsDisplayConditionSatisfied().GetAwaiter().GetResult());
		}

		public void TestIsDisplayConditionSatisfied()
		{
			var termsAgreement = new BaseTermsAgreementForTest();
			termsAgreement.ParentConditionSatisfied = false;
			termsAgreement.LocalDisplaySatisfied = false;
			AssertEquals(false, termsAgreement.IsDisplayConditionSatisfied().GetAwaiter().GetResult());
			termsAgreement.ParentConditionSatisfied = false;
			termsAgreement.LocalDisplaySatisfied = true;
			AssertEquals(false, termsAgreement.IsDisplayConditionSatisfied().GetAwaiter().GetResult());
			termsAgreement.ParentConditionSatisfied = true;
			termsAgreement.LocalDisplaySatisfied = false;
			AssertEquals(false, termsAgreement.IsDisplayConditionSatisfied().GetAwaiter().GetResult());
			termsAgreement.ParentConditionSatisfied = true;
			termsAgreement.LocalDisplaySatisfied = true;
			AssertEquals(true, termsAgreement.IsDisplayConditionSatisfied().GetAwaiter().GetResult());
		}

		public void TestShowErrorMessage()
		{
			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var termsAgreement = new BaseTermsAgreementForTest();
			termsAgreement.ResponseErrorMessage_Exposed = new ErrorMessage() { Code = "404", Message = "Congratulations, you broke the Internet." };
			termsAgreement.ShowTermsFetchError_Exposed();
			AssertEquals("There was a connection error, please try again later. (Error Code:404)", UnitTestUserNotification.Instance.LastMessage.Text);
			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			termsAgreement.ResponseErrorMessage_Exposed = null;
			termsAgreement.ShowTermsFetchError_Exposed();
			AssertEquals("There was a connection error, please try again later.", UnitTestUserNotification.Instance.LastMessage.Text);
			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestReportServerSideErrorSilently()
		{
			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var termsAgreement = new BaseTermsAgreementForTest();
			termsAgreement.ReportServerSideErrorSilently_Exposed(new ErrorMessages(ErrorCodes.TaskCanceled, "ErrorCodes.TaskCanceled!!!").Messages, null);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			termsAgreement.ReportServerSideErrorSilently_Exposed(new ErrorMessages(ErrorCodes.InternalServerError, "ErrorCodes.InternalServerError!!!").Messages, null);
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Server side or network problems, for details please see the error", ExceptionReporterTestListener.Instance[0].Message);
			AssertEquals("ErrorCode:500,ErrorMessage:ErrorCodes.InternalServerError!!!,InnerException:null.\r\n", ExceptionReporterTestListener.Instance[0].InnerException.Message);

			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			termsAgreement.ReportServerSideErrorSilently_Exposed(new ErrorMessages(ErrorCodes.HttpRequestFailed, "ErrorCodes.HttpRequestFailed!!!").Messages, null);
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Server side or network problems, for details please see the error", ExceptionReporterTestListener.Instance[0].Message);
			AssertEquals("ErrorCode:504,ErrorMessage:ErrorCodes.HttpRequestFailed!!!,InnerException:null.\r\n", ExceptionReporterTestListener.Instance[0].InnerException.Message);

			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			termsAgreement = new BaseTermsAgreementForTest();
			termsAgreement.ReportServerSideErrorSilently_Exposed(new ErrorMessages(ErrorCodes.UnhandledExceptionThrown, "UnhandledExceptionThrown!!!").Messages, new Exception("UnhandledExceptionThrown!!!"));
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Server side or network problems, for details please see the error", ExceptionReporterTestListener.Instance[0].Message);
			AssertEquals("ErrorCode:502,ErrorMessage:UnhandledExceptionThrown!!!,InnerException:UnhandledExceptionThrown!!!.\r\n", ExceptionReporterTestListener.Instance[0].InnerException.Message);

			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			termsAgreement = new BaseTermsAgreementForTest();
			termsAgreement.ReportServerSideErrorSilently_Exposed(new ErrorMessages(ErrorCodes.UnhandledExceptionThrown, "DatabaseUpgradeInProgressException!!!").Messages, new DatabaseUpgradeInProgressException());
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			SqlException sqlEx = null;
			try
			{
#pragma warning disable CW1116 // Use CargoWise.Data.Db.Connection
				var conn = new SqlConnection($@"Data Source=.;Database=GUARANTEED_TO_FAIL_{ZGuid.NewZGuid()};Connection Timeout=1"); // Test code only, we want to simulate method call failing with SqlException.
#pragma warning restore CW1116 // Use CargoWise.Data.Db.Connection
				conn.Open();
			}
			catch (SqlException ex)
			{
				sqlEx = ex;
			}
			termsAgreement.ReportServerSideErrorSilently_Exposed(new ErrorMessages(ErrorCodes.UnhandledExceptionThrown, "SqlException!!!").Messages, sqlEx);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			termsAgreement.ReportServerSideErrorSilently_Exposed(new ErrorMessages(ErrorCodes.UnhandledExceptionThrown, "UnauthorizedAccessException!!!").Messages, new UnauthorizedAccessException());
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			termsAgreement.ReportServerSideErrorSilently_Exposed(new ErrorMessages(ErrorCodes.UnhandledExceptionThrown, "DirectoryNotFoundException!!!").Messages, new DirectoryNotFoundException());
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			var jsonEx = new JsonSerializationException("JsonSerializationException!!!", new OutOfMemoryException());
			termsAgreement.ReportServerSideErrorSilently_Exposed(new ErrorMessages(ErrorCodes.UnhandledExceptionThrown, "JsonSerializationException!!!").Messages, jsonEx);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestTryFallbackFormLocal_Failure()
		{
			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var term = new BaseTermsAgreementForTest
			{
				LocalDisplaySatisfied = true,
				IsLoadSucceed = false
			};

			var result = term.IsDisplayConditionSatisfied().GetAwaiter().GetResult();
			AssertEquals(false, result);
			AssertEquals("There was a connection error, please try again later.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			term = new TermsAgreementWithFallback
			{
				LocalDisplaySatisfied = true,
				IsLoadSucceed = false,
				IsFallbackSucceed = false
			};
			result = term.IsDisplayConditionSatisfied().GetAwaiter().GetResult();
			AssertEquals(false, result);
			AssertEquals("There was a connection error, please try again later.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Local fallback setting exception", ExceptionReporterTestListener.Instance[0].Message);

			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestTryFallbackFormLocal_Success()
		{
			var term = new TermsAgreementWithFallback
			{
				LocalDisplaySatisfied = true,
				IsLoadSucceed = false,
				IsFallbackSucceed = true
			};

			CombineAssertions("Pre-Condition: Term's details are empty", () =>
			{
				AssertNullOrEmpty(term.Title);
				AssertNullOrEmpty(term.Contents);
				AssertEquals(0, term.VersionNo);
			});

			var result = term.IsDisplayConditionSatisfied().GetAwaiter().GetResult();

			AssertEquals(true, result);
			AssertEquals("Test Title", term.Title);
			AssertEquals("Test Content", term.Contents);
			AssertEquals(1, term.VersionNo);
		}

		public void TestContentsAsBlob()
		{
			var term = new BaseTermsAgreementForTest();
			term.Contents_Exposed = null;
			AssertEquals("", term.ContentsAsBlob.ToUTF8());
			term.Contents_Exposed = "";
			AssertEquals("", term.ContentsAsBlob.ToUTF8());
			term.Contents_Exposed = "plain text";
			AssertEquals(ORtfTextUtil.TextToRtf("plain text"), term.ContentsAsBlob.ToUTF8());
			term.Contents_Exposed = ORtfTextUtil.TextToRtf("RTF text");
			AssertEquals(ORtfTextUtil.TextToRtf("RTF text"), term.ContentsAsBlob.ToUTF8());
		}

		public void TestShowTermsFetchErrorWithRetry()
		{
			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var term = new TermsAgreementWithFallback
			{
				LocalDisplaySatisfied = true,
				IsLoadSucceed = false,
				IsFallbackSucceed = false
			};

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Retry);
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Retry);
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);

			var result = term.IsDisplayConditionSatisfied().GetAwaiter().GetResult();
			AssertEquals(false, result);

			var previousMessages = UnitTestUserNotification.Instance.PreviousMessages.Where(x => x.Caption != null).Reverse().ToArray();
			AssertEquals(3, previousMessages.Length);
			foreach (var msg in previousMessages)
			{
				AssertEquals("There was a connection error, please try again later.", msg.Text);
				AssertEquals("Network Error", msg.Caption);
			}

			AssertEquals(ZDialogResult.Retry, previousMessages[0].Answer);
			AssertEquals(ZDialogResult.Retry, previousMessages[1].Answer);
			AssertEquals(ZDialogResult.Cancel, previousMessages[2].Answer);

			ExceptionReporterTestListener.Instance.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}
	}
}
