using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Management.SessionLogging;

namespace Enterprise.UniversalDataBuss.Testing.Management
{
	public class TaskLoggerTest : TestCaseWithFactory
	{
		public void TestServiceTaskLoggerDebug()
		{
			var logger = new SimpleLogger();
			var taskLogger = new TaskLoggerForTest(logger);
			taskLogger.ShouldLogForTest = true;

			taskLogger.Log(LogType.Information, "Information");
			taskLogger.Log(LogType.Debug, "Debug");
			taskLogger.Log(LogType.Warning, "Warning");
			taskLogger.Log(LogType.Error, "Error");

			AssertContains("Information", logger.ToString());
			AssertContains("Debug - Debug", logger.ToString());
			AssertContains("Warning - Warning", logger.ToString());
			AssertContains("Error - Error", logger.ToString());
		}

		public void TestServiceTaskLoggerRelease()
		{
			var logger = new SimpleLogger();
			var taskLogger = new TaskLoggerForTest(logger);
			taskLogger.ShouldLogForTest = false;

			taskLogger.Log(LogType.Information, "Information");
			taskLogger.Log(LogType.Debug, "Debug");
			taskLogger.Log(LogType.Warning, "Warning");
			taskLogger.Log(LogType.Error, "Error");

			AssertEquals("Should be zero", 0, taskLogger.Logs.Count());
		}

		class TaskLoggerForTest : TaskLogger
		{
			public TaskLoggerForTest(ISimpleLogger taskLogger) : base(taskLogger) { }

			public bool ShouldLogForTest { get; set; }

			public override bool ShouldLog => ShouldLogForTest;
		}
	}
}
