using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers
{
	public class LinehaulManifestPickupInstructionStrategy : LinehaulManifestCommonInstructionStrategy
	{
		#region constructor

		public LinehaulManifestPickupInstructionStrategy(DtbLinehaulManifest manifest, BusinessObjectFactory factory)
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
			return new AddressWrapper(LinehaulManifest.OriginDepot, ContactType.Depot, Factory);
		}

		#endregion

		#region GetSequence

		public override ZInt GetSequence()
		{
			return 1;
		}

		#endregion

		#region GetInstructionType

		public override ZString GetInstructionType()
		{
			return InstructionTypes.Descriptions.PickUp;
		}

		#endregion
	}
}
