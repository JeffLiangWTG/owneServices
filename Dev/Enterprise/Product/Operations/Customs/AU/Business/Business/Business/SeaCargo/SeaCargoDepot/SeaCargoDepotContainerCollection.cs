using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for SeaCargoDepotContainerCollection.
	/// </summary>
	public class SeaCargoDepotContainerCollection : NonPersistentBusinessObjectCollection<SeaCargoDepotContainer>
	{
		public SeaCargoDepotContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(SeaCargoDepotContainer);
		}

		public void RemoveRelated(CommonContainer cFSContainerToRemove)
		{
			for (int i = Count - 1; i > 0; i--)
			{
				SeaCargoDepotContainer container = (SeaCargoDepotContainer)Elements[i];
				if (container.WrappedBusinessObject.PK == cFSContainerToRemove.PK)
				{
					Remove(container.PK);
					break;
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			CFSContainer tempContainer = Factory.New<CFSContainer>();
			SeaCargoDepotContainer result = SeaCargoDepotContainer.Load(tempContainer);
			return result;
		}
	}
}
