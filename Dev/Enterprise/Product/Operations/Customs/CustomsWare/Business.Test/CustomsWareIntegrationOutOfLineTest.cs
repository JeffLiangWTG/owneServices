using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	class CustomsWareIntegrationOutOfLineTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSubmitSucceeded_True()
		{
			var submissionResult = new XElement("QueueForSubmission", new XElement("StatusCode", "0"));
			NUnit.Framework.Assert.That(Integration.SubmitSucceeded(submissionResult), Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestSubmitSucceeded_FalseIfMissingStatusCode()
		{
			var submissionResult = new XElement("QueueForSubmission");
			NUnit.Framework.Assert.That(Integration.SubmitSucceeded(submissionResult), Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestSubmitSucceeded_FalseIfStatusCodeIsNot0()
		{
			var submissionResult = new XElement("QueueForSubmission", new XElement("StatusCode", "1"));
			NUnit.Framework.Assert.That(Integration.SubmitSucceeded(submissionResult), Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestGetSubmitFailedReasons()
		{
			var submissionResult = new XElement("ErrorItem", new XElement("ErrorDescription", "Exception"));
			NUnit.Framework.Assert.That(Integration.GetSubmitFailedReasons(submissionResult), Is.EqualTo("\nError Description: Exception\n").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestExecute()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var result = new CustomsWareIntegrationOutOfLineForTest().Execute(declaration);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(result, Is.EqualTo("Queue for Submission Succeeded.").Using(CustomComparers.TypeComparison), "Queue for Submission Succeeded.");
				NUnit.Framework.Assert.That(declaration.JE_EntryStatus, Is.EqualTo(CustomsWareEntryStatusList.Codes.Queued).Using(CustomComparers.TypeComparison), "JE_EntryStatus");
				AssertSubmissionMessage(declaration.DiscardedMessages[0]);
				NUnit.Framework.Assert.That(declaration.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.CustomsCommencedCode || x.SL_SE_NKEvent == AutoEvents.ExportCustomsCommencedCode).Any(), Is.EqualTo(false), "No Customs Commenced Event");
				NUnit.Framework.Assert.That(declaration.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DataExportCode).Any(), Is.EqualTo(false), "No Data Export Event");
			}

			);
		}

		CustomsWareIntegrationOutOfLine Integration => integration ?? (integration = new CustomsWareIntegrationOutOfLine());
		CustomsWareIntegrationOutOfLine integration;
		[ExpectNoExceptions]
		static void AssertSubmissionMessage(EDIMessage message)
		{
			NUnit.Framework.Assert.That(message.EM_MessageType, Is.EqualTo(ApplicationCodeList.Codes.CustomsWare).Using(CustomComparers.TypeComparison), "EM_MessageType");
			NUnit.Framework.Assert.That(message.EM_ApplicationCode, Is.EqualTo(ApplicationCodeList.Codes.CustomsWare).Using(CustomComparers.TypeComparison), "EM_ApplicationCode");
			NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(EDIMessageStatusList.Codes.Queued).Using(CustomComparers.TypeComparison), "EM_Status");
		}
	}
}
