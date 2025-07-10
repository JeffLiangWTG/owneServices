using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using IENctsHeader = Enterprise.Customs.IE.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IE.NCTS.DocumentWrappers.Testing
{
	[TestedType(typeof(IENctsDepartureCargoDescWrapperCollection))]
	[MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Ireland)]
	sealed class IENctsDepartureCargoDescWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<IENctsDepartureCargoDescWrapperCollection>
	{
		protected override IENctsDepartureCargoDescWrapperCollection GetCollectionToTest()
		{
			var nctsHeader = Factory.NewWithValidTestData<IENctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();

			return new IENctsDepartureCargoDescWrapperCollection(bill, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line = Factory.NewWithValidTestData<NctsDepartureCargoDesc>();
			return IENctsDepartureCargoDescWrapper.New(line, Factory);
		}

		public void TestCollectionItems()
		{
			var header = Factory.New<IENctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			bill.GoodsItems.AddNew();
			bill.GoodsItems.AddNew();
			bill.GoodsItems.AddNew();

			var collectionWrapper = new IENctsDepartureCargoDescWrapperCollection(bill, Factory);
			AssertEquals("wrapped Goods Items number", 3, collectionWrapper.Count);
		}
	}
}
