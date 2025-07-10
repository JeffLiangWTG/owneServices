using System;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class NullTypeTest : NUnit.Framework.TestCase
	{
		public void TestGetNullTypes()
		{
			AssertEquals("Null value for DateTime", new DateTime(1753, 1, 1), NullType.DateTime);
			AssertEquals("Null value for Guid", Guid.Empty, NullType.Guid);
			AssertEquals("Null value for String", "", NullType.String);
			AssertEquals("Null value for Double", 0D, NullType.Double);
			AssertEquals("Null value for Float", 0F, NullType.Float);
			AssertEquals("Null value for Decimal", 0M, NullType.Decimal);
			AssertEquals("Null value for Int", 0, NullType.Int);
			AssertEquals("Null value for Long", 0L, NullType.Long);
			AssertEquals("Null value for Object", System.DBNull.Value, NullType.Object);
			AssertEquals("Null value for Byte", Byte.MinValue, NullType.Byte);
		}

		public void TestGetNullObject()
		{
			AssertEquals("DateTime NullObject", new DateTime(1753, 1, 1), NullType.GetNullObject(typeof(DateTime)));
			AssertEquals("Guid NullObject", Guid.Empty, NullType.GetNullObject(typeof(Guid)));
			AssertEquals("String NullObject", "", NullType.GetNullObject(typeof(string)));
			AssertEquals("Double NullObject", 0D, NullType.GetNullObject(typeof(double)));
			AssertEquals("Float NullObject", 0F, NullType.GetNullObject(typeof(float)));
			AssertEquals("Decimal NullObject", 0M, NullType.GetNullObject(typeof(decimal)));
			AssertEquals("Int NullObject", 0, NullType.GetNullObject(typeof(int)));
			AssertEquals("Long NullObject", 0L, NullType.GetNullObject(typeof(long)));
			AssertEquals("Object NullObject", System.DBNull.Value, NullType.GetNullObject(typeof(object)));
			AssertEquals("Byte NullObject", Byte.MinValue, NullType.GetNullObject(typeof(byte)));
		}
	}
}
