using System;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Validators;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Xml.Serializers
{
	public class PropertyElementGeneratorTest : TestCase
	{
		public void TestGenerateValue_DateTime()
		{
			AssertEquals("2011-09-19T01:03:02", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.DateTime), new DateTime(2011, 9, 19, 1, 3, 2)));
		}

		public void TestGenerateValue_SmallDateTime()
		{
			AssertEquals("2017-09-19T01:03:02", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.SmallDateTime), new DateTime(2017, 9, 19, 1, 3, 2)));
		}

		public void TestGenerateValue_Time()
		{
			AssertEquals("01:03:02", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.SmallDateTime), new TimeSpan(1, 3, 2)));
		}

		public void TestGenerateValue_Geography()
		{
			AssertEquals("POINT (121 48)", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.Geography), SqlGeography.STGeomFromText(new SqlChars("POINT (121 48)"), 4326)));
		}

		public void TestGenerateValue_DateTimeOffset()
		{
			AssertEquals("2011-09-19T01:03:02.1230000+10:00", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.DateTimeOffset), new DateTimeOffset(2011, 9, 19, 1, 3, 2, 123, TimeSpan.FromHours(10))));
			AssertEquals("2011-09-19T01:03:02.1230000+00:00", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.DateTimeOffset), new DateTimeOffset(2011, 9, 19, 1, 3, 2, 123, TimeSpan.Zero)));
		}

		public void TestGenerateValue_Boolean()
		{
			AssertEquals("true", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.Char, BoolDefaultValue.False), true));
			AssertEquals("false", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.Char, BoolDefaultValue.False), false));
			AssertEquals("true", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.Char, BoolDefaultValue.True), "Y"));
			AssertEquals("false", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.Char, BoolDefaultValue.True), "N"));
			AssertEquals("false", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.Char, BoolDefaultValue.False), ""));
		}

		public void TestGenerateValue_Decimal()
		{
			AssertEquals("123.45", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.Decimal), 123.45m));
			AssertEquals("654.321", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.Decimal), 654.321m));
			AssertEquals("949", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.Decimal), 949m));
		}

		public void TestGenerateValue_Integer()
		{
			AssertEquals("123", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.Integer), 123));
		}

		public void TestGenerateValue_Text()
		{
			AssertEquals("Millet123", propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.Text), "Millet123"));
		}

		public void TestGenerateValue_TextIsNotTrimmed()
		{
			AssertEquals("                *******************", propertyGenerator.GenerateValue(new PropertyDef("FridayWorkingHours", DbDataType.Text), "                *******************"));
		}

		public void TestGenerateValue_BinaryArray()
		{
			AssertEquals(Base64String, propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.VarBinary), Convert.FromBase64String(Base64String)));
		}

		public void TestGenerateValue_ComressedBinaryArray()
		{
			byte[] value = Convert.FromBase64String(Base64String);
			byte[] compressedValue = (byte[])ZCompressor.GetCompressedVersion(value, "Z0_VarBinaryMax");

			AssertNotEquals(value, compressedValue);
			AssertEquals(Base64String, propertyGenerator.GenerateValue(new PropertyDef("aaa", DbDataType.VarBinary), compressedValue));
		}

		readonly PropertyElementGenerator propertyGenerator = new PropertyElementGenerator();

		const string Base64String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/ABCD";

		class PropertyDef : IPropertyDef
		{
			public PropertyDef(string name, string dataType)
				: this(name, dataType, null)
			{
			}

			public PropertyDef(string name, string dataType, object defaultValue)
			{
				PropertyName = name;
				ColumnDef = new ColumnDefinition(name, dataType, defaultValue);
			}

			public string PropertyName { get; private set; }

			public IColumnDef ColumnDef { get; private set; }

			public bool IsForExport { get { return true; } }

			public bool StripCRLF { get { return true; } }

			class ColumnDefinition : IColumnDef
			{
				public ColumnDefinition(string name, string dataType, object defaultValue)
				{
					this.Name = name;
					this.DataType = dataType;
					this.DefaultValue = defaultValue;
				}

				public string Name { get; private set; }

				public string DataType { get; private set; }

				public object DefaultValue { get; private set; }

				public int Length { get; private set; }
				public int Scale { get; private set; }
				public int Precision { get; private set; }

				public Table Table
				{
					get { throw new NotImplementedException(); }
				}

				public bool DoesNotRequireAValue
				{
					get { throw new NotImplementedException(); }
				}

				public bool Nullable
				{
					get { throw new NotImplementedException(); }
				}

				public string HumanName
				{
					get { throw new NotImplementedException(); }
				}

				public ColumnType Type
				{
					get { throw new NotImplementedException(); }
				}

				public IColumnValidator Validator => throw new NotImplementedException();
			}
		}
	}
}
