using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.DataTransfer.Testing
{
	public class FieldAttributeTest : TestCase
	{
		public void TestField()
		{
			var attribute = new FieldAttribute(123, 321);
			AssertEquals("Position specified", (short)123, attribute.Position);
			AssertEquals("Length specified", (short)321, attribute.Length);
		}

#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(ArgumentOutOfRangeException), "Length must be 1 or greater.\r\nParameter name: length")]
		public void TestInvalidLengthParameter()
		{
			FieldAttribute attrib = new FieldAttribute(0, 0);
		}

		[ExpectExceptionMessage(typeof(ArgumentOutOfRangeException), "Position must be 0 or greater.\r\nParameter name: position")]
		public void TestInvalidPositionParameter()
		{
			FieldAttribute attrib = new FieldAttribute(-1, 1);
		}
#elif NET
		[ExpectExceptionMessage(typeof(ArgumentOutOfRangeException), "Length must be 1 or greater. (Parameter 'length')")]
		public void TestInvalidLengthParameter()
		{
			FieldAttribute attrib = new FieldAttribute(0, 0);
		}

		[ExpectExceptionMessage(typeof(ArgumentOutOfRangeException), "Position must be 0 or greater. (Parameter 'position')")]
		public void TestInvalidPositionParameter()
		{
			FieldAttribute attrib = new FieldAttribute(-1, 1);
		}
#endif

		public void TestGetValue()
		{
			TestGetValueCore();
		}

		protected virtual void TestGetValueCore()
		{
			ZString value = new ZString("OH YEAH");
			FlatFileDataRow dataRow = new FlatFileDataRow(1);
			dataRow[0] = value;

			FieldAttribute attrib = new FieldAttribute(0);
			AssertEquals("value of datarow[0]", value, attrib.GetValue(dataRow));
		}

		public void TestSetValueAsString()
		{
			TestSetValueAsStringCore();
		}

		protected virtual void TestSetValueAsStringCore()
		{
			IZType value = new ZString("OH \r\nYEAH");
			FlatFileDataRow dataRow = new FlatFileDataRow(1);

			AssertEquals("datarow[0] ", ZString.Empty, dataRow[0]);
			FieldAttribute attrib = new FieldAttribute(0, 3);
			attrib.SetValueAsString(value, dataRow);
			AssertEquals("datarow[0]", "OH ", dataRow[0]);
		}
	}
}
