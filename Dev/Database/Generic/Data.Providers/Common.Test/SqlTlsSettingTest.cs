using NUnit.Framework;

namespace CargoWise.Data.Providers.Common.Test
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Test Class")]
	public class SqlTlsSettingTests
	{
		[TestCase("abcxyz.db.wisegrid.net", ExpectedResult = "abcxyz.db.wisegrid.net")]
		[TestCase("abcxyzd.db.wisegrid.net", ExpectedResult = "abcxyzd.db.wisegrid.net")]
		[TestCase("abcxyz.wisegrid.net", ExpectedResult = "abcxyz.wisegrid.net")]
		[TestCase("abcxyzd.wisegrid.net", ExpectedResult = "abcxyzd.wisegrid.net")]
		[TestCase("abcxyzd.db.wtg.zone", ExpectedResult = "abcxyzd.db.wtg.zone")]
		[TestCase("ABCD-XYZN-10A.wtg.zone\\INSTANCE10A", ExpectedResult = "ABCD-XYZN-10A.wtg.zone$INSTANCE10A")]
		public string GetRegKeyFromTest(string serverName)
		{
			return SqlTlsSetting.GetRegKeyFrom(serverName);
		}

		[TestCase(null, null, ExpectedResult = false)]
		[TestCase(true, null, ExpectedResult = true)]
		[TestCase(null, true, ExpectedResult = true)]
		[TestCase(true, true, ExpectedResult = true)]
		[TestCase(false, false, ExpectedResult = false)]
		[TestCase(true, false, ExpectedResult = false)]
		[TestCase(false, true, ExpectedResult = true)]
		public bool GetEncryptionSettingTest(bool? globalSetting, bool? customerSetting)
		{
			return SqlTlsSetting.GetEncryptionSetting(globalSetting, customerSetting);
		}
	}
}
