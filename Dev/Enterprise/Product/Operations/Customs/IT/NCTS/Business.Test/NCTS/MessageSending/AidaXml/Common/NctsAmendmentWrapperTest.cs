using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class NctsAmendmentWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception when sendingObject argument is null", () => new NctsAmendmentWrapper(null));
	}

	public void TestMrn()
	{
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";
		var wrapper = GetWrapper();

		AssertEquals(nameof(INctsAmendment.Mrn), "MRN123", wrapper.Mrn);
	}

	public void TestReason()
	{
		sendingObject.Reason = "R";
		var wrapper = GetWrapper();

		AssertEquals(nameof(INctsAmendment.Reason), "R", wrapper.Reason);
	}

	public void TestLegislativeReference()
	{
		sendingObject.LegislativeReference = "L";
		var wrapper = GetWrapper();

		AssertEquals(nameof(INctsAmendment.LegislativeReference), "L", wrapper.LegislativeReference);
	}

	public void TestHouseConsignmentToBeDeleted()
	{
		var bill1 = nctsHeader.Bills.AddNew();
		var bill2 = nctsHeader.Bills.AddNew();
		var bill3 = nctsHeader.Bills.AddNew();
		bill3.B0_BillStatus = "DEL";
		var bill4 = nctsHeader.Bills.AddNew();
		bill4.B0_BillStatus = "DLR";

		bill1.SequenceNumber = 1;
		bill2.SequenceNumber = 2;
		bill3.SequenceNumber = 3;
		bill4.SequenceNumber = 4;

		_ = bill1.GoodsItems.AddNew();
		bill1.GoodsItems.AddNew().BY_Status = "DLR";
		bill1.GoodsItems.AddNew().BY_Status = "DLR";

		bill2.GoodsItems.AddNew().BY_Status = "DLR";
		_ = bill2.GoodsItems.AddNew();
		bill2.GoodsItems.AddNew().BY_Status = "DEL";

		bill3.GoodsItems.AddNew();
		bill3.GoodsItems.AddNew();

		bill4.GoodsItems.AddNew();
		bill4.GoodsItems.AddNew();

		nctsHeader.AssignDeclarationGoodsItemNumbers();

		var wrapper = GetWrapper();
		AssertNotNull(nameof(INctsAmendment.HouseConsignmentToBeDeleted), wrapper.HouseConsignmentToBeDeleted);
		AssertEquals($"{nameof(INctsAmendment.HouseConsignmentToBeDeleted)} count", 3, wrapper.HouseConsignmentToBeDeleted.Count);

		CombineAssertions("HouseConsignmentToBeDeleted 1", () =>
		{
			var houseToBeDeleted1 = wrapper.HouseConsignmentToBeDeleted.ElementAt(0);
			AssertEquals("HouseConsignmentNumberToBeDeleted", 1, houseToBeDeleted1.HouseConsignmentNumberToBeDeleted);
			AssertArrayEqualsByElements("ArticleNumberToBeDeleted", new int[] { 2, 3 }, houseToBeDeleted1.ArticleNumberToBeDeleted.ToArray());
		});

		CombineAssertions("HouseConsignmentToBeDeleted 2", () =>
		{
			var houseToBeDeleted2 = wrapper.HouseConsignmentToBeDeleted.ElementAt(1);
			AssertEquals("HouseConsignmentNumberToBeDeleted", 2, houseToBeDeleted2.HouseConsignmentNumberToBeDeleted);
			AssertArrayEqualsByElements("ArticleNumberToBeDeleted", new int[] { 1 }, houseToBeDeleted2.ArticleNumberToBeDeleted.ToArray());
		});

		CombineAssertions("HouseConsignmentToBeDeleted 3", () =>
		{
			var houseToBeDeleted2 = wrapper.HouseConsignmentToBeDeleted.ElementAt(2);
			AssertEquals("HouseConsignmentNumberToBeDeleted", 4, houseToBeDeleted2.HouseConsignmentNumberToBeDeleted);
			AssertNull("ArticleNumberToBeDeleted", houseToBeDeleted2.ArticleNumberToBeDeleted);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
	}

	INctsAmendment GetWrapper() => new NctsAmendmentWrapper(sendingObject);

	NctsHeader nctsHeader;
	NctsHeaderMessageSendingObject sendingObject;
}
