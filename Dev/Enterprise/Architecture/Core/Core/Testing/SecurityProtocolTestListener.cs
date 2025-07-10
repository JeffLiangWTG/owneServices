#if DEBUG
using System;
using System.Net;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class SecurityProtocolTestListener : BaseTestListener
	{
		public override void AfterEachTest(DateTime endTime)
		{
			if (ServicePointManager.SecurityProtocol != SecurityProtocolType.SystemDefault)
			{
				Assertion.Fail($"SystemDefault should be used as the value of SecurityProtocol unless there is a specific reason. It's changed to {ServicePointManager.SecurityProtocol}.");
			}

			base.AfterEachTest(endTime);
		}
	}
}
#endif
