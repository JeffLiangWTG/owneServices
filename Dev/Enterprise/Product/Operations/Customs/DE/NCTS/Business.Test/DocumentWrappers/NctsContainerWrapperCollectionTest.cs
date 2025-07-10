using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(NctsContainerWrapperCollection))]
	sealed class NctsContainerWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NctsContainerWrapperCollection>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("when nctsHeader is null", () => new NctsContainerWrapperCollection(null, Factory));

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertNoExceptionThrown("when nctsHeader is valid", () => new NctsContainerWrapperCollection(nctsHeader, Factory));
		}

		public void TestCollectionItems()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			header.DepartureHeaderContainers.AddNew();
			header.DepartureHeaderContainers.AddNew();
			header.DepartureHeaderContainers.AddNew();

			var collectionWrapper = new NctsContainerWrapperCollection(header, Factory);

			AssertEquals("wrapped container number", 3, collectionWrapper.Count);
		}

		protected override NctsContainerWrapperCollection GetCollectionToTest()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			return new NctsContainerWrapperCollection(nctsHeader, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var container = Factory.NewWithValidTestData<NctsDepartureHeaderContainer>();
			return NctsContainerWrapper.New(container);
		}
	}
}
