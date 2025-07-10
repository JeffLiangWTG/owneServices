using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsArrivalHeaderContainerCollection))]
	sealed class NctsArrivalHeaderContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return containers;
		}

		protected override System.Type GetExpectedCollectionType()
		{
			return typeof(NctsArrivalHeaderContainerCollection);
		}

		public void TestSetDefaultsForNewChildPhase5()
		{
			CombineAssertions(() =>
			{
				var container1 = containers.AddNew();
				AssertEquals("container1 doesn't have BC_Mode set", ZString.Empty, container1.BC_Mode);
				AssertEquals("Sequence of container1 should be 1", (ZShort)1, container1.BC_SequenceNumber);
				container1.BC_Mode = Constants.ContainerModes.Containerised;
				var container2 = containers.AddNew();
				AssertEquals("container2 has BC_Mode the same as the container1", container2.BC_Mode, container1.BC_Mode);
				AssertEquals("Sequence of container2 should be 2", (ZShort)2, container2.BC_SequenceNumber);

				containers.Sort(new SortInfo(nameof(NctsArrivalHeaderContainer.BC_SequenceNumber), ListSortDirection.Descending));
				AssertEquals("First collection element should be container2", container2.BC_SequenceNumber, containers[0].BC_SequenceNumber);
				AssertEquals("Second collection element should be container1", container1.BC_SequenceNumber, containers[1].BC_SequenceNumber);

				var container3 = containers.AddNew();
				AssertEquals("Sequence of container3 should be 3", (ZShort)3, container3.BC_SequenceNumber);

				containers.RemoveAndDelete(container1);
				containers.Sort(new SortInfo(nameof(NctsArrivalHeaderContainer.BC_SequenceNumber), ListSortDirection.Descending));
				AssertEquals("First collection element should be container3", container3.BC_SequenceNumber, containers[0].BC_SequenceNumber);
				AssertEquals("Second collection element should be container2", container2.BC_SequenceNumber, containers[1].BC_SequenceNumber);

				var container4 = containers.AddNew();
				AssertEquals("Sequence of container4 should be 3", (ZShort)3, container4.BC_SequenceNumber);
			});
		}

		public void TestOnRemoved()
		{
			var container1 = containers.AddNew();
			var container2 = containers.AddNew();
			var container3 = containers.AddNew();
			var container4 = containers.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Sequence of container1 should be 1", (ZShort)1, container1.BC_SequenceNumber);
				AssertEquals("Sequence of container2 should be 2", (ZShort)2, container2.BC_SequenceNumber);
				AssertEquals("Sequence of container3 should be 3", (ZShort)3, container3.BC_SequenceNumber);
				AssertEquals("Sequence of container4 should be 4", (ZShort)4, container4.BC_SequenceNumber);

				container2.Delete();
				AssertEquals("Sequence of container1 should remain as 1", (ZShort)1, container1.BC_SequenceNumber);
				AssertEquals("Sequence of container3 should recalculate to 2", (ZShort)2, container3.BC_SequenceNumber);
				AssertEquals("Sequence of container4 should recalculate to 3", (ZShort)3, container4.BC_SequenceNumber);
			});
		}

		public void TestISequenceNumberHeader()
		{
			var container1 = containers.AddNew();
			var container2 = containers.AddNew();
			var sequenceHeader = containers as ISequenceNumberHeader;
			AssertContainsExactElementsInExactOrder("SequenceNumberHeader.Lines", new[] { container1, container2 }, sequenceHeader.Lines);
		}

		public void TestLoad_Arrival()
		{
			var container = containers.AddNew();
			container.BC_ContainerNum = "MSCU1234566";
			containers = header.ArrivalHeaderContainers;
			containers.Load();
			AssertEquals("Number of containers should be 1", 1, containers.Count);
		}

		public void TestNoContainersLoaded_Phase4Arrival()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var containers = header.ArrivalHeaderContainers;
			var container = containers.AddNew();
			container.BC_ContainerNum = "MSCU1234566";
			Factory.Save();

			var reloadedHeader = new BusinessObjectFactory().Load<NctsHeader>(header.PK);
			var reloadedContainers = reloadedHeader.ArrivalHeaderContainers;
			AssertEquals("Number of containers should be 0", 0, reloadedContainers.Count);
		}

		public void TestLoad_Departure()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var containers = header.DepartureHeaderContainers;
			var container = containers.AddNew();
			container.BC_ContainerNum = "MSCU1234566";
			Factory.Save();
			var containersA = header.ArrivalHeaderContainers;
			containersA.Load();
			AssertEquals("Number of containers should be 0", 0, containersA.Count);
		}

		public void TestRemoveAndDeleteAll_NoExceptionThrown()
		{
			var container = containers.AddNew();
			var seal = container.Seals.AddNew();
			seal.BK_SealNumber = "1234";

			Factory.Save();

			var secondFactory = new BusinessObjectFactory();
			var reloadedHeader = secondFactory.Load<NctsHeader>(header.PK);

			AssertNoExceptionThrown(() => reloadedHeader.ArrivalHeaderContainers.RemoveAndDeleteAll());
		}

		public void TestAllowNew()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var containers = header.ArrivalHeaderContainers;

			AssertEquals(false, containers.AllowNew);

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			containers = header.ArrivalHeaderContainers;
			AssertEquals(true, containers.AllowNew);

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			containers = header.ArrivalHeaderContainers;
			AssertEquals(false, containers.AllowNew);
		}

		protected override void SetUp()
		{
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			containers = header.ArrivalHeaderContainers;
		}

		NctsArrivalHeaderContainerCollection containers;
		NctsHeader header;
	}
}
