using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Environment
{
	public class WorkflowUserContextManager : IService
	{
		IUserContext userContext;

		public IDisposable SetWorkflowUserContext(IUserContext theUserContext, UserContextSwitchLogger logger = null)
		{
			UserContext = theUserContext;
			Logger = logger;

			bool disposed = false;
			return new DisposableAction(() =>
			{
				if (disposed)
				{
					ErrorReporter.ReportOnce("Double dispose of Workflow user context");
					return;
				}

				disposed = true;
				userContext = null;
			});
		}

		public IUserContext UserContext
		{
			get => userContext;
			private set
			{
				if (value != null && userContext != null)
				{
					throw new InvalidOperationException("Nesting of Workflow user contexts is not permitted");
				}

				userContext = value;
			}
		}

		public UserContextSwitchLogger Logger { get; private set; }
	}
}
