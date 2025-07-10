namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;

	public class TestSimpleTypeFormatter : TestCaseWithFactory
	{
		public void TestFormatForMultiTypedStringElementTargetInDataObject()
		{
			var value = (object)new ZString(@"&""Value""");
			AssertEquals(@"&""Value""", SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(value));

			value = new ZCodeMappedZString(@"&""Value""");
			AssertEquals(@"&""Value""", SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(value));

			AssertEquals("false", SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(ZBool.False));
			AssertEquals("1971-09-18T00:00:00", SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(ZDateTime.BrettsBirthday));
			AssertEquals("1971-09-18T00:00:00", SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(new UXmlDateTime(ZDateTime.BrettsBirthday)));
			AssertEquals("1971-09-18", SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(ZDate.BrettsBirthday));
			AssertEquals($"1971-09-18T00:00:00.000{((ZString)new ZDateTimeOffset(ZDateTime.Now).ToString()).Right(6)}", SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(new ZDateTimeOffset(ZDate.BrettsBirthday)));
			AssertEquals($"1971-09-18T00:00:00.000{((ZString)new ZDateTimeOffset(ZDateTime.Now).ToString()).Right(6)}", SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(new UXmlDateTime(new ZDateTimeOffset(ZDate.BrettsBirthday))));
			AssertEquals("POINT (12 12 123)", SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(ZGeography.CreatePoint(12,12,123)));
			AssertEquals("123.456", SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(new ZDecimal(123.456)));
			AssertEquals("AQIDBAU=", SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(new ZBlob(new byte[] { 1,2,3,4,5 })));
		}

		public void TestGetFormattedValueForWritingToXml()
		{
			var value = (object)new ZString(@"&""Value""");
			AssertEquals(@"&amp;""Value""", SimpleTypeFormatter.GetFormattedValueForWritingToXml(value, () => 39));
			AssertEquals(@"&amp;""Val", SimpleTypeFormatter.GetFormattedValueForWritingToXml(value, () => 5));

			value = new ZCodeMappedZString(@"&""Value""");
			AssertEquals(@"&amp;""Value""", SimpleTypeFormatter.GetFormattedValueForWritingToXml(value, () => 39));
			AssertEquals(@"&amp;""Val", SimpleTypeFormatter.GetFormattedValueForWritingToXml(value, () => 5));

			AssertEquals("false", SimpleTypeFormatter.GetFormattedValueForWritingToXml(ZBool.False, null));
			AssertEquals("1971-09-18T00:00:00", SimpleTypeFormatter.GetFormattedValueForWritingToXml(ZDateTime.BrettsBirthday, null));
			AssertEquals("1971-09-18T00:00:00", SimpleTypeFormatter.GetFormattedValueForWritingToXml(new UXmlDateTime(ZDateTime.BrettsBirthday), null));
			AssertEquals("1971-09-18", SimpleTypeFormatter.GetFormattedValueForWritingToXml(ZDate.BrettsBirthday, null));
			AssertEquals("POINT (12 12 123)", SimpleTypeFormatter.GetFormattedValueForWritingToXml(ZGeography.CreatePoint(12, 12, 123), null));
			AssertEquals("123.456", SimpleTypeFormatter.GetFormattedValueForWritingToXml(new ZDecimal(123.456), null));
			AssertEquals("AQIDBAU=", SimpleTypeFormatter.GetFormattedValueForWritingToXml(new ZBlob(new byte[] { 1, 2, 3, 4, 5 }), null));
		}

		public void TestGetFormattedValueAsAttributeForWritingToXml()
		{
			var value = (object)new ZString(@"&""Value""");
			AssertEquals(@" My Name Is=""&amp;&quot;Value&quot;""", SimpleTypeFormatter.GetFormattedValueAsAttributeForWritingToXml("My Name Is", value, () => 39));
			AssertEquals(@" My Name Is=""&amp;&quot;Val""", SimpleTypeFormatter.GetFormattedValueAsAttributeForWritingToXml("My Name Is", value, () => 5));

			value = new ZCodeMappedZString(@"&""Value""");
			AssertEquals(@" My Name Is=""&amp;&quot;Value&quot;""", SimpleTypeFormatter.GetFormattedValueAsAttributeForWritingToXml("My Name Is", value, () => 39));
			AssertEquals(@" My Name Is=""&amp;&quot;Val""", SimpleTypeFormatter.GetFormattedValueAsAttributeForWritingToXml("My Name Is", value, () => 5));

			AssertEquals(@" My Name Is=""false""", SimpleTypeFormatter.GetFormattedValueAsAttributeForWritingToXml("My Name Is", ZBool.False, null));
			AssertEquals(@" My Name Is=""1971-09-18T00:00:00""", SimpleTypeFormatter.GetFormattedValueAsAttributeForWritingToXml("My Name Is", ZDateTime.BrettsBirthday, null));
			AssertEquals(@" My Name Is=""1971-09-18T00:00:00""", SimpleTypeFormatter.GetFormattedValueAsAttributeForWritingToXml("My Name Is", new UXmlDateTime(ZDateTime.BrettsBirthday), null));
			AssertEquals(@" My Name Is=""1971-09-18""", SimpleTypeFormatter.GetFormattedValueAsAttributeForWritingToXml("My Name Is", ZDate.BrettsBirthday, null));
			AssertEquals(@" My Name Is=""POINT (12 12 123)""", SimpleTypeFormatter.GetFormattedValueAsAttributeForWritingToXml("My Name Is", ZGeography.CreatePoint(12, 12, 123), null));
			AssertEquals(@" My Name Is=""123.456""", SimpleTypeFormatter.GetFormattedValueAsAttributeForWritingToXml("My Name Is", new ZDecimal(123.456), null));
			AssertEquals(@" My Name Is=""AQIDBAU=""", SimpleTypeFormatter.GetFormattedValueAsAttributeForWritingToXml("My Name Is", new ZBlob(new byte[] { 1, 2, 3, 4, 5 }), null));
		}
	}
}
