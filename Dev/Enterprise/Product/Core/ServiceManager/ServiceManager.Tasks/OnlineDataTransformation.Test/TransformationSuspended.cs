using System;
using System.Threading;

namespace Enterprise.ServiceManager.Tasks.OnlineDataTransformation.Testing
{
	sealed class TransformationSuspended : TestTransformation
	{
		int willBeCompletedOnThirdRun;

		public override void Run(Action<string> logInformation, CancellationToken token)
		{
			if (++willBeCompletedOnThirdRun < 3)
			{
				throw new OperationCanceledException("This replaces the previous 'notCompleted' state.");
			}
		}
	}
}
