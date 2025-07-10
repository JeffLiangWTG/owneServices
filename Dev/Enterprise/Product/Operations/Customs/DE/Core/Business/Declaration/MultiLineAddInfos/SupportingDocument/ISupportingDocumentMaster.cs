using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	public interface ISupportingDocumentMaster
	{
		HugeSequenceNumberGenerator LineNumberGenerator { get; }
	}
}
