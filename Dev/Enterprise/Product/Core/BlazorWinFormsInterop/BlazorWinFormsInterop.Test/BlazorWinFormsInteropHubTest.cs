using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BlazorWinFormsInterop.Test
{
	[GuiTest]
	public class BlazorWinFormsInteropHubTest : TestCase
	{
		protected override void SetUp()
		{
			using (var hub = new BlazorWinFormsInteropHub())
			{
				hub.OnDisconnected(false);
			}

			base.SetUp();
		}

		public void TestRunEnterpriseUrl()
		{
			var mockHandler = new MockUrlHandler();
			EnterpriseUrlHandlerService.RegisterUrlHandler(mockHandler);
			try
			{
				Assert("Nothing should have handled the URL yet", mockHandler.HandledCount == 0);
				using (var hub = new BlazorWinFormsInteropHub())
				{
					hub.RunEnterpriseUrl("edient:Command=BlazorWinFormsInteropTest");
				}

				Assert("Should have handled URL", mockHandler.HandledCount == 1);
			}
			finally
			{
				EnterpriseUrlHandlerService.UnregisterUrlHandler(mockHandler);
			}
		}

		public void TestDisableHybridModeTransitionsListenerStateToDisabled()
		{
			using (var hub = new BlazorWinFormsInteropHub())
			{
				hub.DisableHybridMode();
				var winFormsListener = ObjectFactory.Get<IWinFormsListener>();
				var winFormsListenerState = winFormsListener
					.GetType()
					.GetField("winFormsListenerState", BindingFlags.NonPublic | BindingFlags.Instance)
					.GetValue(winFormsListener);

				var disabled = winFormsListenerState
					.GetType()
					.GetProperty("CurrentState", BindingFlags.NonPublic | BindingFlags.Instance)
					.GetValue(winFormsListenerState).ToString();

				AssertEquals(disabled, "Disabled");
			}
		}

		class MockUrlHandler : UrlHandler
		{
			public int HandledCount;
			protected override string ExpectedCommandText => "BlazorWinFormsInteropTest";

			protected override bool HandleCore(QueryString queryString)
			{
				HandledCount++;
				return true;
			}
		}
	}
}
