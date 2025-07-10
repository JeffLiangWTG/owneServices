using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZDataRowTest : TestCaseWithFactory
	{
		public void TestIColumnIndexerMembers()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var row = ((IBusinessObjectInternals)dummy).Row;
			var dataIndexer = (IColumnIndexer)row;
			var columName = DummyBizoSchema.Z0_Bool.Name;
			dataIndexer[columName] = ZBool.True;
			AssertEquals(true, row[columName]);
			AssertEquals(true, dataIndexer[columName]);
			dataIndexer[columName] = false;
			AssertEquals(false, row[columName]);
			AssertEquals(false, dataIndexer[columName]);

			columName = DummyBizoSchema.Z0_Byte.Name;
			dataIndexer[columName] = (ZByte)110;
			AssertEquals((byte)110, row[columName]);
			AssertEquals((byte)110, dataIndexer[columName]);
			dataIndexer[columName] = (byte)210;
			AssertEquals((byte)210, row[columName]);
			AssertEquals((byte)210, dataIndexer[columName]);
			dataIndexer[columName] = ZByte.Zero;
			AssertEquals((byte)0, row[columName]);
			AssertEquals((byte)0, dataIndexer[columName]);

			AssertEquals("IsNullable", true, DummyBizoSchema.Z0_Code.IsNullable);
			columName = DummyBizoSchema.Z0_Code.Name;
			dataIndexer[columName] = new ZString("HI");
			AssertEquals("HI", row[columName]);
			AssertEquals("HI", dataIndexer[columName]);
			dataIndexer[columName] = "BYE";
			AssertEquals("BYE", row[columName]);
			AssertEquals("BYE", dataIndexer[columName]);
			dataIndexer[columName] = ZString.Empty;
			AssertEquals(DBNull.Value, row[columName]);
			AssertEquals(DBNull.Value, dataIndexer[columName]);
			dataIndexer[columName] = null;
			AssertEquals(DBNull.Value, row[columName]);
			AssertEquals(DBNull.Value, dataIndexer[columName]);
			dataIndexer[columName] = DBNull.Value;
			AssertEquals(DBNull.Value, row[columName]);
			AssertEquals(DBNull.Value, dataIndexer[columName]);

			AssertEquals("IsNullable", false, DummyBizoSchema.Z0_Description.IsNullable);
			columName = DummyBizoSchema.Z0_Description.Name;
			dataIndexer[columName] = new ZString("HI");
			AssertEquals("HI", row[columName]);
			AssertEquals("HI", dataIndexer[columName]);
			dataIndexer[columName] = "BYE";
			AssertEquals("BYE", row[columName]);
			AssertEquals("BYE", dataIndexer[columName]);
			dataIndexer[columName] = ZString.Empty;
			AssertEquals("", row[columName]);
			AssertEquals("", dataIndexer[columName]);

			columName = DummyBizoSchema.Z0_Date.Name;
			dataIndexer[columName] = new ZDateTime(2012, 4, 5, 10, 35, 53);
			AssertEquals(new DateTime(2012, 4, 5, 10, 35, 53), row[columName]);
			AssertEquals(new DateTime(2012, 4, 5, 10, 35, 53), dataIndexer[columName]);
			dataIndexer[columName] = new DateTime(2011, 5, 6, 11, 25, 43);
			AssertEquals(new DateTime(2011, 5, 6, 11, 25, 43), row[columName]);
			AssertEquals(new DateTime(2011, 5, 6, 11, 25, 43), dataIndexer[columName]);
			dataIndexer[columName] = ZDateTime.Empty;
			AssertEquals(DBNull.Value, row[columName]);
			AssertEquals(DBNull.Value, dataIndexer[columName]);
			dataIndexer[columName] = DateTime.MinValue;
			AssertEquals(DateTime.MinValue, row[columName]);
			AssertEquals(DateTime.MinValue, dataIndexer[columName]);
			dataIndexer[columName] = DBNull.Value;
			AssertEquals(DBNull.Value, row[columName]);
			AssertEquals(DBNull.Value, dataIndexer[columName]);

			columName = DummyBizoSchema.Z0_Decimal.Name;
			dataIndexer[columName] = (ZDecimal)110.15m;
			AssertEquals(110.15m, row[columName]);
			AssertEquals(110.15m, dataIndexer[columName]);
			dataIndexer[columName] = 210.15m;
			AssertEquals(210.15m, row[columName]);
			AssertEquals(210.15m, dataIndexer[columName]);
			dataIndexer[columName] = ZDecimal.Zero;
			AssertEquals(0m, row[columName]);
			AssertEquals(0m, dataIndexer[columName]);

			columName = DummyBizoSchema.Z0_Guid.Name;
			var zGuid = ZGuid.NewZGuid();
			dataIndexer[columName] = zGuid;
			AssertEquals(zGuid.ToGuid(), row[columName]);
			AssertEquals(zGuid.ToGuid(), dataIndexer[columName]);
			var guid = Guid.NewGuid();
			dataIndexer[columName] = guid;
			AssertEquals(guid, row[columName]);
			AssertEquals(guid, dataIndexer[columName]);
			dataIndexer[columName] = ZGuid.Empty;
			AssertEquals(DBNull.Value, row[columName]);
			AssertEquals(DBNull.Value, dataIndexer[columName]);
			dataIndexer[columName] = DBNull.Value;
			AssertEquals(DBNull.Value, row[columName]);
			AssertEquals(DBNull.Value, dataIndexer[columName]);

			columName = DummyBizoSchema.Z0_Number.Name;
			dataIndexer[columName] = (ZInt)110;
			AssertEquals(110, row[columName]);
			AssertEquals(110, dataIndexer[columName]);
			dataIndexer[columName] = 210;
			AssertEquals(210, row[columName]);
			AssertEquals(210, dataIndexer[columName]);
			dataIndexer[columName] = ZInt.Zero;
			AssertEquals(0, row[columName]);
			AssertEquals(0, dataIndexer[columName]);

			columName = DummyBizoSchema.Z0_Short.Name;
			dataIndexer[columName] = (ZShort)110;
			AssertEquals((short)110, row[columName]);
			AssertEquals((short)110, dataIndexer[columName]);
			dataIndexer[columName] = (short)210;
			AssertEquals((short)210, row[columName]);
			AssertEquals((short)210, dataIndexer[columName]);
			dataIndexer[columName] = ZShort.Zero;
			AssertEquals((short)0, row[columName]);
			AssertEquals((short)0, dataIndexer[columName]);

			columName = DummyBizoSchema.Z0_VarBinaryMax.Name;
			var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes("HELLO");
			dataIndexer[columName] = new ZBlob(bytes);
			AssertEquals(bytes, row[columName]);
			AssertEquals(bytes, dataIndexer[columName]);
			bytes = System.Text.ASCIIEncoding.ASCII.GetBytes("GOODBYE");
			dataIndexer[columName] = bytes;
			AssertEquals(bytes, row[columName]);
			AssertEquals(bytes, dataIndexer[columName]);
			dataIndexer[columName] = ZBlob.Empty;
			AssertEquals(DBNull.Value, row[columName]);
			AssertEquals(DBNull.Value, dataIndexer[columName]);
			dataIndexer[columName] = null;
			AssertEquals(DBNull.Value, row[columName]);
			AssertEquals(DBNull.Value, dataIndexer[columName]);
			dataIndexer[columName] = DBNull.Value;
			AssertEquals(DBNull.Value, row[columName]);
			AssertEquals(DBNull.Value, dataIndexer[columName]);
		}
	}
}
