using Enterprise.AuditDataServices.Subscription.Common;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Subscription.Testing
{
	class ObjectExtensionsTest : TestCase
	{
		public void TestGetDataValue()
		{
			string nullString = null;
			string stringWithValue = "string with value";
			short shortWithValue = 1;
			AssertEquals("GetDataValue<string>(null) should return null [default of string]", null, nullString.GetDataRowValue<string>());
			AssertEquals("GetDataValue<short>(null) should return (short)0 [default of short]", (short)0, nullString.GetDataRowValue<short>());
			AssertEquals("GetDataValue<string>(stringWithValue) should return value of stringWithValue", stringWithValue, stringWithValue.GetDataRowValue<string>());
			AssertEquals("GetDataValue<short>(shortWithValue) should return value of shortWithValue", shortWithValue, shortWithValue.GetDataRowValue<short>());
		}
	}
}
