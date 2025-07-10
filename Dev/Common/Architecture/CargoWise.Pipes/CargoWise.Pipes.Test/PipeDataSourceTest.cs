using NUnit.Framework;

namespace CargoWise.Pipes.Test
{
	class PipeDataSourceTest : TestCase
	{
		public void TestOutputType()
		{
			var pipe = new PipeDataSource<int>("");
			AssertEquals(typeof(int), ((IPipeDataSource)pipe).Output);
		}
	}
}
