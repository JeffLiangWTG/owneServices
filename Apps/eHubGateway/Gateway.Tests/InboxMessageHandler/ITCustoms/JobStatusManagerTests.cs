using System.Collections.Generic;
using System.Linq;
using CargoWise.eHub.Gateway.ITCustoms;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler.ITCustoms
{
	[TestClass]
	public class JobStatusManagerTests
	{
		private JobStatusManager _manager;
		private eHubTransactionsContext _context;
		private TestDbSet<eHubITCustomsJobStatus> _testDbSet;

		[TestInitialize]
		public void Setup()
		{
			_manager = MockRepository.GeneratePartialMock<JobStatusManager>();
			_context = MockRepository.GenerateMock<eHubTransactionsContext>();
			_testDbSet = new TestDbSet<eHubITCustomsJobStatus>();
			JobStatusManager.DbContext = () => _context;

			var clientSystem = new eHubClientSystem { EH_ID = "ClientSysId" };
			var fileNameList = new List<string>
			{
				"4UDG0927.R0A",
				"4UDG0927.U0A",
				"4UDG0927.X0A",
				"4UDG0927.Q0A",
				"4UDG0927.L0A",

				// Invalid file type
				"4UDG0927.00A",
				"4UDG0927.A0A",

				// different prefix
				"XYZ00001.R0A",
				"ZYX00001.R0A",
				"YXZ00002.R0A",

				// different suffix
				"4UDG0927.RAA",
				"4UDG0927.R01",
				"4UDG0927.R1A",
			};

			_context.Stub(x => x.eHubITCustomsJobStatuses).Return(_testDbSet);
			_context.eHubITCustomsJobStatuses.AddRange(fileNameList.Select(x => new eHubITCustomsJobStatus
			{
				IT_FileName = x,
				eHubClientSystem = clientSystem
			}));
		}

		[TestMethod]
		public void TestTriggerJobStatus_ExpectedJobStatuesAreMatched()
		{
			List<eHubITCustomsJobStatus> capturedJobStatus = null;
			_manager.Stub(x => x.TriggerPollingStartUTC(Arg<List<eHubITCustomsJobStatus>>.Is.Anything))
				.WhenCalled(call =>
				{
					capturedJobStatus = (List<eHubITCustomsJobStatus>)call.Arguments.FirstOrDefault();
				});

			_manager.TriggerJobStatus("ClientSysId", new Files { File = new []
			{
				new File { Name = "4UDG0927.R0A" }
			}});

			Assert.IsNotNull(capturedJobStatus);
			Assert.AreEqual(4, capturedJobStatus.Count);
			Assert.IsTrue(capturedJobStatus.Any(x => x.IT_FileName == "4UDG0927.U0A"));
			Assert.IsTrue(capturedJobStatus.Any(x => x.IT_FileName == "4UDG0927.X0A"));
			Assert.IsTrue(capturedJobStatus.Any(x => x.IT_FileName == "4UDG0927.Q0A"));
			Assert.IsTrue(capturedJobStatus.Any(x => x.IT_FileName == "4UDG0927.L0A"));
		}

		[TestMethod]
		public void TestTriggerJobStatus_NoMatch_InvalidInputFileType()
		{
			List<eHubITCustomsJobStatus> capturedJobStatus = null;
			_manager.Stub(x => x.TriggerPollingStartUTC(Arg<List<eHubITCustomsJobStatus>>.Is.Anything))
				.WhenCalled(call =>
				{
					capturedJobStatus = (List<eHubITCustomsJobStatus>)call.Arguments.FirstOrDefault();
				});

			_manager.TriggerJobStatus("ClientSysId", new Files
			{
				File = new[]
				{
					new File { Name = "4UDG0927.B0A" }
				}
			});

			Assert.IsNull(capturedJobStatus);
		}

		[TestMethod]
		public void TestTriggerPollingStartUTC_UpdateITLastStatus()
		{
			var clientSystem = new eHubClientSystem { EH_ID = "ClientSysId" };
			var fileNameList = new List<string>
			{
				"4UDG0927.R0A",
				"4UDG0927.U0A",
				"4UDG0927.X0A",
				"4UDG0927.Q0A",
				"4UDG0927.L0A",
			};

			var jobStatus = fileNameList.Select(x => new eHubITCustomsJobStatus
			{
				IT_FileName = x,
				eHubClientSystem = clientSystem,
				IT_LastStatus = "Init",
			}).ToList();

			_manager.TriggerPollingStartUTC(jobStatus);

			Assert.IsNotNull(jobStatus);
			Assert.AreEqual(5, jobStatus.Count);
			Assert.IsNull(jobStatus.Single(x => x.IT_FileName == "4UDG0927.U0A").IT_LastStatus);
			Assert.IsNull(jobStatus.Single(x => x.IT_FileName == "4UDG0927.X0A").IT_LastStatus);
			Assert.AreEqual("NR ", jobStatus.Single(x => x.IT_FileName == "4UDG0927.Q0A").IT_LastStatus);
			Assert.AreEqual("NR ", jobStatus.Single(x => x.IT_FileName == "4UDG0927.L0A").IT_LastStatus);
			Assert.AreEqual("Init", jobStatus.Single(x => x.IT_FileName == "4UDG0927.R0A").IT_LastStatus);
		}
	}
}
