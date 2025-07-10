using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Core.Testing
{
	class ValueSetterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGeographyValidation()
		{
			var arbitraryGeography = new ZGeography("POINT (-121 48)");

			CombineAssertions("Setting arbitrary Geography", delegate
			{
				new GeographyColumnValueSetter(bizObj, DummyBizoSchema.Z0_Geography, () => arbitraryGeography, logger).SetValue();
				AssertEquals("bizObj.Z0_Geography", arbitraryGeography, bizObj.Z0_Geography);
				AssertEquals("logger.Logs", "", logger.Logs);
			});
		}

		public void TestDateTimeOffsetValidation()
		{
			var australiaColonised = new ZDateTimeOffset(1788, 1, 26, 0, 0, 0, TimeSpan.FromHours(11));
			var henryViiiDied = new ZDateTimeOffset(1547, 1, 28, 0, 0, 0, TimeSpan.FromHours(11));

			CombineAssertions("Setting date Australia Colonised", delegate
			{
				new DateTimeOffsetColumnValueSetter(bizObj, DummyBizoSchema.Z0_DateTimeOffset, () => australiaColonised, logger).SetValue();
				AssertEquals("bizObj.Z0_DateTimeOffset", australiaColonised, bizObj.Z0_DateTimeOffset);
				AssertEquals("logger.Logs", "", logger.Logs);
			});

			//Unlike DateTime, DateTimeOffset can handle such long ago times.
			CombineAssertions("Setting date Henry VIII died", delegate
			{
				new DateTimeOffsetColumnValueSetter(bizObj, DummyBizoSchema.Z0_DateTimeOffset, () => henryViiiDied, logger).SetValue();
				AssertEquals("bizObj.Z0_DateTimeOffset", henryViiiDied, bizObj.Z0_DateTimeOffset);
				AssertEquals("logger.Logs", "", logger.Logs);
			});
		}

		public void TestDateTimeValidation()
		{
			var australiaColonised = new ZDateTime(1788, 1, 26);
			var henryViiiDied = new ZDateTime(1547, 1, 28);

			CombineAssertions("Setting Brett's Birthday", delegate
			{
				new DateTimeColumnValueSetter(bizObj, DummyBizoSchema.Z0_Date, () => ZDateTime.BrettsBirthday, logger).SetValue();
				AssertEquals("bizObj.Z0_Date", ZDateTime.BrettsBirthday, bizObj.Z0_Date);
				AssertEquals("logger.Logs", "", logger.Logs);
			});

			CombineAssertions("Setting date Australia Colonised", delegate
			{
				new DateTimeColumnValueSetter(bizObj, DummyBizoSchema.Z0_Date, () => australiaColonised, logger).SetValue();
				AssertEquals("bizObj.Z0_Date", australiaColonised, bizObj.Z0_Date);
				AssertEquals("logger.Logs", "", logger.Logs);
			});

			CombineAssertions("Setting date Henry VIII died", delegate
			{
				new DateTimeColumnValueSetter(bizObj, DummyBizoSchema.Z0_Date, () => henryViiiDied, logger).SetValue();
				AssertEquals("bizObj.Z0_Date", australiaColonised, bizObj.Z0_Date);
				AssertEquals("logger.Logs", "Warning - Cannot set Column DummyBizo.Z0_Date to '28-Jan-47 00:00:00' - Value is out of range.", logger.Logs);
			});
		}

		public void TestSmallDateTimeValidation()
		{
			var australiaColonised = new ZDateTime(1788, 1, 26);
			var henryViiiDied = new ZDateTime(1547, 1, 28);

			CombineAssertions("Setting Brett's Birthday", delegate
			{
				new DateTimeColumnValueSetter(bizObj, DummyBizoSchema.Z0_SmallDateTime, () => ZDateTime.BrettsBirthday, logger).SetValue();
				AssertEquals("bizObj.Z0_SmallDateTime", ZDateTime.BrettsBirthday, bizObj.Z0_SmallDateTime);
				AssertEquals("logger.Logs", "", logger.Logs);
			});

			CombineAssertions("Setting date Australia Colonised", delegate
			{
				new DateTimeColumnValueSetter(bizObj, DummyBizoSchema.Z0_SmallDateTime, () => australiaColonised, logger).SetValue();
				AssertEquals("bizObj.Z0_SmallDateTime", ZDateTime.BrettsBirthday, bizObj.Z0_SmallDateTime);
				AssertEquals("logger.Logs", "Warning - Cannot set Column DummyBizo.Z0_SmallDateTime to '26-Jan-88 00:00:00' - Value is out of range.", logger.Logs);
			});

			logger.ClearLogs();

			CombineAssertions("Setting date Henry VIII died", delegate
			{
				new DateTimeColumnValueSetter(bizObj, DummyBizoSchema.Z0_SmallDateTime, () => henryViiiDied, logger).SetValue();
				AssertEquals("bizObj.Z0_SmallDateTime", ZDateTime.BrettsBirthday, bizObj.Z0_SmallDateTime);
				AssertEquals("logger.Logs", "Warning - Cannot set Column DummyBizo.Z0_SmallDateTime to '28-Jan-47 00:00:00' - Value is out of range.", logger.Logs);
			});
		}

		public void TestDateValidation()
		{
			var australiaColonised = new ZDateTime(1788, 1, 26);
			var henryViiiDied = new ZDateTime(1547, 1, 28);

			CombineAssertions("Setting Brett's Birthday", delegate
			{
				new DateTimeColumnValueSetter(bizObj, DummyBizoSchema.Z0_DateOnly, () => ZDateTime.BrettsBirthday, logger).SetValue();
				AssertEquals("bizObj.Z0_DateOnly", ZDate.BrettsBirthday, bizObj.Z0_DateOnly);
				AssertEquals("logger.Logs", "", logger.Logs);
			});

			CombineAssertions("Setting date Australia Colonised", delegate
			{
				new DateTimeColumnValueSetter(bizObj, DummyBizoSchema.Z0_DateOnly, () => australiaColonised, logger).SetValue();
				AssertEquals("bizObj.Z0_DateOnly", australiaColonised.Date, bizObj.Z0_DateOnly);
				AssertEquals("logger.Logs", "", logger.Logs);
			});

			logger.ClearLogs();

			CombineAssertions("Setting date Henry VIII died", delegate
			{
				new DateTimeColumnValueSetter(bizObj, DummyBizoSchema.Z0_DateOnly, () => henryViiiDied, logger).SetValue();
				AssertEquals("bizObj.Z0_DateOnly", henryViiiDied.Date, bizObj.Z0_DateOnly);
				AssertEquals("logger.Logs", "", logger.Logs);
			});
		}

		public void TestMatchingKey()
		{
			AssertEquals(ColumnValueSetter.GetKey(bizObj.PK, DummyBizoSchema.Z0_VarBinaryMax), new BinaryColumnValueSetter(bizObj, DummyBizoSchema.Z0_VarBinaryMax, () => ZBlob.Empty, logger).MatchingKey);
			AssertEquals(ColumnValueSetter.GetKey(bizObj.PK, DummyBizoSchema.Z0_BitTrue), new BooleanColumnValueSetter(bizObj, DummyBizoSchema.Z0_BitTrue, () => ZBool.True, logger).MatchingKey);
			AssertEquals(ColumnValueSetter.GetKey(bizObj.PK, DummyBizoSchema.Z0_Byte), new ByteColumnValueSetter(bizObj, DummyBizoSchema.Z0_Byte, () => ZByte.Zero, logger).MatchingKey);
			AssertEquals(ColumnValueSetter.GetKey(bizObj.PK, DummyBizoSchema.Z0_Code), new CodeDataObjectColumnValueSetter(bizObj, DummyBizoSchema.Z0_Code, () => new EntryType() { Code = "IMP" }, logger, CharacterCase.Normal).MatchingKey);
			AssertEquals(ColumnValueSetter.GetKey(bizObj.PK, DummyBizoSchema.Z0_Date), new DateTimeColumnValueSetter(bizObj, DummyBizoSchema.Z0_Date, () => ZDateTime.Empty, logger).MatchingKey);
			AssertEquals(ColumnValueSetter.GetKey(bizObj.PK, DummyBizoSchema.Z0_DateTimeOffset), new DateTimeOffsetColumnValueSetter(bizObj, DummyBizoSchema.Z0_DateTimeOffset, () => ZDateTimeOffset.Empty, logger).MatchingKey);
			AssertEquals(ColumnValueSetter.GetKey(bizObj.PK, DummyBizoSchema.Z0_Decimal), new DecimalColumnValueSetter(bizObj, DummyBizoSchema.Z0_Decimal, () => ZDecimal.Zero, logger).MatchingKey);
			AssertEquals(ColumnValueSetter.GetKey(bizObj.PK, DummyBizoSchema.Z0_Guid), new GuidColumnValueSetter(bizObj, DummyBizoSchema.Z0_Guid, () => ZGuid.Empty, logger).MatchingKey);
			AssertEquals(ColumnValueSetter.GetKey(bizObj.PK, DummyBizoSchema.Z0_Number), new IntColumnValueSetter(bizObj, DummyBizoSchema.Z0_Number, () => ZInt.Zero, logger).MatchingKey);
			AssertEquals(ColumnValueSetter.GetKey(bizObj.PK, DummyBizoSchema.Z0_Long), new LongColumnValueSetter(bizObj, DummyBizoSchema.Z0_Long, () => ZLong.Zero, logger).MatchingKey);
			AssertEquals(ColumnValueSetter.GetKey(bizObj.PK, DummyBizoSchema.Z0_Short), new ShortColumnWithIntValueSetter(bizObj, DummyBizoSchema.Z0_Short, () => ZInt.Zero, logger).MatchingKey);
			AssertEquals(ColumnValueSetter.GetKey(bizObj.PK, DummyBizoSchema.Z0_Short), new ShortColumnValueSetter(bizObj, DummyBizoSchema.Z0_Short, () => ZShort.Zero, logger).MatchingKey);
			AssertEquals(ColumnValueSetter.GetKey(bizObj.PK, DummyBizoSchema.Z0_Code), new StringColumnValueSetter(bizObj, DummyBizoSchema.Z0_Code, () => ZString.Empty, logger).MatchingKey);
		}

		public void TestSetValueZLong()
		{
			var zLong = new ZLong(32);
			bizObj.Z0_Long = ZLong.Zero;
			var setter = new LongColumnValueSetter(bizObj, DummyBizoSchema.Z0_Long, () => zLong, logger);
			CombineAssertions(() =>
			{
				AssertEquals("Not Set", ZLong.Zero, bizObj.Z0_Long);
				setter.SetValue();
				AssertEquals("Set Long Value", zLong, bizObj.Z0_Long);
			});
		}

		public void TestSetValue()
		{
			var blob = new ZBlob(ASCIIEncoding.ASCII.GetBytes("HI"));
			ColumnValueSetter setter = new BinaryColumnValueSetter(bizObj, DummyBizoSchema.Z0_VarBinaryMax, () => blob, logger);
			AssertEquals(ZBlob.Empty, bizObj.Z0_VarBinaryMax);
			setter.SetValue();
			AssertEquals(blob, bizObj.Z0_VarBinaryMax);

			bizObj.Z0_BitFalse = ZBool.False;
			setter = new BooleanColumnValueSetter(bizObj, DummyBizoSchema.Z0_BitFalse, () => ZBool.True, logger);
			AssertEquals(ZBool.False, bizObj.Z0_BitFalse);
			setter.SetValue();
			AssertEquals(ZBool.True, bizObj.Z0_BitFalse);

			var zbyte = new ZByte(23);
			bizObj.Z0_Byte = ZByte.Zero;
			setter = new ByteColumnValueSetter(bizObj, DummyBizoSchema.Z0_Byte, () => zbyte, logger);
			AssertEquals(ZByte.Zero, bizObj.Z0_Byte);
			setter.SetValue();
			AssertEquals(zbyte, bizObj.Z0_Byte);

			setter = new CodeDataObjectColumnValueSetter(bizObj, DummyBizoSchema.Z0_Code, () => new EntryType { Code = "Imp" }, logger, CharacterCase.Normal);
			bizObj.Z0_Code = ZString.Empty;
			setter.SetValue();
			AssertEquals("Imp", bizObj.Z0_Code);

			setter = new CodeDataObjectColumnValueSetter(bizObj, DummyBizoSchema.Z0_Code, () => new EntryType { Code = "Imp" }, logger, CharacterCase.Lower);
			bizObj.Z0_Code = ZString.Empty;
			setter.SetValue();
			AssertEquals("imp", bizObj.Z0_Code);

			setter = new CodeDataObjectColumnValueSetter(bizObj, DummyBizoSchema.Z0_Code, () => new EntryType { Code = "Imp" }, logger, CharacterCase.Upper);
			bizObj.Z0_Code = ZString.Empty;
			setter.SetValue();
			AssertEquals("IMP", bizObj.Z0_Code);

			bizObj.Z0_Date = ZDateTime.Empty;
			setter = new DateTimeColumnValueSetter(bizObj, DummyBizoSchema.Z0_Date, () => ZDateTime.BrettsBirthday, logger);
			AssertEquals(ZDateTime.Empty, bizObj.Z0_Date);
			setter.SetValue();
			AssertEquals(ZDateTime.BrettsBirthday, bizObj.Z0_Date);

			bizObj.Z0_DateTimeOffset = ZDateTimeOffset.Empty;
			var value = ZDateTimeOffset.Today;
			setter = new DateTimeOffsetColumnValueSetter(bizObj, DummyBizoSchema.Z0_DateTimeOffset, () => value, logger);
			AssertEquals(ZDateTimeOffset.Empty, bizObj.Z0_DateTimeOffset);
			setter.SetValue();
			AssertEquals(value, bizObj.Z0_DateTimeOffset);

			bizObj.Z0_Geography = ZGeography.Empty;
			var geoValue = new ZGeography("POINT (121 41)");
			setter = new GeographyColumnValueSetter(bizObj, DummyBizoSchema.Z0_Geography, () => geoValue, logger);
			AssertEquals(ZGeography.Empty, bizObj.Z0_Geography);
			setter.SetValue();
			AssertEquals(geoValue, bizObj.Z0_Geography);

			var zdecimal = new ZDecimal(123.2m);
			bizObj.Z0_AnotherDecimal = ZDecimal.Zero;
			setter = new DecimalColumnValueSetter(bizObj, DummyBizoSchema.Z0_AnotherDecimal, () => zdecimal, logger);
			AssertEquals(ZDecimal.Zero, bizObj.Z0_AnotherDecimal);
			setter.SetValue();
			AssertEquals(zdecimal, bizObj.Z0_AnotherDecimal);

			var zGuid = ZGuid.NewZGuid();
			bizObj.Z0_Guid = ZGuid.Empty;
			setter = new GuidColumnValueSetter(bizObj, DummyBizoSchema.Z0_Guid, () => zGuid, logger);
			AssertEquals(ZGuid.Empty, bizObj.Z0_Guid);
			setter.SetValue();
			AssertEquals(zGuid, bizObj.Z0_Guid);

			var zInt = new ZInt(32);
			bizObj.Z0_Number = ZInt.Zero;
			setter = new IntColumnValueSetter(bizObj, DummyBizoSchema.Z0_Number, () => zInt, logger);
			AssertEquals(ZInt.Zero, bizObj.Z0_Number);
			setter.SetValue();
			AssertEquals(zInt, bizObj.Z0_Number);

			bizObj.Z0_Short = ZShort.Zero;
			setter = new ShortColumnWithIntValueSetter(bizObj, DummyBizoSchema.Z0_Short, () => zInt, logger);
			AssertEquals(ZShort.Zero, bizObj.Z0_Short);
			setter.SetValue();
			AssertEquals(new ZShort(32), bizObj.Z0_Short);

			bizObj.Z0_Short = new ZShort(32);
			setter = new ShortColumnValueSetter(bizObj, DummyBizoSchema.Z0_Short, () => new ZShort(45), logger);
			AssertEquals(new ZShort(32), bizObj.Z0_Short);
			setter.SetValue();
			AssertEquals(new ZShort(45), bizObj.Z0_Short);

			bizObj.Z0_FK_Code = ZString.Empty;
			setter = new StringColumnValueSetter(bizObj, DummyBizoSchema.Z0_FK_Code, () => "HID", logger);
			AssertEquals(ZString.Empty, bizObj.Z0_FK_Code);
			setter.SetValue();
			AssertEquals("HID", bizObj.Z0_FK_Code);

			// Using GlbStaff as DummyBizO currently has no date only columns
			var glbStaff = Factory.New<GlbStaff>();
			new DateColumnValueSetter(glbStaff, GlbStaffSchema.GS_Birthdate, () => ZDate.BrettsBirthday, logger).SetValue();
			AssertEquals(ZDateTime.BrettsBirthday, glbStaff.GS_Birthdate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
			bizObj = Factory.New<DummyBusinessObject>();
		}
		TestErrorLogger logger;
		DummyBusinessObject bizObj;
	}
}
