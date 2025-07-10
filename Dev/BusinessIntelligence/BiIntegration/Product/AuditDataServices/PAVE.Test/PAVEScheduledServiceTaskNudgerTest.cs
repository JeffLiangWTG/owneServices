using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.AuditDataServices.PAVE.Test
{
	[TestedType(typeof(PAVEScheduledServiceTaskNudger))]
	class PAVEScheduledServiceTaskNudgerTest : TestCase
	{
		public void TestShouldNudgeServiceTask()
		{
			var loggerMock = new Mock<ILogger>();
			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
			{
				new PAVEScheduledServiceTaskNudger().Execute(new CancellationToken(), new BusinessObjectFactory(), loggerMock.Object, ZGuid.Empty, string.Empty, parameter: "AAA", ZDateTime.UtcNow);
			}
			serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask("AAA", null), Times.Once);
			loggerMock.Verify(l => l.Log(LogType.Information, "Nudged the AAA service task."), Times.Once);
			Assert(true);
		}
	}
}
