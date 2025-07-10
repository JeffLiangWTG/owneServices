using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryHeaderContainerCollection))]
	class CusContainerWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EntryHeaderContainerCollection>
	{
		public void TestCollectionLoadedCorrectly()
		{
			var declaration = ContainerWrapperTest.PrepareDataForContainerWrapperTest(Factory);
			var cusEntryHeader = declaration.ActiveEntryHeaders[0];
			var containers1 = cusEntryHeader.EntryHeaderContainers.ToArray<EntryHeaderContainer>();
			cusEntryHeader.EntryHeaderContainers.ToArray<EntryHeaderContainer>();
			AssertEquals(3, containers1.Length);
			cusEntryHeader.ClearCachedMergedData();
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_ContainerMode = "";
			cusEntryHeader.ClearCachedMergedData();
			AssertEquals(0, cusEntryHeader.EntryHeaderContainers.Count);
		}

		protected override EntryHeaderContainerCollection GetCollectionToTest() => new EntryHeaderContainerCollection(Factory.New<CusEntryHeader>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => new EntryHeaderContainer(Factory.New<CusEntryHeader>(), Factory.New<CusContainer>());
	}
}
