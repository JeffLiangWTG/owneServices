using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Testing
{
	public static class UniversalDocumentRequestCreator
	{
		public static DocumentRequest Create(IDataContextDataObject dataContext)
		{
			var request = new DocumentRequest();
			request.DataContext = dataContext;
			return request;
		}
	}
}
