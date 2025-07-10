using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Audit;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.Audit.Testing
{
	class udf_GetDataBetweenLSNTest : TransactionedTestCase
	{
		public void TestHashValue()
		{
			var expectedHash = "2815EF160FB4AD3C816B2A55D825DFA77FB09DD0209456B17446B6CAC21975C4";

			var scriptToTest = BiAuditScriptHelper.GenerateGetDataBetweenLSNScript("dbo", "GlbStaff");
			var script = scriptToTest.Trim();
			var actualHash = GetHashString(script);

			var message = "Script has been modified. Please update $/Shared/CargoWise.BusinessIntelligence/src/CargoWise.Bi.Shared.Testing/AuditHelperTest.cs to reflect the changes and match the hash string.";

			AssertEquals(message, expectedHash, actualHash);
		}

		public void TestGetPKsBetweenLSN()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			{
				var pk1 = Guid.NewGuid();
				var pk2 = Guid.NewGuid();
				var pk3 = Guid.NewGuid();

				SetupLsnTimeMapping(new byte[] { 0x0 }, new DateTime(2021, 12, 01));
				SetupLsnTimeMapping(new byte[] { 0x1 }, new DateTime(2022, 01, 01));
				SetupLsnTimeMapping(new byte[] { 0x2 }, new DateTime(2022, 02, 01));
				SetupLsnTimeMapping(new byte[] { 0x3 }, new DateTime(2022, 03, 01));

				InsertStaff(new byte[] { 0x1 }, 1, 2201, pk1);
				InsertStaff(new byte[] { 0x2 }, 1, 2202, pk2);
				InsertStaff(new byte[] { 0x3 }, 1, 2203, pk3);

				var pks = GetPKsBetweenLsn(new byte[] { 0x0 }, new byte[] { 0x3 });
				AssertContainsExactElementsInAnyOrder("SP exec must return all PKs.", new[] { pk1, pk2, pk3 }, pks);

				pks = GetPKsBetweenLsn(new byte[] { 0x1 }, new byte[] { 0x3 });
				AssertContainsExactElementsInAnyOrder("SP exec should not include minimum LSN.", new[] { pk2, pk3 }, pks);

				pks = GetPKsBetweenLsn(new byte[] { 0x2 }, new byte[] { 0x3 });
				AssertContainsExactElementsInAnyOrder("SP exec should only include maximum LSN.", new[] { pk3 }, pks);
			}
		}

		public void TestGetPKsBetweenLSNWithUpdateMask()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			{
				var pk1 = Guid.NewGuid();
				var pk2 = Guid.NewGuid();
				var pk3 = Guid.NewGuid();

				SetupLsnTimeMapping(new byte[] { 0x0 }, new DateTime(2021, 12, 01));
				SetupLsnTimeMapping(new byte[] { 0x1 }, new DateTime(2022, 01, 01));
				SetupLsnTimeMapping(new byte[] { 0x2 }, new DateTime(2022, 02, 01));
				SetupLsnTimeMapping(new byte[] { 0x3 }, new DateTime(2022, 03, 01));

				InsertStaffWithUpdateMask(new byte[] { 0x1 }, 1, 2201, updateMask: 1, pk1); // update mask is 0b00000001
				InsertStaffWithUpdateMask(new byte[] { 0x2 }, 1, 2202, updateMask: 2, pk2); // update mask is 0b00000010
				InsertStaffWithUpdateMask(new byte[] { 0x3 }, 1, 2203, updateMask: 3, pk3); // update mask is 0b00000100

				var pks = GetPKsBetweenLsn(new byte[] { 0x0 }, new byte[] { 0x3 }, 1);
				AssertContainsExactElementsInAnyOrder("SP exec must return changes that matches with update mask 0b00000001.", new[] { pk1, pk3 }, pks);

				pks = GetPKsBetweenLsn(new byte[] { 0x0 }, new byte[] { 0x3 }, 2);
				AssertContainsExactElementsInAnyOrder("SP exec must return changes that matches with update mask 0b00000010.", new[] { pk2, pk3 }, pks);

				pks = GetPKsBetweenLsn(new byte[] { 0x0 }, new byte[] { 0x3 }, 3);
				AssertContainsExactElementsInAnyOrder("SP exec must return all changes.", new[] { pk1, pk2, pk3 }, pks);

				pks = GetPKsBetweenLsn(new byte[] { 0x0 }, new byte[] { 0x3 }, 4);
				AssertContainsExactElementsInAnyOrder("SP exec must return any changes.", Array.Empty<Guid>(), pks);
			}
		}

		void SetupLsnTimeMapping(byte[] startLsn, DateTime tranEndTimeUtc)
		{
			var sqlText = "INSERT INTO [biadmin].[LsnTimeMapping] VALUES(@startLsn, @tranEndTimeUtc)";
			using (var cmd = TestConnection.Command(sqlText))
			{
				cmd.AddParameter("@startLsn", SqlDbType.Binary, startLsn);
				cmd.AddParameter("@tranEndTimeUtc", SqlDbType.DateTime, tranEndTimeUtc);
				cmd.ExecuteNonQuery();
			}
		}

		void InsertStaff(byte[] lsn, int operationType, int lsnPeriod, Guid pk)
		{
			var sqlText = "INSERT INTO dbo.GlbStaff (__$start_lsn, __$seqval, __$operation, __$update_mask, __$lsn_period, GS_PK) VALUES (@lsn, 0x0, @opType, 0x0, @lsnPeriod, @pk)";
			using (var cmd = TestConnection.Command(sqlText))
			{
				cmd.AddParameter("@lsn", SqlDbType.Binary, lsn);
				cmd.AddParameter("@opType", SqlDbType.Int, operationType);
				cmd.AddParameter("@lsnPeriod", SqlDbType.SmallInt, lsnPeriod);
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);

				cmd.ExecuteNonQuery();
			}
		}

		void InsertStaffWithUpdateMask(byte[] lsn, int operationType, int lsnPeriod, int updateMask, Guid pk)
		{
			var sqlText = "INSERT INTO dbo.GlbStaff (__$start_lsn, __$seqval, __$operation, __$update_mask, __$lsn_period, GS_PK) VALUES (@lsn, 0x0, @opType, @updateMask, @lsnPeriod, @pk)";
			using (var cmd = TestConnection.Command(sqlText))
			{
				cmd.AddParameter("@lsn", SqlDbType.Binary, lsn);
				cmd.AddParameter("@opType", SqlDbType.Int, operationType);
				cmd.AddParameter("@updateMask", SqlDbType.Int, updateMask);
				cmd.AddParameter("@lsnPeriod", SqlDbType.SmallInt, lsnPeriod);
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);

				cmd.ExecuteNonQuery();
			}
		}

		IEnumerable<Guid> GetPKsBetweenLsn(byte[] minLsn, byte[] maxLsn, int updateMask1 = 0)
		{
			var result = new List<Guid>();
			using (var cmd = TestConnection.Command("SELECT GS_PK FROM biadmin.udf_GetDataBetweenLSN_dbo_GlbStaff(@operation_types, @min_lsn, @max_lsn, @update_mask1, default, default, default)"))
			{
				cmd.CommandType = CommandType.Text;
				cmd.AddParameter("@operation_types", SqlDbType.VarChar, "1");
				cmd.AddParameter("@min_lsn", SqlDbType.Binary, minLsn);
				cmd.AddParameter("@max_lsn", SqlDbType.Binary, maxLsn);
				cmd.AddParameter("@update_mask1", SqlDbType.BigInt, updateMask1);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = reader.GetGuid(0);
						result.Add(pk);
					}
				}
			}
			return result;
		}

		string GetHashString(string inputString)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in GetHash(inputString))
			{
				sb.Append(b.ToString("X2"));
			}

			return sb.ToString();
		}

		byte[] GetHash(string inputString)
		{
			using (HashAlgorithm algorithm = SHA256.Create())
			{
				return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
			}
		}
	}
}

