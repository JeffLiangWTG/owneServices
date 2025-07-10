using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.Testing.Container_Wrappers
{
	public class DocFreightBaseContainerHelperClass : DocFreightBaseContainer
	{
		public DocFreightBaseContainerHelperClass(CommonContainer baseContainer, BusinessObjectFactory factoryToWrap)
			: base(baseContainer, factoryToWrap)
		{
		}

		#region Overrides

		public override DocDocAddress JourneyOnePickUpAddress
		{
			get { return null; }
		}

		public override DocDocAddress JourneyOneDeliverToAddressForExport
		{
			get { return null; }
		}

		public override DocDocAddress JourneyOneDeliverToAddressForImport
		{
			get { return null; }
		}

		public override DocDocAddress JourneyTwoPickUpAddressForExport
		{
			get { return null; }
		}

		public override DocDocAddress JourneyTwoPickUpAddressForImport
		{
			get { return null; }
		}

		public override DocDocAddress JourneyTwoDeliverToAddress
		{
			get { return null; }
		}

		public override ZString ForwardingInstructionWeight
		{
			get { return new ZString(); }
		}

		public override ZString ForwardingInstructionVolume
		{
			get { return new ZString(); }
		}

		public override ZString ForwardingInstructionPackages
		{
			get { return new ZString(); }
		}

		public override ZDecimal TotalAllocatedShipmentWeight
		{
			get { return new ZDecimal(); }
		}

		public override ZString DescriptionAndStatus
		{
			get { return new ZString(); }
		}

		public override ZDecimal TotalAllocatedShipmentVolume
		{
			get { return new ZDecimal(); }
		}

		public override ZString TotalAllocatedShipmentVolumeUQ
		{
			get { return new ZString(); }
		}

		public override ZInt TotalAllocatedShipmentPackages
		{
			get { return new ZInt(); }
		}

		public override ZString TotalAllocatedShipmentPackagesPackType
		{
			get { return new ZString(); }
		}

		public override ZDecimal TotalPackLineWeight
		{
			get { return new ZDecimal(); }
		}

		public override ZDecimal TotalPackLineVolume
		{
			get { return new ZDecimal(); }
		}

		public override ZInt TotalPackLinePackages
		{
			get { return new ZInt(); }
		}

		#endregion
	}
}
