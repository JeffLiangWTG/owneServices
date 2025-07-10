using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DbUpgrader
{
	internal static class EdiTransformationTestHelpers
	{
		public static (Guid pk, string code) CreateStaff(Guid? homeBranch, Guid? homeDepartment)
		{
			var pk = Guid.NewGuid();
			var code = FindUniqueCode(GlbStaffSchema.GS_Code);
			var login = FindUniqueCode(GlbStaffSchema.GS_LoginName);
			Db.Connection.ExecuteNonQuery("INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_GB_HomeBranch, GS_GE_HomeDepartment, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@pk, @code, @login, @branch, @department, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", parameters =>
			{
				parameters.AddParameterBasedOnDbColumn("@pk", pk, GlbStaffSchema.PK);
				parameters.AddParameterBasedOnDbColumn("@code", code, GlbStaffSchema.GS_Code);
				parameters.AddParameterBasedOnDbColumn("@login", login, GlbStaffSchema.GS_LoginName);
				parameters.AddParameterBasedOnDbColumn("@branch", (object)homeBranch ?? DBNull.Value, GlbStaffSchema.GS_GB_HomeBranch);
				parameters.AddParameterBasedOnDbColumn("@department", (object)homeDepartment ?? DBNull.Value, GlbStaffSchema.GS_GE_HomeDepartment);
			});

			return (pk, code);
		}

		public static string FindUniqueCode(SchemaStringColumn codeColumn)
		{
			string code;
			do
			{
				code = GenerateRandomString(codeColumn.MaxLength);
			} while (CodeExists(codeColumn, code));

			return code;
		}

		static Random Random => random ?? (random = new Random());
		[ThreadStatic]
		static Random random;

		public static string GenerateRandomString(int length)
		{
			var randomLetters = Enumerable.Range(0, length).Select(_ => (char)Random.Next('A', 'Z' + 1));
			return new string(randomLetters.ToArray());
		}

		static bool CodeExists(SchemaStringColumn codeColumn, string code)
			=> Db.Connection.Exists($"FROM {codeColumn.TableName} WHERE {codeColumn.Name}='{code}'");

		public static IEnumerable<T> ReadQueryResults<T>(string query) where T : ITuple
		{
			var result = new List<T>();
			Db.Connection.ExecuteReader(query, ReadSingleRow);
			return result;

			void ReadSingleRow(IDataRecord record)
			{
				var fields = Enumerable.Range(0, record.FieldCount)
					.Select(fieldIndex => record[fieldIndex] == DBNull.Value ? null : record[fieldIndex])
					.ToArray();

				result.Add((T)Activator.CreateInstance(typeof(T), fields));
			}
		}
	}
}
