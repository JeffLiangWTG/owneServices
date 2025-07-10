using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

abstract class BaseTransitDeclarationDataProviderTest<TDataProvider, TMessage> : BaseDepartureDataProviderTest<TDataProvider, NctsHeaderDepartureMessageSendingObject>
	where TDataProvider : BaseTransitDeclarationDataProvider
{
	protected override TDataProvider CreateDataProvider() => CreateDataProvider(MessageSendingObject);

	protected abstract TDataProvider CreateDataProvider(NctsHeaderDepartureMessageSendingObject messageSendingObject);

	public void TestConstructorNullArgument()
	{
		AssertExceptionThrown<ArgumentNullException>("Argument == null", () => CreateDataProvider(null));
	}

	public void TestTransitOperation()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.TransitOperation);
			AssertSame("cached", DataProvider.TransitOperation, DataProvider.TransitOperation);
		});
	}

	public void TestGuarantees() => CombineAssertions(() =>
	{
		NctsHeader.MovementHeader.Guarantees.AddNew().PW_BondType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
		NctsHeader.MovementHeader.Guarantees.AddNew().PW_BondType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
		AssertEquals(2, DataProvider.Guarantees.Count);
		AssertSame("cached", DataProvider.Guarantees, DataProvider.Guarantees);
	});

	public void TestCustomsOfficeOfDestination()
	{
		NctsHeader.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.CustomsOfficeOfDestination);
			AssertSame("cached", DataProvider.CustomsOfficeOfDestination, DataProvider.CustomsOfficeOfDestination);
		});
	}

	public void TestCustomsOfficesOfTransit()
	{
		NctsHeader.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.CustomsOfficesOfTransit);
			AssertSame("cached", DataProvider.CustomsOfficesOfTransit, DataProvider.CustomsOfficesOfTransit);
		});
	}

	public void TestCustomsOfficesOfExitForTransit()
	{
		NctsHeader.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.CustomsOfficesOfExitForTransit);
			AssertSame("cached", DataProvider.CustomsOfficesOfExitForTransit, DataProvider.CustomsOfficesOfExitForTransit);
		});
	}

	public void TestConsignment()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.Consignment);
			AssertSame("cached", DataProvider.Consignment, DataProvider.Consignment);
		});
	}

	public void TestHolderOfTransitProcedure()
	{
		ZString customsRegNo = "123";
		NctsHeader.Principal.OrganisationPK = Factory.New<OrgHeader>().PK;
		NctsHeader.Principal.Organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, customsRegNo);

		CombineAssertions(() =>
		{
			AssertType<HolderOfTransitProcedureDataProvider>(DataProvider.HolderOfTransitProcedure);
			AssertSame("cached", DataProvider.HolderOfTransitProcedure, DataProvider.HolderOfTransitProcedure);
			AssertEquals("Principal is used", customsRegNo, DataProvider.HolderOfTransitProcedure.IdentificationNumber);
		});
	}

	public void TestRepresentative()
	{
		ZString customsRegNo = "123";
		NctsHeader.MovementHeader.Representative.OrganisationPK = Factory.New<OrgHeader>().PK;
		NctsHeader.MovementHeader.Representative.Organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, customsRegNo);

		CombineAssertions(() =>
		{
			AssertType<PersonDataProvider>(DataProvider.Representative);
			AssertSame("cached", DataProvider.Representative, DataProvider.Representative);
			AssertEquals("Representative is used", customsRegNo, DataProvider.Representative.IdentificationNumber);
			AssertEquals("Representative.ContactPerson.Name", GlbStaff.CurrentUser.GS_FullName, DataProvider.Representative.ContactPerson.Name);
		});
	}

	public void TestConsignmentItemDeclarationGoodsItemNumber()
	{
		var bill1 = NctsHeader.Bills.AddNew();
		bill1.GoodsItems.AddNew();
		bill1.GoodsItems.AddNew();
		var bill2 = NctsHeader.Bills.AddNew();
		bill2.GoodsItems.AddNew();
		bill2.GoodsItems.AddNew();
		bill2.GoodsItems.AddNew();

		NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(NctsHeader);

		CombineAssertions(() =>
		{
			AssertEquals("bill1 item1", 1, DataProvider.Consignment.HouseConsignments.ElementAt(0).ConsignmentItems.ElementAt(0).DeclarationGoodsItemNumber);
			AssertEquals("bill1 item2", 2, DataProvider.Consignment.HouseConsignments.ElementAt(0).ConsignmentItems.ElementAt(1).DeclarationGoodsItemNumber);
			AssertEquals("bill2 item1", 3, DataProvider.Consignment.HouseConsignments.ElementAt(1).ConsignmentItems.ElementAt(0).DeclarationGoodsItemNumber);
			AssertEquals("bill2 item2", 4, DataProvider.Consignment.HouseConsignments.ElementAt(1).ConsignmentItems.ElementAt(1).DeclarationGoodsItemNumber);
			AssertEquals("bill2 item3", 5, DataProvider.Consignment.HouseConsignments.ElementAt(1).ConsignmentItems.ElementAt(2).DeclarationGoodsItemNumber);
		});
	}

	public void TestTransportEquipmentDeclarationGoodsItemNumber()
	{
		var headerContainer1 = NctsHeader.DepartureHeaderContainers.AddNew();
		var headerContainer2 = NctsHeader.DepartureHeaderContainers.AddNew();
		var bill1 = NctsHeader.Bills.AddNew();
		addGoodsItem(bill1, headerContainer1);
		addGoodsItem(bill1, headerContainer2);
		var bill2 = NctsHeader.Bills.AddNew();
		addGoodsItem(bill2, headerContainer2);
		addGoodsItem(bill2, headerContainer1);
		addGoodsItem(bill2, headerContainer1);

		void addGoodsItem(NctsBill bill, NctsDepartureHeaderContainer headerContainer)
		{
			var goodsItem = bill.GoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();
			package.ContainersPivot.AddPivotFor(headerContainer);
		}

		NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(NctsHeader);

		CombineAssertions(() =>
		{
			AssertEquals("bill1 item1", 1, DataProvider.Consignment.TransportEquipments.ElementAt(0).GoodsReferences.ElementAt(0).DeclarationGoodsItemNumber);
			AssertEquals("bill1 item2", 2, DataProvider.Consignment.TransportEquipments.ElementAt(1).GoodsReferences.ElementAt(0).DeclarationGoodsItemNumber);
			AssertEquals("bill2 item1", 3, DataProvider.Consignment.TransportEquipments.ElementAt(1).GoodsReferences.ElementAt(1).DeclarationGoodsItemNumber);
			AssertEquals("bill2 item2", 4, DataProvider.Consignment.TransportEquipments.ElementAt(0).GoodsReferences.ElementAt(1).DeclarationGoodsItemNumber);
			AssertEquals("bill2 item3", 5, DataProvider.Consignment.TransportEquipments.ElementAt(0).GoodsReferences.ElementAt(2).DeclarationGoodsItemNumber);
		});
	}
}
