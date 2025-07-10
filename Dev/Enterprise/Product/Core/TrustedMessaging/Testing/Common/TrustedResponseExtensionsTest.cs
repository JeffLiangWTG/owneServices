using Enterprise.Integration;
using Enterprise.TrustedMessaging.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.TrustedMessaging.Models;

namespace Enterprise.TrustedMessaging.Testing
{
	public class TrustedResponseExtensionsTest : TestCase
	{
		public void TestLog()
		{
			var logger = new TestServiceLogger();
			var rsp = new TrustedResponse<bool>();
			rsp.Log(null);
			rsp.Log(logger);
			rsp = new TrustedResponse<bool>("000", "some error~");
			rsp.Log(logger);
			rsp = new TrustedResponse<bool>(WTG.TrustedMessaging.Constants.ErrorCodes.SecretKeyNotUpToDate, "SecretKeyNotUpToDate");
			rsp.Log(logger);
			AssertEquals(@"Error|000 : some error~
Warning|1002 : SecretKeyNotUpToDate
", logger.ToString());
		}
	}
}
