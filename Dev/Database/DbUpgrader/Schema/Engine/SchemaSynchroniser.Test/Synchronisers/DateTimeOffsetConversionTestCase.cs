using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	abstract class DateTimeOffsetConversionTestCase : TestCaseForColumnConversion
	{
		protected abstract IReadOnlyList<Tuple<Guid, string, string>> Rows { get; }

		public void TestAlterColumn()
		{
			// Arrange
			using (var anotherConnection = Db.NewAdminConnection(mockMainDb))
			{
				Rows.ForEach(tuple => anotherConnection.ExecuteNonQuery($"INSERT INTO DataTest (PK, Data) VALUES('{tuple.Item1}', '{tuple.Item2}')"));

				// Act
				SynchroniseColumnsInTransaction();

				// Assert
				foreach (var row in Rows)
				{
					AssertEquals(row.Item3, anotherConnection.ExecuteScalar<string>($"SELECT CAST(Data as varchar(max)) FROM DataTest WHERE PK = '{row.Item1}'"));
				}
			}
		}
	}
}
