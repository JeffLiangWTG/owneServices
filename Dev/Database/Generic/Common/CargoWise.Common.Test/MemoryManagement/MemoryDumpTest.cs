using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace CargoWise.Common.MemoryManagement.Testing
{
	class MemoryDumpTest : TestCase
	{
		[SnailTest]
		public void TestCreate()
		{
			var profilerFile = Path.Combine(TestingState.TempPath, "session.prfSession");
			Assert(MemoryDump.Create(profilerFile, Process.GetCurrentProcess()));
			File.Delete(profilerFile);
		}
	}
}