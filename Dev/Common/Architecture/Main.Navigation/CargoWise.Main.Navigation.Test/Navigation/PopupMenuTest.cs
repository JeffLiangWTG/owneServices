using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Test;

class PopupMenuTest : TestCase
{
	public void TestPropertyChangeNotification()
	{
		var events = new List<string>();

		var uut = new PopupMenu();
		uut.PropertyChanged += (s, args) => events.Add(args.PropertyName);

		AssertEquals(0, events.Count);

		uut.Title = "Title";
		uut.IsSelected = true;
		uut.InputGestureText = "Ctrl+T";

		AssertEquals(3, events.Count);
		AssertEquals("Title", events[0]);
		AssertEquals("IsSelected", events[1]);
		AssertEquals("InputGestureText", events[2]);
	}
}
