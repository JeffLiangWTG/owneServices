using System;
using System.Net;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Core.Testing
{
	class DefaultWebProxyListener : BaseTestListener
	{
		public static DefaultWebProxyListener Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new DefaultWebProxyListener();
				}

				return fInstance;
			}
		}

		public override void BeforeEachTest(DateTime startTime)
		{
			base.BeforeEachTest(startTime);
			originalProxy = WebRequest.DefaultWebProxy;
		}

		public override void AfterEachTest(DateTime endTime)
		{
			base.AfterEachTest(endTime);
			try
			{
				if (WebRequest.DefaultWebProxy != originalProxy)
				{
					Assertion.Fail("WebRequest.DefaultWebProxy shoudn't be changed after each test run.");
				}
			}
			finally
			{
				WebRequest.DefaultWebProxy = originalProxy;
			}
		}

		IWebProxy originalProxy;

		#region Implementation

		[ThreadSafe]
		static DefaultWebProxyListener fInstance;

		protected DefaultWebProxyListener()
		{
		}

		#endregion
	}
}
