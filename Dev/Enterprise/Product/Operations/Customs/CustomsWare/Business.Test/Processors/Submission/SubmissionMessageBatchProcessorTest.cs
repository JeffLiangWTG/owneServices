using System.Threading;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.CustomsWare.Services.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	class SubmissionMessageBatchProcessorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestProcess()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var message1 = SubmissionMessageProcessorTest.GetSubmissionMessage(declaration);
			var message2 = SubmissionMessageProcessorTest.GetSubmissionMessage(declaration);
			message2.EM_Status = Messaging.Integration.EDIMessageStatusList.Codes.Pending;
			Factory.Save();
			var current = ZDateTime.UtcNow;
			using (CustomsForceWebServiceForTesting.Setup(((binding, address, request) => new XElement("TEST", new XElement("StatusCode", "0")))))
			{
				new SubmissionMessageBatchProcessorForTest(new LoggingInformation()).ProcessMessage(CancellationToken.None);
				CombineAssertions(() =>
				{
					var newFactory = NewFactory();
					AssertMessageProcessed("message1", message1.PK, newFactory, current);
					AssertMessageProcessed("message2", message2.PK, newFactory, current);
				}

				);
			}
		}

		[ExpectNoExceptions]
		public void TestProcess_DifferentBranches()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "TC1";
			var company1Branch = company1.Branches.AddNew();
			company1Branch.GB_Code = "TB1";
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "TC2";
			var company2Branch = company2.Branches.AddNew();
			company2Branch.GB_Code = "TB2";
			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_GB = company1Branch.PK;
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_GB = company2Branch.PK;
			var message1 = SubmissionMessageProcessorTest.GetSubmissionMessage(declaration1);
			message1.EM_GB = declaration1.JE_GB;
			var message2 = SubmissionMessageProcessorTest.GetSubmissionMessage(declaration2);
			message2.EM_GB = declaration2.JE_GB;
			Factory.Save();
			var current = ZDateTime.UtcNow;
			using (CustomsForceWebServiceForTesting.Setup(((binding, address, request) => new XElement("TEST", new XElement("StatusCode", "0")))))
			{
				AssertNoExceptionThrown(() =>
				{
					new SubmissionMessageBatchProcessorForTest(new LoggingInformation()).ProcessMessage(CancellationToken.None);
				}

				);
				CombineAssertions(() =>
				{
					var newFactory = NewFactory();
					AssertMessageProcessed("message1", message1.PK, newFactory, current);
					AssertMessageProcessed("message2", message2.PK, newFactory, current);
				}

				);
			}
		}

		[ExpectNoExceptions]
		static void AssertMessageProcessed(string message, ZGuid pk, BusinessObjectFactory newFactory, ZDateTime current)
		{
			var ediMessage = newFactory.Load<EDIMessage>(pk);
			var status = ediMessage.EM_Status;
			NUnit.Framework.Assert.That((status != Messaging.Integration.EDIMessageStatusList.Codes.Queued && status != Messaging.Integration.EDIMessageStatusList.Codes.Pending) || (status == Messaging.Integration.EDIMessageStatusList.Codes.Pending && ediMessage.EM_HeldUntilDate > current), Is.EqualTo(true), message);
		}
	}
}
