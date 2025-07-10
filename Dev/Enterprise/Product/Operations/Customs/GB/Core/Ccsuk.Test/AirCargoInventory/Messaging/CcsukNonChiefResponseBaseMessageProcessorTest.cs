using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Ccsuk.Declaration;
using Enterprise.Customs.GB.Ccsuk.ServiceTask;
using Moq;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	public abstract class CcsukNonChiefResponseBaseMessageProcessorTest : TestCaseWithFactory
	{
		protected void RunProcessors(bool runSenders = false, bool clearLog = false)
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				if (clearLog)
				{
					log.ClearLog();
				}

				if (runSenders)
				{
					new CcsukInterchangeSender(log).ExecuteBatch();
					new CcsukCDSInterchangeSender(log).ExecuteBatch();
				}

				new CcsukNonChiefResponseBaseMessageProcessor(log).ExecuteBatch();
			}
		}

		protected override void SetUp()
		{
			log = new TestServiceLogger();
		}

		protected TestServiceLogger log { get; private set; }
	}
}
