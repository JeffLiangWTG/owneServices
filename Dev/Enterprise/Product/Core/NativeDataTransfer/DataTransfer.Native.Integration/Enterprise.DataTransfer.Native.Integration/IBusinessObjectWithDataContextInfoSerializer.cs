using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DataTransfer.Native.Integration
{
	public interface IBusinessObjectWithDataContextInfoSerializer
	{
		SubStreamableStream SerializeToStream(BusinessObject businessObject, IDataContextDataObject dataContextInfo, List<IMessageNumber> messageNumberCollection);
	}
}
