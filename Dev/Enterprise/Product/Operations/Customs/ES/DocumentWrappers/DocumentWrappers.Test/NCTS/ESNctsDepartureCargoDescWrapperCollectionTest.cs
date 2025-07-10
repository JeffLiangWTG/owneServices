using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using ESNctsDepartureCargoDesc = Enterprise.Customs.ES.NCTS.Business.NctsDepartureCargoDesc;
using ESNctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS.Testing
{
	[TestedType(typeof(ESNctsDepartureCargoDescWrapperCollection))]
	sealed class ESNctsDepartureCargoDescWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ESNctsDepartureCargoDescWrapperCollection>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("when nctsHeader is null", () => new ESNctsDepartureCargoDescWrapperCollection(null, Factory));

			var nctsHeader = Factory.New<ESNctsHeader>();
			AssertExceptionThrown<ArgumentNullException>("when Factory is null", () => new ESNctsDepartureCargoDescWrapperCollection(nctsHeader, null));

			nctsHeader.SetMovementType(Enterprise.Customs.EU.NCTS.Business.NctsMovementType.Codes.Departure);
			AssertNoExceptionThrown("when nctsHeader and movementHeader are valid", () => new ESNctsDepartureCargoDescWrapperCollection(nctsHeader, Factory));
		}

		protected override ESNctsDepartureCargoDescWrapperCollection GetCollectionToTest()
		{
			var nctsHeader = Factory.NewWithValidTestData<ESNctsHeader>();
			nctsHeader.SetMovementType(Enterprise.Customs.EU.NCTS.Business.NctsMovementType.Codes.Departure);

			return new ESNctsDepartureCargoDescWrapperCollection(nctsHeader, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line = Factory.NewWithValidTestData<ESNctsDepartureCargoDesc>();
			return ESNctsDepartureCargoDescWrapper.New(line, Factory);
		}

		public void TestCollectionItems()
		{
			var header = Factory.New<ESNctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			header.Bills.AddNew().GoodsItems.AddNew();
			header.Bills.AddNew().GoodsItems.AddNew();
			header.Bills.AddNew().GoodsItems.AddNew();

			var collectionWrapper = new ESNctsDepartureCargoDescWrapperCollection(header, Factory);

			AssertEquals("wrapped Goods Items number", 3, collectionWrapper.Count);
		}
	}
}
