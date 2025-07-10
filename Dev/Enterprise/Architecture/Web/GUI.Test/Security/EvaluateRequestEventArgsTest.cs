using System.Web;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Security.Configuration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Security.Testing
{
	[HttpContextEnabledTest]
	public class EvaluateRequestEventArgsTest : TestCase
	{
		public void TestConstructor()
		{
			EvaluateRequestEventArgs args = new EvaluateRequestEventArgs(HttpContext.Current.ApplicationInstance, Settings);
			AssertNotNull("Constructed", args);
			AssertEquals("Application", HttpContext.Current.ApplicationInstance, args.Application);
			AssertEquals("Settings", Settings, args.Settings);
			AssertEquals("CancelEvaluation", false, args.CancelEvaluation);
		}

		public void TestCancelEvaluation()
		{
			EvaluateRequestEventArgs args = new EvaluateRequestEventArgs(HttpContext.Current.ApplicationInstance, Settings);
			AssertEquals("CancelEvaluation default", false, args.CancelEvaluation);

			args.CancelEvaluation = true;
			AssertEquals("CancelEvaluation as set", true, args.CancelEvaluation);
		}

		#region Implementation

		SecureWebPageSettings Settings
		{
			get { return settings ?? (settings = new SecureWebPageSettings()); }
		}
		SecureWebPageSettings settings;

		#endregion

	}
}
