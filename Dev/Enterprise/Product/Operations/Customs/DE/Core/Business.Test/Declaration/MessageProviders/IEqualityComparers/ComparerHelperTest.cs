using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ComparerHelperTest : TestCase
	{
		public void TestCompare_ReferenceEquals()
		{
			var contentInformationMock = IContentInformationEqualityComparerTest.ContentInformationMock;
			AssertEquals(true, ComparerHelper.Compare(contentInformationMock, contentInformationMock, (x, y) => x != y));
		}

		public void TestCompare_FirstObjectNull()
		{
			AssertEquals(false, ComparerHelper.Compare(null, IContentInformationEqualityComparerTest.ContentInformationMock, (x, y) => x != y));
		}

		public void TestCompare_SecondObjectNull()
		{
			AssertEquals(false, ComparerHelper.Compare(IContentInformationEqualityComparerTest.ContentInformationMock, null, (x, y) => x != y));
		}
	}
}
