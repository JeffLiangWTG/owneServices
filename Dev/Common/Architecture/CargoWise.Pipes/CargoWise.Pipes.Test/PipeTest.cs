using System;
using NUnit.Framework;

namespace CargoWise.Pipes.Test
{
	class PipeTest : TestCase
	{
		public void TestOutputType()
		{
			var pipe = new Pipe<int>(PipeType.Synchronous, new Func<int>(() => 2));
			AssertEquals(typeof(int), ((IPipe)pipe).Output);
		}
	}
}
