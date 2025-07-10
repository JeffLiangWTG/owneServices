using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaTransportDocumentSynchroniser : BusinessObjectSynchroniser
	{
		public AsycudaTransportDocumentSynchroniser(CusSupportingInfo destination, CusEntryNumber source)
			: base(destination, source)
		{ }
	}
}
