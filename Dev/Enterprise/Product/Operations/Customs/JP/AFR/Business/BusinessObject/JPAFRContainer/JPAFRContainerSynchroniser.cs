using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRContainerSynchroniser : BusinessObjectSynchroniser
	{
		public JPAFRContainerSynchroniser(JPAFRContainer destination, ForwardingContainer source)
			: base(destination, source)
		{
		}

		public new JPAFRContainer Destination
		{
			get { return (JPAFRContainer)base.Destination; }
		}

		public new ForwardingContainer Source
		{
			get { return (ForwardingContainer)base.Source; }
		}

		#region Implementation

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.JPC_ContainerNumInfo, Source.JC_ContainerNumInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPC_Seal1Info, Source.JC_SealNumInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPC_Seal2Info, Source.JC_AdditionalSealNumInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPC_RC_ContainerTypeInfo, Source.JC_RCInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPC_IsEmptyInfo, Source.JC_IsEmptyContainerInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPC_OwnershipCodeInfo, GetOwnershipCode, GetInfosAffectingOwnership, GetOwnershipReadOnlyFlag));
		}

		#region Sync of ContainerOwnership

		IZType GetOwnershipCode()
		{
			return Source.JC_IsShipperOwned ? new ZString(ContainerOwnershipCodeList.Codes.ShipperSupplied) : Destination.JPC_OwnershipCode;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingOwnership()
		{
			yield return Source.JC_IsShipperOwnedInfo;
		}

		bool GetOwnershipReadOnlyFlag()
		{
			return !Source.JC_IsShipperOwned;
		}

		#endregion

		#endregion
	}
}
