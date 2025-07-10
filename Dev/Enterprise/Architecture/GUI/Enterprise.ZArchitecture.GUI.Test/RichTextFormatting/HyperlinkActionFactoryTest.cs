using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class HyperlinkActionFactoryTest : TestCase
	{
		public void TestLogControllerLink()
		{
			var link = new LogControllerLink("text", ControllerIDs.JobShipment, ZGuid.NewZGuid());
			var action = HyperlinkActionFactory.New(link, "key");

			AssertNotNull(action);
			AssertEquals(typeof(ControllerAction), action.GetType());
			AssertEquals(link, action.Hyperlink);
			AssertEquals("key", action.Key);
		}

		public void TestLogUrlLink()
		{
			var link = new LogUrlLink("text", new Uri("http://www.cargowise.com"));
			var action = HyperlinkActionFactory.New(link, "key");

			AssertNotNull(action);
			AssertEquals(typeof(UrlAction), action.GetType());
			AssertEquals(link, action.Hyperlink);
			AssertEquals("key", action.Key);
		}
	}
}
