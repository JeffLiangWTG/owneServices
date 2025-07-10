using System;
using System.Linq;
using System.ServiceModel;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.CustomsWare.Services.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	class SubmissionMessageProcessorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestProcess_Succeeded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var message = GetSubmissionMessage(declaration);
			Process(message, () => new XElement("TEST", new XElement("StatusCode", "0")));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(Messaging.Integration.EDIMessageStatusList.Codes.Sent).Using(CustomComparers.TypeComparison), "EM_Status");
				NUnit.Framework.Assert.That(declaration.JE_EntryStatus, Is.EqualTo(CustomsWareEntryStatusList.Codes.Submitted).Using(CustomComparers.TypeComparison), "JE_EntryStatus");
				NUnit.Framework.Assert.That(declaration.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.CustomsCommencedCode || x.SL_SE_NKEvent == AutoEvents.ExportCustomsCommencedCode).Any(), Is.EqualTo(true), "Has Customs Commenced Event");
				NUnit.Framework.Assert.That(declaration.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DataExportCode).Any(), Is.EqualTo(true), "Has Data Export Event");
			}

			);
		}

		[ExpectNoExceptions]
		public void TestProcess_IfTransientError_TimeoutException()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var message = GetSubmissionMessage(declaration);
			var current = ZDateTime.UtcNow;
			Process(message, () => throw new TimeoutException("Timeout"));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(Messaging.Integration.EDIMessageStatusList.Codes.Pending).Using(CustomComparers.TypeComparison), "EM_Status");
				NUnit.Framework.Assert.That(message.EM_HeldUntilDate > current, Is.EqualTo(true), "EM_HeldUntilDate");
			}

			);
		}

		[ExpectNoExceptions]
		public void TestProcess_IfTransientError_CommunicationException()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var message = GetSubmissionMessage(declaration);
			var current = ZDateTime.UtcNow;
			Process(message, () => throw new CommunicationException("Communication"));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(Messaging.Integration.EDIMessageStatusList.Codes.Pending).Using(CustomComparers.TypeComparison), "EM_Status");
				NUnit.Framework.Assert.That(message.EM_HeldUntilDate > current, Is.EqualTo(true), "EM_HeldUntilDate");
			}

			);
		}

		[ExpectNoExceptions]
		public void TestProcess_IfNotTransientError()
		{
			var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			staff.GS_EmailAddress = "Dummy@dummy.com";
			var declaration = Factory.New<BaseJobDeclaration>();
			var message = GetSubmissionMessage(declaration);
			message.EM_SystemCreateUser = staff.GS_Code;
			Process(message, () => throw new Exception("Not Transient Error"));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(ExceptionReporterTestListener.Instance[0].Message, Is.EqualTo("Not Transient Error"), "An error should be thrown");
				ExceptionReporterTestListener.Instance.Clear();
				NUnit.Framework.Assert.That(message.Notes.FindByDescription(InterchangeProviderBase.ProcessingLogDescription)[0].ST_NoteDataAsText, Is.EqualTo("Web Service Submit Failed:\r\nError Description: Not Transient Error\r\n").Using(CustomComparers.TypeComparison), "ST_NoteDataAsText");
				var email = Env.OutgoingMailManager.EmailsCreated.Single();
				AssertEmail(email, "Dummy@dummy.com", $"Submit {declaration.HumanReadableName} Failed", "Web Service Submit Failed:\r\nError Description: Not Transient Error\r\n");
			}

			);
		}

		[ExpectNoExceptions]
		public void TestProcess_IfCannotFindJobDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var message = GetSubmissionMessage(declaration);
			message.EM_LinkedObject = null;
			var logger = new LoggingInformation();
			Process(message, () => new XElement("TEST"), logger: logger);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(Messaging.Integration.EDIMessageStatusList.Codes.Error).Using(CustomComparers.TypeComparison), "EM_Status");
				NUnit.Framework.Assert.That(logger.DebugLogStrings.Contains($"\tUnable to find business object for CustomsWare Submission message (Number:{message.EM_MessageNum}); message status set to ERROR."), Is.EqualTo(true), "Warning");
			}

			);
		}

		void Process(EDIMessage message, Func<XElement> execAPIResult, LoggingInformation logger = null)
		{
			using (CustomsForceWebServiceForTesting.Setup((binding, address, request) => execAPIResult()))
			{
				var processor = new SubmissionMessageProcessor(message, Factory, logger ?? new LoggingInformation());
				processor.Process();
			}
		}

		public static EDIMessage GetSubmissionMessage(BaseJobDeclaration declaration)
		{
			new CustomsWareIntegrationOutOfLineForTest().Execute(declaration);
			return declaration.DiscardedMessages[0];
		}

		[ExpectNoExceptions]
		public static void AssertEmail(EmailDef email, string recipient, string subject, string body)
		{
			NUnit.Framework.Assert.That(email.Recipients.Count, Is.EqualTo(1), "Recipients.Count");
			NUnit.Framework.Assert.That(email.Recipients[0].Email, Is.EqualTo(recipient), "Recipients.Email");
			NUnit.Framework.Assert.That(email.Subject, Is.EqualTo(subject), "Subject");
			NUnit.Framework.Assert.That(email.Body, Is.EqualTo(body), "Body");
		}
	}
}
