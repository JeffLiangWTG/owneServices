using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.UniversalDataBuss.Integration.Management
{
	public interface IUniversalTaskWriter
	{
		void PopulateTasks(IEnumerable<IProcessTask> tasks, IDataObject destination);
	}
}
