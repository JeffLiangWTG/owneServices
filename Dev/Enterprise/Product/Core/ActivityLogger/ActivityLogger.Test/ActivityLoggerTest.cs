/*
This test project is created for project Enterprise.ActivityLogger
  Project Enterprise.ActivityLogger is for AnyCPU platform.
  Project Enterprise.ActivityLogger.Test is for AnyCPU platform
	To suppress the MSB3270 warning when compiling this project, the following section is added to Enterprise.ActivityLogger.Test.csproj
		<PropertyGroup>
			<ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>
				None
			</ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>
		</PropertyGroup>
*/

[assembly: System.CLSCompliant(true)]
namespace Enterprise.ActivityLogger.Test
{
	using System.Diagnostics.CodeAnalysis;
	using System.Threading;
	using Enterprise.ActivityLogger;
	using NUnit.Framework;

	public class ActivityLoggerTest : TestCase
	{
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public void TestStartStop()
		{
			if (ActivityLogger.OtherInstanceOfActivityLoggerIsRunning())
			{
				Assert("Other instance already attached syshook. Test cannot be run.", false);
				return;
			}

			try
			{
				var instance = ActivityLogger.Instance;
				instance.ClearTestLog();
				var startResult = instance.Start();
				Assert(instance.Started);
				Assert(instance.GetTestLog(), startResult);
				Thread.Sleep(2000);
				Assert("second call does nothing", instance.Start());
				Assert(instance.Started);
				instance.Stop();
				Assert(!instance.Started);
				Assert(instance.Start());
				Assert(instance.Started);
			}
			finally
			{
				ActivityLogger.Instance.Stop();
			}

			Assert(!ActivityLogger.Instance.Started);
		}
	}
}
