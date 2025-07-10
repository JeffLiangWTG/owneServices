using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectHelperTest : TestCase
	{
		[TestDate(2020, 10, 20)]
		public void TestGetNonDefaultValueForZType_ZDateTime()
		{
			AssertEquals(ZDateTime.Now, BusinessObjectHelper.GetNonDefaultValueForZType(typeof(ZDateTime)));
		}

		public void TestGetNonDefaultValueForZType_ZDecimal()
		{
			AssertEquals(10m, BusinessObjectHelper.GetNonDefaultValueForZType(typeof(ZDecimal)));
		}

		public void TestGetNonDefaultValueForZType_ZInt()
		{
			AssertEquals(10, BusinessObjectHelper.GetNonDefaultValueForZType(typeof(ZInt)));
		}

		public void TestGetNonDefaultValueForZType_ZShort()
		{
			AssertEquals((ZShort)10, BusinessObjectHelper.GetNonDefaultValueForZType(typeof(ZShort)));
		}

		public void TestGetNonDefaultValueForZType_ZByte()
		{
			AssertEquals((ZByte)10, BusinessObjectHelper.GetNonDefaultValueForZType(typeof(ZByte)));
		}

		public void TestGetNonDefaultValueForZType_ZBool()
		{
			AssertEquals(ZBool.True, BusinessObjectHelper.GetNonDefaultValueForZType(typeof(ZBool)));
		}
	}
}
