using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class UNDGPackageTypeListTest : TestCase
	{
		public void TestChangeCode6HBTo6HB1()
		{
			var list = new UNDGPackageTypeList();
			CombineAssertions("Change code 6HB to 6HB1", () =>
			{
				AssertEquals(true, list.ContainsCode("6HB1"));
				AssertEquals(false, list.ContainsCode("6HB"));
				AssertEquals("\u5851\u6599\u5bb9\u5668\u5728\u94dd\u6876\u5185\u590d\u5408\u5305\u88c5", list.GetDescriptionFromCode("6HB1"));
			});
		}
	}
}
