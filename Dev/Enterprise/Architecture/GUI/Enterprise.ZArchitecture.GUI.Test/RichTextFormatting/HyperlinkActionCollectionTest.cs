using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class HyperlinkActionCollectionTest : TestCase
	{
		public void TestKeys()
		{
			var collection = new HyperlinkActionCollection();

			var action1 = collection[new LogUrlLink("name", new Uri("http://www.cargowise.com"))];
			AssertEquals("Can't handle even one link", "KEY0000", action1.Key);

			var action2 = collection[new LogUrlLink("anothername", new Uri("http://www.cargowise.com"))];
			AssertEquals("Can't handle a second link", "KEY0001", action2.Key);

			var action3 = collection[new LogUrlLink("name", new Uri("http://www.cargowise.com"))];
			AssertEquals("Doesn't detect an existing link", "KEY0000", action3.Key);
			AssertSame(action1, action3);

			collection.Clear();
			var action4 = collection[new LogUrlLink("bob", new Uri("http://www.cargowise.com"))];
			AssertEquals("Doesn't clear out existing links", "KEY0000", action4.Key);

			var action5 = collection[new LogUrlLink("name", new Uri("http://www.cargowise.com"))];
			AssertEquals("Doesn't notice that the link name is different", "KEY0001", action5.Key);
		}

		public void TestIsHyperlinkActionCollectionLink()
		{
			var eventArgs1 = new LinkClickedEventArgs("http://test.com#temp=test");
			AssertEquals(HyperlinkActionCollection.IsHyperlinkActionCollectionLink(eventArgs1), false);

			var eventArgs2 = new LinkClickedEventArgs("TEST#KEY0000");
			AssertEquals(HyperlinkActionCollection.IsHyperlinkActionCollectionLink(eventArgs2), true);
		}
	}
}
