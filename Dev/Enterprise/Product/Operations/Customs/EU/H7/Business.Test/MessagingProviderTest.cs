using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(MessagingProvider))]
	class MessagingProviderTest : TestCaseWithFactory
	{
		public void TestGetCustomsEntryNumberTypeList()
		{
			var entryTypeList = new MessagingProvider().GetCustomsEntryNumberTypeList(Factory, string.Empty);
			AssertContainsExactElementsInAnyOrder(expectedEntryNumberTypes, entryTypeList.GetAllCodes());
		}

		public static readonly string[] expectedEntryNumberTypes = new[] { "MRN", "LRN" };
	}
}
