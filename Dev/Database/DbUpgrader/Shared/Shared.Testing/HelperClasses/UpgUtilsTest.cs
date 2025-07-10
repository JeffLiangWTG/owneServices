using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public sealed class UpgUtilsTest : TestCase
	{
		public void TestGetTemplateDbName()
		{
			AssertEquals(UpgUtils.UpgraderPrefix + "NewTemplateDB_" + Db.Connection.CurrentDatabase, UpgUtils.GetTemplateDbName());
			AssertEquals(UpgUtils.UpgraderPrefix + "NewTemplateDB_OdysseyTest", UpgUtils.GetTemplateDbName("OdysseyTest"));
		}
	}
}
