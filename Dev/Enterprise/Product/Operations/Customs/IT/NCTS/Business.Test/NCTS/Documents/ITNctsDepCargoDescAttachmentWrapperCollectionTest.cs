using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using ITNctsDepartureCargoDesc = Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc;
using ITNctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(ITNctsDepCargoDescAttachmentWrapperCollection))]
sealed class ITNctsDepCargoDescAttachmentWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ITNctsDepCargoDescAttachmentWrapperCollection>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("when nctsHeader is null", () => new ITNctsDepCargoDescAttachmentWrapperCollection(null, Factory));

		var nctsHeader = Factory.New<ITNctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		AssertNoExceptionThrown("when nctsHeader and movementHeader are valid", () => new ITNctsDepCargoDescAttachmentWrapperCollection(nctsHeader, Factory));
	}

	protected override ITNctsDepCargoDescAttachmentWrapperCollection GetCollectionToTest()
	{
		var nctsHeader = Factory.NewWithValidTestData<ITNctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		return new ITNctsDepCargoDescAttachmentWrapperCollection(nctsHeader, Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var line = Factory.NewWithValidTestData<ITNctsDepartureCargoDesc>();
		return ITNctsDepCargoDescAttachmentWrapper.New(line, Factory);
	}

	public void TestCollectionItems()
	{
		var header = Factory.New<ITNctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);

		header.Bills.AddNew().GoodsItems.AddNew().Remarks = "remark on first item";
		header.Bills.AddNew().GoodsItems.AddNew();
		header.Bills.AddNew().GoodsItems.AddNew().Remarks = "remark on third item";

		var collectionWrapper = new ITNctsDepCargoDescAttachmentWrapperCollection(header, Factory);

		AssertEquals("wrapped Goods Items number", 2, collectionWrapper.Count);
	}
}
