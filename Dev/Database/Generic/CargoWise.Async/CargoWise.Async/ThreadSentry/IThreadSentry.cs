using System;
using System.Threading;

namespace CargoWise.Async
{
	public interface IThreadSentry
	{
		void RelinquishThreadOwnership(string diagnosticLabel);
		void RelinquishThreadOwnership(Func<string> diagnosticLabelProvider = null);

#if DEBUG
		void ForciblyRelinquishThreadOwnership_ForTest(string diagnosticLabel);

		void ForciblyRelinquishThreadOwnership_ForTest(Func<string> diagnosticLabelProvider = null);
#endif

		void TakeThreadOwnership(string diagnosticLabel);
		void TakeThreadOwnership(Func<string> diagnosticLabelProvider = null);
		void EnsureCurrentThreadIsOwner(string diagnosticLabel);
		void EnsureCurrentThreadIsOwner(Func<string> diagnosticLabelProvider = null);
		IDisposable SuppressReporting();

		bool IsOwner { get; }
		ThreadLog CreationThread { get; }
		ThreadLog OwnerThread { get; }

		void Post(SendOrPostCallback d, object state, string callingMethodName);

		void Post(SendOrPostCallback method, object state, IThreadSentry callbackSentry, SendOrPostCallback callback, string callingMethodName);

		bool IsPostable { get; }
	}
}
