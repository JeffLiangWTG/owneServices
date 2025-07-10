using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing.ContainerWrappers
{
	public abstract class DocFreightBaseContainerAbstractTest : DocumentWrapperTestCase
	{
		#region Abstract Fields

		#region Cartage Advice Fields

		public abstract void TestJourneyOnePickUpAddress();
		public abstract void TestJourneyOneDeliverToAddressForExport();
		public abstract void TestJourneyOneDeliverToAddressForImport();
		public abstract void TestJourneyTwoPickUpAddressForExport();
		public abstract void TestJourneyTwoPickUpAddressForImport();
		public abstract void TestJourneyTwoDeliverToAddress();

		#endregion

		#region IDocContainer Members

		public abstract void TestForwardingInstructionWeight();
		public abstract void TestForwardingInstructionVolume();
		public abstract void TestForwardingInstructionPackages();
		public abstract void TestDescriptionAndStatus();
		public abstract void TestTotalAllocatedShipmentWeight();
		public abstract void TestTotalAllocatedShipmentVolume();
		public abstract void TestTotalPackLineWeight();
		public abstract void TestTotalPackLineVolume();
		public abstract void TestTotalPackLinePackages();

		#endregion

		#endregion
	}
}
