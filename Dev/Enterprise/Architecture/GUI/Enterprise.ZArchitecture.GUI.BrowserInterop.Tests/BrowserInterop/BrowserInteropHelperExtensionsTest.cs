using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Newtonsoft.Json.Linq;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop.Tests
{
	public class BrowserInteropHelperExtensionsTest : TestCaseWithFactory
	{
		public void TestBrowserInteropHelperExtensions()
		{
			ErrorReporter.Clear();
			var dummyBrowserInteropWindowFactory = new DummyBrowserInteropWindowFactory();
			ObjectFactory.Substitute<IBrowserInteropWindowFactory>(dummyBrowserInteropWindowFactory);
			var bizo = Factory.New<DummyBusinessObject>();
			Factory.Save();

			ClientUpdateFilterData updateFilterData(DummyBusinessObject bizo)
			{
				var bizoLoaded = Factory.Load(typeof(DummyBusinessObject), bizo.PK);
				var data = new ClientUpdateFilterData();
				data.clientPK = bizoLoaded.PK;
				return data;
			}
			var window = ((IBrowserInteropWindowFactory)dummyBrowserInteropWindowFactory).CreateBrowserInteropWindow("WindowName", new Uri("http://test.com"));
			BrowserInteropHelperExtensions.AddBrowserListenerCommandHandler(window, () => updateFilterData(bizo));

			dummyBrowserInteropWindowFactory.MessageTransportLayer.HandleObjectFromBrowser<JToken>(WebViewCommands.Ready);

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}
	}
}
