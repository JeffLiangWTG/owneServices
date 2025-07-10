using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices;
using Moq;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	class MessageProcessingLoggingTest : TestCaseWithFactory
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			eAdaptorRegistry.Instance.AllowParallelUMI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.ParallelUMIQueueHistoryInHours.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 24);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		static void EnableLog(string key)
		{
			var pairs = eAdaptorRegistry.Instance.ExtendedUniversalLogging.Value;
			pairs.Set(key, true);
			eAdaptorRegistry.Instance.ExtendedUniversalLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pairs);
		}

		(EDIMessage[] messages, string logs) RunServiceTasks()
		{
			var messages = Enumerable.Range(0, 12).Select(i => UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(Factory, i)).ToArray();
			Factory.Save();
			var z = 0;
			foreach (var m in messages)
			{
				m.EM_SystemCreateTimeUtc = m.EM_SystemCreateTimeUtc.AddMinutes(z++);
			}
			Factory.Save();

			var serviceTask1 = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (var manager1 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen))
			using (var manager2 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.Flipper))
			using (var manager3 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.Worker))
			{
				manager1.ExecuteBatch();
				manager2.FlipGrEngine();
				manager3.ExecuteBatch();
				var logs = string.Join(System.Environment.NewLine, manager1.Logger.UserLogStrings.Cast<string>().Concat(manager2.Logger.UserLogStrings.Cast<string>()).Concat(manager3.Logger.UserLogStrings.Cast<string>()));

				return (messages, logs);
			}
		}

		#endregion

		public void TestUMIFirstMessageLoad()
		{
			EnableLog(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMIFirstMessageLoad);
			var (messages, logs) = RunServiceTasks();
			AssertContains(FormattableString.Invariant($@"Loaded messages for the first time: {string.Join(", ", messages.Select(s => s.EM_MessageNum))}"), logs);
		}

		public void TestUMIMessagesAtFront()
		{
			EnableLog(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMIMessageAtFront);
			var (messages, logs) = RunServiceTasks();
			AssertContains(FormattableString.Invariant($@"Messages at front of queue: {string.Join(", ", messages.Select(m => m.EM_MessageNum))}"), logs);
		}

		public void TestUMIChainStatistics()
		{
			EnableLog(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMIChainStatistics);
			var (messages, logs) = RunServiceTasks();
			AssertContains("Number of chains: 0", logs);
			AssertContains("Chain Lengths:", logs);
		}

		public void TestUMIShowOldest()
		{
			EnableLog(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMIShowOldest);
			var (messages, logs) = RunServiceTasks();
			AssertContains(FormattableString.Invariant($@"Oldest message: {messages[0].EM_MessageNum}"), logs);
		}

		public void TestUMIAllMessageLoads()
		{
			EnableLog(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMIAllLoads);
			var (messages, logs) = RunServiceTasks();
			AssertContains(FormattableString.Invariant($@"Messages loaded (12): {string.Join(", ", messages.Select(s => s.EM_MessageNum))}"), logs);
		}

		public void TestUMKAllMessageLoads()
		{
			EnableLog(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMKAllLoads);
			var (messages, logs) = RunServiceTasks();
			AssertContains(FormattableString.Invariant($@"Messages loaded (12): {string.Join(", ", messages.Select(s => s.EM_MessageNum))}"), logs);
		}

		public void TestUMKLocksTaken()
		{
			EnableLog(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMKLocksTaken);
			var (messages, logs) = RunServiceTasks();
			AssertContains("Lock taken:", logs);
		}

		public void TestUMQAllMessageLoads()
		{
			EnableLog(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMQAllLoads);
			var (messages, logs) = RunServiceTasks();
			AssertContains(FormattableString.Invariant($@"Messages loaded (12): {string.Join(", ", messages.Select(s => s.EM_MessageNum))}"), logs);
		}

		public void TestUMQLocksTaken()
		{
			EnableLog(eAdaptorRegistry.ExtendedUniversalLoggingKeys.UMQLocksTaken);
			var (messages, logs) = RunServiceTasks();
			AssertContains("Locks taken:", logs);
		}
	}
}
