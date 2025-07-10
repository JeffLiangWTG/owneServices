#if DEBUG

using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.Integration
{
	public interface IAccountingDataTransferTestHelper
	{
		string SerializeDataObject<T>(T dataObject, IUniversalXmlSchema schema = null) where T : IDataObject;
	}
}

#endif
