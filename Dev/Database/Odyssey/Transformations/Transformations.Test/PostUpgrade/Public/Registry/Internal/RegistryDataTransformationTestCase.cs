using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing
{
	public abstract class RegistryDataTransformationTestCase : DataTransformationTestCase
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Helper = new RegistryTransformationTestHelper();
		}

		protected RegistryTransformationTestHelper Helper;

		public static Guid InsertCompany(string code = null, string name = "McLaren", string country = "UA", string currency = "UAH")
		{
			var companyCode = code;

			if (companyCode == null)
			{
				companyCode = GetUniqueStringValue("GlbCompany", "GC_Code", 3);
			}
			else
			{
				GetUniqueValuesCache("GlbCompany", "GC_Code").Add(companyCode);
			}

			Guid pK = Guid.NewGuid();
			string sQL = @"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_IsReciprocal, GC_IsGSTRegistered) VALUES (@PK, @Code, @Name, @Currency, @Country, @IsReciprocal, @IsGSTRegistered)";
			DbCommand cmd = Db.Connection.Command(sQL);
			cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pK);
			cmd.AddParameter("@Code", SqlDbType.Char, companyCode);
			cmd.AddParameter("@Name", SqlDbType.VarChar, name);
			cmd.AddParameter("@Currency", SqlDbType.VarChar, currency);
			cmd.AddParameter("@Country", SqlDbType.VarChar, country);
			cmd.AddParameter("@IsReciprocal", SqlDbType.Bit, true);
			cmd.AddParameter("@IsGSTRegistered", SqlDbType.Bit, false);
			cmd.ExecuteNonQuery();
			return pK;
		}

		public static Guid InsertBranch(Guid companyPK, string code = null, string name = "McLaren", string port = "UAIEV")
		{
			var branchCode = code;

			if (branchCode == null)
			{
				branchCode = GetUniqueStringValue("GlbBranch", "GB_Code", 3);
			}
			else
			{
				GetUniqueValuesCache("GlbBranch", "GB_Code").Add(branchCode);
			}

			Guid pK = Guid.NewGuid();
			string sQL = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_BranchName, GB_RL_NKHomePort, GB_GC) VALUES (@PK, @Code, @Name, @Port, @CompanyPK)";
			DbCommand cmd = Db.Connection.Command(sQL);
			cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pK);
			cmd.AddParameter("@Code", SqlDbType.Char, branchCode);
			cmd.AddParameter("@Name", SqlDbType.VarChar, name);
			cmd.AddParameter("@Port", SqlDbType.VarChar, port);
			cmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
			cmd.ExecuteNonQuery();
			return pK;
		}

		static string GetUniqueStringValue(string table, string column, int length)
		{
			var usedValues = GetUniqueValuesCache(table, column);
			var newValue = GenerateString(length);

			while (usedValues.Contains(newValue))
			{
				newValue = GenerateString(length);
			}

			usedValues.Add(newValue);

			return newValue;
		}

		static List<string> GetUniqueValuesCache(string table, string column)
		{
			var key = table + column;
			List<string> usedValues;

			if (!usedValuesCache.TryGetValue(key, out usedValues))
			{
				usedValues = new List<string>();
				usedValuesCache[key] = usedValues;

				using (var reader = Db.Connection.Command(string.Format("SELECT {0} FROM {1}", column, table)).ExecuteReader())
				{
					if (reader.Read())
					{
						usedValues.Add(reader[0].ToString());
					}
				}
			}

			return usedValues;
		}

		internal static string GenerateString(int length)
		{
			return new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", length).Select(s => s[random.Next(s.Length)]).ToArray());
		}

		static readonly Random random = new Random();
		static readonly Dictionary<string, List<string>> usedValuesCache = new Dictionary<string, List<string>>();

		#endregion
	}
}
