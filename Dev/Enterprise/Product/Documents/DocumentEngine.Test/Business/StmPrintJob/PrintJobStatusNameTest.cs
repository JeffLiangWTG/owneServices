using System;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class PrintJobStatusNameTest : TestCase
	{
		public void TestJobStatusDisplayNameNotEmpty()
		{
			AssertEquals("Queued", PrintJobStatusName.Name("QUE"));
			AssertEquals("Failed", PrintJobStatusName.Name("FAL"));
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), () => { PrintJobStatusName.Name("WWW"); });
		}
	}
}
