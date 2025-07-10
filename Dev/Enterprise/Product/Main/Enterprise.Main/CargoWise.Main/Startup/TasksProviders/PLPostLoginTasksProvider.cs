using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup
{
	public class PLPostLoginTasksProvider : PostLoginTasksProvider
	{
		public override IReadOnlyList<IPostLoginTask> GetPostLoginTasks()
		{
			return new IPostLoginTask[] { new StartupCheckPLCertificateExpirationDate() };
		}
	}
}
