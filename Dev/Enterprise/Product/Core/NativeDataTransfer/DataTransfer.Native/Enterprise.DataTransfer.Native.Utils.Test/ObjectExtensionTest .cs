using System;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Utils
{
	class ObjectExtensionTest : TestCase
	{
		public void TestIsEmpty()
		{
			object value = null;
			Assert("Empty When Value is Null", value.IsEmpty());

			value = Guid.Empty;
			Assert("Empty When Value is Guid.Empty", value.IsEmpty());

			value = String.Empty;
			Assert("Empty When Value is String.Empty", value.IsEmpty());

			value = "";
			Assert("Empty When Value is String.Empty", value.IsEmpty());

			value = string.Empty;
			Assert("Empty When Value is string.Empty", value.IsEmpty());

			value = null;
			Assert("Empty When Value is string.Empty", value.IsEmpty());

			value = DBNull.Value;
			Assert("Empty When Value is string.Empty", value.IsEmpty());
		}
	}
}
