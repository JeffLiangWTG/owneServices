using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.ModelView.Testing
{
	sealed class AddInfoTest : TestCase
	{
		public void TestNormalizedName()
		{
			CombineAssertions(() =>
			{
				AssertNormalizedName(null, null);
				AssertNormalizedName("", "");
				AssertNormalizedName("Some_AddInfo1", "AddInfo1");
				AssertNormalizedName("Some_XAddInfo1", "XAddInfo1");
				AssertNormalizedName("Some_Other_XAddInfo1", "Other_XAddInfo1");
				AssertNormalizedName("XAddInfo1", "XAddInfo1");
			});
		}

		void AssertNormalizedName(string name, string expectedNormalizedName)
		{
			var addInfo = new AddInfo { Name = name };

			AssertEquals(expectedNormalizedName, addInfo.NormalizedName);
		}

		public void TestEquals()
		{
			var addInfo1 = new AddInfo { Name = "ABC" };
			var addInfo2 = new AddInfo { Name = "ABC" };
			var addInfo3 = new AddInfo { Name = "DEF" };
			var addInfo4 = new AddInfo { Name = "DEF" };

			AssertEquals(addInfo1, addInfo1);
			AssertEquals(addInfo1, addInfo2);
			AssertNotEquals(addInfo1, addInfo3);
			AssertNotEquals(addInfo1, addInfo4);
			AssertEquals(addInfo2, addInfo1);
			AssertEquals(addInfo2, addInfo2);
			AssertNotEquals(addInfo2, addInfo3);
			AssertNotEquals(addInfo2, addInfo4);
			AssertNotEquals(addInfo3, addInfo1);
			AssertNotEquals(addInfo3, addInfo2);
			AssertEquals(addInfo3, addInfo3);
			AssertEquals(addInfo3, addInfo4);
			AssertNotEquals(addInfo4, addInfo1);
			AssertNotEquals(addInfo4, addInfo2);
			AssertEquals(addInfo4, addInfo3);
			AssertEquals(addInfo4, addInfo4);
		}

		public void TestGetHashCode()
		{
			var addInfo1 = new AddInfo { Name = "ABC" };
			var addInfo2 = new AddInfo { Name = "abc" };
			var addInfo3 = new AddInfo { Name = "DEF" };
			var addInfo4 = new AddInfo { Name = "DEF" };

			AssertEquals(addInfo1.GetHashCode(), addInfo1.GetHashCode());
			AssertEquals(addInfo1.GetHashCode(), addInfo2.GetHashCode());
			AssertNotEquals(addInfo1.GetHashCode(), addInfo3.GetHashCode());
			AssertEquals(addInfo3.GetHashCode(), addInfo4.GetHashCode());
		}
	}
}
