#if DEBUG

using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CargoWise.Async
{
	public static class TaskRegistryForTest
	{
		public static void Reset()
		{
			registry = new ConcurrentDictionary<int, string>();
		}

		static ConcurrentDictionary<int, string> registry = new ConcurrentDictionary<int, string>();

		public static void RegisterTask(Task task, string description)
		{
			if (task != null)
			{
				registry.TryAdd(task.Id, description);
			}
		}

		public static bool TryGetTaskDescriptionById(int id, out string description) => registry.TryGetValue(id, out description);
	}
}
#endif
