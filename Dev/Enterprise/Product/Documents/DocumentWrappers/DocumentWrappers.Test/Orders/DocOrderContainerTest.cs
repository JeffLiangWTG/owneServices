using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Orders.Testing
{
	[TestedType(typeof(DocOrderContainer))]
	sealed class DocOrderContainerTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocOrderContainer.New(OrderContainerBisObj, Factory)
			};
		}

		#region Overrides

		public void TestToString()
		{
			ZString containerNumber = new ZString("Num");
			OrderContainerBisObj.J1_ContainerNumber = containerNumber;
			AssertEquals("ToString", containerNumber, OrderContainerWrapper.ToString());

			containerNumber = new ZString("AnotherNum");
			OrderContainerBisObj.J1_ContainerNumber = containerNumber;
			AssertEquals("ToString", containerNumber, OrderContainerWrapper.ToString());
		}

		#endregion

		#region ZByte Fields

		public void TestContainerCount()
		{
			ZShort containerCount = 1;
			OrderContainerBisObj.J1_ContainerCount = containerCount;
			AssertEquals("Container Count", containerCount, OrderContainerWrapper.ContainerCount);

			containerCount = 3;
			OrderContainerBisObj.J1_ContainerCount = containerCount;
			AssertEquals("Container Count", containerCount, OrderContainerWrapper.ContainerCount);
		}

		#endregion

		#region ZString

		public void TestContainerNumber()
		{
			ZString containerNumber = new ZString("ConNum");
			OrderContainerBisObj.J1_ContainerNumber = containerNumber;
			AssertEquals("Container Number", containerNumber, OrderContainerWrapper.ContainerNumber);

			containerNumber = new ZString("OtherNum");
			OrderContainerBisObj.J1_ContainerNumber = containerNumber;
			AssertEquals("Container Number", containerNumber, OrderContainerWrapper.ContainerNumber);
		}

		public void TestSize()
		{
			RefContainer @ref = Factory.New<RefContainer>();
			OrderContainerBisObj.J1_RC = @ref.PK;
			@ref.RC_Code = "20FR";
			AssertEquals("Size should be 20FR", "20FR", OrderContainerWrapper.Size);
		}

		#endregion

		#region Wrapper Fields

		public void TestRefContainer()
		{
			AssertNull("RefContainer", OrderContainerWrapper.Container);

			var container = Factory.LoadTop1<RefContainer>(new ZQuery());
			OrderContainerBisObj.J1_RC = container.PK;
			AssertNotNull("RefContainer", OrderContainerWrapper.Container);
			AssertEquals("RefContainer is of type DocRefContainer", typeof(DocRefContainer), OrderContainerWrapper.Container.GetType());
		}

		#endregion

		#region ZDateTime Fields
		public void TestEmptyReturnedBy()
		{
			AssertEquals("Empty Returned By", ZDateTime.Empty, OrderContainerWrapper.EmptyReturnedBy);
		}

		public void TestContainerParkEmptyReturnGateIn()
		{
			AssertEquals("Container Park Empty Return GateIn", ZDateTime.Empty, OrderContainerWrapper.ContainerParkEmptyReturnGateIn);
		}

		#endregion

		#region IDocContainer

		#region ZString Fields

		public void TestSealNumber()
		{
			ZShort containerCount = 1;
			OrderContainerBisObj.J1_ContainerCount = containerCount;
			AssertEquals("Seal number", containerCount.ToString(), OrderContainerWrapper.SealNumber);

			containerCount = 3;
			OrderContainerBisObj.J1_ContainerCount = containerCount;
			AssertEquals("Seal number", containerCount.ToString(), OrderContainerWrapper.SealNumber);
		}

		public void TestClientRef()
		{
			AssertEquals("ClientRef", ZString.Empty, OrderContainerWrapper.ClientRef);
		}

		public void TestContainerMode()
		{
			AssertEquals("ContainerMode", ZString.Empty, OrderContainerWrapper.ContainerMode);
		}

		public void TestTempRecorderSerialNo()
		{
			AssertEquals("TempRecorderSerialNo", ZString.Empty, OrderContainerWrapper.TempRecorderSerialNo);
		}

		public void TestForwardingInstructionPackages()
		{
			AssertEquals("ForwardingInstructionPackages ", ZString.Empty, OrderContainerWrapper.ForwardingInstructionPackages);
		}

		public void TestForwardingInstructionWeightHeading()
		{
			AssertEquals("ForwardingInstructionWeightHeading ", ZString.Empty, OrderContainerWrapper.ForwardingInstructionWeightHeading);
		}

		public void TestForwardingInstructionVolumeHeading()
		{
			AssertEquals("ForwardingInstructionVolumeHeading ", ZString.Empty, OrderContainerWrapper.ForwardingInstructionVolumeHeading);
		}

		public void TestForwardingInstructionPackageHeading()
		{
			AssertEquals("ForwardingInstructionPackageHeading ", ZString.Empty, OrderContainerWrapper.ForwardingInstructionPackageHeading);
		}

		public void TestForwardingInstructionWeight()
		{
			AssertEquals("ForwardingInstructionWeight ", ZString.Empty, OrderContainerWrapper.ForwardingInstructionWeight);
		}

		public void TestForwardingInstructionVolume()
		{
			AssertEquals("ForwardingInstructionVolume ", ZString.Empty, OrderContainerWrapper.ForwardingInstructionVolume);
		}

		public void TestDescriptionAndStatus()
		{
			AssertEquals("DescriptionAndStatus ", ZString.Empty, OrderContainerWrapper.DescriptionAndStatus);
		}

		public void TestDeliveryMode()
		{
			AssertEquals("DeliveryMode ", ZString.Empty, OrderContainerWrapper.DeliveryMode);
		}

		public void TestTareWeightWithUQ()
		{
			AssertEquals("TareWeightWithUQ ", ZString.Empty, OrderContainerWrapper.TareWeightWithUQ);
		}

		public void TestGrossWeightWithUQ()
		{
			AssertEquals("GrossWeightWithUQ ", ZString.Empty, OrderContainerWrapper.GrossWeightWithUQ);
		}

		public void TestWeightUQ()
		{
			AssertEquals("WeightUQ ", ZString.Empty, OrderContainerWrapper.WeightUQ);
		}

		#endregion

		#region ZDecimal Fields

		public void TestTotalAllocatedShipmentWeight()
		{
			AssertEquals("TotalPackLineWeightForContainerForOneShipment", 0M, OrderContainerWrapper.TotalAllocatedShipmentWeight);
		}

		public void TestTotalAllocatedShipmentVolume()
		{
			AssertEquals("TotalPackLineVolumeForContainerForOneShipment", 0M, OrderContainerWrapper.TotalAllocatedShipmentVolume);
		}

		public void TestTotalPackLineVolume()
		{
			AssertEquals("TotalPackLineVolumeForContainer ", 0M, OrderContainerWrapper.TotalPackLineVolume);
		}

		public void TestTotalPackLineWeight()
		{
			AssertEquals("TotalPackLineWeightForContainer ", 0M, OrderContainerWrapper.TotalPackLineWeight);
		}

		#endregion

		#region ZDateTime Fields

		public void TestContainerAvailable()
		{
			AssertEquals("ContainerAvailable", ZDateTime.Empty, OrderContainerWrapper.ContainerAvailable);
		}

		public void TestLCLAvailable()
		{
			AssertEquals("LCLAvailable", ZDateTime.Empty, OrderContainerWrapper.LCLAvailable);
		}

		public void TestStorageCommences()
		{
			AssertEquals("StorageCommences", ZDateTime.Empty, OrderContainerWrapper.StorageCommences);
		}

		public void TestLCLStorageCommences()
		{
			AssertEquals("LCLStorageCommences", ZDateTime.Empty, OrderContainerWrapper.LCLStorageCommences);
		}

		#endregion

		#region ZInt Fields

		public void TestTotalPackLinePackages()
		{
			AssertEquals("TotalPackLinePackagesForContainer ", 0, OrderContainerWrapper.TotalPackLinePackages);
		}

		public void TestTotalAllocatedShipmentPackages()
		{
			AssertEquals("TotalAllocatedShipmentPackages", 0, OrderContainerWrapper.TotalAllocatedShipmentPackages);
		}

		public void TestQuantityCount()
		{
			OrderContainerBisObj.J1_ContainerCount = 10;
			OrderContainerWrapper = DocOrderContainer.New(OrderContainerBisObj, Factory);
			AssertEquals("Container count", 10, OrderContainerWrapper.QuantityCount);
		}
		#endregion

		#region Extra CommonCartage Field Tests

		public void TestContainerLegs()
		{
			AssertEquals("ContainerLegs collection should be cached.", OrderContainerWrapper.ContainerLegs, OrderContainerWrapper.ContainerLegs);
			AssertEquals("ContainerLegs not empty.", 0, OrderContainerWrapper.ContainerLegs.Count);
		}

		public void TestCartageParent()
		{
			AssertNull("CartageParent not null.", OrderContainerWrapper.Cartage);
		}

		public void TestContainerType()
		{
			AssertEquals("ContainerType not empty.", "", OrderContainerWrapper.ContainerType);
		}

		public void TestTareWeight()
		{
			AssertEquals("TareWeight not zero.", 0m, OrderContainerWrapper.TareWeight);
		}

		public void TestGrossWeight()
		{
			AssertEquals("GrossWeight not zero.", 0m, OrderContainerWrapper.GrossWeight);
		}

		public void TestTotalVolume()
		{
			AssertEquals("TotalVolume not zero.", 0m, OrderContainerWrapper.TotalVolume);
		}

		public void TestSlotArrivalReference()
		{
			AssertEquals("SlotArrivalReference not empty.", "", OrderContainerWrapper.SlotArrivalReference);
		}

		public void TestSlotDepartureReference()
		{
			AssertEquals("SlotDepartureReference not empty.", "", OrderContainerWrapper.SlotDepartureReference);
		}

		public void TestSlotArrivalTime()
		{
			AssertEquals("SlotArrivalTime not empty.", ZDateTime.Empty, OrderContainerWrapper.SlotArrivalTime);
		}

		public void TestSlotDepartureTime()
		{
			AssertEquals("SlotDepartureTime not empty.", ZDateTime.Empty, OrderContainerWrapper.SlotDepartureTime);
		}

		public void TestSlotArrivalDetails()
		{
			AssertEquals("SlotArrivalDetails not empty.", "", OrderContainerWrapper.SlotArrivalDetails);
		}

		public void TestSlotDepartureDetails()
		{
			AssertEquals("SlotDepartureDetails not empty.", "", OrderContainerWrapper.SlotDepartureDetails);
		}

		public void TestSlotAsArrivalOrDeparture()
		{
			AssertEquals("SlotAsArrivalOrDeparture not empty.", "", OrderContainerWrapper.SlotAsArrivalOrDeparture);
		}

		#endregion

		#region IDocSimpleContainer Tests

		public void TestTotalAllocatedJobPackages()
		{
			AssertEquals("Should return 0", 0, OrderContainerWrapper.TotalAllocatedJobPackages);
		}

		public void TestTotalAllocatedJobWeight()
		{
			AssertEquals("Should return 0", 0M, OrderContainerWrapper.TotalAllocatedJobWeight);
		}

		public void TestTotalAllocatedJobVolume()
		{
			AssertEquals("Should return 0", 0M, OrderContainerWrapper.TotalAllocatedJobVolume);
		}

		public void TestType()
		{
			RefContainer @ref = Factory.New<RefContainer>();
			OrderContainerBisObj.J1_RC = @ref.PK;
			@ref.RC_Code = "20FR";
			AssertEquals("Should return 20FR", "20FR", OrderContainerWrapper.Type);
		}

		#endregion

		#endregion

		#region Implementation

		OrderContainer OrderContainerBisObj;
		DocOrderContainer OrderContainerWrapper;

		protected override void SetUp()
		{
			OrderContainerBisObj = Factory.New<OrderContainer>();
			OrderContainerWrapper = DocOrderContainer.New(OrderContainerBisObj, Factory);
			base.SetUp();
		}

		#endregion
	}
}
