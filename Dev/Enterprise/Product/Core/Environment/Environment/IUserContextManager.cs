using System;
using CargoWise.Common;
using Enterprise.Security;

namespace Enterprise.Environment
{
	public interface IUserContextManager : IDisposable
	{
		IUserContext Context { get; }
		IUserContext ContextForReadOnly { get; }
		SecurityCore Security { get; }
		bool CurrentThreadContextIsOverriden { get; }

		void SetMasterUserContext(IUserContext userContext);
		void SetCurrentThreadUserContext(IUserContext userContext, bool isRevert);
		void ClearCurrentThread();

#if DEBUG
		void SetSecurityForTest(SecurityCore securityInstance);
		bool IsSecurityCreatedForTest { get; }
		void ResetSecurityForTest();

		DisposableAction SetEmptyThreadUserContextSecurityForTest();
#endif
	}
}
