using System;
using System.IO;
using NUnit.Framework;

namespace Enterprise.Builder.GenerateDbUpgraderResources.Testing
{
	sealed class ConsoleLoggerTest : TestCase
	{
		public void TestLogLineRaw()
		{
			var oldOut = Console.Out;

			try
			{
				using (var writer = new StringWriter())
				{
					Console.SetOut(writer);

					var logger = new ConsoleLogger();
					logger.LogLineRaw("FooBar");

					AssertEquals("FooBar\r\n", writer.ToString());
				}
			}
			finally
			{
				Console.SetOut(oldOut);
			}
		}
	}
}
