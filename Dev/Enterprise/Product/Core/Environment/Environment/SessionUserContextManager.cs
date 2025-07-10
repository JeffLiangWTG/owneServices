using CargoWise.Common;
using Enterprise.Security;

namespace Enterprise.Environment
{
	public sealed class SessionUserContextManager : Disposable, IUserContextManager
	{
		public IUserContext ContextForReadOnly => ContextSecurity.Context;
		public IUserContext Context => ContextSecurity.Context;
		public SecurityCore Security => ContextSecurity.Security;
		public bool CurrentThreadContextIsOverriden => ContextSecurity.ContextIsOverriden;

		public void ClearCurrentThread()
		{
			lock (securityLock)
			{
				ResetContextSecurity();
			}
		}

		public void SetCurrentThreadUserContext(IUserContext userContext, bool isRevert)
		{
			if (userContext == null)
			{
				lock (securityLock)
				{
					ResetContextSecurity();
				}
			}

			ContextSecurity.SetCurrentThreadUserContext(userContext ?? new UserContext(), isRevert);
		}

		public void SetMasterUserContext(IUserContext userContext)
		{
			if (userContext == null)
			{
				lock (securityLock)
				{
					ResetContextSecurity();
				}
			}

			ContextSecurity.SetMasterUserContext(userContext ?? new UserContext());
		}

		protected override void Dispose(bool isDisposing) { }

		void ResetContextSecurity()
		{
			contextSecurity = new UserContextSecurity();
		}

		UserContextSecurity ContextSecurity
		{
			get
			{
				if (contextSecurity == null)
				{
					lock (securityLock)
					{
						if (contextSecurity == null)
						{
							ResetContextSecurity();
						}
					}
				}

				return contextSecurity;
			}
		}

		UserContextSecurity contextSecurity;

		readonly object securityLock = new object();

#if DEBUG
		public void SetSecurityForTest(SecurityCore securityInstance) => contextSecurity.Security = securityInstance;

		public bool IsSecurityCreatedForTest => contextSecurity.IsSecurityCreatedForTest;

		public void ResetSecurityForTest()
		{
			contextSecurity.ResetSecurityForTest();
		}

		public DisposableAction SetEmptyThreadUserContextSecurityForTest()
		{
			return new DisposableAction(null);
		}
#endif
	}
}
