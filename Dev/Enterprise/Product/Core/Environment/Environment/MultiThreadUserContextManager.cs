using System.Diagnostics;
using System.Threading;
using CargoWise.Common;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Environment
{
	/// <summary>
	/// Each thread gets a thread local UserContext.
	/// </summary>
	public sealed class MultiThreadUserContextManager : Disposable, IUserContextManager
	{
		public MultiThreadUserContextManager()
		{
			SetMasterUserContext(new UserContext());
		}

		/// <summary>
		/// Set the user context for this thread, and any threads that don't yet have have a local context.
		/// </summary>
		public void SetMasterUserContext(IUserContext userContext)
		{
			var newUserContext = userContext ?? new UserContext();
			masterUserContext = newUserContext;
			if (userContext == null)
			{
				// Discard old security
#if DEBUG
				if (Globals.IsTest)
				{
					threadUserContext.Value?.ResetSecurityForTest();
				}
				else
				{
#endif
					threadUserContext.Value = null;
#if DEBUG
				}
#endif
			}
			ThreadContext.SetMasterUserContext(newUserContext);
			if (newUserContext.Branch == null)
			{
				setMasterUserContextNoBranchStackTrace = new StackTrace();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Accessed by reflection in Masterfiles Business by LocalBranchTimeOffset extension")]
		StackTrace setMasterUserContextNoBranchStackTrace;

		public void SetCurrentThreadUserContext(IUserContext userContext, bool isRevert)
		{
			ThreadContext.SetCurrentThreadUserContext(userContext ?? new UserContext(), isRevert);
		}

		UserContextSecurity ThreadContext
		{
			get
			{
				if (threadUserContext.Value == null)
				{
					threadUserContext.Value = new UserContextSecurity();
				}
				return threadUserContext.Value;
			}
		}

		public bool CurrentThreadContextIsOverriden => (threadUserContext.IsValueCreated && threadUserContext.Value != null) && threadUserContext.Value.ContextIsOverriden;

		public IUserContext ContextForReadOnly
		{
			get
			{
				// Note, no locking. It's no problem if another thread is changing the masterUserContext since reference fields are thread safe.
				var master = masterUserContext;
				if (!threadUserContext.IsValueCreated || threadUserContext.Value == null)
				{
					return master;
				}
				else
				{
					var localContext = threadUserContext.Value.Context;
					return localContext != null && !localContext.Equals(master) ? localContext : master;
				}
			}
		}

		public IUserContext Context => ContextSecurity?.Context;

		UserContextSecurity ContextSecurity
		{
			get
			{
				if (threadUserContext.IsValueCreated && threadUserContext.Value != null)
				{
					return threadUserContext.Value;
				}
				else
				{
					if (isCreatingUserContext.Value)
					{
						return null;
					}

					try
					{
						isCreatingUserContext.Value = true;
						var newContext = masterUserContext.ThreadSafeClone();
						if (newContext.Branch == null)
						{
							contextSecurityNoBranchStackTrace = new StackTrace();
						}
						ThreadContext.SetMasterUserContext(newContext);
						return ThreadContext;
					}
					finally
					{
						isCreatingUserContext.Value = false;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Accessed by reflection in Masterfiles Business by LocalBranchTimeOffset extension")]
		StackTrace contextSecurityNoBranchStackTrace;

		ThreadLocal<bool> isCreatingUserContext = new ThreadLocal<bool>(() => false);

		public SecurityCore Security => ContextSecurity?.Security;

		public void ClearCurrentThread()
		{
			if (threadUserContext.IsValueCreated)
			{
				threadUserContext.Value = null;
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (threadUserContext != null)
				{
					threadUserContext.Dispose();
					threadUserContext = null;
				}
				if (isCreatingUserContext != null)
				{
					isCreatingUserContext.Dispose();
					isCreatingUserContext = null;
				}
			}
		}

		IUserContext masterUserContext;
		ThreadLocal<UserContextSecurity> threadUserContext = new ThreadLocal<UserContextSecurity>();

#if DEBUG
		public void SetSecurityForTest(SecurityCore securityInstance)
		{
			ContextSecurity.Security = securityInstance;
		}

		public bool IsSecurityCreatedForTest => ContextSecurity.IsSecurityCreatedForTest;

		public void ResetSecurityForTest()
		{
			ContextSecurity.ResetSecurityForTest();
		}

		public DisposableAction SetEmptyThreadUserContextSecurityForTest()
		{
			var context = threadUserContext.Value;
			threadUserContext.Value = new UserContextSecurity();
			return new DisposableAction(() => threadUserContext.Value = context);
		}
#endif
	}
}
