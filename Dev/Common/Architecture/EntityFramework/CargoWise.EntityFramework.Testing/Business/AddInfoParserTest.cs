using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Moq;

namespace CargoWise.EntityFramework.Testing
{
	sealed class AddInfoParserTest : TestCaseWithFactory
	{
		public void TestCreateAddInfoDictionary()
		{
			var hashtable = AddInfoParser.CreateDictionaryWithAddInfoString("*String=Te¤st*Int=3*String=Why*");
			AssertEquals(2, hashtable.Count);
			AssertEquals("Te*st^Why", hashtable["String"]);
			AssertEquals("3", hashtable["Int"]);
		}

		public void TestSerialise()
		{
			AssertEquals("*String=Te¤st", AddInfoParser.Serialise("String", "Te*st"));
			var hashtable = AddInfoParser.CreateDictionaryWithAddInfoString("*String=Te¤st*Int=3*String=Why*");
			AssertEquals("String=Te¤st^Why*Int=3", AddInfoParser.Serialise(hashtable));
			AssertEquals("Int=3*String=Te¤st^Why", AddInfoParser.Serialise(hashtable, true));
		}

		public void TestConcatAddInfoStrings()
		{
			AssertEquals("String=123", AddInfoParser.ConcatAddInfoStrings("String=123", ZString.Empty));
			AssertEquals("NString=一二三", AddInfoParser.ConcatAddInfoStrings("NString=一二三", ZString.Empty));
			AssertEquals("String=123*NString=一二三", AddInfoParser.ConcatAddInfoStrings("String=123", "NString=一二三"));
		}

		public void TestConvertToZType_ZBool() => AssertConvertToZType("Y", ZBool.True);
		public void TestConvertToZType_ZByte() => AssertConvertToZType(byte.MaxValue.ToString(), (ZByte)byte.MaxValue);
		public void TestConvertToZType_ZDate() => AssertConvertToZType(ZDate.BrettsBirthday.ToISO8601ShortDateString(), ZDate.BrettsBirthday);
		public void TestConvertToZType_ZDateTime() => AssertConvertToZType(ZDateTime.BrettsBirthday.ToISO8601String(), ZDateTime.BrettsBirthday);
		public void TestConvertToZType_ZDateTimeOffset() => AssertConvertToZType(new ZDateTimeOffset(2021, 1, 1, 2, 3, 4, TimeSpan.FromHours(-1)).ToISO8601String(), new ZDateTimeOffset(2021, 1, 1, 2, 3, 4, TimeSpan.FromHours(-1)));
		public void TestConvertToZType_ZDecimal() => AssertConvertToZType(decimal.MaxValue.ToString(CultureInfo.InvariantCulture), (ZDecimal)decimal.MaxValue);
		public void TestConvertToZType_ZGeography() => AssertConvertToZType("48.8583 2.2923", ZGeography.CreatePoint(48.8583, 2.2923));
		public void TestConvertToZType_ZGuid() => AssertConvertToZType(ZGuid.BrettsGuid.ToString(), ZGuid.BrettsGuid);
		public void TestConvertToZType_ZInt() => AssertConvertToZType(int.MaxValue.ToString(), (ZInt)int.MaxValue);
		public void TestConvertToZType_ZLong() => AssertConvertToZType(long.MaxValue.ToString(), (ZLong)long.MaxValue);
		public void TestConvertToZType_ZShort() => AssertConvertToZType(short.MaxValue.ToString(), (ZShort)short.MaxValue);
		public void TestConvertToZType_ZString() => AssertConvertToZType("HELLO", (ZString)"HELLO");

		void AssertConvertToZType<TZType>(object value, TZType expected)
			where TZType : IZType
		{
			var converted = AddInfoParser.ConvertToZType(typeof(TZType), value);
			AssertType<TZType>(converted);
			AssertEquals(expected, converted);

			converted = AddInfoParser.ConvertToZType(typeof(TZType), "");
			AssertType<TZType>(converted);
			AssertEquals(expected.Default, converted);

			converted = AddInfoParser.ConvertToZType(typeof(TZType), null);
			AssertEquals(null, converted);
		}

		public void TestDeserialise_Empty()
		{
			var intAddInfoPropertyData = new AddInfoPropertyData<ZInt>("Int");
			intAddInfoPropertyData.Value = 10;
			var addInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData>
			{
				{ "Date", new AddInfoPropertyData<ZDate>("Date") },
				{ "Int", intAddInfoPropertyData },
				{ "String", new AddInfoPropertyData<ZString>("String") },
			};
			AddInfoParser.Deserialise("", addInfoNamesMapping, true);
			AssertType<ZDate>(addInfoNamesMapping["Date"].Value);
			AssertType<ZInt>(addInfoNamesMapping["Int"].Value);
			AssertType<ZString>(addInfoNamesMapping["String"].Value);

			AssertEquals(ZDate.Empty, addInfoNamesMapping["Date"].Value);
			AssertEquals(ZInt.Zero, addInfoNamesMapping["Int"].Value);
			AssertEquals(ZString.Empty, addInfoNamesMapping["String"].Value);
		}

		public void TestDeserialise_String()
		{
			var addInfoString = "Date=2023-06-30*String=Te¤st^Why*Int=3";
			var addInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData>
			{
				{ "Date", new AddInfoPropertyData<ZDate>("Date") },
				{ "Int", new AddInfoPropertyData<ZInt>("Int") },
				{ "String", new AddInfoPropertyData<ZString>("String") },
			};
			AddInfoParser.Deserialise(addInfoString, addInfoNamesMapping, true);
			AssertType<ZDate>(addInfoNamesMapping["Date"].Value);
			AssertType<ZInt>(addInfoNamesMapping["Int"].Value);
			AssertType<ZString>(addInfoNamesMapping["String"].Value);

			AssertEquals(new ZDate(2023, 6, 30), addInfoNamesMapping["Date"].Value);
			AssertEquals((ZInt)3, addInfoNamesMapping["Int"].Value);
			AssertEquals((ZString)"Te*st^Why", addInfoNamesMapping["String"].Value);

			AssertEquals(new ZDate(2023, 6, 30), addInfoNamesMapping["Date"].OriginalValue);
			AssertEquals((ZInt)3, addInfoNamesMapping["Int"].OriginalValue);
			AssertEquals((ZString)"Te*st^Why", addInfoNamesMapping["String"].OriginalValue);

			addInfoString = "Date=2024-06-30*String=Te¤st^Who";
			AddInfoParser.Deserialise(addInfoString, addInfoNamesMapping, false);
			AssertEquals(new ZDate(2024, 6, 30), addInfoNamesMapping["Date"].OriginalValue);
			AssertEquals((ZInt)3, addInfoNamesMapping["Int"].OriginalValue);
			AssertEquals((ZString)"Te*st^Who", addInfoNamesMapping["String"].OriginalValue);
		}

		public void TestSerialise_Default()
		{
			var addInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData>
			{
				{ "Date", new AddInfoPropertyData<ZDate>("Date", ZDate.Empty) },
				{ "Int", new AddInfoPropertyData<ZInt>("Int", ZInt.Zero) },
				{ "String", new AddInfoPropertyData<ZString>("String", ZString.Empty) },
			};
			var addInfoString = AddInfoParser.Serialise(addInfoNamesMapping);
			AssertEquals("", addInfoString);
		}

		public void TestSerialise_String()
		{
			var addInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData>
			{
				{ "Date", new AddInfoPropertyData<ZDate>("Date", new ZDate(2023, 6, 30)) },
				{ "Int", new AddInfoPropertyData<ZInt>("Int", 3) },
				{ "String", new AddInfoPropertyData<ZString>("String", (ZString)"Te*st^Why") },
			};
			var addInfoString = AddInfoParser.Serialise(addInfoNamesMapping);
			AssertEquals("Date=2023-06-30*Int=3*String=Te¤st^Why", addInfoString);
		}

		public void TestSerialise_AdditionalPairs()
		{
			var addInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData>
			{
				{ "Date", new AddInfoPropertyData<ZDate>("Date", new ZDate(2023, 6, 30)) },
				{ "Int", new AddInfoPropertyData<ZInt>("Int", 3) },
				{ "String", new AddInfoPropertyData<ZString>("String", (ZString)"Te*st^Why") },
			};
			var additionalPairs = new KeyValuePair<ZString, ZString>[]
			{
				new KeyValuePair<ZString, ZString>("Additional1", "Value1"),
				new KeyValuePair<ZString, ZString>("Additional2", "Value2"),
			};
			var addInfoString = AddInfoParser.Serialise(addInfoNamesMapping, additionalPairs);
			AssertEquals("Additional1=Value1*Additional2=Value2*Date=2023-06-30*Int=3*String=Te¤st^Why", addInfoString);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "ABC";
			dummy.Z0_Description = "DEF";
			var additionalPairs2 = new KeyValuePair<ZString, ZPropertyInfo>[]
			{
				new KeyValuePair<ZString, ZPropertyInfo>("Code", dummy.Z0_CodeInfo),
				new KeyValuePair<ZString, ZPropertyInfo>("Description", dummy.Z0_DescriptionInfo),
			};
			addInfoString = AddInfoParser.Serialise(addInfoNamesMapping, additionalPairs2);
			AssertEquals("Code=ABC*Date=2023-06-30*Description=DEF*Int=3*String=Te¤st^Why", addInfoString);
		}

		public void TestHandleConcurrencyException_Date() => AssertConcurrencyException(AutoZZDummyBizo.Schema.Z0_AddInfoDate, ZDate.BrettsBirthday, ZDate.Today, ZDate.Today.AddDays(1));
		public void TestHandleConcurrencyException_DateTime() => AssertConcurrencyException(AutoZZDummyBizo.Schema.Z0_AddInfoDateTime, ZDateTime.BrettsBirthday, ZDateTime.Today, ZDateTime.Today.AddDays(1));
		public void TestHandleConcurrencyException_Decimal() => AssertConcurrencyException(AutoZZDummyBizo.Schema.Z0_AddInfoDecimal073, new ZDecimal(123m), new ZDecimal(456m), new ZDecimal(789m));
		public void TestHandleConcurrencyException_Guid() => AssertConcurrencyException(AutoZZDummyBizo.Schema.Z0_AddInfoGuid, ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());
		public void TestHandleConcurrencyException_Short() => AssertConcurrencyException(AutoZZDummyBizo.Schema.Z0_AddInfoInt16, new ZShort(123), new ZShort(456), new ZShort(789));
		public void TestHandleConcurrencyException_Int() => AssertConcurrencyException(AutoZZDummyBizo.Schema.Z0_AddInfoInt32, new ZInt(123), new ZInt(456), new ZInt(789));

		void AssertConcurrencyException<TZType>(string propertyName, TZType originalValue, TZType otherValue, TZType newValue)
			where TZType : IZType
		{
			var propertyNameWithoutPrefix = propertyName.Substring(3);
			var bizO = Factory.New<DummyZZBizo>();
			var mockPropertyRecord = new Mock<IPropertyRecord>();
			mockPropertyRecord.SetupGet(x => x.ColumnName).Returns(AutoDummyBizo.Schema.Z0_AddInfo);
			mockPropertyRecord.SetupGet(x => x.DatabaseValue).Returns(propertyNameWithoutPrefix + "=" + otherValue);
			mockPropertyRecord.SetupGet(x => x.LastModified).Returns("User");
			var propertyRecords = new List<IPropertyRecord>
			{
				mockPropertyRecord.Object
			};
			var addInfoNamesMappings = new Dictionary<string, IAddInfoPropertyData>
			{
				{ propertyNameWithoutPrefix, new AddInfoPropertyData<TZType>(propertyName, newValue) }
			};
			var addInfoColumnMappings = new Dictionary<string, IDictionary<string, IAddInfoPropertyData>>
			{
				{ AutoDummyBizo.Schema.Z0_AddInfo, addInfoNamesMappings }
			};

			AddInfoParser.HandleConcurrencyException(propertyRecords, GetPropertyInfo, addInfoColumnMappings);

			AssertHasWarningContaining("Notifications should be added against actual property info.",
				bizO.GetZPropertyInfo(propertyName),
				$@"Another user (User) has changed this field.
Yours: '{newValue.GetStringRepresentation()}', Theirs: '{otherValue.GetStringRepresentation()}'");

			ZPropertyInfo GetPropertyInfo(string name) => bizO.GetZPropertyInfo(name);
		}

		public void TestAppendDecoratedDisplayName()
		{
			var bizO = Factory.New<DummyZZBizo>();
			Assert(bizO is IConcurrencyExceptionDecorator);

			var propertyNameWithoutPrefix = AutoZZDummyBizo.Schema.Z0_AddInfoString3.Substring(3);
			var record = new Mock<IPropertyRecord>();
			record.Setup(m => m.ColumnName).Returns(AutoDummyBizo.Schema.Z0_AddInfo);
			record.Setup(m => m.OriginalValue).Returns(propertyNameWithoutPrefix + "=ABC");
			record.Setup(m => m.CurrentValue).Returns(propertyNameWithoutPrefix + "=DEF");
			record.Setup(m => m.DatabaseValue).Returns(propertyNameWithoutPrefix + "=GHI");
			record.Setup(m => m.DisplayName).Returns(bizO.Z0_AddInfoInfo.HumanReadableName);
			var addInfoNamesMappings = new Dictionary<string, IAddInfoPropertyData>
			{
				{ propertyNameWithoutPrefix, new AddInfoPropertyData<ZString>(AutoZZDummyBizo.Schema.Z0_AddInfoString3, "DEF") }
			};
			var addInfoColumnMappings = new Dictionary<string, IDictionary<string, IAddInfoPropertyData>>
			{
				{ AutoDummyBizo.Schema.Z0_AddInfo, addInfoNamesMappings }
			};

			var stringBuilder = new StringBuilder();
			AddInfoParser.AppendDecoratedDisplayName(stringBuilder, record.Object, GetPropertyInfo, addInfoColumnMappings);

			var expected = @"	Add Info
		Add Info String 3
";
			AssertEquals(expected, stringBuilder.ToString());

			ZPropertyInfo GetPropertyInfo(string name) => bizO.GetZPropertyInfo(name);
		}

		public void TestCopyAddInfoPropertyData_WithSetter()
		{
			var source = new Dictionary<string, IAddInfoPropertyData>
			{
				{ DummyZZBizo.Schema.Z0_AddInfoString35, new AddInfoPropertyData<ZString>(DummyZZBizo.Schema.Z0_AddInfoString35, "Test") },
			};
			var target = new Dictionary<string, IAddInfoPropertyData>
			{
				{ DummyZZBizo.Schema.Z0_AddInfoString35, new AddInfoPropertyData<ZString>(DummyZZBizo.Schema.Z0_AddInfoString35) },
			};

			var bizO = Factory.New<DummyZZBizo>();
			var args = new BusinessObjectCloneArgs();

			AddInfoParser.CopyAddInfoPropertyData(bizO, args, (target, source));
			AssertEquals("Test", bizO[DummyZZBizo.Schema.Z0_AddInfoString35]);
		}

		public void TestCopyAddInfoPropertyData_WithoutSetter()
		{
			var source = new Dictionary<string, IAddInfoPropertyData>
			{
				{ "AddInfoString35", new AddInfoPropertyData<ZString>("AddInfoString35", "Test") },
			};
			var target = new Dictionary<string, IAddInfoPropertyData>
			{
				{ "AddInfoString35", new AddInfoPropertyData<ZString>("AddInfoString35") },
			};

			var bizO = Factory.New<DummyZZBizo>();
			var args = new BusinessObjectCloneArgs([], true);

			AddInfoParser.CopyAddInfoPropertyData(bizO, args, (target, source));
			AssertEquals("Test", target["AddInfoString35"].Value);
		}
	}
}
