using System;
using System.Threading;

namespace Enterprise.ServiceManager.Tasks.OnlineDataTransformation.Testing
{
	sealed class DecrementingFromFiveSecondsTransformation : TestTransformation
	{
		public override string UserDescription => "Decrementing transform";

		public DecrementingFromFiveSecondsTransformation()
		{
			sleep = 5;
		}
		int sleep;
		public override void Run(Action<string> logInformation, CancellationToken token)
		{
			token.WaitHandle.WaitOne(TimeSpan.FromSeconds(sleep));
			sleep -= 1;
			token.ThrowIfCancellationRequested();
		}
	}
}
