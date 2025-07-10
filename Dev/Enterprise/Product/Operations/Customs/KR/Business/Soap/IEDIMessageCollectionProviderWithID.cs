using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.KR.Business
{
	public interface IEDIMessageCollectionProviderWithID : IEDIMessageCollectionProvider
	{
		ZString IDNumber { get; }
		void MarkAsFailed();
	}
}
