using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaTransportDocumentCollectionSynchroniser : BusinessObjectCollectionSynchroniser
	{
		public AsycudaTransportDocumentCollectionSynchroniser(ForwardingConsol source, AsycudaBill destination)
			: base(source, destination)
		{
		}

		protected virtual AsycudaTransportDocumentSynchroniser GetAsycudaTransportDocumentSynchroniser(CusSupportingInfo asycudaTransportDocument, CusEntryNumber sourceNumber) => new AsycudaTransportDocumentSynchroniser(asycudaTransportDocument, sourceNumber);

		protected new AsycudaBill Destination
		{
			get { return (AsycudaBill)base.Destination; }
		}

		protected new ForwardingConsol Source
		{
			get { return (ForwardingConsol)base.Source; }
		}
		
		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			yield return Source.Numbers;
		}
		
		protected override void HookElementSynchronisers()
		{
		}
	}
}
