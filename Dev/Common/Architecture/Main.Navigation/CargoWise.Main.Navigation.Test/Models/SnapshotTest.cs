using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Test;

public class SnapshotTest : TestCase
{
	public void TestPropertyChanged()
	{
		var events = new List<string>();

		var snapshot = new Snapshot();
		snapshot.PropertyChanged += (sender, args) => events.Add(args.PropertyName);

		AssertEquals("1: Events count", 0, events.Count);

		snapshot.Order = 1;
		snapshot.Value = "100.000";

		AssertEquals("2: Events count", 2, events.Count);
		AssertContainsExactElementsInExactOrder(new[] { "Order", "Value" }, events);

		snapshot.Value = "100.000";
		snapshot.Order = 1;

		AssertEquals("3: Events count", 2, events.Count);
		AssertContainsExactElementsInExactOrder(new[] { "Order", "Value" }, events);

		snapshot.Order = 2;
		snapshot.Value = "200.000";

		AssertEquals("4: Events count", 4, events.Count);

		AssertContainsExactElementsInExactOrder(new[] { "Order", "Value", "Order", "Value" }, events);
	}

	public void TestPropertyChanged_ErrorMessage()
	{
		var events = new List<string>();

		var snapshot = new Snapshot();
		snapshot.PropertyChanged += (sender, args) => events.Add(args.PropertyName);

		AssertEquals("1: Events count", 0, events.Count);
		AssertEquals("1: HasError", expected: false, snapshot.HasError);

		snapshot.ErrorMessage = "Something went wrong!";

		AssertEquals("2: Events count", 1, events.Count);
		AssertEquals("2: HasError", expected: true, snapshot.HasError);
		AssertContainsExactElementsInExactOrder(new[] { "ErrorMessage" }, events);

		snapshot.ErrorMessage = string.Empty;

		AssertEquals("3: Events count", 2, events.Count);
		AssertEquals("3: HasError", expected: false, snapshot.HasError);
		AssertContainsExactElementsInExactOrder(new[] { "ErrorMessage", "ErrorMessage" }, events);

		snapshot.ErrorMessage = null;

		AssertEquals("4: Events count", 3, events.Count);
		AssertEquals("4: HasError", expected: false, snapshot.HasError);
		AssertContainsExactElementsInExactOrder(new[] { "ErrorMessage", "ErrorMessage", "ErrorMessage" }, events);
	}
}
