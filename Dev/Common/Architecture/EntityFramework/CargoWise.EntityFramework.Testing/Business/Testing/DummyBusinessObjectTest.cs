using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DummyBusinessObjectTest : TestCaseWithFactory
	{
		public void TestSparseBit_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseBit, ZBool.False, ZBool.True);
		public void TestSparseByte_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseByte, ZByte.Zero, new ZByte(1));
		public void TestSparseShort_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseShort, ZShort.Zero, new ZShort(1));
		public void TestSparseNumber_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseNumber, ZInt.Zero, new ZInt(1));
		public void TestSparseLong_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseLong, ZLong.Zero, new ZLong(1));
		public void TestSparseDecimal_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseDecimal, ZDecimal.Zero, new ZDecimal(1));
		public void TestSparseMoney_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseMoney, ZDecimal.Zero, new ZDecimal(1));
		public void TestSparseDateTime_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseDateTime, ZDateTime.Empty, ZDateTime.Now);
		public void TestSparseDate_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseDate, ZDate.Empty, ZDate.Today);
		public void TestSparseDateTimeOffset_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseDateTimeOffset, ZDateTimeOffset.Empty, ZDateTimeOffset.Today);
		public void TestSparseSmallDateTime_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseSmallDateTime, ZDateTime.Empty, ZDateTime.SmallDateTimeNow);
		public void TestSparseTime_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseTime, ZTime.Empty, new ZTime(0, 1));
		public void TestSparseChar_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseChar, ZString.Empty, new ZString("a"));
		public void TestSparseVarChar_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseVarChar, ZString.Empty, new ZString("a"));
		public void TestSparseNVarChar_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseNVarChar, ZString.Empty, new ZString("a"));
		public void TestSparseGuid_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseGuid, ZGuid.Empty, ZGuid.NewZGuid());
		public void TestSparseVarBinaryMax_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseVarBinaryMax, ZBlob.Empty, new ZBlob(new byte[] { 1, 2, 3, 4 }));
		public void TestSparseXml_DefaultValuesArePersistedAsNull() => AssertDefaultValuesArePersistedAsNull(DummyBizoSchema.Constants.Z0_SparseXml, ZString.Empty, ZString.Format("<Root>0123456789 ღ 0123456789 {0}ღ 0123456789 </Root>", "".PadRight(1024, '1')));

		void AssertDefaultValuesArePersistedAsNull(string schemaColumnName, IZType defaultValue, IZType otherValue)
		{
			var bizo = Factory.New<DummyBusinessObject>();
			bizo[schemaColumnName] = otherValue;
			Factory.Save();

			AssertEquals(otherValue, ((INeedRow)bizo).Row[schemaColumnName]);

			bizo[schemaColumnName] = defaultValue;
			Factory.Save();

			AssertEquals(DBNull.Value, ((INeedRow)bizo).Row[schemaColumnName]);

			var bizoInOtherFactory = Factory.Load<DummyBusinessObject>(bizo.PK);
			AssertEquals(defaultValue, bizo[schemaColumnName]);
			AssertEquals(DBNull.Value, ((INeedRow)bizoInOtherFactory).Row[schemaColumnName]);
		}

		public void TestVarCharMax()
		{
			string bigText = "ABC".PadRight(16000, 'D');
			DummyBusinessObject bizo = Factory.New<DummyBusinessObject>();
			bizo.Z0_VarCharMax = bigText;
			bizo.Z0_NVarCharMax = bigText;
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject bizo2 = factory2.Load<DummyBusinessObject>(bizo.PK);

			AssertEquals(bigText, bizo2.Z0_VarCharMax);
			AssertEquals(bigText, bizo2.Z0_NVarCharMax);
		}

		public void TestVarBinaryMax()
		{
			byte[] bytes = new byte[16000];
			Random random = new Random();
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = (byte)random.Next(255);
			}

			DummyBusinessObject bizo = Factory.New<DummyBusinessObject>();
			bizo.Z0_VarBinaryMax = bytes;
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject bizo2 = factory2.Load<DummyBusinessObject>(bizo.PK);

			AssertEquals(bytes, (byte[])bizo2.Z0_VarBinaryMax);
		}

		public void TestVarBinaryMaxWithShortData()
		{
			byte[] bytes = new byte[16];
			Random random = new Random();
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = (byte)random.Next(255);
			}

			DummyBusinessObject bizo = Factory.New<DummyBusinessObject>();
			bizo.Z0_VarBinaryMax = bytes;
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject bizo2 = factory2.Load<DummyBusinessObject>(bizo.PK);

			AssertEquals(bytes, (byte[])bizo2.Z0_VarBinaryMax);
		}
	}
}
