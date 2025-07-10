using System;
using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public interface ITaskCollectionParent
	{
		List<Task> TaskCollection { get; }

		bool SetTaskCollection(Func<List<Task>> value);
	}
}
