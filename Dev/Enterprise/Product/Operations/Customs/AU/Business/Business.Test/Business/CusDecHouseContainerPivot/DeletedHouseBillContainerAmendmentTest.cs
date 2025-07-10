using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DeletedHouseBillContainerAmendmentTest : TestCaseWithFactory
	{
		public void TestEndToEnd()
		{
			IPackingGroup amendment = new DeletedPackingGroupAmendment(1);

			AssertEquals("House Container Number", (ZShort)1, amendment.HouseContainerNumber);
			AssertEquals("Action Code", LineAction.Delete, amendment.ActionCodeForMessage(null));
			AssertEquals("NumberOfPackages", ZInt.Zero, amendment.NumberOfPackages);
			AssertEquals("WarehouseNumberOfPackages", ZInt.Zero, amendment.WarehouseNumberOfPackages);
			AssertEquals("PackingUnitCount", ZInt.Zero, amendment.PackingUnitCount);
			AssertEquals("ContainerMode", ZString.Empty, amendment.ContainerMode);
			AssertEquals("MasterBillNumber", ZString.Empty, amendment.MasterBillNumber);
			AssertEquals("HouseBillNumber", ZString.Empty, amendment.HouseBillNumber);
			AssertEquals("ContainerNumber", ZString.Empty, amendment.ContainerNumber);
			AssertEquals("MarksAndNumbers", ZString.Empty, amendment.MarksAndNumbers);
			AssertEquals("ConsignRefNumber", ZString.Empty, amendment.ConsignRefNumber);
		}
	}
}
