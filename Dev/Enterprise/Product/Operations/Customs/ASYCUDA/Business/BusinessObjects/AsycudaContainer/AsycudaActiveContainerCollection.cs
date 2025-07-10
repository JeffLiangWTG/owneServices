using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	/// <summary>
	/// We can get rid of this once ICusInBondContainerCollection demands IDependentBusinessObjectCollection, not IActiveBusinessObjectCollection.
	/// </summary>
	public class AsycudaActiveContainerCollection : ActiveBusinessObjectCollection<AsycudaContainer>, Customs.Business.ICusInBondContainerCollection
	{
		public AsycudaActiveContainerCollection(AsycudaManifestHeader master)
			: base(master.Factory, master, new ZQuery(), AsycudaContainerSchema.ACN_AMA_Manifest)
		{
		}

		public AsycudaContainer this[string containerNum]
		{
			get
			{
				AsycudaContainer result = null;
				foreach (AsycudaContainer container in this)
				{
					if (!container.IsDeleted && container.ACN_ContainerNumber == containerNum)
					{
						result = container;
						break;
					}
				}
				return result;
			}
		}

		BusinessObject Customs.Business.ICusInBondContainerCollection.this[ZString containerNum]
		{
			get { return this[containerNum]; }
		}

		BusinessObject Customs.Business.ICusInBondContainerCollection.this[int index] => this[index];
	}
}
