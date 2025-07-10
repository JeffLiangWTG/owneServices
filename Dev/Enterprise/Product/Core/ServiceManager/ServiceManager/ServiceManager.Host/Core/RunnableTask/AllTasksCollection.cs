using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.ServiceManager.Host.Http;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	sealed class AllTasksCollection : IAllTasksCollection
	{
		public AllTasksCollection()
		{
			allTasks = new ConcurrentStack<IRunnableServiceTask>();
		}

		public void Add(IRunnableServiceTask task)
		{
			allTasks.Push(task);
		}

		public bool TryGetByCode(string code, out IRunnableServiceTask result)
		{
			result = allTasks.FirstOrDefault(t => t.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
			return result != null;
		}

		public IEnumerable<IRunnableServiceTask> GetAll()
		{
			return allTasks;
		}

		public void Clear()
		{
			allTasks.Clear();
		}

		public int Count => allTasks.Count;

		readonly ConcurrentStack<IRunnableServiceTask> allTasks;
	}
}
