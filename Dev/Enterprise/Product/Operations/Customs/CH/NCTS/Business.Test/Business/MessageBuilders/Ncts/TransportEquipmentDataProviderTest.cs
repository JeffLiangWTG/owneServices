using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

internal class TransportEquipmentDataProviderTest : TestCaseWithFactory
{
	public void TestNewCollection()
	{
		CombineAssertions(() =>
		{
			AssertNull("null", TransportEquipmentDataProvider.NewCollection(null));

			AssertNotNull("NctsHeader & Containers != null", TransportEquipmentDataProvider.NewCollection(NctsHeader.DepartureHeaderContainers));
		});
	}

	public void TestProvider()
	{
		var headerContainer = NctsHeader.DepartureHeaderContainers.AddNew();
		headerContainer.BC_Mode = Constants.ContainerModes.Containerised;
		headerContainer.BC_ContainerNum = "ABCD1234";
		headerContainer.Seal1 = "1234";

		CombineAssertions(() =>
		{
			AssertEquals("Equipment Row Count", 1, TransportEquipment.Count);
			AssertEquals("Seals Row Count", 1, TransportEquipment.ElementAt(0).Seals.Count);

			AssertEquals("Sequence Number", 1, TransportEquipment.ElementAt(0).SequenceNumber);
			AssertEquals("Container Identification Number", "ABCD1234", TransportEquipment.ElementAt(0).ContainerIdentificationNumber);
			AssertEquals("Number Of Seals", 1, TransportEquipment.ElementAt(0).NumberOfSeals);
			AssertEquals("Seal1 Identifier", "1234", TransportEquipment.ElementAt(0).Seals.ElementAt(0).Identifier);
		});
	}

	public void TestSequenceNumber()
	{
		var headerContainer1 = NctsHeader.DepartureHeaderContainers.AddNew();
		var headerContainer2 = NctsHeader.DepartureHeaderContainers.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Equipment Row Count", 2, TransportEquipment.Count);
			AssertEquals("Sequence Number", 1, TransportEquipment.ElementAt(0).SequenceNumber);
			AssertEquals("Sequence Number", 2, TransportEquipment.ElementAt(1).SequenceNumber);
		});
	}

	public void TestContainerIdentificationNumber()
	{
		var headerContainer = NctsHeader.DepartureHeaderContainers.AddNew();

		CombineAssertions(() =>
		{
			AssertNull("no Identification Number specified", TransportEquipment.ElementAt(0).ContainerIdentificationNumber);

			headerContainer.BC_ContainerNum = "ABCD1234";
			AssertNull("Container Identification Number entered - Container Mode empty - null", TransportEquipment.ElementAt(0).ContainerIdentificationNumber);

			headerContainer.BC_Mode = Constants.ContainerModes.NonContainerised;
			AssertNull("Container Identification Number entered - Container Mode NonContainerised - null", TransportEquipment.ElementAt(0).ContainerIdentificationNumber);

			headerContainer.BC_Mode = Constants.ContainerModes.Containerised;
			AssertEquals("Container Identification Number entered - Container Mode Containerised - contained", "ABCD1234", TransportEquipment.ElementAt(0).ContainerIdentificationNumber);
		});
	}

	public void TestSeals()
	{
		var headerContainer = NctsHeader.DepartureHeaderContainers.AddNew();
		headerContainer.Seal1 = "1234";

		CombineAssertions(() =>
		{
			AssertEquals("Number Of Seals", 1, TransportEquipment.ElementAt(0).NumberOfSeals);
			AssertEquals("Rows Of Seals", 1, TransportEquipment.ElementAt(0).Seals.Count);
			AssertEquals("Seal1 Identifier", "1234", TransportEquipment.ElementAt(0).Seals.ElementAt(0).Identifier);
			AssertSame("cached", TransportEquipment.ElementAt(0).Seals, TransportEquipment.ElementAt(0).Seals);
		});
	}

	public void TestGoodsReferences()
	{
		var headerContainer = NctsHeader.DepartureHeaderContainers.AddNew();
		var goodsItems = NctsHeader.Bills.AddNew().GoodsItems;
		var goodsItem = goodsItems.AddNew();
		var package1 = goodsItem.Packages.AddNew();
		package1.ContainersPivot.AddPivotFor(headerContainer);

		CombineAssertions(() =>
		{
			AssertEquals("Row Of GoodsReferences", 1, TransportEquipment.ElementAt(0).GoodsReferences.Count);
			AssertSame("cached", TransportEquipment.ElementAt(0).GoodsReferences, TransportEquipment.ElementAt(0).GoodsReferences);
		});
	}

	NctsHeader CreateNctsHeader()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader;
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	public IReadOnlyCollection<ITransportEquipment> TransportEquipment => transportEquipment ?? (transportEquipment = TransportEquipmentDataProvider.NewCollection(NctsHeader.DepartureHeaderContainers).ToArray());
	IReadOnlyCollection<ITransportEquipment> transportEquipment;
}
