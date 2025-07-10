using System;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class MultiActionButtonDialogProviderTest : TestCase
	{
		enum ArbitraryDialogReturnValues
		{
			Ein = 0,
			Zwei,
			Drei
		}

		public void TestLogLastShow()
		{
			var actions = new[]
			{
				new ButtonStripAction<ArbitraryDialogReturnValues>
				{
					Text = "Deep Purple",
					Response = ArbitraryDialogReturnValues.Drei
				}
			};

			var dialogProvider = new MultiActionButtonDialogWrapper<ArbitraryDialogReturnValues>();
			AssertEquals(ArbitraryDialogReturnValues.Ein, dialogProvider.ShowDialog("Wombo", "Combo"));
			AssertEquals("Wombo", dialogProvider.LastMessage);
			AssertEquals("Combo", dialogProvider.LastCaption);
		}

		public void TestButtonStripDialog()
		{
			var actions = new[]
			{
				new ButtonStripAction<ArbitraryDialogReturnValues>
				{
					Text = "Deep Purple",
					Response = ArbitraryDialogReturnValues.Drei
				}
			};

			AssertEquals(ArbitraryDialogReturnValues.Ein, MultiActionButtonDialogProvider.ShowDialog<ArbitraryDialogReturnValues>("Wombo", "Combo"));
			AssertEquals(ArbitraryDialogReturnValues.Zwei, MultiActionButtonDialogProvider.ShowDialog("Some have been known", "To eat", ArbitraryDialogReturnValues.Zwei));

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
			{
				var form = (ZForm)f;
				form.Tag = ArbitraryDialogReturnValues.Drei;
			});

			AssertEquals(ArbitraryDialogReturnValues.Drei, MultiActionButtonDialogProvider.ShowDialog("The mighty Lizbanana", "But others", actions));
		}

		public void TestFailsWithoutLeaking()
		{
			var actions = new[]
			{
				new ButtonStripAction<ArbitraryDialogReturnValues>
				{
					Text = "Deep Purple",
					Response = ArbitraryDialogReturnValues.Drei,
				}
			};

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
			{
				throw new InvalidOperationException();
			});

			AssertExceptionThrown<InvalidOperationException>(() => MultiActionButtonDialogProvider.ShowDialog("Reject it", "out of apathy", actions));
		}
	}
}
