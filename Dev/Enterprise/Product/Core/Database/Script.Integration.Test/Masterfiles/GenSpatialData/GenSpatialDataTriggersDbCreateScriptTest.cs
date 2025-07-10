using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.GenSpatialData.Testing
{
	abstract class GenSpatialDataTriggersDbCreateScriptTest : TransactionedTestCase
	{
		public abstract string ParentTableCode { get; }

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public abstract void TestTrigger();

		protected void AssertAfterTriggerEvents(Guid pk, string columnName, int expectedCount, ZGeography expectedData)
		{
			using (var reader = TestConnection.Command(
				string.Format("select SPD_Geography from dbo.GenSpatialData where SPD_ParentTableCode = '{0}' and SPD_ParentID = '{1}' and SPD_Column = '{2}'", ParentTableCode, pk, columnName))
				.ExecuteReader())
			{
				var count = 0;
				while (reader.Read())
				{
					if (expectedCount == 0)
					{
						Fail("1 or more DB records found but expecting none.");
					}
					AssertEquals(expectedData, new ZGeography(reader[0].ToString()));
					count++;
				}
				AssertEquals(expectedCount, count);
			}
		}
	}

	internal static class ZGeographyExtensions
	{
		public static string ToPointSqlText(this ZGeography value)
		{
			return value.IsEmpty ? "convert(geography, 'POINT EMPTY')" : string.Format(System.Globalization.CultureInfo.CurrentCulture, "convert(geography, '{0}')", value.AsText());
		}

		public static string ToPolygonSqlText(this ZGeography value)
		{
			return value.IsEmpty ? "convert(geography, 'POLYGON EMPTY')" : string.Format(System.Globalization.CultureInfo.CurrentCulture, "convert(geography, '{0}')", value.AsText());
		}
	}
}
