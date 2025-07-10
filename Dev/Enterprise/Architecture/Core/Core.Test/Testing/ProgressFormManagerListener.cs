using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class ProgressFormManagerListener : BaseTestListener
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly ProgressFormManagerListener Instance = new ProgressFormManagerListener();

		public ProgressFormManagerListener()
		{
		}

		public override void AfterEachTest(DateTime endTime)
		{
			var threadWeakReference = ProcessStatusFormManager.mostRecentThread_DebugOnly;
			if (threadWeakReference != null && threadWeakReference.TryGetTarget(out var thread))
			{
				if (thread.ThreadState == ThreadState.Running)
				{
					if (!thread.Join(TimeSpan.FromSeconds(10)))
					{
						Assertion.Fail(@"ProcessStatusFormManager.mostRecentThread didn't finish up within 10 seconds after the test concluded.
This means that the form's creation leaked past the test finishing and could cause amnesties in other tests through unexpected multi-threaded concurrency.
Let's debug, learn why and fix this. (Or if all else fails, doing a Thread.Abort() in ProgressFormManagerListener might be OK?)");
					}
				}
			}
			threadWeakReference = null;
		}
	}
}
