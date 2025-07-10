using System.Linq;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class MultiActionProvidingButtonStripTest : TestCase
	{
		enum ArbitraryDialogReturnValues
		{
			Ein,
			Zwei,
			Drei
		}

		public void TestButtonStrip()
		{
			var actions = new[]
			{
				new ButtonStripAction<ArbitraryDialogReturnValues>
				{
					Text = "Deep Purple",
					Response = ArbitraryDialogReturnValues.Ein
				}
			};

			using (var strip = new MultiActionProvidingButtonStrip<ArbitraryDialogReturnValues>(actions))
			using (var form = new ZForm())
			{
				form.Controls.Add(strip);
				form.Show();

				var button = (ZButton)strip.Controls.Find("Deep Purple", true).Single();
				button.PerformClick();

				AssertEquals(ArbitraryDialogReturnValues.Ein, form.Tag);
			}
		}
	}
}
