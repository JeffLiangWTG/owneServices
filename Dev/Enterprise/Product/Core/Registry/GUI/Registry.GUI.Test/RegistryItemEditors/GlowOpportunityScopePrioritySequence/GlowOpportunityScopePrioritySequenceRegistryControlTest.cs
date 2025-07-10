using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GlowOpportunityScopePrioritySequenceRegistryControl))]
	sealed class GlowOpportunityScopePrioritySequenceRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new GlowOpportunityScopePrioritySequenceCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((GlowOpportunityScopePrioritySequenceRegistryControl)control).ReadOnly;
		}

		[RequiresSTA]
		public void TestUpDownButtons()
		{
			var collection = new GlowOpportunityScopePrioritySequenceCollection();

			collection.AddNew((NoResString)"AAA");
			collection.AddNew((NoResString)"BBB");
			collection.AddNew((NoResString)"CCC");

			using (ZForm form = new ZForm())
			using (var control = new GlowOpportunityScopePrioritySequenceRegistryControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(collection, null);

				var grid = control.GlowOpportunityScopePrioritySequenceGrid;

				CombineAssertions("Precondition: Sequence is AAA, BBB, CCC", () =>
				{
					AssertEquals("AAA", ((GlowOpportunityScopePrioritySequence)(grid.List[0])).Description);
					AssertEquals("BBB", ((GlowOpportunityScopePrioritySequence)(grid.List[1])).Description);
					AssertEquals("CCC", ((GlowOpportunityScopePrioritySequence)(grid.List[2])).Description);
				});

				grid.CurrentRowIndex = 1;
				control.MovePriorityForTest(true);

				CombineAssertions("Test: Pressing the 'Up' button for 'BBB' at index 1 should move it to index 0 in the list.", () =>
				{
					AssertEquals("BBB", ((GlowOpportunityScopePrioritySequence)(grid.List[0])).Description);
					AssertEquals("AAA", ((GlowOpportunityScopePrioritySequence)(grid.List[1])).Description);
					AssertEquals("CCC", ((GlowOpportunityScopePrioritySequence)(grid.List[2])).Description);
				});

				grid.CurrentRowIndex = 1;
				control.MovePriorityForTest(false);

				CombineAssertions("Test: Pressing the 'Down' button for 'AAA' at index 1 should move it to index 2 in the list.", () =>
				{
					AssertEquals("BBB", ((GlowOpportunityScopePrioritySequence)(grid.List[0])).Description);
					AssertEquals("CCC", ((GlowOpportunityScopePrioritySequence)(grid.List[1])).Description);
					AssertEquals("AAA", ((GlowOpportunityScopePrioritySequence)(grid.List[2])).Description);
				});

				grid.CurrentRowIndex = 0;
				control.MovePriorityForTest(true);

				CombineAssertions("Test: Pressing the 'Up' button for 'BBB' at index 0 does not move it as it is already at the top.", () =>
				{
					AssertEquals("BBB", ((GlowOpportunityScopePrioritySequence)(grid.List[0])).Description);
					AssertEquals("CCC", ((GlowOpportunityScopePrioritySequence)(grid.List[1])).Description);
					AssertEquals("AAA", ((GlowOpportunityScopePrioritySequence)(grid.List[2])).Description);
				});

				grid.CurrentRowIndex = 2;
				control.MovePriorityForTest(false);

				CombineAssertions("Test: Pressing the 'Down' button for 'AAA' at index 2 does not move it as it is already at the bottom.", () =>
				{
					AssertEquals("BBB", ((GlowOpportunityScopePrioritySequence)(grid.List[0])).Description);
					AssertEquals("CCC", ((GlowOpportunityScopePrioritySequence)(grid.List[1])).Description);
					AssertEquals("AAA", ((GlowOpportunityScopePrioritySequence)(grid.List[2])).Description);
				});
			}
		}

		class GlowOpportunityScopePrioritySequenceRegistryControlForTest : GlowOpportunityScopePrioritySequenceRegistryControl
		{
			public void MovePriorityForTest(bool up)
			{
				MovePriority(up);
			}
		}
	}
}
