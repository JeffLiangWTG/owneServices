using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class FilterStripWrapperControlTest : BMSTestCaseWithFactory
	{
		public void TestThatDeletingStripsWorks()
		{
			var tag1 = Factory.New<TagRule>();
			var filter1 = tag1.Filter;

			using (var form = new ZForm())
			using (var control = new BMFilterStripWrapperControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				control.SetDataBinding(tag1, "Filter");
				var stripControl = control.FindAll<FilterRuleFilterStripControl>().Single();
				AssertNull(stripControl.FilterBusinessObject.LastUsedLayout);
				stripControl.AddNewFilterStrip();

				var filterStrip = stripControl.FindAll<ZFilterStrip>().First();
				filterStrip.CurrentDataItem.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
				Application.DoEvents();
				var deleteStripButton = (ZButton)filterStrip.Controls.Find("DeleteStripButton", true).Single();

				var changedFilterData = filter1.S9_FilterData;
				deleteStripButton.PerformClick();
				Application.DoEvents();

				AssertNotEquals(changedFilterData, filter1.S9_FilterData);
			}
		}

		public void TestThatItDoesntStoreLayouts()
		{
			var tag1 = Factory.New<TagRule>();
			var filter1 = tag1.Filter;
			var tag2 = Factory.New<TagRule>();
			var filter2 = tag2.Filter;

			using (var form = new ZForm())
			using (var control = new BMFilterStripWrapperControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				control.SetDataBinding(tag1, "Filter");
				var stripControl = control.FindAll<FilterRuleFilterStripControl>().Single();
				AssertNull(stripControl.FilterBusinessObject.LastUsedLayout);

				control.SetDataBinding(tag2, "Filter");
				AssertNull(stripControl.FilterBusinessObject.LastUsedLayout);
			}
		}

		public void TestThereIsOnlyOneWrappedControl()
		{
			var tag = Factory.New<TagRule>();
			var filter = tag.Filter;

			using (var control = new BMFilterStripWrapperControl())
			{
				control.SetDataBinding(tag, "Filter");
				var stripControl1 = control.FindAll<FilterRuleFilterStripControl>().Single();

				control.SetDataBinding(tag, "Filter");
				var stripControl2 = control.FindAll<FilterRuleFilterStripControl>().Single();

				AssertEquals(stripControl1, stripControl2);
			}
		}
	}
}
