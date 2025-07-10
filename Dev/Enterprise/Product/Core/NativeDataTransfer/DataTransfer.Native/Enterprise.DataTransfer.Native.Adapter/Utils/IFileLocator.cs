using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;

namespace Enterprise.DataTransfer.Native.Adapter.Utils
{
	public interface ISaveFileLocator
	{
		Stream GetFileStream();
		Stream GetFileStream(IEnumerable<IBusiness> businessObjects, out string displayFileName);
	}
}
