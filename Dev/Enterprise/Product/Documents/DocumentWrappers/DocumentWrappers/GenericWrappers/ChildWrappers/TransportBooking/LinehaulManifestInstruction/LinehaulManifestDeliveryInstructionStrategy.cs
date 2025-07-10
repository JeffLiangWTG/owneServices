using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class LinehaulManifestDeliveryInstructionStrategy : LinehaulManifestCommonInstructionStrategy
	{
		#region constructor

		public LinehaulManifestDeliveryInstructionStrategy(DtbLinehaulManifest manifest, BusinessObjectFactory factory)
		{
			LinehaulManifest = manifest;
			Factory = factory;
		}

		readonly DtbLinehaulManifest LinehaulManifest;
		readonly BusinessObjectFactory Factory;

		#endregion

		#region GetAddress

		public override AddressWrapper GetAddress()
		{
			return new AddressWrapper(LinehaulManifest.DestinationDepot, ContactType.Depot, Factory);
		}

		#endregion

		#region GetSequence

		public override ZInt GetSequence()
		{
			return 2;
		}

		#endregion

		#region GetInstructionType

		public override ZString GetInstructionType()
		{
			return InstructionTypes.Descriptions.Delivery;
		}

		#endregion
	}
}
