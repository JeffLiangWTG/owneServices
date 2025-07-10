using Enterprise.Integration.Schedule;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class TransportMeanCollection : ASYCUDA.Business.TransportMeanCollection
	{
		public TransportMeanCollection(ITransportParentCommon parent) : base(parent)
		{
		}

		public new TransportMean this[int index] => (TransportMean)base[index];

		public new TransportMean AddNew() => (TransportMean)base.AddNew();
	}
}
