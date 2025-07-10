using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_UPD_HVLVConsignment_GoodsDescriptionVersion))]
	class TG_UPD_HVLVConsignment_GoodsDescriptionVersionTest : DbCreateScriptTest
	{
		public void TestGoodsDescriptionVersion()
		{
			var hvcPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(
$@"DECLARE @OrgAddressPK uniqueidentifier = (SELECT TOP 1 OA_PK FROM dbo.OrgAddress);
DECLARE @HeaderPK uniqueidentifier = NEWID();

INSERT INTO dbo.HVLVBookingHeader
(HVH_PK, HVH_ClusterKey, HVH_BookingReference, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditTimeUtc, HVH_SystemLastEditUser)
VALUES
(@HeaderPK, 1, 'M00001001', @OrgAddressPK, '2016-06-15 00:00:00', 'E', '2016-06-15 00:00:00', 'E');

INSERT INTO dbo.HVLVConsignment
(HVC_PK, HVC_ClusterKey, HVC_HVH_BookingHeader, HVC_ConsignmentId, HVC_ShipperReference, HVC_Status, HVC_SystemCreateTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditTimeUtc, HVC_SystemLastEditUser)
VALUES
('{hvcPK}', 1, @HeaderPK, 'CONSIGN1', 'SHIPREF1', 'CNF', '2016-06-15 00:00:00', 'E', '2016-06-15 00:00:00', 'E');
");
			AssertEquals((short)0, TestConnection.ExecuteScalar<short>($"SELECT HVC_GoodsDescriptionVersion FROM dbo.HVLVConsignment WHERE HVC_PK='{hvcPK}'"));

			TestConnection.ExecuteNonQuery($"UPDATE dbo.HVLVConsignment SET HVC_ConsignmentId='CONSIGN2' WHERE HVC_PK='{hvcPK}'");
			AssertEquals((short)0, TestConnection.ExecuteScalar<short>($"SELECT HVC_GoodsDescriptionVersion FROM dbo.HVLVConsignment WHERE HVC_PK='{hvcPK}'"));

			TestConnection.ExecuteNonQuery($"UPDATE dbo.HVLVConsignment SET HVC_GoodsDescription='test' WHERE HVC_PK='{hvcPK}'");
			AssertEquals((short)1, TestConnection.ExecuteScalar<short>($"SELECT HVC_GoodsDescriptionVersion FROM dbo.HVLVConsignment WHERE HVC_PK='{hvcPK}'"));
		}
	}
}

