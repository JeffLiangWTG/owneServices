using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class CommonProgramDataTest : TestCase
	{
		public void TestGetCargoWiseDirectory()
		{
			var dir = CommonProgramData.GetCargoWiseDirectory("Mai Stuff", "localhost", "odyssey");
			AssertEndsWith("", @"ProgramData\CargoWise edi\Mai Stuff\localhost\odyssey", dir);
		}
	}
}