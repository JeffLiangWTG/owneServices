using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class EntryHeaderTypeListProviderTest : TestCaseWithFactory
	{
		public void TestGetEntryTypes()
		{
			var objectProvider = ObjectFactory.Get<Integration.Customs.ICACusEntryHeaderTypeListProvider>();
			AssertEquals(typeof(EntryHeaderTypeListProvider), objectProvider.GetType());

			var expectedList = new[] { MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Codes.CommercialAccountingDeclaration, MessageTypeList.Codes.G7Export, MessageTypeList.Codes.EDIRelease };
			var actualList = ((CodeDescriptionPairList)objectProvider.GetEntryTypes()).GetAllCodes();

			AssertContainsExactElementsInAnyOrder(expectedList, actualList);
		}
	}
}
