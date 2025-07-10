using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	sealed class CurrentDatDbBackupPrefixTest : TestCase
	{
		public void TestGetValue()
		{
			AssertNotNullOrEmpty("CurrentDatDbBackupPrefix.GetValue()", CurrentDatDbBackupPrefix.GetValue());
		}
	}
}
