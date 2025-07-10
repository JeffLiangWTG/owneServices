using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ColourLegendTest : TestCaseWithFactory
	{
		#region TestGetColour

		public void TestGetColour()
		{
			ZGuid pk1 = ZGuid.NewZGuid();
			ZGuid pk2 = ZGuid.NewZGuid();
			AssertEquals("precondition: control count", 0, Legend.Controls.Count);

			Color colour1 = Legend.GetColour(pk1, "pk1");
			AssertEquals("Should now have a control.", 1, Legend.Controls.Count);
			AssertEquals("The control should of the correct type.", typeof(ColourLegendElement), Legend.Controls[0].GetType());
			AssertEquals("The control should have the correct colour.", colour1, ((ColourLegendElement)Legend.Controls[0]).Colour);
			AssertEquals("The control should have the correct text.", "pk1", Legend.Controls[0].Text);

			Color colour2 = Legend.GetColour(pk2, "pk2");
			AssertNotEquals("colour2 should be distinct from colour1", colour1, colour2);
			AssertEquals("A second control should have been added.", 2, Legend.Controls.Count);
			AssertEquals("The new control should be of the correct type.", typeof(ColourLegendElement), Legend.Controls[1].GetType());
			AssertEquals("The new control should have the correct colour.", colour2, ((ColourLegendElement)Legend.Controls[1]).Colour);
			AssertEquals("The new control should have the correct text.", "pk2", Legend.Controls[1].Text);

			Color colour1b = Legend.GetColour(pk1, "pk1b");
			AssertEquals("colour1b should be the same as colour1.", colour1, colour1b);
			AssertEquals("Dont add a new control when re-using the same pk.", 2, Legend.Controls.Count);
			AssertEquals("The existing control should still be using the correct colour.", colour1b, ((ColourLegendElement)Legend.Controls[0]).Colour);
			AssertEquals("The text of the existing control should be updated.", "pk1b", Legend.Controls[0].Text);
		}

		#endregion

		#region TestRelease

		public void TestRelease()
		{
			ZGuid[] pks = new ZGuid[Legend.AvailableColorsCount + 1];
			Color[] colours = new Color[Legend.AvailableColorsCount + 1];

			for (int i = 0; i < pks.Length; i++)
			{
				pks[i] = ZGuid.NewZGuid();
				colours[i] = Legend.GetColour(pks[i], i.ToString());
			}

			AssertEquals("precondition: ", true, colours[colours.Length - 1].IsSystemColor);
			AssertEquals("precondition: ", Legend.AvailableColorsCount + 1, Legend.Controls.Count);

			Legend.Release(pks[0]);
			Legend.Release(pks[1]);
			Legend.Release(pks[colours.Length - 1]);

			AssertEquals("3 elements removed", Legend.AvailableColorsCount - 2, Legend.Controls.Count);
			AssertEquals("released non-system colours should be returned to the pool", colours[1], Legend.GetColour(ZGuid.NewZGuid(), "colour1"));
			AssertEquals("released non-system colours should be returned to the pool", colours[0], Legend.GetColour(ZGuid.NewZGuid(), "colour2"));
			AssertEquals("pool empty again", colours[colours.Length - 1], Legend.GetColour(ZGuid.NewZGuid(), "colour3"));
		}

		#endregion

		#region TestReleasingAnUnknownGuidShouldNotThrowAnException

		[NUnit.Framework.ExpectNoExceptions]
		public void TestReleasingAnUnknownGuidShouldNotThrowAnException()
		{
			Legend.Release(ZGuid.NewZGuid());
		}

		#endregion

		#region TestColourPool

		public void TestColourPool()
		{
			for (int i = 0; i < Legend.AvailableColorsCount; i++)
			{
				Color colour = Legend.GetColour(ZGuid.NewZGuid(), i.ToString());
				AssertEquals("pool colours should not be system colours", false, colour.IsSystemColor);
			}
		}

		#endregion

		#region TestAutoScroll

		public void TestAutoScroll()
		{
			Assert("AutoScroll is enabled by default", Legend.AutoScroll);
			Legend.AutoScroll = false;
			Assert("AutoScroll is enabled always", Legend.AutoScroll);
		}

		#endregion

		#region Implementation

		ColourLegend Legend
		{
			get { return legend ?? (legend = new ColourLegend()); }
		}
		ColourLegend legend;

		protected override void TearDown()
		{
			base.TearDown();
			if (legend != null)
			{
				legend.Dispose();
			}
		}

		#endregion
	}
}
