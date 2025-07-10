using System;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class TIRLineWrapperTest : Customs.Business.Testing.DataProviderTestCase<TIRLineWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("NctsMovementDetail", () => new TIRLineWrapper(null));
		}

		public void TestInternalPackages()
		{
			goodsItem.IsVehicles = true;
			goodsItem.Packages.AddNew();
			goodsItem.Packages.AddNew();
			var internalPackages = wrapper.InternalPackages;

			CombineAssertions(() =>
			{
				AssertEquals("Expected filled InternalPackages.Packages with vehicles", 2, internalPackages.Packages.Count);
				AssertSame("Cached InternalPackages", wrapper.InternalPackages, internalPackages);
			});
		}

		public void TestInternalPackagesIsNotVehicles()
		{
			goodsItem.IsVehicles = false;
			goodsItem.Packages.AddNew();
			goodsItem.Packages.AddNew();
			var internalPackages = wrapper.InternalPackages;
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled InternalPackages.Packages with packages", 2, internalPackages.Packages.Count);
				AssertSame("Cached InternalPackages", wrapper.InternalPackages, internalPackages);
			});
		}

		public void TestDocuments()
		{
			goodsItem.SupportingDocuments.AddNew();
			goodsItem.SupportingDocuments.AddNew();
			var documents = wrapper.Documents;
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled Documents", 2, documents.Count);
				AssertSame("Cached Documents", wrapper.Documents, documents);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			wrapper = new TIRLineWrapper(goodsItem);
		}
		NctsDepartureCargoDesc goodsItem;
		TIRLineWrapper wrapper;

		protected override TIRLineWrapper GetProvider() => wrapper;
	}
}
