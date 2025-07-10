using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DE.Business
{
	public interface IInboundMessageDataProvider<TIDataProvider>
		where TIDataProvider : IDataProvider
	{
		TIDataProvider DataProvider { get; }

		List<AttachedDocument> AttachedDocuments { get; }

		(bool Success, ResponseMessageDetails ResponseMessageDetails) GetResponseMessageDetails(string applicationReference);
	}
}
