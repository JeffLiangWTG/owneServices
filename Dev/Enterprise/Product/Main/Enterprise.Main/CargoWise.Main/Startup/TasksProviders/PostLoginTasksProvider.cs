using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup
{
	public class PostLoginTasksProvider
	{
		public virtual IReadOnlyList<IPostLoginTask> GetPostLoginTasks()
		{
			return System.Array.Empty<IPostLoginTask>();
		}
	}
}
