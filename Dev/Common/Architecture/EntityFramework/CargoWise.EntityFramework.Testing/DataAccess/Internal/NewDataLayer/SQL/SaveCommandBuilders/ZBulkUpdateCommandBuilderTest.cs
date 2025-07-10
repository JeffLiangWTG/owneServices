using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZBulkUpdateCommandBuilderTest : ZSqlCommandBuilderTest
	{
		// Look at other tests in C:\Dev\Common\Architecture\EntityFramework.Testing\DataAccess\ZSqlSaverTest.cs

		public override void TestWithDummyRow()
		{
			var factory = new BusinessObjectFactory();
			var bizO = factory.New<DummyBusinessObject>();
			factory.Save();
			var description = "something else";
			bizO.Z0_Description = description;

			factory.Save();
			AssertEquals(0, Db.Connection.ExecuteScalar("select count(*) from dbo.DummyBizO where Z0_Description = @desc", cmd => cmd.AddParameterBasedOnDbColumn("@desc", "Original", DummyBizoSchema.Z0_Description)));
			AssertEquals(1, Db.Connection.ExecuteScalar("select count(*) from dbo.DummyBizO where Z0_Description = @desc", cmd => cmd.AddParameterBasedOnDbColumn("@desc", description, DummyBizoSchema.Z0_Description)));
		}

		IEnumerable<DataRow> CreateDummyRows(int numberOfRows)
		{
			for (var i = 0; i < numberOfRows; i++)
			{
				var row = CreateDummyRow(Guid.NewGuid());
				row.AcceptChanges();
				row[DummyBizoSchema.Z0_Geography.Name] = ZGeography.EmptySqlGeography;
				var bytes = new List<byte>();
				for (var n = 0; n < ZLargeColumnSaver.MaxChunkSize + 1; n++)
				{
					bytes.Add(1);
				}

				row[DummyBizoSchema.Z0_VarBinaryMax.Name] = bytes.ToArray();
				yield return row;
			}
		}

		protected override ZSqlCommandBuilder GetBuilder(DataRow row)
		{
			return new ZBulkUpdateCommandBuilder(CreateDummyRows(1), DummyBizoSchema.Constants.TableName, true, ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(DummyBizoSchema.Constants.TableName));
		}
	}
}
