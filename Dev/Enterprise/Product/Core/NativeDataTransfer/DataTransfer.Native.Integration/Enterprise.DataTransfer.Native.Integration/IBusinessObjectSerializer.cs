using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.IO;

namespace Enterprise.DataTransfer.Native.Integration
{
	public interface IBusinessObjectSerializer
	{
		SubStreamableStream SerializeToStream(BusinessObject businessObject);
		SubStreamableStream SerializeToStream(IEnumerable<BusinessObject> businessObject);
	}
}
