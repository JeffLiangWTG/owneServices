using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Testing
{
	[TestedType(typeof(IsLocationPartOfRegion))]
	class IsLocationPartOfRegionTest : DbCreateScriptTest
	{
		public void TestIsLocationPartOfTest()
		{
			AssertEquals("Y", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('AU', 'AU')").ToString());
			AssertEquals("N", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('AU', 'SG')").ToString());
			AssertEquals("N", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('SG', 'AU')").ToString());
			AssertEquals("Y", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('AUBNE', 'AU')").ToString());
			AssertEquals("N", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('AU', 'AUBNE')").ToString());
			AssertEquals("Y", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('AUBNE', 'AUBNE')").ToString());
			AssertEquals("N", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('AUSYD', 'AUBNE')").ToString());
			AssertEquals("Y", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('CN', 'SEAR')").ToString());
			AssertEquals("Y", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('TH', 'SEAR')").ToString());
			AssertEquals("N", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('SEAR', 'CN')").ToString());
			AssertEquals("N", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('SEAR', 'TH')").ToString());
			AssertEquals("Y", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('CNBJS', 'SEAR')").ToString());
			AssertEquals("Y", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('CNSHA', 'SEAR')").ToString());
			AssertEquals("N", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('SEAR', 'CNBJS')").ToString());
			AssertEquals("N", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('SEAR', 'CNSHA')").ToString());
			AssertEquals("Y", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('SEAR', 'SEAR')").ToString());
			AssertEquals("Y", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('XXX', 'XXX')").ToString());
			AssertEquals("N", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('XXXX', 'XXX')").ToString());
			AssertEquals("N", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('XXX', 'XXXX')").ToString());
			AssertEquals("N", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('AU', '')").ToString());
			AssertEquals("N", TestConnection.ExecuteScalar("select dbo.IsLocationPartOfRegion('', 'AU')").ToString());
		}
	}
}

