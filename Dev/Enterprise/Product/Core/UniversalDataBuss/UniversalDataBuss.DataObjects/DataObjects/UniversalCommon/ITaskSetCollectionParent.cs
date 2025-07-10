using System;
using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public interface ITaskSetCollectionParent
	{
		List<TaskSet> TaskSetCollection { get; }

		bool SetTaskSetCollection(Func<List<TaskSet>> value);
	}
}
