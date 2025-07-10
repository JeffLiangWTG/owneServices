using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	sealed class AsycudaContainerSynchronisationTargetCollection : ISailingSynchronisationTargetCollection<BillOfLadingContainer, AsycudaContainer>
	{
		public AsycudaContainerSynchronisationTargetCollection(IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> targetCollection)
		{
			this.targetCollection = targetCollection;
		}

		readonly IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> targetCollection;

		#region ISailingSynchronisationTargetCollection

		AsycudaContainer ISailingSynchronisationTargetCollection<BillOfLadingContainer, AsycudaContainer>.AddNew()
		{
			return targetCollection.AddNew();
		}

		void ISailingSynchronisationTargetCollection<BillOfLadingContainer, AsycudaContainer>.Delete(AsycudaContainer target)
		{
			targetCollection.RemoveAndDelete(target);
		}

		public IEnumerator<AsycudaContainer> GetEnumerator()
		{
			return targetCollection.Cast<AsycudaContainer>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
