using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.CustomsWare.Services.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	class SubmissionMessageProcessorByMultipleServiceTaskCyclesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSucceeded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var message = SubmissionMessageProcessorTest.GetSubmissionMessage(declaration);
			var count = 0;
			ProcessByMultipleServiceTaskCycles(message, () =>
			{
				count++;
				return new XElement("TEST", new XElement("StatusCode", "0"));
			}

			);
			NUnit.Framework.Assert.That(count, Is.EqualTo(1), "1 call only");
		}

		[ExpectNoExceptions]
		public void TestRetryIfTransientError_TimeoutException()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var message = SubmissionMessageProcessorTest.GetSubmissionMessage(declaration);
			var count = 0;
			ProcessByMultipleServiceTaskCycles(message, () =>
			{
				count++;
				throw new TimeoutException("Timeout");
			}

			);
			NUnit.Framework.Assert.That(count, Is.EqualTo(1 + RetryCount), "1 call + retry count");
		}

		[ExpectNoExceptions]
		public void TestRetryIfTransientError_CommunicationException()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var message = SubmissionMessageProcessorTest.GetSubmissionMessage(declaration);
			var count = 0;
			ProcessByMultipleServiceTaskCycles(message, () =>
			{
				count++;
				throw new CommunicationException("Communication");
			}

			);
			NUnit.Framework.Assert.That(count, Is.EqualTo(1 + RetryCount), "1 call + retry count");
		}

		[ExpectNoExceptions]
		public void TestFailPermanentlyIfNotTransientError()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var message = SubmissionMessageProcessorTest.GetSubmissionMessage(declaration);
			var count = 0;
			ProcessByMultipleServiceTaskCycles(message, () =>
			{
				count++;
				throw new Exception("Not Transient Error");
			}

			);
			NUnit.Framework.Assert.That(count, Is.EqualTo(1), "Only 1 call");
			ExceptionReporterTestListener.Instance.Clear();
		}

		[ExpectNoExceptions]
		public void TestFailed()
		{
			var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			staff.GS_EmailAddress = "Dummy@dummy.com";
			var declaration = Factory.New<BaseJobDeclaration>();
			var message = SubmissionMessageProcessorTest.GetSubmissionMessage(declaration);
			message.EM_SystemCreateUser = staff.GS_Code;
			var count = 0;
			ProcessByMultipleServiceTaskCycles(message, () =>
			{
				count++;
				throw new TimeoutException($"{count} Timeout");
			}

			);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(Messaging.Integration.EDIMessageStatusList.Codes.Failed).Using(CustomComparers.TypeComparison), "EM_Status");
				var notes = message.Notes.FindByDescription(InterchangeProviderBase.ProcessingLogDescription).OrderBy(x => x.ST_CreatedDateUtc).ToArray();
				NUnit.Framework.Assert.That(notes.Length, Is.EqualTo(4), "Notes Count");
				NUnit.Framework.Assert.That(notes[0].ST_NoteDataAsText, Is.EqualTo("Web Service Submit Failed:\r\nError Description: 1 Timeout\r\n").Using(CustomComparers.TypeComparison), "1st ST_NoteDataAsText");
				NUnit.Framework.Assert.That(notes[1].ST_NoteDataAsText, Is.EqualTo("Web Service Submit Failed:\r\nError Description: 2 Timeout\r\n").Using(CustomComparers.TypeComparison), "2nd ST_NoteDataAsText");
				NUnit.Framework.Assert.That(notes[2].ST_NoteDataAsText, Is.EqualTo("Web Service Submit Failed:\r\nError Description: 3 Timeout\r\n").Using(CustomComparers.TypeComparison), "3rd ST_NoteDataAsText");
				NUnit.Framework.Assert.That(notes[3].ST_NoteDataAsText, Is.EqualTo("Web Service Submit Failed:\r\nError Description: 4 Timeout\r\n").Using(CustomComparers.TypeComparison), "4th ST_NoteDataAsText");
				var email = Env.OutgoingMailManager.EmailsCreated.Single();
				SubmissionMessageProcessorTest.AssertEmail(email, "Dummy@dummy.com", $"Submit {declaration.HumanReadableName} Failed", "Web Service Submit Failed:\r\nError Description: 1 Timeout\r\nWeb Service Submit Failed:\r\nError Description: 2 Timeout\r\nWeb Service Submit Failed:\r\nError Description: 3 Timeout\r\nWeb Service Submit Failed:\r\nError Description: 4 Timeout\r\n");
			}

			);
		}

		void ProcessByMultipleServiceTaskCycles(EDIMessage message, Func<XElement> execAPIResult)
		{
			using (CustomsForceWebServiceForTesting.Setup((binding, address, request) => execAPIResult()))
			{
				var logger = new LoggingInformation();
				for (int i = 0; i <= RetryCount; i++)
				{
					var processor = new SubmissionMessageProcessor(message, Factory, logger);
					processor.Process();
					Factory.Save();
					Thread.Sleep(100);
					if (message.EM_Status != Messaging.Integration.EDIMessageStatusList.Codes.Queued && message.EM_Status != Messaging.Integration.EDIMessageStatusList.Codes.Pending)
					{
						break;
					}
				}
			}
		}

		const int RetryCount = 3;
	}
}
