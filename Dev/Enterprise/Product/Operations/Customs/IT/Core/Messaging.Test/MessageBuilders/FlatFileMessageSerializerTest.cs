using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.MessageBuilders;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class FlatFileMessageSerializerTest : TestCase
{
	public void TestDummyTestMessageSerialization()
	{
		string expectedSerializedMessage = @"DummyTestObject_ZBool	1	0	
DummyTestObject_ZDateTime	20200101	01012020	100122	010120	202001011011				
DummyTestObject_ZDecimal	100.39	0000100.390	100.390	0000100.390	100.39	0000100.390	00000100.000	00000100.0	100	
DummyTestObject_ZInt	0	0000000000	5124	00005543	
DummyTestObject_ZString	test	test      	test	test	
DummyTestObject_IZType_Array	FIRST ITEM	SECOND ITEM	THIRD ITEM          	FOURTH ITEM         	0	1	0000000999	0000100000	123.32	554345.54	0000000123.320	0000554345.540	
DummyTestObject_ComplexObject_Array	DummyTestObject_ComplexObject_Item	1	i end with spaces                                 	0000000432	
DummyTestObject_MessageFixedLength4   000000095473";
		AssertMultilineASCIIEquals(expectedSerializedMessage, flatFileMessageSerializer.Serialize(new DummyTestMessage()));
	}

	public void TestException()
	{
		AssertExceptionThrown<ArgumentNullException>(() => flatFileMessageSerializer.Serialize(null));
		AssertNoExceptionThrown(() => flatFileMessageSerializer.Serialize(new DummyTestMessage()));
	}

	protected override void SetUp()
	{
		base.SetUp();
		flatFileMessageSerializer = new FlatFileMessageSerializer("\t");
	}

	FlatFileMessageSerializer flatFileMessageSerializer;
}

#region Dummy classes

class DummyTestMessage : SadCustomsMessage
{
	[MessageLayout(Order = 0)]
	public DummyTestObject_ZBool ZBoolDummyTestObject => new DummyTestObject_ZBool();

	[MessageLayout(Order = 1)]
	public DummyTestObject_ZDateTime ZDateTimeDummyTestObject => new DummyTestObject_ZDateTime();

	[MessageLayout(Order = 2)]
	public DummyTestObject_ZDecimal ZDecimalDummyTestObject => new DummyTestObject_ZDecimal();

	[MessageLayout(Order = 3)]
	public DummyTestObject_ZInt ZIntDummyTestObject => new DummyTestObject_ZInt();

	[MessageLayout(Order = 4)]
	public DummyTestObject_ZString ZStringDummyTestObject => new DummyTestObject_ZString();

	[MessageLayout(Order = 5)]
	public DummyTestObject_IZType_Array ZString_ArrayDummyTestObject => new DummyTestObject_IZType_Array();

	[MessageLayout(Order = 6)]
	public DummyTestObject_ComplexObject_Array ComplexObject_ArrayDummyTestObject => new DummyTestObject_ComplexObject_Array();

	[MessageLayout(Order = 7)]
	public DummyTestObject_MessageFixedLength FixedLengthDummyTestObject => new DummyTestObject_MessageFixedLength();
}

class DummyTestObject_ZBool : IMessageHeader
{
	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 999, false)]
	public ZString Description => nameof(DummyTestObject_ZBool);

	[MessageLayout(Order = 1)]
	[MessageFieldBoolRepresentation]
	public ZBool Property_True => true;

	[MessageLayout(Order = 2)]
	[MessageFieldBoolRepresentation]
	public ZBool Property_False => false;
}

class DummyTestObject_ZDateTime : IMessageHeader
{
	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 999, false)]
	public ZString Description => nameof(DummyTestObject_ZDateTime);

	[MessageLayout(Order = 1)]
	[MessageFieldDateYYYYMMDDRepresentation]
	public ZDateTime Property_yyyyMMdd => new ZDateTime(2020, 01, 01);

	[MessageLayout(Order = 2)]
	[MessageFieldDateDDMMYYYYRepresentation]
	public ZDateTime Property_ddMMyyyy => new ZDateTime(2020, 01, 01);

	[MessageLayout(Order = 3)]
	[MessageFieldDateHHMMSSRepresentation]
	public ZDateTime Property_HHmmss => new ZDateTime(2020, 01, 01, 10, 01, 22);

	[MessageLayout(Order = 4)]
	[MessageFieldDateDDMMYYRepresentation]
	public ZDateTime Property_ddMMyy => new ZDateTime(2020, 01, 01);

	[MessageLayout(Order = 5)]
	[MessageFieldDateYYYYMMDDHHMMRepresentation]
	public ZDateTime Property_yyyyMMddHHmm => new ZDateTime(2020, 01, 01, 10, 11, 12);

	[MessageLayout(Order = 6)]
	[MessageFieldDateDDMMYYRepresentation]
	public ZDateTime Property_yyyyMMdd_empty => ZDateTime.Empty;

	[MessageLayout(Order = 7)]
	[MessageFieldDateYYYYMMDDHHMMRepresentation]
	public ZDateTime Property_yyyyMMddHHmm_empty => ZDateTime.Empty;

	[MessageLayout(Order = 8)]
	[MessageFieldDateHHMMSSRepresentation]
	public ZDateTime Property_HHmmss_empty => ZDateTime.Empty;
}

class DummyTestObject_ZDecimal : IMessageHeader
{
	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 999, false)]
	public ZString Description => nameof(DummyTestObject_ZDecimal);

	[MessageLayout(Order = 1)]
	[MessageFieldDecimalRepresentation(integerPartLength: 7, decimalPartLength: 3, isFixedLength: false, isDecimalPartFixedLength: false)]
	public ZDecimal Property_7_3_false_false => new ZDecimal(100.39m);

	[MessageLayout(Order = 2)]
	[MessageFieldDecimalRepresentation(integerPartLength: 7, decimalPartLength: 3, isFixedLength: true, isDecimalPartFixedLength: false)]
	public ZDecimal Property_7_3_true_false => new ZDecimal(100.39m);

	[MessageLayout(Order = 3)]
	[MessageFieldDecimalRepresentation(integerPartLength: 7, decimalPartLength: 3, isFixedLength: false, isDecimalPartFixedLength: true)]
	public ZDecimal Property_7_3_false_true => new ZDecimal(100.39m);

	[MessageLayout(Order = 4)]
	[MessageFieldDecimalRepresentation(integerPartLength: 7, decimalPartLength: 3, isFixedLength: true, isDecimalPartFixedLength: true)]
	public ZDecimal Property_7_3_true_true => new ZDecimal(100.39m);

	[MessageLayout(Order = 5)]
	[MessageFieldDecimalRepresentation(integerPartLength: 7, decimalPartLength: 3, isFixedLength: false)]
	public ZDecimal Property_7_3_false => new ZDecimal(100.39m);

	[MessageLayout(Order = 6)]
	[MessageFieldDecimalRepresentation(integerPartLength: 7, decimalPartLength: 3, isFixedLength: true)]
	public ZDecimal Property_7_3_true => new ZDecimal(100.39m);

	[MessageLayout(Order = 7)]
	[MessageFieldDecimalRepresentation(integerPartLength: 8, decimalPartLength: 3, isFixedLength: true)]
	public ZDecimal Property_8_3_true => new ZDecimal(100m);

	[MessageLayout(Order = 8)]
	[MessageFieldDecimalRepresentation(integerPartLength: 8, decimalPartLength: 1, isFixedLength: true)]
	public ZDecimal Property_9_1_true => new ZDecimal(100m);

	[MessageLayout(Order = 9)]
	[MessageFieldDecimalRepresentation(integerPartLength: 8, decimalPartLength: 1, isFixedLength: false)]
	public ZDecimal Property_9_1_false => new ZDecimal(100m);
}

class DummyTestObject_ZInt : IMessageHeader
{
	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 999, false)]
	public ZString Description => nameof(DummyTestObject_ZInt);

	[MessageLayout(Order = 1)]
	[MessageFieldIntegerRepresentation(10, false)]
	public ZInt Property_10_false => 0;

	[MessageLayout(Order = 2)]
	[MessageFieldIntegerRepresentation(10, true)]
	public ZInt Property_10_true => 0;

	[MessageLayout(Order = 3)]
	[MessageFieldIntegerRepresentation(8, false)]
	public ZInt Property_8_false => 5124;

	[MessageLayout(Order = 4)]
	[MessageFieldIntegerRepresentation(8, true)]
	public ZInt Property_8_true => 5543;
}

class DummyTestObject_ZString : IMessageHeader
{
	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 999, false)]
	public ZString Description => nameof(DummyTestObject_ZString);

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 10, false)]
	public ZString Property_10_false => "test";

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 10, true)]
	public ZString Property_10_true => "test";

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 4, false)]
	public ZString Property_4_false => "test";

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 4, true)]
	public ZString Property_4_true => "test";
}

class DummyTestObject_IZType_Array : IMessageContinuation
{
	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 999, false)]
	public ZString Description => nameof(DummyTestObject_IZType_Array);

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 20, false)]
	public IEnumerable<ZString> Property_ZString_Array_false => new ZString[] { "FIRST ITEM", "SECOND ITEM" };

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 20, true)]
	public IEnumerable<ZString> Property_ZString_Array_true => new ZString[] { "THIRD ITEM", "FOURTH ITEM" };

	[MessageLayout(Order = 3)]
	[MessageFieldIntegerRepresentation(10, false)]
	public IEnumerable<ZInt> Property_ZInt_Array_false => new ZInt[] { 0, 1 };

	[MessageLayout(Order = 4)]
	[MessageFieldIntegerRepresentation(10, true)]
	public IEnumerable<ZInt> Property_ZInt_Array_true => new ZInt[] { 999, 100000 };

	[MessageLayout(Order = 5)]
	[MessageFieldDecimalRepresentation(10, 3, false)]
	public IEnumerable<ZDecimal> Property_ZDecimal_Array_false => new ZDecimal[] { 123.32m, 554345.54m };

	[MessageLayout(Order = 6)]
	[MessageFieldDecimalRepresentation(10, 3, true)]
	public IEnumerable<ZDecimal> Property_ZDecimal_Array_true => new ZDecimal[] { 123.32m, 554345.54m };
}

class DummyTestObject_ComplexObject_Array : IMessageHeader
{
	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 999, false)]
	public ZString Description => nameof(DummyTestObject_ComplexObject_Array);

	[MessageLayout(Order = 1)]
	public IEnumerable<DummyTestObject_ComplexObject_Item> Property => new DummyTestObject_ComplexObject_Item[] { new DummyTestObject_ComplexObject_Item() };
}

class DummyTestObject_ComplexObject_Item
{
	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 999, false)]
	public ZString Description => nameof(DummyTestObject_ComplexObject_Item);

	[MessageLayout(Order = 1)]
	[MessageFieldBoolRepresentation]
	public ZBool Property_True => true;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 50, true)]
	public ZString Property_50_true => "i end with spaces";

	[MessageLayout(Order = 3)]
	[MessageFieldIntegerRepresentation(10, true)]
	public ZInt Property_10_true => 432;
}

[MessageFixedLength]
class DummyTestObject_MessageFixedLength : IMessageContinuation
{
	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 999, false)]
	public ZString Description => nameof(DummyTestObject_MessageFixedLength);

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 4, true)]
	public ZString Property_4_true => "4";

	[MessageLayout(Order = 2)]
	[MessageFieldIntegerRepresentation(12, true)]
	public ZInt Property_12_true => 95473;
}

#endregion
