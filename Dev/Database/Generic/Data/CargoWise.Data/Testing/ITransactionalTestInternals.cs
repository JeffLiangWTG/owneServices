#if DEBUG

using System;

namespace NUnit.Framework
{
	public interface ITransactionalTestInternals
	{
		void SetInTransactionedTestCase();
		IDisposable SetInReflectionTestTemporary();
	}
}

#endif
